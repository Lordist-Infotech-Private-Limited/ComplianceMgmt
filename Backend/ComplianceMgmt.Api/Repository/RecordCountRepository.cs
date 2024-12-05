using ComplianceMgmt.Api.Extensions;
using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Serilog;
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
                var formattedDate = date.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);

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
                new() { TableName = "stgborrowerdetail", RejectionTableNames = "stgborrowerdetailrejection", MsgStruct = "Borrower Detail" },
                new() { TableName = "stgborrowerloan", RejectionTableNames = "stgborrowerloanrejection", MsgStruct = "Borrower Loan" },
                new() { TableName = "stgborrowermortgage", RejectionTableNames = "stgborrowermortgagerejection", MsgStruct = "Borrower Mortgage" },
                new() { TableName = "stgborrowermortgageother", RejectionTableNames = "stgborrowermortgageotherrejection", MsgStruct = "Borrower Mortgage Other" },
                new() { TableName = "stgcoborrowerdetails", RejectionTableNames = "stgcoborrowerdetailsrejection", MsgStruct = "Co Borrower Details" }
            };

            var serverDetails = await serverDetailRepository.GetServerDetailsAsync();

            foreach (var server in serverDetails)
            {
                try
                {
                    using var clientConnection = context.CreateClientConnection(
                        server.ServerIp,
                        server.DbName,
                        server.ServerName,
                        server.ServerPassword);

                    foreach (var table in tables)
                    {
                        try
                        {
                            string query = $"SELECT * FROM db_a927ee_stgcomp.{table.TableName}";

                            // Try fetching data
                            var clientData = await clientConnection.QueryAsync<dynamic>(query);

                            // Bulk insert with validation
                            await BulkInsertWithValidationAsync(
                                context.CreateConnection().ConnectionString,
                                table.TableName,
                                table.RejectionTableNames,
                                clientData,
                                1);
                        }
                        catch (Exception ex)
                        {
                            // Log table-specific errors
                            Log.Error(ex, "Error fetching or processing data for table {TableName} on server {ServerName}", table.TableName, server.ServerName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log server-specific errors
                    Log.Error(ex, "Error connecting to server {ServerName} at IP {ServerIp}", server.ServerName, server.ServerIp);
                }
            }

            return true;
        }

        //public async Task<bool> FetchAndInsertAllTablesAsync()
        //{
        //    var tables = new List<MsgStructure>
        //    {
        //        new()
        //        {
        //            TableName = "stgborrowerdetail",
        //            RejectionTableNames="stgborrowerdetailrejection",
        //            MsgStruct = "Borrower Detail"
        //        },
        //        new()
        //        {
        //            TableName = "stgborrowerloan",
        //            RejectionTableNames= "stgborrowerloanrejection",
        //            MsgStruct = "Borrower Loan"
        //        },
        //        new()
        //        {
        //            TableName = "stgborrowermortgage",
        //            RejectionTableNames ="stgborrowermortgagerejection",
        //            MsgStruct = "Borrower Mortgage"
        //        },
        //        new()
        //        {
        //            TableName = "stgborrowermortgageother",
        //            RejectionTableNames="stgborrowermortgageotherrejection",
        //            MsgStruct = "Borrower Mortgage Other"
        //        },
        //        new()
        //        {
        //            TableName = "stgcoborrowerdetails",
        //            RejectionTableNames = "stgcoborrowerdetailsrejection",
        //            MsgStruct = "Co Borrower Details"
        //        }
        //    };

        //    var serverDetails = await serverDetailRepository.GetServerDetailsAsync();

        //    foreach (var server in serverDetails)
        //    {
        //        using var clientConnection = context.CreateClientConnection(
        //            server.ServerIp,
        //            server.DbName,
        //            server.ServerName,
        //            server.ServerPassword);

        //        foreach (var table in tables)
        //        {
        //            string query = $"SELECT * FROM db_a927ee_stgcomp.{table.TableName}";
        //            var clientData = await clientConnection.QueryAsync<dynamic>(query);
        //            await BulkInsertWithValidationAsync(context.CreateConnection().ConnectionString, table.TableName, table.RejectionTableNames, clientData, 1);
        //        }
        //    }

        //    return true;
        //}

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

            if (new[] { "stgborrowerdetail", "stgborrowerloan", "stgcoborrowerdetails", "stgborrowermortgage", "stgborrowermortgageother" }.Contains(tableName))
            {
                // Date Validation
                if ((record.Date is MySql.Data.Types.MySqlDateTime mySqlDate && (mySqlDate.IsNull || !mySqlDate.IsValidDateTime)) ||
                    (record.Date is string dateString && string.IsNullOrWhiteSpace(dateString)) || record.Date == null)
                {
                    reason.AppendLine("Date cannot be blank.");
                }
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
                    reason.AppendLine("Compliance Mgmt System Generated Unique ID cannot be blank.");

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

                if (string.IsNullOrWhiteSpace(record.AccntClosedDurMth))
                    reason.AppendLine("Account closed during the month cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.AssetCat))
                    reason.AppendLine("Asset Category/Classification (IRAC) cannot be blank.");

                if (record.ClassDate == null)
                    reason.AppendLine("Date of Classification cannot be blank.");
                else if (!DateTime.TryParseExact(record.ClassDate.ToString(), "dd-MM-yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _))
                    reason.AppendLine("Invalid value for Date of Classification.");

                if (record.Pd != null && record.Pd < 0)
                    reason.AppendLine("Probability of Default (PD) must be numeric and >= 0.");

                if (record.Lgd != null && record.Lgd < 0)
                    reason.AppendLine("Loss Given Default (LGD) must be numeric and >= 0.");

                if (record.ProvAmt == null || record.ProvAmt < 0)
                    reason.AppendLine("Provisions(Rs.) must be numeric and >= 0.");

                if (string.IsNullOrWhiteSpace(record.RefFromNhb))
                    reason.AppendLine("Refinance from NHB cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.UnderPmayClss))
                    reason.AppendLine("Benefit Availed under PMAY-CLSS cannot be blank.");
            }
            else if (tableName == "stgcoborrowerdetails")
            {
                if (record.BankId == null)
                    reason.AppendLine("Compliance Mgmt System Generated Unique ID cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.CbName))
                    reason.AppendLine("Co-Borrower Name cannot be blank.");

                if (record.CbDob != null && !DateTime.TryParseExact(record.CbDob.ToString(), "dd-MM-yyyy hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _))
                    reason.AppendLine("Invalid value for Co-Borrower Date of Birth.");

                if (string.IsNullOrWhiteSpace(record.CbCitizenship))
                    reason.AppendLine("Co-Borrower Citizenship cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.Cin) && string.IsNullOrWhiteSpace(record.CbPanNo))
                    reason.AppendLine("PAN is mandatory if CIN is not provided.");

                if (string.IsNullOrWhiteSpace(record.CbAadhaar))
                    reason.AppendLine("Aadhaar cannot be blank.");

                if (record.CbMonthlyIncome != null || record.CbMonthlyIncome < 0)
                    reason.AppendLine("Co-Borrower Monthly income must be numeric and >= 0.");
            }
            else if (tableName == "stgborrowermortgage")
            {
                if (record.BankId == null)
                    reason.AppendLine("Compliance Mgmt System Generated Unique ID cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.BLoanNo))
                    reason.AppendLine("Loan Account Number cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.PropType))
                    reason.AppendLine("Type of Property cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.PropAdd))
                    reason.AppendLine("Asset / Property Address cannot be blank.");

                if (record.LandArea != null && record.LandArea < 0)
                    reason.AppendLine("Area of Land must be numeric and >= 0.");

                if (record.BuildingArea != null && record.BuildingArea < 0)
                    reason.AppendLine("Carpet Area of Building must be numeric and >= 0.");

                if (string.IsNullOrWhiteSpace(record.TownName))
                    reason.AppendLine("Name of Town/Village cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.District))
                    reason.AppendLine("District cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.State))
                    reason.AppendLine("State cannot be blank.");

                if (record.Pin != null && record.Pin >= 100000 && record.Pin <= 999999)
                    reason.AppendLine("Pincode must be of 6 digits.");

                if (string.IsNullOrWhiteSpace(record.RuralUrban))
                    reason.AppendLine("Urban or Rural cannot be blank.");

                if (record.PropValAtSanct == null || record.PropValAtSanct < 0)
                    reason.AppendLine("Value of Property at the Time of Sanction must be numeric and >= 0.");

                if (record.PresentValue != null || record.PresentValue < 0)
                    reason.AppendLine("Present Value of Property must be numeric and >= 0.");

                if (string.IsNullOrWhiteSpace(record.Insurance))
                    reason.AppendLine("Asset Insurance cannot be blank.");
            }
            else if (tableName == "stgborrowermortgageother")
            {
                if (record.BankId == null)
                    reason.AppendLine("Compliance Mgmt System Generated Unique ID cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.BLoanNo))
                    reason.AppendLine("Loan Account Number cannot be blank.");

                if (string.IsNullOrWhiteSpace(record.CollType))
                    reason.AppendLine("Type of Other Collateral cannot be blank.");

                if (record.ValueAtSanct == null || record.ValueAtSanct < 0)
                    reason.AppendLine("Value at the Time of Sanction must be numeric and >= 0.");

                if (record.PresentValue != null || record.PresentValue < 0)
                    reason.AppendLine("Present Value must be numeric and >= 0.");

            }

            return (reason.Length == 0, reason.ToString());
        }

        private (bool isValid, string reason) ValidateConstraints(string tableName, dynamic record, ref List<MasterData> masterData)
        {
            var reason = new StringBuilder();
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

                // Gender
                if (record.BGender != null && !masterData.Any(data => string.Equals(data.MasterName, "Gender", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(data.Code, record.BGender, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Gender value.");

                // Occupation
                if (record.BOoccupation != null && !masterData.Any(data => string.Equals(data.MasterName, "Occupation", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(data.Code, record.BOoccupation, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Occupation value.");

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

                // Account closed during the month
                if (record.AccntClosedDurMth != null && !masterData.Any(data => string.Equals(data.MasterName, "Yes/No", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.AccntClosedDurMth, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Account closed during the month value.");

                // Asset Category/Classification (IRAC)
                if (record.AssetCat != null && !masterData.Any(data => string.Equals(data.MasterName, "Asset Category/Classification", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.AssetCat, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Asset Category/Classification (IRAC) value.");

                // Expected Credit Loss (ECL)
                if (record.Ecl != null && !masterData.Any(data => string.Equals(data.MasterName, "Expected Credit Loss (ECL)", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.Ecl, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Expected Credit Loss (ECL) value.");

                // Refinance from NHB
                if (record.RefFromNhb != null && !masterData.Any(data => string.Equals(data.MasterName, "Yes/No", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.RefFromNhb, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Refinance from NHB value.");

                // Refinance from NHB
                if (record.UnderPmayClss != null && !masterData.Any(data => string.Equals(data.MasterName, "Yes/No", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.UnderPmayClss, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Benefit Availed under PMAY-CLSS value.");

                // Refinance from NHB
                if (record.SerfaseiAct != null && !masterData.Any(data => string.Equals(data.MasterName, "Yes/No", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.SerfaseiAct, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Notice(s) issued u/s 13(2) SARFAESI Act value.");

                // Refinance from NHB
                if (record.StayGranted != null && !masterData.Any(data => string.Equals(data.MasterName, "Yes/No", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.StayGranted, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid value for stay has been granted by DRT/DRAT.");
            }
            else if (tableName == "stgcoborrowerdetails")
            {
                // Citizenship
                if (record.CbCitizenship != null && !masterData.Any(data => string.Equals(data.MasterName, "Citizenship", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.CbCitizenship, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Co-Borrower Citizenship value.");

                // Aadhaar
                if (record.CbAadhaar != null && !masterData.Any(data => string.Equals(data.MasterName, "Yes/No", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.CbAadhaar, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Aadhaar value.");

                // Gender
                if (record.CbGender != null && !masterData.Any(data => string.Equals(data.MasterName, "Gender", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(data.Code, record.CbGender, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Gender value.");

                // Occupation
                if (record.CbOccupation != null && !masterData.Any(data => string.Equals(data.MasterName, "Occupation", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(data.Code, record.CbOccupation, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Occupation value.");

                // Religion
                if (record.CbReligion != null && !masterData.Any(data => string.Equals(data.MasterName, "Religion", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(data.Code, record.CbReligion, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Religion value.");

                // Cast
                if (record.CbCast != null && !masterData.Any(data => string.Equals(data.MasterName, "Cast", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(data.Code, record.CbCast, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Cast value.");

            }
            else if (tableName == "stgborrowermortgage")
            {
                // Type of Property
                if (record.PropType != null && !masterData.Any(data => string.Equals(data.MasterName, "Type of Property", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.PropType, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Type of Property value.");

                // District
                if (record.District != null && !masterData.Any(data => string.Equals(data.MasterName, "District", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.District, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid District value.");

                // State
                if (record.State != null && !masterData.Any(data => string.Equals(data.MasterName, "STATE", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.State, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid State value.");

                // Rural/Urban
                if (record.RuralUrban != null && !masterData.Any(data => string.Equals(data.MasterName, "Rural/Urban", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.RuralUrban, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid State value.");

                // Asset Insurance
                if (record.Insurance != null && !masterData.Any(data => string.Equals(data.MasterName, "Yes/No", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.Insurance, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Asset Insurance value.");
            }
            else if (tableName == "stgborrowermortgageother")
            {
                // Type of Other Collateral
                if (record.CollType != null && !masterData.Any(data => string.Equals(data.MasterName, "Type of Other Collateral", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(data.Code, record.CollType, StringComparison.OrdinalIgnoreCase)))
                    reason.AppendLine("Invalid Type of Other Collateral value.");

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
                Log.Information($"Parameters: {string.Join(", ", parameters.Select(p => $"{p.ParameterName}: {p.Value}"))}");

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
