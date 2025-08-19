using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using PlayerControllers.PlayerCharacterStatusStrategy;
using UnityEngine;
using Utilities;

namespace PlayerControllers.PlayerCharacterStatusStrategyHFSM
{
    public static class PlayerCharacterStatusStrategyFactory
    {
        private static readonly Dictionary<Type, HierarchicalBaseState> StateDictionary = new();
        private static bool _isInitialized = false;
        
        /// <summary>
        ///  初始化状态策略工厂
        /// </summary>
        public static void InitializeStates()
        {
            var strategyTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && typeof(HierarchicalBaseState).IsAssignableFrom(t));

            foreach (var type in strategyTypes)
            {
                var instance = (HierarchicalBaseState)Activator.CreateInstance(type);
                StateDictionary[type] = instance;
            }

            foreach (var hierarchicalBaseState in StateDictionary)
            {
                LogUtil.Log($"已注册状态策略: {hierarchicalBaseState.Key.Name}");
            }
            
            _isInitialized = true;
        }


        public static T GetState<T>() where T : HierarchicalBaseState
        {
            if(_isInitialized == false)
            {
                LogUtil.LogError("PlayerCharacterStatusStrategyFactory未初始化，请先调用InitializeStates方法");
            }
            if (StateDictionary.TryGetValue(typeof(T), out var state))
            {
                return (T)state;
            }

            throw new KeyNotFoundException($"未找到 {typeof(T).Name} ");
        }
    }
}