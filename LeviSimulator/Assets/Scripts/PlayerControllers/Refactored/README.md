# 重构后的玩家3C系统

## 📋 概述

这是一个完全重构的Unity玩家3C（Character, Camera, Control）系统，采用模块化设计，具有清晰的职责分离和良好的可扩展性。

## 🏗️ 架构设计

### 核心原则
- **单一职责原则**：每个系统只负责特定功能
- **开闭原则**：易于扩展新功能，无需修改现有代码
- **依赖倒置**：通过接口解耦，降低系统间依赖
- **事件驱动**：使用事件系统实现松耦合通信

### 系统架构
```
PlayerController (主协调器)
├── PlayerInputSystem (输入处理)
├── PlayerMovementSystem (移动逻辑)
├── PlayerCameraSystem (视角控制)
├── PlayerStateMachine (状态管理)
└── PlayerRuntimeData (运行时数据)
```

## 📁 文件结构

```
Refactored/
├── Core/                    # 核心接口和基础类
│   ├── IPlayerSystem.cs     # 系统基础接口
│   ├── IState.cs           # 状态接口
│   └── PlayerInputEvents.cs # 输入事件系统
├── Data/                    # 数据结构
│   ├── PlayerMovementConfig.cs  # 移动配置（ScriptableObject）
│   └── PlayerRuntimeData.cs     # 运行时数据
├── Systems/                 # 各个子系统
│   ├── PlayerInputSystem.cs     # 输入处理系统
│   ├── PlayerMovementSystem.cs  # 移动系统
│   ├── PlayerCameraSystem.cs    # 摄像机系统
│   └── PlayerStateMachine.cs    # 状态机实现
├── States/                  # 状态实现
│   ├── PlayerStateBase.cs  # 状态基类
│   ├── IdleState.cs        # 站立状态
│   ├── WalkingState.cs     # 行走状态
│   ├── RunningState.cs     # 跑步状态
│   ├── JumpingState.cs     # 跳跃状态
│   ├── FallingState.cs     # 下落状态
│   ├── CrouchingState.cs   # 蹲伏状态
│   └── SlidingState.cs     # 滑铲状态
├── PlayerController.cs      # 主控制器
└── PlayerSetupWizard.cs    # 设置向导
```

## 🚀 快速开始

### 1. 设置新的玩家控制器

#### 方法一：使用菜单（推荐）
1. 在Hierarchy中选择你的玩家GameObject
2. 点击菜单：`Tools > Player Controller > Setup New Player Controller`
3. 系统会自动添加必要的组件并配置

#### 方法二：手动设置
1. 将`PlayerController.cs`添加到你的玩家GameObject
2. 确保GameObject有以下组件：
   - `Rigidbody`
   - `CapsuleCollider`
   - `PlayerInput`
3. 在子对象中添加摄像机并添加`PlayerCameraSystem`组件

### 2. 创建移动配置
1. 点击菜单：`Tools > Player Controller > Create Player Movement Config`
2. 选择保存位置
3. 在PlayerController的Inspector中分配这个配置文件

### 3. 配置输入
确保PlayerInput组件分配了正确的Input Actions资产，包含以下动作：
- Move (Vector2)
- Look (Vector2)
- Jump (Button)
- Crouch (Button)
- Sprint (Button)
- Grapple (Button)

## 🎮 支持的状态

- **Idle**: 静止状态
- **Walking**: 行走状态
- **Running**: 跑步状态（按住Sprint键）
- **Jumping**: 跳跃状态
- **Falling**: 下落状态
- **Crouching**: 蹲伏状态
- **Sliding**: 滑铲状态（高速时蹲伏）

## ⚙️ 主要特性

### 输入系统
- 统一的事件驱动输入处理
- 支持多种输入设备
- 易于添加新的输入动作

### 移动系统
- 精确的地面检测
- 平滑的移动加速/减速
- 支持不同表面的物理特性
- 自动碰撞体高度调整

### 摄像机系统
- 平滑的鼠标控制
- 视角限制
- 动态倾斜效果
- 摄像机震动支持

### 状态机
- 简化的状态转换逻辑
- 清晰的状态转换条件
- 易于添加新状态
- 完整的状态生命周期管理

## 🔧 扩展指南

### 添加新状态
1. 继承`PlayerStateBase`类
2. 实现`CheckTransitions()`和`HandleMovement()`方法
3. 在`PlayerController.InitializeStates()`中注册新状态

```csharp
public class CustomState : PlayerStateBase
{
    public CustomState(PlayerController controller) : base(controller) { }
    
    protected override void CheckTransitions()
    {
        // 状态转换逻辑
    }
    
    protected override void HandleMovement()
    {
        // 移动处理逻辑
    }
}
```

### 添加新系统
1. 实现`IPlayerSystem`接口
2. 在`PlayerController.InitializeSystems()`中注册
3. 使用事件系统与其他系统通信

```csharp
public class CustomSystem : MonoBehaviour, IPlayerSystem
{
    public bool IsEnabled { get; set; } = true;
    
    public void Initialize(PlayerController playerController) { }
    public void Update() { }
    public void FixedUpdate() { }
    public void Cleanup() { }
}
```

## 🐛 调试

PlayerController的Inspector面板显示实时调试信息：
- 当前状态
- 移动速度
- 是否接地
- 当前速度向量
- 输入值

## 📈 性能优化

- 使用对象池管理临时对象
- 条件编译优化调试代码
- 缓存常用组件引用
- 避免不必要的字符串操作

## 🔄 从旧系统迁移

1. 备份当前项目
2. 使用设置向导自动移除旧组件
3. 重新配置移动参数
4. 测试所有功能

## ⚠️ 注意事项

- 确保Input System包已安装
- PlayerInput组件需要正确的Input Actions配置
- 摄像机必须是玩家的子对象
- 地面Layer需要正确设置

## 🤝 贡献

如需添加功能或报告问题，请遵循以下原则：
- 保持代码简洁清晰
- 添加适当的注释
- 遵循现有的命名约定
- 编写单元测试（如果适用）
