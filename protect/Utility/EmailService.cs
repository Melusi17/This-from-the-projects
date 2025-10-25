//using MailKit.Net.Smtp;
//using MailKit.Security;
//using MimeKit;
//using Microsoft.Extensions.Options;
//using System.Net;
//using System.Net.Mail;
//using IbhayiPharmacy.Models;

//namespace IbhayiPharmacy.Utility
//{
//    public class EmailService
//    {
//        private readonly SmtpSettings _smtpSettings;

//        public EmailService(IOptions<SmtpSettings> smtpSettings)
//        {
//            _smtpSettings = smtpSettings.Value;
//        }
//        //public async Task SendEmailAsync(string to, string subject, string body, string from = null)
//        //{
//        //    var smtpClient = new SmtpClient("smtp.office365.com", 587)
//        //    {
//        //        Credentials = new NetworkCredential("Siphokuhle.Tana@mandela.ac.za", "Madiba24#"),
//        //        EnableSsl = true
//        //    };

//        //    var mailMessage = new MailMessage
//        //    {
//        //        From = new MailAddress(from ?? "Siphokuhle.Tana@mandela.ac.za"),
//        //        Subject = subject,
//        //        Body = body,
//        //        IsBodyHtml = true
//        //    };

//        //    mailMessage.To.Add(to);

//        //    await smtpClient.SendMailAsync(mailMessage);
//        //}
//        public async Task SendEmailAsync(string to, string subject, string body)
//        {
//            var email = new MimeMessage();
//            email.From.Add(new MailboxAddress("Ibhayi ", _smtpSettings.Username));
//            email.To.Add(MailboxAddress.Parse(to));
//            email.Subject = subject;
//            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

//            using var smtp = new System.Net.Mail.SmtpClient();
//            try
//            {
//                await smtp.ConnectAsync(_smtpSettings.Server, 587, SecureSocketOptions.StartTls);   
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine("Error connecting to SMTP server: " + ex.Message);
//                throw;
//            }
//            await smtp.AuthenticAsync(_smtpSettings.Username, _smtpSettings.Password);
//            await smtp.SendAsync(email);
//            await smtp.DisconnectAsync(true);



//        }
//    }
//}
