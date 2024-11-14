namespace ComplianceMgmt.Api.Models
{
    public class TableSummary
    {
        public string MsgStructure { get; set; }
        public int TotalRecords { get; set; }
        public int SuccessRecords { get; set; }
        public int ConstraintRejection { get; set; }
        public int BusinessRejection { get; set; }
    }

}
