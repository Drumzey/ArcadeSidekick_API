using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace Arcade.Shared
{
    public class Email : IEmail
    {
        public void EmailSecret(string secret, string email)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential("arcadesidekick@gmail.com", "Am1darTetr15"),
                EnableSsl = true
            };
            client.Send("arcadesidekick@gmail.com", email, "Arcade Sidekick - Verify Account", secret);
        }
    }
}
