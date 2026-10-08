using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class QuestService
{
    private readonly PetData _data;
    private readonly Action _persist;

    public QuestService(PetData data, Action persist)
    {
        _data = data;
        _persist = persist;
    }

    public void EnsureToday()
    {
        if (_data.QuestsDate.Date == DateTime.Today && _data.DailyQuests.Count > 0)
            return;

        _data.QuestsDate = DateTime.Today;
        _data.DailyQuests =
        [
            new QuestProgress { Id = "q_pet", Title = "摸摸我 3 次", Type = "pet", Target = 3, RewardCoins = 25, RewardExp = 20, RewardAffection = 3 },
            new QuestProgress { Id = "q_feed", Title = "喂我吃一次", Type = "feed", Target = 1, RewardCoins = 20, RewardExp = 15, RewardAffection = 2 },
            new QuestProgress { Id = "q_game", Title = "玩一局小游戏", Type = "game", Target = 1, RewardCoins = 30, RewardExp = 20, RewardAffection = 2 },
            new QuestProgress { Id = "q_chat", Title = "和我聊一句", Type = "chat", Target = 1, RewardCoins = 20, RewardExp = 15, RewardAffection = 3 },
            new QuestProgress { Id = "q_focus", Title = "专注 25 分钟", Type = "focus", Target = 1, RewardCoins = 40, RewardExp = 30, RewardAffection = 4 }
        ];
        _persist();
    }

    public void Progress(string type, int amount = 1)
    {
        EnsureToday();
        foreach (var q in _data.DailyQuests.Where(q => q.Type == type && !q.Claimed))
            q.Current = Math.Min(q.Target, q.Current + amount);
        _persist();
    }

    public string? Claim(string id)
    {
        var q = _data.DailyQuests.FirstOrDefault(x => x.Id == id);
        if (q is null) return "没有这个任务。";
        if (q.Claimed) return "已经领过啦。";
        if (q.Current < q.Target) return "还没完成哦。";
        q.Claimed = true;
        _data.Coins += q.RewardCoins;
        _data.Exp += q.RewardExp;
        while (_data.Exp >= CareService.ExpToNext(_data.Level))
        {
            _data.Exp -= CareService.ExpToNext(_data.Level);
            _data.Level++;
            _data.Coins += 30;
        }
        _data.Affection = Math.Clamp(_data.Affection + q.RewardAffection, 0, 120);
        _persist();
        return $"领取成功！+{q.RewardCoins} 金币 +{q.RewardExp} 经验";
    }
}
