using HmCrudPLusAuth.Common;

namespace HmCrudPLusAuth.Services.Auth
{
    public interface IAuthService
    {
        Result<string> Register(string firstName, string lastName, string email, string password);
        Result<string> Login(string email, string password);
    }
}
