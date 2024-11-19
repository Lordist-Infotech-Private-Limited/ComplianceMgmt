namespace ComplianceMgmt.Api.Models
{
    public class CoBorrowerDetails
    {
        public long? RowNo { get; set; }
        public DateTime Date { get; set; }
        public int BankId { get; set; }
        public string Cin { get; set; }
        public string CbCin { get; set; }
        public string CbName { get; set; }
        public DateTime? CbDob { get; set; }
        public string? CbCitizenship { get; set; }
        public string? CbPanNo { get; set; }
        public string CbAadhaar { get; set; }
        public string? IdType { get; set; }
        public string? IdNumber { get; set; }
        public long? CbMonthlyIncome { get; set; }
        public string? CbReligion { get; set; }
        public string? CbCast { get; set; }
        public string? CbGender { get; set; }
        public string? CbOccupation { get; set; }
        public bool? IsValidated { get; set; }
        public string? RejectedReason { get; set; }
        public DateTime? ValidatedDate { get; set; }
    }
}
