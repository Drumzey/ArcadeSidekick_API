using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.GameDetails
{
    public class Setting
    {
        public string SettingsId { get; set; }

        public string LevelName { get; set; }

        public string Difficulty { get; set; }

        public string Lives { get; set; }

        public string ExtraLivesAt { get; set; }

        public string MameOrPCB { get; set; }

        public override bool Equals(Object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !this.GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Setting s = (Setting)obj;
                return
                    (LevelName == s.LevelName) &&
                    (Difficulty == s.Difficulty) &&
                    (Lives == s.Lives) &&
                    (ExtraLivesAt == s.ExtraLivesAt) &&
                    (MameOrPCB == s.MameOrPCB);
            }
        }

        public override int GetHashCode()
        {
            return new { LevelName, Difficulty, Lives, ExtraLivesAt, MameOrPCB }.GetHashCode();
        }

        public override string ToString()
        {        
            return $"{SettingsId} - {LevelName} - {Difficulty} - {Lives} - {ExtraLivesAt} - {MameOrPCB}";
        }
    }
}
