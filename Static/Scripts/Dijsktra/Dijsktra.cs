using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Topacai.Static.Disjkstra
{
    // Interface used unique to represent a tile on scene, not used for dijkstra logic
    public interface ITileNode
    {
        string Name { get; }

        void MarkAsVisited(float cost);

        void MarkAsPath();

        void MarkAsStart();

        void MarkAsEnd();
    }


    [System.Serializable]
    public class TileNode
    {
        public Dictionary<TileNode, float> Neighbors = new Dictionary<TileNode, float>();

        public ITileNode TileInterface { get; private set; }

        public TileNode(ITileNode tile = null)
        {
            TileInterface = tile;
        }

        public void SetInterface(ITileNode tile) => TileInterface = tile;
    }
    [System.Serializable]
    public class Graph
    {
        public List<TileNode> Tiles = new List<TileNode>();

        public void AddTile(TileNode tile) => Tiles.Add(tile);

        public void ConnectNodes(TileNode from, TileNode to, float cost)
        {
            if (to == null || from == null) return;
            from.Neighbors.Add(to, Mathf.Clamp(cost, 0, float.MaxValue));

            if (from.TileInterface != null && to.TileInterface != null)
                Debug.Log($"Connected {from.TileInterface.Name} to {to.TileInterface.Name} with cost {cost}");
        }
    }

    // This struct is used to set up the weights of the tiles on the prototype graph environment
    // When a new graph is created, this struct is used to set up weights checking for other graphs on the scene in that directions
    [System.Serializable]
    public struct TileWeights
    {
        [field: SerializeField] public float UpCost { get; private set; }
        [field: SerializeField] public float DownCost { get; private set; }
        [field: SerializeField] public float LeftCost { get; private set; }
        [field: SerializeField] public float RightCost { get; private set; }

        public TileWeights(float up = 1, float down = 1, float left = 1, float right = 1)
        {
            UpCost = up;
            DownCost = down;
            LeftCost = left;
            RightCost = right;
        }
    }
}
