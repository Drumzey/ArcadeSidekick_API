namespace Arcade.Shared.Repositories
{
    public interface IClubRepository
    {
        void SetupTable();

        void Save(ClubInformation key);

        ClubInformation Load(string partitionKey);
    }
}
