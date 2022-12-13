using CourseWebAPI.Data.Context;
using CourseWebAPI.Data.Entities;
using CourseWebAPI.Dto.Auth;
using CourseWebAPI.Services.Interfaces;
using CourseWebAPI.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CourseWebAPI.Services.Implementations
{
    public class IdentityService : IIdentityService
    {
        private readonly TourDBContext context;
        private readonly UserManager<User> userManager;
        private readonly JwtSettings jwtSettings;
        private readonly TokenValidationParameters tokenValidationParameters;

        public IdentityService(TourDBContext context,
                                UserManager<User> userManager,
                                JwtSettings jwtSettings,
                                TokenValidationParameters tokenValidationParameters)
        {
            this.userManager = userManager;
            this.jwtSettings = jwtSettings;
            this.tokenValidationParameters = tokenValidationParameters;
            this.context = context;
        }

        public async Task<(bool isSuccessful, AuthenticationResultDto authResult)> RegisterAsync(RegistrationDto registrationDto)
        {
            var exsistingUser = await userManager.FindByEmailAsync(registrationDto.Email);

            if (exsistingUser is not null)
            {
                return (false, null);
            }

            var newUser = new User
            {
                Email = registrationDto.Email,
                Name = registrationDto.Name,
                UserName = registrationDto.Name
            };

            var createdUser = await userManager.CreateAsync(newUser, registrationDto.Password);

            if (!createdUser.Succeeded)
            {
                return (false, null);
            }

            var authResult = await GenerateToken(newUser);
            return (true, authResult);
        }

        public async Task<(bool isSuccessful, AuthenticationResultDto authResult)> LoginAsync(LoginDto loginDto)
        {
            var user = await userManager.FindByNameAsync(loginDto.UserName);

            if (user is null)
            {
                return (false, null);
            }

            var userHasValidPassword = await userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!userHasValidPassword)
            {
                return (false, null);
            }

            var authResult = await GenerateToken(user);
            return (true, authResult);
        }

        public async Task<(bool IsSuccessful, AuthenticationResultDto authResult)> RefreshAsync(RefreshTokenDto refreshTokenDto)
        {
            var validatedToken = GetPrincipalFromToken(refreshTokenDto.Token);
            if (validatedToken is null)
            {
                return (false, null);
            }
            var expiryDateUnix =
                long.Parse(validatedToken.Claims
                .Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var expiryDateTime =
                new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix)
                .Subtract(jwtSettings.LifeTime);
            if (expiryDateTime > DateTime.UtcNow)
            {
                return (false, null);
            }
            var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = await context.RefreshTokens.FindAsync(refreshTokenDto.RefreshToken);

            if (storedRefreshToken is null
                && DateTime.UtcNow > storedRefreshToken.ExpiryDate
                && storedRefreshToken.Invalidated
                && storedRefreshToken.IsUsed
                && storedRefreshToken.JwtId != jti)
            {
                return (false, null);
            }

            storedRefreshToken.IsUsed = true;
            context.RefreshTokens.Update(storedRefreshToken);
            await context.SaveChangesAsync();

            var user = await userManager.FindByIdAsync(validatedToken.Claims.Single(c => c.Type == "Id").Value);

            var result = await GenerateToken(user);
            return (true, result);
        }

        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                if (!HasValidSecurityAlgorithm(validatedToken))
                {
                    return null;
                }
                return principal;
            }
            catch
            {
                return null;
            }
        }

        private bool HasValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return (validatedToken is JwtSecurityToken jwtSecurityToken)
                && jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase);
        }

        private async Task<AuthenticationResultDto> GenerateToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email),
                    new Claim("Id", user.Id)
                }),
                Expires = DateTime.UtcNow.Add(jwtSettings.LifeTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(6)
            };

            await context.AddAsync(refreshToken);
            await context.SaveChangesAsync();

            return new AuthenticationResultDto
            {
                JwtId = token.Id,
                JwtToken = tokenHandler.WriteToken(token),
                JwtExpireTime = tokenDescriptor.Expires,
                RefreshToken = refreshToken.Token,
                User = user,
            };
        }
    }
}
