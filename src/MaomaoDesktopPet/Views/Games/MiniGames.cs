using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using MaomaoDesktopPet.Services;

namespace MaomaoDesktopPet.Views.Games;

public static class CatchFishGame
{
    public static void Open(Action<string>? onBubble = null)
    {
        var w = new Window
        {
            Title = "接小鱼",
            Width = 420,
            Height = 480,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = new SolidColorBrush(Color.FromRgb(230, 245, 255))
        };
        var canvas = new Canvas();
        var pet = new TextBlock { Text = "🐱", FontSize = 36 };
        Canvas.SetLeft(pet, 180);
        Canvas.SetTop(pet, 380);
        canvas.Children.Add(pet);
        var scoreText = new TextBlock { Text = "分数 0", FontSize = 16, Margin = new Thickness(8) };
        var score = 0;
        var fishes = new List<(Ellipse Shape, double Y, double X, double Speed)>();
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
        var spawn = 0;
        timer.Tick += (_, _) =>
        {
            spawn++;
            if (spawn % 18 == 0)
            {
                var fish = new Ellipse { Width = 22, Height = 14, Fill = Brushes.Coral };
                var x = Random.Shared.Next(20, 360);
                Canvas.SetLeft(fish, x);
                Canvas.SetTop(fish, 0);
                canvas.Children.Add(fish);
                fishes.Add((fish, 0, x, 3 + Random.Shared.NextDouble() * 3));
            }

            for (var i = fishes.Count - 1; i >= 0; i--)
            {
                var f = fishes[i];
                f.Y += f.Speed;
                Canvas.SetTop(f.Shape, f.Y);
                fishes[i] = f;
                var petX = Canvas.GetLeft(pet);
                if (Math.Abs(f.X - petX) < 40 && f.Y > 360 && f.Y < 420)
                {
                    canvas.Children.Remove(f.Shape);
                    fishes.RemoveAt(i);
                    score++;
                    scoreText.Text = $"分数 {score}";
                }
                else if (f.Y > 440)
                {
                    canvas.Children.Remove(f.Shape);
                    fishes.RemoveAt(i);
                }
            }
        };

        w.KeyDown += (_, e) =>
        {
            var x = Canvas.GetLeft(pet);
            if (e.Key is Key.Left or Key.A) Canvas.SetLeft(pet, Math.Max(0, x - 24));
            if (e.Key is Key.Right or Key.D) Canvas.SetLeft(pet, Math.Min(360, x + 24));
        };
        w.Closing += (_, _) =>
        {
            timer.Stop();
            var coins = Math.Max(5, score * 3);
            AppServices.Care.RewardGame(coins, score * 2, Math.Min(15, score));
            onBubble?.Invoke($"接鱼结束！得分 {score}");
        };

        var root = new DockPanel();
        DockPanel.SetDock(scoreText, Dock.Top);
        root.Children.Add(scoreText);
        root.Children.Add(new TextBlock { Text = "← → 或 A D 移动，关闭窗口结算", Margin = new Thickness(8, 0, 8, 4) });
        root.Children.Add(canvas);
        w.Content = root;
        w.Loaded += (_, _) => { w.Focus(); timer.Start(); };
        w.Show();
    }
}

public static class DodgeGame
{
    public static void Open(Action<string>? onBubble = null)
    {
        var w = new Window
        {
            Title = "躲障碍",
            Width = 480,
            Height = 280,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = new SolidColorBrush(Color.FromRgb(255, 248, 240))
        };
        var canvas = new Canvas();
        var pet = new TextBlock { Text = "🐱", FontSize = 32 };
        Canvas.SetLeft(pet, 40);
        Canvas.SetTop(pet, 160);
        canvas.Children.Add(pet);
        var ground = 160.0;
        var vy = 0.0;
        var onGround = true;
        var obstacles = new List<(Rectangle R, double X)>();
        var alive = true;
        var frames = 0;
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(20) };
        timer.Tick += (_, _) =>
        {
            if (!alive) return;
            frames++;
            if (!onGround)
            {
                vy += 0.9;
                var y = Canvas.GetTop(pet) + vy;
                if (y >= ground)
                {
                    y = ground;
                    vy = 0;
                    onGround = true;
                }
                Canvas.SetTop(pet, y);
            }
            if (frames % 45 == 0)
            {
                var r = new Rectangle { Width = 18, Height = 28, Fill = Brushes.SeaGreen };
                Canvas.SetLeft(r, 460);
                Canvas.SetTop(r, ground + 8);
                canvas.Children.Add(r);
                obstacles.Add((r, 460));
            }
            for (var i = obstacles.Count - 1; i >= 0; i--)
            {
                var o = obstacles[i];
                o.X -= 5;
                Canvas.SetLeft(o.R, o.X);
                obstacles[i] = o;
                var py = Canvas.GetTop(pet);
                if (o.X < 70 && o.X > 20 && py > ground - 20)
                {
                    alive = false;
                    timer.Stop();
                    var score = frames / 10;
                    AppServices.Care.RewardGame(Math.Max(5, score / 2), score / 3, 5);
                    onBubble?.Invoke("撞到啦！");
                    MessageBox.Show($"游戏结束！坚持分 {score}");
                    w.Close();
                    return;
                }
                if (o.X < -20)
                {
                    canvas.Children.Remove(o.R);
                    obstacles.RemoveAt(i);
                }
            }
        };
        w.KeyDown += (_, e) =>
        {
            if ((e.Key is Key.Space or Key.Up or Key.W) && onGround)
            {
                onGround = false;
                vy = -12;
            }
        };
        var dodgeRoot = new DockPanel();
        var hint = new TextBlock { Text = "空格 / W 跳跃，撞到仙人掌就结束", Margin = new Thickness(8) };
        DockPanel.SetDock(hint, Dock.Top);
        dodgeRoot.Children.Add(hint);
        dodgeRoot.Children.Add(canvas);
        w.Content = dodgeRoot;
        w.Loaded += (_, _) => { w.Focus(); timer.Start(); };
        w.Closing += (_, _) => timer.Stop();
        w.Show();
    }
}

