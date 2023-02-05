using Arcade.GameDetails.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Arcade.GameDetails.Handlers
{
    public class HintsAndTipsHandler
    {
        private IServiceProvider services;

        public HintsAndTipsHandler()
        {
        }

        public HintsAndTipsHandler(IServiceProvider services)
        {
            this.services = services;
        }

        public List<HintsAndTips> GetTipsForGame(IGameDetailsRepository repo, string gameName)
        {
            var details = repo.Load(gameName, "HintsAndTips");
            if (details == null)
            {
                return new List<HintsAndTips>();
            }

            return GetTipsFromDetailsRecord(details);
        }

        private List<HintsAndTips> GetTipsFromDetailsRecord(GameDetailsRecord details)
        {
            var hintsAndTips = new List<HintsAndTips>();
            hintsAndTips.AddRange(details.HintsAndTips.Where(x => x.Verified));
            var uniqueSettings = hintsAndTips.Distinct();
            return uniqueSettings.ToList();
        }

        public void SaveTipsForGame(IGameDetailsRepository repo, string body)
        {
            var data = JsonConvert.DeserializeObject<dynamic>(body);

            var gameName = (string)data.GameName;
            var userName = (string)data.UserName;
            var hintText = (string)data.HintText;

            var hintsRecord = repo.Load(gameName, "HintsAndTips");
            var submissionHints = new HintsAndTips
            {
                UserName = userName,
                HintText = hintText,
            };

            if (hintsRecord == null)
            {
                hintsRecord = new GameDetailsRecord();
                hintsRecord.Game = gameName;
                hintsRecord.SortKey = "HintsAndTips";
                hintsRecord.DataType = "HintsAndTips";
                hintsRecord.HintsAndTips = new List<HintsAndTips>();
            }

            hintsRecord.HintsAndTips.Add(submissionHints);

            repo.Save(hintsRecord);
        }
    }
}
