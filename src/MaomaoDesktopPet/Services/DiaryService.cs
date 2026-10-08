using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class DiaryService
{
    private readonly PetData _data;
    private readonly Action _persist;
    private readonly MemoryStore _memory;

    public DiaryService(PetData data, Action persist, MemoryStore memory)
    {
        _data = data;
        _persist = persist;
        _memory = memory;
    }

    public void EnsureToday()
    {
        if (_data.Diary.Any(d => d.Date.Date == DateTime.Today))
            return;

        var weekday = DateTime.Today.DayOfWeek switch
        {
            DayOfWeek.Monday => "又是周一……",
            DayOfWeek.Friday => "周五！接近周末啦。",
            DayOfWeek.Sunday => "周日适合贴贴。",
            _ => "普通的一天。"
        };
        var memLine = _data.Memories.Count == 0
            ? "今天还没有新的记忆碎片。\n"
            : $"我记得最近一件事：{_data.Memories[0].Text}\n";
        var text =
            $"{DateTime.Today:yyyy年M月d日}\n\n" +
            $"{weekday}\n" +
            $"今天主人摸了我 {_data.TotalPets} 次（累计）。\n" +
            $"喂食累计 {_data.TotalFeeds} 次，聊天 {_data.TotalChatMessages} 句。\n" +
            $"心情 {_data.Mood:0}，饱腹 {_data.Hunger:0}，精力 {_data.Energy:0}。\n" +
            $"我们相识 {_data.DaysTogether} 天，现在是「{_data.RelationTitle}」。\n" +
            $"成长阶段：{_data.GrowthStage}（Lv.{_data.Level}）。\n" +
            memLine +
            (string.IsNullOrEmpty(_data.ExploringMap) ? "我偷偷陪着主人。\n" : "我今天还出去冒险了。\n") +
            "明天也要继续加油。";

        _data.Diary.Insert(0, new DiaryEntry { Date = DateTime.Today, Text = text });
        if (_data.Diary.Count > 60)
            _data.Diary.RemoveRange(60, _data.Diary.Count - 60);
        _persist();
    }

    public void AddMemory(string text)
    {
        _memory.Add(_data, text);
        _persist();
    }
}
