using ComplianceMgmt.Api.Extensions;
using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Dynamic;
using System.Globalization;
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

        public async Task<bool> FetchAndInsertAllTablesAsync()
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
                    RejectionTableNames ="stgborrowermortgagerejection",
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
                    await BulkInsertWithValidationAsync(context.CreateConnection().ConnectionString, table.TableName, table.RejectionTableNames, clientData, 1);
                }
            }

            return true;
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
            
            var jsonFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Resources" ,"masters.json");
            
            if (!File.Exists(jsonFilePath))
                throw new FileNotFoundException($"Masters.json file not found.");

            var masterData = new List<MasterData>();

            var jsonContent = File.ReadAllText(jsonFilePath);
            masterData = JsonConvert.DeserializeObject<List<MasterData>>(jsonContent);


            // Validate each record and segregate
            foreach (var record in data)
            {
                var businessValidation = ValidateBusinessRules(tableName,record);
                var constraintValidation = ValidateConstraints(tableName,record, ref masterData);

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

                    try
                    {
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
                    catch (Exception ex)
                    {
                        throw new Exception($"Error inserting records into {tableName}: {ex.Message}", ex);
                    }
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

        private (bool isValid, string reason) ValidateBusinessRules(string tableName, dynamic record)
        {
            var reason = new StringBuilder();

            if (new[] { "stgborrowerdetail", "stgborrowerloan" }.Contains(tableName))
            {
                // Date Validation
                if (record.Date == null)
                    reason.AppendLine("Date cannot be blank.");
            }

            if (tableName == "stgborrowerdetail")
            {

                // CIN Validation
                if (string.IsNullOrWhiteSpace(record.Cin) && string.IsNullOrWhiteSpace(record.BPanNo))
                    reason.AppendLine("CIN and PAN cannot both be blank.");

                // PAN Conditional Validation
                if (string.IsNullOrWhiteSpace(record.Cin) && string.IsNullOrWhiteSpace(record.BPanNo))
                    reason.AppendLine("PAN is mandatory if CIN is not provided.");

                // DOB Validation
                if (record.BDob == null)
                    reason.AppendLine("Primary Borrower Date of Birth cannot be blank.");
                else if (!DateTime.TryParseExact(record.BDob.ToString(), "dd-MM-yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _))
                    reason.AppendLine("Invalid value for Primary Borrower Date of Birth.");

                // Monthly Income Validation
                if (record.BMonthlyIncome == null || record.BMonthlyIncome < 0)
                    reason.AppendLine("Monthly income must be numeric and >= 0.");

                // Other Field Validations
                if (string.IsNullOrWhiteSpace(record.BName))
                    reason.AppendLine("Primary Borrower Name cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.BCitizenship))
                    reason.AppendLine("Primary Borrower Citizenship cannot be blank.");
                
                //if (record.BDob == null || !DateTime.TryParse(record.BDob.ToString(), out DateTime _))
                //    reason.AppendLine("Primary Borrower Date of Birth is invalid or blank.");
                
                if (string.IsNullOrWhiteSpace(record.Aadhaar))
                    reason.AppendLine("Aadhaar cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.BGender))
                    reason.AppendLine("Gender cannot be blank.");
            }
            else if (tableName == "stgborrowerloan")
            {
                if (record.BankId == null)
                    reason.AppendLine("Bank ID cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.BLoanNo))
                    reason.AppendLine("Loan Account Number cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.LoanType))
                    reason.AppendLine("Type of Loan cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.LoanPurpose))
                    reason.AppendLine("Loan Purpose cannot be blank.");
                else
                {
                    // Number of Dwelling Unit(DU)
                    string loanPurpose = record.LoanPurpose;

                    if (new[] { "POL-01", "POL-02", "POL-03", "POL-04", "POL-05" }.Contains(loanPurpose))
                    {
                        if (string.IsNullOrWhiteSpace(record.DwellingUnit))
                            reason.AppendLine("Number of Dwelling Unit (DU) cannot be blank.");
                    }
                }

                if (record.SanctAmount == null || record.SanctAmount <= 0)
                    reason.AppendLine("Sanctioned Amount (Rs.) must be numeric and > 0.");

                // Date of Sanction Validation
                if (record.SanctDate == null)
                    reason.AppendLine("Date of Sanction cannot be blank.");
                else if (!DateTime.TryParseExact(record.SanctDate.ToString(), "dd-MM-yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _))
                    reason.AppendLine("Invalid value for Date of Sanction.");

                if (record.MoratoriumPeriod == null || record.MoratoriumPeriod < 0)
                    reason.AppendLine("Moratorium Period must be numeric and >= 0.");

                if (record.LoanTenCont == null || record.LoanTenCont < 0)
                    reason.AppendLine("Loan Tenure - Contractual must be numeric and >= 0.");

                if (record.LoanTenResidual == null || record.LoanTenResidual < 0)
                    reason.AppendLine("Loan Tenure - Residual must be numeric and >= 0.");

                if (record.Roi == null || record.Roi <= 0)
                    reason.AppendLine("Rate of Interest must be numeric and > 0.00");

                if (string.IsNullOrWhiteSpace(record.IntType))
                    reason.AppendLine("Type of Interest cannot be blank.");

                if (record.Emi != null || record.Emi < 0)
                    reason.AppendLine("Equated Monthly Installment (EMI) must be numeric and >= 0.");

                if (record.PreEmi != null || record.PreEmi < 0)
                    reason.AppendLine("Pre-EMI Interest (PEMI) must be numeric and >= 0.");

                if (record.FirstDisbDate != null && !DateTime.TryParseExact(record.FirstDisbDate.ToString(), "dd-MM-yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _))
                    reason.AppendLine("Invalid value for Date of first Disbursement.");

                if (record.EmiStartDate != null && !DateTime.TryParseExact(record.EmiStartDate.ToString(), "dd-MM-yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _))
                    reason.AppendLine("Invalid value for EMI Start Date.");

                if (record.PreEmiStartDate != null && !DateTime.TryParseExact(record.PreEmiStartDate.ToString(), "dd-MM-yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _))
                    reason.AppendLine("Invalid value for Pre-EMI interest (PEMI) Start Date.");

                if (record.LoanDisbDuringMonth == null || record.LoanDisbDuringMonth < 0)
                    reason.AppendLine("Loan Amount Disbursed during the Month must be numeric and >= 0.");

                if (record.CummuLoanDisb == null || record.CummuLoanDisb < 0)
                    reason.AppendLine("Cumulative Loan Disbursed must be numeric and >= 0.");

                if (string.IsNullOrWhiteSpace(record.LoanStatus))
                    reason.AppendLine("Loan Status cannot be blank.");

                if (record.AmtUnderCons != null || record.AmtUnderCons < 0)
                    reason.AppendLine("Amount outstanding under consideration must be numeric and >= 0.");

                if (string.IsNullOrWhiteSpace(record.PartyName))
                    reason.AppendLine("Counter Party cannot be blank.");

                if (record.AmoutUnderGuar != null || record.AmoutUnderGuar < 0)
                    reason.AppendLine("Amount outstanding under Guarantee must be numeric and >= 0.");

                if (record.TotalLoanOut == null)
                    reason.AppendLine("Total Loan Outstanding cannot be blank.");

                if (record.POut == null)
                    reason.AppendLine("Principal Outstanding cannot be blank.");

                if (record.IOut == null)
                    reason.AppendLine("Interest Outstanding cannot be blank.");

                if (record.OtherDueOut == null)
                    reason.AppendLine("Other Dues cannot be blank.");
            
                if (record.LoanRepayDurMth == null || record.LoanRepayDurMth < 0)
                    reason.AppendLine("Loan Repayment During the Month must be numeric and >= 0.");

                if (record.TotalLoanOverDue == null || record.TotalLoanOverDue < 0)
                    reason.AppendLine("Total Loan Overdue must be numeric and >= 0.");

                if (record.POverDue == null || record.POverDue < 0)
                    reason.AppendLine("Principal Overdue must be numeric and >= 0.");

                if (record.IOverDue == null || record.IOverDue < 0)
                    reason.AppendLine("Interest Overdue must be numeric and >= 0.");

                if (record.OtherOverDue == null || record.OtherOverDue < 0)
                    reason.AppendLine("Other Dues Overdues must be numeric and >= 0.");
            }

            return (reason.Length == 0, reason.ToString());
        }

        private (bool isValid, string reason) ValidateConstraints(string tableName, dynamic record, ref List<MasterData> masterData)
        {
            var reason = new StringBuilder();

            /*
            // Example: Citizenship Validation (Check against Master Values)
            if (!IsValidMasterValue("Citizenship", record.BCitizenship))
                reason.AppendLine("Invalid Citizenship value.");

            // Gender Validation
            if (!IsValidMasterValue("Gender", record.BGender))
                reason.AppendLine("Invalid Gender value.");

            // Occupation Validation
            if (!IsValidMasterValue("Occupation", record.BOoccupation))
                reason.AppendLine("Invalid Occupation value.");
            */

            if (tableName == "stgborrowerdetail")
            {
                // Other ID Type
                if (record.IdType != null && record.IdType != "NULL" && !masterData.Any(data => string.Equals(data.MasterName, "Unique ID Type", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.IdType, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Other ID Type value.");

                // Citizenship
                if (record.BCitizenship != null && !masterData.Any(data => string.Equals(data.MasterName, "Citizenship", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(data.Code, record.BCitizenship, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Citizenship value.");

                // Citizenship
                if (record.BGender != null && !masterData.Any(data => string.Equals(data.MasterName, "Gender", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(data.Code, record.BGender, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Gender value.");

                // Occupation
                if (record.BOoccupation != null && !masterData.Any(data => string.Equals(data.MasterName, "Occupation", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(data.Code, record.BOoccupation, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Gender value.");

                // Religion
                if (record.BReligion != null && !masterData.Any(data => string.Equals(data.MasterName, "Religion", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(data.Code, record.BReligion, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Religion value.");

                // Cast
                if (record.BCast != null && !masterData.Any(data => string.Equals(data.MasterName, "Cast", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(data.Code, record.BCast, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Cast value.");
            }
            else if (tableName == "stgborrowerloan")
            {
                // Type of Loan
                if (record.LoanType != null && !masterData.Any(data => string.Equals(data.MasterName, "Type of Loan", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.LoanType, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Type of Loan value.");

                // Loan Purpose
                if (record.LoanPurpose != null && !masterData.Any(data => string.Equals(data.MasterName, "Purpose of Loan", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.LoanPurpose, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Type of Loan value.");

                // Number of Dwelling Unit(DU)
                if (record.DwellingUnit != null && !masterData.Any(data => string.Equals(data.MasterName, "Number of Dwelling Unit (DU)", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.DwellingUnit, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Type of Loan value.");

                // Type of Interest
                if (record.IntType != null && !masterData.Any(data => string.Equals(data.MasterName, "Type of Interest", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.IntType, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Type of Interest value.");

                // Loan Status
                if (record.LoanStatus != null && !masterData.Any(data => string.Equals(data.MasterName, "Loan Status", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.LoanStatus, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Loan Status value.");

                // Counter Party
                if (record.PartyName != null && !masterData.Any(data => string.Equals(data.MasterName, "Counter Party", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.PartyName, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Counter Party value.");

                // Mortgage Guarantee
                if (record.MortGuarantee != null && !masterData.Any(data => string.Equals(data.MasterName, "Mortgage Guarantee", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.MortGuarantee, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Mortgage Guarantee value.");
            }

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

                            // Handle invalid DateTime values
                            if (column == "Date" || column == "BDob")
                            {
                                if (propertyValue == null || !DateTime.TryParse(propertyValue.ToString(), out _))
                                    propertyValue = DBNull.Value;
                            }

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
                command.CommandTimeout = 1800; 
                Console.WriteLine($"Query: {insertQuery}");
                Console.WriteLine($"Parameters: {string.Join(", ", parameters.Select(p => $"{p.ParameterName}: {p.Value}"))}");

                try
                {
                    await command.ExecuteNonQueryAsync();
                }
                catch (MySqlException ex)
                {
                    throw new Exception($"MySQL Error: {ex.Message}\nQuery: {insertQuery}", ex);
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
