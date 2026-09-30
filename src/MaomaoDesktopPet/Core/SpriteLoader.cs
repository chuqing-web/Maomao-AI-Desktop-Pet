using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MaomaoDesktopPet.Core;

public sealed class SpriteLoader
{
    private readonly string _assetsRoot;
    private readonly Dictionary<PetState, IReadOnlyList<ImageSource>> _cache = new();

    public SpriteLoader(string assetsRoot)
    {
        _assetsRoot = assetsRoot;
    }

    public IReadOnlyList<ImageSource> GetFrames(PetState state)
    {
        if (_cache.TryGetValue(state, out var cached))
            return cached;

        var folder = Path.Combine(_assetsRoot, state.ToString().ToLowerInvariant());
        var frames = LoadFromFolder(folder);
        if (frames.Count == 0)
            frames = [PlaceholderFactory.Create(state)];

        _cache[state] = frames;
        return frames;
    }

    public void Reload()
    {
        _cache.Clear();
    }

    private static List<ImageSource> LoadFromFolder(string folder)
    {
        var list = new List<ImageSource>();
        if (!Directory.Exists(folder))
            return list;

        var files = Directory.EnumerateFiles(folder, "*.png")
            .Concat(Directory.EnumerateFiles(folder, "*.PNG"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase);

        foreach (var file in files)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(file, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                list.Add(bitmap);
            }
            catch
            {
                // skip broken frame
            }
        }

        return list;
    }
}
