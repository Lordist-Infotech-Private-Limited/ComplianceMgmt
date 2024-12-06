using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;
using MySqlConnector;
using System.Dynamic;
using System.Text;
using MiniExcelLibs;

namespace ComplianceMgmt.Api.Repository
{
    public class BorrowerMortgageOtherRepository(ComplianceMgmtDbContext context) : IBorrowerMortgageOtherRepository
    {
        public async Task<BorrowerMortgageOther> GetByPrimaryKeyAsync(DateTime date)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"SELECT * FROM stgborrowermortgageother 
                      WHERE `Date` = @Date";
                return await connection.QuerySingleOrDefaultAsync<BorrowerMortgageOther>(query, new { Date = date });
            }
        }

        public async Task<IEnumerable<BorrowerMortgageOther>> GetAllByDateAsync(DateTime date)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"SELECT * FROM stgborrowermortgageother WHERE `Date` = @Date";
                return await connection.QueryAsync<BorrowerMortgageOther>(query, new { Date = date });
            }
        }

        public async Task UpdateAsync(BorrowerMortgageOther borrowerMortgageOther)
        {
            using (var connection = context.CreateConnection())
            {
                var query = @"UPDATE stgborrowermortgageother 
                      SET PresentValue = @PresentValue, IsValidated = @IsValidated, RejectedReason = @RejectedReason 
                      WHERE `Date` = @Date AND BankId = @BankId AND Cin = @Cin AND BLoanNo = @BLoanNo AND CollType = @CollType";
                await connection.ExecuteAsync(query, borrowerMortgageOther);
            }
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

            return await BulkUpdateAsync(context.CreateConnection().ConnectionString, "stgborrowermortgageother", borrowers);
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
            var keyColumns = new[] { "Date", "BankId", "Cin", "BLoanNo", "CollType" };

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
    }
}
