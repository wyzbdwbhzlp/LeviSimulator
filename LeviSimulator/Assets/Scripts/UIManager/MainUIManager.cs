using System;
using System.Collections.Generic;
using System.Linq;
using CollectibleEcho;
using HUD;
using Manager;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using GlobalGameManager;
using System.Reflection;
using PlayerControllers.Refactored;
using PlayerControllers.Refactored.Systems;
using SettingPanel; // 新增

namespace UIManager
{
    //tip UI管理器，负责UI组件的显示与隐藏，支持按特性自动加载预制体
    //todo 自动加载预制体逻辑可能需要分离，防止MainUIManager臃肿
    public class MainUIManager : Singleton<MainUIManager>
    {
        [Header("运行时参数")] [ShowInInspector]
        private Dictionary<Type, (IViewComponent, GameObject, bool)> _uiComponentsDic = new ();//tip Type-(脚本实例,GameObject,是否显示)

        [ShowInInspector] private Dictionary<Type, IHUDComponent> _hudComponentsDic = new();
        [ShowInInspector] private Stack<Type> _uiComponentStack = new Stack<Type>(); // UI组件栈，用于管理UI组件的显示和隐藏

        [Header("实例化父节点（可选）")] [SerializeField]
        private Transform uiRoot; // 用于放置按特性实例化出来的UI
        
        private UIAssetLoader _uiAssetLoader;
        
        private static List<Type> _cachedViewComponentTypes;


        protected override void Awake()
        {
            base.Awake();
            _uiAssetLoader = new UIAssetLoader();
            DontDestroyOnLoad(gameObject);
        }

        public void Initialize()
        {
            EnsureCachedViewComponentTypes<ViewComponentAttribute>();
            //InitUiComponentsDic();
            
            InitUiComponentsFromTypes();
            InitHUDComponentsDic();
            // 订阅游戏状态变化（若可用）
            var gm = GlobalManager.Instance;
            if (gm != null && gm.gameStateManager != null)
            {
                gm.gameStateManager.OnStateChanged += OnGameStateChanged;
            }
        }

        public static T ShowUIComponent<T>() where T : class, IViewComponent
        {
            return Instance?.ShowUIComponent(typeof(T)) as T;
        }

        public static void HideUIComponent<T>() where T : class, IViewComponent
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

        private void OnDestroy()
        {
            var gm = GlobalManager.Instance;
            if (gm != null && gm.gameStateManager != null)
            {
                gm.gameStateManager.OnStateChanged -= OnGameStateChanged;
            }
        }

        private void Update()
        {
            // ESC 关闭顶层UI
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                PopOneUIComponent();
            }
        }

        /// <summary>
        /// 扫描所有已加载程序集，找到标记了 ViewComponentAttribute 的类型并缓存。
        /// </summary>
        private static void EnsureCachedViewComponentTypes<T>() where T : Attribute
        {
            if (_cachedViewComponentTypes != null) return;

            var types = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var asm in assemblies)
            {
                Type[] asmTypes;
                try
                {
                    asmTypes = asm.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // 部分程序集可能加载失败，尽量使用能加载的类型
                    asmTypes = ex.Types.Where(t => t != null).ToArray();
                }
                catch
                {
                    // 忽略无法读取的程序集
                    continue;
                }

                foreach (var t in asmTypes)
                {
                    if (t.IsAbstract) continue; // 排除抽象类
                    // 仅类/引用类型（排除接口/枚举/值类型）
                    if (!t.IsClass) continue;

                    var viewAttr = t.GetCustomAttribute<T>(false); 
                    if (viewAttr != null)
                    {
                        types.Add(t);
                    }
                }
            }

