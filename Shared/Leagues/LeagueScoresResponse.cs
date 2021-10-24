using System.Collections.Generic;

namespace Arcade.Shared.Leagues
{
    public class LeagueScoresResponse
    {
        public Dictionary<string, PlayerLeagueResponse> LeagueResults { get; set; }
    }
}
