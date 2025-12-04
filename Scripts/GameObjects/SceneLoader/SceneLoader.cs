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

        private enum SelectType
        {
            SwitchGroupA,
            SwitchGroups,
            GoToScene
        }

        [Header("Main Config")]
#if UNITY_EDITOR
        [SceneName]
#endif
        [SerializeField, EnableField(nameof(activationType), ActivationType.OnLoadMainScene)] private string _mainScene;
#if UNITY_EDITOR
        [SceneName]
#endif
        [SerializeField, EnableField(nameof(_selectType), SelectType.GoToScene)] private string _goToScene;
#if UNITY_EDITOR
        [SceneName]
#endif
        [SerializeField, DisableField(nameof(_selectType), SelectType.GoToScene)] private string[] scenes_A;
#if UNITY_EDITOR
        [SceneName]
#endif
        [SerializeField, EnableField(nameof(_selectType), SelectType.SwitchGroups)] private string[] scenes_B;
        [Space(2)]
        [SerializeField] private ActivationType activationType;
        [Tooltip("What type of gate is in use. \n'SwitchGroupsA' uses only group A, the group is enabled/disabled\n'SwitchGroups' uses both groups A and B, the groups are enabled/disabled\n'GoToScene' switch current scene to the selected one")]
        [SerializeField] private SelectType _selectType;
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

        private System.Action CurrentAction => _selectType == SelectType.SwitchGroups ? 
            SwitchingAction : _selectType == SelectType.GoToScene ?
            GoToSceneAction : UseGroupAAction;

        private bool _isGroupAEnabled => scenes_A.Length > 0 && SceneManager.GetSceneByName(scenes_A[0]).isLoaded;

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

        private void SwitchingAction()
        {
            SwitchGroups(_isGroupAEnabled);
        }

        private void UseGroupAAction()
        {
            if (scenes_A.Length < 0) return;

            if (_isGroupAEnabled)
                DisableGroup(scenes_A);
            else
                EnableGroup(scenes_A);
        }

        private void GoToSceneAction()
        {
            GameManager.Instance.LoadScene(this, _goToScene, LoadSceneMode.Single);
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

            CurrentAction?.Invoke();
        }

        private void OnMainSceneUnloaded(Scene scene)
        {
            if (activationType != ActivationType.OnLoadMainScene) return;
            if (scene.name != _mainScene) return;

            CurrentAction?.Invoke();
        }

        private void Update()
        {
            if (_Player == null || activationType != ActivationType.Range) return;

            float distance = Vector3.Distance(_Player.transform.position, transform.position);


            if (distance < activationRange && !_inRange)
            {
                CurrentAction?.Invoke();

                _inRange = true;

#if UNITY_EDITOR
                SwitchRangeColor();
                Invoke(nameof(SwitchRangeColor), 0.5f);
#endif
            }
            else if (distance > activationRange && _inRange)
            {
                CurrentAction?.Invoke();

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
                CurrentAction?.Invoke();
            }
        }

        private void OnPlayerExitGate(Collider other)
        {
            if (activationType == ActivationType.Collision)
            {
                CurrentAction?.Invoke();
            }
        }
    }
}

