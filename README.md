# Maomao AI Desktop Pet

[English](README.md) | [中文](README.zh-CN.md)

> A living companion on your Windows desktop — cute, interactive, and growing into a full AI pet.

![Maomao showcase](picture/Display%20Images1.png)

![Desktop menu showcase](picture/Display%20Images2.png)

---

## What is this?

**Maomao AI Desktop Pet** is a Windows desktop companion app. Maomao is not just a looping GIF — it is designed as a long-term deskmate with:

- **Presence** — lives on your desktop with transparent, always-on-top rendering  
- **Interaction** — click, drag, feed, pet, chat, and play  
- **Growth** — mood, energy, affection, outfits, room, and collections  
- **AI personality** — remembers you, talks in character, and can accompany focus work  

The current build is **V0.1**: the pet already *feels alive* on the desktop. AI chat, deep care systems, room/dress-up gameplay, and mini-games are planned next.

Product direction in one line:

> **Desktop pet = companionship + care + interaction + mini-games + desktop tools + AI personality**

---

## Features

### Available in V0.1

| Area | Status |
|------|--------|
| Borderless transparent always-on-top window (hidden from taskbar) | Done |
| Sprite-frame animation from transparent PNGs | Done |
| State machine: Idle / Walk / Sleep / Drag / Click | Done |
| Click reactions + mash-click escalation | Done |
| Drag & drop landing feedback | Done |
| Random idle behaviors & speech bubbles | Done |
| Full product context menu (many items still placeholders) | Done |
| App icon from `picture/icon.png` | Done |
| Auto placeholder art when frames are missing | Done |

### Planned roadmap

| Version | Focus |
|---------|--------|
| **V0.2** | Hunger, mood, energy, affection, feeding, petting, sleep, XP/coins |
| **V0.3** | Mini-games, daily quests, achievements, outfits, room & furniture, collections, random events |
| **V0.4** | AI chat, long-term memory, diary, proactive talk, personality engine |
| **V1.0** | Full ecosystem: world exploration, multi-pet, seasons, desktop tools (Pomodoro, gentle reminders) |

Design principle:

> **Aliveness > interaction > care > games > numbers**  
> Don’t turn the pet into a pure grind / gacha loop.

---

## Screenshots & branding

| Asset | Path | Use |
|-------|------|-----|
| Showcase 1 | [`picture/Display Images1.png`](picture/Display%20Images1.png) | Character + speech bubble |
| Showcase 2 | [`picture/Display Images2.png`](picture/Display%20Images2.png) | Desktop pet + context menu |
| App icon | [`picture/icon.png`](picture/icon.png) | Source icon (converted to `.ico` for the exe) |
| Animation reference | [`picture/move.png`](picture/move.png) | State / interaction concept sheet |
| Runtime frames | [`picture/deskpet_transparent_frames/`](picture/deskpet_transparent_frames/) | Transparent cutout PNG sequence |

---

## Tech stack

| Item | Choice |
|------|--------|
| Language | C# |
| UI | WPF |
| Runtime | .NET 8 (Windows) |
| IDE | Visual Studio 2022 |
| Output | Native `MaomaoDesktopPet.exe` |
| Architecture (V0.1) | Lightweight single window + sprite animator + state machine |

Why WPF: mature transparent windows, easy drag/click handling, fast iteration for a desktop pet MVP.

---

## Requirements

- Windows 10 / 11  
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or Visual Studio 2022 with .NET desktop workload)  
- Optional: Visual Studio 2022 for GUI debugging and publishing  

---

## Quick start

### Option A — Visual Studio 2022

1. Open `MaomaoDesktopPet.sln`  
2. Set configuration to **Debug** or **Release**  
3. Press **F5** (or Build → Build Solution)  
4. Maomao appears near the bottom-right of the screen  

### Option B — Command line

```powershell
cd "C:\Projects\Maomao AI Desktop Pet"
dotnet run --project src\MaomaoDesktopPet -c Release
```

Built exe (after Release build):

```text
src\MaomaoDesktopPet\bin\Release\net8.0-windows\MaomaoDesktopPet.exe
```

---

## How to use (V0.1)

