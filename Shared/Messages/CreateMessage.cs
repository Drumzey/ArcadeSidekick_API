using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Messages
{
    public static class CreateMessage
    {
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

            var messages = ((IMessageRepository)services.GetService(typeof(IMessageRepository))).Load(to);

            if (messages == null)
            {
                messages = new Messages();
                messages.UserName = to;
                messages.Notifications = new List<Message>();
            }

            messages.Notifications.Add(mess);

            ((IMessageRepository)services.GetService(typeof(IMessageRepository))).Save(messages);
        }
    }
}
