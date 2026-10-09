# Maomao AI Desktop Pet

[English](README.md) | [中文](README.zh-CN.md)

> A living companion on your Windows desktop — cute, interactive, and growing into a full AI pet.

<br/>

<table align="center">
  <tr>
    <td align="center" width="50%">
      <img src="picture/readme-showcase-1.png" alt="Maomao character showcase" width="300" />
      <br/>
      <sub>Meet Maomao</sub>
    </td>
    <td align="center" width="50%">
      <img src="picture/readme-showcase-2.png" alt="Desktop right-click menu" width="300" />
      <br/>
      <sub>Right-click menu</sub>
    </td>
  </tr>
</table>

<br/>

---

## What is this?

**Maomao AI Desktop Pet** (*Little Paw Companion Plan*) is a Windows desktop companion. Maomao is not a looping GIF — it is a long-term deskmate with mood, hunger, personality, room, outfits, quests, mini-games, diary, dreams, exploration, and optional cloud AI.

One-line product formula:

> **Desktop pet = companionship + care + interaction + mini-games + desktop tools + AI personality**

Design priority:

> **Aliveness > interaction > care > games > numbers**  
> Not a grind / gacha game glued to the desktop. No pet death.

Data files (next to the exe):

- `save.json` — full game save  
- `memory.json` — long-term AI memories (portable with the exe)

---

## Features at a glance

| Pillar | What’s included |
|--------|-----------------|
| **Presence** | Transparent always-on-top window, no taskbar icon, sprite states & expressions |
| **Life** | Idle walk / run / jump / lie / yawn / stretch / sleep; chase mouse; hide in corner; peek from edge |
| **Input** | Hover look, click → mash lines, drag with direction speech, drop landing, clickable speech bubbles |
| **Care** | Hunger / mood / energy / cleanliness / affection / luck / level / coins; feed; zone petting; sleep; offline decay; login streak |
| **Growth** | Level & growth stage (progress only); daily quests; achievements (incl. hidden); **all features open — no level gates** |
| **Dress & home** | Outfits + hats catalog; room furniture with buffs; wallpapers / seasonal themes |
| **Play** | Catch fish, dodge, whack-a-mole, guess mood |
| **AI & story** | Local personality chat (+ optional OpenAI-compatible API); memories; diary; dreams |
| **World** | Explore maps & loot; second pet relations |
| **Desktop tools** | 25-min focus companion; todos; water / sit / network reminders; weather (Open-Meteo) |
| **Social feel** | Birthdays; greetings by time of day; proactive bubbles; disturb & mischief modes |

Growth stages (cosmetic): **Juvenile** (&lt;10) → **Growing** (&lt;20) → **Mature** (&lt;30) → **Special** (30+).

### Soft / art-limited (honest gaps)

| Item | Notes |
|------|--------|
| Layered clothing / hat sprites | Equipped in data & catalog; not separate overlay art yet |
| System tray / click-through | Not implemented |
| Deep OS hooks (downloads, CPU heat) | Network + idle reminders only |

---

## Screenshots & assets

| Asset | Path | Use |
|-------|------|-----|
| Showcase 1 (source) | [`picture/Display Images1.png`](picture/Display%20Images1.png) | Character + bubble |
| Showcase 2 (source) | [`picture/Display Images2.png`](picture/Display%20Images2.png) | Pet + context menu |
| README pair | [`picture/readme-showcase-1.png`](picture/readme-showcase-1.png) / [`2`](picture/readme-showcase-2.png) | Unified 480×640 for docs |
| App icon | [`picture/icon.png`](picture/icon.png) | Source → `Assets/icon.ico` for the exe |
| Concept sheet | [`picture/move.png`](picture/move.png) | States / interactions reference |
| Runtime frames | [`picture/deskpet_transparent_frames/`](picture/deskpet_transparent_frames/) | Transparent cutout PNG sequence |

---

## Tech stack

| Item | Choice |
|------|--------|
| Language | C# |
| UI | WPF |
| Runtime | .NET 8 (Windows) |
| IDE | Visual Studio 2022 |
| Output | `MaomaoDesktopPet.exe` |
| Persistence | JSON via `System.Text.Json` |
| Optional AI | OpenAI-compatible HTTP chat completions |
| Weather | [Open-Meteo](https://open-meteo.com/) (no API key) |

Architecture: transparent `MainWindow` shell + `PetController` / sprite pipeline + `AppServices` domain layer (`Care`, `AI`, `Explore`, `Dreams`, `Weather`, …) + feature panels under `Views/`.

---

## Requirements

- Windows 10 / 11  
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) **or** Visual Studio 2022 with the **.NET desktop development** workload  
- Network optional (weather / cloud AI)

---

## Quick start

### Visual Studio 2022

1. Open `MaomaoDesktopPet.sln`  
2. Select **Debug** or **Release**  
3. Press **F5** (or Build → Build Solution)  
4. First launch opens onboarding (your name, pet name, personality, birthdays)  
5. Maomao appears near the bottom-right of the screen  

### Command line

```powershell
cd "C:\Projects\Maomao AI Desktop Pet"
dotnet run --project src\MaomaoDesktopPet -c Release
```

Release exe:

```text
src\MaomaoDesktopPet\bin\Release\net8.0-windows\MaomaoDesktopPet.exe
```

---

## How to use

### On the pet

| Action | Result |
|--------|--------|
| **Hover** | Curious / hover expression |
| **Left click** | `嗯？` → mash lines → frenzy |
| **Drag** | “诶诶诶！”, left/right lines, drop “啪！” |
| **Click a bubble** | Triggers feed / game / chat / dream / etc. when the bubble has an action |
| **Idle** | Walk, run, jump, sleep, stretch, chase mouse, hide, peek, random talk |
| **Right click** | Full product menu |

