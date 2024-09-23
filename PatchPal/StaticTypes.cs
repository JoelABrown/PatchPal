namespace Mooseware.PatchPal
{
    /// <summary>
    /// The kind of connection represented by a VideoNode
    /// </summary>
    internal enum NodeType
    {
        /// <summary>
        /// An HDMI cable used in the video patch panel
        /// </summary>
        HdmiCable,
        /// <summary>
        /// An input to the video matrix switch
        /// </summary>
        MxInput,
        /// <summary>
        /// An output from the video matrix switch
        /// </summary>
        MxOutput,
        /// <summary>
        /// The selection of a route from a source to a destination on the video matrix switch
        /// </summary>
        MxSelection,
        /// <summary>
        /// An HDMI connection on the patch panel connected to something that receives video
        /// </summary>
        PatchSink,
        /// <summary>
        /// An HDMI connection on the patch panel connect to something which produces video
        /// </summary>
        PatchSource,
        /// <summary>
        /// A device which recieves video (e.g. a projector)
        /// </summary>
        VideoDestination,
        /// <summary>
        /// A device which produces video (e.g. a Blu-ray player)
        /// </summary>
        VideoSource,
        /// <summary>
        /// An unknown device. Use when the specific device is not (yet) known.
        /// </summary>
        Undefined
    }

    /// <summary>
    /// The identifier of a specific VideoNode in the video workflow
    /// NOTE: When adding new nodes to this Enum, also make corresponding
    ///       changes to both the Node class in StaticClasses.cs and to
    ///       the appropriate .json files (HardwiredConfiguration.json
    ///       and PatchConfiguration.json)
    /// </summary>
    internal enum NodeId
    {
        Undefined,
        Atem6Input,
        Atem6Patch,
        AtemAuxOut,
        Bluray,
        BlurayPatch,
        Cam3Patch,
        ExternalDevice,
        ExternalPseudoPatch,
        FutureSinkPatch,
        FutureSourcePatch,
        HdmiCable1,
        HdmiCable2,
        HdmiCable3,
        HdmiCable4,
        HdmiCable5,
        HdmiCable6,
        HdmiCable7,
        HdmiCable8,
        HdmiCable9,
        HdmiCable10,
        MxDest1Patch,
        MxDest2Patch,
        MxDest3Patch,
        MxDest4Patch,
        MxIn1,
        MxIn2,
        MxIn3,
        MxIn4,
        MxOut1,
        MxOut2,
        MxOut3,
        MxOut4,
        MxPick1,
        MxPick2,
        MxPick3,
        MxPick4,
        MxSrc3Patch,
        MxSrc4Patch,
        Pc3Shinybow,
        PulpitVga,
        PulpitVgaPatch,
        SanctuaryPatch,
        SanctuaryProjector,
        SocHallNorthPatch,
        SocHallNorthProjector,
        SocHallSouthPatch,
        SocHallSouthProjector,
        SplitterInputPatch,
        SplitterOutput1Patch,
        SplitterOutput2Patch,
        TeleprompterPatch,
        TeleprompterProjector
    }
}