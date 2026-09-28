using HmCrudPLusAuth.Common;
using HmCrudPLusAuth.Data;
using HmCrudPLusAuth.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HmCrudPLusAuth.Services.Auth
{
    public class AuthService : IAuthService
    {
        private DataContext _db;
        private IConfiguration _config;

        public AuthService(DataContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public Result<string> Register(string firstName, string lastName, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || 
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return Result<string>.BadRequest("All fields are required");

            var existingUser = _db.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser != null)
                return Result<string>.BadRequest("Email already registered");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                Password = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            _db.SaveChanges();

            var token = CreateAccessToken(user);
            return Result<string>.Ok(token);
        }

        public Result<string> Login(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return Result<string>.BadRequest("Email and password are required");

            var user = _db.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
                return Result<string>.NotFound("Invalid email or password");

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(password, user.Password);
            if (!isPasswordValid)
                return Result<string>.BadRequest("Invalid email or password");

            var token = CreateAccessToken(user);
            return Result<string>.Ok(token);
        }

        private string CreateAccessToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new (ClaimTypes.Email, user.Email),
                new ("firstName", user.FirstName),
                new ("lastName", user.LastName),
            };

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
