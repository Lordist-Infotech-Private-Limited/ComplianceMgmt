namespace ComplianceMgmt.Api.Models
{
    public class RegisterUser
    {
        public string Name { get; set; } = "";
        public string LoginId { get; set; }
        public string UserName { get; set; } = "";
        public string Password { get; set; }
        public string MailId { get; set; }
        public string PhoneNumber { get; set; }
        public int RoleID { get; set; }
        public DateTime LastLoginDate { get; set; }
        public bool IsActive { get; set; }
    }
}
