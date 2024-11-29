namespace ComplianceMgmt.Api.Models
{
    public class ComplianceReport
    {
        public int Id { get; set; }
        public string ReportName { get; set; }
        public DateTime GeneratedDate { get; set; }
        public string Status { get; set; }
    }

}
