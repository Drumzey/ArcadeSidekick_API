namespace Arcade.Shared.SettingsByGameName
{
    public interface ISettingsByGameNameRepository
    {
        void SetupTable();

        void Save(SettingsByGameName settings);

        SettingsByGameName Load(string gamename);
    }
}
