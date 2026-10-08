using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class DreamService
{
    private readonly PetData _data;
    private readonly Action _persist;
    private readonly AchievementService _achievements;

    private static readonly (string Id, string Title, string Text)[] Dreams =
    [
        ("can", "巨大猫罐头", "梦里有一座罐头山，我吃撑了还想吃。"),
        ("candy", "糖果森林", "树上长着软糖，风一吹就沙沙响。"),
        ("stars", "星空", "我坐在月亮上看你敲键盘，小小的像蚂蚁。"),
        ("sea", "海底", "我变成了潜水猫，和鱼打招呼。"),
        ("cloud", "云朵世界", "云好软，像我的肚皮。")
    ];

    public DreamService(PetData data, Action persist, AchievementService achievements)
    {
        _data = data;
        _persist = persist;
        _achievements = achievements;
    }

    public void MaybePrepareAfterSleep()
    {
        if (DateTime.Now.Hour is < 21 and > 8) return;
        if (_data.PendingDream is not null) return;
        if (Random.Shared.Next(100) < 55)
            _data.PendingDream = Dreams[Random.Shared.Next(Dreams.Length)].Id;
        _persist();
    }

    public string? PeekMorningLine()
    {
        if (_data.PendingDream is null) return null;
        return "我昨天做了一个梦。点气泡看看？";
    }

    public string? OpenDream()
    {
        if (_data.PendingDream is null) return null;
        var dream = Dreams.FirstOrDefault(d => d.Id == _data.PendingDream);
        if (dream.Id is null) dream = Dreams[0];
        _data.PendingDream = null;
        if (!_data.DreamFragments.Contains(dream.Id))
            _data.DreamFragments.Add(dream.Id);
        _data.UnlockedCollection.Add($"dream_{dream.Id}");
        _data.Coins += 12;
        _data.Mood = Math.Clamp(_data.Mood + 10, 0, 100);
        _achievements.Unlock("dreamer");
        _persist();
        return $"🌙 梦境：{dream.Title}\n{dream.Text}\n获得梦境碎片与金币！";
    }
}
