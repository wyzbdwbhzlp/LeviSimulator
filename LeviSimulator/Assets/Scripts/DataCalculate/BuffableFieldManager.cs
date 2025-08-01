using System;
using System.Collections.Generic;
using System.Reflection;
using DataCalculate.DataCalculate;
using UnityEngine;
using Utilities;

namespace DataCalculate
{
    public class BuffableFieldManager : Singleton<BuffableFieldManager>
    {
        Dictionary<(UnityEngine.Object target, FieldInfo field), ICalculatePipeline>  _buffMap = new();
        private readonly Dictionary<string, (MonoBehaviour target, FieldInfo field, object handler)> _handlerMap = new();
        public void ScanBuffableFields(MonoBehaviour targetScript)
        {
            var fields=BuffableFieldCache.GetBuffableFields(targetScript.GetType());
            
            foreach (var field in fields)
            {
                if (!Attribute.IsDefined(field, typeof(BuffableAttribute))) continue;

                var key = (target: targetScript, field: field);
                if (_buffMap.ContainsKey(key)) continue;
                
                try
                {
                    var pipelineType = typeof(CalculatePipeline<>).MakeGenericType(field.FieldType);
                    if (Activator.CreateInstance(pipelineType) is ICalculatePipeline pipeline)
                    {
                        pipeline.InitPipeline(field.FieldType);
                        _buffMap[key] = pipeline;
                        LogUtil.Log($"已添加可Buff字段：{field.Name}，类型：{field.FieldType}");
                    }
                }
                catch (Exception ex)
                {
                    LogUtil.LogError($"为字段 {field.Name} 创建CalculatePipeline失败: {ex.Message}");
                }
            }
        }

        public void AddBuffHandler<T>(MonoBehaviour target, FieldInfo field, CalculateHandler<T> handler) where T : IConvertible
        {
            var key = (target, field);
            if (_buffMap.TryGetValue(key, out var pipeline))
            {
                if(pipeline.ValueType!=handler.HandlerType)
                {
                    LogUtil.LogError($"添加BuffHandler时，BuffHandler的指定计算类型{field.FieldType} 与欲被修饰的字段 {field.Name} 的类型 {pipeline.ValueType} 不匹配");
                    return;
                }
                var handlerId=pipeline.AddHandler(handler);
                _handlerMap[handlerId] = (target, field, handler);
            }
            else
            {
                LogUtil.LogWarning($"未找到可Buff字段：{field.Name}");
            }
        }
        public void RemoveBuffHandler(MonoBehaviour target, FieldInfo field, string handlerID)
        {
            var key = (target, field);
            if (_handlerMap.TryGetValue(handlerID, out var info))
            {
                if(target != info.target || field != info.field)
                {
                    LogUtil.LogError($"尝试移除的BuffHandler与目标或字段不匹配: {handlerID}");
                    return;
                }
                var pipeline = _buffMap[key];
                pipeline.RemoveHandler(info.handler);
            }
            else
            {
                LogUtil.LogWarning($"未找到可被移除BuffHandler的字段：{field.Name}");
            }
        }
        public object GetBuffedValue<T>(MonoBehaviour target, FieldInfo field)where T : IConvertible
        {
            var key = (target, field);
            if (_buffMap.TryGetValue(key, out var pipeline))
            {
                if (pipeline.ValueType != typeof(T))
                {
                    LogUtil.LogError($"请求的类型 {typeof(T)} 与字段 {field.Name} 的类型 {pipeline.ValueType} 不匹配");
                    return null;
                }
                
                var originalValue = field.GetValue(target);
                return (T)pipeline.ExecuteCalculate(originalValue);
            }

            LogUtil.LogWarning($"未找到可被Buff的字段：{field.Name}，将返回原始值");
            return (T)field.GetValue(target);
        }

        public void UnregisterTarget(MonoBehaviour target)
        {
            var fields = BuffableFieldCache.GetBuffableFields(target.GetType());
            
            foreach (var field in fields)
            {
                if (Attribute.IsDefined(field, typeof(BuffableAttribute)))
                {
                    var key = (target: (UnityEngine.Object)target, field: field);
                    if (_buffMap.ContainsKey(key))
                    {
                        var handlerIDs= _buffMap[key].GetHandlerIDs();
                        foreach (var handlerID in handlerIDs)
                        {
                            if (_handlerMap.TryGetValue(handlerID, out var info) && info.target == target && info.field == field)
                            {
                                _handlerMap.Remove(handlerID);
                                LogUtil.Log($"已从 HandlerMap 移除目标 '{target.name}' 的 Handler ID: {handlerID}");
                            }
                        }
                        _buffMap[key].ClearPipeline();
                        _buffMap.Remove(key);
                        LogUtil.Log($"已从 BuffMap 移除目标 '{target.name}' 的 Buffable 字段: {field.Name}");
                    }
                    else
                    {
                        LogUtil.LogWarning($"未找到 Buffable 字段: {field.Name} 在 BuffMap 中");
                    }
                }
            }
            
        }


    }
    /// <summary>
    /// 未来考虑替换成存在硬盘的预烘培缓存（
    /// </summary>
    public static class BuffableFieldCache 
    {
        private static readonly Dictionary<Type, FieldInfo[]> cache = new();

        public static FieldInfo[] GetBuffableFields(Type type)
        {
            if (cache.TryGetValue(type, out var fields))
                return fields;

            fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var buffableFields = Array.FindAll(fields, f => f.IsDefined(typeof(BuffableAttribute), true));
            cache[type] = buffableFields;
            return buffableFields;
        }
    }
}