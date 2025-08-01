using System;
using System.Collections.Generic;
using System.Linq;
using DataCalculate.DataCalculate;

namespace DataCalculate
{
    /// <summary>
    /// 计算管道
    /// </summary>
    public class CalculatePipeline<T> : ICalculatePipeline where T : IConvertible
    {
        private Type _valueType=null;
        
        private bool _needRecalculate = true;
        private T _lastValue;//上一次计算的值

        private readonly Dictionary<int, List<CalculateHandler<T>>> _calculationLayersDic = new();
       
       

        public void InitPipeline(Type valueType)
        {
            _valueType = valueType;
        }

        public Type ValueType => _valueType;
        public void ClearPipeline()//一般销毁时调用
        {
            _calculationLayersDic.Clear();
            _valueType = null;
        }

        public string[] GetHandlerIDs()
        {
            return _calculationLayersDic.Values
                .SelectMany(layer => layer.Select(handler => handler.HandlerID))
                .ToArray();
        }

        /// <summary>
        /// 添加Handler到管道
        /// </summary>
        private string AddHandler(CalculateHandler<T> handler)
        {
            if (_valueType == null)
            {
                _valueType = typeof(T);
            }
            _needRecalculate= true;
            
            // 检查并添加到对应的计算层
            var belongCalculationLayer= handler.BelongCalculationLayer;
            if (!_calculationLayersDic.TryGetValue(belongCalculationLayer, out var handlerList))
            {
                handlerList = new List<CalculateHandler<T>>();
                _calculationLayersDic[belongCalculationLayer] = handlerList;
                handlerList.Add(handler);
            }
            else
            {
                handlerList.Add(handler);
            }

            return handler.HandlerID;
        }
        private void RemoveHandler(CalculateHandler<T> handler)
        {
            var belongCalculationLayer = handler.BelongCalculationLayer;
            if (_calculationLayersDic.TryGetValue(belongCalculationLayer, out var handlerList))
            {
                handlerList.Remove(handler);
                if (handlerList.Count == 0)
                {
                    _calculationLayersDic.Remove(belongCalculationLayer);
                }
            }
            _needRecalculate= true;
        }
        private T ExecuteCalculate(T inputValue)
        {
            if (_needRecalculate)
            {
                T result = inputValue;
                
                var sortedLayers = _calculationLayersDic.Keys.OrderBy(k => k);
                
                foreach (var layerKey in sortedLayers)
                {
                    var handlersInLayer = _calculationLayersDic[layerKey];
                    foreach (var handler in handlersInLayer)
                    {
                        result = handler.ExecuteCalculate(result);
                    }
                }

                _lastValue = result;
                _needRecalculate = false;
                return result;
            }
            else
            {
                return _lastValue;
            }
        }

        string ICalculatePipeline.AddHandler(object handler)
        {
            if (handler is CalculateHandler<T> typedHandler)
            {
                AddHandler(typedHandler);
                return typedHandler.HandlerID;
            }

            return null;
        }

        void ICalculatePipeline.RemoveHandler(object handler)
        {
            if (handler is CalculateHandler<T> typedHandler)
            {
                RemoveHandler(typedHandler);
            }
        }

        object ICalculatePipeline.ExecuteCalculate(object initialValue)
        {
            return ExecuteCalculate((T)initialValue);
        }
    }
}