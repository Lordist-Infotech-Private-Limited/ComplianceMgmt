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
    public class CoBorrowerDetailsRepository(ComplianceMgmtDbContext context) : ICoBorrowerDetailsRepository
    {
        public async Task<CoBorrowerDetails> GetByPrimaryKeyAsync(DateTime date)
        {
            using (var connection = await context.CreateDefaultConnectionAsync())
            {
                var query = @"SELECT * FROM stgcoborrowerdetails 
                      WHERE `Date` = @Date";
                return await connection.QuerySingleOrDefaultAsync<CoBorrowerDetails>(query, new { Date = date});
            }
        }

        public async Task<IEnumerable<CoBorrowerDetails>> GetAllByDateAsync(DateTime date)
        {
            using (var connection = await context.CreateDefaultConnectionAsync())
            {
                var query = @"SELECT * FROM stgcoborrowerdetails WHERE `Date` = @Date";
                return await connection.QueryAsync<CoBorrowerDetails>(query, new { Date = date });
            }
        }

        public async Task UpdateAsync(CoBorrowerDetails coBorrowerDetails)
        {
            using (var connection = await context.CreateDefaultConnectionAsync())
            {
                var query = @"UPDATE stgcoborrowerdetails 
                      SET CbName = @CbName, CbDob = @CbDob, CbCitizenship = @CbCitizenship, CbPanNo = @CbPanNo,
                          CbAadhaar = @CbAadhaar, IdType = @IdType, IdNumber = @IdNumber, CbMonthlyIncome = @CbMonthlyIncome,
                          CbReligion = @CbReligion, CbCast = @CbCast, CbGender = @CbGender, CbOccupation = @CbOccupation,
                          IsValidated = @IsValidated, RejectedReason = @RejectedReason, ValidatedDate = @ValidatedDate
                      WHERE `Date` = @Date AND BankId = @BankId AND Cin = @Cin AND CbCin = @CbCin";
                await connection.ExecuteAsync(query, coBorrowerDetails);
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

            return await BulkUpdateAsync("stgcoborrowerdetails", borrowers);
        }

        public async Task<bool> BulkUpdateAsync(string tableName, IEnumerable<dynamic> data)
        {
            if (data == null || !data.Any()) return false;

            var connection = await context.CreateDefaultConnectionAsync();

            var updateQuery = new StringBuilder();
            var parameters = new List<MySqlParameter>();
            int counter = 0;

            // Define the composite key columns
            var keyColumns = new[] { "Date", "BankId", "Cin", "CbCin" };

            // Use the first item as a reference to get columns
            dynamic firstItem = data.First();

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
                using var command = new MySqlCommand(updateQuery.ToString(), (MySqlConnection)connection)
                {
                    CommandTimeout = 12000
                };
                command.Parameters.AddRange(parameters.ToArray());

                try
                {
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
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
