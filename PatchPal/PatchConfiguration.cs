using System.Collections.Generic;

namespace Mooseware.PatchPal
{
    /// <summary>
    /// The list of VideoNodes which are user-configurable at runtime. These include HDMI Patch Cables and Video Matrix Selections. 
    /// Implements a SettingManager to read the data from JSON.
    /// </summary>
    internal class PatchConfiguration : SettingsManager<PatchConfiguration>
    {
        public List<ConfigurableItem> PatchItems { get; set; }
        public PatchConfiguration()
        {
            PatchItems = new ();
        }
    }
}
