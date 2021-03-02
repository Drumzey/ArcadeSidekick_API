using System.Collections.Generic;

namespace Arcade.GameDetails
{
    public interface IGameDetailsRepository
    {
        void SetupTable();

        void Save(GameDetailsRecord record);

        GameDetailsRecord Load(string gameName, string sortKey);

        List<GameDetailsRecord> QueryByGameName(string gameName);

        IEnumerable<GameDetailsRecord> AllRows();

        IEnumerable<GameDetailsRecord> AllGamesForUser(string userName);

        void Delete(string gameName, string sortKey);
    }
}
