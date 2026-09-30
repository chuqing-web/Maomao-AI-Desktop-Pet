# 毛毛 AI 桌宠（Maomao AI Desktop Pet）

[English](README.md) | [中文](README.zh-CN.md)

> 住在 Windows 桌面上的小伙伴——可爱、可互动，并逐步成长为完整的 AI 桌宠。

![毛毛展示](picture/Display%20Images1.png)

![桌面菜单展示](picture/Display%20Images2.png)

---

## 这是什么？

**毛毛 AI 桌宠** 是一款 Windows 桌面陪伴程序。毛毛不只是循环播放的动图，而是按长期桌面伙伴来设计：

- **陪伴感** — 透明置顶，真正“住在”桌面上  
- **互动** — 点击、拖拽、喂食、摸摸、聊天、小游戏  
- **养成** — 心情、精力、好感、换装、房间、收藏  
- **AI 人格** — 记住你、按角色说话、陪你专注做事  

当前版本为 **V0.1**：桌宠已经能在桌面上“活起来”。AI 对话、深度养成、房间/换装玩法、小游戏等将在后续版本加入。

一句话定位：

> **桌宠 = 陪伴 + 养成 + 互动 + 小游戏 + 桌面工具 + AI 人格**

---

## 功能

### V0.1 已实现

| 模块 | 状态 |
|------|------|
| 无边框透明置顶窗口（不进任务栏） | 已完成 |
| 透明 PNG 序列帧动画 | 已完成 |
| 状态机：Idle / Walk / Sleep / Drag / Click | 已完成 |
| 点击反应 + 连点升级文案 | 已完成 |
| 拖拽与落地反馈 | 已完成 |
| 随机待机行为与气泡 | 已完成 |
| 完整产品右键菜单（多数仍为占位） | 已完成 |
| 应用图标（来自 `picture/icon.png`） | 已完成 |
| 缺少帧时自动占位绘制 | 已完成 |

### 版本规划

| 版本 | 重点 |
|------|------|
| **V0.2** | 饥饿、心情、精力、好感、喂食、摸摸、睡觉、经验/金币 |
| **V0.3** | 小游戏、每日任务、成就、换装、房间与家具、图鉴、随机事件 |
| **V0.4** | AI 聊天、长期记忆、日记、主动搭话、人格引擎 |
| **V1.0** | 完整生态：探索、多宠物、季节活动、桌面工具（番茄钟、轻提醒） |

设计原则：

> **生命感 > 互动感 > 养成感 > 游戏性 > 数值系统**  
> 不要做成挂在桌面上的纯肝度手游。

---

## 展示与品牌素材

| 素材 | 路径 | 用途 |
|------|------|------|
| 展示图 1 | [`picture/Display Images1.png`](picture/Display%20Images1.png) | 角色 + 气泡 |
| 展示图 2 | [`picture/Display Images2.png`](picture/Display%20Images2.png) | 桌宠 + 右键菜单 |
| 应用图标 | [`picture/icon.png`](picture/icon.png) | 源图标（构建时转为 `.ico`） |
| 动画参考 | [`picture/move.png`](picture/move.png) | 状态 / 互动概念图 |
| 运行序列帧 | [`picture/deskpet_transparent_frames/`](picture/deskpet_transparent_frames/) | 透明抠图 PNG 序列 |

---

## 技术栈

| 项目 | 选型 |
|------|------|
| 语言 | C# |
| UI | WPF |
| 运行时 | .NET 8（Windows） |
| IDE | Visual Studio 2022 |
| 产出 | `MaomaoDesktopPet.exe` |
| V0.1 架构 | 轻量单窗口 + 序列帧播放器 + 状态机 |

选择 WPF 的原因：透明窗口成熟、拖拽/点击好做、适合快速做出桌宠 MVP。

---

## 环境要求

- Windows 10 / 11  
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)（或安装了 .NET 桌面开发工作负载的 VS2022）  
- 可选：Visual Studio 2022 用于调试与发布  

---

## 快速开始

### 方式 A — Visual Studio 2022

1. 打开 `MaomaoDesktopPet.sln`  
2. 选择 **Debug** 或 **Release**  
3. 按 **F5**（或“生成 → 生成解决方案”）  
4. 毛毛会出现在屏幕右下角附近  

### 方式 B — 命令行

```powershell
cd "C:\Projects\Maomao AI Desktop Pet"
dotnet run --project src\MaomaoDesktopPet -c Release
```

Release 构建后的 exe：

```text
src\MaomaoDesktopPet\bin\Release\net8.0-windows\MaomaoDesktopPet.exe
```

