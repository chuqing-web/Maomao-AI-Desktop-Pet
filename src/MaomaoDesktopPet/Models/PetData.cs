using System.Text.Json.Serialization;

namespace MaomaoDesktopPet.Models;

public enum PersonalityType
{
    Clingy,
    Tsundere,
    Soft,
    Silly,
    Scholar,
    Lazy
}

public enum DisturbMode
{
    Quiet,
    Companion,
    Active
}

public enum MischiefMode
{
    Off,
    Mild,
    Full
}

public enum EmotionKind
{
    Happy,
    Excited,
    Normal,
    Sleepy,
    Bored,
    Pity,
    Angry,
    Shy,
    Sad,
    Surprised
}

public sealed class PetData
{
    public string PetName { get; set; } = "毛毛";
    public string OwnerName { get; set; } = "主人";
    public bool OnboardingDone { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? OwnerBirthday { get; set; }
    public DateTime PetBirthday { get; set; } = DateTime.Today;
    public DateTime LastSeenAt { get; set; } = DateTime.Now;
    public DateTime LastLoginDate { get; set; } = DateTime.MinValue;
    public DateTime LastInteractAt { get; set; } = DateTime.Now;
    public DateTime LastChatAt { get; set; } = DateTime.MinValue;
    public int LoginStreak { get; set; }

    public double Hunger { get; set; } = 80;
    public double Mood { get; set; } = 80;
    public double Energy { get; set; } = 80;
    public double Cleanliness { get; set; } = 90;
    public int Affection { get; set; } = 10;
    public int Level { get; set; } = 1;
    public int Exp { get; set; }
    public int Coins { get; set; } = 100;
    public int Luck { get; set; } = 5;
    public PersonalityType Personality { get; set; } = PersonalityType.Soft;
    public EmotionKind Emotion { get; set; } = EmotionKind.Normal;
    public string GrowthStage => Level switch
    {
        < 10 => "幼年期",
        < 20 => "成长期",
        < 30 => "成熟期",
        _ => "特别形态"
    };

    public Dictionary<string, int> Inventory { get; set; } = new()
    {
        ["fish"] = 5,
        ["milk"] = 3,
        ["biscuit"] = 2,
        ["cake"] = 1,
        ["strawberry"] = 2,
        ["apple"] = 1,
        ["candy"] = 1,
        ["can"] = 1,
        ["mystery"] = 1,
        ["dark"] = 1,
        ["yarn"] = 1
    };

    public List<string> UnlockedOutfits { get; set; } = ["default", "hoodie_blue"];
    public string EquippedOutfit { get; set; } = "hoodie_blue";
    public string EquippedHat { get; set; } = "none";
    public List<string> UnlockedHats { get; set; } = ["none"];
    public List<string> UnlockedFurniture { get; set; } = ["bed_basic", "plant_small", "sofa_soft"];
    public List<string> PlacedFurniture { get; set; } = ["bed_basic", "plant_small"];
    public string Wallpaper { get; set; } = "cream";
    public string Floor { get; set; } = "wood";
    public string RoomTheme { get; set; } = "default";

    public HashSet<string> UnlockedAchievements { get; set; } = [];
    public HashSet<string> UnlockedCollection { get; set; } = ["idle", "walk", "sleep"];
    public HashSet<string> UnlockedMaps { get; set; } = ["room"];
    public List<QuestProgress> DailyQuests { get; set; } = [];
    public DateTime QuestsDate { get; set; } = DateTime.MinValue;

    public List<DiaryEntry> Diary { get; set; } = [];
    public List<MemoryEntry> Memories { get; set; } = [];
    public List<ChatMessage> ChatHistory { get; set; } = [];
    public List<TodoItem> Todos { get; set; } = [];
    public List<string> DreamFragments { get; set; } = [];
    public DateTime? ExploreReturnAt { get; set; }
    public string? ExploringMap { get; set; }
    public string? PendingDream { get; set; }

    public bool SecondPetUnlocked { get; set; }
    public string? SecondPetId { get; set; }
    public string? SecondPetName { get; set; }
    public int PetRelation { get; set; } = 40;

    public AppSettings Settings { get; set; } = new();

    public int TotalClicks { get; set; }
    public int TotalPets { get; set; }
    public int HeadPetsStreak { get; set; }
    public int TotalFeeds { get; set; }
    public int TotalGames { get; set; }
    public int TotalFocusMinutes { get; set; }
    public int TotalChatMessages { get; set; }
    public int FocusStreakDays { get; set; }
    public DateTime LastFocusDate { get; set; } = DateTime.MinValue;
    public double TotalCompanionHours { get; set; }
    public int ObserveIdleSeconds { get; set; }
    public DateTime LastWaterRemindAt { get; set; } = DateTime.MinValue;
    public string? LastWeather { get; set; }
    public DateTime LastWeatherCheck { get; set; } = DateTime.MinValue;

    [JsonIgnore]
    public string RelationTitle => Affection switch
    {
        < 20 => "陌生",
        < 40 => "认识",
        < 60 => "熟悉",
        < 80 => "朋友",
        < 100 => "亲密",
        _ => "挚友"
    };

    [JsonIgnore]
    public int DaysTogether => Math.Max(1, (DateTime.Today - CreatedAt.Date).Days + 1);
}

public sealed class TodoItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public string Text { get; set; } = "";
    public bool Done { get; set; }
}

