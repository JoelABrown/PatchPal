using System;

namespace Mooseware.PatchPal
{
    /// <summary>
    /// Utility methods for classifying and handling VideoNode objects
    /// NOTE: When adding nodes all of the methods in this class must
    ///       be updated to include the new NodeId values.
    /// </summary>
    internal static class Node
    {
        /// <summary>
        /// Gets the NodeType for a given NodeId
        /// </summary>
        /// <param name="nodeId">The NodeId to look up</param>
        /// <returns>The NodeType value corresponding to nodeId</returns>
        public static NodeType Type(NodeId nodeId)
        {
            return nodeId switch
            {
                NodeId.Atem6Input => NodeType.VideoDestination,
                NodeId.Atem6Patch => NodeType.PatchSink,
                NodeId.AtemAuxOut => NodeType.VideoSource,
                NodeId.Cam3Patch => NodeType.PatchSource,
                NodeId.ExternalDevice => NodeType.VideoSource,
                NodeId.ExternalPseudoPatch => NodeType.PatchSource,
                NodeId.HdmiCable1 => NodeType.HdmiCable,
                NodeId.HdmiCable2 => NodeType.HdmiCable,
                NodeId.HdmiCable3 => NodeType.HdmiCable,
                NodeId.HdmiCable4 => NodeType.HdmiCable,
                NodeId.HdmiCable5 => NodeType.HdmiCable,
                NodeId.HdmiCable6 => NodeType.HdmiCable,
                NodeId.HdmiCable7 => NodeType.HdmiCable,
                NodeId.HdmiCable8 => NodeType.HdmiCable,
                NodeId.HdmiCable9 => NodeType.HdmiCable,
                NodeId.HdmiCable10 => NodeType.HdmiCable,
                NodeId.HdmiSplitInputPatch => NodeType.PatchSink,
                NodeId.HdmiSplitOutput1Patch => NodeType.PatchSource,
                NodeId.HdmiSplitOutput2Patch => NodeType.PatchSource,
                NodeId.MxDest1Patch => NodeType.PatchSource,
                NodeId.MxDest2Patch => NodeType.PatchSource,
                NodeId.MxDest3Patch => NodeType.PatchSource,
                NodeId.MxDest4Patch => NodeType.PatchSource,
                NodeId.MxIn1 => NodeType.MxInput,
                NodeId.MxIn2 => NodeType.MxInput,
                NodeId.MxIn3 => NodeType.MxInput,
                NodeId.MxIn4 => NodeType.MxInput,
                NodeId.MxOut1 => NodeType.MxOutput,
                NodeId.MxOut2 => NodeType.MxOutput,
                NodeId.MxOut3 => NodeType.MxOutput,
                NodeId.MxOut4 => NodeType.MxOutput,
                NodeId.MxPick1 => NodeType.MxSelection,
                NodeId.MxPick2 => NodeType.MxSelection,
                NodeId.MxPick3 => NodeType.MxSelection,
                NodeId.MxPick4 => NodeType.MxSelection,
                NodeId.MxSrc3Patch => NodeType.PatchSink,
                NodeId.MxSrc4Patch => NodeType.PatchSink,
                NodeId.Pc3Shinybow => NodeType.VideoSource,
                NodeId.PulpitVga => NodeType.VideoSource,
                NodeId.PulpitVgaPatch => NodeType.PatchSource,
                NodeId.SanctuaryPatch => NodeType.PatchSink,
                NodeId.SanctuaryProjector => NodeType.VideoDestination,
                NodeId.SocHallNorthPatch => NodeType.PatchSink,
                NodeId.SocHallNorthProjector => NodeType.VideoDestination,
                NodeId.SocHallSouthPatch => NodeType.PatchSink,
                NodeId.SocHallSouthProjector => NodeType.VideoDestination,
                NodeId.ExtlSplitOutput1Patch => NodeType.PatchSource,
                NodeId.ExtlSplitOutput2Patch => NodeType.PatchSource,
                NodeId.ExtlSplitInputPatch => NodeType.PatchSink,
                NodeId.TeleprompterPatch => NodeType.PatchSink,
                NodeId.TeleprompterProjector => NodeType.VideoDestination,
                NodeId.Undefined => NodeType.Undefined,
                _ => NodeType.Undefined,
            };
        }

