using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Mooseware.ClipRunner.AtemApi;

namespace Mooseware.PatchPal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The list of VideoNode objects which cannot be changed by the user at runtime. These are physically connected at build time.
        /// </summary>
        readonly Dictionary<NodeId, VideoNode> hardwiredNodes = new();
        /// <summary>
        /// The list of VideoNode objects which can be configured by the user at runtime. Inclues HDMI patch cables and video matrix selections.
        /// </summary>
        readonly Dictionary<NodeId, VideoNode> configuredNodes = new();

        /// <summary>
        /// The NodeId of the currently selected patch panel source VideoNode, if any.
        /// </summary>
        private NodeId _selectedSourcePatch = NodeId.Undefined;
        /// <summary>
        /// The NodeId of the currently selected patch panel video sink VideoNode, if any.
        /// </summary>
        private NodeId _selectedSinkPatch = NodeId.Undefined;
        /// <summary>
        /// The NodeId of the currently selected HDMI patch VideoNode, if any.
        /// </summary>
        private NodeId _selectedHdmiPatch = NodeId.Undefined;
        /// <summary>
        /// The NodeId of the currently selected VideoDestination type VideoNode, if any.
        /// </summary>
        private NodeId _selectedSinkMatrix = NodeId.Undefined;

        /// <summary>
        /// Canvas object tag used in the names of Grid objects which contain the representation of VideoNodes using a shape.
        /// </summary>
        private const string NodeContainerTag = "Container";
        /// <summary>
        /// Canvas object tag used in the names of the Rectangle objects used to represent a VideoNode using a shape.
        /// </summary>
        private const string NodeOutlineTag = "Box";
        /// <summary>
        /// Canvas object tag used in the names of the TextBlock objects used to represent a VideoNode using a shape.
        /// </summary>
        private const string NodeLabelTag = "Label";
        /// <summary>
        /// Canvas object tag used in the names of the Path objects used to represent a VideoNode using a path.
        /// </summary>
        private const string HdmiPatchTag = "Wire";
        /// <summary>
        /// Canvas object tag used in the names of the Line objects used to represent a VideoNode using a line.
        /// </summary>
        private const string MxSelectTag = "Selection";

        /// <summary>
        /// Thickness for Matrix Canvas items which are not currently selected.
        /// </summary>
        private const double MatrixUnselectedStrokeThickness = 2.0;
        /// <summary>
        /// Thickness for Matrix Canvas items which are currently selected.
        /// </summary>
        private const double MatrixSelectedStrokeThickness = 4.0;
        /// <summary>
        /// Thickness for Patch Canvas items which are not currently selected.
        /// </summary>
        private const double PatchUnselectedStrokeThickness = 2.0;
        /// <summary>
        /// Thickness for Patch Canvas items which are currently selected.
        /// </summary>
        private const double PatchSelectedStrokeThickness = 4.0;
        /// <summary>
        /// Thickness for Patch Canvas items, specifically HDMI patch paths, which are not currently selected.
        /// </summary>
        private const double HdmiUnselectedStrokeThickness = 3.0;
        /// <summary>
        /// Thickness for Patch Canvas items, specifically HDMI patch paths, which are currently selected.
        /// </summary>
        private const double HdmiSelectedStrokeThickness = 5.0;
        /// <summary>
        /// Thickness for Matrix Canvas items, specifically the MatrixSelection line paths.
        /// </summary>
        private const double MxPickUnselectedStrokeThickness = 2.5;

        /// <summary>
        /// Brush for items which are not selected.
        /// </summary>
        private readonly SolidColorBrush UnselectedPatchBrush = Brushes.Black;
        /// <summary>
        /// Brush for items which are selected.
        /// </summary>
        private readonly SolidColorBrush SelectedPatchBrush = Brushes.Blue;
        /// <summary>
        /// Brush for the background of the Matrix Selection Summary when the connection is complete
        /// </summary>
        private readonly SolidColorBrush ActiveMatrixConnection = Brushes.White;
        /// <summary>
        /// Brush for the background of the Matrix Selection Summary when the selection is meaningless because of an incomplete end to end connection
        /// </summary>
        private readonly SolidColorBrush InactiveMatrixConnection = new(Color.FromRgb(0xe0, 0xe0, 0xe0));

        // TODO: Add the ATEM object back in once I figure out how to use it.
        // TODO: Consider doing direct manipulation of the BMD API like TimeToAir instead.
        ///// <summary>
        ///// The ATEM Switcher controller which wraps the necessary parts of the ATEM API
        ///// </summary>
        //private Switcher? _atemSwitcher;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadNodeConfiguration();
            DrawMatrixCanvas();
            SetPatchCmdButtonStatus();

            // TODO: Figure out how to get the current Aux Out selection info, if it's available.
            //_atemSwitcher = new Switcher(atemIpAddress: "192.168.0.240");
            //if (_atemSwitcher.IsReady)
            //{
            //    var inputs = _atemSwitcher.ListSwitcherInputs;
            //    int gar = 0;
            //}
        }

        /// <summary>
        /// Draws the contents of the Patch Canvas based on the current configuration
        /// </summary>
        private void DrawPatchCanvas()
        {
            PatchCanvas.Children.Clear();

            List<VideoNode> sourceNodes = hardwiredNodes.Values
                .Where(s => s.Type == NodeType.PatchSource)
                .OrderBy(s => s.DisplayOrder)
                .ToList();

            List<VideoNode> sinkNodes = hardwiredNodes.Values
                .Where(s => s.Type == NodeType.PatchSink)
                .OrderBy(s => s.DisplayOrder)
                .ToList();

            // How big are the patch node squares?
            // Make enough room for double the number of nodes which are in the source patch list
            // But also allow for margins of 10% on either side and in between
            double patchNodeSize = PatchCanvas.ActualWidth / (((sourceNodes.Count + sinkNodes.Count) * 1.1) + 0.1);
            double patchMargin = patchNodeSize * 0.1;
            double patchRowTop = patchMargin * 3;           // For now use as a simplifying assumption.
            double patchLabelFontSize = patchMargin * 2;    // For now use as a simplifying assumption.
            double cableOffset = patchMargin * 5;           // For now use as a simplifying assumption.
            double patchRadius = patchMargin * 2;           // For now use as a simplyfying assumption.
            double labelHeaderFontSize = patchNodeSize / 2; // For now, etc.
            double labelFooterFontSize = labelHeaderFontSize * 0.66;    // Arbitrary

            // TODO: Figure out how to avoid bailing due to zero width on the patch tab.
            if (patchLabelFontSize == 0)
            {
                return;
            }

            // Display the patch connections (sources then sinks)...
            int column = 0;
            foreach (var patchNode in sourceNodes)
            {
                column++;
                Grid patchContainer = BuildPatchNode(patchNode, patchNodeSize, patchLabelFontSize);
                Canvas.SetTop(patchContainer, patchRowTop);
                Canvas.SetLeft(patchContainer, ((column - 1) * (patchNodeSize + patchMargin)) + patchMargin);
                PatchCanvas.Children.Add(patchContainer);
            }
            foreach (var patchNode in sinkNodes)
            {
                column++;
                Grid patchContainer = BuildPatchNode(patchNode, patchNodeSize, patchLabelFontSize);
                Canvas.SetTop(patchContainer, patchRowTop);
                Canvas.SetLeft(patchContainer, ((column - 1) * (patchNodeSize + patchMargin)) + patchMargin);
                PatchCanvas.Children.Add(patchContainer);
            }

            // Display the patch HDMI cables...
            // Only show the cables that are connected.
            // Sort them in the order of their source connectors.
            List<VideoNode> hdmiNodes = configuredNodes.Values
                .Where(s => s.Type == NodeType.HdmiCable && s.Input != null && s.Output != null)
                .OrderBy(s => s.Input?.DisplayOrder)
                .ToList();

            int cable = hdmiNodes.Count;

            foreach (var hdmi in hdmiNodes)
            {
                Path hdmiPath = new()
                {
                    Stroke = Brushes.Black,
                    StrokeThickness = HdmiUnselectedStrokeThickness,
                    Fill = null,  // Brushes.Transparent;
                    Name = hdmi.Id.ToString() + HdmiPatchTag
                };

                // Where are we going from and to?
                Point startingPoint = GetPatchNodeCnxPoint(hdmi.Input?.Id);
                Point endingPoint = GetPatchNodeCnxPoint(hdmi.Output?.Id);

                PathFigure figure = new()
                {
                    StartPoint = new Point(0.0, 0.0),
                    IsClosed = false,
                    IsFilled = false
                };
                figure.Segments.Clear();
                figure.Segments.Add(new LineSegment(new Point(0.0, (cable * cableOffset) - patchRadius), true));
                figure.Segments.Add(new ArcSegment(new Point(patchRadius, (cable * cableOffset)),
                    new Size(patchRadius, patchRadius), 90.0, false, SweepDirection.Counterclockwise, true));
                figure.Segments.Add(new LineSegment(new Point(endingPoint.X - startingPoint.X - patchRadius, (cable * cableOffset)), true));
                figure.Segments.Add(new ArcSegment(new Point(endingPoint.X - startingPoint.X, (cable * cableOffset) - patchRadius),
                    new Size(patchRadius, patchRadius), 90.0, false, SweepDirection.Counterclockwise, true));
                figure.Segments.Add(new LineSegment(new Point(endingPoint.X - startingPoint.X, 0.0), true));
                PathFigureCollection figures = new()
                {
                    figure
                };
                PathGeometry geo = new()
                {
                    Figures = figures
                };
                hdmiPath.Data = geo;

                Canvas.SetTop(hdmiPath, startingPoint.Y);
                Canvas.SetLeft(hdmiPath, startingPoint.X);
                PatchCanvas.Children.Add(hdmiPath);

                cable--;
            }

            SetPatchCmdButtonStatus();
        }

        /// <summary>
        /// Gets the coordinates of the connection point for a given VideoNode
        /// </summary>
        /// <param name="nodeId">NodeId of the VideoNode whose connection point is to be found.</param>
        /// <returns>A Point containing the X,Y coordinates where a line or path should connect to the VideoNode</returns>
        private Point GetPatchNodeCnxPoint(NodeId? nodeId)
        {
            Point result = new();

            // Find the patch container and get the point which is the center of it's bottom line...
            foreach (var item in PatchCanvas.Children)
            {
                if (item.GetType() == typeof(Grid))
                {
                    Grid hit = (Grid)item;
                    if (hit.Name == nodeId.ToString() + "Container")
                    {
                        // This is the one. What is the middle point of the bottom...
                        double bottom = (double)hit.GetValue(Canvas.TopProperty)
                            + (double)hit.GetValue(Canvas.HeightProperty);
                        double halfWayOver = (double)hit.GetValue(Canvas.LeftProperty)
                            + ((double)hit.GetValue(Canvas.WidthProperty) / 2);

                        result.X = halfWayOver;
                        result.Y = bottom;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Constructs a Canvas object (Grid) containing the visual elements required to display a representation of a patch panel connection VideoNode.
        /// </summary>
        /// <param name="patchNode">The VideoNode to be represented visually</param>
        /// <param name="patchNodeSize">The size, on the Canvas, which the VideoNode should take up (the result will be a square of this size)</param>
        /// <param name="patchLabelFontSize">The font size to use for the TextBlock which labels the VideoNode</param>
        /// <returns>A Grid containing a Rectangle and a TextBlock</returns>
        private static Grid BuildPatchNode(VideoNode patchNode, double patchNodeSize, double patchLabelFontSize)
        {
            Grid patchContainer = new()
            {
                Width = patchNodeSize,
                Height = patchNodeSize,
                Name = patchNode.Id.ToString() + NodeContainerTag,
                VerticalAlignment = VerticalAlignment.Center
            };

            Rectangle patchBox = new()
            {
                Width = patchNodeSize,
                Height = patchNodeSize,
                IsHitTestVisible = true
            };
            // Colour scheme depends on type and state of the patch...
            if (patchNode.Id == NodeId.ExternalPseudoPatch
             || patchNode.Id == NodeId.ExtlSplitInputPatch
             || patchNode.Id == NodeId.ExtlSplitOutput1Patch
             || patchNode.Id == NodeId.ExtlSplitOutput2Patch)
            {
                patchBox.Fill = Brushes.Gray;
                patchBox.Stroke = Brushes.Black;
            }
            else if (patchNode.Type == NodeType.PatchSource)
            {
                if (patchNode.Enabled)
                {
                    patchBox.Fill = Brushes.Yellow;
                    patchBox.Stroke = Brushes.Black;
                }
                else
                {
                    patchBox.Fill = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xFF, 0x00));   // Yellow with alpha
                    patchBox.Stroke = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                }
            }
            else if (patchNode.Type == NodeType.PatchSink)
            {
                if (patchNode.Enabled)
                {
                    patchBox.Fill = Brushes.Orange;
                    patchBox.Stroke = Brushes.Black;
                }
                else
                {
                    patchBox.Fill = new SolidColorBrush(Color.FromArgb(0x99, 0xFF, 0xA5, 0x00));   // Orange with alpha
                    patchBox.Stroke = Brushes.DimGray;  // new SolidColorBrush(Color.FromRgb(80, 80, 80));
                }
            }
            patchBox.StrokeThickness = PatchUnselectedStrokeThickness;
            patchBox.Name = patchNode.Id.ToString() + NodeOutlineTag;

            TextBlock patchText = new()
            {
                Width = patchNodeSize,
                Height = patchNodeSize,
                Text = patchNode.Nickname,
                FontSize = patchLabelFontSize,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Padding = new Thickness(2, patchLabelFontSize, 2, 0),  // Always allow just a hint of whitespace left and right of the text.
                IsHitTestVisible = false,
                Name = patchNode.Id.ToString() + NodeLabelTag
            };
            if (patchNode.Id == NodeId.ExternalPseudoPatch
             || patchNode.Id == NodeId.ExtlSplitInputPatch
             || patchNode.Id == NodeId.ExtlSplitOutput1Patch
             || patchNode.Id == NodeId.ExtlSplitOutput2Patch)
            {
                patchText.Foreground = Brushes.White;
            }
            else if (!patchNode.Enabled)
            {
                patchText.Foreground = Brushes.DimGray;
            }
            else
            {
                patchText.Foreground = Brushes.Black;
            }

            patchContainer.Children.Add(patchBox);
            patchContainer.Children.Add(patchText);

            return patchContainer;
        }

        /// <summary>
        /// Retrieves the persisted configuration of VideoNode objects (both hardwired and user-configurable)
        /// </summary>
        private void LoadNodeConfiguration()
        {
            // Get this from configuration files...
            configuredNodes.Clear();
            hardwiredNodes.Clear();

            // NOTE: Loading Hardwired Nodes has to be done in two steps because of logical build sequence.
            //       1. Load all of the nodes according to their types
            //       2. Review each node and wire up its input, if it has one. (Each node can have at most 1 input.)
            HardwiredConfiguration.Load();
            if (HardwiredConfiguration.Settings != null)
            {
                foreach (var item in HardwiredConfiguration.Settings.HardwiredItems)
                {
                    if (Enum.TryParse<NodeType>(item.NodeType, out var nodeType))
                    {
                        if (Enum.TryParse<NodeId>(item.NodeId, out var thisNodeId))
                        {
                            if (nodeType == NodeType.MxInput)
                            {
                                MatrixInputNode mxNode = new(thisNodeId);
                                hardwiredNodes.Add(mxNode.Id, mxNode);
                            }
                            else
                            {
                                VideoNode videoNode = new(thisNodeId);
                                hardwiredNodes.Add(videoNode.Id, videoNode);
                            }
                        }
                    }
                }
                // Now wire up the inputs, when they have them...
                foreach (var item in HardwiredConfiguration.Settings.HardwiredItems)
                {
                    if (Enum.TryParse<NodeId>(item.NodeId, out var thisNodeId))
                    {
                        if (Enum.TryParse<NodeId>(item.Input, out var inputNodeId))
                        {
                            hardwiredNodes[thisNodeId].SetInput(hardwiredNodes[inputNodeId]);
                        }
                    }
                }
            }

            // Now load the configurable items...
            PatchConfiguration.Load();
            if (PatchConfiguration.Settings != null)
            {
                foreach (var item in PatchConfiguration.Settings.PatchItems)
                {
                    if (!Enum.TryParse<NodeId>(item.Input, out var inputNodeId))
                    {
                        inputNodeId = NodeId.Undefined;
                    }
                    if (!Enum.TryParse<NodeId>(item.Output, out var outputNodeId))
                    {
                        outputNodeId = NodeId.Undefined;
                    }
                    if (Enum.TryParse<NodeId>(item.NodeId, out var thisNodeId))
                    {
                        VideoNode videoNode = new(thisNodeId);
                        videoNode.SetInput(hardwiredNodes[inputNodeId]);
                        videoNode.SetOutput(hardwiredNodes[outputNodeId]);
                        configuredNodes.Add(videoNode.Id, videoNode);
                    }
                }
            }

            #region Hard-coded configuration statements (for development purposes only)
            //VideoNode node = new(NodeId.AtemAuxOut);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.Pc3Shinybow);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.Bluray);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.PulpitVga);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.ExternalDevice);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.BlurayPatch);
            //node.SetInput(hardwiredNodes[NodeId.Bluray]);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.PulpitVgaPatch);
            //node.SetInput(hardwiredNodes[NodeId.PulpitVga]);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.ExternalPseudoPatch);
            //node.SetInput(hardwiredNodes[NodeId.ExternalDevice]);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.MxSrc3Patch);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.MxSrc4Patch);
            //hardwiredNodes.Add(node.Id, node);

            //MatrixInputNode mxNode = new(NodeId.MxIn1);
            //mxNode.SetInput(hardwiredNodes[NodeId.Pc3Shinybow]);
            //hardwiredNodes.Add(mxNode.Id, mxNode);

            //mxNode = new(NodeId.MxIn2);
            //mxNode.SetInput(hardwiredNodes[NodeId.AtemAuxOut]);
            //hardwiredNodes.Add(mxNode.Id, mxNode);

            //mxNode = new(NodeId.MxIn3);
            //mxNode.SetInput(hardwiredNodes[NodeId.MxSrc3Patch]);
            //hardwiredNodes.Add(mxNode.Id, mxNode);

            //mxNode = new(NodeId.MxIn4);
            //mxNode.SetInput(hardwiredNodes[NodeId.MxSrc4Patch]);
            //hardwiredNodes.Add(mxNode.Id, mxNode);

            //node = new(NodeId.MxOut1);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.MxOut2);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.MxOut3);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.MxOut4);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.MxDest1Patch);
            //node.SetInput(hardwiredNodes[NodeId.MxOut1]);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.MxDest2Patch);
            //node.SetInput(hardwiredNodes[NodeId.MxOut2]);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.MxDest3Patch);
            //node.SetInput(hardwiredNodes[NodeId.MxOut3]);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.MxDest4Patch);
            //node.SetInput(hardwiredNodes[NodeId.MxOut4]);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.Atem6Patch);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.SanctuaryPatch);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.SocHallNorthPatch);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.SocHallSouthPatch);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.TeleprompterPatch);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.Atem6Input);
            //node.SetInput(hardwiredNodes[NodeId.Atem6Patch]);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.SanctuaryProjector);
            //node.SetInput(hardwiredNodes[NodeId.SanctuaryPatch]);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.SocHallNorthProjector);
            //node.SetInput(hardwiredNodes[NodeId.SocHallNorthPatch]);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.SocHallSouthProjector);
            //node.SetInput(hardwiredNodes[NodeId.SocHallSouthPatch]);
            //hardwiredNodes.Add(node.Id, node);

            //node = new(NodeId.TeleprompterProjector);
            //node.SetInput(hardwiredNodes[NodeId.TeleprompterPatch]);
            //hardwiredNodes.Add(node.Id, node);

            //// Future use patch panel nodes which are not connected at all...
            //node = new(NodeId.Cam3Patch);
            //hardwiredNodes.Add(node.Id, node);
            //node = new(NodeId.FutureSinkPatch);
            //hardwiredNodes.Add(node.Id, node);
            //node = new(NodeId.FutureSourcePatch);
            //hardwiredNodes.Add(node.Id, node);

            // Use this code to create a new HardwiredConfiguration.json file out of the 
            // currently loaded hardwiredNodes collection.  This is a design-time activity only.
            //foreach (var hardwiredNode in hardwiredNodes)
            //{
            //    HardwiredItem hardwiredItem = new()
            //    {
            //        NodeId = hardwiredNode.Value.Id.ToString(),
            //        NodeType = hardwiredNode.Value.Type.ToString(),
            //        Input = hardwiredNode.Value.Input?.Id.ToString() ?? String.Empty
            //    };
            //    ////hardwiredConfiguration.HardwiredItems.Add(hardwiredItem);
            //    HardwiredConfiguration.Settings?.HardwiredItems.Add(hardwiredItem);
            //}
            //HardwiredConfiguration.Save();


            // Configured nodes...
            //VideoNode node;

            //node = new(NodeId.HdmiCable1);
            //node.SetInput(hardwiredNodes[NodeId.PulpitVgaPatch]);
            //node.SetOutput(hardwiredNodes[NodeId.MxSrc4Patch]);
            //configuredNodes.Add(node.Id, node);

            //node = new(NodeId.HdmiCable2);
            //node.SetInput(hardwiredNodes[NodeId.ExternalPseudoPatch]);
            //node.SetOutput(hardwiredNodes[NodeId.MxSrc3Patch]);
            //configuredNodes.Add(node.Id, node);

            //node = new(NodeId.HdmiCable3);
            //node.SetInput(hardwiredNodes[NodeId.BlurayPatch]);
            //node.SetOutput(hardwiredNodes[NodeId.SocHallSouthPatch]);
            //configuredNodes.Add(node.Id, node);

            //node = new(NodeId.HdmiCable4);
            //node.SetInput(hardwiredNodes[NodeId.MxDest1Patch]);
            //node.SetOutput(hardwiredNodes[NodeId.SanctuaryPatch]);
            //configuredNodes.Add(node.Id, node);

            //node = new(NodeId.HdmiCable5);
            //node.SetInput(hardwiredNodes[NodeId.MxDest2Patch]);
            //node.SetOutput(hardwiredNodes[NodeId.TeleprompterPatch]);
            //configuredNodes.Add(node.Id, node);

            //node = new(NodeId.HdmiCable6);
            //node.SetInput(hardwiredNodes[NodeId.MxDest3Patch]);
            //node.SetOutput(hardwiredNodes[NodeId.SocHallNorthPatch]);
            //configuredNodes.Add(node.Id, node);

            //node = new(NodeId.HdmiCable7);
            //node.SetInput(hardwiredNodes[NodeId.MxDest4Patch]);
            //node.SetOutput(hardwiredNodes[NodeId.Atem6Patch]);
            //configuredNodes.Add(node.Id, node);

            //// Configurable MX selection nodes...
            //// -------------------------------
            //node = new(NodeId.MxPick1);
            //node.SetInput(hardwiredNodes[NodeId.MxIn1]);
            //node.SetOutput(hardwiredNodes[NodeId.MxOut1]);
            //configuredNodes.Add(node.Id, node);

            //node = new(NodeId.MxPick2);
            //node.SetInput(hardwiredNodes[NodeId.MxIn2]);
            //node.SetOutput(hardwiredNodes[NodeId.MxOut2]);
            //configuredNodes.Add(node.Id, node);

            //node = new(NodeId.MxPick3);
            //node.SetInput(hardwiredNodes[NodeId.MxIn1]);
            //node.SetOutput(hardwiredNodes[NodeId.MxOut3]);
            //configuredNodes.Add(node.Id, node);

            //node = new(NodeId.MxPick4);
            //node.SetInput(hardwiredNodes[NodeId.MxIn1]);
            //node.SetOutput(hardwiredNodes[NodeId.MxOut4]);
            //configuredNodes.Add(node.Id, node);

            //PatchConfiguration.Load();
            //foreach (var patchNode in configuredNodes)
            //{
            //    ConfigurableItem patchItem = new()
            //    {
            //        NodeId = patchNode.Value.Id.ToString(),
            //        NodeType = patchNode.Value.Type.ToString(),
            //        Input = patchNode.Value.Input?.Id.ToString() ?? String.Empty,
            //        Output = patchNode.Value.Output?.Id.ToString() ?? String.Empty
            //    };
            //    PatchConfiguration.Settings?.PatchItems.Add(patchItem);
            //}
            //PatchConfiguration.Save();


            #endregion
        }

        private void TabList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Reset any node selections if the tab is changing.
            _selectedSourcePatch = NodeId.Undefined;
            _selectedHdmiPatch = NodeId.Undefined;
            _selectedSinkPatch = NodeId.Undefined;

            _selectedSinkMatrix = NodeId.Undefined;

            if (e.Source == TabList && MatrixTabItem.IsSelected)
            {
                DrawMatrixCanvas();
            }
            else if (e.Source == TabList && PatchTabItem.IsSelected)
            {
                DrawPatchCanvas();
            }
        }

        private void PatchTabItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // TODO: Figure out if this is the right place to draw the patch tab contents.
            if (PatchTabItem.IsSelected)
            {
                DrawPatchCanvas();
            }
        }

        private void PatchCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Figure out what on the Patch Canvas was clicked and then set the selection accordingly.
            // Where was the click? Was it on a patch node or an HDMI cable node?
            if (e.Source.GetType() == typeof(Rectangle))
            {
                Rectangle clickedRectangle = (Rectangle)(e.Source);
                if (Enum.TryParse<NodeId>(clickedRectangle.Name.Replace(NodeOutlineTag, string.Empty), out NodeId clickedNodeId))
                {
                    if (hardwiredNodes.ContainsKey(clickedNodeId))
                    {
                        ShowPatchSelection(clickedNodeId);
                    }
                }
            }
            else if (e.Source.GetType() == typeof(Path))
            {
                Path clickedPath = (Path)(e.Source);
                if (Enum.TryParse<NodeId>(clickedPath.Name.Replace(HdmiPatchTag, string.Empty), out NodeId clickedNodeId))
                {
                    if (configuredNodes.ContainsKey(clickedNodeId) && configuredNodes[clickedNodeId].Type == NodeType.HdmiCable)
                    {
                        ShowPatchSelection(clickedNodeId);
                    }
                }
            }
            else
            {
                // No selection...
                ShowPatchSelection(NodeId.Undefined);
            }

            SetPatchCmdButtonStatus();
        }

        /// <summary>
        /// Change the visual representation of elements on the PatchCanvas 
        /// depending on what is already selected and what has been clicked on.
        /// </summary>
        /// <param name="selectedNodeId">Identifier of the item which has just been clicked.</param>
        private void ShowPatchSelection(NodeId selectedNodeId)
        {
            VideoNode? selectedNode = null;
            // What has been selected now?
            if (hardwiredNodes.ContainsKey(selectedNodeId))
            {
                selectedNode = hardwiredNodes[selectedNodeId];
            }
            else if (configuredNodes.ContainsKey(selectedNodeId))
            {
                selectedNode = configuredNodes[selectedNodeId];
            }
            // Otherwise nothing was selected.

            // If there is a new selection, does it imply a connected chain or just a single node?
            bool chainSelection = false;
            if (selectedNode != null)
            {
                switch (selectedNode.Type)
                {
                    case NodeType.HdmiCable:
                        chainSelection = selectedNode.Input != null && selectedNode.Output != null;
                        break;
                    case NodeType.PatchSink:
                        chainSelection = selectedNode.Input != null && selectedNode.Input.Input != null;
                        break;
                    case NodeType.PatchSource:
                        chainSelection = selectedNode.Output != null && selectedNode.Output.Output != null;
                        break;
                    default:
                        // Don't care in any other case.
                        break;
                }
            }

            // Now that we know if we're selecting just one node or a chain of nodes,
            // set the selection visual cues, remembering to reset any prior visual selection first.

            // Resetting current visual selection, if any...
            if (_selectedHdmiPatch != NodeId.Undefined)
            {
                Path? hdmi = FindHdmiPatchPathById(PatchCanvas, _selectedHdmiPatch);
                if (hdmi != null)
                {
                    hdmi.Stroke = UnselectedPatchBrush;
                    hdmi.StrokeThickness = HdmiUnselectedStrokeThickness;
                }
            }
            if (_selectedSourcePatch != NodeId.Undefined)
            {
                Rectangle? rectangle = FindGridRectangleByPatchId(PatchCanvas, _selectedSourcePatch);
                if (rectangle != null)
                {
                    rectangle.Stroke = UnselectedPatchBrush;
                    rectangle.StrokeThickness = PatchUnselectedStrokeThickness;
                }
            }
            if (_selectedSinkPatch != NodeId.Undefined)
            {
                Rectangle? rectangle = FindGridRectangleByPatchId(PatchCanvas, _selectedSinkPatch);
                if (rectangle != null)
                {
                    rectangle.Stroke = UnselectedPatchBrush;
                    rectangle.StrokeThickness = PatchUnselectedStrokeThickness;
                }
            }

            // Was the old selection a chain (from source through hdmi to sink)
            // This is important because it impacts what to reset as far as visuals...
            if ((_selectedSourcePatch != NodeId.Undefined
                && _selectedHdmiPatch != NodeId.Undefined
                && _selectedSinkPatch != NodeId.Undefined)
                || (selectedNode != null && selectedNode.Enabled == false))
            {
                // All of these should be reset based on whatever is being selected now.
                _selectedSourcePatch = NodeId.Undefined;
                _selectedHdmiPatch = NodeId.Undefined;
                _selectedSinkPatch = NodeId.Undefined;
            }

            // Now set the cues for the new selection, as appropriate...
            if (selectedNode != null)
            {
                // Never show a disabled patch as selected.
                if (selectedNode.Enabled == false)
                {
                    return;
                }

                // Get all of the affected pieces...
                NodeId affectedSourcePatch = NodeId.Undefined;
                NodeId affectedHdmiPatch = NodeId.Undefined;
                NodeId affectedSinkPatch = NodeId.Undefined;

                if (selectedNode.Type == NodeType.PatchSource)
                {
                    affectedSourcePatch = selectedNode.Id;
                    if (chainSelection)
                    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        affectedHdmiPatch = selectedNode.Output.Id;
                        affectedSinkPatch = selectedNode.Output.Output.Id;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    }
                    else
                    {
                        // Preserve the selected Sink Patch if this is not a whole chain.
                        affectedSinkPatch = _selectedSinkPatch;
                    }
                }
                if (selectedNode.Type == NodeType.HdmiCable)
                {
                    affectedHdmiPatch = selectedNode.Id;
                    if (chainSelection)
                    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        affectedSourcePatch = selectedNode.Input.Id;
                        affectedSinkPatch = selectedNode.Output.Id;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    }
                }
                if (selectedNode.Type == NodeType.PatchSink)
                {
                    affectedSinkPatch = selectedNodeId;
                    if (chainSelection)
                    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                        affectedHdmiPatch = selectedNode.Input.Id;
                        affectedSourcePatch = selectedNode.Input.Input.Id;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                    }
                    else
                    {
                        // Preserve the selected Source Patch if this is not a whole chain.
                        affectedSourcePatch = _selectedSourcePatch;
                    }
                }

                // Set the visual cues for the new selected item(s)
                if (affectedSourcePatch != NodeId.Undefined)
                {
                    Rectangle? rectangle = FindGridRectangleByPatchId(PatchCanvas, affectedSourcePatch);
                    if (rectangle != null)
                    {
                        rectangle.Stroke = SelectedPatchBrush;
                        rectangle.StrokeThickness = PatchSelectedStrokeThickness;
                    }
                }
                _selectedSourcePatch = affectedSourcePatch;
                if (affectedHdmiPatch != NodeId.Undefined)
                {
                    Path? Path = FindHdmiPatchPathById(PatchCanvas, affectedHdmiPatch);
                    if (Path != null)
                    {
                        Path.Stroke = SelectedPatchBrush;
                        Path.StrokeThickness = HdmiSelectedStrokeThickness;
                    }
                }
                _selectedHdmiPatch = affectedHdmiPatch;
                if (affectedSinkPatch != NodeId.Undefined)
                {
                    Rectangle? rectangle = FindGridRectangleByPatchId(PatchCanvas, affectedSinkPatch);
                    if (rectangle != null)
                    {
                        rectangle.Stroke = SelectedPatchBrush;
                        rectangle.StrokeThickness = PatchSelectedStrokeThickness;
                    }
                }
                _selectedSinkPatch = affectedSinkPatch;
            }
        }

        /// <summary>
        /// Finds the visual representation of an HDMI cable on the Patch Canvas, given it's NodeId
        /// </summary>
        /// <param name="canvas">The Canvas to be searched</param>
        /// <param name="nodeId">The NodeId of the Path to be found</param>
        /// <returns>A Path object that contains the visual representation of the given NodeId (if found)</returns>
        private static Path? FindHdmiPatchPathById(Canvas canvas, NodeId nodeId)
        {
            Path? result = null;

            foreach (var child in canvas.Children)
            {
                if (child.GetType() == typeof(Path))
                {
                    Path path = (Path)child;
                    if (path.Name == nodeId.ToString() + HdmiPatchTag)
                    {
                        result = path;
                        break;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Finds the visual representation of a VideoNode on a Canvas, given it's NodeId
        /// </summary>
        /// <param name="canvas">The Canvas to be searched</param>
        /// <param name="patchRectangleId">The NodeId of the VideoNode to be found</param>
        /// <returns>A Rectangle object which is part of the visual representation of the given NodeId (if found)</returns>
        private static Rectangle? FindGridRectangleByPatchId(Canvas canvas, NodeId patchRectangleId)
        {
            Rectangle? result = null;

            foreach (var child in canvas.Children)
            {
                if (child.GetType() == typeof(Grid))
                {
                    Grid grid = (Grid)child;
                    // Now find the Rectangle inside the grid
                    foreach (var grandchild in grid.Children)
                    {
                        if (grandchild.GetType() == typeof(Rectangle))
                        {
                            Rectangle rectangle = (Rectangle)grandchild;
                            if (rectangle.Name == patchRectangleId.ToString() + NodeOutlineTag)
                            {
                                result = rectangle;
                                break;
                            }
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Set the enabled state of buttons on the Patch tab based on the current state of the tab controls.
        /// </summary>
        private void SetPatchCmdButtonStatus()
        {
            PatchConnectButton.IsEnabled =
                (_selectedSourcePatch != NodeId.Undefined
                && _selectedHdmiPatch == NodeId.Undefined
                && _selectedSinkPatch != NodeId.Undefined);
            PatchDisconnectButton.IsEnabled =
                (_selectedSourcePatch != NodeId.Undefined
                && _selectedHdmiPatch != NodeId.Undefined
                && _selectedSinkPatch != NodeId.Undefined);

        }

        private void PatchConnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Totally unecessary sanity check...
            if (_selectedSinkPatch != NodeId.Undefined
             && _selectedSourcePatch != NodeId.Undefined
             && _selectedHdmiPatch == NodeId.Undefined)
            {
                // Find an unused HDMI patch node
                var hdmi = GetUnusedHdmiPatchNode()
;
                if (hdmi != null)
                {
                    hardwiredNodes[_selectedSourcePatch].SetOutput(hdmi);
                    hardwiredNodes[_selectedSinkPatch].SetInput(hdmi);
                    _selectedHdmiPatch = hdmi.Id;

                    configuredNodes.Add(hdmi.Id, hdmi);

                    DrawPatchCanvas();
                    ShowPatchSelection(_selectedSourcePatch);
                }

                SaveCurrentPatchConfiguration();
            }
        }

        private void PatchDisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            // Totally unecessary sanity check...
            if (_selectedSinkPatch != NodeId.Undefined
             && _selectedSourcePatch != NodeId.Undefined
             && _selectedHdmiPatch != NodeId.Undefined)
            {
                var hdmi = configuredNodes[_selectedHdmiPatch];

                hdmi.DisconnectInput();
                hdmi.DisconnectOutput();
                configuredNodes.Remove(hdmi.Id);
                _selectedHdmiPatch = NodeId.Undefined;

                DrawPatchCanvas();
                ShowPatchSelection(_selectedSourcePatch);

                SaveCurrentPatchConfiguration();
            }
        }

        /// <summary>
        /// Finds an VideoNode of NodeType=HdmiCable which is not currently connected to the patch panel 
        /// </summary>
        /// <returns>The VideoNode of type HdmiCable which can be used to make a new patch connection</returns>
        private VideoNode? GetUnusedHdmiPatchNode()
        {
            VideoNode? result = null;

            foreach (NodeId nodeId in (NodeId[])Enum.GetValues(typeof(NodeId)))
            {
                if (Node.Type(nodeId) == NodeType.HdmiCable && !configuredNodes.ContainsKey(nodeId))
                {
                    result = new VideoNode(nodeId);
                }
            }

            return result;
        }

        /// <summary>
        /// Persists the current configurable patch connections to a settings file.
        /// </summary>
        private void SaveCurrentPatchConfiguration()
        {
            PatchConfiguration.Settings?.PatchItems.Clear();
            foreach (var patchNode in configuredNodes)
            {
                ConfigurableItem patchItem = new()
                {
                    NodeId = patchNode.Value.Id.ToString(),
                    NodeType = patchNode.Value.Type.ToString(),
                    Input = patchNode.Value.Input?.Id.ToString() ?? String.Empty,
                    Output = patchNode.Value.Output?.Id.ToString() ?? String.Empty
                };
                PatchConfiguration.Settings?.PatchItems.Add(patchItem);
            }
            PatchConfiguration.Save();
        }

        /// <summary>
        /// Draws the contents of the Matrix Canvas
        /// </summary>
        private void DrawMatrixCanvas()
        {
            // Draw the matrix canvas

            MatrixCanvas.Children.Clear();

            var destinationNodes = hardwiredNodes.Values
                .Where(n => n.Type == NodeType.VideoDestination)
                .OrderBy(n => n.DisplayOrder)
                .ToList();

            // Go through all of the video destinations and get a list of everything
            // that has a MxOutput upstream of it somehow.
            List<VideoNode> connectedMxDestinations = new();
            foreach (var node in destinationNodes)
            {
                VideoNode? upstreamMxOutput = node.UpstreamMxOutput;
                if (upstreamMxOutput != null && connectedMxDestinations.Contains(upstreamMxOutput) == false)
                {
                    connectedMxDestinations.Add(upstreamMxOutput);
                }
            }

            // Figure out which NodeType.MxOutput nodes are not connected and should be shown as parked.
            List<VideoNode> parkedMxOutputNodes = new();

            var mxOutNodes = hardwiredNodes.Values
                .Where(n => n.Type == NodeType.MxOutput)
                .OrderBy(n => n.DisplayOrder)
                .ToList();
            // Append any parked outputs to parkedMxOutNodes
            foreach (var mxOutNode in mxOutNodes)
            {
                if (connectedMxDestinations.Contains(mxOutNode) == false)
                {
                    parkedMxOutputNodes.Add(mxOutNode);
                }
            }

            var mxInNodes = hardwiredNodes.Values
            .Where(n => n.Type == NodeType.MxInput)
            .OrderByDescending(n => n.DisplayOrder)     // Descending so that parked nodes are in logical order
            .ToList();

            var sourceNodes = hardwiredNodes.Values
                .Where(n => n.Type == NodeType.VideoSource)
                .OrderBy(n => n.DisplayOrder)
                .ToList();

            var mxPicks = configuredNodes.Values
                .Where(n => n.Type == NodeType.MxSelection)
                .OrderBy(n => n.DisplayOrder)
                .ToList();

            // How big are the source/destination node rectangles?
            // Make enough room for the longest list of nodes (sources or destinations)
            // But also allow for margins of 10% on either side and in between
            // And also allow for 1/2 a column for parked MX inputs and outputs
            double matrixNodeWidth = MatrixCanvas.ActualWidth / (((Math.Max(destinationNodes.Count, sourceNodes.Count) + 0.5) * 1.1) + 0.1);
            double matrixNodeHeight = matrixNodeWidth * 0.5;
            double matrixNodeDiameter = matrixNodeWidth * 0.25;
            double matrixMargin = matrixNodeWidth * 0.1;
            double destinationRowTop = matrixMargin;                    // For now use as a simplifying assumption.
            double sourceRowTop = MatrixCanvas.ActualHeight - matrixNodeHeight - matrixMargin;
            double matrixLabelFontSize = matrixMargin * 1.25;           // For now use as a simplifying assumption.
            double mxNodeLabelFontSize = matrixLabelFontSize * 0.75;    // Arbitrary

            // TODO: Figure out how to avoid bailing due to zero width on the matrix tab.
            if (matrixLabelFontSize == 0)
            {
                return;
            }

            // Display the pertinent video nodes...
            int column = 0;
            foreach (var videoNode in destinationNodes)
            {
                column++;
                Grid destinationContainer = BuildSourceDestinationNode(videoNode, matrixNodeWidth, matrixNodeHeight, matrixLabelFontSize);
                Canvas.SetTop(destinationContainer, destinationRowTop);
                Canvas.SetLeft(destinationContainer, ((column - 1) * (matrixNodeWidth + matrixMargin)) + matrixMargin);
                MatrixCanvas.Children.Add(destinationContainer);
            }
            column = 0;
            foreach (var videoNode in sourceNodes)
            {
                column++;
                Grid sourceContainer = BuildSourceDestinationNode(videoNode, matrixNodeWidth, matrixNodeHeight, matrixLabelFontSize);
                Canvas.SetTop(sourceContainer, sourceRowTop);
                Canvas.SetLeft(sourceContainer, ((column - 1) * (matrixNodeWidth + matrixMargin)) + matrixMargin);
                MatrixCanvas.Children.Add(sourceContainer);
            }

            // Handle painting of the MxOutput nodes that are connected.
            foreach (var destVideoNode in destinationNodes)
            {
                var videoNode = destVideoNode.UpstreamMxOutput;
                if (videoNode != null)
                {
                    // NOTE: This is more complicated than for MxOutputs because there is no single, reliable .Downstream
                    //       property on VideoNode.  However, if we know that we're starting with a MxOutput then we
                    //       know that we can reliably follow the .Output all the way to the end.
                    Grid mxOutContainer = BuildMxInOutNode(videoNode, matrixNodeDiameter, matrixLabelFontSize);
                    VideoNode? downstream = videoNode.Output;
                    if (downstream != null)
                    {
                        Point location = GetMatrixNodeCnxPoint(destVideoNode.Id);
                        Canvas.SetTop(mxOutContainer, location.Y);
                        Canvas.SetLeft(mxOutContainer, location.X - (matrixNodeDiameter / 2));
                    }
                    MatrixCanvas.Children.Add(mxOutContainer);
                }
            }

            // Handle painting of parked MxOutput nodes.
            int parkedMxOutputsSoFar = 0;
            foreach (var parkedMxOutputNode in parkedMxOutputNodes)
            {
                Grid mxOutContainer = BuildMxInOutNode(parkedMxOutputNode, matrixNodeDiameter, matrixLabelFontSize);
                Canvas.SetTop(mxOutContainer, destinationRowTop + (parkedMxOutputsSoFar * (matrixNodeDiameter + matrixMargin)));
                Canvas.SetLeft(mxOutContainer, MatrixCanvas.ActualWidth - (matrixNodeDiameter + (2 * matrixMargin)));
                MatrixCanvas.Children.Add(mxOutContainer);
                parkedMxOutputsSoFar++;
            }

            // Handle the painting of MxInput nodes
            int parkedMxInputs = 0;
            foreach (var videoNode in mxInNodes)
            {
                Grid mxInContainer = BuildMxInOutNode(videoNode, matrixNodeDiameter, matrixLabelFontSize);
                // Where does this node belong? Is it on a Source or is it parked?
                if (videoNode.Upstream != null && videoNode.Upstream.Type == NodeType.VideoSource)
                {
                    Point location = GetMatrixNodeCnxPoint(videoNode.Upstream.Id);
                    Canvas.SetTop(mxInContainer, location.Y - matrixNodeDiameter);
                    Canvas.SetLeft(mxInContainer, location.X - (matrixNodeDiameter / 2));
                }
                else
                {
                    // Parked.
                    parkedMxInputs++;
                    Canvas.SetTop(mxInContainer, MatrixCanvas.ActualHeight - (parkedMxInputs * (matrixNodeDiameter + matrixMargin)));
                    Canvas.SetLeft(mxInContainer, MatrixCanvas.ActualWidth - (matrixNodeDiameter + (2 * matrixMargin)));
                }
                MatrixCanvas.Children.Add(mxInContainer);
            }

            // Display the direct connection video node lines
            foreach (var videoNode in destinationNodes)
            {
                if (!IsEligibleForMatrixSelection(videoNode)
                    && videoNode.Upstream != null
                    && videoNode.Upstream.Type == NodeType.VideoSource)
                {
                    // This is a direct connection (via patching)
                    var start = GetMatrixNodeCnxPoint(videoNode.Upstream.Id);
                    var finish = GetMatrixNodeCnxPoint(videoNode.Id);
                    Line mxDirect = new()
                    {
                        StrokeThickness = MxPickUnselectedStrokeThickness,
                        Stroke = Brushes.SlateGray,
                        X1 = start.X,
                        Y1 = start.Y,
                        X2 = finish.X,
                        Y2 = finish.Y,
                        StrokeEndLineCap = PenLineCap.Round
                    };

                    MatrixCanvas.Children.Add(mxDirect);
                }
            }

            // Display the matrix selection video nodes (lines)
            foreach (var mxSelection in mxPicks)
            {
                Point startingPoint = new();
                Point endingPoint = new();

                if (mxSelection.Input != null)
                {
                    startingPoint = GetMatrixNodeCnxPoint(mxSelection.Input.Id);
                }

                // Get every output for the selection.
                var mxSelectedOutputs = hardwiredNodes
                    .Where(n => n.Value.UpstreamMxOutput == mxSelection.Output 
                             && n.Value.Type == NodeType.VideoDestination);
                foreach (var mxSelectionOutput in mxSelectedOutputs)
                {
                    endingPoint = GetMatrixNodeCnxPoint(mxSelectionOutput.Value.Id);
                    // Adjust the Y value of the end point to account for node diameter.
                    endingPoint.Y += matrixNodeDiameter / 2.0; ;

                    // Work out the adjustment to get from the centre of each mx node to it's edge.
                    // NOTE: The starting point is always below the ending point (so Y is bigger at bottom, thanks WPF).
                    // NOTE: If the line is vertical then it's just the Y adjusted by the radius.
                    double dX = 0.0;
                    double dY = 0.0;

                    if (Math.Abs(endingPoint.X - startingPoint.X) < 0.1)
                    {
                        dY = matrixNodeDiameter / 2.0;      // dX = 0.0. No need for ATAN
                    }
                    else
                    {
                        // We need to work out the slope angle.
                        // Remember: endingPoint.Y < startingPoint.Y at all times, but the Y axis is inverted in WPF Canvas.
                        // Therefore take starting Y less ending Y, not the other way around.
                        double theta = Math.Atan2((startingPoint.Y - endingPoint.Y), (endingPoint.X - startingPoint.X));
                        dX = (matrixNodeDiameter / 2.0) * Math.Cos(theta);
                        dY = (matrixNodeDiameter / 2.0) * Math.Sin(theta);
                    }

                    Line mxPick = new()
                    {
                        StrokeThickness = MxPickUnselectedStrokeThickness,
                        Stroke = Brushes.Black,
                        X1 = startingPoint.X + dX,
                        Y1 = startingPoint.Y - dY,
                        X2 = endingPoint.X - dX,
                        Y2 = endingPoint.Y + dY,
                        Name = mxSelection.Id.ToString() + MxSelectTag,
                        StrokeEndLineCap = PenLineCap.Round
                    };

                    MatrixCanvas.Children.Add(mxPick);
                }
            }

            // Show the video node selections in the summary group box
            // Note: Don't do this if the configuration isn't loaded yet.
            if (configuredNodes.ContainsKey(NodeId.MxPick1))
            {
                // Include some visual cue when a matrix selection is irrelevant due to lack of connection from a source all the way to a destination.
                const string DontMatter = "X";
                if (HasEndToEndConnection(NodeId.MxPick1))
                {
                    MatrixSource1SelectionText.Text = configuredNodes[NodeId.MxPick1].Input?.Nickname;
                    MatrixDestination1Indicator.Background = ActiveMatrixConnection;
                }
                else
                {
                    MatrixSource1SelectionText.Text = DontMatter;
                    MatrixDestination1Indicator.Background = InactiveMatrixConnection;
                }

                if (HasEndToEndConnection(NodeId.MxPick2))
                {
                    MatrixSource2SelectionText.Text = configuredNodes[NodeId.MxPick2].Input?.Nickname;
                    MatrixDestination2Indicator.Background = ActiveMatrixConnection;
                }
                else
                {
                    MatrixSource2SelectionText.Text = DontMatter;
                    MatrixDestination2Indicator.Background = InactiveMatrixConnection;
                }

                if (HasEndToEndConnection(NodeId.MxPick3))
                {
                    MatrixSource3SelectionText.Text = configuredNodes[NodeId.MxPick3].Input?.Nickname;
                    MatrixDestination3Indicator.Background = ActiveMatrixConnection;
                }
                else
                {
                    MatrixSource3SelectionText.Text = DontMatter;
                    MatrixDestination3Indicator.Background = InactiveMatrixConnection;
                }

                if (HasEndToEndConnection(NodeId.MxPick4))
                {
                    MatrixSource4SelectionText.Text = configuredNodes[NodeId.MxPick4].Input?.Nickname;
                    MatrixDestination4Indicator.Background = ActiveMatrixConnection;
                }
                else
                {
                    MatrixSource4SelectionText.Text = DontMatter;
                    MatrixDestination4Indicator.Background = InactiveMatrixConnection;
                }

                MatrixSource1Indicator.Background = MatrixDestination1Indicator.Background;
                MatrixSource2Indicator.Background = MatrixDestination2Indicator.Background;
                MatrixSource3Indicator.Background = MatrixDestination3Indicator.Background;
                MatrixSource4Indicator.Background = MatrixDestination4Indicator.Background;

            }
        }

        /// <summary>
        /// Determines whether a given VideoNode has a downstream connection which is of NodeType=VideoDestination and an upstream connection 
        /// which is NodeType=VideoSource (allowing for the fact that the given node could actually be either end of that workflow)
        /// </summary>
        /// <param name="nodeId">The NodeId of the VideoNode of interest</param>
        /// <returns>True if there is a connection downstream all the way to a VideoDestination, false otherwise.</returns>
        private bool HasEndToEndConnection(NodeId nodeId)
        {
            bool result = false;
            VideoNode? videoNode = null;

            // Figure out which node we're starting from...
            if (hardwiredNodes.ContainsKey(nodeId))
            {
                videoNode = hardwiredNodes[nodeId];
            }
            else if (configuredNodes.ContainsKey(nodeId))
            {
                videoNode = configuredNodes[nodeId];
            }

            // Does the node have an upstream source?
            if (videoNode != null
                && (videoNode.Type == NodeType.VideoSource
                || (videoNode.Upstream != null && videoNode.Upstream.Type == NodeType.VideoSource)))
            {
                // OK on the upstream side. What about downstream?
                while (videoNode != null)
                {
                    // Go downstream until we find a destination.
                    if (videoNode.GetType() == typeof(MatrixInputNode))
                    {
                        // Look at each of the outputs in turn until we find at least one good one...
                        foreach (var output in ((MatrixInputNode)videoNode).Outputs)
                        {
                            if (HasEndToEndConnection(output.Id))
                            {
                                result = true;
                                videoNode = null;   // Time to break from the loop.
                                break;
                            }
                        }
                        // If we get this far without a positive result, then one is not forthcoming.
                        videoNode = null;   // Time to break from the loop.
                    }
                    else
                    {
                        // Look at the output...
                        if (videoNode.Output != null && videoNode.Output.Type == NodeType.VideoDestination)
                        {
                            result = true;
                            break;
                        }
                        else
                        {
                            videoNode = videoNode.Output;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Constructs a Canvas object (Grid) containing the visual elements required to display a representation of a video source or destination VideoNode.
        /// </summary>
        /// <param name="videoNode">The VideoNode to be represented visually</param>
        /// <param name="nodeWidth">The width, on the Canvas, which the VideoNode should take up</param>
        /// <param name="nodeHeight">The height, on the Canvas, which the VideoNode should take up</param>
        /// <param name="nodeFontSize">The font size to use for the TextBlock which labels the VideoNode</param>
        /// <returns>A Grid containing a Rectangle and a TextBlock</returns>
        private static Grid BuildSourceDestinationNode(VideoNode videoNode, double nodeWidth, double nodeHeight, double nodeFontSize)
        {
            Grid nodeContainer = new()
            {
                Width = nodeWidth,
                Height = nodeHeight,
                Name = videoNode.Id.ToString() + NodeContainerTag,
            };

            Rectangle nodeBox = new()
            {
                Width = nodeWidth,
                Height = nodeHeight,
                IsHitTestVisible = true
            };
            // Colour scheme depends on type and state of the source/sink...
            bool isEligible = IsEligibleForMatrixSelection(videoNode);
            if (isEligible)
            {
                nodeBox.Fill = Brushes.WhiteSmoke;
                nodeBox.Stroke = Brushes.Black;
            }
            else
            {
                nodeBox.Fill = Brushes.LightSlateGray;
                nodeBox.Stroke = Brushes.DarkSlateGray;
            }
            nodeBox.StrokeThickness = MatrixUnselectedStrokeThickness;
            nodeBox.Name = videoNode.Id.ToString() + NodeOutlineTag;

            TextBlock nodeText = new()
            {
                Width = nodeWidth,
                Text = videoNode.Nickname,
                FontSize = nodeFontSize,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(2, 0, 2, 0),  // Always allow just a hint of whitespace left and right of the text.
                IsHitTestVisible = false,
                Name = videoNode.Id.ToString() + NodeLabelTag
            };
            if (isEligible)
            {
                nodeText.Foreground = Brushes.Black;
            }
            else
            {
                nodeText.Foreground = Brushes.LightGray;
            }
            nodeContainer.Children.Add(nodeBox);
            nodeContainer.Children.Add(nodeText);

            return nodeContainer;
        }

        /// <summary>
        /// Constructs a Canvas object (Grid) containing the visual elements required to display a representation of a Matrix In or Out VideoNode.
        /// </summary>
        /// <param name="videoNode">The VideoNode to be represented visually</param>
        /// <param name="nodeDiameter">The size, on the Canvas, which the VideoNode should take up (diameter of a circle)</param>
        /// <param name="nodeFontSize">The font size to use for the TextBlock which labels the VideoNode</param>
        /// <returns></returns>
        private static Grid BuildMxInOutNode(VideoNode videoNode, double nodeDiameter, double nodeFontSize)
        {
            Grid nodeContainer = new()
            {
                Width = nodeDiameter,
                Height = nodeDiameter,
                Name = videoNode.Id.ToString() + NodeContainerTag,
            };

            Ellipse nodeBox = new()
            {
                Width = nodeDiameter,
                Height = nodeDiameter,
                IsHitTestVisible = false
            };

            // Colour scheme depends on the type of node and it's connections...
            if (videoNode.Type == NodeType.MxInput)
            {
                if (videoNode.Input != null && videoNode.Input.Type == NodeType.PatchSink)
                {
                    nodeBox.Fill = Brushes.Orange;
                    nodeBox.Stroke = Brushes.Black;
                }
                else
                {
                    nodeBox.Fill = Brushes.White;
                    nodeBox.Stroke = Brushes.Black;
                }
            }
            else if (videoNode.Type == NodeType.MxOutput)
            {
                if (videoNode.Output != null && videoNode.Output.Type == NodeType.PatchSource)
                {
                    nodeBox.Fill = Brushes.Yellow;
                    nodeBox.Stroke = Brushes.Black;
                }
                else
                {
                    nodeBox.Fill = Brushes.White;
                    nodeBox.Stroke = Brushes.Black;
                }
            }
            else
            {
                // Shouldn't happen, but you never know.
                nodeBox.Fill = Brushes.LightSlateGray;
                nodeBox.Stroke = Brushes.DarkSlateGray;
            }
            nodeBox.StrokeThickness = MatrixUnselectedStrokeThickness;
            nodeBox.Name = videoNode.Id.ToString() + NodeOutlineTag;

            TextBlock nodeText = new()
            {
                Width = nodeDiameter,
                Text = videoNode.Nickname,
                FontSize = nodeFontSize,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false,
                Name = videoNode.Id.ToString() + NodeLabelTag,
                Foreground = Brushes.Black
            };

            nodeContainer.Children.Add(nodeBox);
            nodeContainer.Children.Add(nodeText);

            return nodeContainer;
        }

        /// <summary>
        /// Determines whether a given VideoNode is eligible for connection via the Video Matrix switch based on its current connections.
        /// </summary>
        /// <param name="videoNode">The VideoNode to be checked (must be either a VideoSource or VideoDestination type)</param>
        /// <returns></returns>
        private static bool IsEligibleForMatrixSelection(VideoNode videoNode)
        {
            bool result = false;

            if (videoNode.Type == NodeType.VideoSource)
            {
                // Must have a downstream connection to a matrix input node to be eligible.
                VideoNode? nextNode = videoNode.Output;
                while (nextNode != null)
                {
                    if (nextNode.Type == NodeType.MxInput)
                    {
                        result = true;
                        break;
                    }
                    nextNode = nextNode.Output;
                }
            }
            else if (videoNode.Type == NodeType.VideoDestination)
            {
                // Must have an upstream connection to a matrix output node to be eligible.
                VideoNode? nextNode = videoNode.Input;
                while (nextNode != null)
                {
                    if (nextNode.Type == NodeType.MxOutput)
                    {
                        result = true;
                        break;
                    }
                    nextNode = nextNode.Input;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the coordinates of the connection point for a given VideoNode on the Matrix Canvas
        /// </summary>
        /// <param name="nodeId">NodeId of the VideoNode whose connection point is to be found.</param>
        /// <returns>A Point containing the X,Y coordinates where a line or path should connect to the VideoNode.
        /// For Video Sources and Destinations this will be the centre point of the top or bottom.
        /// For Matrix Inputs and Outputs this will be the centre point of the circle. Adjustments need to be made to get the circle edge.</returns>
        private Point GetMatrixNodeCnxPoint(NodeId? nodeId)
        {
            Point result = new();

            // Find the patch container and get the point which is the center of it's bottom line...
            foreach (var item in MatrixCanvas.Children)
            {
                if (item.GetType() == typeof(Grid))
                {
                    Grid hit = (Grid)item;
                    if (hit.Name == nodeId.ToString() + "Container")
                    {
                        // This is the one.  What is the middle point?
                        double halfWayOver = (double)hit.GetValue(Canvas.LeftProperty)
                            + ((double)hit.GetValue(Canvas.WidthProperty) / 2);

                        result.X = halfWayOver;

                        // Top or bottom or middle, depends on what we're looking for...
                        if (nodeId != null)
                        {
                            var videoNode = hardwiredNodes[(NodeId)nodeId];
                            if (videoNode.Type == NodeType.VideoSource)
                            {
                                // Use the top...
                                double top = (double)hit.GetValue(Canvas.TopProperty);
                                result.Y = top;
                            }
                            else if (videoNode.Type == NodeType.VideoDestination)
                            {
                                // Use the bottom...
                                double bottom = (double)hit.GetValue(Canvas.TopProperty)
                                + (double)hit.GetValue(Canvas.HeightProperty);
                                result.Y = bottom;
                            }
                            else if (videoNode.Type == NodeType.MxInput || videoNode.Type == NodeType.MxOutput)
                            {
                                // Use the centre...
                                double centre = (double)hit.GetValue(Canvas.TopProperty)
                                + ((double)hit.GetValue(Canvas.HeightProperty) / 2);
                                result.Y = centre;
                            }
                        }
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Handle the process of clicking on a video source or destination node in the Matrix Canvas.
        /// If a destination is selected set or change the selection. If a source is selected and there is already
        /// a selection for the destination then make the matrix selection connection between the two.
        /// </summary>
        /// <param name="selectedNodeId">The NodeId of the VideoNode that was clicked in the UI</param>
        private void HandleMatrixSelection(NodeId selectedNodeId)
        {
            // Show the matrix selection state.

            VideoNode? selectedNode = null;
            if (hardwiredNodes.ContainsKey(selectedNodeId))
            {
                selectedNode = hardwiredNodes[selectedNodeId];
            }
            // Otherwise nothing was selected.

            // Selecting a destination changes the destination selection.
            // Selecting a source maps the selected source to the selected destination, if it exists.
            // Otherwise selecting a destination is ignored.
            if (selectedNode != null)
            {
                if (selectedNode.Type == NodeType.VideoDestination)
                {
                    if (_selectedSinkMatrix != NodeId.Undefined && _selectedSinkMatrix != selectedNode.Id)
                    {
                        // Reset the visual selection of the previously selected destination...
                        Rectangle? rectangle = FindGridRectangleByPatchId(MatrixCanvas, _selectedSinkMatrix);
                        if (rectangle != null)
                        {
                            rectangle.Stroke = UnselectedPatchBrush;
                            rectangle.StrokeThickness = MatrixUnselectedStrokeThickness;
                        }
                        _selectedSinkMatrix = NodeId.Undefined;
                    }

                    // Now set the cues for the new selection, as appropriate...
                    if (IsEligibleForMatrixSelection(selectedNode))
                    {
                        if (selectedNode.Type == NodeType.VideoDestination)
                        {
                            Rectangle? rectangle = FindGridRectangleByPatchId(MatrixCanvas, selectedNode.Id);
                            if (rectangle != null)
                            {
                                rectangle.Stroke = SelectedPatchBrush;
                                rectangle.StrokeThickness = MatrixSelectedStrokeThickness;
                            }
                            _selectedSinkMatrix = selectedNode.Id;
                        }
                    }
                }
                else if (selectedNode.Type == NodeType.VideoSource || _selectedSinkMatrix != NodeId.Undefined)
                {
                    // Make the appropriate MX Selection

                    // Find the MxSelection that is upstream of the selected source...
                    VideoNode? upstreamMxPick = null;
                    VideoNode? nextUp = hardwiredNodes[_selectedSinkMatrix].Input;
                    while (nextUp != null)
                    {
                        if (nextUp.Type == NodeType.MxSelection)
                        {
                            upstreamMxPick = nextUp;
                            break;
                        }
                        nextUp = nextUp.Input;
                    }
                    if (upstreamMxPick != null)
                    {
                        // Find the MxInput that is downstream of the selected source
                        VideoNode? downstreamMxInput = null;
                        VideoNode? nextDown = selectedNode.Output;
                        while (nextDown != null)
                        {
                            if (nextDown.Type == NodeType.MxInput)
                            {
                                downstreamMxInput = nextDown;
                                break;
                            }
                            nextDown = nextDown.Output;
                        }
                        if (downstreamMxInput != null)
                        {
                            // Change the MxPick to the new selected source.
                            upstreamMxPick.DisconnectInput();
                            upstreamMxPick.SetInput(downstreamMxInput);
                            ((MatrixInputNode)downstreamMxInput).AddOutput(upstreamMxPick);

                            // Persist this change.
                            SaveCurrentPatchConfiguration();

                            // Redraw the matrix Canvas
                            DrawMatrixCanvas();
                        }
                    }
                }

            }
        }

        private void MatrixTabItem_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (MatrixTabItem.IsSelected)
            {
                DrawMatrixCanvas();
            }
        }

        private void MatrixCanvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Figure out what on the Matrix Canvas was clicked and then set the selection accordingly.
            // Where was the click? Was it on a source or destination node?
            if (e.Source.GetType() == typeof(Rectangle))
            {
                Rectangle clickedRectangle = (Rectangle)(e.Source);
                if (Enum.TryParse<NodeId>(clickedRectangle.Name.Replace(NodeOutlineTag, string.Empty), out NodeId clickedNodeId))
                {
                    if (hardwiredNodes.ContainsKey(clickedNodeId))
                    {
                        HandleMatrixSelection(clickedNodeId);
                    }
                }
            }
            else
            {
                // No selection...
                HandleMatrixSelection(NodeId.Undefined);
            }
        }
    }
}
