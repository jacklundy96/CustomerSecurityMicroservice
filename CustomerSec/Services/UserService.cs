using CustomerSec.Data;
using CustomerSec.Entities;
using CustomerSec.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CustomerSec.Services
{
    public interface IUserService
    {
        string GenerateClientJWTToken(IdentityUser user);

        JwtSecurityToken GetJWTTokenFromHeaders(HttpContext httpContext);
    }

    public class UserService : IUserService
    {
        private readonly AppSettings _appSettings;

        public UserService(IOptions<AppSettings> appSettings)
        {
            _appSettings = appSettings.Value;
        }

        /// <summary>
        /// Create a new JWT token for the sign-in/log-in event 
        /// </summary>
        /// <param name="user">The currently logged in identity user</param>
        /// <returns>JwtSecurityToken wrote to an output string </returns>
        public string GenerateClientJWTToken(IdentityUser user)
        {
            // authentication successful so generate jwt token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(_appSettings.JwtKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, "Client"),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                
                Expires = DateTime.UtcNow.AddDays(Convert.ToDouble(_appSettings.JwtExpireDays)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
            
        }

        /// <summary>
        /// Pull the JWT token from the request headers 
        /// </summary>
        /// <param name="httpContext">The current HTTP context</param>
        /// <returns>JwtSecurityToken - from the current request</returns>
        public JwtSecurityToken GetJWTTokenFromHeaders(HttpContext httpContext)
        {
            var headers = httpContext.Request.Headers["Authorization"];
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ReadJwtToken(headers.ToString().Replace("Bearer", "").Trim());
        }
    }
}
