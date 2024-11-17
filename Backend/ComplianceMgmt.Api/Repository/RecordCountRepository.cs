using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;

namespace ComplianceMgmt.Api.Repository
{
    public class RecordCountRepository(IConfiguration configuration, ComplianceMgmtDbContext context, IServerDetailRepository serverDetailRepository) : IRecordCountRepository
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

        public async Task FetchDataFromClientDatabasesAsync()
        {
            var serverDetails = await serverDetailRepository.GetServerDetailsAsync();

            foreach (var server in serverDetails)
            {
                using (var clientConnection = context.CreateClientConnection(
                    server.ServerIp,
                    server.DbName,
                    server.ServerName,
                    server.ServerPassword))
                {
                    // Example query to fetch client-specific data
                    var clientDataQuery = "SELECT * FROM db_a927ee_stgcomp.stgborrowerdetail";
                    var clientData = await clientConnection.QueryAsync<StgBorrowerDetail>(clientDataQuery);


                   await BulkInsertAsync(context.CreateConnection().ConnectionString, clientData.AsList());
                    // Process or save client data into your MySQL database
                    //await SaveClientDataToMasterDatabase(clientData);
                }
            }
        }

        public async Task SaveClientDataToMasterDatabase(IEnumerable<StgBorrowerDetail> borrowerDetail)
        {
            using (var connection = context.CreateConnection())
            {
                var insertQuery = new StringBuilder();
                insertQuery.Append("INSERT INTO db_a927ee_comlian.stgborrowerdetail (");
                insertQuery.Append("RowNo, Date, BankId, Cin, BName, BDob, sbcitizenship, BPanNo, Aadhaar, IdType, IdNumber, ");
                insertQuery.Append("BMonthlyIncome, BReligion, BCast, BGender, BOccupation, IsValidated, RejectedReason, ValidatedDate) ");
                insertQuery.Append("VALUES (");
                insertQuery.Append("@RowNo, @Date, @BankId, @Cin, @BName, @BDob, @sbcitizenship, @BPanNo, @Aadhaar, @IdType, @IdNumber, ");
                insertQuery.Append("@BMonthlyIncome, @BReligion, @BCast, @BGender, @BOccupation, @IsValidated, @RejectedReason, @ValidatedDate);");
                
                await connection.ExecuteAsync(insertQuery.ToString(), borrowerDetail);
            }
        }

        public async Task BulkInsertAsync(string connectionString, List<StgBorrowerDetail> borrowerDetails)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var insertQuery = new StringBuilder();
                insertQuery.Append("INSERT INTO db_a927ee_comlian.stgborrowerdetail (");
                insertQuery.Append("RowNo, Date, BankId, Cin, BName, BDob, sbcitizenship, BPanNo, Aadhaar, IdType, IdNumber, ");
                insertQuery.Append("BMonthlyIncome, BReligion, BCast, BGender, BOccupation, IsValidated, RejectedReason, ValidatedDate) ");
                insertQuery.Append("VALUES ");

                var parameters = new List<MySqlParameter>();
                int counter = 0;

                foreach (var borrower in borrowerDetails)
                {
                    insertQuery.Append($"(@RowNo{counter}, @Date{counter}, @BankId{counter}, @Cin{counter}, @BName{counter}, @BDob{counter}, ");
                    insertQuery.Append($"@SBCitizenship{counter}, @BPanNo{counter}, @Aadhaar{counter}, @IdType{counter}, @IdNumber{counter}, ");
                    insertQuery.Append($"@BMonthlyIncome{counter}, @BReligion{counter}, @BCast{counter}, @BGender{counter}, @BOccupation{counter}, ");
                    insertQuery.Append($"@IsValidated{counter}, @RejectedReason{counter}, @ValidatedDate{counter})");

                    if (counter < borrowerDetails.Count - 1)
                        insertQuery.Append(", ");

                    // Add parameters for this borrower
                    parameters.AddRange(
                    [
                        new MySqlParameter($"@RowNo{counter}", borrower.RowNo),
                        new MySqlParameter($"@Date{counter}", borrower.Date),
                        new MySqlParameter($"@BankId{counter}", borrower.BankId),
                        new MySqlParameter($"@Cin{counter}", borrower.Cin),
                        new MySqlParameter($"@BName{counter}", borrower.BName),
                        new MySqlParameter($"@BDob{counter}", borrower.BDob),
                        new MySqlParameter($"@SBCitizenship{counter}", borrower.SBCitizenship),
                        new MySqlParameter($"@BPanNo{counter}", borrower.BPanNo),
                        new MySqlParameter($"@Aadhaar{counter}", borrower.Aadhaar),
                        new MySqlParameter($"@IdType{counter}", borrower.IdType),
                        new MySqlParameter($"@IdNumber{counter}", borrower.IdNumber),
                        new MySqlParameter($"@BMonthlyIncome{counter}", borrower.BMonthlyIncome),
                        new MySqlParameter($"@BReligion{counter}", borrower.BReligion),
                        new MySqlParameter($"@BCast{counter}", borrower.BCast),
                        new MySqlParameter($"@BGender{counter}", borrower.BGender),
                        new MySqlParameter($"@BOccupation{counter}", borrower.BOccupation),
                        new MySqlParameter($"@IsValidated{counter}", borrower.IsValidated ?? (object)DBNull.Value),
                        new MySqlParameter($"@RejectedReason{counter}", borrower.RejectedReason ?? (object)DBNull.Value),
                        new MySqlParameter($"@ValidatedDate{counter}", borrower.ValidatedDate ?? (object)DBNull.Value),
                    ]);

                    counter++;
                }

                insertQuery.Append(";");

                using (var command = new MySqlCommand(insertQuery.ToString(), connection))
                {
                    command.Parameters.AddRange(parameters.ToArray());
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
