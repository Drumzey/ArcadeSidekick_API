namespace Arcade.Shared.ScoreByLocation
{
    public interface IScoreByLocationRepository
    {
        void SetupTable();

        void Save(ScoreByLocation score);

        ScoreByLocation Load(string location);
    }
}
