using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Arcade.Shared
{
    public class Email : IEmail
    {
        public void EmailSecret(string secret, string email, string username)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("arcadesidekick@gmail.com", "Am1darTetr15"),                
                EnableSsl = true                
            };

            //ENBEDDED IMAGES
            //var inlineLogo = new LinkedResource("Images/logo.png", "image/png"); 
            //inlineLogo.ContentId = Guid.NewGuid().ToString();

            //string body = string.Format(@"
            //    <p>Welcome</p>
            //        <img src=""cid:{0}"" />
            //        <p>{1}</p>
            //    ", inlineLogo.ContentId, secret);

            string body = string.Format("<h1>Welcome {0}</h1><br>Here is your secret key<br>{1}<br>Please enter it into the sidekick to verify your account<br>Keep it safe as you will be required to use it if you ever move device", username, secret);

            MailMessage msg = new MailMessage("arcadesidekick@gmail.com", email, "Arcade Sidekick - Verify Account", body);
            msg.IsBodyHtml = true;

            //var view = AlternateView.CreateAlternateViewFromString(body, null, "text/html");
            //view.LinkedResources.Add(inlineLogo);
            //msg.AlternateViews.Add(view);
            
            try
            { 
                client.Send(msg);
            }
            catch (Exception e)
            {
                //Cant send the email for some reason...
            }
            finally
            {
                if (msg != null)
                {
                    msg.Dispose();
                }
            }
        }
    }
}