public static class WhackMoleGame
{
    public static void Open(Action<string>? onBubble = null)
    {
        var w = new Window
        {
            Title = "打地鼠",
            Width = 360,
            Height = 420,
            WindowStartupLocation = WindowStartupLocation.CenterScreen,
            Background = new SolidColorBrush(Color.FromRgb(245, 250, 240))
        };
        var grid = new UniformGrid { Rows = 3, Columns = 3, Margin = new Thickness(12) };
        var score = 0;
        var left = 20;
        var scoreText = new TextBlock { Text = "分数 0　剩余 20", FontSize = 16, Margin = new Thickness(12) };
        var buttons = new Button[9];
        var active = -1;
        for (var i = 0; i < 9; i++)
        {
            var idx = i;
            var b = new Button { Content = "🕳️", FontSize = 28, Margin = new Thickness(4) };
            b.Click += (_, _) =>
            {
                if (idx == active)
                {
                    score++;
                    active = -1;
                    b.Content = "🕳️";
                    scoreText.Text = $"分数 {score}　剩余 {left}";
                }
            };
            buttons[i] = b;
            grid.Children.Add(b);
        }
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(700) };
        timer.Tick += (_, _) =>
        {
            left--;
            if (active >= 0) buttons[active].Content = "🕳️";
            if (left <= 0)
            {
                timer.Stop();
                AppServices.Care.RewardGame(Math.Max(8, score * 2), score, Math.Min(12, score));
                onBubble?.Invoke($"打地鼠结束！{score} 分");
                MessageBox.Show($"结束！得分 {score}");
                w.Close();
                return;
            }
            active = Random.Shared.Next(9);
            buttons[active].Content = "🐭";
            scoreText.Text = $"分数 {score}　剩余 {left}";
        };
        var root = new DockPanel();
        DockPanel.SetDock(scoreText, Dock.Top);
        root.Children.Add(scoreText);
        root.Children.Add(grid);
        w.Content = root;
        w.Loaded += (_, _) => timer.Start();
        w.Closing += (_, _) => timer.Stop();
        w.Show();
    }
}

public static class GuessMoodGame
{
    public static void Open(Action<string>? onBubble = null)
    {
        var moods = new (string Key, string Label)[]
        {
            ("happy", "开心"), ("excited", "兴奋"), ("bored", "无聊"),
            ("sleepy", "困倦"), ("angry", "生气"), ("surprised", "震惊"), ("pity", "委屈")
        };
        var answer = moods[Random.Shared.Next(moods.Length)];
        var w = UiKit.CreateShell("猜表情", 360, 360);
        var panel = new StackPanel { Margin = new Thickness(20) };
        panel.Children.Add(UiKit.H1("毛毛现在是什么心情？"));
        panel.Children.Add(new TextBlock
        {
            Text = answer.Key switch
            {
                "happy" => "😆",
                "excited" => "🤩",
                "bored" => "😐",
                "sleepy" => "🥱",
                "angry" => "😤",
                "surprised" => "😱",
                _ => "🥺"
            },
            FontSize = 64,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        foreach (var m in moods.OrderBy(_ => Random.Shared.Next()))
        {
            panel.Children.Add(UiKit.Btn(m.Label, (_, _) =>
            {
                if (m.Key == answer.Key)
                {
                    AppServices.Care.RewardGame(25, 20, 10);
                    onBubble?.Invoke("答对啦！");
                    MessageBox.Show("答对了！🎉");
                }
                else
                {
                    AppServices.Care.RewardGame(5, 5, 0);
                    onBubble?.Invoke($"是{answer.Label}啦");
                    MessageBox.Show($"不对哦，是「{answer.Label}」");
                }
                w.Close();
            }));
        }
        w.Content = UiKit.Scroll(panel);
        w.Show();
    }
}