        /// <summary>
        /// Gets the full name of a given VideoNode based on its NodeId
        /// </summary>
        /// <param name="nodeId">The NodeId to look up</param>
        /// <returns>The full name of the VideoNode</returns>
        public static string Name(NodeId nodeId)
        {
            return nodeId switch
            {
                NodeId.Atem6Input => "ATEM 6 Input",
                NodeId.Atem6Patch => "ATEM 6 Patch",
                NodeId.AtemAuxOut => "ATEM Aux Out",
                NodeId.Cam3Patch => "Cam 3 Patch",
                NodeId.ExternalDevice => "External Device",
                NodeId.ExternalPseudoPatch => "External Psuedo Patch",
                NodeId.HdmiCable1 => "HDMI Cable 1",
                NodeId.HdmiCable2 => "HDMI Cable 2",
                NodeId.HdmiCable3 => "HDMI Cable 3",
                NodeId.HdmiCable4 => "HDMI Cable 4",
                NodeId.HdmiCable5 => "HDMI Cable 5",
                NodeId.HdmiCable6 => "HDMI Cable 6",
                NodeId.HdmiCable7 => "HDMI Cable 7",
                NodeId.HdmiCable8 => "HDMI Cable 8",
                NodeId.HdmiCable9 => "HDMI Cable 9",
                NodeId.HdmiCable10 => "HDMI Cable 10",
                NodeId.HdmiSplitInputPatch => "Split In Patch",
                NodeId.HdmiSplitOutput1Patch => "Split Out 1 Patch",
                NodeId.HdmiSplitOutput2Patch => "Split Out 2 Patch",
                NodeId.MxDest1Patch => "MX Dest 1 Patch",
                NodeId.MxDest2Patch => "MX Dest 2 Patch",
                NodeId.MxDest3Patch => "MX Dest 3 Patch",
                NodeId.MxDest4Patch => "MX Dest 4 Patch",
                NodeId.MxIn1 => "MX In 1",
                NodeId.MxIn2 => "MX In 2",
                NodeId.MxIn3 => "MX In 3",
                NodeId.MxIn4 => "MX In 4",
                NodeId.MxOut1 => "MX Out 1",
                NodeId.MxOut2 => "MX Out 2",
                NodeId.MxOut3 => "MX Out 3",
                NodeId.MxOut4 => "Mx Out 4",
                NodeId.MxPick1 => "MX Selection 1",
                NodeId.MxPick2 => "MX Selection 2",
                NodeId.MxPick3 => "MX Selection 3",
                NodeId.MxPick4 => "MX Selection 4",
                NodeId.MxSrc3Patch => "Mx Src 3 Patch",
                NodeId.MxSrc4Patch => "Mx Src 4 Patch",
                NodeId.Pc3Shinybow => "PC3 Shinybow",
                NodeId.PulpitVga => "Pulpit VGA",
                NodeId.PulpitVgaPatch => "Pulpit VGA Patch",
                NodeId.SanctuaryPatch => "Sanctuary Patch",
                NodeId.SanctuaryProjector => "Sanctuary Projector",
                NodeId.SocHallNorthPatch => "HH North Patch",
                NodeId.SocHallNorthProjector => "Soc Hall North Projector",
                NodeId.SocHallSouthPatch => "HH South Patch",
                NodeId.SocHallSouthProjector => "Soc Hall South Projector",
                NodeId.ExtlSplitOutput1Patch => "Ext Split Out 1",
                NodeId.ExtlSplitOutput2Patch => "Ext Split Out 2",
                NodeId.ExtlSplitInputPatch => "Ext Split Input",
                NodeId.TeleprompterPatch => "Teleprompter Patch",
                NodeId.TeleprompterProjector => "Teleprompter Projector",
                NodeId.Undefined => string.Empty,
                _ => string.Empty,
            };
        }

        /// <summary>
        /// Gets the short name of a given VideoNode based on its NodeId 
        /// </summary>
        /// <param name="nodeId">The NodeId to look up</param>
        /// <returns>The short name of the VideoNode</returns>
        public static string Nickname(NodeId nodeId)
        {
            return nodeId switch
            {
                NodeId.Atem6Input => "ATEM 6",
                NodeId.Atem6Patch => "ATEM 6",
                NodeId.AtemAuxOut => "ATEM Aux Out",
                NodeId.Cam3Patch => "Cam 3",
                NodeId.ExternalDevice => "External Device",
                NodeId.ExternalPseudoPatch => "External Cable",
                NodeId.HdmiCable1 => "HDMI 1",
                NodeId.HdmiCable2 => "HDMI 2",
                NodeId.HdmiCable3 => "HDMI 3",
                NodeId.HdmiCable4 => "HDMI 4",
                NodeId.HdmiCable5 => "HDMI 5",
                NodeId.HdmiCable6 => "HDMI 6",
                NodeId.HdmiCable7 => "HDMI 7",
                NodeId.HdmiCable8 => "HDMI 8",
                NodeId.HdmiCable9 => "HDMI 9",
                NodeId.HdmiCable10 => "HDMI 10",
                NodeId.HdmiSplitInputPatch => "Split IN",
                NodeId.HdmiSplitOutput1Patch => "Split Out 1",
                NodeId.HdmiSplitOutput2Patch => "Split Out 2",
                NodeId.MxDest1Patch => "MX Dst 1",
                NodeId.MxDest2Patch => "MX Dst 2",
                NodeId.MxDest3Patch => "MX Dst 3",
                NodeId.MxDest4Patch => "MX Dst 4",
                NodeId.MxIn1 => "1",
                NodeId.MxIn2 => "2",
                NodeId.MxIn3 => "3",
                NodeId.MxIn4 => "4",
                NodeId.MxOut1 => "1",
                NodeId.MxOut2 => "2",
                NodeId.MxOut3 => "3",
                NodeId.MxOut4 => "4",
                NodeId.MxPick1 => "1",
                NodeId.MxPick2 => "2",
                NodeId.MxPick3 => "3",
                NodeId.MxPick4 => "4",
                NodeId.MxSrc3Patch => "Mx Src 3",
                NodeId.MxSrc4Patch => "Mx Src 4",
                NodeId.Pc3Shinybow => "PC3 Shinybow",
                NodeId.PulpitVga => "Pulpit VGA",
                NodeId.PulpitVgaPatch => "Pulpit VGA",
                NodeId.SanctuaryPatch => "Sanctuary",
                NodeId.SanctuaryProjector => "Sanctuary",
                NodeId.SocHallNorthPatch => "HH North",
                NodeId.SocHallNorthProjector => "Soc Hall North",
                NodeId.SocHallSouthPatch => "HH South",
                NodeId.SocHallSouthProjector => "Soc Hall South",
                NodeId.ExtlSplitOutput1Patch => "External Split Out 1",
                NodeId.ExtlSplitOutput2Patch => "External Split Out 2",
                NodeId.ExtlSplitInputPatch => "External Split In",
                NodeId.TeleprompterPatch => "Teleprmpt",
                NodeId.TeleprompterProjector => "Teleprompter",
                NodeId.Undefined => String.Empty,
                _ => string.Empty,
            };
        }

