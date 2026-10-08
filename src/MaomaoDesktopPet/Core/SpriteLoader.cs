using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MaomaoDesktopPet.Core;

public sealed class SpriteLoader
{
    private const int DecodeWidth = 320;

    private static readonly Dictionary<PetState, string[]> StatePrefixes = new()
    {
        [PetState.Idle] = ["idle_"],
        [PetState.Walk] = ["walk_"],
        [PetState.Sleep] = ["sleep_"],
        [PetState.Drag] = ["inter_drag_hold_", "inter_drag_slide_"],
        [PetState.Click] = ["inter_click_"],
    };

    /// <summary>Logical action key → file prefixes (first match wins for cache key).</summary>
    private static readonly Dictionary<string, string[]> ActionPrefixes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["happy"] = ["exp_happy_"],
        ["excited"] = ["exp_excited_"],
        ["bored"] = ["exp_bored_"],
        ["sleepy"] = ["exp_sleepy_"],
        ["sleep"] = ["sleep_"],
        ["angry"] = ["exp_angry_"],
        ["cry"] = ["exp_cry_"],
        ["sad"] = ["exp_cry_"],
        ["pity"] = ["exp_pity_"],
        ["surprised"] = ["exp_surprised_"],
        ["wink"] = ["exp_wink_"],
        ["normal"] = ["exp_normal_"],
        ["stretch"] = ["stretch_"],
        ["yawn"] = ["yawn_"],
        ["lie"] = ["lie_down_"],
        ["run"] = ["run_"],
        ["jump"] = ["jump_01_"],
        ["jump_desk"] = ["jump_desk_"],
        ["hover"] = ["inter_hover_"],
        ["chase"] = ["chase_"],
        ["hide"] = ["hide_"],
        ["climb"] = ["climb_window_"],
        ["butterfly"] = ["catch_butterfly_"],
        ["clean"] = ["clean_"],
        ["drink"] = ["drink_"],
        ["dream"] = ["dream_"],
        ["find"] = ["find_item_"],
        ["window"] = ["look_window_"],
        ["tail"] = ["play_tail_"],
        ["toy"] = ["play_toy_"],
        ["sneeze"] = ["sneeze_"],
        ["space"] = ["space_out_"],
        ["spin"] = ["spin_"],
        ["sunbath"] = ["sunbath_"],
        ["talk"] = ["talk_"],
        ["eat_fish"] = ["eat_fish_"],
        ["eat_milk"] = ["eat_milk_"],
        ["eat_cake"] = ["eat_cake_"],
        ["eat_strawberry"] = ["eat_strawberry_"],
        ["eat_snack"] = ["eat_snack_"],
        ["eat"] = ["eat_snack_", "eat_fish_"],
        ["weather_rain"] = ["weather_rain_"],
        ["weather_snow"] = ["weather_snow_"],
        ["weather_sunny"] = ["weather_sunny_"],
        ["time_morning"] = ["time_morning_"],
        ["time_noon"] = ["time_noon_"],
        ["time_evening"] = ["time_evening_"],
        ["time_night"] = ["time_night_"],
    };

    private readonly string _assetsRoot;
    private readonly Dictionary<string, IReadOnlyList<ImageSource>> _cache = new(StringComparer.OrdinalIgnoreCase);

    public SpriteLoader(string assetsRoot) => _assetsRoot = assetsRoot;
    public string AssetsRoot => _assetsRoot;

    public IReadOnlyList<ImageSource> GetFrames(PetState state, bool mashClick = false)
    {
        if (state == PetState.Click && mashClick)
            return GetByPrefixes("mash", ["inter_mash_"], PetState.Click);
        if (!StatePrefixes.TryGetValue(state, out var prefixes))
            return [PlaceholderFactory.Create(state)];
        return GetByPrefixes(state.ToString(), prefixes, state);
    }

    public IReadOnlyList<ImageSource> GetExpression(string moodKey) =>
        GetAction(moodKey);

    public IReadOnlyList<ImageSource> GetAction(string actionKey)
    {
        var key = actionKey.Trim().ToLowerInvariant();
        if (ActionPrefixes.TryGetValue(key, out var prefixes))
            return GetByPrefixes("act_" + key, prefixes, PetState.Idle);

        // fallback: treat as file prefix directly
        return GetByPrefixes("act_raw_" + key, [key.EndsWith('_') ? key : key + "_"], PetState.Idle);
    }

    public static string? FoodAction(string foodId) => foodId switch
    {
        "fish" => "eat_fish",
        "milk" => "eat_milk",
        "cake" => "eat_cake",
        "strawberry" => "eat_strawberry",
        "biscuit" or "candy" or "apple" or "can" or "mystery" => "eat_snack",
        "dark" => "angry",
        _ => "eat"
    };

    public static string? WeatherAction(string weather) => weather switch
    {
        "rain" or "storm" => "weather_rain",
        "snow" or "winter" => "weather_snow",
        "sunny" or "summer" => "weather_sunny",
        _ => null
    };

    public static string TimeOfDayAction()
    {
        var h = DateTime.Now.Hour;
        return h switch
        {
            >= 5 and < 11 => "time_morning",
            >= 11 and < 14 => "time_noon",
            >= 14 and < 18 => "time_noon",
            >= 18 and < 22 => "time_evening",
            _ => "time_night"
        };
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

    public void Reload() => _cache.Clear();

    private List<ImageSource> LoadByPrefixes(IEnumerable<string> prefixes)
    {
        var list = new List<ImageSource>();
        if (!Directory.Exists(_assetsRoot)) return list;

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
            if (frame is not null) list.Add(frame);
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
        catch { return null; }
    }
}
