using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Topacai.Static.Disjkstra
{
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

    [System.Serializable]
    public struct TileConnection
    {
        [field: SerializeField] public TileNodeMonoBehaviour Node { get; private set; }
        [field: SerializeField] public float Cost { get; private set; }

        public TileConnection(TileNodeMonoBehaviour node, float cost = 1)
        {
            Node = node;
            Cost = cost;
        }
    }

    /// <summary>
    /// A graph node represented as a monobehaviour object on unity scene
    /// </summary>
    public class TileNodeMonoBehaviour : MonoBehaviour, ITileNode
    {
        [field: SerializeField] public TileNode TileNode { get; private set; } = new TileNode();

        [SerializeField] private TileWeights _tileWeights;
        [SerializeField] private TileConnection[] _connections = new TileConnection[0];

        private void Awake()
        {
            // Create the graph node and add to the graph on the creation of the instance to avoid null references,
            // connect it to the other nodes based on 3d direction of the scene and using weights predefined on unity editor
            TileNode.SetInterface(this);
            DijkstraManager.Instance.Graph.AddTile(TileNode);

            // Connect adjacent nodes in 3d scene space
            DijkstraManager.Instance.ConnectNodes(TileNode, GetTileInDir(transform.forward).TileNode, _tileWeights.UpCost);
            DijkstraManager.Instance.ConnectNodes(TileNode, GetTileInDir(transform.forward * -1).TileNode, _tileWeights.DownCost);
            DijkstraManager.Instance.ConnectNodes(TileNode, GetTileInDir(transform.right * -1).TileNode, _tileWeights.LeftCost);
            DijkstraManager.Instance.ConnectNodes(TileNode, GetTileInDir(transform.right).TileNode, _tileWeights.RightCost);

            // Connect the other nodes setted on the inspector
            if (_connections.Length == 0) return;
            for (int i = 0; i < _connections.Length; i++)
            {
                DijkstraManager.Instance.ConnectNodes(TileNode, _connections[i].Node.TileNode, _connections[i].Cost);
            }
        }

        private TileNodeMonoBehaviour GetTileInDir(Vector3 dir)
        {
            RaycastHit _hit;
            Debug.DrawRay(transform.position, dir.normalized * 0.75f, Color.red, 4f);
            if (Physics.Raycast(transform.position, dir.normalized, out _hit, 0.75f))
            {
                if (_hit.transform.TryGetComponent(out TileNodeMonoBehaviour node))
                {
                    return node;
                }
            }

            return null;
        }

        #region ITileNode implementation

        public string Name => gameObject.name;

        private void DisableAllChilds()
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(false);
            }
        }

        public void MarkAsPath(float cost)
        {
            DisableAllChilds();
            transform.GetChild(1).gameObject.SetActive(true);

            transform.GetChild(4).gameObject.SetActive(true);
            transform.GetChild(4).GetComponent<TextMeshPro>().text = cost.ToString();
        }

        public void MarkAsVisited(float cost)
        {
            DisableAllChilds();
            transform.GetChild(4).gameObject.SetActive(true);

            transform.GetChild(4).GetComponent<TextMeshPro>().text = cost.ToString();
        }

        public void MarkAsStart()
        {
            DisableAllChilds();
            transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(4).gameObject.SetActive(true);
        }

        public void MarkAsEnd()
        {
            DisableAllChilds();
            transform.GetChild(3).gameObject.SetActive(true);
            transform.GetChild(4).gameObject.SetActive(true);
        }

        #endregion
    }
    
}
