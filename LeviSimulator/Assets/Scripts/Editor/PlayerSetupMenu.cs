using PlayerControllers.Refactored;
using PlayerControllers.Refactored.Data;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// 编辑器菜单项
    /// </summary>
    public static class PlayerSetupMenu
    {
        [MenuItem("Tools/Player Controller/Setup New Player Controller")]
        public static void SetupNewPlayerController()
        {
            var selectedObject = Selection.activeGameObject;
            if (selectedObject == null)
            {
                Debug.LogError("Please select a GameObject in the hierarchy first!");
                return;
            }
            
            PlayerSetupWizard.SetupPlayer(selectedObject);
        }
        
        [MenuItem("Tools/Player Controller/Create Player Movement Config")]
        public static void CreatePlayerMovementConfig()
        {
            var config = ScriptableObject.CreateInstance<PlayerMovementConfig>();
            
            string path = EditorUtility.SaveFilePanelInProject(
                "Save Player Movement Config", 
                "PlayerMovementConfig", 
                "asset", 
                "Choose where to save the config file");
                
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = config;
                
                Debug.Log($"PlayerMovementConfig created at {path}");
            }
        }
    }
}