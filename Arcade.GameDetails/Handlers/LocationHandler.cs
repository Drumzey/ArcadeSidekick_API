using System.Collections.Generic;
using System.Linq;

namespace Arcade.GameDetails.Handlers
{
    public class LocationHandler
    {
        public List<string> Get(IGameDetailsRepository repo, string gameName)
        {
            var details = repo.QueryByGameName(gameName);
            return Get(details.Where(x => x.DataType.Equals("Location")).ToList());
        }

        public List<string> Get(List<GameDetailsRecord> details)
        {
            var locations = new List<string>();
            locations.AddRange(details.Select(x => x.SortKey));
            return locations;
        }
    }
}
