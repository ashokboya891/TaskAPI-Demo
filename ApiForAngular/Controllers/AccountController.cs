using ApiForAngular.ApplicationDbContext;
using ApiForAngular.DTO;
using ApiForAngular.Enums;
using ApiForAngular.ServiceContracts;
using CitiesExample.DTO;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CitiesExample.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationROle> _roleManager;
        private readonly IConfiguration _config;
        private readonly TaskManagerDbContext _context;
        private readonly IEmailService _emailService;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            RoleManager<ApplicationROle> roleManager, IConfiguration config, IJwtService service, TaskManagerDbContext context, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _config = config;
            _jwtService = service;
            this._context = context;
            _emailService = emailService;
        }

        [HttpGet("logout")]
        public async Task<IActionResult> GetLogout()
        {
            await _signInManager.SignOutAsync(); //it will remove identity cookie from developer tool in browser so as long as cookie preset in chrome it will consider as loged in account
            return NoContent();
        }
        [HttpPost]
        [Route("login-2FA")]
        public async Task<IActionResult> LoginWithOTP(string code,string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            var signIn = await _signInManager.TwoFactorSignInAsync("Email", code, false, false);
            if (signIn.Succeeded)
            {
                if (user != null)
                {
                    // Retrieve user roles
                    var roles = await _userManager.GetRolesAsync(user);

                    // Generate JWT token
                    var authenticationResponse = await _jwtService.CreateJwtToken(user);
                    authenticationResponse.Roles = roles.ToList();

                    // Update user if necessary
                    await _userManager.UpdateAsync(user);
                    return Ok(authenticationResponse);

                    //    var authClaims = new List<Claim>
                    //   {
                    //    new Claim(ClaimTypes.Name, user.UserName),
                    //        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    //};
                    //    var userRoles = await _userManager.GetRolesAsync(user);
                    //    foreach (var role in userRoles)
                    //    {
                    //        authClaims.Add(new Claim(ClaimTypes.Role, role));
                    //    }

                    //    var jwtToken = GetToken(authClaims);

                    //    return Ok(new
                    //    {
                    //        token = new JwtSecurityTokenHandler().WriteToken(jwtToken),
                    //        expiration = jwtToken.ValidTo
                    //    });
                    //returning the token...

                }
            }
            return StatusCode(StatusCodes.Status404NotFound,
                new Response { Status = "Success", Message = $"Invalid Code" });
        }
        private JwtSecurityToken GetToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT:Secret"]));

            var token = new JwtSecurityToken(
                issuer: _config["JWT:ValidIssuer"],
                audience: _config["JWT:ValidAudience"],
                expires: DateTime.Now.AddDays(2),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }
        [HttpPost("login")]
        public async Task<IActionResult> PostLogin([FromBody] LoginDTO loginDTO)
        {
            if (!ModelState.IsValid)
            {
                string errorMessage = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Problem(errorMessage);
            }

            // Check if user exists by email
            var user = await _userManager.FindByEmailAsync(loginDTO.Email);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            // Check if email is confirmed
            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                return Unauthorized("Email not confirmed.");
            }
            //ensure twofactor is enabled in db through manull query  with update query 1
            //if (user.TwoFactorEnabled)
            //{
            //    await _signInManager.SignOutAsync();
            //    await _signInManager.PasswordSignInAsync(user, loginDTO.Password, false, true);
            //    var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");

            //    var message = new Message(new string[] { user.Email! }, "OTP Confrimation", token);
            //    _emailService.SendEmail(message);

            //    return StatusCode(StatusCodes.Status200OK,
            //     new Response { Status = "Success", Message = $"We have sent an OTP to your Email {user.Email}" });
            //}

            // Sign in using the UserName
            var result = await _signInManager.PasswordSignInAsync(user.UserName, loginDTO.Password, isPersistent: true, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Retrieve user roles
                var roles = await _userManager.GetRolesAsync(user);

                // Generate JWT token
                var authenticationResponse = await _jwtService.CreateJwtToken(user);
                authenticationResponse.Roles = roles.ToList();

                // Update user if necessary
                await _userManager.UpdateAsync(user);

                return Ok(authenticationResponse);
            }
            else if (result.IsLockedOut)
            {
                return Problem("Account is locked out.");
            }
            else if (result.IsNotAllowed)
            {
                return Problem("Login is not allowed.");
            }
            else if (result.RequiresTwoFactor)
            {
                return Problem("Two-factor authentication is required.");
            }
            else
            {
                return Problem("Invalid email or password.");
            }
        }





        //}
        //[HttpPost("Register")]
        //public async Task<IActionResult> PostRegister([FromBody] RegisterDTO registerDTO)
        //{
        //    // Validation
        //    if (!ModelState.IsValid)
        //    {
        //        string errorMessage = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        //        return Problem(errorMessage);
        //    }

        //    // Create user
        //    ApplicationUser user = new ApplicationUser()
        //    {
        //        Email = registerDTO.Email,
        //        PhoneNumber = registerDTO.Phone,
        //        UserName = registerDTO.Email,
        //        PersonName = registerDTO.PersonName,
        //    };

        //    IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

        //    if (result.Succeeded)
        //    {
        //        // Assign the "User" role

        //            ApplicationROle role = new ApplicationROle()
        //            {
        //                Name = UserTypeOptions.User.ToString()
        //            };
        //            await _roleManager.CreateAsync(role);
        //            await _userManager.AddToRoleAsync(user, UserTypeOptions.User.ToString());

        //        await _userManager.AddToRoleAsync(user, "User");

        //        // Sign-in
        //        await _signInManager.SignInAsync(user, isPersistent: false);

        //        var authenticationResponse = _jwtService.CreateJwtToken(user);
        //        // Retrieve user roles
        //        var roles = await _userManager.GetRolesAsync(user);
        //        authenticationResponse.Roles = roles.ToList(); // Include roles in the response

        //        await _userManager.UpdateAsync(user);

        //        return Ok(authenticationResponse);
        //    }
        //    else
        //    {
        //        string errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description));
        //        return Problem(errorMessage);
        //    }
        //}


        //[HttpPost("Register")]
        //public async Task<IActionResult> PostRegister([FromBody] RegisterDTO registerDTO)
        //{
        //    //Validation
        //    if (ModelState.IsValid == false)
        //    {
        //        string errorMessage = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        //        return Problem(errorMessage);
        //    }


        //    //Create user
        //    ApplicationUser user = new ApplicationUser()
        //    {
        //        Email = registerDTO.Email,
        //        PhoneNumber = registerDTO.Phone,
        //        UserName = registerDTO.Email,
        //        PersonName = registerDTO.PersonName,

        //    };

        //    IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);

        //    if (result.Succeeded)
        //    {
        //        //sign-in
        //        await _signInManager.SignInAsync(user, isPersistent: false);

        //        var authenticationResponse = _jwtService.CreateJwtToken(user);

        //        await _userManager.UpdateAsync(user);

        //        return Ok(authenticationResponse);
        //    }
        //    else
        //    {
        //        string errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description)); //error1 | error2
        //        return Problem(errorMessage);
        //    }
        //}
        [HttpGet]
        [Route("getUserByEmail/{Email}")]
        public async Task<IActionResult> IsEmailAlreadyRegistered(string Email)
        {
            ApplicationUser user = await _userManager.FindByEmailAsync(Email);
            return Ok(user);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> PostRegister([FromBody] RegisterDTO registerDTO)
        {
            // Validation
            if (!ModelState.IsValid)
            {
                string errorMessage = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return Problem(errorMessage);
            }
            //Add the User in the database
            IdentityUser iuser = new()
            {
                Email = registerDTO.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = registerDTO.Email,
                //TwoFactorEnabled = true
            };
            // Create user
            ApplicationUser user = new ApplicationUser
            {
                FirstName = registerDTO.PersonName.FirstName,
                LastName = registerDTO.PersonName.LastName,
                DateOfBirth = Convert.ToDateTime(registerDTO.DateOfBirth),
                Gender = registerDTO.Gender,
                Email = registerDTO.Email,
                CountryID = registerDTO.CountryID,
                PhoneNumber = registerDTO.PhoneNumber,
                ReceiveNewsLetters=registerDTO.ReceiveNewsLetters,
                UserName = registerDTO.Email // Or ensure uniqueness for UserName
                
            };

            IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);
            if (!result.Succeeded)
            {
                string errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description));
                return Problem(errorMessage);
            }

            // Assign the "User" role
            var roleExist = await _roleManager.RoleExistsAsync(UserTypeOptions.User.ToString());
            if (!roleExist)
            {
                await _roleManager.CreateAsync(new ApplicationROle { Name = UserTypeOptions.User.ToString() });
            }
            await _userManager.AddToRoleAsync(user, UserTypeOptions.User.ToString());

            // Sign-in
            await _signInManager.SignInAsync(user, isPersistent: false);

            // Generate JWT token
            var authenticationResponse = await _jwtService.CreateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);
            authenticationResponse.Roles = roles.ToList();

            // Associate skills with the user
            if (registerDTO.Skills != null && registerDTO.Skills.Any())
            {
                var skills = registerDTO.Skills.Select(skillDto => new Skill
                {
                    ApplicationUserId = user.Id,
                    SkillName = skillDto.SkillName,
                    SkillLevel = skillDto.SkillLevel
                }).ToList();

                _context.Skills.AddRange(skills);
                await _context.SaveChangesAsync();
            }

            // Email confirmation
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { token, email = user.Email }, Request.Scheme);
            var message = new Message(new string[] { user.Email }, "Confirmation email link", confirmationLink);
            try
            {
                _emailService.SendEmail(message);
            }
            catch (Exception ex)
            {
                // Log email sending error here
                Console.WriteLine("Email sending failed: " + ex.Message);
            }

            // Update user and complete registration
            await _userManager.UpdateAsync(user);

            return Ok(authenticationResponse);
        }

        //[HttpPost("Register")]
        //public async Task<IActionResult> PostRegister([FromBody] RegisterDTO registerDTO)
        //{
        //    // Validation
        //    if (!ModelState.IsValid)
        //    {
        //        string errorMessage = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
        //        return Problem(errorMessage);
        //    }

        //        // Create user
        //    ApplicationUser user = new ApplicationUser()
        //    {
        //        FirstName=registerDTO.PersonName.FirstName,
        //        LastName=registerDTO.PersonName.LastName,
        //        DateOfBirth=Convert.ToDateTime(registerDTO.DateOfBirth),
        //        Gender=registerDTO.Gender,
        //        Email = registerDTO.Email,
        //        CountryID=registerDTO.CountryID,
        //        PhoneNumber = registerDTO.PhoneNumber,
        //        UserName = registerDTO.PersonName.FirstName+""+registerDTO.PersonName.LastName,
        //    };

        //    IdentityResult result = await _userManager.CreateAsync(user, registerDTO.Password);
        //    if (result.Succeeded)
        //    {
        //        // Assign the "User" role
        //        ApplicationROle role = new ApplicationROle()
        //        {
        //            Name = UserTypeOptions.User.ToString()
        //        };
        //        await _roleManager.CreateAsync(role);
        //        await _userManager.AddToRoleAsync(user, UserTypeOptions.User.ToString());

        //        // Sign-in
        //        await _signInManager.SignInAsync(user, isPersistent: false);

        //        var authenticationResponse = await _jwtService.CreateJwtToken(user); // Create JWT token
        //        var roles = await _userManager.GetRolesAsync(user);
        //        authenticationResponse.Roles = roles.ToList(); // Include roles in the response

        //        // Associate skills with the created user
        //        if (registerDTO.Skills != null && registerDTO.Skills.Any())
        //        {
        //            var skills = registerDTO.Skills.Select(skillDto => new Skill
        //            {
        //                ApplicationUserId = user.Id,    // Set the ApplicationUserId
        //                SkillName = skillDto.SkillName, // Map SkillName from DTO
        //                SkillLevel = skillDto.SkillLevel // Map SkillLevel from DTO
        //            }).ToList();

        //            _context.Skills.AddRange(skills);
        //            await _context.SaveChangesAsync();
        //        }
        //        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        //        var confirmationLink = Url.Action(nameof(ConfirmEmail), "Authentication", new { token, email = user.Email }, Request.Scheme);
        //        var message = new Message(new string[] { user.Email! }, "Confirmation email link", confirmationLink!);
        //        _emailService.SendEmail(message);



        //        await _userManager.UpdateAsync(user);

        //        return Ok(authenticationResponse);
        //    }
        //    else
        //    {
        //        string errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description));
        //        return Problem(errorMessage);
        //    }
        //if (result.Succeeded)
        //{
        //    // Assign the "User" role
        //    ApplicationROle role = new ApplicationROle()
        //    {
        //        Name = UserTypeOptions.User.ToString()
        //    };
        //    await _roleManager.CreateAsync(role);
        //    await _userManager.AddToRoleAsync(user, UserTypeOptions.User.ToString());

        //   /// await _userManager.AddToRoleAsync(user, "User");

        //    // Sign-in
        //    await _signInManager.SignInAsync(user, isPersistent: false);

        //    var authenticationResponse = await _jwtService.CreateJwtToken(user); // Use await here
        //                                                                         // Retrieve user roles
        //    var roles = await _userManager.GetRolesAsync(user);
        //    authenticationResponse.Roles = roles.ToList(); // Include roles in the response

        //    await _userManager.UpdateAsync(user);

        //    return Ok(authenticationResponse);
        //}
        //else
        //{
        //    string errorMessage = string.Join(" | ", result.Errors.Select(e => e.Description));
        //    return Problem(errorMessage);
        //}
        //}
        [HttpGet]
        [Route("api/getallemployees")]
        public async Task<IActionResult> GetAllEmployees()
        {
            List<ApplicationUser> users = this._context.Users.ToList();
            List<ApplicationUser> employeeUsers = new List<ApplicationUser>();

            foreach (var item in users)
            {
                if ((await this._userManager.IsInRoleAsync(item, "User")))
                {
                    employeeUsers.Add(item);
                }
            }
            return Ok(employeeUsers);
        }

        [HttpPost("generate-new-jwt-token")]
        public async Task<IActionResult> GenerateNewAccessToken(TokenModel tokenModel)
        {
            if (tokenModel == null)
            {
                return BadRequest("Invalid client request");
            }

            ClaimsPrincipal? principal =_jwtService.GetPrincipalFromJwtToken(tokenModel.Token);  //payload data from clinet will be there inside pricipal
            if (principal == null)
            {
                return BadRequest("Invalid jwt access token");
            }

            string? email = principal.FindFirstValue(ClaimTypes.Email);

            ApplicationUser? user = await _userManager.FindByEmailAsync(email);

            AuthenticationResponse authenticationResponse = await _jwtService.CreateJwtToken(user);

            //AuthenticationResponse authenticationResponse = _jwtService.CreateJwtToken(user);



            await _userManager.UpdateAsync(user);

            return Ok(authenticationResponse);
        }

    }
}
