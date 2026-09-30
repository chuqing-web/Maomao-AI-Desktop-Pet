# 毛毛 AI 桌宠

Windows 桌面宠物（C# WPF / .NET 8），用 Visual Studio 2022 编译 exe。

## 打开项目

用 VS2022 打开 `MaomaoDesktopPet.sln`，或：

```powershell
cd "c:\Projects\Maomao AI Desktop Pet"
dotnet run --project src\MaomaoDesktopPet
```

## 放入序列帧

把 PNG 放到：

```text
src\MaomaoDesktopPet\Assets\Pet\idle\
src\MaomaoDesktopPet\Assets\Pet\walk\
src\MaomaoDesktopPet\Assets\Pet\sleep\
src\MaomaoDesktopPet\Assets\Pet\drag\
src\MaomaoDesktopPet\Assets\Pet\click\
```

命名如 `001.png`、`002.png`。放入后重新运行，或右键菜单「重新加载动画」。

## V0.1 已有

- 透明置顶无边框窗口（不进任务栏）
- 状态机：Idle / Walk / Sleep / Drag / Click
- 点击连点文案、拖拽反应、随机走动/睡觉/气泡
- 完整右键菜单（多数功能占位）
- 无帧时占位绘制
