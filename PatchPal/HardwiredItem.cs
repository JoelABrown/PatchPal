namespace Mooseware.PatchPal
{
    /// <summary>
    /// Structure used for loading hardwired VideoNode settings from the settings JSON file.
    /// </summary>
    internal class HardwiredItem
    {
        /// <summary>
        /// NodeId enumeration value represented as a string.
        /// </summary>
        public string NodeId { get; set; }
        /// <summary>
        /// NodeType enumeration value represented as a string.
        /// </summary>
        public string NodeType { get; set; }
        /// <summary>
        /// The NodeId enumeration of the configured input of the VideoNode, represented as a string.
        /// </summary>
        public string Input { get; set; }

        public HardwiredItem()
        {
            NodeId = string.Empty;
            NodeType = string.Empty;
            Input = string.Empty;
        }
    }
}
