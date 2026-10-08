using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class AchievementService
{
    private readonly PetData _data;
    private readonly Action _persist;

    public event Action<string>? Unlocked;

    public AchievementService(PetData data, Action persist)
    {
        _data = data;
        _persist = persist;
    }

    public void Unlock(string id)
    {
        if (!_data.UnlockedAchievements.Add(id)) return;
        var a = GameCatalog.Achievements.FirstOrDefault(x => x.Id == id);
        Unlocked?.Invoke(string.IsNullOrEmpty(a.Title) ? id : a.Title);
        _persist();
    }

    public void Check()
    {
        Unlock("first_meet");
        if (_data.TotalPets > 0) Unlock("first_pet");
        if (_data.TotalFeeds >= 10) Unlock("feed_10");
        if (_data.TotalFeeds >= 100) Unlock("feed_100");
        if (_data.Affection >= 80) Unlock("friend");
        if (_data.Affection >= 100) Unlock("bestie");
        if (_data.LoginStreak >= 7) Unlock("early_bird");
        if (_data.TotalClicks >= 100) Unlock("click_100");
        if (_data.TotalGames >= 10) Unlock("gamer");
        if (_data.TotalFocusMinutes >= 60) Unlock("focus_60");
        if (_data.TotalChatMessages >= 30) Unlock("chatter");
        if (_data.UnlockedCollection.Count >= 10) Unlock("collector");
        if (_data.UnlockedOutfits.Count >= 4) Unlock("fashion");
        if (_data.TotalCompanionHours >= 10) Unlock("companion_10h");
        if (_data.SecondPetUnlocked) Unlock("second_friend");
        if (_data.DreamFragments.Count > 0) Unlock("dreamer");

        var now = DateTime.Now;
        if (now.Hour == 3 && now.Minute == 33) Unlock("hidden_333");
        if (_data.HeadPetsStreak >= 50) Unlock("hidden_head50");
        if (_data.ObserveIdleSeconds >= 600) Unlock("hidden_observe");
    }
}
