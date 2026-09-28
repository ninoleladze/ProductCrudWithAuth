using System.Net;
using System.Net.Mail;

namespace HmCrudPLusAuth.Common
{
    public class SmtpService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpService> _logger;

        public SmtpService(IConfiguration config, ILogger<SmtpService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public void SendEmail(string subject, string body, string email, bool isHtml = false)
        {
            try
            {
                var smtpEmail = _config["Smtp:Email"];
                var smtpPassword = _config["Smtp:Password"];
                var smtpHost = _config["Smtp:Host"];
                var smtpPort = int.Parse(_config["Smtp:Port"] ?? "587");

                if (string.IsNullOrEmpty(smtpEmail) || string.IsNullOrEmpty(smtpPassword) || 
                    string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(email))
                {
                    _logger.LogError("SMTP configuration is missing or incomplete");
                    throw new InvalidOperationException("SMTP configuration is missing");
                }

                using var mail = new MailMessage();
                mail.From = new MailAddress(smtpEmail, "HmCrudAuth");
                mail.Subject = subject;
                mail.To.Add(email);
                mail.Body = body;
                mail.IsBodyHtml = isHtml;

                using var smtpClient = new SmtpClient(smtpHost)
                {
                    Port = smtpPort,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpEmail, smtpPassword),
                    Timeout = 10000
                };

                smtpClient.Send(mail);
                _logger.LogInformation($"Email sent successfully to {email}");
            }
            catch (SmtpException ex)
            {
                _logger.LogError($"SMTP error sending email: {ex.Message}");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending email to {email}: {ex.Message}");
                throw;
            }
        }

        public string GenerateVerificationCode()
        {
            return new Random().Next(100000, 999999).ToString();
        }

