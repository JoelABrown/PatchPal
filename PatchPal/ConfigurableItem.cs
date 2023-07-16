namespace Mooseware.PatchPal
{
    /// <summary>
    /// Structure used for persisting and loading configurable VideoNode settings.
    /// </summary>
    internal class ConfigurableItem
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
        /// <summary>
        /// The NodeId enumeration of the configured output of the VideoNode, represented as a string.
        /// </summary>
        public string Output { get; set; }

        public ConfigurableItem()
        {
            NodeId = string.Empty;
            NodeType = string.Empty;
            Input = string.Empty;
            Output = string.Empty;
        }
    }
}
