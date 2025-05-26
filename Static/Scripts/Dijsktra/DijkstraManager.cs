using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Topacai.Utils;

namespace Topacai.Static.Disjkstra
{
    // A graph manager for the dijkstra algorithm as an monobehavior object
    public class DijkstraManager : MonoBehaviour
    {
        public Graph Graph { get; private set; } = new Graph();

        // Graphs used to search a path between two nodes on start
        [SerializeField] private TileNodeMonoBehaviour forceStart;
        [SerializeField] private TileNodeMonoBehaviour forceEnd;

        private void Start()
        {
            // Force to search a path with the start and end nodes setted on the inspector
            GetPath(forceStart.TileNode, forceEnd.TileNode);
        }

        public void ConnectNodes(TileNode from, TileNodeMonoBehaviour to, float cost )
        {
            if (to == null || from == null) return;
            Graph.ConnectNodes(from, to.TileNode, cost);
        }

        public List<TileNode> GetPath(TileNode start, TileNode end)
        {
            Dictionary<TileNode, float> distances = new Dictionary<TileNode, float>(); // Save the distance between the start graph to the graph saved.
            Dictionary<TileNode, TileNode> previous = new Dictionary<TileNode, TileNode>();
            // A priority queue works like a queue but automatically sorts the elements by their priority
            PriorityQueue<TileNode> unvisited = new PriorityQueue<TileNode>();

            distances[start] = 0;

            // The dijkstra algorithms indicates that all graphs needs to be
            // setted as unvisited at the start of the algorithm, but, setting it at the start
            // will force the algorith to search on all graph unnecessarily, so, we only set the start as unvisited
            // then, during iteration, we will add neighbors as unvisited.
            unvisited.Enqueue(start, 0);

            // the iteration finish when there are no more unvisited graphs or when the end graph is found
            while (unvisited.Count > 0)
            {
                TileNode current = unvisited.Dequeue();

                if (current == end)
                    break;

                foreach (var neighbor in current.Neighbors)
                {
                    // Always setup the distance as the higher possible

                    if (!distances.ContainsKey(neighbor.Key))
                    {
                        distances[neighbor.Key] = float.MaxValue;
                    }

                    // Calculate the distance to the neighbor using the previous distance calculated of the current graph
                    float distanceTo = distances[current] + neighbor.Value;
                    
                    // If the new distance is less than the previous one, update it
                    // the neighbor graph is added to the "previous" list to save the path
                    // and also added to the unvisited queue with the new distance as priority
                    if (distanceTo < distances[neighbor.Key])
                    {
                        distances[neighbor.Key] = distanceTo;
                        previous[neighbor.Key] = current;
                        unvisited.Enqueue(neighbor.Key, (int)distanceTo);

                        // Mark graph on scene as visited | not necesary for the algorithm
                        neighbor.Key.TileInterface.MarkAsVisited(distances[neighbor.Key]);
                    }
                }
            }

            // We define the path using the previous list that saves to each graph the previous graph
            // from the end to the start with the lowest graph distance possible.
            List<TileNode> path = new List<TileNode>();
            TileNode currentTile = end;

            while (previous.ContainsKey(currentTile) && previous[currentTile] != null)
            {
                // Mark graph on scene as a path
                if (currentTile.TileInterface != null)
                    currentTile.TileInterface.MarkAsPath();

                path.Insert(0, currentTile);
                currentTile = previous[currentTile];
            }

            // if for some reason the path is not found, return null
            if (!previous.ContainsKey(end))
                path = null;
            else 
                path.Insert(0, start);

            // Mark graph on scene as start and end | not necesary for the algorithm
            if (start.TileInterface != null && end.TileInterface != null)
            {
                start.TileInterface.MarkAsStart();
                end.TileInterface.MarkAsEnd();
            }

            return path;
        }
    }
}
