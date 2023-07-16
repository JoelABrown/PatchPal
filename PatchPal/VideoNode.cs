namespace Mooseware.PatchPal
{
    /// <summary>
    /// A device which produces, transmits or consumes video as part of the video workflow.
    /// </summary>
    internal class VideoNode
    {
        /// <summary>
        /// The unique identifier of the VideoNode
        /// </summary>
        public NodeId Id { get; private set; }
        /// <summary>
        /// The kind of device or connection which this VideoNode represents
        /// </summary>
        public NodeType Type { get; private set; }
        /// <summary>
        /// The full descriptive name of the VideoNode
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The VideoNode which provides signal to this VideoNode (if known and applicable)
        /// </summary>
        public VideoNode? Input { get; private set; }
        /// <summary>
        /// The VideoNode to which this VideoNode provides signal (if known and applicable)
        /// </summary>
        public virtual VideoNode? Output { get; private set; }
        /// <summary>
        /// Whether or not this VideoNode is able to participate in the video workflow. Some VideoNodes are reserved for future use.
        /// </summary>
        public bool Enabled { get; private set; }
        /// <summary>
        /// The relative sequence of this VideoNode when being displayed together with other VideoNodes
        /// </summary>
        public int DisplayOrder { get; private set; }
        /// <summary>
        /// The short name of the VideoNode, for use in tight spaces in the UI.
        /// </summary>
        public string Nickname { get; private set; }

        public VideoNode(NodeId nodeId)
        {
            Id = nodeId;
            Type = Node.Type(nodeId);
            Name = Node.Name(nodeId);
            Enabled = Node.Enabled(nodeId);
            DisplayOrder = Node.DisplayOrder(nodeId);
            Nickname = Node.Nickname(nodeId);
        }

        /// <summary>
        /// Sets the Input of this VideoNode (and takes care coordinating the Output of the given Input so these remain in sync)
        /// </summary>
        /// <param name="inputVideoNode">The VideoNode which provides signal to this VideoNode</param>
        public void SetInput(VideoNode inputVideoNode)
        {
            Input = inputVideoNode;
            // Replace the current output of the new input.
            // This will keep both sides of the connection aligned.
            if (inputVideoNode != null && inputVideoNode.Output != this)
            {
                inputVideoNode.SetOutput(this);
            }
        }

        /// <summary>
        /// Sets the Output of this VideoNode (and takes care of coordinating the Input of the given Output so these remain in sync)
        /// </summary>
        /// <param name="outputVideoNode"></param>
        public virtual void SetOutput(VideoNode outputVideoNode)
        {
            Output = outputVideoNode;
            // Replace the current input of the new output.
            // This will keep both sides of the connection aligned.
            if (outputVideoNode !=null && outputVideoNode.Input != this)
            {
                outputVideoNode.SetInput(this);
            }    
        }

        /// <summary>
        /// Removes the current Input, if any (and takes care coordinating the Output of the given Input so these remain in sync).
        /// </summary>
        public void DisconnectInput()
        {
            if (Input != null)
            {
                if (Input.Type == NodeType.MxInput)
                {
                    ((MatrixInputNode)Input).DisconnectOutput(this);
                }
                else
                {
                    Input.DisconnectOutput();
                }
            }
            Input = null;
        }

        /// <summary>
        /// Removes the current Output, if any.
        /// </summary>
        public virtual void DisconnectOutput()
        {
            Output = null;
        }

        /// <summary>
        /// Finds the VideoNode which is the furthest upstream of this VideoNode. This is the original source of video signal arriving at this VideoNode.
        /// </summary>
        public VideoNode? Upstream
        {
            get
            {
                VideoNode? nextParent = this.Input;
                while (nextParent?.Input != null)
                {
                    nextParent = nextParent.Input;
                }
                return nextParent;
            }
        }
    }
}
