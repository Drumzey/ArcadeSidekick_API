using System;
using System.Collections.Generic;

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

            var messages = messageRepo.Load(to);

            if (messages == null)
            {
                messages = new Messages();
                messages.UserName = to;
                messages.Notifications = new List<Message>();
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

            var messages = messageService.Load(to);

            if (messages == null)
            {
                messages = new Messages();
                messages.UserName = to;
                messages.Notifications = new List<Message>();
            }

            messages.Notifications.Add(mess);

            messageService.Save(messages);
        }
    }
}
