using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.GameDetails
{
    public class ScoreDetails
    {
        public string Date { get; set; }

        public string SettingsId { get; set; }

        public string LevelName { get; set; }

        public string EventName { get; set; }

        public string Score { get; set; }

        public string UserName { get; set; }

        public string Location { get; set; }

        public bool Image { get; set; }

        public bool Verified { get; set; }

        public override bool Equals(Object obj)
        {
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                var s = (ScoreDetails)obj;

                return
                    s.Score == Score &&
                    s.Date == Date &&
                    s.SettingsId == SettingsId &&
                    s.EventName == EventName &&
                    s.LevelName == LevelName &&
                    s.Location == Location;
            }
        }

        public override int GetHashCode()
        {
            return new { Score, Date, SettingsId, EventName, LevelName, Location }.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Date} - {Score} - {SettingsId} - {LevelName} - {Location} - {EventName}";
        }
    }
}
