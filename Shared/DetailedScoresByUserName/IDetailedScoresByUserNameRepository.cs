namespace Arcade.Shared.DetailedScoresByUserName
{
    public interface IDetailedScoresByUserNameRepository
    {
        void SetupTable();

        void Save(DetailedScoresByUserName score);

        DetailedScoresByUserName Load(string username);
    }
}
