using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Topacai.Utils.Collections;
using UnityEngine;

namespace Topacai.Static.Disjkstra
{
#if UNITY_EDITOR
    // Interface used unique to represent a tile on scene, not used for dijkstra logic
    public interface ITileNode
    {
        string Name { get; }

        void MarkAsVisited(float cost);

        void MarkAsPath(float cost);

        void MarkAsStart();

        void MarkAsEnd();
    }
#endif

    [System.Serializable]
    public class TileNode
    {
        public Dictionary<TileNode, float> Neighbors = new Dictionary<TileNode, float>();

#if UNITY_EDITOR
        public ITileNode TileInterface { get; private set; }

        public TileNode(ITileNode tile = null)
        {
            TileInterface = tile;
        }

        public void SetInterface(ITileNode tile) => TileInterface = tile;
#endif
    }

    public enum TileState { Unvisited, Visited, Path, Start, End };

    public class TileVisitedEventArgs : EventArgs
    {
        public TileNode Node { get; private set; }
        public TileState TileState { get; private set; }
        public float CostOnPath { get; private set; }

        public TileVisitedEventArgs(TileNode node, TileState tileState, float costOnPath)
        {
            Node = node;
            TileState = tileState;
            CostOnPath = costOnPath;
        }
    }

    [System.Serializable]
    public class Graph
    {
        public event EventHandler<TileVisitedEventArgs> OnTileVisited;

        public List<TileNode> Tiles = new List<TileNode>();

        public void AddTile(TileNode tile) => Tiles.Add(tile);

        public void ConnectNodes(TileNode from, TileNode to, float cost)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));

            from.Neighbors.Add(to, Mathf.Clamp(cost, 0, float.MaxValue));

#if UNITY_EDITOR
            Debug.Log($"Connected {from.TileInterface.Name} to {to.TileInterface.Name} with cost {cost}");
#endif
        }

        public List<TileNode> GetPath(TileNode start, TileNode end)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (end == null) throw new ArgumentNullException(nameof(end));

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

                        OnTileVisited.Invoke
                        (
                            this,
                            new TileVisitedEventArgs(neighbor.Key, TileState.Visited, distances[neighbor.Key])
                        );
                    }
                }
            }

            // We define the path using the previous list that saves to each graph the previous graph
            // from the end to the start with the lowest graph distance possible.
            List<TileNode> path = new List<TileNode>();
            TileNode currentTile = end;

            while (previous.ContainsKey(currentTile) && previous[currentTile] != null)
            {
                OnTileVisited.Invoke
                (
                    this,
                    new TileVisitedEventArgs(currentTile, TileState.Path, distances[currentTile])
                );

                path.Insert(0, currentTile);
                currentTile = previous[currentTile];
            }

            // if for some reason the path is not found, return null
            if (!previous.ContainsKey(end))
                path = null;
            else
                path.Insert(0, start);

            OnTileVisited.Invoke(this, new TileVisitedEventArgs(end, TileState.End, distances[end]));
            OnTileVisited.Invoke(this, new TileVisitedEventArgs(start, TileState.Start, distances[start]));

            return path;
        }
    }
}
