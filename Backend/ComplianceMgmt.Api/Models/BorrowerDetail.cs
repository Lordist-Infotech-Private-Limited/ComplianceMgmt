using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ComplianceMgmt.Api.Models
{
    public class BorrowerDetail
    {
        public long? RowNo { get; set; }
        public DateTime Date { get; set; }
        public int BankId { get; set; }
        public string Cin { get; set; }
        public string BName { get; set; }
        public DateTime BDob { get; set; }
        public string sbCitizenship { get; set; }
        public string? BPanNo { get; set; }
        public string Aadhaar { get; set; }
        public string? IdType { get; set; }
        public string? IdNumber { get; set; }
        public long BMonthlyIncome { get; set; }
        public string? BReligion { get; set; }
        public string? BCast { get; set; }
        public string BGender { get; set; }
        public string BOccupation { get; set; }
        public bool? IsValidated { get; set; }
        public string? RejectedReason { get; set; }
        public DateTime? ValidatedDate { get; set; }
    }
}
