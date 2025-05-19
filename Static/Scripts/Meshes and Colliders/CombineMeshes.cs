using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Topacai.Meshes
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class CombineMeshes : MonoBehaviour
    {
        [SerializeField] private bool combineOnStart = false;

        public bool setMateials = false;
        private void Start()
        {
            if (combineOnStart) Combine();
        }

        public void Combine()
        {
            MeshFilter[] meshesInChilds = GetComponentsInChildren<MeshFilter>();
            List<CombineInstance> combineInstances = new List<CombineInstance>(meshesInChilds.Length);

            List<Material> materials = new List<Material>();

            foreach (var meshFilter in meshesInChilds)
            {
                if (meshFilter == this.GetComponent<MeshFilter>()) continue;
                MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

                if (!materials.Exists(m => m.name == meshRenderer.material.name))
                {
                    materials.Add(meshRenderer.material);
                }

                combineInstances.Add(new CombineInstance()
                {
                    mesh = meshFilter.sharedMesh,
                    transform = transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix
                });

                meshFilter.gameObject.SetActive(false);
            }

            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combineInstances.ToArray());
            GetComponent<MeshFilter>().sharedMesh = mesh;
            GetComponent<MeshRenderer>().materials = materials.ToArray();
        }
    }
}