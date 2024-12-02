using ComplianceMgmt.Api.Extensions;
using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;
using MySql.Data.MySqlClient;
using System.Dynamic;
using System.Text;

namespace ComplianceMgmt.Api.Repository
{
    public class RecordCountRepository(ComplianceMgmtDbContext context, IServerDetailRepository serverDetailRepository) : IRecordCountRepository
    {
        public async Task<IEnumerable<TableSummary>> GetRecordCountAsync(DateOnly date)
        {
            try
            {
                var formattedDate = date.ToString("yyyy/MM/dd", System.Globalization.CultureInfo.InvariantCulture);

                using (var connection = context.CreateConnection())
                {
                    // Call the stored procedure
                    var summaries = new List<TableSummary>();

                    // List of tables to summarize
                    var tables = new List<MsgStructure>
                    {
                       new() { TableName =  "stgborrowerdetail", MsgStruct =  "Borrower Detail" },
                       new() { TableName =  "stgborrowerloan",MsgStruct = "Borrower Loan" },
                        new() { TableName = "stgborrowermortgage",MsgStruct ="Borrower Mortgage" },
                        new() { TableName = "stgborrowermortgageother",MsgStruct = "Borrower Mortgage Other" },
                        new() { TableName = "stgcoborrowerdetails",MsgStruct ="Co Borrower Details" }
                    };

                    foreach (var table in tables)
                    {
                        // Query for each table, parameterized with @Date
                        var query = $@"
                                    SELECT 
                                        '{table.MsgStruct}' AS MsgStructure,
                                        COUNT(*) AS TotalRecords,
                                        SUM(CASE WHEN IsValidated = 1 THEN 1 ELSE 0 END) AS SuccessRecords,
                                        SUM(CASE WHEN IsValidated = 0 AND RejectedReason IS NOT NULL THEN 1 ELSE 0 END) AS ConstraintRejection,
                                        SUM(CASE WHEN IsValidated = 0 AND RejectedReason IS NULL THEN 1 ELSE 0 END) AS BusinessRejection
                                    FROM {table.TableName}
                                    WHERE Date = @Date";

                        // Execute query and add result to summaries list
                        var summary = await connection.QueryFirstOrDefaultAsync<TableSummary>(query, new { Date = formattedDate });
                        if (summary != null)
                        {
                            summaries.Add(summary);
                        }
                    }

                    return summaries;
                }
            }
            catch (Exception ex)
            {
                // Log or handle the error
                throw new Exception("An error occurred while calling the stored procedure.", ex);
            }
        }

        public async Task FetchAndInsertAllTablesAsync()
        {
            var tables = new List<MsgStructure>
            {
                new()
                {
                    TableName = "stgborrowerdetail",
                    RejectionTableNames="stgborrowerdetailrejection",
                    MsgStruct = "Borrower Detail"
                },
                new()
                {
                    TableName = "stgborrowerloan",
                    RejectionTableNames= "stgborrowerloanrejection",
                    MsgStruct = "Borrower Loan"
                },
                new()
                {
                    TableName = "stgborrowermortgage",
                    RejectionTableNames ="stgborrowermortgage",
                    MsgStruct = "Borrower Mortgage"
                },
                new()
                {
                    TableName = "stgborrowermortgageother",
                    RejectionTableNames="stgborrowermortgageotherrejection",
                    MsgStruct = "Borrower Mortgage Other"
                },
                new()
                {
                    TableName = "stgcoborrowerdetails",
                    RejectionTableNames = "stgcoborrowerdetailsrejection",
                    MsgStruct = "Co Borrower Details"
                }
            };

            var serverDetails = await serverDetailRepository.GetServerDetailsAsync();

            foreach (var server in serverDetails)
            {
                using var clientConnection = context.CreateClientConnection(
                    server.ServerIp,
                    server.DbName,
                    server.ServerName,
                    server.ServerPassword);

                foreach (var table in tables)
                {
                    string query = $"SELECT * FROM db_a927ee_stgcomp.{table.TableName}";
                    var clientData = await clientConnection.QueryAsync<dynamic>(query);
                    await BulkInsertWithValidationAsync(context.CreateConnection().ConnectionString, table.TableName, table.RejectionTableNames, clientData,1);
                }
            }
        }

        public async Task BulkInsertWithValidationAsync(
                string connectionString,
                string tableName,
                string rejectionTableName,
                IEnumerable<dynamic> data,
                int createdBy, // Pass the user ID creating the records
                int batchSize = 1000)
        {
            if (data == null || !data.Any()) return;

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var validRecords = new List<dynamic>();
            var rejectedRecords = new List<dynamic>();

            // Validate each record and segregate
            foreach (var record in data)
            {
                var businessValidation = ValidateBusinessRules(record);
                var constraintValidation = ValidateConstraints(record);

                if (businessValidation.Item1 && constraintValidation.Item1)
                {
                    validRecords.Add(record);
                }
                else
                {
                    // Combine rejection reasons
                    var rejectionReason = new StringBuilder();
                    if (!businessValidation.Item1)
                        rejectionReason.AppendLine($"Business Validation Failed: {businessValidation.Item2}");
                    if (!constraintValidation.Item1)
                        rejectionReason.AppendLine($"Constraint Validation Failed: {constraintValidation.Item2}");

                    // Prepare rejected record
                    var rejectedRecord = new ExpandoObject() as IDictionary<string, object>;
                    foreach (var kvp in (IDictionary<string, object>)record)
                        rejectedRecord[kvp.Key] = kvp.Value;

                    rejectedRecord["RejectedReason"] = rejectionReason.ToString();
                    rejectedRecord["ValidationType"] = !businessValidation.Item1 ? "Business" : "Constraint";
                    rejectedRecord["IsValidated"] = false;
                    rejectedRecord["CreatedBy"] = createdBy;
                    rejectedRecord["CreatedDate"] = DateTime.UtcNow;

                    rejectedRecords.Add(rejectedRecord);
                }
            }

            // Insert valid records into the main table in batches
            if (validRecords.Any())
            {
                foreach (var batch in validRecords.Batch(batchSize))
                {
                    await InsertRecordsAsync(connection, tableName, batch);
                }
            }

            // Insert rejected records into the rejection table in batches
            if (rejectedRecords.Any())
            {
                foreach (var batch in rejectedRecords.Batch(batchSize))
                {
                    await InsertRecordsAsync(connection, rejectionTableName, batch);
                }
            }
        }

