namespace Identity.API.Data.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer"; // Default role

        // Refresh Token attributes to securely manage persistent login sessions
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
        
        // Security stamp to invalidate all tokens globally
        public string SecurityStamp { get; set; } = Guid.NewGuid().ToString();
    }
}
