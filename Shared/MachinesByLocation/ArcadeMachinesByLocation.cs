using System.Collections.Generic;
using Arcade.Shared.Shared;

namespace Arcade.Shared.MachinesByLocation
{
    public class ArcadeMachinesByLocation
    {
        public string Location { get; set; }

        public Dictionary<string, List<Setting>> Machines { get; set; }
    }
}
