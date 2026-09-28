# Product CRUD with Auth

An **ASP.NET Core 8 Web API** with product CRUD and account security: registration, login, and e-mail verification with a code sent over SMTP. Passwords are hashed with BCrypt and requests are authorized with JWT.

## Structure

- Controllers/ — ProductController (GET/POST/PUT/DELETE), AuthController (register, login, verify-code)
- Services/ — AuthService, ProductService
- Common/ — Result<T>, SmtpService, base Entity
- Data/ + Migrations/ — EF Core with SQL Server

## Run

```bash
dotnet ef database update --project HmCrudPLusAuth
dotnet run --project HmCrudPLusAuth
```
