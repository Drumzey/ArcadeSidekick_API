namespace Arcade.Shared.Leagues
{
    public interface ILeagueRepository
    {
        void SetupTable();

        void Save(LeagueDetails league);

        LeagueDetails Load(string partitionKey);
    }
}
