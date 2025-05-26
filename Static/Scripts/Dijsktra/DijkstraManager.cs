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
        public static DijkstraManager Instance { get; private set; }
        public Graph Graph { get; private set; } = new Graph();

        // Graphs used to search a path between two nodes on start
        [SerializeField] private TileNodeMonoBehaviour forceStart;
        [SerializeField] private TileNodeMonoBehaviour forceEnd;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            // Force to search a path with the start and end nodes setted on the inspector
            Graph.GetPath(forceStart.TileNode, forceEnd.TileNode);

            Graph.OnTileVisited += TileVisited;
        }

        private void TileVisited(object sender, TileVisitedEventArgs args)
        {
            ITileNode tileInterface = args.Node.TileInterface;

            if (tileInterface == null) return;

            switch(args.TileState)
            {
                case TileState.Visited:
                    tileInterface.MarkAsVisited(args.CostOnPath);
                    break;
                case TileState.Start:
                    tileInterface.MarkAsStart();
                    break;
                case TileState.End:
                    tileInterface.MarkAsEnd();
                    break;
                case TileState.Path:
                    tileInterface.MarkAsPath(args.CostOnPath);
                    break;
            }
        }

        public void ConnectNodes(TileNode from, TileNode to, float cost)
        {
            Graph.ConnectNodes(from, to, cost);
        }

        public void ConnectNodes(TileNodeMonoBehaviour from, TileNodeMonoBehaviour to, float cost)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (to == null) throw new ArgumentNullException(nameof(to));

            Graph.ConnectNodes(from.TileNode, to.TileNode, cost);
        }
    }
}
