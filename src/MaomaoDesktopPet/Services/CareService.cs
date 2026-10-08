using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class CareService
{
    private readonly PetData _data;
    private readonly Action _persist;
    private readonly AchievementService _achievements;
    private readonly QuestService _quests;

    public event Action? Changed;

    public CareService(PetData data, Action persist, AchievementService achievements, QuestService quests)
    {
        _data = data;
        _persist = persist;
        _achievements = achievements;
        _quests = quests;
    }

    public void ApplyOfflineDecay()
    {
        var hours = Math.Clamp((DateTime.Now - _data.LastSeenAt).TotalHours, 0, 72);
        if (hours < 0.05) return;
        _data.Hunger = Clamp(_data.Hunger - hours * 2.5);
        _data.Energy = Clamp(_data.Energy - hours * 1.2);
        _data.Cleanliness = Clamp(_data.Cleanliness - hours * 0.8);
        _data.Mood = Clamp(_data.Mood - hours * 1.0);
        _data.TotalCompanionHours += hours;
    }

    public void TickMinute()
    {
        _data.Hunger = Clamp(_data.Hunger - 0.4);
        _data.Energy = Clamp(_data.Energy - 0.25);
        _data.Cleanliness = Clamp(_data.Cleanliness - 0.15);
        if (_data.Hunger < 30 || _data.Energy < 25)
            _data.Mood = Clamp(_data.Mood - 0.5);
        else
            _data.Mood = Clamp(_data.Mood + 0.1);

        if (_data.PlacedFurniture.Contains("plant_small") && DateTime.Now.Minute == 0)
            _data.Coins += 1;

        Notify();
    }

    public string Feed(string foodId)
    {
        var food = GameCatalog.Foods.FirstOrDefault(f => f.Id == foodId);
        if (food is null) return "没有这种食物。";
        if (!_data.Inventory.TryGetValue(foodId, out var count) || count <= 0)
            return $"背包里没有{food.Name}了。";

        _data.Inventory[foodId] = count - 1;
        var hunger = food.Hunger;
        var mood = food.Mood;
        var energy = food.Energy;
        var affection = food.Affection;
        var msg = $"吃掉了{food.Name}！";

        if (food.Special == "random")
        {
            var r = Random.Shared.Next(3);
            if (r == 0) { mood += 15; msg = "神秘料理……意外好吃！心情大好！"; }
            else if (r == 1) { energy += 15; msg = "神秘料理让我充满力量！"; }
            else { mood -= 10; msg = "神秘料理味道怪怪的……"; }
        }
        else if (food.Special == "dark")
        {
            msg = "……你确定这是给我吃的？心情 -20";
        }
        else if (food.Special == "luck")
        {
            _data.Luck = Math.Clamp(_data.Luck + 5, 0, 100);
            msg = $"吃掉了{food.Name}！幸运 +5";
        }

        _data.Hunger = Clamp(_data.Hunger + hunger);
        _data.Mood = Clamp(_data.Mood + mood);
        _data.Energy = Clamp(_data.Energy + energy);
        AddAffection(affection);
        _data.TotalFeeds++;
        _data.UnlockedCollection.Add($"food_{foodId}");
        _data.LastInteractAt = DateTime.Now;
        AddExp(8);
        _quests.Progress("feed", 1);
        _achievements.Check();
        Notify();
        return msg;
    }

    public string Pet(string area)
    {
        var (mood, affection, line) = area switch
        {
            "head" => (4.0, 2, "呼噜呼噜～摸头最幸福了"),
            "face" => (3.5, 2, "脸脸软软的……有点害羞"),
            "ear" => (3.0, 1, "呼……耳朵好舒服"),
            "chin" => (5.0, 2, "继续继续～"),
            "belly" => (2.0, 1, "肚子也可以哦"),
            "tail" => (-2.0, 0, "不要碰那里！"),
            _ => (3.0, 1, "嗯嗯～")
        };

        if (area == "head") _data.HeadPetsStreak++;
        else _data.HeadPetsStreak = 0;

        _data.Mood = Clamp(_data.Mood + mood);
        AddAffection(affection);
        _data.TotalPets++;
        _data.LastInteractAt = DateTime.Now;
        AddExp(4);
        _quests.Progress("pet", 1);
        _achievements.Check();
        Notify();
        return line;
    }

    public void RegisterClick()
    {
        _data.TotalClicks++;
        _data.LastInteractAt = DateTime.Now;
        _quests.Progress("click", 1);
        if (DateTime.Now.Hour is >= 0 and < 5)
            _achievements.Unlock("night_owl");
        _achievements.Check();
        Notify();
    }

    public void SleepRest()
    {
        var bonus = _data.PlacedFurniture.Contains("bed_basic") ? 25 : 15;
        _data.Energy = Clamp(_data.Energy + bonus);
        _data.Mood = Clamp(_data.Mood + 5);
        Notify();
    }

    public void RewardGame(int coins, int exp, int mood)
    {
        var mult = _data.PlacedFurniture.Contains("console") ? 1.1 : 1.0;
        _data.Coins += (int)(coins * mult);
        AddExp((int)(exp * mult));
        _data.Mood = Clamp(_data.Mood + mood);
        _data.TotalGames++;
        _quests.Progress("game", 1);
        _achievements.Check();
        Notify();
    }

    public void RewardFocus(int minutes)
    {
        var mult = _data.PlacedFurniture.Contains("bookshelf") ? 1.1 : 1.0;
        _data.Coins += (int)(20 * mult);
        AddExp((int)(10 * mult));
        _data.Mood = Clamp(_data.Mood + 3);
        _data.TotalFocusMinutes += minutes;
        if (_data.LastFocusDate.Date == DateTime.Today.AddDays(-1))
            _data.FocusStreakDays++;
        else if (_data.LastFocusDate.Date != DateTime.Today)
            _data.FocusStreakDays = 1;
        _data.LastFocusDate = DateTime.Today;
        _quests.Progress("focus", 1);
        _achievements.Check();
        Notify();
    }

    public void AddCoins(int n) { _data.Coins += n; Notify(); }
    public void AddExp(int n)
    {
        _data.Exp += n;
        while (_data.Exp >= ExpToNext(_data.Level))
        {
            _data.Exp -= ExpToNext(_data.Level);
            _data.Level++;
            _data.Coins += 30;
        }
    }

    public void AddAffection(int n)
    {
        if (n == 0) return;
        _data.Affection = Math.Clamp(_data.Affection + n, 0, 120);
    }

    public bool Buy(string category, string id, int price)
    {
        if (_data.Coins < price) return false;
        _data.Coins -= price;
        if (category == "food")
        {
            _data.Inventory[id] = _data.Inventory.GetValueOrDefault(id) + 1;
        }
        else if (category == "outfit")
        {
            if (!_data.UnlockedOutfits.Contains(id))
                _data.UnlockedOutfits.Add(id);
        }
        else if (category == "furniture")
        {
            if (!_data.UnlockedFurniture.Contains(id))
                _data.UnlockedFurniture.Add(id);
        }
        _achievements.Check();
        Notify();
        return true;
    }

    public static int ExpToNext(int level) => 40 + level * 20;

    private void Notify()
    {
        Changed?.Invoke();
        _persist();
    }

    private static double Clamp(double v) => Math.Clamp(v, 0, 100);
}
