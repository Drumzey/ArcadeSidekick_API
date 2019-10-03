namespace Arcade.Shared.MachinesByLocation
{
    public interface IArcadeMachinesByLocationRepository
    {
        void SetupTable();

        void Save(ArcadeMachinesByLocation machine);

        ArcadeMachinesByLocation Load(string location);
    }
}
