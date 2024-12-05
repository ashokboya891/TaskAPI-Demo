using ApiForAngular.ApplicationDbContext;
using ApiForAngular.DTO;
using ApiForAngular.ServiceContracts;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApiForAngular.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationROle> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public AuthenticationController(UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationROle> roleManager, IEmailService emailService,
            SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK, "sended successfully");
                      
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError, "This User Doesnot exist!");
        }

        [HttpGet("[action]")]

        public async Task<IActionResult> TestEmail()
        {
            var message = new Message(new string[] { "aboya375@gmail.com" }, "test", "<h3>hey ashok</h3>");
            _emailService.SendEmail(message);
            return StatusCode(StatusCodes.Status200OK,"Email sent successfully");
           
        }
    }
}
