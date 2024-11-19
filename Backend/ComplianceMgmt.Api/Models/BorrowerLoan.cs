namespace ComplianceMgmt.Api.Models
{
    public class BorrowerLoan
    {
        public long? RowNo { get; set; }
        public DateTime Date { get; set; }
        public int BankId { get; set; }
        public string Cin { get; set; }
        public string BLoanNo { get; set; }
        public string LoanType { get; set; }
        public string LoanPurpose { get; set; }
        public long SanctAmount { get; set; }
        public DateTime SanctDate { get; set; }
        public string? DwellingUnit { get; set; }
        public int MoratoriumPeriod { get; set; }
        public int LoanTenCont { get; set; }
        public int LoanTenResidual { get; set; }
        public decimal Roi { get; set; }
        public string IntType { get; set; }
        public long? Emi { get; set; }
        public long? PreEmi { get; set; }
        public DateTime? FirstDisbDate { get; set; }
        public DateTime? EmiStartDate { get; set; }
        public DateTime? PreEmiStartDate { get; set; }
        public long LoanDisbDuringMonth { get; set; }
        public long CummuLoanDisb { get; set; }
        public string LoanStatus { get; set; }
        public long? AmtUnderCons { get; set; }
        public string PartyName { get; set; }
        public string? MortGuarantee { get; set; }
        public long? AmoutUnderGuar { get; set; }
        public long TotalLoanOut { get; set; }
        public long POut { get; set; }
        public long IOut { get; set; }
        public long OtherDueOut { get; set; }
        public long? LoanDsbp { get; set; }
        public long LoanRepayDurMth { get; set; }
        public long TotalLoanOverDue { get; set; }
        public long POverDue { get; set; }
        public long IOverDue { get; set; }
        public long OtherOverDue { get; set; }
        public string AccntClosedDurMth { get; set; }
        public string AssetCat { get; set; }
        public string? Ecl { get; set; }
        public DateTime ClassDate { get; set; }
        public decimal? Pd { get; set; }
        public decimal? Lgd { get; set; }
        public long ProvAmt { get; set; }
        public string Rw { get; set; }
        public string RefFromNhb { get; set; }
        public string UnderPmayClss { get; set; }
        public string? SerfaseiAct { get; set; }
        public long? AmtClaimUnderNotice { get; set; }
        public long? AmtRecovered { get; set; }
        public string? StayGranted { get; set; }
        public bool? IsValidated { get; set; }
        public string? RejectedReason { get; set; }
        public DateTime? ValidatedDate { get; set; }
    }
}