public sealed class AppSettings
{
    public DisturbMode DisturbMode { get; set; } = DisturbMode.Companion;
    public bool AllowMischief { get; set; } = true;
    public MischiefMode MischiefMode { get; set; } = MischiefMode.Mild;
    public bool WaterReminder { get; set; } = true;
    public bool WorkReminder { get; set; } = true;
    public bool SystemStatusReact { get; set; } = true;
    public string? AiApiBase { get; set; }
    public string? AiApiKey { get; set; }
    public string AiModel { get; set; } = "gpt-4o-mini";
    public bool UseCloudAi { get; set; }
    public double AiTemperature { get; set; } = 0.85;
    public int AiMaxTokens { get; set; } = 280;
}

public sealed class QuestProgress
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string Type { get; set; } = "";
    public int Target { get; set; } = 1;
    public int Current { get; set; }
    public bool Claimed { get; set; }
    public int RewardCoins { get; set; } = 20;
    public int RewardExp { get; set; } = 15;
    public int RewardAffection { get; set; } = 2;
}

public sealed class DiaryEntry
{
    public DateTime Date { get; set; }
    public string Text { get; set; } = "";
}

public sealed class MemoryEntry
{
    public DateTime At { get; set; } = DateTime.Now;
    public string Text { get; set; } = "";
}

public sealed class ChatMessage
{
    public DateTime At { get; set; } = DateTime.Now;
    public string Role { get; set; } = "user";
    public string Text { get; set; } = "";
}

public sealed class CatalogItem
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public double Hunger { get; set; }
    public double Mood { get; set; }
    public double Energy { get; set; }
    public int Affection { get; set; }
    public int Price { get; set; }
    public string? Special { get; set; }
}

public static class GameCatalog
{
    public static readonly CatalogItem[] Foods =
    [
        new() { Id = "fish", Name = "小鱼干", Category = "food", Hunger = 15, Mood = 5, Description = "经典小零食", Price = 15 },
        new() { Id = "milk", Name = "牛奶", Category = "food", Hunger = 10, Mood = 10, Energy = 5, Description = "暖暖的", Price = 15 },
        new() { Id = "biscuit", Name = "饼干", Category = "food", Hunger = 12, Mood = 6, Description = "咔嚓脆脆", Price = 12 },
        new() { Id = "cake", Name = "蛋糕", Category = "food", Hunger = 20, Mood = 20, Description = "幸运小甜点", Special = "luck", Price = 25 },
        new() { Id = "strawberry", Name = "草莓", Category = "food", Hunger = 10, Mood = 15, Affection = 5, Description = "甜甜的", Price = 18 },
        new() { Id = "apple", Name = "苹果", Category = "food", Hunger = 14, Mood = 8, Description = "一天一苹果", Price = 14 },
        new() { Id = "candy", Name = "糖果", Category = "food", Hunger = 6, Mood = 18, Description = "开心糖", Price = 16 },
        new() { Id = "can", Name = "猫罐头", Category = "food", Hunger = 28, Mood = 12, Energy = 5, Description = "豪华大餐", Price = 30 },
        new() { Id = "mystery", Name = "神秘料理", Category = "food", Hunger = 12, Mood = 8, Description = "不知道会发生什么", Special = "random", Price = 20 },
        new() { Id = "dark", Name = "黑暗料理", Category = "food", Hunger = 5, Mood = -20, Description = "你确定这是给我吃的？", Special = "dark", Price = 5 }
    ];

