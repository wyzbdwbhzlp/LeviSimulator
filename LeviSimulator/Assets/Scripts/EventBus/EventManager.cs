using System;
using System.Collections.Generic;
using UnityEngine;

namespace EventBus
{
    public class EventManager : Singleton<EventManager>
    {
        // 使用字典来存储事件监听器:键是事件类型，值是一个List，存储具有优先级的EventListener
        private Dictionary<Type, List<EventListener>> eventListeners = new Dictionary<Type, List<EventListener>>();
        public Dictionary<Type, List<EventListener>> GetEventListeners()
        {
            return eventListeners;
        }

        //存储将要被移除的EventListener
        private List<EventListener> _removalList = new List<EventListener>();

        //是否正在移除EventListener
        private bool _isRemoving = false;
        
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
        /// <summary>
        /// 添加监听器,比如 AddListener《AEvent》(BCallback)相当于我们在用的 AEvent += BCallback
        /// 优先值越小，越早执行
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="priority"></param>
        /// <typeparam name="T"></typeparam>
        public void AddListener<T>(Action<T> callback, int priority = 0) where T : BaseEvent
        {
            Type eventType = typeof(T);

            if (!eventListeners.ContainsKey(eventType)) //如果Dic中还没有这个事件，则创建一个空的订阅者列表
            {
                eventListeners[eventType] = new List<EventListener>();
            }
            
            
            // 以索引来界定优先级
            int insertIndex = 0;
            while (insertIndex < eventListeners[eventType].Count &&
                   eventListeners[eventType][insertIndex].Priority <= priority)
            {
                insertIndex++;
            }
            
            
            EventListener listener = new EventListener<T>(callback, priority);
            eventListeners[eventType].Insert(insertIndex, listener); //将订阅者，即监听器加入进订阅者列表中
        }

        /// <summary>
        /// 移除监听器,相当于常用的-=
        /// </summary>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public void RemoveListener<T>(Action<T> callback) where T : BaseEvent
        {
            Type eventType = typeof(T);

            if (!eventListeners.TryGetValue(eventType, out var listenersList))
            {
                Debug.LogWarning($"EventManager: ①尝试移除监听器时没有找到对应的事件类型→→ {eventType}");
                return;
            }
           
            if (listenersList == null)//虽然一般不会触发这个防御性编程
            {
                    Debug.LogWarning($"EventManager: ②尝试移除监听器时没有找到对应的事件类型→→ {eventType}");
                    return;
            }

            foreach (EventListener listener in listenersList)
            {
                    if (listener is EventListener<T> specificListener && specificListener.Callback == callback)
                    {
                        if (!_isRemoving)
                        {
                            listenersList.Remove(listener);
                            return;
                        }
                        else
                        {
                            // 将其加入到删除列表中
                            _removalList.Add(listener);
                        }
                    }
            }
            
            
        }

        /// <summary>
        /// 触发事件，相当于On/DevEventHandler的Call*****
        /// </summary>
        /// <param name="eventData"></param>
        /// <typeparam name="T"></typeparam>
        public void TriggerEvent<T>(T eventData) where T : BaseEvent
        {
            Type eventType = typeof(T);
            if (eventListeners.TryGetValue(eventType, out var listeners))
            {
                if (listeners == null)
                {
                    Debug.LogWarning($"EventManager: ③尝试触发事件时没有找到对应事件类型 →→ {eventType}");
                    return;
                }

                //遍历删除列表
                _isRemoving = true; 
                
                foreach (EventListener listener in listeners)
                    
                {
                    if (_removalList.Contains(listener))
                    {
                        listeners.Remove(listener);
                    }
                }

                _isRemoving = false;
                _removalList.Clear();

                foreach (var listener in listeners)
                {
                    
                    if (listener.Condition == null || listener.Condition())// 如果未设置生效条件，或生效条件返回 true，则执行回调
                    {
                        
                        var eventListener = listener as EventListener<T>;
                        if (eventListener != null)
                        {
                            // ①执行回调函数（广播）
                            eventListener.Callback?.Invoke(eventData); 

                            // ②如有事件级联,则递归触发下一个事件
                            if (eventListener.CascadeEvent != null)
                            {
                                TriggerEvent(eventListener.CascadeEvent);
                            }

                            // ③条件失效处理
                            if (eventListener.InvalidationCondition != null && 
                                eventListener.InvalidationCondition(eventData))
                            {
                                RemoveListener<T>(eventListener.Callback);
                            }
                        }
                        
                    }
                    
                }
            }
        }
    }
}