using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Repositories
{
    public interface IUserRepository
    {
        void SetupTable();

        void Save(UserInformation key);

        UserInformation Load(string partitionKey);
    }
}
