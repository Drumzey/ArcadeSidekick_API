using System;
using System.Collections.Generic;
using System.Text;

namespace Arcade.GameDetails
{
    public class Setting
    {
        public string SettingsId { get; set; }
        
        public string Difficulty { get; set; }

        public string Lives { get; set; }

        public string ExtraLivesAt { get; set; }

        public string Credits { get; set; }

        public string MameOrPCB { get; set; }

        public bool IsBlank
        {
            get
            {
                if (Difficulty == "" &&
                    Lives == "" &&
                    ExtraLivesAt == "" &&
                    Credits == "" &&
                    MameOrPCB == "")
                {
                    return true;
                }

                return false;
            }
        }

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
                    AreEqual(Difficulty,s.Difficulty) &&
                    AreEqual(Lives,s.Lives) &&
                    AreEqual(ExtraLivesAt,s.ExtraLivesAt) &&
                    AreEqual(Credits,s.Credits) &&
                    AreEqual(MameOrPCB,s.MameOrPCB);
            }
        }

        private static bool AreEqual(string a, string b)
        {
            if (string.IsNullOrEmpty(a))
            {
                return string.IsNullOrEmpty(b);
            }
            else
            {
                return string.Equals(a, b);
            }
        }

        public override int GetHashCode()
        {
            return new { Difficulty, Lives, ExtraLivesAt, Credits, MameOrPCB }.GetHashCode();
        }

        public override string ToString()
        {        
            return $"{SettingsId} - {Difficulty} - {Lives} - {ExtraLivesAt} - {Credits} - {MameOrPCB}";
        }
    }
}
