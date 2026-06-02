namespace Shared.Models
{
    public class TokenClaims
    {
        public string Sub { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string Role { get; set; }
        public string Jti { get; set; }
    }

    public class TokenResponse
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string TokenType { get; set; } = "Bearer";
        public int ExpiresIn { get; set; }
    }
}
