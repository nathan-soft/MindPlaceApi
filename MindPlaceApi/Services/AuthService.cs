using AutoMapper;
using MindPlaceApi.Dtos;
using MindPlaceApi.Dtos.Response;
using MindPlaceApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MindPlaceApi.Codes;

namespace MindPlaceApi.Services {
    public interface IAuthService
    {
        Task<ServiceResponse<TokenDto>> Login(string username, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        public AuthService(SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IUserService userService, IConfiguration configuration, IMapper mapper)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userService = userService;
            _mapper = mapper;
            _config = configuration;
        }

        ///<summary>
        ///<para>Logs a user in and creates a jwt with user details.</para>
        ///</summary>
        /// <param name="username">The user's username</param>
        //// <returns>Task<ServiceResponse<TokenDto>></returns>
        public async Task<ServiceResponse<TokenDto>> Login(string username, string password)
        {
            var sr = new ServiceResponse<TokenDto>();
            var user = await _userManager.FindByNameAsync(username);
            //if (!user.IsActive)
            //{
            //    //enable the user.
            //    user.IsActive = true;
            //    await _userManager.UpdateAsync(user);
            //}

            var validPassword = await _userManager.CheckPasswordAsync(user, password);
            if (user == null || !validPassword)
            {
                //username does not exist.
                return sr.HelperMethod(StatusCodes.Status422UnprocessableEntity, "Incorrect username or password", false);
            }

            if (!user.EmailConfirmed)
            {
                //email address need to be confirmed.
                return sr.HelperMethod(StatusCodes.Status422UnprocessableEntity, "You need to confirm your email address before you can login.", false);
            }


            //create jwt token.
            var tokenString = CreateJwtToken(user, out long expiration);
            sr.Code = 200;
            sr.Data = new TokenDto()
            {
                access_token = tokenString,
                expiration = expiration.ToString()
            };
            sr.Message = "Login Success.";
            sr.Success = true;
            return sr;
            

        }

        ///<summary>
        ///<para>Creates a  jwt token using the username and role parameter.</para>
        ///</summary>
        /// <param name="username">The username to be used for the sub claim.</param>
        /// <param name="expiration">The expiration value representing how long the token stay valid for.</param>
        /// <returns>
        /// A jwt token string
        /// </returns>
        private string CreateJwtToken(AppUser user, out long expiration)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt").GetSection("Key").Value));
            //encode key with algorithym
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            //create claims
            var claims = new List<Claim> {
                new Claim (JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim (JwtRegisteredClaimNames.Jti, Guid.NewGuid ().ToString ()),
                new Claim (JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            //Get user's role(s)
            var roleClaims = user.UserRoles.Select(r => new Claim(ClaimTypes.Role, r.Role.Name)).ToList();
            claims.AddRange(roleClaims);
            //create encoded token from claims , specifying the audience and issuer.
            var token = new JwtSecurityToken(
                _config.GetSection("jwt").GetSection("Issuer").Value,
                _config.GetSection("jwt").GetSection("Audience").Value,
                claims,
                expires: DateTime.UtcNow.AddMinutes(90),
                signingCredentials: creds
            );

            //convert generated token to string.
            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            var dateTimeOffset = new DateTimeOffset(token.ValidTo);
            expiration = dateTimeOffset.ToUnixTimeSeconds();
            return tokenString;
        }

        //private string GenerateRefreshToken()
        //{
        //    var randomNumber = new byte[32];
        //    using (var rng = RandomNumberGenerator.Create())
        //    {
        //        rng.GetBytes(randomNumber);
        //        return Convert.ToBase64String(randomNumber);
        //    }
        //}

        //private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        //{
        //    var tokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
        //        ValidateIssuer = false,
        //        ValidateIssuerSigningKey = true,
        //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the server key used to sign the JWT token is here, use more than 16 chars")),
        //        ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
        //    };

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    SecurityToken securityToken;
        //    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
        //    var jwtSecurityToken = securityToken as JwtSecurityToken;
        //    if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        //        throw new SecurityTokenException("Invalid token");

        //    return principal;
        //}
    }
}