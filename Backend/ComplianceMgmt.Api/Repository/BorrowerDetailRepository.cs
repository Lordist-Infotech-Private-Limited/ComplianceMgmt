using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;
using MiniExcelLibs;
using MySql.Data.MySqlClient;
using System.Globalization;
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

            var borrowers = rows.Select(row => new BorrowerDetail
            {
                Date = DateTime.Parse(row.Date.ToString(), CultureInfo.InvariantCulture),
                BankId = Convert.ToInt32(row.BankId),
                Cin = row.Cin.ToString(),
                BName = row.BName.ToString(),
                BDob = DateTime.Parse(row.BDob.ToString(), CultureInfo.InvariantCulture),
                sbcitizenship = row.sbcitizenship.ToString(),
                BPanNo = row.BPanNo?.ToString(),
                Aadhaar = row.Aadhaar.ToString(),
                IdType = row.IdType?.ToString(),
                IdNumber = row.IdNumber?.ToString(),
                BMonthlyIncome = Convert.ToInt64(row.BMonthlyIncome),
                BReligion = row.BReligion?.ToString(),
                BCast = row.BCast?.ToString(),
                BGender = row.BGender.ToString(),
                BOccupation = row.BOccupation.ToString(),
                IsValidated = string.IsNullOrWhiteSpace(row.IsValidated?.ToString()) ? null : (bool?)bool.Parse(row.IsValidated.ToString()),
                RejectedReason = row.RejectedReason?.ToString(),
                ValidatedDate = string.IsNullOrWhiteSpace(row.ValidatedDate?.ToString()) ? null : (DateTime?)DateTime.Parse(row.ValidatedDate.ToString(), CultureInfo.InvariantCulture)
            }).ToList();

            return await BulkInsertAsync(context.CreateConnection().ConnectionString, "stgborrowerdetail", borrowers);
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
    }
}
