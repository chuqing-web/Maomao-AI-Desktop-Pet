using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MaomaoDesktopPet.Core;
using MaomaoDesktopPet.Models;
using MaomaoDesktopPet.Services;
using MaomaoDesktopPet.Views;

namespace MaomaoDesktopPet;

public partial class MainWindow : Window
{
    private readonly PetController _pet;
    private readonly SpriteLoader _loader;
    private readonly DispatcherTimer _tickTimer;
    private readonly DispatcherTimer _minuteTimer;
    private readonly DispatcherTimer _eventTimer;
    private readonly DispatcherTimer _observeTimer;

    private bool _dragging;
    private Point _dragStart;
    private Point _dragScreenOrigin;
    private bool _movedEnough;

    public MainWindow()
    {
        InitializeComponent();

        var assetsRoot = ResolveAssetsRoot();
        _loader = new SpriteLoader(assetsRoot);
        var animator = new SpriteAnimator();
        _pet = new PetController(_loader, animator);
        _pet.FrameChanged += frame => PetImage.Source = frame;
        _pet.BubbleChanged += OnBubbleChanged;
        _pet.WalkDeltaRequested += OnWalkDelta;
        _pet.Clicked += () =>
        {
            AppServices.Care.RegisterClick();
            AppServices.MarkInteract();
        };
        _pet.ChaseMouseRequested += ChaseMouse;
        _pet.HideCornerRequested += HideInCorner;
        _pet.PeekEdgeRequested += PeekFromEdge;
        _pet.BubbleActionRequested += OnBubbleAction;

        AppServices.Achievements.Unlocked += title =>
            _pet.ShowBubble($"🏆 成就：{title}");

        _tickTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _tickTimer.Tick += (_, _) => _pet.Tick();

        _minuteTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _minuteTimer.Tick += (_, _) =>
        {
            AppServices.Care.TickMinute();
            AppServices.Emotion.Recompute();
            RefreshStatusChip();
            MaybeNeedReminder();
            var desk = AppServices.Desktop.TickReminders();
            if (desk is not null) _pet.ShowBubble(desk);
            var exploreDone = AppServices.Explore.TryComplete();
            if (exploreDone is not null)
            {
                _pet.ShowBubble(exploreDone);
                _pet.PlayExpression("happy", 1600);
            }
            AppServices.Achievements.Check();
        };

        _eventTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(18) };
        _eventTimer.Tick += (_, _) =>
        {
            if (AppServices.Data.Settings.DisturbMode == DisturbMode.Quiet) return;

            var text = AppServices.Events.TryTrigger(AppServices.Data.Settings.DisturbMode);
            if (text is not null)
            {
                _pet.ShowBubble(text, "event");
                _pet.PlayExpression("surprised", 1600);
                return;
            }

            if (Random.Shared.Next(100) < 10)
            {
                var lines = new[] { ("主人。", "nudge"), ("没事，就是想叫你一下。", null), ("要不要玩一下？", "game"), ("我刚刚做了一个梦。", "dream") };
                var pick = lines[Random.Shared.Next(lines.Length)];
                _pet.ShowBubble(pick.Item1, pick.Item2);
            }
        };

        _observeTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
        _observeTimer.Tick += (_, _) =>
        {
            if (DesktopCompanionService.GetIdleMinutes() >= 0.08 &&
                (DateTime.Now - AppServices.Data.LastInteractAt).TotalSeconds > 5)
            {
                AppServices.Data.ObserveIdleSeconds += 5;
                AppServices.Achievements.Check();
            }
        };