---

## 使用说明（V0.1）

| 操作 | 效果 |
|------|------|
| **左键点击** | 点击反应（从「嗯？」到狂点文案） |
| **拖拽** | 抓起/滑动帧 + 台词；松手播放落地 |
| **右键** | 完整菜单（互动 / 喂食 / 摸摸 / 游戏 / 聊天 / 换装 / 房间 …） |
| **待机** | 偶尔走动、睡觉或冒气泡 |
| **重新加载动画** | 不重启程序即可重新扫描帧目录 |
| **退出** | 关闭程序 |

尚未实现的菜单项会短暂显示「马上就来～」。

---

## 动画资源

序列帧放在**扁平目录**（不是按子文件夹）：

```text
picture\deskpet_transparent_frames\
```

编译时会复制到输出目录的 `Assets\Pet\`。

### 文件名前缀 → 状态

| 前缀 | 宠物状态 |
|------|----------|
| `idle_` | 待机 |
| `walk_` | 走动 |
| `sleep_` | 睡觉 |
| `inter_drag_hold_` / `inter_drag_slide_` | 拖拽 |
| `inter_drop_` | 拖拽落地 |
| `inter_click_` | 点击 |
| `inter_mash_` | 狂点 |
| `run_` / `jump_` / `yawn_` / `stretch_` / `lie_down_` / `exp_*` / `inter_hover_*` | 预留给后续行为 |

命名示例：`idle_01_cutout.png`、`walk_02_cutout.png`。

建议：

- 使用真正的 **Alpha 透明**（不要只是白底画上去）。  
- 大图运行时会按约 280px 宽度解码以节省内存。  
- 增改 PNG 后请重新编译，或使用菜单「重新加载动画」。

---

## 项目结构

```text
Maomao AI Desktop Pet/
├── MaomaoDesktopPet.sln
├── README.md                 # 英文（默认）
├── README.zh-CN.md           # 中文
├── picture/
│   ├── Display Images1.png
│   ├── Display Images2.png
│   ├── icon.png
│   ├── move.png
│   └── deskpet_transparent_frames/
├── docs/superpowers/specs/   # 设计说明
└── src/MaomaoDesktopPet/
    ├── Assets/
    │   ├── icon.ico          # 应用图标
    │   └── Pet/              # 复制后的帧与说明
    ├── Core/
    │   ├── PetState.cs
    │   ├── PetController.cs
    │   ├── SpriteLoader.cs
    │   ├── SpriteAnimator.cs
    │   └── PlaceholderFactory.cs
    ├── MainWindow.xaml(.cs)  # 透明桌宠窗口 + 菜单
    ├── App.xaml(.cs)
    └── MaomaoDesktopPet.csproj
```

---

## 设计理念

毛毛应该像是**住在电脑里的小生命**，而不是贴在桌面上的肝度手游。

优先级：

1. 生命感（待机动作、气泡、时间感）  
2. 即时反馈（点击 / 拖拽 / 靠近）  
3. 养成循环（喂食、心情、好感）  
4. 趣味扩展（小游戏、房间、换装）  
5. 数值（经验、金币、图鉴）

避免：不停弹窗、饿死惩罚、或“几天不上线就全毁”。

长时间没打开后，更希望是：

> 「你回来啦！我有点想你。」  
> ——而不是——  
> 「你的宠物饿死了。」

---

## 产品愿景（功能地图）

```text
毛毛
├── 日常互动（摸摸、喂食、点击、拖拽、事件）
├── AI 大脑（对话、记忆、人格、主动搭话）
├── 养成成长（属性、等级解锁）
├── 换装与房间
├── 小游戏与任务
├── 故事（日记、梦境、回忆）
├── 桌面工具（专注计时、轻提醒）
└── 世界（探索、多宠物、季节）
```

规格说明：[`docs/superpowers/specs/2026-09-30-maomao-v01-design.md`](docs/superpowers/specs/2026-09-30-maomao-v01-design.md)

---

## 本地开发提示

- V0.1 改动优先保证**生命感与手感**。  
- 新帧放到 `picture/deskpet_transparent_frames/`，前缀清晰。  
- 不要提交 `bin/`、`obj/`、`.vs/`（已在 `.gitignore`）。  
- 后续加 AI / 房间 / 游戏时，尽量拆成可测的小模块。

---

## 许可证

尚未指定。公开发布前请补充 LICENSE。

---

## 语言版本

- **默认 README（英文）：** [`README.md`](README.md)  
- **中文 README：** [`README.zh-CN.md`](README.zh-CN.md)
