using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MaomaoDesktopPet.Core;

public sealed class SpriteLoader
{
    private const int DecodeWidth = 280;

    private static readonly Dictionary<PetState, string[]> StatePrefixes = new()
    {
        [PetState.Idle] = ["idle_"],
        [PetState.Walk] = ["walk_"],
        [PetState.Sleep] = ["sleep_"],
        [PetState.Drag] = ["inter_drag_hold_", "inter_drag_slide_"],
        [PetState.Click] = ["inter_click_"],
    };

    private readonly string _assetsRoot;
    private readonly Dictionary<string, IReadOnlyList<ImageSource>> _cache = new(StringComparer.OrdinalIgnoreCase);

    public SpriteLoader(string assetsRoot)
    {
        _assetsRoot = assetsRoot;
    }

    public string AssetsRoot => _assetsRoot;

    public IReadOnlyList<ImageSource> GetFrames(PetState state, bool mashClick = false)
    {
        if (state == PetState.Click && mashClick)
            return GetByPrefixes("mash", ["inter_mash_"], PetState.Click);

        if (!StatePrefixes.TryGetValue(state, out var prefixes))
            return [PlaceholderFactory.Create(state)];

        return GetByPrefixes(state.ToString(), prefixes, state);
    }

    public IReadOnlyList<ImageSource> GetByPrefixes(string cacheKey, IEnumerable<string> prefixes, PetState fallbackState)
    {
        if (_cache.TryGetValue(cacheKey, out var cached))
            return cached;

        var frames = LoadByPrefixes(prefixes);
        if (frames.Count == 0)
            frames = [PlaceholderFactory.Create(fallbackState)];

        _cache[cacheKey] = frames;
        return frames;
    }

    public void Reload()
    {
        _cache.Clear();
    }

    private List<ImageSource> LoadByPrefixes(IEnumerable<string> prefixes)
    {
        var list = new List<ImageSource>();
        if (!Directory.Exists(_assetsRoot))
            return list;

        var prefixList = prefixes.ToArray();
        var files = Directory.EnumerateFiles(_assetsRoot, "*.png")
            .Concat(Directory.EnumerateFiles(_assetsRoot, "*.PNG"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(path =>
            {
                var name = Path.GetFileName(path);
                return prefixList.Any(p => name.StartsWith(p, StringComparison.OrdinalIgnoreCase));
            })
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            var frame = TryLoad(file);
            if (frame is not null)
                list.Add(frame);
        }

        return list;
    }

    private static ImageSource? TryLoad(string file)
    {
        try
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.CreateOptions = BitmapCreateOptions.IgnoreColorProfile;
            bitmap.DecodePixelWidth = DecodeWidth;
            bitmap.UriSource = new Uri(file, UriKind.Absolute);
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch
        {
            return null;
        }
    }
}