        Loaded += async (_, _) =>
        {
            FeatureWindows.OpenOnboardingIfNeeded(() =>
            {
                _pet.ShowBubble(AppServices.Ai.GreetingOnLaunch());
            });

            PlaceBottomRight();
            _pet.Start();
            _tickTimer.Start();
            _minuteTimer.Start();
            _eventTimer.Start();
            _observeTimer.Start();
            RefreshStatusChip();
            ApplyMoodExpressionSoft();

            if (DateTime.Now is { Hour: 3, Minute: 33 })
                AppServices.Achievements.Unlock("hidden_333");

            // birthday lines
            var d = AppServices.Data;
            if (d.PetBirthday.Month == DateTime.Today.Month && d.PetBirthday.Day == DateTime.Today.Day)
                _pet.ShowBubble("🎂 今天是我的生日！", "feed");
            else if (d.OwnerBirthday is { } ob && ob.Month == DateTime.Today.Month && ob.Day == DateTime.Today.Day)
                _pet.ShowBubble($"🎂 {d.OwnerName} 生日快乐！我准备了小礼物。");

            var dreamLine = AppServices.Dreams.PeekMorningLine();
            if (dreamLine is not null && DateTime.Now.Hour is >= 6 and < 12)
                _pet.ShowBubble(dreamLine, "dream");

            // Time-of-day costume frame once at launch
            _pet.PlayAction(SpriteLoader.TimeOfDayAction(), 1800);

            try
            {
                var weather = await AppServices.Weather.RefreshAsync();
                if (weather is not null && AppServices.Data.Settings.DisturbMode != DisturbMode.Quiet)
                {
                    _pet.ShowBubble(weather);
                    var wAct = SpriteLoader.WeatherAction(AppServices.Data.LastWeather ?? "");
                    if (wAct is not null)
                        _pet.PlayAction(wAct, 2000);
                }
            }
            catch { /* offline ok */ }
        };

