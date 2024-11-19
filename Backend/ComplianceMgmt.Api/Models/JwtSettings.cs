namespace ComplianceMgmt.Api.Models
{
    public class JwtSettings
    {
        public string Secret { get; set; } // The secret key used for signing the token
        public string Issuer { get; set; } // Token issuer
        public string Audience { get; set; } // Token audience
        public int ExpiryMinutes { get; set; } // Token expiration time in minutes
    }

}
