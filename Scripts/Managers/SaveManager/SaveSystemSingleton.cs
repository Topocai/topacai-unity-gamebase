using Topacai.Utils.GameObjects;
using UnityEngine;

namespace Topacai.Utils.SaveSystem
{
    public class SaveSingleton : Singleton<SaveSingleton>
    {
        private void OnApplicationQuit()
        {
            SaveSystemClass.CallSaveGameEvent();
        }

        protected override void Awake()
        {
            base.Awake();

            SaveSystemClass.RecoverProfiles();
        }

        private void Start()
        {
            SaveSystemClass.OnSaveGameEvent += SaveGameHandler;
        }

        private void SaveGameHandler(object sender, System.EventArgs e)
        {
            Debug.Log("Saving game");
        }
    }
}
