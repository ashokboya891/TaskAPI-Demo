using ApiForAngular.ApplicationDbContext;
using ApiForAngular.DTO;
using System.Security.Claims;

namespace ApiForAngular.ServiceContracts
{
    public interface IJwtService
    {
        Task<AuthenticationResponse> CreateJwtToken(ApplicationUser user);
        ClaimsPrincipal? GetPrincipalFromJwtToken(string? token);
    }
}
