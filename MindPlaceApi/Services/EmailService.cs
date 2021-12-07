using MindPlaceApi.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;
using MimeKit.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindPlaceApi.Services
{
    public interface IEmailService
    {
        Task SendBroadcastMailAsync(string subject, string message, IEnumerable<string> receivers, Guid key);
        Task SendMailToUserAsync(string fullName, string userEmail, string mailSubject, string message);
        Task SendSubscriptionMailAsync(AppUser professional, AppUser patient);
        Task SendConfirmationMailAsync(AppUser user, string confirmationLink);
        Task SendTestMailAsync();
        Task SendNotificationMailAsync(string username, string userEmail, string fullName, string subject, string message);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private string SMTPServer;
        private int SMTPPort;
        private string SMTPUser;
        private string SMTPPass;
        public EmailService(IConfiguration configuration, IWebHostEnvironment env)
        {
            _env = env;
            _configuration = configuration;
            SMTPServer = _configuration.GetSection("Smtp").GetSection("Server").Value;
            SMTPPort = int.Parse(_configuration.GetSection("Smtp").GetSection("Port").Value);
            SMTPUser = _configuration.GetSection("Smtp").GetSection("Username").Value;
            SMTPPass = _configuration.GetSection("Smtp").GetSection("Password").Value;
        }

        private async Task SendMailAsync(MimeMessage email)
        {
            // send email
            using var smtp = new SmtpClient();
            smtp.Connect(SMTPServer, SMTPPort, SecureSocketOptions.StartTls);
            smtp.Authenticate(SMTPUser, SMTPPass);
            await smtp.SendAsync(email).ConfigureAwait(false);
            await smtp.DisconnectAsync(true).ConfigureAwait(true);
        }

        private async Task SendMailAsync(MailboxAddress sender, MimeMessage message, List<MailboxAddress> receivers)
        {
            // send email
            using var smtp = new SmtpClient();
            smtp.Connect(SMTPServer, SMTPPort, SecureSocketOptions.StartTls);
            smtp.Authenticate(SMTPUser, SMTPPass);
            await smtp.SendAsync(message, sender, receivers).ConfigureAwait(false);
            await smtp.DisconnectAsync(true).ConfigureAwait(true);
        }

        public async Task SendTestMailAsync()
        {
            // create message
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("MindPlace", SMTPUser));
            email.To.Add(new MailboxAddress($"Nathan Omomowo", "firstnate0@gmail.com"));

            //CONSTRUCT THE MESSAGE BODY.
            string messageBody = "";
            messageBody += GetEmailHeader("Mail From MindPlace App", email);
            messageBody += "This is a test mail from the MindPlace api.";
            messageBody += GetEmailFooter("firstnate0@gmail.com");

            //var builder = new BodyBuilder
            //{
            //    TextBody = bodyPlaintext,
            //    HtmlBody = bodyHtml,
            //};
            //message.Body = builder.ToMessageBody();



            email.Body = new TextPart(TextFormat.Plain) { Text = messageBody };

            // send email
            await SendMailAsync(email);
        }

        public async Task SendMailToUserAsync(string fullName, string userEmail, string mailSubject, string message)
        {
            // create message
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("MindPlace", SMTPUser));
            email.To.Add(new MailboxAddress(fullName, userEmail));

            //CONSTRUCT THE MESSAGE BODY.
            string messageBody = "";
            messageBody += GetEmailHeader(mailSubject, email);
            messageBody += message;
            messageBody += GetEmailFooter(userEmail);

            email.Body = new TextPart(TextFormat.Html) { Text = messageBody };

            // send email
            await SendMailAsync(email).ConfigureAwait(false);
        }

        public async Task SendBroadcastMailAsync(string subject, string message, IEnumerable<string> receivers, Guid key)
        {
            // create message
            var mimeMessage = new MimeMessage();
            var sender = new MailboxAddress("MindsPlace", SMTPUser);

            //CONSTRUCT THE MESSAGE BODY.
            string messageBody = "";
            messageBody += GetEmailHeader(subject, mimeMessage);
            messageBody += message;
            messageBody += GetEmailFooter();
            mimeMessage.Body = new TextPart(TextFormat.Html) { Text = messageBody };


            List<MailboxAddress> validMailAddresses = new List<MailboxAddress>();

            //loop and get all valid email addresses.
            foreach (var mailAddress in receivers)
            {
                try
                {
                    //CONVERT EMAIL ADDRESSES TO "MAILboxAdress" TYPE
                    var validMailAddress = MailboxAddress.Parse(mailAddress);
                    validMailAddresses.Add(validMailAddress);
                }
                catch (Exception)
                {
                    //not a valid email address
                    //skip the address.


                    //maybe write to text file here.
                    continue;
                }
            }

            //sort the list alphabetically.
            validMailAddresses.Sort();
            for (int i = 0; i < validMailAddresses.Count; i += 50)
            {
                //divide into batches of 50
                //add the email adresses to the batch.
                var batchEmails = validMailAddresses.Skip(i).Take(50).ToList();

                //get the LOG folder.
                var contentRootPath = _env.ContentRootPath;
                var logsFolderPath = Path.Combine(contentRootPath, "Logs");

                // send email
                await SendMailAsync(sender, mimeMessage, batchEmails).ConfigureAwait(false);

                //Create or get "Logs" directory/folder.
                DirectoryInfo di = Directory.CreateDirectory(logsFolderPath);
                //token to be appended to filename to make it unique.
                string token = key.ToString().Replace("-", "").Substring(0, 7);
                //creating file path...
                var deliveredMailsFile = Path.Combine(logsFolderPath, $"deliveredMails_{token}.txt");
                if (File.Exists(deliveredMailsFile))
                {
                    //append batch mail addresses to file containing successfully sent mail addresss.
                    File.AppendAllLines(deliveredMailsFile, batchEmails.Select(x => x.ToString()), Encoding.UTF8);
                }
                else
                {
                    //create text file.
                    //brief description about the log file.
                    var logDescription = $"Delivered batch mails for {DateTime.UtcNow.ToShortDateString()} at {DateTime.UtcNow.ToLongTimeString()}";

                    using (var fs = File.OpenWrite(deliveredMailsFile))
                    {
                        //write log description to file...
                        using (var sr = new StreamWriter(fs))
                        {
                            sr.WriteLine(logDescription);
                            sr.WriteLine();
                        }
                    }
                    //append batch mail addresses to file containing successfully sent mail addresss.
                    File.AppendAllLines(deliveredMailsFile, batchEmails.Select(x => x.ToString()), Encoding.UTF8);
                }
            }
        }

        private string GetEmailHeader(string caption, MimeMessage mail)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;

            caption = textInfo.ToTitleCase(caption.ToLower());
            mail.Subject = caption;

            string T = "";
            T += "--------------------------------------------------------------------------------" + Environment.NewLine;
            T += "mindplace.com\n";
            T += caption + "\n";
            T += "Date: " + string.Format("{0:dd/MM/yyyy hh:mm:ss tt} UTC", DateTime.UtcNow) + "\n";
            T += "--------------------------------------------------------------------------------\n";
            T += "\n";
            return T;
        }

        public async Task SendConfirmationMailAsync(AppUser user, string confirmationLink)
        {
            // create message
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("MindsPlace", SMTPUser));
            email.To.Add(new MailboxAddress($"{user.FirstName} {user.LastName}", user.Email));
            email.Subject = "Please confirm your email";

            //CONSTRUCT THE MESSAGE BODY.
            string messageBody = "";
            messageBody += "<div>";
            messageBody += "<div style='background-color: #f0eee7; padding: 50px 50px;'>";
            messageBody += "<div style = 'margin: auto; width: 100px; height: 100px' >";
            messageBody += "<img src='https://pbs.twimg.com/profile_images/1105583743392198658/qdMa06Oc_400x400.jpg' style='width: 100%; height: 100%;' />";
            messageBody += " </div>";
            messageBody += "<div style='background-color: white; text-align: center; margin-top: 5px; padding: 25px 0px; '>";
            messageBody += "<p>Verify Email Account</p>";
            messageBody += $"<p>Hello <span style='font-weight:bold'>{user.FirstName}</span></p>";
            messageBody += "<p>To proceed with your registration on Mindsplace, Please verify your email account by clicking on the button below</p> ";
            messageBody += "<divstyle='margin: auto;'>";
            messageBody += $"<a href='{confirmationLink}' style='border-color: #e08e0b; background-color: #f39c12; border: 1px solid transparent; border-radius: 3px; padding: 10px; color: white; text-decoration: none;'>Verify Email</a>";
            messageBody += "</div>";
            messageBody += "<p>If you didnt request this registration, you can ignore this email</p>";
            messageBody += "<p><b>MindsPlace</b> Support </p>";
            messageBody += $"<p>This link becomes invalid on <span style='color: red'>{DateTime.Now.AddHours(24).ToUniversalTime().ToString("dd-MM-yyyy hh:mm tt")} GMT</span></p>";
            messageBody += $"<p style='font-weight: bold;'>This message was sent to {user.Email}</p>";
            messageBody += $"<p>© Copyright {DateTime.Today.Year} mindplace.com. All Rights Reserved.</p>";
            messageBody += "</div>";
            messageBody += "</div>";
            messageBody += "</div>";

            email.Body = new TextPart(TextFormat.Html) { Text = messageBody };

            // send email
            await SendMailAsync(email).ConfigureAwait(false);
        }

        public async Task SendSubscriptionMailAsync(AppUser professional, AppUser patient)
        {
            // create message
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("MindsPlace", SMTPUser));
            email.To.Add(new MailboxAddress($"{professional.FirstName} {professional.LastName}", professional.Email));

            //CONSTRUCT THE MESSAGE BODY.
            string messageBody = "";
            messageBody += GetEmailHeader($"New Subscription from {patient.FirstName} {patient.LastName} on MindsPlace", email);
            messageBody += $"<b>{patient.FirstName} {patient.LastName}</b> just subscribed to you";
            messageBody += GetEmailFooter(professional.Email);

            email.Body = new TextPart(TextFormat.Html) { Text = messageBody };

            // send email
            await SendMailAsync(email).ConfigureAwait(false);
        }

        public async Task SendNotificationMailAsync(string username, string userEmail, string fullName, string subject, string message)
        {
            // create message
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("MindPlace", SMTPUser));
            email.To.Add(new MailboxAddress($"{fullName}", userEmail));

            //CONSTRUCT THE MESSAGE BODY.
            string messageBody = "";
            messageBody += GetEmailHeader(subject, email);
            messageBody += message;
            messageBody += GetEmailFooter(userEmail);

            email.Body = new TextPart(TextFormat.Html) { Text = messageBody };

            // send email
            await SendMailAsync(email).ConfigureAwait(false);
        }


        /// <summary>
        /// gets info about "MindPlace" that can be appended to the footer of an email so recipients know where the email's from and hopefully build trust.
        /// </summary>
        /// <param name="emailTo"> the intended recipient of the mail.</param>
        /// <returns> info about "MindPlace" that can be appended to the footer of an email so recipients know where the email's from.</returns>
        private string GetEmailFooter(string emailTo = "")
        {
            string messageBody = "\n\nIf you have any questions or need technical assitance, ";
            //messageBody += "support is available here: http://www.mindplace.com/support/ticket.aspx\n";
            messageBody += "send a mail to support@mindplace.com\n";
            messageBody += "\n";
            messageBody += "Or mail to:\n";
            messageBody += "MindsPlace Solutions Ltd\n";
            messageBody += "Ikeja, Lagos.\n";
            messageBody += "\n";
            messageBody += "Thank you for using MindsPlace!\n";
            messageBody += "\n";
            messageBody += "mindplace.com Team\n";
            messageBody += "\n";
            messageBody += "NOTE:\n";
            messageBody += "Please do not reply to this message, which was sent from an unmonitored ";
            messageBody += "e-mail address.\n";
            messageBody += "Mail sent to this address cannot be answered.\n";
            messageBody += "\n";

            if (!string.IsNullOrWhiteSpace(emailTo))
            {
                messageBody += "This message was sent to " + emailTo + ".\n";
                //messageBody += "Remove yourself from future email here:\n";
                //messageBody += "http://portal.smslive247.com/account/remove.aspx?me=" + emailTo + "\n";
            }

            messageBody += "--------------------------------------------------------------------------------\n";
            messageBody += "© Copyright " + DateTime.Today.Year + " mindplace.com. All Rights Reserved.\n";
            messageBody += "\n";

            return messageBody;
        }

        private bool VerifyEmailAddress(string emailAddress)
        {
            try
            {
                MailboxAddress.Parse(emailAddress);
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
    }
}
