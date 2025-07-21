using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Utilities.Danmaku
{

    public class DanmakuMessage : MonoBehaviour
    {
        public TextMeshProUGUI text;
        private RectTransform rectTransform;
        private float speed;

        void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }

        public void Play(string content, float speed, float y)
        {
            text.text = content;
            this.speed = speed;

            LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransform);

            float startX = DanmakuManager.Instance.danmakuContainer.rect.width- rectTransform.rect.width;
            rectTransform.anchoredPosition = new Vector2(startX, y);

            StopAllCoroutines();
            StartCoroutine(MoveOut());
        }

        private IEnumerator MoveOut()
        {
            while (rectTransform.anchoredPosition.x + rectTransform.rect.width > -rectTransform.rect.width)
            {
                rectTransform.anchoredPosition += Vector2.left * speed * Time.deltaTime;
                yield return null;
            }

            DanmakuManager.Instance.RecycleMessage(this);
        }
    }
}