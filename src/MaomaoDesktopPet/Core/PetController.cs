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

    public event Action<ImageSource>? FrameChanged;
    public event Action<string?>? BubbleChanged;
    public event Action<Vector>? WalkDeltaRequested;

    public PetState State => _state;

    public PetController(SpriteLoader loader, SpriteAnimator animator)
    {
        _loader = loader;
        _animator = animator;
        _animator.FrameChanged += frame => FrameChanged?.Invoke(frame);

        _behaviorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _behaviorTimer.Tick += (_, _) => OnBehaviorTick();
    }

    public void Start()
    {
        SetState(PetState.Idle);
        _behaviorTimer.Start();
    }

    public void BeginDrag()
    {
        _clickStreak = 0;
        SetState(PetState.Drag);
        BubbleChanged?.Invoke("诶诶诶！");
    }

    public void EndDrag()
    {
        BubbleChanged?.Invoke("啪！");
        // 落地帧：优先 drop，没有则回 Idle
        var drop = _loader.GetByPrefixes("drop", ["inter_drop_"], PetState.Idle);
        _state = PetState.Idle;
        _animator.SetFrames(drop, 6);
        ScheduleReturnIdle(500);
        ScheduleClearBubble(1200);
    }

    public void HandleClick()
    {
        if (_state == PetState.Drag)
            return;

        var now = DateTime.UtcNow;
        if ((now - _lastClickUtc).TotalMilliseconds <= 700)
            _clickStreak++;
        else
            _clickStreak = 1;
        _lastClickUtc = now;

        var mash = _clickStreak >= 8;
        var text = _clickStreak switch
        {
            1 => "嗯？",
            2 or 3 => "喵？",
            >= 4 and < 8 => "你是不是很闲呀……",
            _ => "救命！别戳啦！"
        };

        BubbleChanged?.Invoke(text);
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
        if (_state is PetState.Drag or PetState.Click)
            return;

        var roll = _random.Next(100);
        if (_state == PetState.Sleep)
        {
            if (roll < 40)
            {
                BubbleChanged?.Invoke(null);
                SetState(PetState.Idle);
            }
            return;
        }

        if (_state == PetState.Walk)
        {
            SetState(PetState.Idle);
            return;
        }

        if (roll < 25)
        {
            SetState(PetState.Walk);
            var dx = _random.Next(-40, 41);
            var dy = _random.Next(-20, 21);
            WalkDeltaRequested?.Invoke(new Vector(dx, dy));
        }
        else if (roll < 40)
        {
            BubbleChanged?.Invoke("Zzz…");
            SetState(PetState.Sleep);
        }
        else if (roll < 55)
        {
            BubbleChanged?.Invoke(_random.Next(2) == 0 ? "好无聊呀……" : "嘿嘿。");
            ScheduleClearBubble(2000);
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
            if (_state != PetState.Drag)
                SetState(PetState.Idle);
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
                BubbleChanged?.Invoke(null);
        };
        timer.Start();
    }
}