            _cachedViewComponentTypes = types;
        }
           /// <summary>
    /// 根据缓存的类型列表实例化并注册 UI 组件。
    /// </summary>
    private void InitUiComponentsFromTypes()
    {
        foreach (var componentType in _cachedViewComponentTypes)
        {
            // 忽略未实现 IViewComponent 的类型
            if (!typeof(IViewComponent).IsAssignableFrom(componentType))
            {
                Debug.LogError($"类型 {componentType.FullName} 标记了 ViewComponentAttribute，但未实现 IViewComponent。");
                continue;
            }

            // 如果已经存在于字典中则跳过（避免重复初始化）
            if (_uiComponentsDic.ContainsKey(componentType)) continue;

            // 尝试先通过已存在的字段注入（如果当前类声明了对应字段），回写字段是可选的：
            // 找到 this 类型中声明的字段，其 FieldType 是 componentType 或其基类/接口
            FieldInfo matchedField = null;
            var allFields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var f in allFields)
            {
                var ft = f.FieldType;
                // 如果字段类型与 componentType 相同，或者字段类型为接口/基类且 componentType 可赋值给字段类型
                if (ft == componentType || ft.IsAssignableFrom(componentType))
                {
                    matchedField = f;
                    break;
                }
            }

            // 读取特性实例（类型级特性）
            var viewAttr = componentType.GetCustomAttribute<ViewComponentAttribute>(false) as ViewComponentAttribute;

            // 通过特性或其他路径实例化组件（你原来的 TryLoadUIComponentByAttribute）
            IViewComponent uiComponent = null;
            GameObject go = null;
            try
            {
                uiComponent = TryLoadUIComponentByAttribute(componentType, viewAttr, out go, GetParent(viewAttr));
            }
            catch (Exception ex)
            {
                Debug.LogError($"实例化 UI 组件 {componentType.FullName} 时抛出异常：{ex}");
                continue;
            }

            if (uiComponent == null)
            {
                Debug.LogError($"UI组件类型 {componentType.FullName} 实例化失败（TryLoadUIComponentByAttribute 返回 null）。");
                continue;
            }

            // 回写到匹配到的字段（如果有且字段为空）
            if (matchedField != null)
            {
                var existing = matchedField.GetValue(this) as IViewComponent;
                if (existing == null)
                {
                    // 如果字段类型比真实类型更抽象，也应该能赋值（注意可能需要装箱/转换）
                    matchedField.SetValue(this, uiComponent);
                }
            }

            // 如果是 MonoBehaviour，获取 GameObject 并执行初始化
            if (uiComponent is MonoBehaviour mb)
            {
                go = go ?? mb.gameObject;
                go.SetActive(false); // 按原逻辑默认隐藏
                _uiComponentsDic[componentType] = (uiComponent, go, false);
            }
            else
            {
                Debug.LogError($"UI组件 {uiComponent} 不是 MonoBehaviour，无法获取 GameObject（类型：{componentType.FullName}）。");
            }
        }
    }
        // private void InitUiComponentsDic()
        // {
        //     var UIfields = GetType()
        //         .GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        //     foreach (var field in UIfields)
        //     {
        //         var componentType = field.FieldType;
        //         var viewAttr = componentType.GetCustomAttribute(typeof(ViewComponentAttribute), false) as ViewComponentAttribute;
        //         if (viewAttr == null) continue; // 该字段类型未标记 ViewComponentAttribute，跳过
        //
        //         if (!typeof(IViewComponent).IsAssignableFrom(componentType))
        //         {
        //             LogUtil.LogError($"字段 {field.Name} 的类型 {componentType.Name} 标记了 ViewComponentAttribute 但未实现 IViewComponent 接口", true);
        //             continue;
        //         }
        //
        //         (IViewComponent, GameObject, bool) value = (null, null, false);
        //         var uiComponent = field.GetValue(this) as IViewComponent;
        //
        //         // 若字段尚未被手动赋值，则尝试按类特性实例化
        //         if (uiComponent == null)
        //         {
        //             uiComponent = TryLoadUIComponentByAttribute(viewAttr, out var go, GetParent(viewAttr));
        //             if (uiComponent != null)
        //             {
        //                 field.SetValue(this, uiComponent); // 回写字段
        //             }
        //         }
        //
        //         if (uiComponent != null)
        //         {
        //             value.Item1 = uiComponent;
        //             if (uiComponent is MonoBehaviour monoBehaviour)
        //             {
        //                 value.Item2 = monoBehaviour.gameObject;
        //                 value.Item2.SetActive(false);
        //                 value.Item3 = false;
        //                 _uiComponentsDic.Add(componentType, value);
        //             }
        //             else
        //             {
        //                 LogUtil.LogError($"UI组件 {uiComponent} 不是 MonoBehaviour，无法获取 GameObject", true);
        //             }
        //         }
        //         else
        //         {
        //             LogUtil.LogError($"UI组件字段 {field.Name} (类型 {componentType.Name}) 实例化失败", true);
        //         }
        //     }
        // }

        private void InitHUDComponentsDic()
        {
            var hudFields = GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in hudFields)
            {
                var componentType = field.FieldType;
                var hudAttr = componentType.GetCustomAttribute(typeof(HUDAttribute), false) as HUDAttribute;
                if (hudAttr == null) continue; // 类型未标记 HUDAttribute

                if (!typeof(IHUDComponent).IsAssignableFrom(componentType))
                {
                    LogUtil.LogError($"字段 {field.Name} 的类型 {componentType.Name} 标记了 HUDAttribute 但未实现 IHUDComponent 接口", true);
                    continue;
                }

                var hudComponent = field.GetValue(this) as IHUDComponent;
                if (hudComponent == null)
                {
                    hudComponent = TryLoadHUDComponentByAttribute(componentType, hudAttr, out var go, GetParent(hudAttr));
                    if (hudComponent != null)
                    {
                        field.SetValue(this, hudComponent);
                    }
                }

                if (hudComponent != null)
                {
                    if (hudComponent is MonoBehaviour monoBehaviour)
                    {
                        if (hudComponent.IsDefaultHide)
                        {
                            hudComponent.HideHUD();
                        }
                        _hudComponentsDic.Add(componentType, hudComponent);
                    }
                    else
                    {
                        LogUtil.LogError($"HUD组件 {hudComponent} 不是 MonoBehaviour，无法管理", true);
                    }
                }
                else
                {
                    LogUtil.LogError($"HUD组件字段 {field.Name} (类型 {componentType.Name}) 实例化失败", true);
                }
            }
        }
        

        private Transform GetParent(BasePrefabAttribute attr)
        {
            var root = uiRoot != null ? uiRoot : transform;
            if (attr == null || string.IsNullOrEmpty(attr.ParentPath)) return root;
            var found = root.Find(attr.ParentPath);
            return found != null ? found : root;
        }

