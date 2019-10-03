namespace Arcade.Shared.ScoreByGameName
{
    public interface IScoreByGameNameRepository
    {
        void SetupTable();

        void Save(ScoreByGameName score);

        ScoreByGameName Load(string gamename);
    }
}
