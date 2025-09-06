# FPS游戏优化指南

## 🎯 针对快速FPS游戏的摄像机控制优化

### 主要改进

1. **移除输入平滑**: 去除了可能造成延迟的平滑处理
2. **原始输入模式**: 直接使用鼠标输入，获得最即时的响应
3. **帧率无关控制**: 确保在不同帧率下保持一致的灵敏度体验
4. **提高默认灵敏度**: 从2调整到100，更适合FPS游戏

### 配置选项

#### PlayerCameraSystem中的设置
- `useRawInput = true`: 启用原始输入模式（推荐）
- `frameRateIndependent = true`: 启用帧率无关控制（推荐）

#### PlayerMovementConfig中的设置
- `mouseSensitivity = 100f`: FPS游戏推荐灵敏度
- `adsSensitivityMultiplier = 0.5f`: 瞄准时的灵敏度倍率

### 使用方法

#### 在代码中动态调整
```csharp
// 获取摄像机系统
var cameraSystem = playerController.CameraSystem;

// 切换到原始输入模式（最跟手）
cameraSystem.SetRawInputMode(true);

// 启用帧率无关控制
cameraSystem.SetFrameRateIndependent(true);
```

#### 在Inspector中调整
1. 选择PlayerCameraSystem组件
2. 勾选"Use Raw Input"
3. 勾选"Frame Rate Independent"
4. 在PlayerMovementConfig中调整Mouse Sensitivity

### 灵敏度推荐值

| 游戏类型 | 推荐灵敏度 | 说明 |
|---------|-----------|------|
| 竞技FPS | 80-120 | 快速反应，精确瞄准 |
| 休闲FPS | 60-100 | 平衡舒适度和反应速度 |
| 探索类 | 40-80 | 更平滑的视角转换 |

### 进阶优化

#### 1. 针对不同输入设备优化
```csharp
// 检测输入设备类型并调整设置
if (Input.mousePresent)
{
    cameraSystem.SetRawInputMode(true);
}
else if (Input.GetJoystickNames().Length > 0)
{
    // 手柄可能需要一些平滑处理
    cameraSystem.SetRawInputMode(false);
}
```

#### 2. 根据帧率动态调整
```csharp
void Update()
{
    // 低帧率时可能需要不同的处理方式
    if (Application.targetFrameRate < 60)
    {
        cameraSystem.SetFrameRateIndependent(true);
    }
}
```

#### 3. 添加灵敏度配置文件
可以为不同玩家创建个性化的灵敏度配置：
```csharp
[System.Serializable]
public class PlayerSensitivityProfile
{
    public string profileName;
    public float generalSensitivity;
    public float adsSensitivity;
    public bool useRawInput;
}
```

### 测试建议

1. **延迟测试**: 在高帧率下测试鼠标输入到视角变化的延迟
2. **精度测试**: 测试小幅度鼠标移动的精确性
3. **一致性测试**: 在不同帧率下测试灵敏度是否一致
4. **舒适度测试**: 长时间游戏的舒适度评估

### 常见问题解决

#### Q: 灵敏度感觉过高或过低
A: 调整PlayerMovementConfig中的mouseSensitivity值

#### Q: 在不同帧率下感觉不一致
A: 确保frameRateIndependent设置为true

#### Q: 仍然感觉有延迟
A: 确保useRawInput设置为true，并检查是否有其他系统在处理输入

#### Q: 瞄准时需要不同的灵敏度
A: 使用ADSSensitivityMultiplier来设置瞄准时的灵敏度倍率

### 性能考虑

- 原始输入模式减少了计算开销
- 帧率无关控制确保了一致的用户体验
- 避免了不必要的平滑计算，提升了性能