        public string GetWelcomeEmailTemplate(string firstName, string lastName, string email)
        {
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Welcome to HmCrudAuth</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #2c3e50;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px;
        }
        .wrapper { max-width: 600px; margin: 0 auto; }
        .email-container {
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            overflow: hidden;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 50px 30px;
            text-align: center;
            color: white;
        }
        .header-emoji { font-size: 48px; margin-bottom: 15px; display: block; }
        .header-title { font-size: 28px; font-weight: 700; margin-bottom: 8px; }
        .content { padding: 50px 40px; }
        .greeting { font-size: 22px; font-weight: 600; color: #2c3e50; margin-bottom: 20px; }
        .greeting strong {
            background: linear-gradient(135deg, #667eea, #764ba2);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }
        .message { color: #555; font-size: 15px; line-height: 1.8; margin-bottom: 25px; }
        .features-section { margin: 40px 0; }
        .features-title { font-size: 16px; font-weight: 600; color: #2c3e50; margin-bottom: 20px; }
        .features-grid {
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 15px;
        }
        .feature-item {
            background: rgba(102, 126, 234, 0.08);
            border-left: 3px solid #667eea;
            padding: 15px;
            border-radius: 6px;
        }
        .feature-icon { font-size: 20px; margin-bottom: 8px; }
        .feature-text { color: #2c3e50; font-size: 13px; font-weight: 600; }
        .cta-section { margin: 35px 0; text-align: center; }
        .cta-button {
            display: inline-block;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            text-decoration: none;
            padding: 14px 35px;
            border-radius: 8px;
            font-weight: 600;
        }
        .footer {
            background: linear-gradient(135deg, #f8f9fa 0%, #f0f2f5 100%);
            padding: 30px 40px;
            border-top: 1px solid #e0e0e0;
            text-align: center;
        }
        .footer-content { color: #95a5a6; font-size: 12px; }
        .footer-logo { font-weight: 700; color: #667eea; margin-bottom: 8px; }
    </style>
</head>
<body>
    <div class=""wrapper"">
        <div class=""email-container"">
            <div class=""header"">
                <span class=""header-emoji"">🎉</span>
                <div class=""header-title"">Welcome to HmCrudAuth</div>
            </div>
            <div class=""content"">
                <div class=""greeting"">Welcome, <strong>" + firstName + " " + lastName + @"</strong>! 👋</div>
                <div class=""message"">We're thrilled to have you join the HmCrudAuth community! Your account has been successfully created.</div>
                <div class=""features-section"">
                    <div class=""features-title"">✨ What You Can Do</div>
                    <div class=""features-grid"">
                        <div class=""feature-item"">
                            <div class=""feature-icon"">📦</div>
                            <div class=""feature-text"">Manage Products</div>
                        </div>
                        <div class=""feature-item"">
                            <div class=""feature-icon"">🔐</div>
                            <div class=""feature-text"">Enterprise Security</div>
                        </div>
                        <div class=""feature-item"">
                            <div class=""feature-icon"">⚡</div>
                            <div class=""feature-text"">Real-time Updates</div>
                        </div>
                        <div class=""feature-item"">
                            <div class=""feature-icon"">🎯</div>
                            <div class=""feature-text"">Full CRUD Control</div>
                        </div>
                    </div>
                </div>
                <div class=""cta-section"">
                    <a href=""https://yourapp.com/dashboard"" class=""cta-button"">Go to Dashboard →</a>
                </div>
                <div class=""message"">Your email: <strong>" + email + @"</strong></div>
            </div>
            <div class=""footer"">
                <div class=""footer-content"">
                    <div class=""footer-logo"">HmCrudAuth</div>
                    <p>&copy; 2024 HmCrudAuth. All rights reserved.</p>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";
        }

        public string GetVerificationEmailTemplate(string firstName, string verificationCode)
        {
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Email Verification - HmCrudAuth</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
            line-height: 1.6;
            color: #2c3e50;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            padding: 20px;
        }
        .wrapper { max-width: 600px; margin: 0 auto; }
        .email-container {
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3);
            overflow: hidden;
        }
        .header {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            padding: 40px 30px;
            text-align: center;
            color: white;
        }
        .header-title { font-size: 24px; font-weight: 700; }
        .content { padding: 50px 40px; }
        .greeting { font-size: 22px; font-weight: 600; color: #2c3e50; margin-bottom: 20px; }
        .greeting strong {
            background: linear-gradient(135deg, #667eea, #764ba2);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
        }
        .message { color: #555; font-size: 15px; line-height: 1.8; margin-bottom: 25px; }
        .code-box {
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 40px;
            border-radius: 10px;
            text-align: center;
            margin: 35px 0;
        }
        .code-label {
            font-size: 12px;
            font-weight: 600;
            text-transform: uppercase;
            letter-spacing: 1px;
            opacity: 0.95;
            margin-bottom: 15px;
        }
        .verification-code {
            font-size: 48px;
            font-weight: 700;
            letter-spacing: 8px;
            font-family: 'Courier New', monospace;
            margin: 20px 0;
        }
        .warning-box {
            background-color: #fff8e1;
            border-left: 4px solid #ffc107;
            padding: 20px;
            margin: 30px 0;
            border-radius: 6px;
        }
        .warning-text { color: #856404; font-size: 14px; line-height: 1.6; }
        .footer {
            background: linear-gradient(135deg, #f8f9fa 0%, #f0f2f5 100%);
            padding: 30px 40px;
            border-top: 1px solid #e0e0e0;
            text-align: center;
        }
        .footer-content { color: #95a5a6; font-size: 12px; }
        .footer-logo { font-weight: 700; color: #667eea; margin-bottom: 8px; }
    </style>
</head>
<body>
    <div class=""wrapper"">
        <div class=""email-container"">
            <div class=""header"">
                <div class=""header-title"">🔐 Email Verification</div>
            </div>
            <div class=""content"">
                <div class=""greeting"">Hey <strong>" + firstName + @"</strong>! 👋</div>
                <div class=""message"">Thank you for joining HmCrudAuth! To complete your registration, please verify your email using the code below.</div>
                <div class=""code-box"">
                    <div class=""code-label"">Your Verification Code</div>
                    <div class=""verification-code"">" + verificationCode + @"</div>
                    <div style=""color: rgba(255,255,255,0.85); font-size: 12px; margin-top: 10px;"">Valid for 15 minutes</div>
                </div>
                <div class=""warning-box"">
                    <div class=""warning-text""><strong>⚠️ Never share this code</strong> with anyone. HmCrudAuth support will never ask for your verification code.</div>
                </div>
                <div class=""message"">If you didn't create this account, please ignore this email and your account will remain unverified.</div>
            </div>
            <div class=""footer"">
                <div class=""footer-content"">
                    <div class=""footer-logo"">HmCrudAuth</div>
                    <p>&copy; 2024 HmCrudAuth. All rights reserved.</p>
                </div>
            </div>
        </div>
    </div>
</body>
</html>";
        }
    }
}
