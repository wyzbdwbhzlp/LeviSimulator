using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventBus.NewEventBus
{
    public class EventBus:Singleton<EventBus>
    {
        private readonly Dictionary<EventNums, Action<object>> _eventDictionary = new ();
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(this.gameObject);
        }
        public void Subscribe(EventNums eventType, Action<object> listener)
        {
            if (listener == null) return;
            if (!_eventDictionary.TryGetValue(eventType, out var existing))
            {
                _eventDictionary[eventType] = listener;
            }
            else
            {
                _eventDictionary[eventType] = existing + listener;
            }
        }
        public void Publish<T>(EventNums eventType, T arg)
        {
            if (_eventDictionary.TryGetValue(eventType, out var value))
            {
                value?.Invoke(arg);
            }
            
        }
        public void Unsubscribe(EventNums eventType, Action<object> listener)
        {
            if (listener == null) return;
            if (_eventDictionary.ContainsKey(eventType))
            {
                _eventDictionary[eventType]-= listener;
            }
        }
    }
}