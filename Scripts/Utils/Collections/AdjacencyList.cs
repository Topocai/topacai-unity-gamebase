using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace Topacai.Utils.Collections
{
    /// <summary>
    /// Represents a node in an adjacency list using a string name as a unique identifier
    /// </summary>
    public class Node
    {
        // Used for unique identification
        public string Name { get; set; } = "";

        public override bool Equals(object obj)
        {
            return obj is Node other && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public Node(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// Represents a graph using an adjacency list
    /// </summary>
    public class AdjacencyList
    {
        /// <summary>
        /// The Adjacency Matrix, it contains all the connections between nodes
        /// Key = From, Value = To as a dictionary with all other nodes
        /// </summary>
        public Dictionary<Node, Dictionary<Node, bool>> Matrix = new Dictionary<Node, Dictionary<Node, bool>>();

        /// <summary>
        /// Inserts (or discconects) an edge between two nodes and optionally bidirectional
        /// </summary>
        /// <param name="from">Node from connects to</param>
        /// <param name="to">Node to connects from</param>
        /// <param name="disconnect"> Disconnects the edge </param>
        /// <param name="bidirectional"> Also connects the other way </param>
        public void InsertEdge(Node from, Node to, bool disconnect = false, bool bidirectional = false)
        {
            if (!Matrix.ContainsKey(from)) InsertVertex(from);
            if (!Matrix.ContainsKey(to)) InsertVertex(to);

            Matrix[from][to] = disconnect ? false : true;

            if (bidirectional) Matrix[to][from] = disconnect ? false : true;
        }

        /// <summary>
        /// Inserts many edges to one node
        /// Also you can define if disconnect or bidirectional
        /// </summary>
        /// <param name="from">Node from connects to</param>
        /// <param name="to[]">Nodes to connects from</param>
        /// <param name="disconnect"> Disconnects the edge </param>
        /// <param name="bidirectional"> Also connects the other way </param>
        public void InsertEdge(Node from, Node[] to, bool disconnect = false, bool bidirectional = false)
        {
            foreach (var node in to)
            {
                InsertEdge(from, node, disconnect, bidirectional);
            }
        }

        /// <summary>
        /// Removes a node from the graph
        /// </summary>
        /// <param name="vertex">The node to remove</param>
        public void RemoveVertex(Node vertex)
        {
            if (!Matrix.ContainsKey(vertex)) return;

            foreach (var connections in Matrix.Values)
            {
                connections.Remove(vertex);
            }

            Matrix.Remove(vertex);
        }

        /// <summary>
        /// Inserts a node to the graph
        /// </summary>
        /// <param name="node">The node with a unique name to insert</param>
        /// <param name="connections"></param>
        /// <returns></returns>
        public Node InsertVertex(Node node, List<Node> connections = null)
        {
            if (Matrix.ContainsKey(node))
            {
                // If already has that Vertex we make update them connections/edges
#if UNITY_EDITOR 
                Debug.LogWarning($"Node {node.Name} already exists in the graph, it will be updated.");
#else
                System.Console.WriteLine($"Node {node.Name} already exists in the graph, it will be updated.");
#endif
                if (connections != null) InsertEdge(node, connections.ToArray());
                return node;
            }

            if (connections == null)
            {
                connections = new List<Node>();
            }

            var newConnections = new Dictionary<Node, bool>();

            // For each node already in graph, we add it to the Edges Values, if is not specified in connections, the edge will be false
            foreach (var nodes in Matrix.Keys)
            {
                newConnections[nodes] = connections.Contains(nodes) ? true : false;
                Matrix[nodes][node] = false;
            }

            // Add the last edge at the matrix (its own edge)
            newConnections[node] = false;
            Matrix.Add(node, newConnections);

            return node;
        }
    }
}
