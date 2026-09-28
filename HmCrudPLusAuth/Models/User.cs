using HmCrudPLusAuth.Common;

namespace HmCrudPLusAuth.Models
{
    public class User : Entity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string? VerificationCode { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? VerificationCodeExpiry { get; set; }
        public List<Product> Products { get; set; } = new List<Product>();
    }
}
