using UnityEngine;
using System.Collections.Generic;
using GlobalGameManager;
using UIManager;

namespace GlobalGameManager
{
    public class GlobalManager : MonoBehaviour
    {
        [SerializeField]private SceneEnum initialScene;
        
        
        
        private static GlobalManager _instance;

        public static GlobalManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GlobalManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("GlobalManager");
                        _instance = go.AddComponent<GlobalManager>();
                        DontDestroyOnLoad(go);
                    }
                }

                return _instance;
            }
        }

        [Header("管理器引用")]
        public SceneLoadManager sceneLoadManager;
        public PlayerSpawnManager playerSpawnManager;
        public GameStateManager gameStateManager;
        public MainUIManager mainUIManager;
        

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeManagers();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeManagers()
        {
            // 初始化各个管理器
            if (sceneLoadManager == null)
                sceneLoadManager = gameObject.AddComponent<SceneLoadManager>();

            if (playerSpawnManager == null)
                playerSpawnManager = gameObject.AddComponent<PlayerSpawnManager>();

            if (gameStateManager == null)
                gameStateManager = gameObject.AddComponent<GameStateManager>();
            
            mainUIManager=MainUIManager.Instance;
            if(mainUIManager==null)
                mainUIManager= gameObject.AddComponent<MainUIManager>();
            
            playerSpawnManager.Initialize();
            gameStateManager.Initialize();
            mainUIManager.Initialize();
            sceneLoadManager.LoadScene(initialScene);
        }
    }
}