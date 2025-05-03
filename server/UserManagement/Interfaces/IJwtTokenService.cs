using System.Security.Claims;
using UserManagement.Models.Entities;

namespace UserManagement.Interfaces;

public interface IJwtTokenService
{
    string GenerateJwtToken(ApplicationUser user);
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}