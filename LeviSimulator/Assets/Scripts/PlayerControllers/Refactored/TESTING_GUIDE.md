测试场景设置指南
================

## 快速设置步骤

### 1. 场景基础设置

1. 创建一个新的测试场景
2. 添加基础几何体：
   - 地面：创建一个大的Plane或Cube作为地面
   - 墙壁：创建几个Cube作为墙壁用于测试滑墙功能
   - 平台：创建一些不同高度的平台用于测试跳跃

### 2. Layer配置

在Project Settings → Layers中设置：
```
Layer 8: Ground
Layer 9: Wall  
Layer 10: Platform
```

为对应的GameObject设置正确的Layer：
- 地面物体 → Ground Layer
- 墙壁物体 → Wall Layer
- 平台物体 → Platform Layer

### 3. 玩家设置

#### 3.1 Player GameObject结构
```
Player
├── Model (玩家模型)
├── Main Camera
└── PlayerController (脚本)
```

#### 3.2 必要组件
Player GameObject需要包含：
- `Rigidbody` (Mass: 1, Drag: 0, Angular Drag: 0.05)
- `CapsuleCollider` (半径: 0.5, 高度: 2)
- `PlayerController` (新系统的主控制器)

#### 3.3 摄像机设置
Main Camera应该是Player的子物体，位置设置为：
- Position: (0, 1.6, 0) - 模拟眼部高度
- Rotation: (0, 0, 0)

### 4. 输入系统配置

#### 4.1 Input Actions Asset
确保场景中有EventSystem，并且PlayerController引用了正确的Input Actions Asset。

#### 4.2 输入映射检查
验证以下输入映射：
- WASD/左摇杆 → Move
- 鼠标移动/右摇杆 → Look  
- 空格键/A按钮 → Jump
- Shift键/左肩键 → Run
- Ctrl键/右摇杆按下 → Crouch

### 5. PlayerMovementConfig设置

创建并配置PlayerMovementConfig ScriptableObject：

#### 5.1 基础移动参数
```
Walk Speed: 4.0
Run Speed: 8.0  
Crouch Speed: 2.0
Acceleration: 10.0
Air Acceleration: 2.0
Friction: 10.0
```

#### 5.2 跳跃参数
```
Jump Force: 12.0
Gravity: 20.0
Fall Multiplier: 2.5
Low Jump Multiplier: 2.0
```

#### 5.3 地面检测参数
```
Ground Check Radius: 0.4
Ground Check Distance: 0.1
Ground Layer Mask: Ground (Layer 8)
```

#### 5.4 相机参数
```
Mouse Sensitivity: 2.0
Enable Raw Input: true
Max Look Angle: 80.0
Smooth Time: 0.0 (FPS模式建议为0)
```

#### 5.5 滑墙参数
```
Wall Run Threshold Speed: 6.0
Wall Run Minimum Speed: 2.0
Wall Max Distance: 1.0
Wall Jump Force: 15.0
Wall Jump Bounce Force: 8.0
Wall Speed Multiplier: 1.2
Wall Friction: 2.0
Minimum Height For Wall Run: 1.5
Wall Layer Mask: Wall (Layer 9)
```

### 6. 测试场景布局建议

#### 6.1 基础移动测试区域
- 大片平坦地面用于测试走路、跑步、蹲伏
- 不同材质的地面测试摩擦力差异

#### 6.2 跳跃测试区域  
- 不同高度的平台测试跳跃力度
- 长距离跳跃测试空中控制

#### 6.3 滑墙测试区域
- 垂直墙面，高度至少5米
- L型或U型墙面结构测试转角滑墙
- 不同角度的斜面测试滑墙检测精度

#### 6.4 综合测试区域
- 结合跳跃、滑墙、爬坡的复杂地形
- 模拟实际游戏关卡的挑战设计

### 7. 调试设置

#### 7.1 启用Gizmos显示
在Scene视图中启用Gizmos，可以看到：
- 绿色球体：地面检测范围
- 紫色球体：滑墙检测范围  
- 红色射线：墙面法线
- 蓝色射线：滑墙方向

#### 7.2 Inspector监视
在运行时观察PlayerController的Inspector面板：
- Current State: 当前状态
- Is Grounded: 是否着地
- Horizontal Speed: 水平速度
- Is Wall Running: 是否在滑墙

#### 7.3 控制台日志
打开Console窗口观察状态变化日志。

### 8. 性能测试建议

#### 8.1 帧率测试
- 启用Stats面板或使用Profiler
- 确保在目标帧率下稳定运行
- 特别关注FixedUpdate的性能

#### 8.2 输入延迟测试
- 测试鼠标输入到视角变化的延迟
- 确保Raw Input模式下延迟最小

#### 8.3 物理精度测试
- 测试高速移动时的碰撞检测精度
- 验证滑墙检测在各种角度下的准确性

### 9. 常见问题排查

#### 9.1 角色穿透地面
- 检查Rigidbody的Collision Detection设置
- 确保地面Collider没有设置为Trigger
- 验证GroundLayerMask设置正确

#### 9.2 滑墙无法触发
- 确保墙面有Collider且Layer设置正确
- 检查WallLayerMask配置
- 验证角色达到了WallRunThresholdSpeed

#### 9.3 摄像机抖动
- 确保摄像机更新在LateUpdate中进行
- 检查SmoothTime设置，FPS游戏建议设为0
- 验证EnableRawInput是否开启

#### 9.4 输入无响应
- 检查EventSystem是否存在且启用
- 验证Input Actions Asset是否正确分配
- 确保Input System Package已安装

### 10. 优化建议

#### 10.1 碰撞优化
- 使用简单的几何形状作为Collider
- 合理设置Physics层级矩阵避免不必要检测
- 考虑使用Mesh Collider的Convex选项

#### 10.2 渲染优化
- 合理设置LOD系统
- 使用遮挡剔除
- 优化光照设置

#### 10.3 脚本优化
- 缓存组件引用
- 避免在Update中进行昂贵操作
- 使用对象池管理临时对象

通过以上设置，你应该能够完整测试新的模块化3C系统的所有功能。
