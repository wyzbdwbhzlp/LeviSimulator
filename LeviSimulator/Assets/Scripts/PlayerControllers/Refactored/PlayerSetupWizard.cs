using UnityEngine;
using PlayerControllers.Refactored.Data;

namespace PlayerControllers.Refactored
{
    /// <summary>
    /// 玩家控制器设置向导
    /// </summary>
    public static class PlayerSetupWizard
    {
        /// <summary>
        /// 为现有的GameObject设置新的玩家控制器
        /// </summary>
        public static PlayerController SetupPlayer(GameObject targetObject)
        {
            if (targetObject == null)
            {
                Debug.LogError("Target object is null!");
                return null;
            }
            
            // 移除旧的控制器组件（如果存在）
            
            // 确保必要的组件存在
            EnsureRequiredComponents(targetObject);
            
            // 添加新的控制器
            var playerController = targetObject.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = targetObject.AddComponent<PlayerController>();
            }
            
            // 设置摄像机
            SetupCamera(targetObject);
            
            // 创建默认配置
            CreateDefaultConfig();
            
            Debug.Log("Player setup complete! Don't forget to assign the PlayerMovementConfig in the inspector.");
            return playerController;
        }
        
        private static void EnsureRequiredComponents(GameObject targetObject)
        {
            // 确保有Rigidbody
            var rigidbody = targetObject.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = targetObject.AddComponent<Rigidbody>();
                rigidbody.mass = 1f;
                rigidbody.linearDamping = 0f;
                rigidbody.angularDamping = 0f;
                rigidbody.freezeRotation = true; // 防止角色翻滚
            }
            
            // 确保有CapsuleCollider
            var collider = targetObject.GetComponent<CapsuleCollider>();
            if (collider == null)
            {
                collider = targetObject.AddComponent<CapsuleCollider>();
                collider.height = 2f;
                collider.radius = 0.5f;
                collider.center = new Vector3(0, 1f, 0);
            }
            
            // 确保有PlayerInput组件
            var playerInput = targetObject.GetComponent<UnityEngine.InputSystem.PlayerInput>();
            if (playerInput == null)
            {
                playerInput = targetObject.AddComponent<UnityEngine.InputSystem.PlayerInput>();
                Debug.LogWarning("PlayerInput component added. Please assign the Input Actions asset in the inspector.");
            }
        }
        
        private static void SetupCamera(GameObject playerObject)
        {
            // 查找或创建摄像机
            Camera playerCamera = playerObject.GetComponentInChildren<Camera>();
            
            if (playerCamera == null)
            {
                // 创建摄像机对象
                GameObject cameraObject = new GameObject("PlayerCamera");
                cameraObject.transform.SetParent(playerObject.transform);
                cameraObject.transform.localPosition = new Vector3(0, 1.6f, 0); // 头部高度
                cameraObject.transform.localRotation = Quaternion.identity;
                
                // 添加摄像机组件
                playerCamera = cameraObject.AddComponent<Camera>();
                playerCamera.fieldOfView = 75f;
                playerCamera.nearClipPlane = 0.01f;
                
                // 设置为主摄像机
                playerCamera.tag = "MainCamera";
                
                Debug.Log("Player camera created and configured.");
            }
            
            // 确保摄像机有AudioListener
            if (playerCamera.GetComponent<AudioListener>() == null)
            {
                playerCamera.gameObject.AddComponent<AudioListener>();
            }
        }
        
        private static void CreateDefaultConfig()
        {
            // 检查是否已存在默认配置
            string configPath = "Assets/PlayerMovementConfig_Default.asset";
            var existingConfig = UnityEditor.AssetDatabase.LoadAssetAtPath<PlayerMovementConfig>(configPath);
            
            if (existingConfig == null)
            {
                // 创建默认配置
                var config = ScriptableObject.CreateInstance<PlayerMovementConfig>();
                
                // 这里可以设置默认值，但由于PlayerMovementConfig的字段是私有的，
                // 我们需要在ScriptableObject中添加一个初始化方法
                
                UnityEditor.AssetDatabase.CreateAsset(config, configPath);
                UnityEditor.AssetDatabase.SaveAssets();
                
                Debug.Log($"Default PlayerMovementConfig created at {configPath}");
            }
        }
    }
}


