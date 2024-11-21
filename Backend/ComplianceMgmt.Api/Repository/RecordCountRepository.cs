using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
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
                new() { TableName = "stgborrowerdetail", MsgStruct = "Borrower Detail" },
                new() { TableName = "stgborrowerloan", MsgStruct = "Borrower Loan" },
                new() { TableName = "stgborrowermortgage", MsgStruct = "Borrower Mortgage" },
                new() { TableName = "stgborrowermortgageother", MsgStruct = "Borrower Mortgage Other" },
                new() { TableName = "stgcoborrowerdetails", MsgStruct = "Co Borrower Details" }
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
                    await BulkInsertAsync(context.CreateConnection().ConnectionString, table.TableName, clientData);
                }
            }
        }

        public async Task BulkInsertAsync(string connectionString, string tableName, IEnumerable<dynamic> data)
        {
            if (data == null || !data.Any()) return;

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var insertQuery = new StringBuilder();
            var parameters = new List<MySqlParameter>();
            int counter = 0;

            // Use the first item as a reference to get columns
            dynamic firstItem = data.First();

            // Check if firstItem is a dictionary and extract keys (columns)
            if (firstItem is IDictionary<string, object> dictionary)
            {
                var columns = dictionary.Keys.ToArray(); // Get column names

                // Start building the insert query
                insertQuery.Append($"INSERT INTO {tableName} (");
                insertQuery.Append(string.Join(", ", columns));
                insertQuery.Append(") VALUES ");

                // Loop through all data and build values
                foreach (var record in data)
                {
                    var values = new List<string>();

                    if (record is IDictionary<string, object> recordDictionary)
                    {
                        foreach (var column in columns)
                        {
                            var paramName = $"@{column}{counter}";
                            values.Add(paramName);

                            // Get the value for each column
                            var propertyValue = recordDictionary.ContainsKey(column)
                                ? recordDictionary[column]
                                : DBNull.Value;

                            // Add parameter for each column
                            parameters.Add(new MySqlParameter(paramName, propertyValue ?? DBNull.Value));
                        }
                    }

                    insertQuery.Append($"({string.Join(", ", values)})");

                    if (counter < data.Count() - 1)
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

                }
            }
            else
            {
                throw new InvalidOperationException("The first item in the data is not of expected type IDictionary<string, object>.");
            }
        }

        public void ValidateStagingTable(DateTime date, int bankId)
        {
            using (var connection = context.CreateConnection())
            {
                // Parameters to pass to the stored procedure
                var parameters = new DynamicParameters();
                parameters.Add("@V_DATE", date, DbType.Date);
                parameters.Add("@V_BANKID", bankId, DbType.Int32);
                parameters.Add("@V_ERRNO", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@V_ERRMSG", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                // Call the stored procedure
                connection.Execute("spValidateStagingTable", parameters, commandType: CommandType.StoredProcedure);

                // Retrieve the output values
                int errorNo = parameters.Get<int>("@V_ERRNO");
                string errorMessage = parameters.Get<string>("@V_ERRMSG");

                // Display the results
                Console.WriteLine($"Error Number: {errorNo}");
                Console.WriteLine($"Error Message: {errorMessage}");

                // Handle errors if any
                if (errorNo != 0)
                {
                    throw new Exception($"Error occurred during validation: {errorMessage}");
                }
            }
        }
    }
}
