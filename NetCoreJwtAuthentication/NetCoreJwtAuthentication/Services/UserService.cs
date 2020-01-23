using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetCoreJwtAuthentication.Entities;
using NetCoreJwtAuthentication.Helpers;
using Microsoft.Extensions.Configuration;

namespace NetCoreJwtAuthentication.Services
{
    public interface IUserService
    {
        User Authenticate(string username, string password);
        IEnumerable<User> GetAll();
    }

    public class UserService : IUserService
    {
        // users hardcoded for simplicity, store in a db with hashed passwords in production applications
        private List<User> _users = new List<User>
        {
            new User { Id = 1, FirstName = "Test", LastName = "User", Username = "test", Password = "test" }
        };

        private readonly IOptions<AppSettings> _configuration;

        public UserService(IOptions<AppSettings> configuration)
        {
            _configuration = configuration;
        }

        public User Authenticate(string username, string password)
        {
            var user = _users.SingleOrDefault(x => x.Username == username && x.Password == password);

            // return null if user not found
            if (user == null)
                return null;

            // authentication successful so generate jwt token
            //var tokenHandler = new JwtSecurityTokenHandler();
            //var key = Encoding.ASCII.GetBytes(_configuration.Value.Secret);

            //var tokenDescriptor = new SecurityTokenDescriptor
            //{
            //    Subject = new ClaimsIdentity(new Claim[]
            //    {
            //        new Claim(ClaimTypes.Name, user.Id.ToString())
            //    }),
            //    Expires = DateTime.UtcNow.AddDays(7),
            //    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            //};
            //var token = tokenHandler.CreateToken(tokenDescriptor);
            //user.Token = tokenHandler.WriteToken(token);

            user.Token = GenerateToken(username);

            return user.WithoutPassword();
        }

        private string GenerateToken(string username)
        {
            //So we checked, and let's create a valid token with some Claim
            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.JwtKey));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Audience = _configuration.Value.JwtAudience,
                Issuer = _configuration.Value.JwtIssuer,
                Subject = new ClaimsIdentity(new Claim[]{
                    //Add any claim
                    new Claim(ClaimTypes.Name, username),
                }),
                //Expire token after some time
                Expires = DateTime.UtcNow.AddDays(_configuration.Value.JwtExpireDays),
                //Let's also sign token credentials for a security aspect, this is important!!!
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return tokenString;
        }

        //private string GenerateToken(string username)
        //{
        //    SecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.Value.JwtKey));

        //    var token = new JwtSecurityToken(
        //        issuer: _configuration.Value.JwtIssuer,
        //        audience: _configuration.Value.JwtAudience,
        //        claims: new[]
        //        {
        //            new Claim(JwtRegisteredClaimNames.UniqueName, username),
        //            //new Claim(JwtRegisteredClaimNames.Email, email),
        //            new Claim(JwtRegisteredClaimNames.NameId, Guid.NewGuid().ToString())
        //        },
        //        expires: DateTime.Now.AddMinutes(_configuration.Value.JwtExpireMinute),
        //        signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
        //    );

        //    return new JwtSecurityTokenHandler().WriteToken(token);
        //}

        public IEnumerable<User> GetAll()
        {
            return _users.WithoutPasswords();
        }
    }
}