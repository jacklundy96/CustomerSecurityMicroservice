using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CustomerSec.Data;
using CustomerSec.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using CustomerSec.Helpers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace CustomerSec.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signInManager;
        private IEmailSender _emailSender;

        public UsersController( UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IHttpContextAccessor HTTPContextAccessor, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody]User userParam)
        {
            try
            {
                //If field is empty on register return bad request
                foreach (var param in userParam.GetAll())
                    if (param == null)
                        return BadRequest("Incomplete registration information was supplied");


               
                var user = new IdentityUser
                    {UserName = userParam.Email, Email = userParam.Email, PhoneNumber = userParam.PhoneNumber};
              
                Claim claims = (new Claim(ClaimTypes.Name, user.Email));
                var result = await _userManager.CreateAsync(user, userParam.Password);
                if (result.Succeeded)
                {
                    var re = await _signInManager.PasswordSignInAsync(userParam.Email, userParam.Password, false, false);

                    var token = new JwtSecurityToken("", "", User.Claims, DateTime.Now,
                        DateTime.Now + TimeSpan.FromMinutes(10),
                        new SigningCredentials(
                            new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes("00afef5b-502f-45b1-80ce-8d88568ccb80")),
                            SecurityAlgorithms.HmacSha256));
                    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                    return Ok(jwt);
                }
                else
                    return BadRequest();
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        /// <summary>
        /// Log in a user by their supplied email and password and return a valid JWT token
        /// </summary>
        /// <param name="userParam"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("Authenticate")]
        public async Task<IActionResult> LogInAsync([FromBody]User userParam)
        {
            try
            {
                IdentityUser user = await _userManager.FindByEmailAsync(userParam.Email);
                var result = await _signInManager.PasswordSignInAsync(user, userParam.Password, false, false);

                //return new token
                if (result.Succeeded)
                {
                    var token = new JwtSecurityToken("", "", User.Claims, DateTime.Now,
                        DateTime.Now + TimeSpan.FromMinutes(10),
                        new SigningCredentials(
                            new SymmetricSecurityKey(
                                System.Text.Encoding.UTF8.GetBytes("00afef5b-502f-45b1-80ce-8d88568ccb80")),
                            SecurityAlgorithms.HmacSha256));
                    var jwt = new JwtSecurityTokenHandler().WriteToken(token);
                    return Ok(jwt);
                }
                else
                    return BadRequest(new { message = "Username or password is incorrect" } );
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "Missing username and password" } );
            }
        }

        /// <summary>
        /// Get all the User information for the user associated with the bearer token
        /// </summary>
        /// <returns>Reponse</returns>
        [Authorize(Policy = "ReisteredUsersOnly")]
        [HttpGet("Get/{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            try
            {
                var headers = HttpContext.Request.Headers["Authorization"];
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(headers.ToString().Replace("Bearer", "").Trim());

                IdentityUser user = await _userManager.FindByEmailAsync(token.Payload["email"].ToString());

                return Ok(JsonConvert.SerializeObject(user));
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "User not authorized " });
            }
        }

        /// <summary>
        /// Get all the User information for the user associated with the bearer token
        /// </summary>
        /// <returns>Reponse</returns>
        [Authorize(Policy = "ReisteredUsersOnly")]
        [HttpGet("GetUser")]
        public async Task<IActionResult> LogoutUser()
        {
            throw new NotImplementedException();
        }
    }
}