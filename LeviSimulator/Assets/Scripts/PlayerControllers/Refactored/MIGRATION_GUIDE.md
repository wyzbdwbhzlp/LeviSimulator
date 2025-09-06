使用指南 - 从旧3C系统迁移到新模块化系统
===========================================

## 迁移步骤

### 1. 禁用旧系统
在Unity Scene中，找到Player GameObject上的以下组件并禁用它们：
- PlayerMovementController
- PlayerCameraController
- PlayerInputRouter  
- PlayerWallRunController

### 2. 设置新系统
1. 在Player GameObject上添加 `PlayerController` 组件
2. 创建 `PlayerMovementConfig` ScriptableObject Asset:
   - 右键点击Project窗口 → Create → Player → Movement Config
   - 命名为 "PlayerMovementConfig"
   - 配置所有参数（见下方配置指南）

### 3. 配置新系统
将新创建的PlayerMovementConfig拖拽到PlayerController的Config字段中。

## 配置参数对照表

### 基础移动参数
```
旧系统 → 新系统
walkSpeed → WalkSpeed
runSpeed → RunSpeed  
acceleration → Acceleration
airAcceleration → AirAcceleration
friction → Friction
gravity → Gravity
jumpForce → JumpForce
```

### 相机参数
```
mouseSensitivity → MouseSensitivity
smoothTime → SmoothTime (如需要)
enableRawInput → EnableRawInput (建议设为true)
maxLookAngle → MaxLookAngle
```

### 滑墙参数
```
wallRunThresholdSpeed → WallRunThresholdSpeed
wallRunMinimumSpeed → WallRunMinimumSpeed
wallMaxDistance → WallMaxDistance
wallJumpForce → WallJumpForce
wallJumpBounceForce → WallJumpBounceForce
wallSpeedMultiplier → WallSpeedMultiplier
wallFriction → WallFriction
minimumHeightForWallRun → MinimumHeightForWallRun
```

### LayerMask配置
```
groundMask → GroundLayerMask
wallMask → WallLayerMask
```

## 新架构优势

### 1. 模块化设计
- 每个系统职责单一，便于维护
- 可以独立开发和测试各个系统
- 支持运行时启用/禁用特定功能

### 2. 事件驱动
- 输入系统与其他系统解耦
- 方便添加新的输入响应逻辑
- 便于实现输入重映射

### 3. 配置驱动
- 所有参数集中在ScriptableObject中
- 支持运行时修改参数
- 便于创建不同的移动配置预设

### 4. 状态管理
- 清晰的状态转换逻辑
- 易于添加新状态
- 状态之间的依赖关系明确

## 调试功能

### 1. Inspector调试
PlayerController组件提供实时状态显示：
- 当前状态
- 移动速度
- 是否着地
- 滑墙状态

### 2. 可视化调试
启用Gizmos可查看：
- 地面检测范围（绿色球体）
- 滑墙检测范围（紫色球体）
- 墙面法线和滑行方向（红色和蓝色射线）

### 3. 控制台日志
系统会输出关键状态变化：
- 状态切换
- 滑墙开始/结束
- 跳跃和着地事件

## 扩展指南

### 添加新状态
1. 创建继承自`PlayerStateBase`的新状态类
2. 实现`OnEnter`、`OnUpdate`、`OnExit`方法
3. 在`PlayerStateMachine`中注册新状态
4. 在需要的地方添加状态转换逻辑

### 添加新系统
1. 创建实现`IPlayerSystem`接口的新系统类
2. 在`PlayerController`中添加系统初始化
3. 如需要，在`PlayerMovementConfig`中添加相关配置参数

### 自定义输入处理
1. 在`PlayerInputEvents`中添加新事件
2. 在`PlayerInputSystem`中处理输入并触发事件
3. 在相关系统中订阅和处理事件

## 性能优化建议

### 1. FPS优化
- 启用RawInput模式以获得最低延迟
- 调整MouseSensitivity以适配高DPI
- 禁用相机平滑以获得即时响应

### 2. 物理优化
- 合理设置FixedUpdate频率
- 使用Physics.CheckSphere代替复杂的碰撞检测
- 优化LayerMask配置以减少不必要的碰撞检测

### 3. 内存优化
- 使用对象池管理临时Vector3对象
- 缓存组件引用避免GetComponent调用
- 合理配置ScriptableObject避免重复实例化

## 常见问题解决

### Q: 角色移动感觉延迟
A: 检查EnableRawInput是否开启，确保SmoothTime设置较小

### Q: 滑墙检测不准确
A: 调整WallMaxDistance和WallLayerMask，确保墙面Collider设置正确

### Q: 状态切换异常
A: 检查状态转换条件，使用Inspector实时查看状态变化

### Q: 地面检测误判
A: 调整GroundCheckRadius和GroundLayerMask，确保地面Collider标记正确

## 后续开发建议

1. 考虑添加声音系统集成
2. 实现移动特效系统（如跑步粒子、滑墙特效）
3. 添加体力系统限制连续滑墙
4. 实现更复杂的空中机动（如二段跳、空中冲刺）
5. 添加移动数据记录和回放功能用于调试
