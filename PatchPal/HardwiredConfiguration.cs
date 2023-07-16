using System.Collections.Generic;

namespace Mooseware.PatchPal
{
    /// <summary>
    /// The list of VideoNodes which are not user-configurable at runtime. Implements a SettingManager to read the data from JSON.
    /// </summary>
    internal class HardwiredConfiguration : SettingsManager<HardwiredConfiguration>
    {
        public List<HardwiredItem> HardwiredItems { get; set; }
        public HardwiredConfiguration()
        {
            HardwiredItems = new();
        }
    }
}
