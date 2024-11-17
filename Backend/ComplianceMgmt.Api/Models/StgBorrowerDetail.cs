using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ComplianceMgmt.Api.Models
{
    [Table("stgborrowerdetail")]
    public class StgBorrowerDetail
    {
        public int RowNo { get; set; }

        public DateTime Date { get; set; }

        public int BankId { get; set; }

        [StringLength(50)]
        public string Cin { get; set; }

        [Required]
        [StringLength(50)]
        public string BName { get; set; }

        [Required]
        public DateTime BDob { get; set; }

        [Required]
        [StringLength(50)]
        public string SBCitizenship { get; set; }

        [StringLength(50)]
        public string BPanNo { get; set; }

        [Required]
        [StringLength(50)]
        public string Aadhaar { get; set; }

        [StringLength(50)]
        public string IdType { get; set; }

        [StringLength(50)]
        public string IdNumber { get; set; }

        [Required]
        public long BMonthlyIncome { get; set; }

        [StringLength(50)]
        public string BReligion { get; set; }

        [StringLength(50)]
        public string BCast { get; set; }

        [Required]
        [StringLength(50)]
        public string BGender { get; set; }

        [Required]
        [StringLength(50)]
        public string BOccupation { get; set; }

        public bool? IsValidated { get; set; }

        [StringLength(8000)]
        public string RejectedReason { get; set; }

        public DateTime? ValidatedDate { get; set; }
    }
}
