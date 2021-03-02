namespace Arcade.Shared
{
    public static class StringExtensions
    {
        public static string EmailReminder(this string str)
        {
            var split = str.Split('@');
            var domain = split[1];
            var name = split[0];
            var reminder = string.Empty;

            switch (name.Length)
            {
                case 1:
                    reminder = "*";
                    break;
                case 2:
                    reminder = name[0] + "*";
                    break;
                case 3:
                    reminder = name[0] + "**";
                    break;
                case 4:
                    reminder = name[0] + "***";
                    break;
                case 5:
                    reminder = name[0] + "***" + name[4];
                    break;
                case 6:
                    reminder = name[0] + "***" + name[4] + "*";
                    break;
                case 7:
                    reminder = name[0] + "***" + name[4] + "**";
                    break;
                case 8:
                    reminder = name[0] + "***" + name[4] + "***";
                    break;
                case 9:
                    reminder = name[0] + "***" + name[4] + "****";
                    break;
                case 10:
                    reminder = name[0] + "***" + name[4] + "****" + name[9];
                    break;
                case 11:
                    reminder = name[0] + "***" + name[4] + "****" + name[9] + "*";
                    break;
                case 12:
                    reminder = name[0] + "***" + name[4] + "****" + name[9] + "**";
                    break;
                case 13:
                    reminder = name[0] + "***" + name[4] + "****" + name[9] + "***";
                    break;
                case 14:
                    reminder = name[0] + "***" + name[4] + "****" + name[9] + "****";
                    break;
                case 15:
                    reminder = name[0] + "***" + name[4] + "****" + name[9] + "****" + name[14];
                    break;
                case 16:
                    reminder = name[0] + "***" + name[4] + "****" + name[9] + "****" + name[14] + "*";
                    break;
                case 17:
                    reminder = name[0] + "***" + name[4] + "****" + name[9] + "****" + name[14] + "**";
                    break;
                case 18:
                    reminder = name[0] + "***" + name[4] + "****" + name[9] + "****" + name[14] + "***";
                    break;
                case 19:
                    reminder = name[0] + "***" + name[4] + "****" + name[9] + "****" + name[14] + "****";
                    break;
                case 20:
                    reminder = name[0] + "***" + name[4] + "****" + name[9] + "****" + name[14] + "****" + name[19];
                    break;
                default:
                    reminder = name[0] + "***" + name[4] + "****" + name[9] + "****" + name[14] + "****" + name[19];
                    reminder = reminder.PadRight(name.Length, '*');
                    break;
            }

            return reminder + "@" + domain;
        }
    }
}
