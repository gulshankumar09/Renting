namespace UserManagement.DTOs
{
    public class LoginResponse
    {
        public required bool Success { get; set; }
        public required string Message { get; set; }
        public required string Token { get; set; }
        public required string RefreshToken { get; set; }
        public required UserInfo User { get; set; }
    }

    public class UserInfo
    {
        public required string Email { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
    }
}