        private (bool isValid, string reason) ValidateBusinessRules(dynamic record)
        {
            var reason = new StringBuilder();

            // CIN Validation
            if (string.IsNullOrWhiteSpace(record.scin) && string.IsNullOrWhiteSpace(record.sbpanno))
                reason.AppendLine("CIN and PAN cannot both be blank.");

            // PAN Conditional Validation
            if (!string.IsNullOrWhiteSpace(record.scin) && string.IsNullOrWhiteSpace(record.sbpanno))
                reason.AppendLine("PAN is mandatory if CIN is provided.");

            // Date Validation
            if (string.IsNullOrWhiteSpace(record.dbdob) || !DateTime.TryParse(record.dbdob.ToString(), out DateTime _))
                reason.AppendLine("Invalid Date or blank Date field.");

            // Monthly Income Validation
            if (record.nbmonthlyincome == null || record.nbmonthlyincome < 0)
                reason.AppendLine("Monthly income must be numeric and >= 0.");

            // Other Field Validations
            if (string.IsNullOrWhiteSpace(record.sbname))
                reason.AppendLine("Primary Borrower Name cannot be blank.");
            if (string.IsNullOrWhiteSpace(record.dbdob) || !DateTime.TryParse(record.dbdob.ToString(), out DateTime _))
                reason.AppendLine("Primary Borrower Date of Birth is invalid or blank.");
            if (string.IsNullOrWhiteSpace(record.saadhaar))
                reason.AppendLine("Aadhaar must not be blank.");
            if (string.IsNullOrWhiteSpace(record.sbgender))
                reason.AppendLine("Gender must not be blank.");

            return (reason.Length == 0, reason.ToString());
        }

        private (bool isValid, string reason) ValidateConstraints(dynamic record)
        {
            var reason = new StringBuilder();

            // Example: Citizenship Validation (Check against Master Values)
            if (!IsValidMasterValue("Citizenship", record.sbcitizenship))
                reason.AppendLine("Invalid Citizenship value.");

            // Gender Validation
            if (!IsValidMasterValue("Gender", record.sbgender))
                reason.AppendLine("Invalid Gender value.");

            // Occupation Validation
            if (!IsValidMasterValue("Occupation", record.sboccupation))
                reason.AppendLine("Invalid Occupation value.");

            return (reason.Length == 0, reason.ToString());
        }

        private async Task InsertRecordsAsync(MySqlConnection connection, string tableName, IEnumerable<dynamic> records)
        {
            if (!records.Any()) return;

            var insertQuery = new StringBuilder();
            var parameters = new List<MySqlParameter>();
            int counter = 0;

            // Use the first record as a reference for column names
            dynamic firstItem = records.First();
            if (firstItem is IDictionary<string, object> dictionary)
            {
                var columns = dictionary.Keys.ToArray();

                // Build the insert query
                insertQuery.Append($"INSERT INTO {tableName} (");
                insertQuery.Append(string.Join(", ", columns));
                insertQuery.Append(") VALUES ");

                foreach (var record in records)
                {
                    var values = new List<string>();
                    if (record is IDictionary<string, object> recordDictionary)
                    {
                        foreach (var column in columns)
                        {
                            var paramName = $"@{column}{counter}";
                            values.Add(paramName);

                            var propertyValue = recordDictionary.ContainsKey(column)
                                ? recordDictionary[column]
                                : DBNull.Value;

                            parameters.Add(new MySqlParameter(paramName, propertyValue ?? DBNull.Value));
                        }
                    }

                    insertQuery.Append($"({string.Join(", ", values)})");
                    if (counter < records.Count() - 1)
                        insertQuery.Append(", ");

                    counter++;
                }

                insertQuery.Append(";");

                // Execute the insert query
                using var command = new MySqlCommand(insertQuery.ToString(), connection);
                command.Parameters.AddRange(parameters.ToArray());
                try
                {
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error inserting records into {tableName}: {ex.Message}", ex);
                }
            }
            else
            {
                throw new InvalidOperationException("The first item in the data is not of expected type IDictionary<string, object>.");
            }
        }

        private bool IsValidMasterValue(string columnName, string value)
        {
            // Replace this with actual master value checks (e.g., a database query)
            var masterValues = new Dictionary<string, List<string>>
            {
                { "Citizenship", new List<string> { "Indian", "NRI", "OCI" } },
                { "Gender", new List<string> { "Male", "Female", "Other" } },
                { "Occupation", new List<string> { "Salaried", "Self-Employed", "Retired" } }
            };

            return masterValues.TryGetValue(columnName, out var validValues) && validValues.Contains(value);
        }
    }
}
