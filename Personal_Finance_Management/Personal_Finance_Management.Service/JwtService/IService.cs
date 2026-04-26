using System.Security.Claims;

namespace Personal_Finance_Management.Service.JwtService;

public interface IService
{
    public string GenerateAccessToken(IEnumerable<Claim> claims);

    ClaimsPrincipal ValidateToken(string token);

}