//         private static GameObject LoadPrefab(string prefabPath, string prefabName)
//         {
//             //todo 替换为yooasset加载
//             GameObject prefab = null;
//             if (!string.IsNullOrWhiteSpace(prefabPath))
//             {
//                 prefab = Resources.Load<GameObject>(prefabPath);
//                 if (prefab == null)
//                     LogUtil.LogError($"Resources 未找到预制体路径: {prefabPath}", true);
//             }
//             if (!string.IsNullOrWhiteSpace(prefabName)&& prefab == null)
//             {
//                 LogUtil.LogWarning($"通过{prefabPath}加载失败，尝试按名称加载 {prefabName}");
//                 // 名称加载：约定在 Resources/UI 或 Resources/HUD 下
//                 prefab = Resources.Load<GameObject>($"UI/{prefabName}") ??
//                          Resources.Load<GameObject>($"HUD/{prefabName}") ??
//                          Resources.Load<GameObject>(prefabName);
// #if UNITY_EDITOR
//                 if (prefab == null)
//                 {
//                     LogUtil.LogWarning($"{prefabName} 未能通过 Resources 加载，尝试在编辑器中按名称查找（仅编辑器有效）");
//                     var guids = UnityEditor.AssetDatabase.FindAssets($"{prefabName} t:prefab");
//                     if (guids != null && guids.Length > 0)
//                     {
//                         var assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
//                         prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
//                     }
//                 }
// #endif
//                 if (prefab == null)
//                     LogUtil.LogError($"未找到名为 {prefabName} 的预制体（请放入 Resources 并传入相对路径，或在编辑器中检查名称是否匹配）", true);
//             }
//
//             return prefab;
//         }

      
        private IViewComponent TryLoadUIComponentByAttribute(Type componentType, ViewComponentAttribute attr,
            out GameObject go, Transform parent) //尝试按特性加载UI组件
        {
            go = null;
            if (attr == null) return null;
            if (componentType == null)
            {
                LogUtil.LogError("TryLoadUIComponentByAttribute 调用时 componentType 为空", true);
                return null;
            }

            if (!typeof(Component).IsAssignableFrom(componentType))
            {
                LogUtil.LogError(
                    $"类型 {componentType.FullName} 未继承自 UnityEngine.Component，无法作为 UI 组件实例化。", true);
                return null;
            }

            var prefab = _uiAssetLoader.LoadUIPrefab(attr.PrefabPath, attr.PrefabName);
            var instantiated = InstantiateAndBind(prefab, parent, componentType, out go);
            if (instantiated == null)
            {
                if (go != null) Destroy(go);
                go = null;
                return null;
            }

            if (instantiated is IViewComponent viewComponent)
            {
                return viewComponent;
            }

            var fallback = go != null ? go.GetComponentInChildren<IViewComponent>(true) : null;
            if (fallback != null)
            {
                return fallback;
            }

            LogUtil.LogError(
                $"实例 {go?.name ?? prefab?.name ?? "<unknown>"} 上未找到 IViewComponent 实现（类型：{componentType.FullName}）。", true);
            return null;
        }

        private IHUDComponent TryLoadHUDComponentByAttribute(Type componentType, HUDAttribute attr, out GameObject go,
            Transform parent) //尝试按特性加载HUD组件
        {
            go = null;
            if (attr == null) return null;
            if (componentType == null)
            {
                LogUtil.LogError("TryLoadHUDComponentByAttribute 调用时 componentType 为空", true);
                return null;
            }

            if (!typeof(Component).IsAssignableFrom(componentType))
            {
                LogUtil.LogError(
                    $"类型 {componentType.FullName} 未继承自 UnityEngine.Component，无法作为 HUD 组件实例化。", true);
                return null;
            }

            var prefab = _uiAssetLoader.LoadUIPrefab(attr.PrefabPath, attr.PrefabName);
            var instantiated = InstantiateAndBind(prefab, parent, componentType, out go);
            if (instantiated == null)
            {
                if (go != null) Destroy(go);
                go = null;
                return null;
            }

            if (instantiated is IHUDComponent hudComponent)
            {
                return hudComponent;
            }

            var fallback = go != null ? go.GetComponentInChildren<IHUDComponent>(true) : null;
            if (fallback != null)
            {
                return fallback;
            }

            LogUtil.LogError(
                $"实例 {go?.name ?? prefab?.name ?? "<unknown>"} 上未找到 IHUDComponent 实现（类型：{componentType.FullName}）。", true);
            return null;
        }
        private static Component InstantiateAndBind(GameObject prefab, Transform parent, Type requiredType,
            out GameObject instance)
        {
            instance = null;
            if (prefab == null) return null;

            instance = Instantiate(prefab, parent, false);
            instance.name = prefab.name;
            instance.SetActive(false); // 与现有流程一致，初始化时先隐藏

            Component comp = null;
            if (requiredType != null)
            {
                comp = instance.GetComponent(requiredType);
                if (comp == null)
                {
                    comp = instance.GetComponentInChildren(requiredType, true);
                }
            }
            else
            {
                comp = instance.GetComponent<MonoBehaviour>();
            }

            if (comp == null)
            {
                LogUtil.LogError(
                    $"实例 {instance.name} 上未找到组件 {requiredType?.Name ?? nameof(MonoBehaviour)}", true);
            }

            return comp;
        }
        protected IViewComponent ShowUIComponent(Type componentType)
        {
            if (_uiComponentsDic.TryGetValue(componentType, out var component))
            {
                if (component.Item3)
                {
                    // 已经显示则直接返回，避免重复入栈
                    return component.Item1;
                }

                // 移除旧位置，推入栈顶
                RemoveFromStack(componentType);
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
                // 无论是否在栈顶，都从栈中移除该类型的所有记录
                RemoveFromStack(componentType);
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
                if (_uiComponentStack.Count == 0)
                {
                    PlayerController.LockAndHideCursor(); //关闭最后一个UI时锁定并隐藏鼠标
                }
            }
            else
            {
                var hasController = SettingController.HasInstance;
                var controller = hasController ? SettingController.Instance : null;
                if (controller == null)
                {
                    LogUtil.LogWarning("UI组件栈为空，未找到 SettingController，无法切换设置面板。");
                    return;
                }

                if (controller.IsSettingsVisible)
                {
                    controller.HideSettings();
                }
                else
                {
                    controller.ShowSettings();
                }
            }
        }

        protected IHUDComponent ShowHUDComponent(Type componentType)
        {
            if (_hudComponentsDic.TryGetValue(componentType, out var hudComponent))
            {
                hudComponent.IsHUDEnabled = true;
                hudComponent.ShowHUD();
                return hudComponent;
            }

            return null;
        }

        protected void HideHUDComponent(Type componentType)
        {
            if (_hudComponentsDic.TryGetValue(componentType, out var hudComponent))
            {
                hudComponent.IsHUDEnabled = false;
                hudComponent.HideHUD();
            }
        }

        // 运行时注册/反注册 HUD 组件，便于动态加载的 HUD 管理
        public void RegisterHUDComponent<T>(T hudComponent) where T : class, IHUDComponent
        {
            var type = typeof(T);
            if (hudComponent == null)
            {
                LogUtil.LogError($"注册 HUD 失败：{type.FullName} 实例为 null", true);
                return;
            }

            if (!_hudComponentsDic.ContainsKey(type))
            {
                _hudComponentsDic.Add(type, hudComponent);
                if (hudComponent.IsDefaultHide)
                {
                    hudComponent.HideHUD();
                }
            }
        }

        public void UnregisterHUDComponent<T>() where T : class, IHUDComponent
        {
            var type = typeof(T);
            if (_hudComponentsDic.TryGetValue(type, out var hud))
            {
                hud.HideHUD();
                _hudComponentsDic.Remove(type);
            }
        }

        public bool HasActiveUI()
        {
            return _uiComponentStack.Count > 0;
        }

        public void CloseAllUI()
        {
            // 逐一关闭，确保 CloseUIPanel 被调用
            while (_uiComponentStack.Count > 0)
            {
                var type = _uiComponentStack.Pop();
                HideUIComponent(type);
            }
        }

        private void SetAllHUDEnabled(bool enabled)
        {
            foreach (var kv in _hudComponentsDic)
            {
                kv.Value.IsHUDEnabled = enabled;
                if (!enabled)
                {
                    kv.Value.HideHUD();
                }
                else
                {
                    kv.Value.ShowHUD();
                }
            }
        }

        private void OnGameStateChanged(GameState from, GameState to)
        {
            // 这里仅做基础联动，避免侵入具体UI：
            switch (to)
            {
                case GameState.InGame:
                    // 返回游戏时，关闭残留的顶层 UI
                    CloseAllUI();
                    break;
                case GameState.Paused:
                    // 可在此处显示暂停菜单（若接入），此处暂不强制处理
                    break;
                case GameState.GameOver:
                    // 由 DeathScreenHUD 自行展示，不在此全局处理
                    break;
            }
        }

        // 从栈中移除所有匹配类型（保持其余顺序）
        private void RemoveFromStack(Type t)
        {
            if (_uiComponentStack.Count == 0) return;
            var temp = new Stack<Type>();
            while (_uiComponentStack.Count > 0)
            {
                var x = _uiComponentStack.Pop();
                if (x != t) temp.Push(x);
            }

            while (temp.Count > 0)
            {
                _uiComponentStack.Push(temp.Pop());
            }
        }

        public void RegisterViewComponent(EchoDisplayViewUI echoDisplayViewUI)
        {
            var type = typeof(EchoDisplayViewUI);
            if (echoDisplayViewUI == null)
            {
                LogUtil.LogError($"注册 UI 失败：{type.FullName} 实例为 null", true);
                return;
            }

            if (!_uiComponentsDic.ContainsKey(type))
            {
                var go = echoDisplayViewUI.gameObject;
                go.SetActive(false);
                _uiComponentsDic.Add(type, (echoDisplayViewUI, go, false));
            }
        }
    }


    public abstract class BasePrefabAttribute : Attribute
    {
        public string PrefabPath { get; }
        public string PrefabName { get; }
        public string ParentPath { get; }

        protected BasePrefabAttribute(string prefabPath = null, string prefabName = null, string parentPath = null)
        {
            PrefabPath = prefabPath;
            PrefabName = prefabName;
            ParentPath = parentPath;
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ViewComponentAttribute : BasePrefabAttribute
    {
        //tip 传入path时优先按path加载
        public ViewComponentAttribute(string prefabName = null, string prefabPath = null, string parentPath = null)
            : base(prefabPath, prefabName, parentPath)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class HUDAttribute : BasePrefabAttribute
    {
        //tip 传入path时优先按path加载
        public HUDAttribute(string prefabName = null, string prefabPath = null, string parentPath = null)
            : base(prefabPath, prefabName, parentPath)
        {
        }
    }
}