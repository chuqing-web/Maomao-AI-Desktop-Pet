using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace MaomaoDesktopPet.Core;

public sealed class PetController
{
    private readonly SpriteLoader _loader;
    private readonly SpriteAnimator _animator;
    private readonly DispatcherTimer _behaviorTimer;
    private readonly Random _random = new();
    private PetState _state = PetState.Idle;
    private int _clickStreak;
    private DateTime _lastClickUtc = DateTime.MinValue;
    private DateTime _clickStateUntilUtc;
    private bool _expressionHold;
    private string? _activeBubbleAction;

    public event Action<ImageSource>? FrameChanged;
    public event Action<string?>? BubbleChanged;
    public event Action<Vector>? WalkDeltaRequested;
    public event Action? Clicked;
    public event Action? ChaseMouseRequested;
    public event Action? HideCornerRequested;
    public event Action? PeekEdgeRequested;
    public event Action<string>? BubbleActionRequested;

    public PetState State => _state;
    public string? ActiveBubbleAction => _activeBubbleAction;

    public PetController(SpriteLoader loader, SpriteAnimator animator)
    {
        _loader = loader;
        _animator = animator;
        _animator.FrameChanged += frame => FrameChanged?.Invoke(frame);
        _behaviorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3.2) };
        _behaviorTimer.Tick += (_, _) => OnBehaviorTick();
    }

    public void Start()
    {
        SetState(PetState.Idle);
        _behaviorTimer.Start();
    }

    public void ShowBubble(string? text, string? action = null)
    {
        _activeBubbleAction = action;
        BubbleChanged?.Invoke(text);
    }

    public void ActivateBubble()
    {
        if (_activeBubbleAction is null) return;
        var a = _activeBubbleAction;
        _activeBubbleAction = null;
        BubbleActionRequested?.Invoke(a);
    }

    public void PlayExpression(string key, int ms = 1400) => PlayAction(key, ms);

    public void PlayAction(string key, int ms = 1600)
    {
        _expressionHold = true;
        _state = PetState.Idle;
        _animator.SetFrames(_loader.GetAction(key), 5);
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            _expressionHold = false;
            if (_state != PetState.Drag)
                SetState(PetState.Idle);
        };
        timer.Start();
    }

    public void BeginDrag()
    {
        _clickStreak = 0;
        _expressionHold = false;
        SetState(PetState.Drag);
        ShowBubble("诶诶诶！");
    }

    public void UpdateDragSpeech(double deltaX)
    {
        if (_state != PetState.Drag) return;
        if (Math.Abs(deltaX) < 40) return;
        ShowBubble(deltaX < 0 ? "我们去哪？" : "这里风好大……");
    }

    public void EndDrag()
    {
        ShowBubble("啪！");
        var drop = _loader.GetByPrefixes("drop", ["inter_drop_"], PetState.Idle);
        _state = PetState.Idle;
        _animator.SetFrames(drop, 6);
        ScheduleReturnIdle(500);
        ScheduleClearBubble(1200);
    }

    public void HandleClick()
    {
        if (_state == PetState.Drag) return;
        Clicked?.Invoke();

        var now = DateTime.UtcNow;
        _clickStreak = (now - _lastClickUtc).TotalMilliseconds <= 700 ? _clickStreak + 1 : 1;
        _lastClickUtc = now;

        var mash = _clickStreak >= 8;
        ShowBubble(_clickStreak switch
        {
            1 => "嗯？",
            2 or 3 => "喵？",
            >= 4 and < 8 => "你是不是很闲呀……",
            _ => "救命！别戳啦！"
        });
        _clickStateUntilUtc = now.AddMilliseconds(mash ? 1100 : 900);
        SetState(PetState.Click, mash);
    }

    public void Tick()
    {
        if (_state == PetState.Click && DateTime.UtcNow >= _clickStateUntilUtc)
            SetState(PetState.Idle);
    }

    private void OnBehaviorTick()
    {
        if (_expressionHold || _state is PetState.Drag or PetState.Click) return;

        var roll = _random.Next(100);
        if (_state == PetState.Sleep)
        {
            if (roll < 35)
            {
                ShowBubble(null);
                SetState(PetState.Idle);
            }
            return;
        }

        if (_state == PetState.Walk)
        {
            SetState(PetState.Idle);
            return;
        }

        if (roll < 10)
        {
            SetState(PetState.Walk);
            WalkDeltaRequested?.Invoke(new Vector(_random.Next(-48, 49), _random.Next(-24, 25)));
        }
        else if (roll < 15)
        {
            PlayAction("run", 1200);
            WalkDeltaRequested?.Invoke(new Vector(_random.Next(-80, 81), _random.Next(-10, 11)));
            ShowBubble("跑跑跑！");
            ScheduleClearBubble(1200);
        }
        else if (roll < 19)
        {
            PlayAction(_random.Next(2) == 0 ? "jump" : "jump_desk", 1100);
            WalkDeltaRequested?.Invoke(new Vector(_random.Next(-20, 21), -28));
            ShowBubble("跳！");
            ScheduleClearBubble(1100);
        }
        else if (roll < 26)
        {
            ShowBubble("Zzz…");
            SetState(PetState.Sleep);
        }
        else if (roll < 32)
        {
            PlayAction(_random.Next(2) == 0 ? "yawn" : "stretch", 1600);
            ShowBubble(_random.Next(2) == 0 ? "哈欠……" : "伸个懒腰～");
            ScheduleClearBubble(1800);
        }
        else if (roll < 37)
        {
            PlayAction("lie", 1800);
            ShowBubble("趴一会儿……");
            ScheduleClearBubble(1800);
        }
        else if (roll < 41)
        {
            PlayAction("space", 1600);
            ShowBubble("正在发呆...");
            ScheduleClearBubble(1800);
        }
        else if (roll < 45)
        {
            PlayAction("tail", 1600);
            ShowBubble("尾巴好好玩～");
            ScheduleClearBubble(1600);
        }
        else if (roll < 49)
        {
            PlayAction("toy", 1600);
            ShowBubble("玩具时间！");
            ScheduleClearBubble(1600);
        }
        else if (roll < 52)
        {
            PlayAction("butterfly", 1800);
            ShowBubble("蝴蝶！等等我～");
            ScheduleClearBubble(1800);
        }
        else if (roll < 55)
        {
            PlayAction("spin", 1400);
            ShowBubble("转圈圈～");
            ScheduleClearBubble(1400);
        }
        else if (roll < 58)
        {
            PlayAction("window", 1600);
            ShowBubble("窗外有什么？");
            ScheduleClearBubble(1600);
        }
        else if (roll < 61)
        {
            PlayAction("sunbath", 1800);
            ShowBubble("晒太阳真舒服……");
            ScheduleClearBubble(1800);
        }
        else if (roll < 64)
        {
            PlayAction("clean", 1600);
            ShowBubble("洗脸洗脸～");
            ScheduleClearBubble(1600);
        }
        else if (roll < 67)
        {
            PlayAction("drink", 1500);
            ShowBubble("咕咚咕咚");
            ScheduleClearBubble(1500);
        }
        else if (roll < 70)
        {
            PlayAction("sneeze", 1200);
            ShowBubble("哈啾！");
            ScheduleClearBubble(1200);
        }
        else if (roll < 74)
        {
            ChaseMouseRequested?.Invoke();
            PlayAction("chase", 1500);
            ShowBubble("鼠标！我看见你了！");
            ScheduleClearBubble(1500);
        }
        else if (roll < 78)
        {
            HideCornerRequested?.Invoke();
            PlayAction("hide", 1600);
            ShowBubble("嘻嘻，躲起来～");
            ScheduleClearBubble(1600);
        }
        else if (roll < 81)
        {
            PeekEdgeRequested?.Invoke();
            PlayAction("climb", 1500);
            ShowBubble("探头看看……");
            ScheduleClearBubble(1500);
        }
        else if (roll < 84)
        {
            PlayAction("find", 1600);
            ShowBubble("我发现了什么！", "event");
            ScheduleClearBubble(2000);
        }
        else if (roll < 87)
        {
            PlayAction("talk", 1400);
            ShowBubble("嘿嘿。", "nudge");
            ScheduleClearBubble(2000);
        }
        else if (roll < 92)
        {
            var (text, action) = _random.Next(5) switch
            {
                0 => ("好无聊呀……", "play"),
                1 => ("今天吃什么？", "feed"),
                2 => ("你在干什么？", "chat"),
                3 => ("我有一个秘密。", "event"),
                _ => ("要不要玩一下？", "game")
            };
            PlayAction("talk", 1200);
            ShowBubble(text, action);
            ScheduleClearBubble(4000);
        }
    }

    private void SetState(PetState state, bool mashClick = false)
    {
        _state = state;
        var fps = state switch
        {
            PetState.Walk => 8,
            PetState.Drag => 6,
            PetState.Click => 6,
            PetState.Sleep => 3,
            _ => 5
        };
        _animator.SetFrames(_loader.GetFrames(state, mashClick), fps);
    }

    private void ScheduleReturnIdle(int ms)
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            if (_state != PetState.Drag) SetState(PetState.Idle);
        };
        timer.Start();
    }

    private void ScheduleClearBubble(int ms)
    {
        var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(ms) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            if (_state is not PetState.Sleep and not PetState.Drag)
            {
                _activeBubbleAction = null;
                BubbleChanged?.Invoke(null);
            }
        };
        timer.Start();
    }
}
