using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public static class FeatureGate
{
    public static bool Can(string feature, out string? reason)
    {
        var lv = AppServices.Data.Level;
        reason = feature switch
        {
            "outfit" when lv < UnlockRules.OutfitLevel => $"换衣服需要 Lv.{UnlockRules.OutfitLevel}（现在 Lv.{lv}）",
            "room" when lv < UnlockRules.RoomLevel => $"房间需要 Lv.{UnlockRules.RoomLevel}（现在 Lv.{lv}）",
            "games" when lv < UnlockRules.GamesLevel => $"小游戏需要 Lv.{UnlockRules.GamesLevel}（现在 Lv.{lv}）",
            "ai" when lv < UnlockRules.AiLevel => $"AI 聊天需要 Lv.{UnlockRules.AiLevel}（现在 Lv.{lv}）",
            "explore" when lv < UnlockRules.ExploreLevel => $"探索需要 Lv.{UnlockRules.ExploreLevel}（现在 Lv.{lv}）",
            "second" when lv < UnlockRules.SecondPetLevel => $"第二只宠物需要 Lv.{UnlockRules.SecondPetLevel}（现在 Lv.{lv}）",
            _ => null
        };
        return reason is null;
    }
}
