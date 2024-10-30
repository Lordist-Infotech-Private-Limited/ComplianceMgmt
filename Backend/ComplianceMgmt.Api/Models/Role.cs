namespace ComplianceMgmt.Api.Models
{
    public class Role
    {
        public int RoleId { get; set; }

        public string RoleName { get; set; }

        public int? UpdatedByUserId { get; set; }
    }
}
