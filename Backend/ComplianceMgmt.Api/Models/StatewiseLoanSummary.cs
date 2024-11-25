namespace ComplianceMgmt.Api.Models
{
    public class StatewiseLoanSummary
    {
        public string State { get; set; }
        public string District { get; set; }
        public decimal SanctAmount { get; set; }
        public decimal TotalLoanOut { get; set; }
        public decimal CummuLoanDisb { get; set; }
        public string NPAClassification { get; set; }
        public decimal TotalLoanOutByNPA { get; set; }
    }

}
