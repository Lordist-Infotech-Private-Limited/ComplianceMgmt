namespace ComplianceMgmt.Api.Models
{
    public class User
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string PasswordHash { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public int RoleId { get; set; }

        public DateTime LastLoginDate { get; set; }

        public bool IsActive { get; set; }

        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }

        public int? UpdatedByUserId { get; set; }

        public string ProfilePicture { get; set; }

        public int? ClientId { get; set; }

        public virtual Role Role { get; set; }

    }
}
