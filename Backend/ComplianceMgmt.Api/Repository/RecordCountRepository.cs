using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;
using MySql.Data.MySqlClient;
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
                    //await BulkInsertAsync(context.CreateConnection().ConnectionString, table.TableName, clientData);
                    await BulkInsertWithValidationAsync(context.CreateConnection().ConnectionString, table.TableName, table.RejectionTableNames, clientData,
                        validateRecord: record =>
                        {
                            // Example validation logic
                            if (record.sbcitizenship == null)
                                return (isValid: false, errorMessage: "sbcitizenship cannot be null");

                            if (record.Aadhaar == null)
                                return (isValid: false, errorMessage: "Aadhaar cannot be null");

                            if (record.IdType == null)
                                return (isValid: false, errorMessage: "IdType cannot be null");

                            if (record.BMonthlyIncome >= 0)
                                return (isValid: false, errorMessage: "BMonthlyIncome cannot be negative");

                            return (isValid: true, errorMessage: string.Empty);
                        });
                }
            }
        }

        //public async Task BulkInsertAsync(string connectionString, string tableName, IEnumerable<dynamic> data)
        //{
        //    if (data == null || !data.Any()) return;

        //    using var connection = new MySqlConnection(connectionString);
        //    await connection.OpenAsync();

        //    var insertQuery = new StringBuilder();
        //    var parameters = new List<MySqlParameter>();
        //    int counter = 0;

        //    // Use the first item as a reference to get columns
        //    dynamic firstItem = data.First();

        //    // Check if firstItem is a dictionary and extract keys (columns)
        //    if (firstItem is IDictionary<string, object> dictionary)
        //    {
        //        var columns = dictionary.Keys.ToArray(); // Get column names

        //        // Start building the insert query
        //        insertQuery.Append($"INSERT INTO {tableName} (");
        //        insertQuery.Append(string.Join(", ", columns));
        //        insertQuery.Append(") VALUES ");

        //        // Loop through all data and build values
        //        foreach (var record in data)
        //        {
        //            var values = new List<string>();

        //            if (record is IDictionary<string, object> recordDictionary)
        //            {
        //                foreach (var column in columns)
        //                {
        //                    var paramName = $"@{column}{counter}";
        //                    values.Add(paramName);

        //                    // Get the value for each column
        //                    var propertyValue = recordDictionary.ContainsKey(column)
        //                        ? recordDictionary[column]
        //                        : DBNull.Value;

        //                    // Add parameter for each column
        //                    parameters.Add(new MySqlParameter(paramName, propertyValue ?? DBNull.Value));
        //                }
        //            }

        //            insertQuery.Append($"({string.Join(", ", values)})");

        //            if (counter < data.Count() - 1)
        //                insertQuery.Append(", ");

        //            counter++;
        //        }

        //        insertQuery.Append(";");

        //        // Execute the insert query
        //        using var command = new MySqlCommand(insertQuery.ToString(), connection);
        //        command.Parameters.AddRange(parameters.ToArray());
        //        try
        //        {
        //            await command.ExecuteNonQueryAsync();
        //        }
        //        catch (Exception ex)
        //        {

        //        }
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException("The first item in the data is not of expected type IDictionary<string, object>.");
        //    }
        //}

        public async Task BulkInsertWithValidationAsync(
        string connectionString,
        string tableName,
        string rejectionTableName,
        IEnumerable<dynamic> data,
        Func<dynamic, (bool isValid, string errorMessage)> validateRecord)
        {
            if (data == null || !data.Any()) return;

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var validRecords = new List<dynamic>();
            var rejectedRecords = new List<dynamic>();

            // Validate each record and segregate
            foreach (var record in data)
            {
                var validation = validateRecord(record);
                if (validation.Item1)
                {
                    validRecords.Add(record);
                }
                else
                {
                    // Add error details to the record for rejection table
                    var rejectedRecord = new Dictionary<string, object>(record);
                    rejectedRecord["ErrorMessage"] = validation.Item2;
                    rejectedRecords.Add(rejectedRecord);
                }
            }

            // Insert valid records into the main table
            if (validRecords.Any())
            {
                await InsertRecordsAsync(connection, tableName, validRecords);
            }

            // Insert rejected records into the rejection table
            if (rejectedRecords.Any())
            {
                await InsertRecordsAsync(connection, rejectionTableName, rejectedRecords);
            }
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
                    // Log or handle query execution errors
                    throw new Exception($"Error inserting records into {tableName}: {ex.Message}", ex);
                }
            }
            else
            {
                throw new InvalidOperationException("The first item in the data is not of expected type IDictionary<string, object>.");
            }
        }
    }
}
