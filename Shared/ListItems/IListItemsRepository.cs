namespace Arcade.Shared.ListItems
{
    public interface IListItemsRepository
    {
        void SetupTable();

        void Save(ListItems items);

        ListItems Load(string key);
    }
}
