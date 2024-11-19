namespace ComplianceMgmt.Api.Models
{
    public class BorrowerMortgage
    {
        public long? RowNo { get; set; }
        public DateTime Date { get; set; }
        public int BankId { get; set; }
        public string Cin { get; set; }
        public string BLoanNo { get; set; }
        public string PropType { get; set; }
        public string PropAdd { get; set; }
        public decimal? LandArea { get; set; }
        public decimal? BuildingArea { get; set; }
        public string TownName { get; set; }
        public string District { get; set; }
        public string State { get; set; }
        public int? Pin { get; set; }
        public string RuralUrban { get; set; }
        public long PropValAtSanct { get; set; }
        public long? PresentValue { get; set; }
        public string Insurance { get; set; }
        public bool? IsValidated { get; set; }
        public string? RejectedReason { get; set; }
        public DateTime? ValidatedDate { get; set; }
    }

}
