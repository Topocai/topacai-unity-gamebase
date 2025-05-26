using EditorAttributes;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Topacai.Static.Disjkstra
{
    // A graph node represented as a monobehaviour object on unity scene
    public class TileNodeMonoBehaviour : MonoBehaviour, ITileNode
    {
        [field: SerializeField] public TileNode TileNode { get; private set; } = new TileNode();

        [SerializeField] private DijkstraManager _graphManager;
        [SerializeField] private TileWeights _tileWeights;

        private void Awake()
        {
            // Create the graph node and add to the graph on the creation of the instance to avoid null references,
            // connect it to the other nodes based on 3d direction of the scene and using weights predefined on unity editor
            TileNode.SetInterface(this);
            _graphManager.Graph.AddTile(TileNode);

            _graphManager.ConnectNodes(TileNode, GetTileInDir(transform.forward), _tileWeights.UpCost);
            _graphManager.ConnectNodes(TileNode, GetTileInDir(transform.forward * -1), _tileWeights.DownCost);
            _graphManager.ConnectNodes(TileNode, GetTileInDir(transform.right * -1), _tileWeights.LeftCost);
            _graphManager.ConnectNodes(TileNode, GetTileInDir(transform.right), _tileWeights.RightCost);
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
    }
    
}
