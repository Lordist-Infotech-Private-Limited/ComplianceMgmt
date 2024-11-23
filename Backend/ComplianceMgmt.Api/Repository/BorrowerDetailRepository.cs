using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;
using MiniExcelLibs;
using MySql.Data.MySqlClient;
using System.Data;
using System.Dynamic;
using System.Text;

namespace ComplianceMgmt.Api.Repository
{
    public class BorrowerDetailRepository(ComplianceMgmtDbContext context) : IBorrowerDetailRepository
    {
        public async Task<BorrowerDetail> GetByPrimaryKeyAsync(DateTime date)
        {
            using var connection = context.CreateConnection();

            var query = @"SELECT * FROM stgborrowerdetail 
                      WHERE `Date` = @Date";
            return await connection.QuerySingleOrDefaultAsync<BorrowerDetail>(query, new { Date = date});
        }

        public async Task<IEnumerable<BorrowerDetail>> GetAllByDateAsync(DateTime date)
        {
            using var connection = context.CreateConnection();

            var query = @"SELECT * FROM stgborrowerdetail WHERE `Date` = @Date";
            return await connection.QueryAsync<BorrowerDetail>(query, new { Date = date });
        }

        public async Task UpdateAsync(BorrowerDetail borrowerDetail)
        {
            using var connection = context.CreateConnection();

            var query = @"UPDATE stgborrowerdetail 
                      SET BName = @BName, BDob = @BDob, sbCitizenship = @sbCitizenship, BPanNo = @BPanNo,
                          Aadhaar = @Aadhaar, IdType = @IdType, IdNumber = @IdNumber, BMonthlyIncome = @BMonthlyIncome,
                          BReligion = @BReligion, BCast = @BCast, BGender = @BGender, BOccupation = @BOccupation,
                          IsValidated = @IsValidated, RejectedReason = @RejectedReason, ValidatedDate = @ValidatedDate
                      WHERE `Date` = @Date AND BankId = @BankId AND Cin = @Cin";
            await connection.ExecuteAsync(query, borrowerDetail);
        }

        public async Task<byte[]> ExportBorrowerDetailsToExcelAsync(DateTime date)
        {
            var borrowers = await GetAllByDateAsync(date);

            // Convert to a list of dictionaries for MiniExcel
            var data = borrowers.Select(b => new
            {
                b.Date,
                b.BankId,
                b.Cin,
                b.BName,
                b.BDob,
                b.sbcitizenship,
                b.BPanNo,
                b.Aadhaar,
                b.IdType,
                b.IdNumber,
                b.BMonthlyIncome,
                b.BReligion,
                b.BCast,
                b.BGender,
                b.BOccupation,
                b.IsValidated,
                b.RejectedReason,
                b.ValidatedDate
            });

            using var memoryStream = new MemoryStream();
            // MiniExcel writes data to the memory stream
            MiniExcel.SaveAs(memoryStream, data);

            // Reset the stream position to the beginning
            memoryStream.Position = 0;

            return memoryStream.ToArray();
        }

        public async Task<bool> ImportBorrowerDetailsFromExcelAsync(Stream excelStream)
        {
            // Use MiniExcel to read rows from the uploaded file
            var rows = excelStream.Query(useHeaderRow: true);

            var borrowers = rows.Select(row =>
            {
                dynamic dynamicRow = new ExpandoObject();
                var dictionary = (IDictionary<string, object>)dynamicRow;

                foreach (var column in row)
                {
                    dictionary[column.Key] = column.Value;
                }

                return dynamicRow;
            });

            return await BulkUpdateAsync(context.CreateConnection().ConnectionString, "stgborrowerdetail", borrowers);
        }

        public async Task<bool> BulkInsertAsync(string connectionString, string tableName, IEnumerable<dynamic> data)
        {
            if (data == null || !data.Any()) return false;

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
                    return false;
                }
            }
            else
            {
                throw new InvalidOperationException("The first item in the data is not of expected type IDictionary<string, object>.");
            }
            return true;
        }

        public async Task<bool> BulkUpdateAsync(string connectionString, string tableName, IEnumerable<dynamic> data)
        {
            if (data == null || !data.Any()) return false;

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            var updateQuery = new StringBuilder();
            var parameters = new List<MySqlParameter>();
            int counter = 0;

            // Define the composite key columns
            var keyColumns = new[] { "Date", "BankId", "Cin" };

            // Use the first item as a reference to get columns
            dynamic firstItem = data.First();

            // Check if firstItem is a dictionary and extract keys (columns)
            if (firstItem is IDictionary<string, object> dictionary)
            {
                var columns = dictionary.Keys
                    .Where(k => !keyColumns.Contains(k, StringComparer.OrdinalIgnoreCase)) // Exclude key columns from the update set
                    .ToArray();

                foreach (var record in data)
                {
                    if (record is IDictionary<string, object> recordDictionary)
                    {
                        var setClauses = new List<string>();
                        var whereClauses = new List<string>();

                        // Build SET clauses for non-key columns
                        foreach (var column in columns)
                        {
                            var paramName = $"@{column}{counter}";
                            var propertyValue = recordDictionary.ContainsKey(column) ? recordDictionary[column] : DBNull.Value;

                            setClauses.Add($"{column} = {paramName}");
                            parameters.Add(new MySqlParameter(paramName, propertyValue ?? DBNull.Value));
                        }

                        // Build WHERE clauses for key columns
                        foreach (var keyColumn in keyColumns)
                        {
                            var keyParamName = $"@{keyColumn}{counter}";
                            var keyValue = recordDictionary.ContainsKey(keyColumn) ? recordDictionary[keyColumn] : DBNull.Value;

                            whereClauses.Add($"{keyColumn} = {keyParamName}");
                            parameters.Add(new MySqlParameter(keyParamName, keyValue ?? DBNull.Value));
                        }

                        // Append the UPDATE statement
                        updateQuery.Append($"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE {string.Join(" AND ", whereClauses)}; ");
                        counter++;
                    }
                }

                // Execute the update query
                using var command = new MySqlCommand(updateQuery.ToString(), connection);
                command.Parameters.AddRange(parameters.ToArray());

                try
                {
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            else
            {
                throw new InvalidOperationException("The first item in the data is not of expected type IDictionary<string, object>.");
            }

            return true;
        }

        public async Task ValidateAsync(BorrowerDetailValidationRequest request)
        {
            using (var connection = context.CreateConnection())
            {
                // Parameters to pass to the stored procedure
                var parameters = new DynamicParameters();
                parameters.Add("@V_DATE", request.Date, DbType.Date);
                parameters.Add("@V_BANKID", request.BankId, DbType.Int32);
                parameters.Add("@V_ERRNO", dbType: DbType.Int32, direction: ParameterDirection.Output);
                parameters.Add("@V_ERRMSG", dbType: DbType.String, size: 4000, direction: ParameterDirection.Output);

                try
                {
                    // Call the stored procedure
                    await connection.ExecuteAsync("spValidateStagingTable", parameters, commandType: CommandType.StoredProcedure);

                }
                catch (Exception ex)
                {

                }
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
