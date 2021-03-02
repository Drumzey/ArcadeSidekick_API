namespace Arcade.Shared.Leagues
{
    public class PlayerLeagueResponse
    {
        public string UserName { get; set; }

        public int GamesPlayed { get; set; }

        public int FirstPlaces { get; set; }

        public int SecondPlaces { get; set; }

        public int ThirdPlaces { get; set; }

        public int TotalPoints { get; set; }
    }
}