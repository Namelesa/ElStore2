using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ElStore.Data;
using ElStore.Models;
using ElStore.Models.ViewModel;
using System.Security.Cryptography;

namespace ElStore.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UserController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError(string.Empty, "User with this email already exists.");
                    return View(model);
                }
                
                var user = new Users
                {
                    Email = model.Email,
                    HashPassword = HashPassword(model.Password),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Role = "Customer"
                };
                
                if (user.Email == "pdo090318@gmail.com")
                {
                    user.Role = "Admin";
                }
                
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Generate and store JWT token
                var token = GenerateJwtToken(user);
                Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true });

                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user != null && VerifyPassword(model.Password, user.HashPassword))
                {
                    // Generate and store JWT token
                    var token = GenerateJwtToken(user);
                    Response.Cookies.Append("jwt", token, new CookieOptions { HttpOnly = true });

                    Console.WriteLine($"User {user.Email} logged in successfully.");
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Console.WriteLine($"Invalid login attempt for email {model.Email}.");
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(model); // Возвращаем представление с моделью и ошибкой
                }
            }

            Console.WriteLine("Invalid model state.");
            return View(model); // Возвращаем представление с моделью, если данные не валидны
        }


        private bool VerifyPassword(string enteredPassword, string storedHashPassword)
        {
            using var sha256 = SHA256.Create();
            var enteredPasswordHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
            var enteredPasswordHashBase64 = Convert.ToBase64String(enteredPasswordHash);

            return storedHashPassword == enteredPasswordHashBase64;
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private string GenerateJwtToken(Users user)
        {
            var secretKey = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentException("JWT secret key is missing in configuration.");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
