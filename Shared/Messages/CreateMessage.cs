using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcade.Shared.Messages
{
    public static class CreateMessage
    {
        public static Messages CreateWithoutPost(
            IMessageRepository messageRepo,
            string to,
            string from,
            string message,
            MessageTypeEnum messageType,
            Dictionary<string, string> data)
        {
            Message mess = new Message
            {
                From = from,
                Seen = false,
                TimeSet = DateTime.Now,
                Text = message,
                MessageType = messageType,
                Data = data,
            };

            Console.WriteLine($"Sending Messages to {to}");
            var messages = messageRepo.Load(to);

            if (messages == null)
            {
                messages = new Messages();
                messages.UserName = to;
                messages.Notifications = new List<Message>();
            }
            else
            {
                // This is loading all messages for this user and then appending one on the end
                // We dont want to keep all messages for ever.
                // Keep messages for the past 2 months ONLY
                var messagesThisMonth = messages.Notifications.Where(x => x.TimeSet > DateTime.Now.AddMonths(-2)).ToList();
                messages.Notifications = messagesThisMonth;
            }

            messages.Notifications.Add(mess);

            return messages;
        }

        public static void Create(
            IServiceProvider services,
            string to,
            string from,
            string message,
            MessageTypeEnum messageType,
            Dictionary<string, string> data)
        {
            Message mess = new Message
            {
                From = from,
                Seen = false,
                TimeSet = DateTime.Now,
                Text = message,
                MessageType = messageType,
                Data = data,
            };

            var messageService = (IMessageRepository)services.GetService(typeof(IMessageRepository));

            Console.WriteLine($"Sending Messages to {to}");
            var messages = messageService.Load(to);

            if (messages == null)
            {
                messages = new Messages();
                messages.UserName = to;
                messages.Notifications = new List<Message>();
            }
            else
            {
                // This is loading all messages for this user and then appending one on the end
                // We dont want to keep all messages for ever.
                // Keep messages for the past 2 months ONLY
                var messagesThisMonth = messages.Notifications.Where(x => x.TimeSet > DateTime.Now.AddMonths(-2)).ToList();
                messages.Notifications = messagesThisMonth;
            }

            messages.Notifications.Add(mess);

            messageService.Save(messages);
        }
    }
}
