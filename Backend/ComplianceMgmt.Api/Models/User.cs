namespace ComplianceMgmt.Api.Models
{
    public class User
    {
        public int UserID { get; set; }

        public string LoginId { get; set; }

        public string UserName { get; set; }

        public string MailId { get; set; }

        public string MobileNo { get; set; }

        public string Designation { get; set; }

        public int CreatedBy { get; set; }
        public DateTime CreateDate { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLogin { get; set; }
    }
}
