using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MaomaoDesktopPet.Core;

namespace MaomaoDesktopPet;

public partial class MainWindow : Window
{
    private readonly PetController _pet;
    private readonly SpriteLoader _loader;
    private readonly DispatcherTimer _tickTimer;

    private bool _dragging;
    private Point _dragStart;
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

        _tickTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _tickTimer.Tick += (_, _) => _pet.Tick();

        Loaded += (_, _) =>
        {
            PlaceBottomRight();
            _pet.Start();
            _tickTimer.Start();
        };

        BuildContextMenu();
    }

    private static string ResolveAssetsRoot()
    {
        var baseDir = AppContext.BaseDirectory;
        var candidates = new[]
        {
            Path.Combine(baseDir, "Assets", "Pet"),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Assets", "Pet")),
            Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "..", "..", "picture", "frames"))
        };

        foreach (var path in candidates)
        {
            if (Directory.Exists(path))
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

    private void OnWalkDelta(Vector delta)
    {
        var work = SystemParameters.WorkArea;
        var nextLeft = Left + delta.X;
        var nextTop = Top + delta.Y;
        Left = Math.Clamp(nextLeft, work.Left, work.Right - Width);
        Top = Math.Clamp(nextTop, work.Top, work.Bottom - Height);
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragging = true;
        _movedEnough = false;
        _dragStart = e.GetPosition(this);
        CaptureMouse();
    }

    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_dragging || e.LeftButton != MouseButtonState.Pressed)
            return;

        var pos = e.GetPosition(this);
        var delta = pos - _dragStart;
        if (!_movedEnough && delta.Length < 6)
            return;

        if (!_movedEnough)
        {
            _movedEnough = true;
            _pet.BeginDrag();
        }

        var screen = PointToScreen(pos);
        Left = screen.X - _dragStart.X;
        Top = screen.Y - _dragStart.Y;
    }

    private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_dragging)
            return;

        _dragging = false;
        ReleaseMouseCapture();

        if (_movedEnough)
            _pet.EndDrag();
        else
            _pet.HandleClick();
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

        menu.Items.Add(CreateItem("❤️ 互动", () => ShowComingSoon("互动")));
        menu.Items.Add(CreateItem("🍖 喂食", () => ShowComingSoon("喂食")));
        menu.Items.Add(CreateItem("👋 摸摸", () =>
        {
            OnBubbleChanged("呼噜呼噜～");
            _pet.HandleClick();
        }));
        menu.Items.Add(CreateItem("🎮 玩游戏", () => ShowComingSoon("游戏")));
        menu.Items.Add(CreateItem("💬 和我聊天", () => ShowComingSoon("聊天")));
        menu.Items.Add(CreateItem("👕 换衣服", () => ShowComingSoon("换装")));
        menu.Items.Add(CreateItem("🏠 我的房间", () => ShowComingSoon("房间")));
        menu.Items.Add(CreateItem("🎒 背包", () => ShowComingSoon("背包")));
        menu.Items.Add(CreateItem("📖 日记", () => ShowComingSoon("日记")));
        menu.Items.Add(CreateItem("⭐ 成就", () => ShowComingSoon("成就")));
        menu.Items.Add(new Separator());
        menu.Items.Add(CreateItem("🔄 重新加载动画", () =>
        {
            _loader.Reload();
            _pet.Start();
            OnBubbleChanged("动画已刷新");
        }));
        menu.Items.Add(CreateItem("⚙ 设置", () => ShowComingSoon("设置")));
        menu.Items.Add(CreateItem("❌ 退出", Close));

        ContextMenu = menu;
    }

    private static MenuItem CreateItem(string header, Action action)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) => action();
        return item;
    }

    private void ShowComingSoon(string feature)
    {
        OnBubbleChanged($"{feature}马上就来～");
    }
}
