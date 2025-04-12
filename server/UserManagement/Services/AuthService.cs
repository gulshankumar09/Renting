using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserManagement.Models;
using UserManagement.Data;

namespace UserManagement.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly UserManagementDbContext _dbContext;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration,
            UserManagementDbContext dbContext)
        {
            _userManager = userManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        public async Task<(bool Success, string Token, string RefreshToken)> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, password))
            {
                return (false, string.Empty, string.Empty);
            }

            var token = await GenerateJwtToken(user);
            var refreshToken = await GenerateRefreshToken(user);

            user.LastLoginAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return (true, token, refreshToken);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(string email, string password, string firstName, string lastName)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return (false, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // Create the UserProfile
            var profile = new UserProfile
            {
                ApplicationUserId = user.Id,
                FirstName = firstName,
                LastName = lastName,
                Status = UserStatus.Active,
                CreatedBy = user.Id,
                UpdatedBy = user.Id
            };

            _dbContext.UserProfiles.Add(profile);
            await _dbContext.SaveChangesAsync();

            return (true, "User registered successfully");
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            // Include profile data in claims if available
            var profile = await _dbContext.UserProfiles.FindAsync(user.Id);
            if (profile != null)
            {
                claims.Add(new Claim("FirstName", profile.FirstName));
                claims.Add(new Claim("LastName", profile.LastName));
            }

            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpiryInMinutes"] ?? "30"));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private async Task<string> GenerateRefreshToken(ApplicationUser user)
        {
            var refreshToken = Guid.NewGuid().ToString();
            await _userManager.SetAuthenticationTokenAsync(user, "Renting", "RefreshToken", refreshToken);
            return refreshToken;
        }

        public async Task<(bool Success, string Token, string RefreshToken)> RefreshTokenAsync(string token, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
            {
                return (false, string.Empty, string.Empty);
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = userId != null ? await _userManager.FindByIdAsync(userId) : null;

            if (user == null)
            {
                return (false, string.Empty, string.Empty);
            }

            var storedRefreshToken = await _userManager.GetAuthenticationTokenAsync(user, "Renting", "RefreshToken");
            if (storedRefreshToken != refreshToken)
            {
                return (false, string.Empty, string.Empty);
            }

            var newToken = await GenerateJwtToken(user);
            var newRefreshToken = await GenerateRefreshToken(user);

            return (true, newToken, newRefreshToken);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? string.Empty)),
                ValidateLifetime = false,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidIssuer = _configuration["Jwt:Issuer"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            if (!(securityToken is JwtSecurityToken jwtSecurityToken) || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
    }
}