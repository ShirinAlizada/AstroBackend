using AstroBackend.Application.Interfaces.Services;
using MailKit.Net.Smtp;
using MimeKit;

namespace AstroBackend.Infrastructure.Services
{
    /// <summary>
    /// IEmailService-in SMTP (MailKit) ilə tətbiqi. Supabase/Resend əvəzinə birbaşa SMTP
    /// istifadə olunur (istifadəçinin tapşırığına uyğun olaraq — "Supabase qoşmağa ehtiyac
    /// yoxdur"). Konfiqurasiya appsettings.json-dakı Smtp bölməsindən gəlir; Host boşdursa
    /// sakitcə heç nə göndərilmir (best-effort kanal, çağıranlar artıq try/catch edir).
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly string? _host;
        private readonly int _port;
        private readonly string? _user;
        private readonly string? _password;
        private readonly string _fromEmail;
        private readonly string _fromName;
        private readonly bool _enableSsl;

        public SmtpEmailService(string? host, int port, string? user, string? password, string fromEmail, string fromName, bool enableSsl)
        {
            _host = host;
            _port = port;
            _user = user;
            _password = password;
            _fromEmail = fromEmail;
            _fromName = fromName;
            _enableSsl = enableSsl;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(_host))
                return; // SMTP konfiqurasiya olunmayıbsa, sakitcə heç nə göndərmir.

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_fromName, _fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            using var client = new SmtpClient();
            await client.ConnectAsync(_host, _port, _enableSsl ? MailKit.Security.SecureSocketOptions.StartTls : MailKit.Security.SecureSocketOptions.Auto, ct);

            if (!string.IsNullOrWhiteSpace(_user))
                await client.AuthenticateAsync(_user, _password, ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