        BuildContextMenu();
    }

    private void RefreshStatusChip()
    {
        var d = AppServices.Data;
        StatusText.Text = $"Lv.{d.Level}·{d.GrowthStage} 💰{d.Coins} {d.Emotion}";
    }

    private void ApplyMoodExpressionSoft()
    {
        var key = AppServices.Emotion.ExpressionKey();
        _pet.PlayExpression(key, 1000);
    }

    private void MaybeNeedReminder()
    {
        if (AppServices.Data.Settings.DisturbMode == DisturbMode.Quiet) return;
        var d = AppServices.Data;
        if (d.Hunger < 25) _pet.ShowBubble("我有点饿了……", "feed");
        else if (d.Energy < 20)
        {
            _pet.ShowBubble("好困……想睡觉。");
            AppServices.Dreams.MaybePrepareAfterSleep();
        }
        else if (d.Mood < 25) _pet.ShowBubble("可以陪陪我吗？", "pet");
        else if ((DateTime.Now - d.LastInteractAt).TotalMinutes > 30)
            _pet.ShowBubble("好无聊呀……", "play");
    }

    private void OnBubbleAction(string action)
    {
        AppServices.MarkInteract();
        switch (action)
        {
            case "feed":
                FeatureWindows.OpenFeed((msg, foodId) =>
                {
                    _pet.ShowBubble(msg);
                    var act = foodId is null ? "eat" : SpriteLoader.FoodAction(foodId);
                    if (act is not null) _pet.PlayAction(act, 1800);
                    RefreshStatusChip();
                });
                break;
            case "pet":
                FeatureWindows.OpenPet(msg => { _pet.ShowBubble(msg); RefreshStatusChip(); });
                break;
            case "game":
                FeatureWindows.OpenGamesMenu(msg => { _pet.ShowBubble(msg); RefreshStatusChip(); });
                break;
            case "chat":
                FeatureWindows.OpenChat(msg => _pet.ShowBubble(msg));
                break;
            case "play":
                FeatureWindows.OpenGamesMenu(msg => _pet.ShowBubble(msg));
                break;
            case "dream":
                var dream = AppServices.Dreams.OpenDream();
                if (dream is not null)
                {
                    MessageBox.Show(dream, "梦境");
                    _pet.ShowBubble("梦好神奇……");
                    _pet.PlayAction("dream", 2000);
                }
                break;
            case "event":
                _pet.PlayAction("find", 1600);
                _pet.ShowBubble("嘻嘻，被你点到啦");
                break;
            case "nudge":
                _pet.PlayAction("talk", 1200);
                _pet.ShowBubble("没事。就是想叫你一下。");
                break;
        }
        RefreshStatusChip();
    }

    private void ChaseMouse()
    {
        if (AppServices.Data.Settings.MischiefMode == MischiefMode.Off) return;
        var mouse = GetMouseScreenDip();
        Left = Math.Clamp(mouse.X - Width / 2, SystemParameters.WorkArea.Left, SystemParameters.WorkArea.Right - Width);
        Top = Math.Clamp(mouse.Y - Height / 2, SystemParameters.WorkArea.Top, SystemParameters.WorkArea.Bottom - Height);
    }

    private void HideInCorner()
    {
        if (AppServices.Data.Settings.MischiefMode == MischiefMode.Off) return;
        var work = SystemParameters.WorkArea;
        var corner = Random.Shared.Next(4);
        Left = corner is 0 or 2 ? work.Left + 8 : work.Right - Width - 8;
        Top = corner is 0 or 1 ? work.Top + 8 : work.Bottom - Height - 8;
    }

    private void PeekFromEdge()
    {
        if (AppServices.Data.Settings.MischiefMode == MischiefMode.Off) return;
        var work = SystemParameters.WorkArea;
        Left = work.Right - Width * 0.45;
        Top = work.Bottom - Height - 8;
    }

    /// <summary>Cursor position in WPF DIPs (matches Window.Left/Top).</summary>
    private Point GetMouseScreenDip()
    {
        GetCursorPos(out var p);
        return DeviceToDip(new Point(p.X, p.Y));
    }

    private Point DeviceToDip(Point devicePoint)
    {
        var source = PresentationSource.FromVisual(this);
        if (source?.CompositionTarget is null) return devicePoint;
        return source.CompositionTarget.TransformFromDevice.Transform(devicePoint);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out NativePoint lpPoint);

    private static string ResolveAssetsRoot()
    {
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "Assets", "Pet"),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "picture", "deskpet_transparent_frames")),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "picture", "deskpet_transparent_frames")),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "picture", "deskpet_transparent_frames"))
        };

        foreach (var path in candidates)
        {
            if (Directory.Exists(path) &&
                Directory.EnumerateFiles(path, "idle_*.png").Any())
                return path;
        }

        var fallback = Path.Combine(baseDir, "Assets", "Pet");
        Directory.CreateDirectory(fallback);
        return fallback;
    }

    private void PlaceBottomRight()
    {
        var work = SystemParameters.WorkArea;
        Left = work.Right - Width - 24;
        Top = work.Bottom - Height - 24;
    }

    private void OnBubbleChanged(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            BubbleBorder.Visibility = Visibility.Collapsed;
            return;
        }

        BubbleText.Text = text;
        BubbleBorder.Visibility = Visibility.Visible;
    }

    private void Bubble_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        e.Handled = true;
        _pet.ActivateBubble();
    }

    private void OnWalkDelta(Vector delta)
    {
        var work = SystemParameters.WorkArea;
        Left = Math.Clamp(Left + delta.X, work.Left, work.Right - Width);
        Top = Math.Clamp(Top + delta.Y, work.Top, work.Bottom - Height);
    }

    private void Window_MouseEnter(object sender, MouseEventArgs e)
    {
        if (_dragging || _pet.State is PetState.Drag or PetState.Click) return;
        _pet.PlayExpression("hover", 900);
        if (Random.Shared.Next(100) < 30)
            _pet.ShowBubble("嗯？");
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject d && IsUnderBubble(d))
            return;

        _dragging = true;
        _movedEnough = false;
        _dragStart = e.GetPosition(this);
        _dragScreenOrigin = GetMouseScreenDip();
        CaptureMouse();
    }

    private bool IsUnderBubble(DependencyObject src)
    {
        while (src is not null)
        {
            if (ReferenceEquals(src, BubbleBorder)) return true;
            src = VisualTreeHelper.GetParent(src);
        }
        return false;
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_dragging || e.LeftButton != MouseButtonState.Pressed) return;
        var pos = e.GetPosition(this);
        if (!_movedEnough && (pos - _dragStart).Length < 6) return;
        if (!_movedEnough)
        {
            _movedEnough = true;
            _pet.BeginDrag();
        }

        // PointToScreen / GetCursorPos are device pixels; Left/Top are DIPs — convert first.
        var screenDip = GetMouseScreenDip();
        Left = screenDip.X - _dragStart.X;
        Top = screenDip.Y - _dragStart.Y;
        _pet.UpdateDragSpeech(screenDip.X - _dragScreenOrigin.X);
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging) return;
        _dragging = false;
        ReleaseMouseCapture();
        if (_movedEnough) _pet.EndDrag();
        else _pet.HandleClick();
        AppServices.MarkInteract();
        RefreshStatusChip();
    }

    private void Window_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (ContextMenu is not null)
        {
            ContextMenu.IsOpen = true;
            e.Handled = true;
        }
    }

    private void BuildContextMenu()
    {
        var menu = new ContextMenu();
        menu.Items.Add(CreateItem("📊 状态 / 任务", () => FeatureWindows.OpenStatus()));
        menu.Items.Add(CreateItem("❤️ 我们的关系", FeatureWindows.OpenRelation));
        menu.Items.Add(CreateItem("❤️ 互动 / 摸摸", () => FeatureWindows.OpenPet(msg =>
        {
            _pet.ShowBubble(msg);
            _pet.PlayExpression(msg.Contains("不要") ? "angry" : "wink");
            RefreshStatusChip();
        })));
        menu.Items.Add(CreateItem("🍖 喂食", () => FeatureWindows.OpenFeed((msg, foodId) =>
        {
            _pet.ShowBubble(msg);
            var act = foodId is null ? null : SpriteLoader.FoodAction(foodId);
            _pet.PlayAction(act ?? (msg.Contains("黑暗") || msg.Contains("怪") ? "angry" : "happy"), 1800);
            RefreshStatusChip();
        })));
        menu.Items.Add(CreateItem("🎮 玩游戏", () => FeatureWindows.OpenGamesMenu(msg =>
        {
            _pet.ShowBubble(msg);
            RefreshStatusChip();
        })));
        menu.Items.Add(CreateItem("💬 和我聊天", () => FeatureWindows.OpenChat(msg =>
        {
            _pet.ShowBubble(msg);
            _pet.PlayAction("talk", 1400);
        })));
        menu.Items.Add(CreateItem("⏱ 专注模式", () => FeatureWindows.OpenFocus(msg => _pet.ShowBubble(msg))));
        menu.Items.Add(CreateItem("📝 待办", FeatureWindows.OpenTodos));
        menu.Items.Add(CreateItem("👕 换衣服", () => FeatureWindows.OpenOutfit(msg =>
        {
            _pet.ShowBubble(msg);
            RefreshStatusChip();
        })));
        menu.Items.Add(CreateItem("🏠 我的房间", FeatureWindows.OpenRoom));
        menu.Items.Add(CreateItem("🌍 探索世界", () => FeatureWindows.OpenExplore(msg => _pet.ShowBubble(msg))));
        menu.Items.Add(CreateItem("🐾 第二只宠物", () => FeatureWindows.OpenSecondPet(msg => _pet.ShowBubble(msg))));
        menu.Items.Add(CreateItem("🎒 背包", FeatureWindows.OpenBag));
        menu.Items.Add(CreateItem("📕 收藏图鉴", FeatureWindows.OpenCollection));
        menu.Items.Add(CreateItem("📖 日记", FeatureWindows.OpenDiary));
        menu.Items.Add(CreateItem("⭐ 成就", FeatureWindows.OpenAchievements));
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateItem("😴 去睡觉", () =>
        {
            AppServices.Care.SleepRest();
            AppServices.Dreams.MaybePrepareAfterSleep();
            _pet.ShowBubble("Zzz…");
            _pet.PlayAction("sleep", 1600);
            RefreshStatusChip();
        }));
        menu.Items.Add(CreateItem("🌤 看看天气", async () =>
        {
            var w = await AppServices.Weather.RefreshAsync();
            _pet.ShowBubble(w ?? "今天也要加油");
            var wAct = SpriteLoader.WeatherAction(AppServices.Data.LastWeather ?? "");
            if (wAct is not null) _pet.PlayAction(wAct, 2000);
            else _pet.PlayAction(SpriteLoader.TimeOfDayAction(), 1800);
        }));
        menu.Items.Add(CreateItem("🔄 重新加载动画", () =>
        {
            _loader.Reload();
            _pet.Start();
            _pet.ShowBubble("动画已刷新");
        }));
        menu.Items.Add(CreateItem("⚙ 设置", FeatureWindows.OpenSettings));
        menu.Items.Add(CreateItem("❌ 退出", () =>
        {
            AppServices.Save.Save(AppServices.Data);
            Close();
        }));
        ContextMenu = menu;
    }

    private static MenuItem CreateItem(string header, Action action)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) => action();
        return item;
    }
}
