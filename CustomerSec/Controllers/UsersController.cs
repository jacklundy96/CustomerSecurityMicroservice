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
using CustomerSec.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CustomerSec.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private UserManager<IdentityUser> _userManager;
        private SignInManager<IdentityUser> _signInManager;
        private IEmailSender _emailSender;

        public UsersController(IUserService userService, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IHttpContextAccessor HTTPContextAccessor, IEmailSender emailSender)
        {
            _userService = userService;
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody]User userParam)
        {
            //If field is empty on register return bad request
            foreach (var param in userParam.GetAll())
                if (param == null) return BadRequest("Incomplete registration information was supplied");

            var user = new IdentityUser { UserName = userParam.Username, Email = userParam.Email, PhoneNumber = userParam.PhoneNumber };
            var token = _userService.GenerateClientJWTToken(user);
            var result = await _userManager.CreateAsync(user, userParam.Password);
            if (result.Succeeded)
            {
                var re = await _signInManager.PasswordSignInAsync(user, userParam.Password, false, false);
                var Token = _userService.GenerateClientJWTToken(user);
                return Ok(Token);
            }
            else
                return BadRequest(result.Errors);
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
                
                if (result.Succeeded)
                    return Ok(_userService.GenerateClientJWTToken(user));
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
        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser()
        {
            try
            {
                var headers = HttpContext.Request.Headers["Authorization"];
                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.ReadJwtToken(headers.ToString().Replace("Bearer", "").Trim());

                IdentityUser user = await _userManager.FindByEmailAsync(token.Payload["email"].ToString());
                user.PasswordHash = "";
                user.SecurityStamp = "";
                user.ConcurrencyStamp = "";

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
        [HttpGet("GetUser")]
        public async Task<IActionResult> LogoutUser()
        {
            try
            {

                return Ok(); 
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = "User not authorized " });
            }
        }


    }
}