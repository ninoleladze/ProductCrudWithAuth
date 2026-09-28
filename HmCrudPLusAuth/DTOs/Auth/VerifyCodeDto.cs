namespace HmCrudPLusAuth.DTOs.Auth
{
    public class VerifyCodeDto
    {
        public string Email { get; set; } = string.Empty;
        public string VerificationCode { get; set; } = string.Empty;
    }
}
