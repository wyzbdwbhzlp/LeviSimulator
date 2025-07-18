using System;
namespace EventBus
{
    public class EventListener
    {
        private int _priority;
        private Func<bool> _condition;
        private BaseEvent _cascadeEvent;
        private Func<BaseEvent, bool> _invalidationCondition;
        
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority => _priority;
        /// <summary>
        /// 触发条件
        /// </summary>
        public Func<bool> Condition =>  _condition;
        /// <summary>
        /// 失效条件
        /// </summary>
        public Func<BaseEvent, bool> InvalidationCondition => _invalidationCondition;
        /// <summary>
        /// 触发的级联事件
        /// </summary>
        public BaseEvent CascadeEvent => _cascadeEvent;
        

        public EventListener(int priority = 0,
            Func<bool> condition = null,
            BaseEvent cascade = null,
            Func<BaseEvent, bool> invalidation = null)
        {
            _priority = priority;
            _condition = condition;
            _cascadeEvent = cascade;
            _invalidationCondition = invalidation;
        }
    }
    
    
    public class EventListener<T> : EventListener where T : BaseEvent
    {
        private Action<T> _callback;
        public Action<T> Callback=> _callback;

        public EventListener(Action<T> cb, int priority = 0,
            Func<bool> condition = null,
            BaseEvent cascade = null,
            Func<T, bool> invalidation = null)
            : base(priority, condition, cascade, e => invalidation != null && invalidation((T)e))
        {
            _callback = cb;
        }
    }

}