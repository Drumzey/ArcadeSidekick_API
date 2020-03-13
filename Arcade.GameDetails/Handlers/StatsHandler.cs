using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.GameDetails.Handlers
{
    public class StatsHandler
    {
        public GameDetailsRecord Get(IGameDetailsRepository repo, string gameName, string userName)
        {
            var details = repo.Load(gameName, userName);
            var settings = repo.Load(gameName, "Settings");
            details.Settings = settings.Settings;
            return details;
        }
    }
}
