using System.Windows.Media;
using System.Windows.Threading;

namespace MaomaoDesktopPet.Core;

public sealed class SpriteAnimator
{
    private readonly DispatcherTimer _timer;
    private IReadOnlyList<ImageSource> _frames = Array.Empty<ImageSource>();
    private int _index;

    public event Action<ImageSource>? FrameChanged;

    public SpriteAnimator()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
        _timer.Tick += (_, _) => Advance();
    }

    public void SetFrames(IReadOnlyList<ImageSource> frames, int fps = 8)
    {
        _frames = frames.Count > 0 ? frames : Array.Empty<ImageSource>();
        _index = 0;
        _timer.Interval = TimeSpan.FromMilliseconds(Math.Max(40, 1000.0 / Math.Max(1, fps)));

        if (_frames.Count == 0)
        {
            _timer.Stop();
            return;
        }

        FrameChanged?.Invoke(_frames[0]);
        if (_frames.Count == 1)
            _timer.Stop();
        else
            _timer.Start();
    }

    public void Stop() => _timer.Stop();

    private void Advance()
    {
        if (_frames.Count == 0)
            return;

        _index = (_index + 1) % _frames.Count;
        FrameChanged?.Invoke(_frames[_index]);
    }
}
