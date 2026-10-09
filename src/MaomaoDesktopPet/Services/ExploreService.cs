using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class ExploreService
{
    private readonly PetData _data;
    private readonly Action _persist;
    private readonly AchievementService _achievements;

    private static readonly (string Item, string Name)[] Loot =
    [
        ("mushroom", "蘑菇"),
        ("pinecone", "松果"),
        ("feather", "羽毛"),
        ("stone", "神秘石头"),
        ("shell", "贝壳"),
        ("star_dust", "星尘")
    ];

    public ExploreService(PetData data, Action persist, AchievementService achievements)
    {
        _data = data;
        _persist = persist;
        _achievements = achievements;
    }

    public string? Start(string mapId)
    {
        if (_data.ExploringMap is not null)
            return "还在外面冒险呢，等等我回来～";
        if (!GameCatalog.Maps.TryGetValue(mapId, out var map))
            return "未知地图";
        if (map.Minutes <= 0)
            return "房间不用出门啦";

        _data.ExploringMap = mapId;
        _data.ExploreReturnAt = DateTime.Now.AddMinutes(map.Minutes);
        _data.UnlockedMaps.Add(mapId);
        _persist();
        return $"我今天去{map.Name}看看！大约 {map.Minutes} 分钟回来～";
    }

    public string? TryComplete()
    {
        if (_data.ExploringMap is null || _data.ExploreReturnAt is null)
            return null;
        if (DateTime.Now < _data.ExploreReturnAt)
            return null;

        var mapId = _data.ExploringMap;
        var mapName = GameCatalog.Maps.TryGetValue(mapId, out var m) ? m.Name : mapId;
        _data.ExploringMap = null;
        _data.ExploreReturnAt = null;

        var loot = Loot[Random.Shared.Next(Loot.Length)];
        _data.Inventory[loot.Item] = _data.Inventory.GetValueOrDefault(loot.Item) + 1;
        _data.UnlockedCollection.Add(loot.Item);
        _data.UnlockedCollection.Add($"map_{mapId}");
        var coins = 15 + Random.Shared.Next(20);
        _data.Coins += coins;
        _data.Mood = Math.Clamp(_data.Mood + 8, 0, 100);
        _achievements.Unlock("explorer");
        _persist();
        return $"我回来啦！从{mapName}带回了{loot.Name}，还有 {coins} 金币！";
    }
}