### Right-click menu (main)

Status & quests · Relationship · Pet · Feed · Games · Chat · Focus · Todos · Outfit · Room · Explore · Second pet · Bag · Collection · Diary · Achievements · Sleep · Weather · Reload animation · Settings · Exit  

### Settings highlights

- Owner / pet names, personality (Clingy / Tsundere / Soft / Silly / Scholar / Lazy)  
- Disturb mode: **Quiet** / **Companion** / **Active**  
- Mischief: **Off** / **Mild** / **Full**  
- Water reminder, work reminder, network reactions  
- Optional cloud AI: presets (OpenAI / DeepSeek / 通义 / Ollama), API base, key, model, temperature, max tokens, **Test connection**  
- Memories & save live beside the exe (`save.json` + `memory.json`)

### Mini-games

1. **Catch fish** — move with ←→ / A D  
2. **Dodge** — Space / W to jump  
3. **Whack-a-mole** — tap moles before time runs out  
4. **Guess mood** — pick the emotion  

Rewards grant coins, XP, and mood.

### Care loop

```text
Interact → hearts / coins / XP → dress-up / room / unlocks → new interactions → repeat
```

Foods include dried fish, milk, biscuit, cake, strawberry, apple, candy, cat can, mystery dish, and dark cuisine (mood −20 gag).

---

## Animation assets

Frames live in a **flat folder** (copied to `Assets\Pet\` on build):

```text
picture\deskpet_transparent_frames\
```

### Prefix → behavior (59 frames)

| Prefix | Behavior |
|--------|----------|
| `idle_` / `space_out_` | Idle / spacing out |
| `walk_` / `run_` / `spin_` | Locomotion |
| `jump_01_` / `jump_desk_` | Jump |
| `sleep_` / `yawn_` / `stretch_` / `lie_down_` / `sunbath_` | Rest |
| `chase_` / `hide_` / `climb_window_` / `catch_butterfly_` | Playful desktop antics |
| `clean_` / `drink_` / `play_tail_` / `play_toy_` / `sneeze_` / `look_window_` | Daily life |
| `eat_fish_` / `eat_milk_` / `eat_cake_` / `eat_strawberry_` / `eat_snack_` | Feeding |
| `inter_drag_*` / `inter_drop_` / `inter_click_` / `inter_mash_` / `inter_hover_` | Input |
| `exp_*` | Facial emotions |
| `talk_` / `find_item_` / `dream_` | Talk / event / dream |
| `weather_rain_` / `weather_snow_` / `weather_sunny_` | Weather |
| `time_morning_` / `time_noon_` / `time_evening_` / `time_night_` | Time of day |

Example names: `idle_01_cutout.png`, `eat_fish_01_cutout.png`.

Tips:

- Prefer real **alpha transparency** (current cutouts use alpha)  
- Runtime decode width ≈ 320px (source is 2048²)  
- After replacing PNGs: rebuild or use **Reload animation**

---

## Project structure

```text
Maomao AI Desktop Pet/
├── MaomaoDesktopPet.sln
├── README.md                      # English (default)
├── README.zh-CN.md                # Chinese
├── .gitignore
├── picture/
│   ├── Display Images1.png / Display Images2.png
│   ├── readme-showcase-1.png / readme-showcase-2.png  # unified 480×640
│   ├── icon.png
│   ├── move.png
│   └── deskpet_transparent_frames/
├── docs/superpowers/
│   ├── specs/                     # Design notes
│   └── plans/                     # Implementation plans
└── src/MaomaoDesktopPet/
    ├── Assets/
    │   ├── icon.ico
    │   └── Pet/                   # Copied frames
    ├── Core/                      # States, animator, loader, controller
    ├── Models/                    # PetData, catalogs, unlock rules
    ├── Services/                  # Care, AI, quests, explore, weather, …
    ├── Views/                     # Feature panels + mini-games
    ├── MainWindow.xaml(.cs)       # Transparent pet shell + menu
    ├── App.xaml(.cs)
    └── MaomaoDesktopPet.csproj
```

---

## Product map

```text
Maomao
├── Daily interaction (pet, feed, click, drag, bubbles, events)
├── AI brain (chat, memory, personality, proactive talk)
├── Care / growth (stats, level progress, quests, achievements; all features open)
├── Dress-up & room (outfits, hats, furniture buffs, themes)
├── Mini-games
├── Story (diary, dreams, memories)
├── Desktop tools (focus, todos, gentle reminders, weather)
└── World (exploration, second pet, seasons / birthdays)
```

Related docs:

- [`docs/superpowers/specs/2026-09-30-maomao-v01-design.md`](docs/superpowers/specs/2026-09-30-maomao-v01-design.md)  
- [`docs/superpowers/plans/2026-10-08-full-feature-landing.md`](docs/superpowers/plans/2026-10-08-full-feature-landing.md)

---

## Development tips

- Prefer changes that improve **aliveness and input feel** first  
- Add frames under `picture/deskpet_transparent_frames/` with clear prefixes  
- Do not commit `bin/`, `obj/`, `.vs/` (see `.gitignore`)  
- Keep new systems in `Services/` + thin UI in `Views/`  
- Cloud AI keys stay in local save / settings — never commit secrets  

---

## License

Not specified yet. Add a `LICENSE` before public distribution.

---

## Language

- **Default:** [`README.md`](README.md) (English)  
- **Chinese:** [`README.zh-CN.md`](README.zh-CN.md)
