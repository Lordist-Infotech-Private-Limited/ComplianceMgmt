using ComplianceMgmt.Api.Infrastructure;
using ComplianceMgmt.Api.IRepository;
using ComplianceMgmt.Api.Models;
using Dapper;

namespace ComplianceMgmt.Api.Repository
{
    public class StatewiseLoanRepository(ComplianceMgmtDbContext context) : IStatewiseLoanRepository
    {
        public async Task<IEnumerable<StatewiseLoanSummary>> GetStatewiseLoanDataAsync()
        {
            string query = @"
        SELECT 
            bm.State,
            bm.District,
            bl.SanctAmount,
            bl.TotalLoanOut,
            bl.CummuLoanDisb,
            CASE 
                WHEN bl.AssetCat IN ('ACC-04', 'ACC-05', 'ACC-06', 'ACC-07', 'ACC-08') THEN bl.AssetCat
                ELSE 'Other'
            END AS NPAClassification,
            SUM(bl.TotalLoanOut) OVER (PARTITION BY bm.State, bm.District, bl.AssetCat) AS TotalLoanOutByNPA
        FROM 
            stgborrowerloan bl
        INNER JOIN 
            stgborrowermortgage bm
        ON 
            bl.Date = bm.Date AND 
            bl.BankId = bm.BankId AND 
            bl.Cin = bm.Cin AND 
            bl.BLoanNo = bm.BLoanNo
        WHERE 
            bl.AssetCat IN ('ACC-04', 'ACC-05', 'ACC-06', 'ACC-07', 'ACC-08');
    ";

            var connection = context.CreateConnection();
            return await connection.QueryAsync<StatewiseLoanSummary>(query);
        }
    }
}
