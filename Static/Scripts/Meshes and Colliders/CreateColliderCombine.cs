using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Topacai.Meshes;

namespace Topacai.Colliders
{
    [RequireComponent(typeof(CombineMeshes))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshCollider))]
    public class CreateColliderCombine : MonoBehaviour
    {
        private CombineMeshes CombineMesh;

        private void Start()
        {
            MeshCollider collider = GetComponent<MeshCollider>();

            CombineMesh = GetComponent<CombineMeshes>();
            CombineMesh.setMateials = true;
            CombineMesh.Combine();

            collider.sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        }
    }
}