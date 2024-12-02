using ComplianceMgmt.Api.Extensions;
using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
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

           await ExecuteValidationSPAsync();
            //var serverDetails = await serverDetailRepository.GetServerDetailsAsync();

            //foreach (var server in serverDetails)
            //{
            //    using var clientConnection = context.CreateClientConnection(
            //        server.ServerIp,
            //        server.DbName,
            //        server.ServerName,
            //        server.ServerPassword);

            //    foreach (var table in tables)
            //    {
            //        string query = $"SELECT * FROM db_a927ee_stgcomp.{table.TableName}";
            //        var clientData = await clientConnection.QueryAsync<dynamic>(query);
            //        //await BulkInsertWithValidationInBackgroundAsync(context.CreateConnection().ConnectionString, table.TableName, table.RejectionTableNames, clientData,1, GetValidationErrors);
            //    }
            //}
        }



        public async Task ExecuteValidationSPAsync()
        {
            using (var connection = new MySqlConnection(context.CreateConnection().ConnectionString))
            {
                try
                {
                    await connection.OpenAsync();
                    var command = new MySqlCommand("CALL ProcessRemoteBorrowerDetails()", connection);
                    await command.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {

                }
            }
        }

        public async Task BulkInsertWithValidationInBackgroundAsync(
            string connectionString,
            string tableName,
            string rejectionTableName,
            IEnumerable<dynamic> data,
            int createdBy, // Pass the user ID creating the records
            Func<dynamic, string> getValidationErrors)
        {
            // Run in background
            _ = Task.Run(async () =>
            {
                var validRecords = new List<dynamic>();
                var rejectedRecords = new List<dynamic>();

                foreach (var record in data)
                {
                    var rejectionReason = getValidationErrors(record);
                    if (string.IsNullOrEmpty(rejectionReason))
                    {
                        validRecords.Add(record);
                    }
                    else
                    {
                        var rejectedRecord = new Dictionary<string, object>(record);
                        rejectedRecord["RejectedReason"] = rejectionReason;
                        rejectedRecords.Add(rejectedRecord);
                    }
                }

                // Process valid and rejected records
                using var connection = new MySqlConnection(connectionString);
                await connection.OpenAsync();

                if (validRecords.Any())
                {
                    await InsertRecordsAsync(connection, tableName, validRecords);
                }

                if (rejectedRecords.Any())
                {
                    await InsertRecordsAsync(connection, rejectionTableName, rejectedRecords);
                }
            });
        }

        private string GetValidationErrors(dynamic record)
        {
            var rejectionReasons = new List<string>();

            // Business validations
            if (string.IsNullOrWhiteSpace((string)record.scin) && string.IsNullOrWhiteSpace((string)record.sbpanno))
            {
                rejectionReasons.Add("CIN and PAN cannot both be blank.");
            }

            if (string.IsNullOrWhiteSpace((string)record.dbdob) || !DateTime.TryParse((string)record.dbdob.ToString(), out _))
            {
                rejectionReasons.Add("Invalid Date or blank Date field.");
            }

            // Initialize 'income' to avoid CS0165
            long income = 0;
            if (record.nbmonthlyincome == null ||
                !long.TryParse((string)record.nbmonthlyincome.ToString(), out income) ||
                income < 0)
            {
                rejectionReasons.Add("Monthly income must be numeric and >= 0.");
            }

            if (string.IsNullOrWhiteSpace((string)record.sbname))
            {
                rejectionReasons.Add("Primary Borrower Name cannot be blank.");
            }

            if (string.IsNullOrWhiteSpace((string)record.saadhaar))
            {
                rejectionReasons.Add("Aadhaar must not be blank.");
            }

            if (string.IsNullOrWhiteSpace((string)record.sbgender))
            {
                rejectionReasons.Add("Gender must not be blank.");
            }

            // Constraint validations
            var validCitizenship = new[] { "Indian", "Non-Resident", "Foreign National" };
            if (!string.IsNullOrWhiteSpace((string)record.sbcitizenship) &&
                !validCitizenship.Contains((string)record.sbcitizenship))
            {
                rejectionReasons.Add("Invalid Citizenship value.");
            }

            var validGenders = new[] { "Male", "Female", "Other" };
            if (!string.IsNullOrWhiteSpace((string)record.sbgender) &&
                !validGenders.Contains((string)record.sbgender))
            {
                rejectionReasons.Add("Invalid Gender value.");
            }

            var validOccupations = new[] { "Salaried", "Self-Employed", "Student", "Retired" };
            if (!string.IsNullOrWhiteSpace((string)record.sboccupation) &&
                !validOccupations.Contains((string)record.sboccupation))
            {
                rejectionReasons.Add("Invalid Occupation value.");
            }

            return string.Join("; ", rejectionReasons);
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
    }
}
