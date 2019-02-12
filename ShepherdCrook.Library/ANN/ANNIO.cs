using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ShepherdCrook.Library.ANN
{
    public static class ANNIO
    {
        #region Constants

        private const String FileRepComment = "#";
        private const String LabelNetworkId = "Network #";
        private const String LabelNodeCount = "Node Count: ";
        private const String LabelNodeId = "Node #";
        private const String LabelLinkCount = "Link Count: ";
        private const String LabelLinkId = "Link #";
        private const char LineDelimiter = ',';

        #endregion

        #region Public Methods

        #region Reading

        /// <summary>
        /// Creates a network from a list of strings.
        /// Assumes that comment strings have been removed.
        /// </summary>
        public static void FromStringRepresentation(Network network, List<String> lines)
        {
            int lineIndex = 0;
            network.Id = Int32.Parse(lines[lineIndex++].Replace(LabelNetworkId, String.Empty));

            int numNodes = Int32.Parse(lines[lineIndex++].Replace(LabelNodeCount, String.Empty));
            for (int n = 0; n < numNodes; n++)
            {
                String[] lineSplit = lines[lineIndex++].Split(LineDelimiter);
                Node node = new Node(Int32.Parse(lineSplit[0].Replace(LabelNodeId, String.Empty)));
                node.Role = (NodeRole)Enum.Parse(typeof(NodeRole), lineSplit[1]);
                network.AddNode(node);
            }

            int numLinks = Int32.Parse(lines[lineIndex++].Replace(LabelLinkCount, String.Empty));
            for (int n = 0; n < numLinks; n++)
            {
                String[] lineSplit = lines[lineIndex++].Split(LineDelimiter);
                Link link = new Link(Int32.Parse(lineSplit[0].Replace(LabelLinkId, String.Empty)));
                link.NodeIn = network.MapIdToAllNodes[Int32.Parse(lineSplit[1])];
                link.NodeOut = network.MapIdToAllNodes[Int32.Parse(lineSplit[2])];
                link.Weight = Double.Parse(lineSplit[3]);
                network.AddLink(link);
            }
        }

        /// <summary>
        /// Generates a list of networks given a filepath.  Ignores all comments in the file.
        /// </summary>
        public static List<Network> LoadFromFile(String filepath)
        {
            List<Network> allNets = new List<Network>();
            String[] allLines = File.ReadAllLines(filepath);
            int index = 0;
            while (index < allLines.Length)
            {
                List<String> netLines = new List<string>();
                String line = allLines[index];
                while (line != String.Empty)
                {
                    if (!line.StartsWith(FileRepComment))
                    {
                        netLines.Add(line);
                    }
                    index++;
                    line = allLines[index];
                }
                Network n = new Network(netLines);
                allNets.Add(n);
                index++;
            }
            return allNets;
        }

        #endregion

        #region Writing

        /// <summary>
        /// Represents this network as a string.
        /// </summary>
        public static String ToStringRepresentation(Network network)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(LabelNetworkId + network.Id);
            sb.AppendLine(LabelNodeCount + network.NodeCount);
            foreach (Node node in network.MapIdToAllNodes.Values)
            {
                sb.AppendLine(ToStringRepresentation(node));
            }

            sb.AppendLine(LabelLinkCount + network.LinkCount);
            foreach (Link link in network.MapIdToAllLinks.Values)
            {
                sb.AppendLine(ToStringRepresentation(link));
            }

            return sb.ToString();
        }

        public static String ToStringRepresentation(Node node)
        {
            return LabelNodeId + node.Id + ", " + node.Role;
        }

        public static String ToStringRepresentation(Link link)
        {
            return LabelLinkId + link.Id + LineDelimiter +
                    link.NodeIn.Id + LineDelimiter +
                    link.NodeOut.Id + LineDelimiter +
                    link.Weight;
        }

        /// <summary>
        /// Saves a list of networks to a file.
        /// </summary>
        public static void SaveToFile(List<Network> networks, String filepath)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Network n in networks)
            {
                sb.AppendLine(n.ToString());
                sb.AppendLine();
            }
            File.WriteAllText(filepath, sb.ToString());
        }

        #endregion

        #endregion
    }
}
