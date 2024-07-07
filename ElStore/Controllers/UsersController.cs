using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElStore.Models;
using ElStore.Models.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace ElStore.Controllers
{
    public class UsersController : Controller
    {
        private readonly IConfiguration _config;
        private readonly SignInManager<AllUser> _signInManager;
        private readonly UserManager<AllUser> _userManager;

        public UsersController(IConfiguration config, SignInManager<AllUser> signInManager, UserManager<AllUser> userManager)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize]
        public async Task<IActionResult> AccountInformation()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction(nameof(Login));
            }
            return View(user);
        }
        
        public IActionResult Register()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVm)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVm);
            }

            var existingUser = await _userManager.FindByNameAsync(registerVm.Login);
            if (existingUser != null)
            {
                ModelState.AddModelError("Login", "Chose another login");
                return View(registerVm);
            }

            existingUser = await _userManager.FindByEmailAsync(registerVm.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Chose another email");
                return View(registerVm);
            }

            var user = new AllUser
            {
                UserName = registerVm.Login,
                Email = registerVm.Email,
                PhoneNumber = registerVm.PhoneNumber,
                NormalizedEmail = registerVm.Email.ToUpper(),
                NormalizedUserName = registerVm.Login.ToUpper(),
                Login = registerVm.Login,
                JWT = GenerateToken(registerVm.Login),
                PasswordHash = GenerateToken(registerVm.Password)
            };

            var result = await _userManager.CreateAsync(user, registerVm.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, WC.CustomerRole);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(nameof(AccountInformation));
            }
            
            return View(registerVm);
        }

        public IActionResult Login()
        {
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVm)
        {
            if (!ModelState.IsValid)
            {
                return View(loginVm);
            }

            var result = await _signInManager.PasswordSignInAsync(loginVm.UserName, loginVm.Password, false, lockoutOnFailure: false);
            if (result.Succeeded)
            {
                var user = await _userManager.FindByNameAsync(loginVm.UserName);
                if (user != null && user.UserName != null)
                {
                    return RedirectToAction(nameof(AccountInformation));
                }
            }

            return RedirectToAction(nameof(Register));
            
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
        
        private string GenerateToken(string userName)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? string.Empty));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userName)
            };

            var token = new JwtSecurityToken(
                _config["Jwt:Issuer"],
                _config["Jwt:Audience"],
                claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdmin(RegisterVM registerVm)
        {
            if (!ModelState.IsValid)
            {
                return View(registerVm);
            }

            var existingUser = await _userManager.FindByNameAsync(registerVm.Login);
            if (existingUser != null)
            {
                ModelState.AddModelError("Login", "Chose another login");
                return View(registerVm);
            }

            existingUser = await _userManager.FindByEmailAsync(registerVm.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Chose another email");
                return View(registerVm);
            }

            var user = new AllUser
            {
                UserName = registerVm.Login,
                Email = registerVm.Email,
                PhoneNumber = registerVm.PhoneNumber,
                NormalizedEmail = registerVm.Email.ToUpper(),
                NormalizedUserName = registerVm.Login.ToUpper(),
                Login = registerVm.Login,
                JWT = GenerateToken(registerVm.Login),
                PasswordHash = GenerateToken(registerVm.Password)
            };

            var result = await _userManager.CreateAsync(user, registerVm.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, WC.AdminRole);
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(nameof(AccountInformation));
            }
            
            return View(registerVm);
        }
        
        [AllowAnonymous]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties { RedirectUri = Url.Action(nameof(GoogleResponse)) };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        [AllowAnonymous]
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
            if (!result.Succeeded)
                return RedirectToAction(nameof(Login));

            var email = result.Principal.FindFirstValue(ClaimTypes.Email);
            if (email != null)
            {
                string emailPrefix = email.Replace("@gmail.com", "");
                var user = await _userManager.FindByEmailAsync(email);
            
                var phoneNumber = result.Principal.FindFirstValue(ClaimTypes.MobilePhone);
            
                if (user == null)
                {
                    user = new AllUser
                    {
                        UserName = emailPrefix,
                        Email = email,
                        PhoneNumber = phoneNumber,
                        NormalizedEmail = email.ToUpper(),
                        NormalizedUserName = emailPrefix.ToUpper(),
                        Login = emailPrefix,
                        JWT = GenerateToken(emailPrefix),
                        PasswordHash = GenerateToken(emailPrefix),
                    };
                    
                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return RedirectToAction(nameof(Register));
                    }
                    
                    await _userManager.AddToRoleAsync(user, WC.CustomerRole);
                }

                await _signInManager.SignInAsync(user, isPersistent: false);
            }

            return RedirectToAction(nameof(AccountInformation));
        }
    }
}
