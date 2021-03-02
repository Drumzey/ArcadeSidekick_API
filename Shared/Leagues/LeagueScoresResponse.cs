using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.Shared.Leagues
{
    public class LeagueScoresResponse
    {
        public Dictionary<string, PlayerLeagueResponse> LeagueResults { get; set; }
    }
}
