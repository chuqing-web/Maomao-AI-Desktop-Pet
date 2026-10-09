using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using MaomaoDesktopPet.Models;
using MaomaoDesktopPet.Services;

namespace MaomaoDesktopPet.Views;

public static class FeatureWindows
{
    public static void OpenStatus()
    {
        var d = AppServices.Data;
        var w = UiKit.CreateShell("毛毛状态", 380, 460);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1($"{d.PetName} · Lv.{d.Level} · {d.GrowthStage}"));
        panel.Children.Add(UiKit.P($"关系：{d.RelationTitle}　亲密度 {d.Affection}　金币 {d.Coins}　经验 {d.Exp}/{CareService.ExpToNext(d.Level)}"));
        panel.Children.Add(UiKit.P($"性格：{d.Personality}　情绪：{d.Emotion}　幸运：{d.Luck}"));
        panel.Children.Add(UiKit.P($"相识 {d.DaysTogether} 天　连续登录：{string.Concat(Enumerable.Repeat("🐾", Math.Min(7, Math.Max(1, d.LoginStreak))))} ({d.LoginStreak})"));
        panel.Children.Add(UiKit.P($"🍖 饱腹 {d.Hunger:0}"));
        panel.Children.Add(UiKit.Bar(d.Hunger));
        panel.Children.Add(UiKit.P($"❤️ 心情 {d.Mood:0}"));
        panel.Children.Add(UiKit.Bar(d.Mood));
        panel.Children.Add(UiKit.P($"⚡ 精力 {d.Energy:0}"));
        panel.Children.Add(UiKit.Bar(d.Energy));
        panel.Children.Add(UiKit.P($"✨ 清洁 {d.Cleanliness:0}"));
        panel.Children.Add(UiKit.Bar(d.Cleanliness));
        panel.Children.Add(UiKit.P($"今日任务："));
        AppServices.Quests.EnsureToday();
        foreach (var q in d.DailyQuests)
        {
            var line = $"{(q.Claimed ? "✅" : "☐")} {q.Title} ({q.Current}/{q.Target})";
            var sp = new StackPanel { Orientation = Orientation.Horizontal };
            sp.Children.Add(new TextBlock { Text = line, Width = 220, VerticalAlignment = VerticalAlignment.Center });
            if (!q.Claimed && q.Current >= q.Target)
            {
                sp.Children.Add(UiKit.Btn("领取", (_, _) =>
                {
                    MessageBox.Show(AppServices.Quests.Claim(q.Id));
                    w.Close();
                    OpenStatus();
                }));
            }
            panel.Children.Add(sp);
        }
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }

    public static void OpenFeed(Action<string, string?>? onResult = null)
    {
        var w = UiKit.CreateShell("喂食", 420, 480);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("今天吃什么？"));
        panel.Children.Add(UiKit.P($"金币：{AppServices.Data.Coins}"));

        foreach (var food in GameCatalog.Foods)
        {
            var count = AppServices.Data.Inventory.GetValueOrDefault(food.Id);
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };
            row.Children.Add(new TextBlock
            {
                Text = $"{food.Name} x{count}\n{food.Description}  饱腹+{food.Hunger} 心情+{food.Mood}",
                Width = 240,
                TextWrapping = TextWrapping.Wrap
            });
            row.Children.Add(UiKit.Btn("喂食", (_, _) =>
            {
                var msg = AppServices.Care.Feed(food.Id);
                onResult?.Invoke(msg, food.Id);
                MessageBox.Show(msg, "喂食");
                w.Close();
            }));
            var price = food.Price > 0 ? food.Price : 15;
            row.Children.Add(UiKit.Btn($"买(+1) {price}币", (_, _) =>
            {
                if (AppServices.Care.Buy("food", food.Id, price))
                {
                    MessageBox.Show($"买到了{food.Name}");
                    w.Close();
                    OpenFeed(onResult);
                }
                else MessageBox.Show("金币不够啦");
            }));
            panel.Children.Add(row);
        }
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }

    public static void OpenPet(Action<string>? onResult = null)
    {
        var w = UiKit.CreateShell("摸摸", 320, 360);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("想摸哪里？"));
        foreach (var (id, name) in new[] { ("head", "摸头"), ("face", "摸脸"), ("ear", "摸耳朵"), ("chin", "挠下巴"), ("belly", "拍肚子"), ("tail", "摸尾巴") })
        {
            panel.Children.Add(UiKit.Btn(name, (_, _) =>
            {
                var msg = AppServices.Care.Pet(id);
                onResult?.Invoke(msg);
                w.Close();
            }));
        }
        w.Content = panel;
        w.Show();
    }

    public static void OpenBag()
    {
        var d = AppServices.Data;
        var w = UiKit.CreateShell("背包", 400, 420);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("🎒 我的背包"));
        panel.Children.Add(UiKit.P($"金币：{d.Coins}"));
        foreach (var kv in d.Inventory.OrderBy(k => k.Key))
        {
            var food = GameCatalog.Foods.FirstOrDefault(f => f.Id == kv.Key);
            var name = food?.Name ?? kv.Key;
            panel.Children.Add(UiKit.P($"{name} × {kv.Value}"));
        }
        panel.Children.Add(UiKit.H1("收藏"));
        panel.Children.Add(UiKit.P(string.Join("、", d.UnlockedCollection)));
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }

    public static void OpenOutfit(Action<string>? onResult = null)
    {
        if (!FeatureGate.Can("outfit", out var reason))
        {
            MessageBox.Show(reason);
            return;
        }
        var d = AppServices.Data;
        var w = UiKit.CreateShell("换装", 420, 520);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("👕 衣柜"));
        panel.Children.Add(UiKit.P($"当前：{GameCatalog.Outfits.GetValueOrDefault(d.EquippedOutfit).Name}"));
        foreach (var (id, info) in GameCatalog.Outfits)
        {
            var unlocked = d.UnlockedOutfits.Contains(id);
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 4) };
            row.Children.Add(new TextBlock { Text = $"{info.Name}\n{info.Desc}", Width = 200, TextWrapping = TextWrapping.Wrap });
            if (unlocked)
            {
                row.Children.Add(UiKit.Btn(d.EquippedOutfit == id ? "穿着中" : "穿上", (_, _) =>
                {
                    d.EquippedOutfit = id;
                    AppServices.Save.Save(d);
                    onResult?.Invoke($"换上了{info.Name}！");
                    w.Close();
                }));
            }
            else
            {
                row.Children.Add(UiKit.Btn($"解锁 {info.Price}", (_, _) =>
                {
                    if (AppServices.Care.Buy("outfit", id, info.Price))
                    {
                        MessageBox.Show($"解锁了{info.Name}");
                        w.Close();
                        OpenOutfit(onResult);
                    }
                    else MessageBox.Show("金币不够");
                }));
            }
            panel.Children.Add(row);
        }
        panel.Children.Add(UiKit.H1("🎩 帽子"));
        foreach (var (id, info) in GameCatalog.Hats)
        {
            var unlocked = d.UnlockedHats.Contains(id);
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            row.Children.Add(new TextBlock { Text = $"{info.Name} — {info.Desc}", Width = 200, TextWrapping = TextWrapping.Wrap });
            if (unlocked)
                row.Children.Add(UiKit.Btn(d.EquippedHat == id ? "戴着" : "戴上", (_, _) =>
                {
                    d.EquippedHat = id;
                    AppServices.Save.Save(d);
                    onResult?.Invoke($"戴上了{info.Name}");
                    w.Close();
                }));
            else
                row.Children.Add(UiKit.Btn($"解锁 {info.Price}", (_, _) =>
                {
                    if (d.Coins < info.Price) { MessageBox.Show("金币不够"); return; }
                    d.Coins -= info.Price;
                    d.UnlockedHats.Add(id);
                    AppServices.Save.Save(d);
                    w.Close();
                    OpenOutfit(onResult);
                }));
            panel.Children.Add(row);
        }
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }

    public static void OpenRoom()
    {
        if (!FeatureGate.Can("room", out var reason))
        {
            MessageBox.Show(reason);
            return;
        }
        var d = AppServices.Data;
        var w = UiKit.CreateShell("我的房间", 480, 520);
        var root = new DockPanel { Margin = new Thickness(16) };
        var panel = new StackPanel();
        panel.Children.Add(UiKit.H1("🏠 毛毛的小房间"));
        panel.Children.Add(UiKit.P($"墙纸：{d.Wallpaper}　地板：{d.Floor}"));

        var stage = new Border
        {
            Height = 180,
            CornerRadius = new CornerRadius(12),
            Background = new LinearGradientBrush(Color.FromRgb(255, 240, 230), Color.FromRgb(220, 235, 250), 90),
            Margin = new Thickness(0, 0, 0, 12),
            Child = new TextBlock
            {
                Text = "🐱  " + string.Join("  ", d.PlacedFurniture.Select(id =>
                    GameCatalog.Furniture.TryGetValue(id, out var f) ? f.Name : id)),
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            }
        };
        panel.Children.Add(stage);

        foreach (var (id, info) in GameCatalog.Furniture)
        {
            var unlocked = d.UnlockedFurniture.Contains(id);
            var placed = d.PlacedFurniture.Contains(id);
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(new TextBlock
            {
                Text = $"{info.Name} — {info.Desc}（{info.Buff}）",
                Width = 260,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            });
            if (!unlocked)
            {
                row.Children.Add(UiKit.Btn($"购买 {info.Price}", (_, _) =>
                {
                    if (AppServices.Care.Buy("furniture", id, info.Price))
                    {
                        w.Close();
                        OpenRoom();
                    }
                    else MessageBox.Show("金币不够");
                }));
            }
            else
            {
                row.Children.Add(UiKit.Btn(placed ? "收起" : "摆放", (_, _) =>
                {
                    if (placed) d.PlacedFurniture.Remove(id);
                    else d.PlacedFurniture.Add(id);
                    AppServices.Save.Save(d);
                    w.Close();
                    OpenRoom();
                }));
            }
            panel.Children.Add(row);
        }

        panel.Children.Add(UiKit.Btn("换奶油墙纸", (_, _) => { d.Wallpaper = "cream"; AppServices.Save.Save(d); }));
        panel.Children.Add(UiKit.Btn("换星空墙纸", (_, _) => { d.Wallpaper = "starry"; AppServices.Save.Save(d); }));
        panel.Children.Add(UiKit.Btn("按季节换主题", (_, _) =>
        {
            AppServices.Weather.ApplySeasonalRoom();
            MessageBox.Show($"房间主题 → {d.RoomTheme}");
            w.Close();
            OpenRoom();
        }));
        root.Children.Add(UiKit.Scroll(panel));
        w.Content = root;
        w.Show();
    }

    public static void OpenDiary()
    {
        AppServices.Diary.EnsureToday();
        var w = UiKit.CreateShell("小爪日记", 440, 520);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("📖 日记 & 记忆"));
        panel.Children.Add(UiKit.P($"记忆文件：{AppServices.Memory.FilePath}"));
        panel.Children.Add(UiKit.P("—— 记忆碎片 ——"));
        foreach (var m in AppServices.Data.Memories.Take(20))
            panel.Children.Add(UiKit.P($"• {m.At:MM/dd HH:mm} {m.Text}"));
        if (AppServices.Data.Memories.Count == 0)
            panel.Children.Add(UiKit.P("（还没有记忆，去和毛毛聊天吧）"));
        panel.Children.Add(UiKit.P("—— 日记 ——"));
        foreach (var d in AppServices.Data.Diary.Take(15))
            panel.Children.Add(UiKit.P(d.Text + "\n"));
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }

    public static void OpenAchievements()
    {
        AppServices.Achievements.Check();
        var w = UiKit.CreateShell("成就", 420, 500);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("⭐ 成就馆"));
        foreach (var a in GameCatalog.Achievements)
        {
            var ok = AppServices.Data.UnlockedAchievements.Contains(a.Id);
            if (a.Hidden && !ok)
            {
                panel.Children.Add(UiKit.P("❓ 隐藏成就 — ???"));
                continue;
            }
            panel.Children.Add(UiKit.P($"{(ok ? "🏆" : "🔒")} {(a.Hidden ? "[隐藏] " : "")}{a.Title} — {a.Desc}"));
        }
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }

    public static void OpenSettings()
    {
        var s = AppServices.Data.Settings;
        var w = UiKit.CreateShell("设置", 480, 640);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("⚙ 设置"));

        var nameBox = new TextBox { Text = AppServices.Data.OwnerName, Margin = new Thickness(0, 0, 0, 8) };
        var petBox = new TextBox { Text = AppServices.Data.PetName, Margin = new Thickness(0, 0, 0, 8) };
        var mode = new ComboBox { Margin = new Thickness(0, 0, 0, 8) };
        mode.Items.Add("Quiet");
        mode.Items.Add("Companion");
        mode.Items.Add("Active");
        mode.SelectedItem = s.DisturbMode.ToString();
        var personality = new ComboBox { Margin = new Thickness(0, 0, 0, 8) };
        foreach (var p in Enum.GetNames<PersonalityType>()) personality.Items.Add(p);
        personality.SelectedItem = AppServices.Data.Personality.ToString();
        var mischief = new ComboBox { Margin = new Thickness(0, 0, 0, 8) };
        mischief.Items.Add("Off");
        mischief.Items.Add("Mild");
        mischief.Items.Add("Full");
        mischief.SelectedItem = s.MischiefMode.ToString();
        var water = new CheckBox { Content = "喝水提醒", IsChecked = s.WaterReminder, Margin = new Thickness(0, 0, 0, 8) };
        var work = new CheckBox { Content = "久坐提醒", IsChecked = s.WorkReminder, Margin = new Thickness(0, 0, 0, 8) };
        var sys = new CheckBox { Content = "网络状态互动", IsChecked = s.SystemStatusReact, Margin = new Thickness(0, 0, 0, 8) };

        var useAi = new CheckBox { Content = "启用云端 AI（OpenAI 兼容）", IsChecked = s.UseCloudAi, Margin = new Thickness(0, 0, 0, 8) };
        var preset = new ComboBox { Margin = new Thickness(0, 0, 0, 8) };
        preset.Items.Add("自定义");
        preset.Items.Add("OpenAI");
        preset.Items.Add("DeepSeek");
        preset.Items.Add("通义兼容模式");
        preset.Items.Add("Ollama 本地");
        preset.SelectedIndex = 0;
        var apiBase = new TextBox { Text = s.AiApiBase ?? "https://api.openai.com/v1", Margin = new Thickness(0, 0, 0, 8) };
        var apiKey = new TextBox { Text = s.AiApiKey ?? "", Margin = new Thickness(0, 0, 0, 8) };
        var model = new TextBox { Text = s.AiModel, Margin = new Thickness(0, 0, 0, 8) };
        var tempBox = new TextBox { Text = s.AiTemperature.ToString("0.##"), Margin = new Thickness(0, 0, 0, 8) };
        var maxTokBox = new TextBox { Text = s.AiMaxTokens.ToString(), Margin = new Thickness(0, 0, 0, 8) };
        var statusAi = UiKit.P(AppServices.Ai.CloudConfigured ? "云端：已配置" : "云端：未配置（将用本地人格）");

        preset.SelectionChanged += (_, _) =>
        {
            switch (preset.SelectedItem?.ToString())
            {
                case "OpenAI":
                    apiBase.Text = "https://api.openai.com/v1";
                    if (string.IsNullOrWhiteSpace(model.Text) || model.Text.Contains("deepseek", StringComparison.OrdinalIgnoreCase))
                        model.Text = "gpt-4o-mini";
                    break;
                case "DeepSeek":
                    apiBase.Text = "https://api.deepseek.com/v1";
                    model.Text = "deepseek-chat";
                    break;
                case "通义兼容模式":
                    apiBase.Text = "https://dashscope.aliyuncs.com/compatible-mode/v1";
                    model.Text = "qwen-plus";
                    break;
                case "Ollama 本地":
                    apiBase.Text = "http://127.0.0.1:11434/v1";
                    model.Text = "llama3.2";
                    break;
            }
        };

        void ApplyAiFields()
        {
            s.UseCloudAi = useAi.IsChecked == true;
            s.AiApiBase = apiBase.Text.Trim();
            s.AiApiKey = apiKey.Text.Trim();
            s.AiModel = model.Text.Trim();
            if (double.TryParse(tempBox.Text.Trim(), out var t)) s.AiTemperature = t;
            if (int.TryParse(maxTokBox.Text.Trim(), out var mt)) s.AiMaxTokens = mt;
        }

        panel.Children.Add(UiKit.P("主人名字"));
        panel.Children.Add(nameBox);
        panel.Children.Add(UiKit.P("宠物名字"));
        panel.Children.Add(petBox);
        panel.Children.Add(UiKit.P("打扰模式 Quiet / Companion / Active"));
        panel.Children.Add(mode);
        panel.Children.Add(UiKit.P("性格"));
        panel.Children.Add(personality);
        panel.Children.Add(UiKit.P("恶作剧 Off / Mild / Full"));
        panel.Children.Add(mischief);
        panel.Children.Add(water);
        panel.Children.Add(work);
        panel.Children.Add(sys);

        panel.Children.Add(UiKit.H1("☁ 云端 AI"));
        panel.Children.Add(useAi);
        panel.Children.Add(UiKit.P("服务商预设（会填写 Base / 模型）"));
        panel.Children.Add(preset);
        panel.Children.Add(UiKit.P("API Base（可填到 /v1，会自动补 /chat/completions）"));
        panel.Children.Add(apiBase);
        panel.Children.Add(UiKit.P("API Key"));
        panel.Children.Add(apiKey);
        panel.Children.Add(UiKit.P("Model"));
        panel.Children.Add(model);
        panel.Children.Add(UiKit.P("Temperature（建议 0.7～1.0）"));
        panel.Children.Add(tempBox);
        panel.Children.Add(UiKit.P("Max Tokens（建议 200～400）"));
        panel.Children.Add(maxTokBox);
        panel.Children.Add(statusAi);
        panel.Children.Add(UiKit.Btn("测试连接", async (_, _) =>
        {
            ApplyAiFields();
            AppServices.Save.Save(AppServices.Data);
            var (ok, msg) = await AppServices.Ai.TestConnectionAsync();
            statusAi.Text = ok ? "云端：连接成功" : "云端：连接失败";
            MessageBox.Show(msg, ok ? "成功" : "失败");
        }));

        panel.Children.Add(UiKit.H1("💾 本地数据（exe 同目录）"));
        panel.Children.Add(UiKit.P($"存档：{AppServices.Save.SavePath}"));
        panel.Children.Add(UiKit.P($"记忆：{AppServices.Memory.FilePath}"));
        panel.Children.Add(UiKit.P($"当前记忆条数：{AppServices.Data.Memories.Count}"));
        panel.Children.Add(UiKit.Btn("打开数据文件夹", (_, _) =>
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = AppServices.Save.DataDirectory,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }));

        panel.Children.Add(UiKit.Btn("保存设置", (_, _) =>
        {
            AppServices.Data.OwnerName = nameBox.Text.Trim();
            AppServices.Data.PetName = petBox.Text.Trim();
            if (Enum.TryParse<DisturbMode>(mode.SelectedItem?.ToString(), out var dm)) s.DisturbMode = dm;
            if (Enum.TryParse<PersonalityType>(personality.SelectedItem?.ToString(), out var pt)) AppServices.Data.Personality = pt;
            if (Enum.TryParse<MischiefMode>(mischief.SelectedItem?.ToString(), out var mm))
            {
                s.MischiefMode = mm;
                s.AllowMischief = mm != MischiefMode.Off;
            }
            s.WaterReminder = water.IsChecked == true;
            s.WorkReminder = work.IsChecked == true;
            s.SystemStatusReact = sys.IsChecked == true;
            ApplyAiFields();
            AppServices.Save.Save(AppServices.Data);
            MessageBox.Show("已保存到 exe 同目录的 save.json / memory.json");
            w.Close();
        }));
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }

    public static void OpenChat(Action<string>? onBubble = null)
    {
        if (!FeatureGate.Can("ai", out var reason))
        {
            MessageBox.Show(reason);
            return;
        }

        var modeLabel = AppServices.Ai.CloudConfigured ? "☁ 云端 AI" : "🧠 本地人格";
        var w = UiKit.CreateShell($"和毛毛聊天 · {modeLabel}", 480, 600);
        var root = new DockPanel { Margin = new Thickness(12) };
        var tip = UiKit.P(AppServices.Ai.CloudConfigured
            ? $"云端已启用 · 记忆 {AppServices.Data.Memories.Count} 条 · {AppServices.Memory.FilePath}"
            : $"本地模式 · 记忆 {AppServices.Data.Memories.Count} 条（可在设置开启云端）");
        DockPanel.SetDock(tip, Dock.Top);
        root.Children.Add(tip);

        var input = new TextBox { Margin = new Thickness(0, 8, 8, 0) };
        var send = UiKit.Btn("发送", (_, _) => { });
        var history = new ListBox { Height = 400 };
        foreach (var m in AppServices.Data.ChatHistory.TakeLast(40))
            history.Items.Add($"{(m.Role == "user" ? "你" : AppServices.Data.PetName)}: {m.Text}");

        async Task SendAsync()
        {
            var text = input.Text;
            if (string.IsNullOrWhiteSpace(text)) return;
            input.Clear();
            history.Items.Add($"你: {text}");
            history.Items.Add($"{AppServices.Data.PetName}: ……思考中");
            var thinkingIndex = history.Items.Count - 1;
            send.IsEnabled = false;
            try
            {
                var reply = await AppServices.Ai.ChatAsync(text);
                history.Items[thinkingIndex] = $"{AppServices.Data.PetName}: {reply}";
                history.ScrollIntoView(history.Items[^1]);
                tip.Text = (AppServices.Ai.CloudConfigured ? "☁ 云端 AI" : "🧠 本地人格") +
                           $" · 记忆 {AppServices.Data.Memories.Count} 条";
                onBubble?.Invoke(reply.Length > 28 ? reply[..28] + "…" : reply);
            }
            catch (Exception ex)
            {
                history.Items[thinkingIndex] = $"{AppServices.Data.PetName}: （出错了）{ex.Message}";
            }
            finally
            {
                send.IsEnabled = true;
                input.Focus();
            }
        }

        send.Click += async (_, _) => await SendAsync();
        input.KeyDown += async (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter &&
                (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
            {
                e.Handled = true;
                await SendAsync();
            }
        };

        var bottom = new DockPanel();
        DockPanel.SetDock(send, Dock.Right);
        bottom.Children.Add(send);
        bottom.Children.Add(input);
        DockPanel.SetDock(bottom, Dock.Bottom);
        root.Children.Add(bottom);
        root.Children.Add(history);
        w.Content = root;
        w.Show();
        input.Focus();
    }

    public static void OpenFocus(Action<string>? onBubble = null)
    {
        var w = UiKit.CreateShell("专注陪伴", 360, 280);
        var panel = new StackPanel { Margin = new Thickness(20) };
        var label = UiKit.H1("25:00");
        panel.Children.Add(UiKit.P("毛毛当你的小监督员"));
        panel.Children.Add(label);
        var remaining = TimeSpan.FromMinutes(25);
        var timer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) =>
        {
            remaining -= TimeSpan.FromSeconds(1);
            label.Text = $"{remaining.Minutes:00}:{remaining.Seconds:00}";
            if (remaining <= TimeSpan.Zero)
            {
                timer.Stop();
                AppServices.Care.RewardFocus(25);
                onBubble?.Invoke("完成！休息 5 分钟！");
                MessageBox.Show("专注完成！获得金币与经验～");
                w.Close();
            }
        };
        panel.Children.Add(UiKit.Btn("开始 25 分钟", (_, _) =>
        {
            onBubble?.Invoke("开始啦！");
            timer.Start();
        }));
        panel.Children.Add(UiKit.Btn("提前结束", (_, _) =>
        {
            timer.Stop();
            w.Close();
        }));
        w.Content = panel;
        w.Show();
    }

    public static void OpenGamesMenu(Action<string>? onBubble = null)
    {
        if (!FeatureGate.Can("games", out var reason))
        {
            MessageBox.Show(reason);
            return;
        }
        var w = UiKit.CreateShell("小游戏", 320, 340);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("🎮 玩什么？"));
        panel.Children.Add(UiKit.Btn("接小鱼", (_, _) => { w.Close(); Games.CatchFishGame.Open(onBubble); }));
        panel.Children.Add(UiKit.Btn("躲障碍", (_, _) => { w.Close(); Games.DodgeGame.Open(onBubble); }));
        panel.Children.Add(UiKit.Btn("打地鼠", (_, _) => { w.Close(); Games.WhackMoleGame.Open(onBubble); }));
        panel.Children.Add(UiKit.Btn("猜表情", (_, _) => { w.Close(); Games.GuessMoodGame.Open(onBubble); }));
        w.Content = panel;
        w.Show();
    }

    public static void OpenOnboardingIfNeeded(Action? done = null)
    {
        if (AppServices.Data.OnboardingDone)
        {
            done?.Invoke();
            return;
        }

        var w = UiKit.CreateShell("欢迎来到小爪陪伴计划", 440, 520);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("🐾 认识一下吧"));
        panel.Children.Add(UiKit.P("我是住在你桌面上的云朵小猫。先告诉我这些～"));
        var owner = new TextBox { Text = "主人", Margin = new Thickness(0, 0, 0, 8) };
        var pet = new TextBox { Text = "毛毛", Margin = new Thickness(0, 0, 0, 8) };
        var personality = new ComboBox { Margin = new Thickness(0, 0, 0, 8) };
        foreach (var p in Enum.GetNames<PersonalityType>()) personality.Items.Add(p);
        personality.SelectedItem = "Soft";
        var ownerBday = new TextBox { Text = "2000-01-01", Margin = new Thickness(0, 0, 0, 8) };
        var petBday = new TextBox { Text = DateTime.Today.ToString("yyyy-MM-dd"), Margin = new Thickness(0, 0, 0, 8) };
        panel.Children.Add(UiKit.P("你的名字"));
        panel.Children.Add(owner);
        panel.Children.Add(UiKit.P("给我取名"));
        panel.Children.Add(pet);
        panel.Children.Add(UiKit.P("我的性格"));
        panel.Children.Add(personality);
        panel.Children.Add(UiKit.P("你的生日 yyyy-MM-dd"));
        panel.Children.Add(ownerBday);
        panel.Children.Add(UiKit.P("我的生日 yyyy-MM-dd"));
        panel.Children.Add(petBday);
        panel.Children.Add(UiKit.Btn("开始陪伴！", (_, _) =>
        {
            var d = AppServices.Data;
            d.OwnerName = string.IsNullOrWhiteSpace(owner.Text) ? "主人" : owner.Text.Trim();
            d.PetName = string.IsNullOrWhiteSpace(pet.Text) ? "毛毛" : pet.Text.Trim();
            if (Enum.TryParse<PersonalityType>(personality.SelectedItem?.ToString(), out var pt))
                d.Personality = pt;
            if (DateTime.TryParse(ownerBday.Text, out var ob)) d.OwnerBirthday = ob;
            if (DateTime.TryParse(petBday.Text, out var pb)) d.PetBirthday = pb;
            d.OnboardingDone = true;
            AppServices.Diary.AddMemory($"{d.OwnerName} 第一次给我取名：{d.PetName}");
            AppServices.Save.Save(d);
            w.Close();
            done?.Invoke();
        }));
        w.Content = UiKit.Scroll(panel);
        w.ShowDialog();
    }

    public static void OpenCollection()
    {
        var d = AppServices.Data;
        var w = UiKit.CreateShell("收藏图鉴", 420, 500);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("📕 收藏图鉴"));
        var foods = d.UnlockedCollection.Count(x => x.StartsWith("food_"));
        var dreams = d.DreamFragments.Count;
        var maps = d.UnlockedMaps.Count;
        panel.Children.Add(UiKit.P($"食物图鉴　{foods}/{GameCatalog.Foods.Length}"));
        panel.Children.Add(UiKit.P($"家具　　　{d.UnlockedFurniture.Count}/{GameCatalog.Furniture.Count}"));
        panel.Children.Add(UiKit.P($"服装　　　{d.UnlockedOutfits.Count}/{GameCatalog.Outfits.Count}"));
        panel.Children.Add(UiKit.P($"帽子　　　{d.UnlockedHats.Count}/{GameCatalog.Hats.Count}"));
        panel.Children.Add(UiKit.P($"地图　　　{maps}/{GameCatalog.Maps.Count}"));
        panel.Children.Add(UiKit.P($"梦境碎片　{dreams}/5"));
        panel.Children.Add(UiKit.P($"事件/收藏　{d.UnlockedCollection.Count}"));
        panel.Children.Add(UiKit.P("—— 明细 ——"));
        panel.Children.Add(UiKit.P(string.Join("、", d.UnlockedCollection.OrderBy(x => x))));
        if (d.DreamFragments.Count > 0)
            panel.Children.Add(UiKit.P("梦境：" + string.Join("、", d.DreamFragments)));
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }

    public static void OpenExplore(Action<string>? onBubble = null)
    {
        if (!FeatureGate.Can("explore", out var reason))
        {
            MessageBox.Show(reason);
            return;
        }
        var done = AppServices.Explore.TryComplete();
        if (done is not null)
        {
            onBubble?.Invoke(done);
            MessageBox.Show(done);
        }

        var d = AppServices.Data;
        var w = UiKit.CreateShell("桌面世界 · 探索", 420, 420);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("🌍 探索"));
        if (d.ExploringMap is not null && d.ExploreReturnAt is not null)
        {
            var left = d.ExploreReturnAt.Value - DateTime.Now;
            panel.Children.Add(UiKit.P($"正在探索 {d.ExploringMap}… 剩余约 {Math.Max(0, (int)left.TotalMinutes)} 分钟"));
        }
        foreach (var (id, info) in GameCatalog.Maps.Where(m => m.Value.Minutes > 0))
        {
            panel.Children.Add(UiKit.Btn($"{info.Name}（约 {info.Minutes} 分钟）\n{info.Desc}", (_, _) =>
            {
                var msg = AppServices.Explore.Start(id) ?? "出发失败";
                onBubble?.Invoke(msg);
                MessageBox.Show(msg);
                w.Close();
            }));
        }
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }

    public static void OpenTodos()
    {
        var d = AppServices.Data;
        var w = UiKit.CreateShell("待办清单", 400, 420);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("📝 和毛毛一起的待办"));
        var input = new TextBox { Margin = new Thickness(0, 0, 0, 8) };
        panel.Children.Add(input);
        panel.Children.Add(UiKit.Btn("添加", (_, _) =>
        {
            if (string.IsNullOrWhiteSpace(input.Text)) return;
            d.Todos.Add(new TodoItem { Text = input.Text.Trim() });
            AppServices.Save.Save(d);
            w.Close();
            OpenTodos();
        }));
        foreach (var t in d.Todos.ToList())
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal };
            var cb = new CheckBox { Content = t.Text, IsChecked = t.Done, Width = 260 };
            cb.Checked += (_, _) => { t.Done = true; AppServices.Save.Save(d); };
            cb.Unchecked += (_, _) => { t.Done = false; AppServices.Save.Save(d); };
            row.Children.Add(cb);
            row.Children.Add(UiKit.Btn("删", (_, _) =>
            {
                d.Todos.Remove(t);
                AppServices.Save.Save(d);
                w.Close();
                OpenTodos();
            }));
            panel.Children.Add(row);
        }
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }

    public static void OpenSecondPet(Action<string>? onBubble = null)
    {
        if (!FeatureGate.Can("second", out var reason))
        {
            MessageBox.Show(reason);
            return;
        }
        var d = AppServices.Data;
        var w = UiKit.CreateShell("第二只伙伴", 400, 420);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("🐾 多宠物"));
        if (d.SecondPetUnlocked)
        {
            panel.Children.Add(UiKit.P($"伙伴：{d.SecondPetName}（{d.SecondPetId}）"));
            panel.Children.Add(UiKit.P($"与毛毛关系值：{d.PetRelation}/100"));
            panel.Children.Add(UiKit.Btn("让它们一起玩", (_, _) =>
            {
                d.PetRelation = Math.Clamp(d.PetRelation + 5, 0, 100);
                d.Mood = Math.Clamp(d.Mood + 5, 0, 100);
                AppServices.Save.Save(d);
                onBubble?.Invoke($"{d.PetName} 和 {d.SecondPetName} 玩成一团！");
                w.Close();
            }));
            panel.Children.Add(UiKit.Btn("偶尔吵吵嘴", (_, _) =>
            {
                d.PetRelation = Math.Clamp(d.PetRelation - 3, 0, 100);
                onBubble?.Invoke($"{d.PetName}：这里我先来的。\n{d.SecondPetName}：哦。");
                AppServices.Save.Save(d);
                w.Close();
            }));
        }
        else
        {
            panel.Children.Add(UiKit.P("选择一位新伙伴入住吧"));
            foreach (var (id, name) in GameCatalog.CompanionPets)
            {
                panel.Children.Add(UiKit.Btn(name, (_, _) =>
                {
                    d.SecondPetUnlocked = true;
                    d.SecondPetId = id;
                    d.SecondPetName = name;
                    AppServices.Achievements.Unlock("second_friend");
                    AppServices.Diary.AddMemory($"新伙伴 {name} 来到了桌面小岛。");
                    AppServices.Save.Save(d);
                    onBubble?.Invoke($"{d.PetName}：你是谁？\n{name}：我是新来的。");
                    w.Close();
                }));
            }
        }
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }

    public static void OpenRelation()
    {
        var d = AppServices.Data;
        var w = UiKit.CreateShell("我们的关系", 380, 360);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("❤️ 我们的关系"));
        panel.Children.Add(UiKit.P($"亲密度：{d.Affection}（{d.RelationTitle}）"));
        panel.Children.Add(UiKit.P($"相识天数：{d.DaysTogether}"));
        panel.Children.Add(UiKit.P($"成长：{d.GrowthStage} Lv.{d.Level}"));
        panel.Children.Add(UiKit.P($"情绪：{d.Emotion}　幸运：{d.Luck}"));
        panel.Children.Add(UiKit.P($"共同经历：喂食{d.TotalFeeds} / 摸摸{d.TotalPets} / 游戏{d.TotalGames} / 专注{d.TotalFocusMinutes}分钟"));
        panel.Children.Add(UiKit.P($"连续登录：{string.Concat(Enumerable.Repeat("🐾", Math.Min(7, d.LoginStreak)))} ({d.LoginStreak}天)"));
        if (d.FocusStreakDays > 0)
            panel.Children.Add(UiKit.P($"🔥 连续专注 {d.FocusStreakDays} 天"));
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }
}
