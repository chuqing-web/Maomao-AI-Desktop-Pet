using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public static class AppServices
{
    private static bool _ready;

    public static PetData Data { get; private set; } = null!;
    public static SaveService Save { get; private set; } = null!;
    public static CareService Care { get; private set; } = null!;
    public static AchievementService Achievements { get; private set; } = null!;
    public static QuestService Quests { get; private set; } = null!;
    public static DiaryService Diary { get; private set; } = null!;
    public static RandomEventService Events { get; private set; } = null!;
    public static AiService Ai { get; private set; } = null!;
    public static EmotionService Emotion { get; private set; } = null!;
    public static ExploreService Explore { get; private set; } = null!;
    public static DreamService Dreams { get; private set; } = null!;
    public static WeatherService Weather { get; private set; } = null!;
    public static DesktopCompanionService Desktop { get; private set; } = null!;
    public static MemoryStore Memory => Save.Memory;

    public static void Initialize()
    {
        if (_ready) return;
        Save = new SaveService();
        Data = Save.LoadOrCreate();

        void Persist() => Save.Save(Data);

        Achievements = new AchievementService(Data, Persist);
        Quests = new QuestService(Data, Persist);
        Diary = new DiaryService(Data, Persist, Save.Memory);
        Events = new RandomEventService(Data, Persist);
        Care = new CareService(Data, Persist, Achievements, Quests);
        Ai = new AiService(Data, Persist, Quests, Achievements, Save.Memory);
        Emotion = new EmotionService(Data, Persist);
        Explore = new ExploreService(Data, Persist, Achievements);
        Dreams = new DreamService(Data, Persist, Achievements);
        Weather = new WeatherService(Data, Persist);
        Desktop = new DesktopCompanionService(Data);

        Care.ApplyOfflineDecay();
        HandleLoginStreak();
        HandleBirthday();
        Quests.EnsureToday();
        Diary.EnsureToday();
        Emotion.Recompute();
        Achievements.Check();
        Persist();
        _ready = true;
    }

    private static void HandleLoginStreak()
    {
        var today = DateTime.Today;
        if (Data.LastLoginDate.Date == today)
            return;

        if (Data.LastLoginDate.Date == today.AddDays(-1))
            Data.LoginStreak++;
        else
            Data.LoginStreak = 1;

        Data.LastLoginDate = today;
        Data.Coins += 10 + Math.Min(20, Data.LoginStreak * 2);
        if (Data.LoginStreak == 1)
            Diary.AddMemory("主人上线见面。");
        if (Data.LoginStreak >= 7)
        {
            Achievements.Unlock("early_bird");
            Data.Inventory["mystery"] = Data.Inventory.GetValueOrDefault("mystery") + 1;
            Diary.AddMemory($"连续见面 {Data.LoginStreak} 天，收到神秘礼物！");
        }

        // 社恐隐藏成就：7 天没聊天
        if (Data.LastChatAt != DateTime.MinValue &&
            (today - Data.LastChatAt.Date).TotalDays >= 7)
            Achievements.Unlock("hidden_shy");
    }

    private static void HandleBirthday()
    {
        var today = DateTime.Today;
        if (Data.PetBirthday.Month == today.Month && Data.PetBirthday.Day == today.Day)
        {
            Data.RoomTheme = "birthday";
            Data.Coins += 50;
            Data.Inventory["cake"] = Data.Inventory.GetValueOrDefault("cake") + 1;
            Diary.AddMemory("今天是我的生日！房间变成了派对。");
        }
        if (Data.OwnerBirthday is { } ob && ob.Month == today.Month && ob.Day == today.Day)
        {
            Data.Coins += 30;
            Diary.AddMemory($"今天是{Data.OwnerName}的生日，我准备了小礼物。");
        }
    }

    public static void MarkInteract()
    {
        Data.LastInteractAt = DateTime.Now;
        Data.ObserveIdleSeconds = 0;
        Emotion.Recompute();
    }
}
