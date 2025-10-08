using Topacai.Utils.GameObjects;
using UnityEngine;

namespace Topacai.Utils.SaveSystem
{
    public class SaveSingleton : Singleton<SaveSingleton>
    {
        private void OnApplicationQuit()
        {
            SaveSystemClass.OnSaveGameEvent?.Invoke();
        }

        protected override void Awake()
        {
            base.Awake();

            SaveSystemClass.RecoverProfiles();
        }

        private void Start()
        {
            SaveSystemClass.OnSaveGameEvent.AddListener(SaveGameHandler);
        }

        private void SaveGameHandler()
        {
            Debug.Log("Saving game");
        }
    }
}
