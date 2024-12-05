using ApiForAngular.ApplicationDbContext;
using ApiForAngular.DTO;
using ApiForAngular.ServiceContracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApiForAngular.Services
{
    public class JwtServices:IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtServices(IConfiguration configuration, UserManager<ApplicationUser> userManage)
        {
            _configuration = configuration;
            this._userManager = userManage;
        }
        public async Task<AuthenticationResponse> CreateJwtToken(ApplicationUser user)
        {
            DateTime expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));

            // Retrieve user roles
            var roles = await _userManager.GetRolesAsync(user);

            // Create claims including roles
            var claims = new List<Claim>
              {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject (user id)
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // JWT unique ID
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()), // Issued at
        new Claim(ClaimTypes.NameIdentifier, user.Email?? throw new ArgumentNullException("Email cannot be null for identifier")), // Unique name identifier of the user (Email)
        new Claim(ClaimTypes.Email, user.Email ?? throw new ArgumentNullException("Email cannot be null")), // Email of the user
        new Claim(ClaimTypes.Name, user.Id.ToString())

     };

            // Add each role as a claim
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role)); // Ensure role is a string
            }

            // Create token
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenGenerator = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: expiration,
                signingCredentials: signingCredentials
            );

            var tokenHandler = new JwtSecurityTokenHandler();
            string token = tokenHandler.WriteToken(tokenGenerator);

            return new AuthenticationResponse
            {
                Id=user.Id,
                Token = token,
                Email = user.Email,
                PersonName = user.FirstName,
                Expire = expiration,
            };
        }

        //public AuthenticationResponse CreateJwtToken(ApplicationUser user)
        //{


        //    DateTime expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));

        //    Claim[] claims = new Claim[] {
        //      new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), //Subject (user id)

        //      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //JWT unique ID

        //      new Claim(JwtRegisteredClaimNames.Iat,DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()), //Issued at (date and time of token generation)

        //      new Claim(ClaimTypes.NameIdentifier, user.Email), //Unique name identifier of the user (Email)

        //      new Claim(ClaimTypes.Name, user.PersonName), //Name of the user
        //      new Claim(ClaimTypes.Email, user.Email) //email of the user

        //      };


        //    SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
        //     Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
        //     );

        //    SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        //    JwtSecurityToken tokenGenerator = new JwtSecurityToken(
        //     _configuration["Jwt:Issuer"],
        //     _configuration["Jwt:Audience"],
        //     claims,
        //     expires: expiration,
        //     signingCredentials: signingCredentials
        //     );

        //    JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        //    string token = tokenHandler.WriteToken(tokenGenerator);


        //    return new AuthenticationResponse()
        //    {
        //        Token = token,
        //        Email = user.Email,
        //        PersonName = user.PersonName,
        //        Expire = expiration,
        //    };

        //}
        //Creates a refresh token (base 64 string of random numbers)
        private string GenerateRefreshToken()
        {
            byte[] bytes = new byte[64];
            var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal? GetPrincipalFromJwtToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters()
            {
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),

                ValidateLifetime = false //should be false
            };

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            ClaimsPrincipal principal = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))  //if the given token is not jwt token or given token is not maching with hmacsha256 alogirtm token then it not valida one 
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    
    }
}
