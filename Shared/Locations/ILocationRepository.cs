namespace Arcade.Shared.Locations
{
    public interface ILocationRepository
    {
        void SetupTable();

        void Save(Location key);

        Location Load(string partitionKey);
    }
}
