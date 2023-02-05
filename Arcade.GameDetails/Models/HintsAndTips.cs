namespace Arcade.GameDetails.Models
{
    public class HintsAndTips
    {
        public string UserName { get; set; }

        public string HintText { get; set; }

        public int UpVotes { get; set; } = 0;

        public int DownVotes { get; set; } = 0;

        public bool Verified { get; set; } = false;
    }
}
