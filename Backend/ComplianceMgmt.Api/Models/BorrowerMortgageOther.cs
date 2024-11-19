namespace ComplianceMgmt.Api.Models
{
    public class BorrowerMortgageOther
    {
        public long? RowNo { get; set; }
        public DateTime Date { get; set; }
        public int BankId { get; set; }
        public string Cin { get; set; }
        public string BLoanNo { get; set; }
        public string CollType { get; set; }
        public long ValueAtSanct { get; set; }
        public long? PresentValue { get; set; }
        public bool? IsValidated { get; set; }
        public string? RejectedReason { get; set; }
        public DateTime? ValidatedDate { get; set; }
    }

}
