using System.Collections.Generic;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace CollectibleEcho.SO
{
    [CreateAssetMenu(fileName = "CollectibleEchoSO", menuName = "ScriptableObjects/CollectibleEchoSO", order = 1)]
    public class CollectibleEchoSO : ScriptableObject
    {
        [ListDrawerSettings(CustomAddFunction = "CustomAddFunction")]
        [SerializeField]    
        List<CollectibleEcho> collectibleEchoes=new List<CollectibleEcho>();
        public List<CollectibleEcho> CollectibleEchoes => collectibleEchoes;
        private void CustomAddFunction()
        {
            CollectibleEcho newEcho = new CollectibleEcho();
            newEcho.echoID = collectibleEchoes.Count + 1; // 自动分配ID
            collectibleEchoes.Add(newEcho);
        }
#if UNITY_EDITOR
        
        [Button("保存所有回声插图路径")]
        private void SaveAllEchoIllustratiosnsPath()
        {
            foreach (var echo in collectibleEchoes)
            {
                echo.SaveEchoIllustratiosnsPath();
            }
        }
#endif
        public CollectibleEcho GetCollectibleEchoByID(int id)
        {
            return collectibleEchoes.Find(echo => echo.echoID == id);
        }
    }
    [System.Serializable]
    public class CollectibleEcho
    {
        [SerializeField][ReadOnly] public int echoID;
        [SerializeField] private string echoTitle;
        [SerializeField][TextArea(4,15)] private string echoText;
        [SerializeField] private Sprite echoIllustrationsSprite;
        [ReadOnly] [SerializeField] private string echoIllustrationsPath;
        
        public string EchoTitle => echoTitle;
        public string EchoText => echoText;
        public Sprite GetEchoIllustrationsSprite()
        {
            if (echoIllustrationsSprite == null && !string.IsNullOrEmpty(echoIllustrationsPath))
            {
                echoIllustrationsSprite = Resources.Load<Sprite>(echoIllustrationsPath);
            }
            return echoIllustrationsSprite;
        }
#if UNITY_EDITOR
        
        [Button]
        public void SaveEchoIllustratiosnsPath()
        {
            if (echoIllustrationsSprite != null)
            {
                echoIllustrationsPath = AssetDatabase.GetAssetPath(echoIllustrationsSprite);
            }
        }
#endif
    }
}
