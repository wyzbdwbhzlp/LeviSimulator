using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities.Danmaku
{
    

    public class DanmakuManager : MonoBehaviour
    {
        public static DanmakuManager Instance;

        [Header("弹幕相关")]
        public RectTransform danmakuContainer;      // UI容器
        public DanmakuMessage danmakuPrefab;        // 弹幕预制体
        public float messageSpeed = 200f;           // 像素/秒
        public int maxLines = 5;                    // 弹幕最大行数
        [Header("弹幕样式")]
        public float outlineWidth = 0.2f;           // 描边宽度
        public Color outlineColor = Color.black;    // 描边颜色

        private Queue<DanmakuMessage> messagePool = new();
        private int currentLineIndex = 0;
        

        void Awake()
        {
            Instance = this;
        }
        

        [Button("释放弹幕")]
        public void ShowMessage(string content)
        {
            var msg = GetMessage();
            
            msg.text.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
            msg.text.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
            
            msg.Play(content, messageSpeed, GetLineY());
        }

        private float GetLineY()
        {
            float lineHeight = danmakuPrefab.GetComponent<RectTransform>().sizeDelta.y;
            float y = -lineHeight * currentLineIndex;
            currentLineIndex = (currentLineIndex + 1) % maxLines;
            return y;
        }

        private DanmakuMessage GetMessage()
        {
            DanmakuMessage msg;
            if (messagePool.Count > 0)
            {
                msg = messagePool.Dequeue();
                msg.gameObject.SetActive(true);
            }
            else
            {
                msg = Instantiate(danmakuPrefab, danmakuContainer);
            }

            return msg;
        }

        public void RecycleMessage(DanmakuMessage msg)
        {
            msg.gameObject.SetActive(false);
            messagePool.Enqueue(msg);
        }
    }
    

   

}