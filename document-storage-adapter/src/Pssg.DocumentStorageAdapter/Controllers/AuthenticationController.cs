﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Pssg.DocumentStorageAdapter.Controllers
{
    [Route("[controller]")]
    public class AuthenticationController : Controller
    {
        private readonly IConfiguration Configuration;

        public AuthenticationController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Returns a security token.
        /// </summary>
        /// <param name="secret"></param>
        /// <returns>A new JWT</returns>
        [HttpGet("token")]
        [AllowAnonymous]
        public string GetToken([FromQuery] string secret)
        {
            string result = "Invalid secret.";
            string configuredSecret = Configuration["JWT_TOKEN_KEY"];
            if (configuredSecret.Equals(secret))
            {
                byte[] secretBytes = Encoding.UTF8.GetBytes(Configuration["JWT_TOKEN_KEY"]);
                Array.Resize(ref secretBytes, 32);

                var key = new SymmetricSecurityKey(secretBytes);
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var jwtSecurityToken = new JwtSecurityToken(
                    Configuration["JWT_VALID_ISSUER"],
                    Configuration["JWT_VALID_ISSUER"],
                    expires: DateTime.UtcNow.AddYears(5),
                    signingCredentials: creds
                    );
                result = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken); 
            }
            else
            {
                Log.Error($"Invalid secret supplied - {secret}");
            }

            return result;
        }
    }
}
