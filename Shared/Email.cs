using System.Net;
using System.Net.Mail;

namespace Arcade.Shared
{
    public class Email : IEmail
    {
        public void EmailSecret(string secret, string email, string username, IEnvironmentVariables environment)
        {
            var client = new SmtpClient("smtp.live.com", 587)
            {
                Credentials = new NetworkCredential(environment.EmailAddress, environment.EmailPassword),
                EnableSsl = true,
            };

            string header = string.Format("<p><span style=\"font-size:28px;\">Welcome {0}!</span></p>", username);

            string body = string.Format(
                "<p>Thankyou for downloading Arcade Sidekick, the best place to record your classic arcade game scores.</p> " +
                "<p></p>" +
                "<p>Your secret code is {0}</p>" +
                "<p></p>" +
                "<p>Enter this into Arcade Sidekick to verify your account.</p>" +
                "<p>Please keep this code safe, you&#39;ll need it if you want to restore your account onto another device.</p>" +
                "<p></p>" +
                "<p>Now get setting some scores, rating some games and challenging your friends to a bit of healthy competition!</p>" +
                "<p></p>" +
                "<p>Rumz</p>" +
                "<p></p>" +
                "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/home\">Home</a></p>" +
                "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/faq\">Frequently Asked Questions</a></p>" +
                "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/games\">Available Games</a></p>" +
                "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/privacy\">Privacy Policy</a></p>", secret);

            MailMessage msg = new MailMessage(environment.EmailAddress, email, "Arcade Sidekick - Verify Account", header + body);
            msg.IsBodyHtml = true;

            try
            {
                client.Send(msg);
            }
            catch
            {
                // Cant send the email for some reason...
            }
            finally
            {
                if (msg != null)
                {
                    msg.Dispose();
                }
            }
        }

        public void EmailSecretReminder(string secret, string email, string username, IEnvironmentVariables environment)
        {
            var client = new SmtpClient("smtp.live.com", 587)
            {
                Credentials = new NetworkCredential(environment.EmailAddress, environment.EmailPassword),
                EnableSsl = true,
            };

            string header = string.Format("<p><span style=\"font-size:28px;\">Hello again {0}!</span></p>", username);

            string body = string.Format(
                "<p>You've lost your secret code. We did tell you not to do that! Never mind.</p> " +
                "<p></p>" +
                "<p>Your secret code is {0}</p>" +
                "<p></p>" +
                "<p>Try not to lose it again and keep it in a safe place.</p>" +
                "<p></p>" +
                "<p>Rumz</p>" +
                "<p></p>" +
                "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/home\">Home</a></p>" +
                "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/faq\">Frequently Asked Questions</a></p>" +
                "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/games\">Available Games</a></p>" +
                "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/privacy\">Privacy Policy</a></p>", secret);

            MailMessage msg = new MailMessage(environment.EmailAddress, email, "Arcade Sidekick - Forgotten Secret", header + body);
            msg.IsBodyHtml = true;

            try
            {
                client.Send(msg);
            }
            catch
            {
                // Cant send the email for some reason...
            }
            finally
            {
                if (msg != null)
                {
                    msg.Dispose();
                }
            }
        }

        public void EmailUsernameReminder(string email, IEnvironmentVariables environment)
        {
            var client = new SmtpClient("smtp.live.com", 587)
            {
                Credentials = new NetworkCredential(environment.EmailAddress, environment.EmailPassword),
                EnableSsl = true,
            };

            string header = string.Format("<p><span style=\"font-size:28px;\">Hello again!</span></p>");

            string body = string.Format("<p>You cant remember your username? Did you lose your memory when you added your last credit and continued?</p> " +
                        "<p></p>" +
                        "<p>Anyway your request has been logged and we will get back to you as soon as possible</p>" +
                        "<p></p>" +
                        "<p>Rumz</p>" +
                        "<p></p>" +
                        "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/home\">Home</a></p>" +
                        "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/faq\">Frequently Asked Questions</a></p>" +
                        "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/games\">Available Games</a></p>" +
                        "<p><a href=\"https://sites.google.com/site/arcadesidekickapp/privacy\">Privacy Policy</a></p>");

            MailMessage msg = new MailMessage(environment.EmailAddress, email, "Arcade Sidekick - Forgotten Username", header + body);
            msg.IsBodyHtml = true;

            string requestMessage = string.Format("<p>{0} has forgotten their user name. Find it for them and reply!</p>", email);

            MailMessage request = new MailMessage(environment.EmailAddress, environment.EmailAddress, "URGENT - Forgotten Username", requestMessage);
            request.IsBodyHtml = true;

            try
            {
                client.Send(request);
                client.Send(msg);
            }
            catch
            {
                // Cant send the emails for some reason...
            }
            finally
            {
                if (msg != null)
                {
                    msg.Dispose();
                    request.Dispose();
                }
            }
        }
    }
}
