using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using UserManagement.Interfaces;

namespace UserManagement.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly string _fromEmail;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly bool _enableSsl;
    private readonly string _frontendBaseUrl;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        _fromEmail = _configuration["Email:FromEmail"] ?? "noreply@example.com";
        _smtpServer = _configuration["Email:SmtpServer"] ?? "localhost";
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "25");
        _smtpUsername = _configuration["Email:SmtpUsername"] ?? string.Empty;
        _smtpPassword = _configuration["Email:SmtpPassword"] ?? string.Empty;
        _enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "false");
        _frontendBaseUrl = _configuration["Frontend:BaseUrl"] ?? "http://localhost:3000";
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        try
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(to);

            using var smtpClient = new SmtpClient(_smtpServer, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = _enableSsl
            };

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation("Email sent successfully to {Email}", to);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", to);
            throw;
        }
    }

    public async Task SendEmailVerificationAsync(string email, string userId, string token)
    {
        var encodedToken = WebUtility.UrlEncode(token);
        var verificationLink = $"{_frontendBaseUrl}/verify-email?userId={userId}&token={encodedToken}";

        var subject = "Verify Your Email Address";
        var body = $@"
            <html>
            <body>
                <h2>Email Verification</h2>
                <p>Thank you for registering! Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationLink}'>Verify Email</a></p>
                <p>If you did not request this, please ignore this email.</p>
                <p>This link will expire in 24 hours.</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }

    public async Task SendPasswordResetEmailAsync(string email, string userId, string token)
    {
        var encodedToken = WebUtility.UrlEncode(token);
        var resetLink = $"{_frontendBaseUrl}/reset-password?userId={userId}&token={encodedToken}";

        var subject = "Reset Your Password";
        var body = $@"
            <html>
            <body>
                <h2>Password Reset</h2>
                <p>You have requested to reset your password. Please click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you did not request this, please ignore this email.</p>
                <p>This link will expire in 15 minutes.</p>
            </body>
            </html>";

        await SendEmailAsync(email, subject, body);
    }
}