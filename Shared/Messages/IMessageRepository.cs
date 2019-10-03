using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Messages
{
    public interface IMessageRepository
    {
        void SetupTable();

        void Save(Messages messages);

        Messages Load(string partitionKey);
    }
}
