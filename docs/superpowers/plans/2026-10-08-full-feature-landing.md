# Full Feature Landing Implementation Plan

> **For agentic workers:** Implement task-by-task. Checkboxes track progress.

**Goal:** Land the full Maomao AI Desktop Pet feature map as a working Windows WPF app (care, UI panels, games, AI, tools), with local save and optional cloud AI.

**Architecture:** Composition root `AppServices` holds `PetData` + domain services. `MainWindow` is the transparent pet shell; feature windows open from the context menu. Persistence via JSON in `%AppData%/MaomaoDesktopPet/save.json`.

**Tech Stack:** C# / WPF / .NET 8 / System.Text.Json / HttpClient (optional AI)

---

### Task 1: Models + Save
- [x] PetData, catalogs, SaveService

### Task 2: Care / Quests / Achievements / Events / Diary
- [x] Domain services + decay clock

### Task 3: Feature windows
- [x] Status, Feed, Pet, Bag, Room, Outfit, Diary, Achievements, Settings, Focus, Chat

### Task 4: Mini-games
- [x] Catch fish, dodge, guess mood

### Task 5: Wire MainWindow + AI fallback
- [x] Menus, proactive talk, expression frames

### Task 6: Build + docs
- [x] Release build, README status update
