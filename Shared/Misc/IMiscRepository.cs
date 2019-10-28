namespace Arcade.Shared.Misc
{
    public interface IMiscRepository
    {
        void SetupTable();

        void Save(Misc key);

        Misc Load(string partitionKey, string sortKey);
    }
}
