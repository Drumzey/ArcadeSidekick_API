namespace Arcade.Shared.LocationByMachine
{
    public interface ILocationByMachineRepository
    {
        void SetupTable();

        void Save(LocationByMachine location);

        LocationByMachine Load(string machine);
    }
}
