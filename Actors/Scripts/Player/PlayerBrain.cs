using System.Collections;
using System.Collections.Generic;

using Topacai.Inputs;
using Topacai.Utils.GameObjects;

using UnityEngine;
using UnityEngine.InputSystem;

namespace Topacai.Player
{
    public interface IPlayerModule
    {
        public object Controller { get; }
    }
    [System.Serializable]
    public class PlayerReferences
    {
        private Dictionary<System.Type, object> _modules = new();
        public bool RegisterModule<T>(T module) where T : class, IPlayerModule
        {
            if (GetModule<T>() != null)
            {
                Debug.LogError("Already exists a component of this type");
                return false;
            }

            _modules.Add(typeof(T), module);
            return true;
        }

        public T GetModule<T>() where T : class, IPlayerModule
        {
            _modules.TryGetValue(typeof(T), out var module);
            return (T)module;
        }
    }

    public class PlayerBrain : MonoBehaviour
    {
        public const bool SINGLEPLAYER_MODE = true;

        protected static PlayerBrain sp_Player;

        public static PlayerBrain SP_Player
        {
            get
            {
                if (sp_Player == null)
                {
                    Debug.LogWarning("SP_Player is null");
                }
                return sp_Player;
            }

            protected set
            {
                sp_Player = value;
            }
        }

        public static List<PlayerBrain> Players { get; protected set; }

        [field: SerializeField] public PlayerReferences PlayerReferences { get; protected set; }

        [SerializeField] protected InputActionAsset _inputAsset;
        [SerializeField] protected InputHandler _playerInputs;

        public InputHandler InputHandler
        {
            get
            {
                if (_playerInputs == null)
                {
                    CreateInputs();
                    throw new System.Exception("Inputs not initialized for this player");
                }
                return _playerInputs;
            }
        }

        protected virtual void CreateInputs()
        {
            if (_playerInputs != null || _inputAsset == null) return;

            var playerInput = gameObject.AddComponent<PlayerInput>();

            playerInput.actions = _inputAsset;

            gameObject.AddComponent<InputHandler>();

        }

        protected virtual void Awake()
        {
            if (SINGLEPLAYER_MODE)
            {
                if (SP_Player != null && SP_Player != this)
                {
                    Destroy(this);
                }
                else
                {
                    SP_Player = this;
                }
            }
        }

        protected virtual void Initialize()
        {
            CreateInputs();
        }

        protected virtual void Start()
        {
            CreateInputs();
        }

        #region Factory Pattern

        public static GameObject CreatePlayerPrefab(GameObject playerPrefab)
        {
            var instance = Instantiate(playerPrefab);

            if (instance.TryGetComponent(out PlayerBrain playerBrain))
            {
                Players.Add(playerBrain);

                playerBrain.Initialize();

                if (playerBrain.InputHandler == null)
                {
                    playerBrain.CreateInputs();
                }
            }
            else
            {
                throw new System.Exception("Player prefab must have a PlayerBrain component");
            }

            return instance;
        }

        public static GameObject CreatePlayerPrefab(GameObject playerPrefab, Vector3 pos)
        {
            var instance = CreatePlayerPrefab(playerPrefab);
            instance.transform.position = pos;
            return instance;
        }

        public static GameObject CreatePlayerPrefab(GameObject playerPrefab, Transform pos) => CreatePlayerPrefab(playerPrefab, pos.position);

        #endregion

    }
}

