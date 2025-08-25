using Meeting.Application.Services;
using Meeting.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Meeting.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly SmtpClient _smtpClient;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            _smtpClient = new SmtpClient
            {
                Host = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com",
                Port = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587"),
                EnableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true"),
                Credentials = new NetworkCredential(
                    _configuration["EmailSettings:FromEmail"],
                    _configuration["EmailSettings:FromPassword"]
                )
            };
        }

        public async Task SendWelcomeEmailAsync(User user)
        {
            try
            {
                var subject = "Hoş Geldiniz - Meeting App";
                var body = GenerateWelcomeEmailBody(user);

                await SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation($"Welcome email sent to {user.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send welcome email to {user.Email}");
                throw;
            }
        }

        public async Task SendMeetingNotificationEmailAsync(User user, MeetingEntity meeting, string notificationType)
        {
            try
            {
                var subject = $"Toplantı {notificationType} - {meeting.Title}";
                var body = GenerateMeetingNotificationBody(user, meeting, notificationType);

                await SendEmailAsync(user.Email, subject, body);
                _logger.LogInformation($"Meeting {notificationType} email sent to {user.Email} for meeting {meeting.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send meeting {notificationType} email to {user.Email}");
                throw;
            }
        }

        public async Task SendMeetingReminderEmailAsync(User user, MeetingEntity meeting)
        {
            await SendMeetingNotificationEmailAsync(user, meeting, "Hatırlatması");
        }

        public async Task SendMeetingCancellationEmailAsync(User user, MeetingEntity meeting)
        {
            await SendMeetingNotificationEmailAsync(user, meeting, "İptali");
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_configuration["EmailSettings:FromEmail"] ?? "noreply@meetingapp.com"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await _smtpClient.SendMailAsync(mailMessage);
        }

        private string GenerateWelcomeEmailBody(User user)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>Hoş Geldiniz {user.FirstName} {user.LastName}!</h2>
                        <p style='color: #666; line-height: 1.6;'>
                            Meeting App'e başarıyla kayıt oldunuz. Artık toplantılarınızı kolayca yönetebilir, 
                            düzenleyebilir ve takip edebilirsiniz.
                        </p>
                        <div style='background-color: #f8f9fa; padding: 15px; border-radius: 5px; margin: 20px 0;'>
                            <h3 style='color: #333; margin-top: 0;'>Hesap Bilgileriniz:</h3>
                            <p><strong>Ad Soyad:</strong> {user.FirstName} {user.LastName}</p>
                            <p><strong>E-posta:</strong> {user.Email}</p>
                            <p><strong>Telefon:</strong> {user.PhoneNumber}</p>
                            <p><strong>Kayıt Tarihi:</strong> {user.CreatedAt:dd.MM.yyyy HH:mm}</p>
                        </div>
                        <p style='color: #666;'>
                            Sorularınız için bizimle iletişime geçebilirsiniz.
                        </p>
                        <p style='color: #666;'>
                            İyi günler dileriz,<br>
                            Meeting App Ekibi
                        </p>
                    </div>
                </body>
                </html>";
        }

        private string GenerateMeetingNotificationBody(User user, MeetingEntity meeting, string notificationType)
        {
            return $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
                        <h2 style='color: #333;'>Toplantı {notificationType}</h2>
                        <p style='color: #666;'>Merhaba {user.FirstName} {user.LastName},</p>
                        
                        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
                            <h3 style='color: #333; margin-top: 0;'>{meeting.Title}</h3>
                            <p><strong>Başlangıç:</strong> {meeting.StartDate:dd.MM.yyyy HH:mm}</p>
                            <p><strong>Bitiş:</strong> {meeting.EndDate:dd.MM.yyyy HH:mm}</p>
                            {(string.IsNullOrEmpty(meeting.Description) ? "" : $"<p><strong>Açıklama:</strong> {meeting.Description}</p>")}
                            {(string.IsNullOrEmpty(meeting.DocumentPath) ? "" : "<p><strong>Döküman:</strong> Eklenti mevcut</p>")}
                        </div>
                        
                        <p style='color: #666;'>
                            Detaylar için uygulamaya giriş yapabilirsiniz.
                        </p>
                        
                        <p style='color: #666;'>
                            İyi günler dileriz,<br>
                            Meeting App Ekibi
                        </p>
                    </div>
                </body>
                </html>";
        }

        public void Dispose()
        {
            _smtpClient?.Dispose();
        }
    }
}