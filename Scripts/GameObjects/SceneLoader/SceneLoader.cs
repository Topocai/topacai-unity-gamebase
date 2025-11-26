using EditorAttributes;

using Topacai.Managers.GM;
using Topacai.Player;

using Unity.VisualScripting;

using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Topacai.Static.GameObjects.Scenes
{
    public class SceneLoader : MonoBehaviour
    {
        private enum ActivationType
        {
            Range,
            Collision,
            OnLoadMainScene
        }

        [Header("Main Config")]
#if UNITY_EDITOR
        [SceneName]
#endif
        [SerializeField, EnableField(nameof(activationType), ActivationType.OnLoadMainScene)] private string _mainScene;
#if UNITY_EDITOR
        [SceneName]
#endif
        [SerializeField] private string[] scenes_A;
#if UNITY_EDITOR
        [SceneName]
#endif
        [SerializeField, EnableField(nameof(_switchGroupsFlag))] private string[] scenes_B;
        [Space(2)]
        [SerializeField] private ActivationType activationType;
        [SerializeField] private bool _switchGroupsFlag;
        [Space(5)]
        [SerializeField] private PlayerBrain _player;

        [Header("Collision - gates config")]
        [SerializeField, EnableField(nameof(activationType), ActivationType.Collision)] private SceneLoaderGate _enterGate;
        [SerializeField, EnableField(nameof(activationType), ActivationType.Collision)] private SceneLoaderGate _exitGate;

        [Header("Range config")]
        [SerializeField, EnableField(nameof(activationType), ActivationType.Range)] private float activationRange = 10f;
        
        private PlayerBrain _Player => PlayerBrain.SINGLEPLAYER_MODE ? PlayerBrain.SP_Player : _player;
        public void SetPlayerBrain(PlayerBrain player) => _player = player;

        private bool _inRange = false;

#if UNITY_EDITOR

        private Color _rangeColor = Color.blue;

        private void SwitchRangeColor()
        {
            _rangeColor = _rangeColor == Color.red ? Color.blue : Color.red;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _rangeColor;
            if (activationType == ActivationType.Range)
                Gizmos.DrawWireSphere(transform.position, activationRange);
        }
#endif

        private void Awake()
        {
            SceneManager.sceneLoaded += OnMainSceneLoaded;
            SceneManager.sceneUnloaded += OnMainSceneUnloaded;

            _enterGate?.OnPlayerEnter.AddListener(OnPlayerEnterGate);
            _exitGate?.OnPlayerEnter.AddListener(OnPlayerExitGate);
        }

        private void EnableGroup(string[] scenes)
        {
            if (scenes.Length == 0) return;

            GameManager.Instance.LoadSetOfScenes(this, scenes);
        }

        private void DisableGroup(string[] scenes)
        {
            if (scenes.Length == 0) return;

            GameManager.Instance.UnloadSetOfScenes(this, scenes);
        }

        private void SwitchGroups(bool groupA)
        {
            if (groupA)
            {
                EnableGroup(scenes_A);
                DisableGroup(scenes_B);
            }
            else
            {
                EnableGroup(scenes_B);
                DisableGroup(scenes_A);
            }
        }

        private void OnMainSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (activationType != ActivationType.OnLoadMainScene) return;
            if (scene.name != _mainScene) return;

            if (_switchGroupsFlag)
                SwitchGroups(true);
            else
                EnableGroup(scenes_A);
        }

        private void OnMainSceneUnloaded(Scene scene)
        {
            if (activationType != ActivationType.OnLoadMainScene) return;
            if (scene.name != _mainScene) return;

            if (_switchGroupsFlag)
                SwitchGroups(false);
            else
                DisableGroup(scenes_A);
        }

        private void Update()
        {
            if (_Player == null || activationType != ActivationType.Range) return;

            float distance = Vector3.Distance(_Player.transform.position, transform.position);


            if (distance < activationRange && !_inRange)
            {
                if (_switchGroupsFlag) SwitchGroups(true);
                else EnableGroup(scenes_A);

                _inRange = true;

#if UNITY_EDITOR
                SwitchRangeColor();
                Invoke(nameof(SwitchRangeColor), 0.5f);
#endif
            }
            else if (distance > activationRange && _inRange)
            {
                if (_switchGroupsFlag) SwitchGroups(false);
                else DisableGroup(scenes_A);

                _inRange = false;

#if UNITY_EDITOR
                SwitchRangeColor();
                Invoke(nameof(SwitchRangeColor), 0.5f);
#endif
            }
        }

        private void OnPlayerEnterGate(Collider other)
        {
            if (activationType == ActivationType.Collision)
            {
                if (_switchGroupsFlag) SwitchGroups(true);
                else EnableGroup(scenes_A);
            }
        }

        private void OnPlayerExitGate(Collider other)
        {
            if (activationType == ActivationType.Collision)
            {
                if (_switchGroupsFlag) SwitchGroups(false);
                else DisableGroup(scenes_A);
            }
        }
    }
}

