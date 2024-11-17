using ComplianceMgmt.Api.Models;
using MySql.Data.MySqlClient;
using System.Data;

namespace ComplianceMgmt.Api.Utility
{
    public class BulkInsertHelper
    {
        public static DataTable ToDataTable(List<StgBorrowerDetail> borrowerDetails)
        {
            // Create a DataTable with the same schema as the database table
            var table = new DataTable();

            // Define columns
            table.Columns.Add("RowNo", typeof(long));
            table.Columns.Add("Date", typeof(DateTime));
            table.Columns.Add("BankId", typeof(int));
            table.Columns.Add("Cin", typeof(string));
            table.Columns.Add("BName", typeof(string));
            table.Columns.Add("BDob", typeof(DateTime));
            table.Columns.Add("SBCitizenship", typeof(string));
            table.Columns.Add("BPanNo", typeof(string));
            table.Columns.Add("Aadhaar", typeof(string));
            table.Columns.Add("IdType", typeof(string));
            table.Columns.Add("IdNumber", typeof(string));
            table.Columns.Add("BMonthlyIncome", typeof(long));
            table.Columns.Add("BReligion", typeof(string));
            table.Columns.Add("BCast", typeof(string));
            table.Columns.Add("BGender", typeof(string));
            table.Columns.Add("BOccupation", typeof(string));
            table.Columns.Add("IsValidated", typeof(bool));
            table.Columns.Add("RejectedReason", typeof(string));
            table.Columns.Add("ValidatedDate", typeof(DateTime));

            // Populate the table with data from the list
            foreach (var borrower in borrowerDetails)
            {
                table.Rows.Add(
                    borrower.RowNo,
                    borrower.Date,
                    borrower.BankId,
                    borrower.Cin,
                    borrower.BName,
                    borrower.BDob,
                    borrower.SBCitizenship,
                    borrower.BPanNo,
                    borrower.Aadhaar,
                    borrower.IdType,
                    borrower.IdNumber,
                    borrower.BMonthlyIncome,
                    borrower.BReligion,
                    borrower.BCast,
                    borrower.BGender,
                    borrower.BOccupation,
                    borrower.IsValidated,
                    borrower.RejectedReason,
                    borrower.ValidatedDate
                );
            }

            return table;
        }

        //public static async Task BulkInsertAsync(string connectionString, DataTable table)
        //{
        //    using (var connection = new SqlConnection(connectionString))
        //    {
        //        await connection.OpenAsync();

        //        using (var bulkCopy = new SqlBulkCopy(connection))
        //        {
        //            bulkCopy.DestinationTableName = "stgborrowerdetail";

        //            // Map columns in the DataTable to the database table
        //            bulkCopy.ColumnMappings.Add("RowNo", "RowNo");
        //            bulkCopy.ColumnMappings.Add("Date", "Date");
        //            bulkCopy.ColumnMappings.Add("BankId", "BankId");
        //            bulkCopy.ColumnMappings.Add("Cin", "Cin");
        //            bulkCopy.ColumnMappings.Add("BName", "BName");
        //            bulkCopy.ColumnMappings.Add("BDob", "BDob");
        //            bulkCopy.ColumnMappings.Add("SBCitizenship", "sbcitizenship");
        //            bulkCopy.ColumnMappings.Add("BPanNo", "BPanNo");
        //            bulkCopy.ColumnMappings.Add("Aadhaar", "Aadhaar");
        //            bulkCopy.ColumnMappings.Add("IdType", "IdType");
        //            bulkCopy.ColumnMappings.Add("IdNumber", "IdNumber");
        //            bulkCopy.ColumnMappings.Add("BMonthlyIncome", "BMonthlyIncome");
        //            bulkCopy.ColumnMappings.Add("BReligion", "BReligion");
        //            bulkCopy.ColumnMappings.Add("BCast", "BCast");
        //            bulkCopy.ColumnMappings.Add("BGender", "BGender");
        //            bulkCopy.ColumnMappings.Add("BOccupation", "BOccupation");
        //            bulkCopy.ColumnMappings.Add("IsValidated", "IsValidated");
        //            bulkCopy.ColumnMappings.Add("RejectedReason", "RejectedReason");
        //            bulkCopy.ColumnMappings.Add("ValidatedDate", "ValidatedDate");

        //            // Perform the bulk insert
        //            await bulkCopy.WriteToServerAsync(table);
        //        }
        //    }
        //}
    }
}
