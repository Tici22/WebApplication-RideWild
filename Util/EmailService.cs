using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_config["MailSettings:FromEmail"]));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Text) { Text = body };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_config["MailSettings:SmtpServer"], int.Parse(_config["MailSettings:Port"]), SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_config["MailSettings:Username"], _config["MailSettings:Password"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
public static class PasswordResetStore
{
    public static Dictionary<string, string> ResetCodes = new(); // email -> codice
}

