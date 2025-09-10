using System;
using System.Collections.Generic;
using CollectibleEcho;
using HUD;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace UIManager
{
    public class MainUIManager:Singleton<MainUIManager>
    {
        
        [LabelText("回声UI视窗")][SerializeField][UIComponent]private EchoDisplayViewUI echoViewUI; // 回声UI视窗
        [LabelText("E交互按钮")][SerializeField][HUD]private InteractionHUD interactionHUD; // E交互按钮
        [Header("运行时参数")]
        [ShowInInspector]private Dictionary<Type, (IUIComponent,GameObject,bool)> _uiComponentsDic = new Dictionary<Type, (IUIComponent, GameObject,bool)>();
        [ShowInInspector]private Dictionary<Type, IHUDComponent> _hudComponentsDic = new();
        [ShowInInspector]private Stack<Type> _uiComponentStack = new Stack<Type>(); // UI组件栈，用于管理UI组件的显示和隐藏
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
        public void Initialize()
        {
            InitUiComponentsDic();
            InitHUDComponentsDic();
        }
        public static T ShowUIComponent<T>() where T : class, IUIComponent
        {
            return Instance?.ShowUIComponent(typeof(T)) as T;
        }
    
        public static void HideUIComponent<T>() where T : class, IUIComponent
        {
            Instance?.HideUIComponent(typeof(T));
        }
    
        public static T ShowHUDComponent<T>() where T : class, IHUDComponent
        {
            return Instance?.ShowHUDComponent(typeof(T)) as T;
        }
    
        public static void HideHUDComponent<T>() where T : class, IHUDComponent
        {
            Instance?.HideHUDComponent(typeof(T));
        }

        protected void InitUiComponentsDic()
        {
            var UIfields = GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in UIfields)
            {
                if (field.GetCustomAttributes(typeof(UIComponentAttribute), false).Length > 0)
                {
                    (IUIComponent,GameObject,bool) value = (null, null,false);
                    var uiComponent = field.GetValue(this) as IUIComponent;
                    if (uiComponent != null)
                    {
                        value.Item1 = uiComponent;
                        if (uiComponent is MonoBehaviour monoBehaviour)
                        {
                            value.Item2= monoBehaviour.gameObject;
                            value.Item2.SetActive(false);
                            value.Item3 = false; 
                            _uiComponentsDic.Add(field.FieldType, value);
                        }
                        else
                        {
                            LogUtil.LogError($"UI组件 {uiComponent} 不是 MonoBehaviour，无法获取 GameObject", true);
                        }
                    }else
                    {
                        LogUtil.LogError($"UI组件 {field.Name} 未设置或为 null", true);
                    }
                }
            }
        }
        protected void InitHUDComponentsDic()
        {
            var hudFields = GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            foreach (var field in hudFields)
            {
                if (field.GetCustomAttributes(typeof(HUDAttribute), false).Length > 0)
                {
                    var hudComponent = field.GetValue(this) as IHUDComponent;
                    if (hudComponent != null)
                    {
                        if (hudComponent is MonoBehaviour monoBehaviour)
                        {
                            if (hudComponent.IsDefaultHide)
                            {
                                hudComponent.HideHUD();
                            }
                            _hudComponentsDic.Add(field.FieldType, hudComponent);
                        }
                        else
                        {
                            LogUtil.LogError($"HUD组件 {hudComponent} 不是 MonoBehaviour，无法管理", true);
                        }
                    }
                    else
                    {
                        LogUtil.LogError($"HUD组件 {field.Name} 未设置或为 null", true);
                    }
                }
            }
        }


        protected IUIComponent ShowUIComponent(Type componentType)
        {
            if (_uiComponentsDic.TryGetValue(componentType, out var component))
            {
                component.Item2.SetActive(true);
                component.Item3 = true;
                _uiComponentStack.Push(componentType);
                return component.Item1;
            }
            
            LogUtil.LogError($"UI组件 {componentType.FullName} 未找到。", true);
            return null;
        }
        protected void HideUIComponent(Type componentType)
        {
            if (_uiComponentsDic.TryGetValue(componentType, out var component))
            {
                component.Item1.CloseUIPanel();
                component.Item2.SetActive(false);
                component.Item3 = false;
                if (_uiComponentStack.Count > 0 && _uiComponentStack.Peek() == componentType)
                {
                    _uiComponentStack.Pop();
                }
            }
            else
            {
                LogUtil.LogError($"UI组件 {componentType.FullName} 未找到。", true);
            }
        }
        protected void PopOneUIComponent()
        {
            if (_uiComponentStack.Count > 0)
            {
                var componentType = _uiComponentStack.Pop();
                HideUIComponent(componentType);
            }
            else
            {
                LogUtil.LogWarning("UI组件栈为空，无法弹出组件。");
            }
        }
        protected IHUDComponent ShowHUDComponent(Type componentType) 
        {
            if(_hudComponentsDic.TryGetValue(componentType, out var hudComponent))
            {
                hudComponent.IsHUDEnabled = true;
                hudComponent.ShowHUD();
                return hudComponent;
            }

            return null;
        }
        protected void HideHUDComponent(Type componentType) 
        {
            if(_hudComponentsDic.TryGetValue(componentType, out var hudComponent))
            {
                hudComponent.IsHUDEnabled = false;
            }
        }
        
    }
    public class UIComponentAttribute: System.Attribute { }
    public class HUDAttribute: System.Attribute { }
}