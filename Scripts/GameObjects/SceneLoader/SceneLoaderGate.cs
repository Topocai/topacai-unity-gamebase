using UnityEngine;
using UnityEngine.Events;

namespace Topacai.Static.GameObjects.Scenes
{
    public class SceneLoaderGate : MonoBehaviour
    {
        public UnityEvent<Collider> OnPlayerEnter = new();

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player")) OnPlayerEnter?.Invoke(other);
        }
    }
}