    public static readonly Dictionary<string, (string Name, string Desc, int Price)> Outfits = new()
    {
        ["default"] = ("素颜毛毛", "最原本的样子", 0),
        ["hoodie_blue"] = ("蓝白耳机卫衣", "经典造型", 0),
        ["bear"] = ("小熊卫衣", "软软抱抱", 90),
        ["sailor"] = ("水手服", "哟西！", 100),
        ["pajamas"] = ("星星睡衣", "睡觉更香", 80),
        ["chef"] = ("厨师服", "黑暗料理专业户", 110),
        ["coder"] = ("程序员服", "今天也要写代码", 120),
        ["raincoat"] = ("小雨衣", "下雨也不怕", 120),
        ["wizard"] = ("魔法师服", "喵法咏唱", 160),
        ["santa"] = ("圣诞装", "叮叮当", 150),
        ["astronaut"] = ("宇航服", "准备去月球", 200)
    };

    public static readonly Dictionary<string, (string Name, string Desc, int Price)> Hats = new()
    {
        ["none"] = ("无帽子", "光溜溜", 0),
        ["top"] = ("礼帽", "绅士猫", 70),
        ["grad"] = ("学士帽", "学霸附体", 80),
        ["cap"] = ("鸭舌帽", "街头感", 60),
        ["pumpkin"] = ("南瓜头", "万圣节", 90),
        ["crown"] = ("皇冠", "本殿下", 150),
        ["bunny"] = ("兔耳朵", "假耳真可爱", 85),
        ["unicorn"] = ("独角兽角", "闪闪发光", 120)
    };

    public static readonly Dictionary<string, (string Name, string Desc, int Price, string Buff)> Furniture = new()
    {
        ["bed_basic"] = ("小床", "睡觉恢复更快", 0, "sleep"),
        ["sofa_soft"] = ("软沙发", "休息心情更好", 60, "mood"),
        ["plant_small"] = ("绿植", "每日产少量金币", 40, "coins"),
        ["desk"] = ("小书桌", "陪伴工作时更安心", 70, "work"),
        ["bookshelf"] = ("书架", "专注效率提升", 100, "focus"),
        ["console"] = ("小游戏机", "小游戏奖励提升", 120, "game"),
        ["lamp"] = ("暖光灯", "晚上更安心", 50, "night"),
        ["carpet"] = ("地毯", "房间更温馨", 55, "mood"),
        ["window"] = ("小窗户", "可以看外面", 65, "explore"),
        ["toybox"] = ("玩具箱", "无聊时自己玩", 75, "bored")
    };

    public static readonly Dictionary<string, (string Name, string Desc, int Minutes)> Maps = new()
    {
        ["room"] = ("房间", "安全的小窝", 0),
        ["town"] = ("小镇", "逛逛商店街", 3),
        ["forest"] = ("森林", "蘑菇与松果", 5),
        ["beach"] = ("海边", "听听海浪", 6),
        ["stars"] = ("星空", "摘一颗小星星", 8)
    };

    public static readonly (string Id, string Name)[] CompanionPets =
    [
        ("dog", "小狗"),
        ("rabbit", "小兔"),
        ("hamster", "仓鼠"),
        ("fox", "小狐狸")
    ];

    public static readonly (string Id, string Title, string Desc, bool Hidden)[] Achievements =
    [
        ("first_meet", "第一次见面", "第一次启动毛毛", false),
        ("first_pet", "第一次摸摸", "摸摸毛毛一次", false),
        ("feed_10", "投喂新手", "累计喂食 10 次", false),
        ("feed_100", "投喂大师", "累计喂食 100 次", false),
        ("friend", "好朋友", "亲密度达到 80", false),
        ("bestie", "挚友", "亲密度达到 100", false),
        ("night_owl", "夜猫子", "凌晨互动", false),
        ("early_bird", "早起鸟", "连续 7 天登录", false),
        ("click_100", "疯狂点击", "累计点击 100 次", false),
        ("gamer", "小玩家", "完成 10 局小游戏", false),
        ("focus_60", "专注达人", "累计专注 60 分钟", false),
        ("chatter", "话痨搭子", "聊天 30 条", false),
        ("collector", "收藏家", "解锁 10 个收藏", false),
        ("fashion", "小小模特", "解锁 4 套服装", false),
        ("companion_10h", "陪伴工作", "累计陪伴 10 小时", false),
        ("explorer", "小小探险家", "完成一次探索", false),
        ("dreamer", "梦境旅人", "获得梦境碎片", false),
        ("second_friend", "新朋友", "迎来第二只宠物", false),
        ("hidden_333", "你还没睡？", "凌晨 3:33 打开", true),
        ("hidden_head50", "别摸了！", "连续摸头 50 次", true),
        ("hidden_observe", "观察者", "连续观察 10 分钟", true),
        ("hidden_shy", "社恐", "连续 7 天不聊天", true)
    ];
}
