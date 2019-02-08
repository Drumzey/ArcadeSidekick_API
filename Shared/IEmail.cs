using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared
{
    public interface IEmail
    {
        void EmailSecret(string secret, string email);
    }
}
