namespace Arcade.Shared.Messages
{
    public interface IMessageRepository
    {
        void SetupTable();

        void Save(Messages messages);

        Messages Load(string partitionKey);

        // List<Messages> Scan(IEnumerable<ScanCondition> scanConditions);
    }
}
