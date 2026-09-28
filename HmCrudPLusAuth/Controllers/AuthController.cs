using HmCrudPLusAuth.Common;
using HmCrudPLusAuth.Data;
using HmCrudPLusAuth.DTOs.Auth;
using HmCrudPLusAuth.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HmCrudPLusAuth.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DataContext _db;
        private readonly IConfiguration _config;
        private readonly SmtpService _smtpService;

        public AuthController(DataContext db, IConfiguration config, SmtpService smtpService)
        {
            _db = db;
            _config = config;
            _smtpService = smtpService;
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto request)
        {
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName) || 
                string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(Result<string>.BadRequest("All fields are required"));

            var existingUser = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (existingUser != null)
                return BadRequest(Result<string>.BadRequest("Email already registered"));

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // Generate verification code for registration
            var verificationCode = _smtpService.GenerateVerificationCode();

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = hashedPassword,
                VerificationCode = verificationCode,
                VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15),
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            try
            {
                var emailBody = _smtpService.GetVerificationEmailTemplate(user.FirstName, verificationCode);
                _smtpService.SendEmail(
                    "Verify Your HmCrudAuth Account 🔐",
                    emailBody,
                    user.Email,
                    isHtml: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Verification email failed to send: {ex.Message}");
                return StatusCode(500, Result<string>.BadRequest("Failed to send verification email"));
            }

            return Ok(Result<string>.Ok("Account created! Verification code sent to your email. Please verify to complete registration."));
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(Result<string>.BadRequest("Email and password are required"));

            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return NotFound(Result<string>.NotFound("Invalid email or password"));

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!isPasswordValid)
                return BadRequest(Result<string>.BadRequest("Invalid email or password"));

            // Generate verification code
            var verificationCode = _smtpService.GenerateVerificationCode();
            user.VerificationCode = verificationCode;
            user.VerificationCodeExpiry = DateTime.UtcNow.AddMinutes(15);
            _db.SaveChanges();

            // Send verification code email
            try
            {
                var emailBody = _smtpService.GetVerificationEmailTemplate(user.FirstName, verificationCode);
                _smtpService.SendEmail(
                    "Your HmCrudAuth Verification Code 🔐",
                    emailBody,
                    user.Email,
                    isHtml: true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Verification email failed: {ex.Message}");
                return StatusCode(500, Result<string>.BadRequest("Failed to send verification email"));
            }

            return Ok(Result<string>.Ok("Verification code sent to your email. Please verify to complete login."));
        }

        [HttpPost("verify-code")]
        public IActionResult VerifyCode([FromBody] VerifyCodeDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.VerificationCode))
                return BadRequest(Result<string>.BadRequest("Email and verification code are required"));

            var user = _db.Users.FirstOrDefault(u => u.Email == request.Email);
            if (user == null)
                return NotFound(Result<string>.NotFound("User not found"));

            // Check if verification code is expired
            if (user.VerificationCodeExpiry == null || DateTime.UtcNow > user.VerificationCodeExpiry)
                return BadRequest(Result<string>.BadRequest("Verification code has expired"));

            // Check if verification code matches
            if (user.VerificationCode != request.VerificationCode)
                return BadRequest(Result<string>.BadRequest("Invalid verification code"));

            // If email not verified yet (registration), mark as verified
            if (!user.IsEmailVerified)
                user.IsEmailVerified = true;

            // Clear verification code
            user.VerificationCode = null;
            user.VerificationCodeExpiry = null;
            _db.SaveChanges();

            // Generate JWT token
            var token = CreateAccessToken(user);
            return Ok(Result<string>.Ok(token));
        }

        private string CreateAccessToken(User user)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA256(
                System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var claims = new System.Collections.Generic.List<System.Security.Claims.Claim>
            {
                new (System.Security.Claims.ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (System.Security.Claims.ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new (System.Security.Claims.ClaimTypes.Email, user.Email),
                new ("firstName", user.FirstName),
                new ("lastName", user.LastName),
            };

            var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
                key, Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
