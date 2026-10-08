using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class EmotionService
{
    private readonly PetData _data;
    private readonly Action _persist;

    public EmotionService(PetData data, Action persist)
    {
        _data = data;
        _persist = persist;
    }

    public EmotionKind Recompute()
    {
        var idleMin = (DateTime.Now - _data.LastInteractAt).TotalMinutes;
        EmotionKind emotion;
        if (_data.Energy < 25) emotion = EmotionKind.Sleepy;
        else if (_data.Hunger < 25) emotion = EmotionKind.Pity;
        else if (_data.Mood < 25) emotion = EmotionKind.Sad;
        else if (_data.Mood < 40 || idleMin > 25) emotion = EmotionKind.Bored;
        else if (_data.Mood > 90 && _data.Energy > 60) emotion = EmotionKind.Excited;
        else if (_data.Mood > 75) emotion = EmotionKind.Happy;
        else emotion = EmotionKind.Normal;

        _data.Emotion = emotion;
        _persist();
        return emotion;
    }

    public string ExpressionKey(EmotionKind? e = null) => (e ?? _data.Emotion) switch
    {
        EmotionKind.Happy => "happy",
        EmotionKind.Excited => "excited",
        EmotionKind.Sleepy => "sleepy",
        EmotionKind.Bored => "bored",
        EmotionKind.Pity => "pity",
        EmotionKind.Angry => "angry",
        EmotionKind.Sad => "cry",
        EmotionKind.Surprised => "surprised",
        EmotionKind.Shy => "wink",
        _ => "normal"
    };
}
