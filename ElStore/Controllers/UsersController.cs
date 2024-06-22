using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ElStore.Models;
using ElStore.Models.ViewModel;
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
        private readonly IWebHostEnvironment _webHostEnvironment;

        public UsersController(IConfiguration config, SignInManager<AllUser> signInManager, UserManager<AllUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _config = config;
            _userManager = userManager;
            _signInManager = signInManager;
            _webHostEnvironment = webHostEnvironment;
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
                ModelState.AddModelError("Login", "Пользователь с таким логином уже существует.");
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
                if (user.Email == "pdo090318@gmail.com")
                {
                    await _userManager.AddToRoleAsync(user, WC.AdminRole);
                }
                else
                { 
                    await _userManager.AddToRoleAsync(user, WC.CustomerRole);
                }
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction(nameof(AccountInformation));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
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
                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateToken(user.UserName);
                return RedirectToAction(nameof(AccountInformation));
            }
            else
            {
                return RedirectToAction(nameof(Register));
            }
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
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
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
    }
}
