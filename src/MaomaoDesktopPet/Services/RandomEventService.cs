using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class RandomEventService
{
    private readonly PetData _data;
    private readonly Action _persist;
    private DateTime _nextAt = DateTime.Now.AddMinutes(2);

    private static readonly (string Text, int Coins, int Mood, string? Collect)[] Events =
    [
        ("我发现了一颗纽扣。", 5, 3, "button"),
        ("我刚刚试图理解你的代码。失败了。", 0, 5, "code_fail"),
        ("今天发现了彩虹！", 15, 10, "rainbow"),
        ("我好像发现了一个秘密……", 20, 8, "secret"),
        ("哈啾！刚才打了个喷嚏。", 0, 2, null),
        ("我梦见了巨大的猫罐头。", 8, 6, "dream_can"),
        ("窗外好像有蝴蝶。", 5, 4, "butterfly")
    ];

    public RandomEventService(PetData data, Action persist)
    {
        _data = data;
        _persist = persist;
    }

    public string? TryTrigger(DisturbMode mode)
    {
        if (mode == DisturbMode.Quiet) return null;
        if (DateTime.Now < _nextAt) return null;

        var minutes = mode == DisturbMode.Active ? 3 : 7;
        _nextAt = DateTime.Now.AddMinutes(minutes + Random.Shared.Next(0, 5));

        var e = Events[Random.Shared.Next(Events.Length)];
        _data.Coins += e.Coins;
        _data.Mood = Math.Clamp(_data.Mood + e.Mood, 0, 100);
        if (e.Collect is not null)
            _data.UnlockedCollection.Add(e.Collect);
        _persist();
        return e.Text;
    }
}
