using System.Net.Http;
using System.Text.Json;
using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class WeatherService
{
    private readonly PetData _data;
    private readonly Action _persist;
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(8) };

    public WeatherService(PetData data, Action persist)
    {
        _data = data;
        _persist = persist;
    }

    public async Task<string?> RefreshAsync()
    {
        if ((DateTime.Now - _data.LastWeatherCheck).TotalMinutes < 90 && _data.LastWeather is not null)
            return React(_data.LastWeather);

        try
        {
            // Open-Meteo, no key: IP-less default Beijing-ish coords; fine for vibe.
            var url = "https://api.open-meteo.com/v1/forecast?latitude=31.23&longitude=121.47&current=weather_code";
            var json = await Http.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var code = doc.RootElement.GetProperty("current").GetProperty("weather_code").GetInt32();
            var weather = code switch
            {
                0 or 1 => "sunny",
                2 or 3 => "cloudy",
                >= 51 and <= 67 => "rain",
                >= 71 and <= 77 => "snow",
                >= 80 and <= 82 => "rain",
                >= 95 => "storm",
                _ => "cloudy"
            };
            _data.LastWeather = weather;
            _data.LastWeatherCheck = DateTime.Now;
            MaybeAutoOutfit(weather);
            _persist();
            return React(weather);
        }
        catch
        {
            var season = SeasonTheme();
            _data.LastWeather = season;
            _data.LastWeatherCheck = DateTime.Now;
            _persist();
            return React(season);
        }
    }

    private void MaybeAutoOutfit(string weather)
    {
        if (weather == "rain" && _data.UnlockedOutfits.Contains("raincoat"))
            _data.EquippedOutfit = "raincoat";
        if (weather == "snow" && _data.UnlockedHats.Contains("crown") == false)
        {
            // soft hint only
        }
    }

    public static string SeasonTheme()
    {
        var m = DateTime.Now.Month;
        return m switch
        {
            3 or 4 or 5 => "spring",
            6 or 7 or 8 => "summer",
            9 or 10 or 11 => "autumn",
            _ => "winter"
        };
    }

    public string React(string weather) => weather switch
    {
        "sunny" => "☀️ 今天好晴！要不要戴墨镜？",
        "rain" => "🌧️ 今天下雨哦。我穿雨衣啦。",
        "snow" => "❄️ 下雪了！围巾呢围巾呢～",
        "storm" => "雷声好大……我躲你身后。",
        "spring" => "🌸 春天到了，房间想换成樱花主题吗？",
        "summer" => "🏖️ 好热呀，想去海边。",
        "autumn" => "🍂 落叶季，适合发呆。",
        "winter" => "冬天适合贴贴。",
        _ => "今天天气还不错。"
    };

    public void ApplySeasonalRoom()
    {
        var season = SeasonTheme();
        _data.RoomTheme = season;
        _data.Wallpaper = season switch
        {
            "spring" => "sakura",
            "summer" => "beach",
            "autumn" => "maple",
            _ => "snow"
        };
        _persist();
    }
}