        /// <summary>
        /// Whether or not a given VideoNode is enabled for connection and configuration.
        /// Nodes which are not enbled are still displayable but they cannot be connected.
        /// </summary>
        /// <param name="nodeId">The NodeId of the VideoNode</param>
        /// <returns>True if the node can be used, False if it cannot.</returns>
        public static bool Enabled(NodeId nodeId)
        {
            if (nodeId == NodeId.Cam3Patch)  // Use || nodeId == NodeId.OtherDisabledNodeKey for more
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the sequence in which a VideoNode should be displayed 
        /// </summary>
        /// <param name="nodeId">NodeId of the desired VideoNode</param>
        /// <returns>An integer indicating the relative sequence for displaying VideoNodes</returns>
        public static int DisplayOrder(NodeId nodeId)
        {
            return nodeId switch
            {
                NodeId.Atem6Input => 41,
                NodeId.Atem6Patch => 40,
                NodeId.AtemAuxOut => 2,
                NodeId.Cam3Patch => 13,
                NodeId.ExternalDevice => 5,
                NodeId.ExternalPseudoPatch => 36,
                NodeId.HdmiCable1 => 14,
                NodeId.HdmiCable2 => 15,
                NodeId.HdmiCable3 => 16,
                NodeId.HdmiCable4 => 17,
                NodeId.HdmiCable5 => 18,
                NodeId.HdmiCable6 => 19,
                NodeId.HdmiCable7 => 20,
                NodeId.HdmiCable8 => 21,
                NodeId.HdmiCable9 => 22,
                NodeId.HdmiCable10 => 23,
                NodeId.HdmiSplitInputPatch => 52,
                NodeId.HdmiSplitOutput1Patch => 11,
                NodeId.HdmiSplitOutput2Patch => 12,
                NodeId.MxDest1Patch => 6,
                NodeId.MxDest2Patch => 7,
                NodeId.MxDest3Patch => 8,
                NodeId.MxDest4Patch => 9,
                NodeId.MxIn1 => 24,
                NodeId.MxIn2 => 25,
                NodeId.MxIn3 => 26,
                NodeId.MxIn4 => 27,
                NodeId.MxOut1 => 32,
                NodeId.MxOut2 => 33,
                NodeId.MxOut3 => 34,
                NodeId.MxOut4 => 35,
                NodeId.MxPick1 => 28,
                NodeId.MxPick2 => 29,
                NodeId.MxPick3 => 30,
                NodeId.MxPick4 => 31,
                NodeId.MxSrc3Patch => 42,
                NodeId.MxSrc4Patch => 43,
                NodeId.Pc3Shinybow => 1,
                NodeId.PulpitVga => 3,
                NodeId.PulpitVgaPatch => 10,
                NodeId.SanctuaryPatch => 50,
                NodeId.SanctuaryProjector => 51,
                NodeId.SocHallNorthPatch => 44,
                NodeId.SocHallNorthProjector => 45,
                NodeId.SocHallSouthPatch => 46,
                NodeId.SocHallSouthProjector => 47,
                NodeId.ExtlSplitOutput1Patch => 37,
                NodeId.ExtlSplitOutput2Patch => 38,
                NodeId.ExtlSplitInputPatch => 39,
                NodeId.TeleprompterPatch => 48,
                NodeId.TeleprompterProjector => 49,
                NodeId.Undefined => 0,
                _ => 0,
            };
        }
    }
}
