using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NetCoreJwtAuthentication.Services;
using NetCoreJwtAuthentication.Entities;
using System.Linq;
using NetCoreJwtAuthentication.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NetCoreJwtAuthentication.Helpers;

namespace NetCoreJwtAuthentication.Controllers
{
    //[Authorize]
    [EnableCors("ApiCorsPolicy")]
    [Route("[controller]/[action]")]
    public class UsersController : Controller
    {
        private UserService _userService;
        private readonly IOptions<AppSettings> configuration;
        public UsersController(IOptions<AppSettings> configuration)
        {
            this.configuration = configuration;
            _userService = new UserService(this.configuration);
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Username, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            return Ok(user);
        }

        [Authorize("Bearer")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }
    }
}

