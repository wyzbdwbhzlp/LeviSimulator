using System;
using UnityEngine;

namespace EventBus
{
    public static class EventBusExtensions
    {
        // 简化事件监听语法
        public static void On<T>(this MonoBehaviour component, Action<T> callback, int priority = 0) where T : BaseEvent
        {
            EventManager.Instance.AddListener<T>(callback, priority);
        }
    
        // 简化事件触发语法
        public static void Trigger<T>(this MonoBehaviour component, T eventData = null) where T : BaseEvent
        {
            eventData = eventData ?? Activator.CreateInstance<T>();
            EventManager.Instance.TriggerEvent(eventData);
        }
    }
}