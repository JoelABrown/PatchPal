using System.Collections.Generic;

namespace Mooseware.PatchPal
{
    /// <summary>
    /// A type of VideoNode that is a Video Matrix Input. This is distinct from other VideoNodes insofar as it can have multiple outputs.
    /// </summary>
    internal class MatrixInputNode : VideoNode
    {
        /// <summary>
        /// Always null. Use the list of Outputs when working with MatrixInputNode objects.
        /// </summary>
        public override VideoNode? Output
        {
            get { return null; }
        }

        /// <summary>
        /// The list of Video Nodes which use this object as their input.
        /// </summary>
        public List<VideoNode> Outputs { get; private set; }

        public MatrixInputNode(NodeId nodeId) : base(nodeId)
        {
            Outputs = new List<VideoNode>();
        }

        /// <summary>
        /// Adds an output to the list of outputs from this MatrixInputNode.
        /// </summary>
        /// <param name="outputVideoNode">The VideoNode to be added to the list of outputs</param>
        public void AddOutput(VideoNode outputVideoNode)
        {
            if (outputVideoNode != null && !Outputs.Contains(outputVideoNode))
            {
                Outputs.Add(outputVideoNode);
            }
        }

        /// <summary>
        /// Removes an output from the list
        /// </summary>
        /// <param name="outputVideoNode">The VideoNode to be removed</param>
        public void RemoveOutput(VideoNode outputVideoNode)
        {
            if (outputVideoNode != null && Outputs.Contains(outputVideoNode))
            {
                Outputs.Remove(outputVideoNode);
            }
        }

        /// <summary>
        /// Adds a single output to the list of outputs
        /// </summary>
        /// <param name="outputVideoNode"></param>
        public override void SetOutput(VideoNode outputVideoNode)
        {
            // Don't set the single output. Add it to the collection instead.
            if (outputVideoNode != null)
            {
                Outputs.Add(outputVideoNode);
            }
        }

        /// <summary>
        /// Removes a given output (implements the base method)
        /// </summary>
        public override void DisconnectOutput()
        {
            base.DisconnectOutput();
        }

        /// <summary>
        /// Removes a specified output (replaces the base method)
        /// </summary>
        /// <param name="input">The VideoNode to be removed from the list of outputs</param>
        public void DisconnectOutput(VideoNode input)
        {
            if (!Outputs.Contains(input))
            {
                Outputs.Remove(input);
            }
        }
    }
}