| Action | Result |
|--------|--------|
| **Left click** | Click reaction (`嗯？` → mash lines) |
| **Drag** | Held / slide frames + speech; release plays drop |
| **Right click** | Full menu (Interact / Feed / Pet / Games / Chat / Outfit / Room / …) |
| **Idle** | Occasional walk, sleep, or bubble text |
| **Reload animation** (menu) | Re-scan frame folder without restarting the app |
| **Exit** (menu) | Quit |

Unimplemented menu items currently show a short “coming soon” bubble.

---

## Animation assets

Frames live in a **flat folder** (not subfolders):

```text
picture\deskpet_transparent_frames\
```

They are copied into the build output as `Assets\Pet\` on compile.

### Filename → state mapping

| Prefix | Pet state |
|--------|-----------|
| `idle_` | Idle |
| `walk_` | Walk |
| `sleep_` | Sleep |
| `inter_drag_hold_` / `inter_drag_slide_` | Drag |
| `inter_drop_` | Drop after drag |
| `inter_click_` | Click |
| `inter_mash_` | Mash click |
| `run_` / `jump_` / `yawn_` / `stretch_` / `lie_down_` / `exp_*` / `inter_hover_*` | Reserved for later behaviors |

Naming example: `idle_01_cutout.png`, `walk_02_cutout.png`.

Tips:

- Prefer true **alpha transparency** (not a painted white background).  
- Large source images are decoded at ~280px width at runtime to save memory.  
- After adding/replacing PNGs, rebuild or use **Reload animation**.

---

## Project structure

```text
Maomao AI Desktop Pet/
├── MaomaoDesktopPet.sln
├── README.md                 # English (default)
├── README.zh-CN.md           # Chinese
├── picture/
│   ├── Display Images1.png
│   ├── Display Images2.png
│   ├── icon.png
│   ├── move.png
│   └── deskpet_transparent_frames/
├── docs/superpowers/specs/   # Design notes
└── src/MaomaoDesktopPet/
    ├── Assets/
    │   ├── icon.ico          # Application icon
    │   └── Pet/              # Copied frames + notes
    ├── Core/
    │   ├── PetState.cs
    │   ├── PetController.cs
    │   ├── SpriteLoader.cs
    │   ├── SpriteAnimator.cs
    │   └── PlaceholderFactory.cs
    ├── MainWindow.xaml(.cs)  # Transparent pet window + menu
    ├── App.xaml(.cs)
    └── MaomaoDesktopPet.csproj
```

---

## Design philosophy

Maomao should feel like **a small creature living on your PC**, not a mobile grind game glued to the desktop.

Priority order:

1. Aliveness (idle motion, bubbles, time-aware presence)  
2. Instant feedback (click / drag / hover)  
3. Care loops (feed, mood, affection)  
4. Playful extras (games, room, outfits)  
5. Numbers (XP, coins, collections)

Avoid: constant popups, punishment death, or “must log in every day or lose everything”.

When offline for a while, the preferred tone is:

> “You’re back! I missed you.”  
> — not —  
> “Your pet starved.”

---

## Vision (product map)

```text
Maomao
├── Daily interaction (pet, feed, click, drag, events)
├── AI brain (chat, memory, personality, proactive talk)
├── Care / growth (stats, level unlocks)
├── Dress-up & room
├── Mini-games & quests
├── Story (diary, dreams, memories)
├── Desktop tools (focus timer, gentle reminders)
└── World (exploration, multi-pet, seasons)
```

Details and MVP notes: [`docs/superpowers/specs/2026-09-30-maomao-v01-design.md`](docs/superpowers/specs/2026-09-30-maomao-v01-design.md)

---

## Contributing / local tips

- Keep V0.1 changes focused on **aliveness and input feel**.  
- Put new frames into `picture/deskpet_transparent_frames/` with clear prefixes.  
- Don’t commit build output (`bin/`, `obj/`, `.vs/`) — covered by `.gitignore`.  
- Prefer small, testable modules when adding AI / room / games later.

---

## License

Not specified yet. Add a license file before public distribution.

---

## Language

- **Default README:** English — [`README.md`](README.md)  
- **Chinese README:** [`README.zh-CN.md`](README.zh-CN.md)
