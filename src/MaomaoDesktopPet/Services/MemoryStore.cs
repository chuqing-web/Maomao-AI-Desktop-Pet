using System.IO;
using System.Text.Json;
using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

/// <summary>Long-term memories stored next to the exe as memory.json.</summary>
public sealed class MemoryStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string FilePath { get; }

    public MemoryStore(string dataDirectory)
    {
        Directory.CreateDirectory(dataDirectory);
        FilePath = Path.Combine(dataDirectory, "memory.json");
    }

    public List<MemoryEntry> Load()
    {
        try
        {
            if (!File.Exists(FilePath)) return [];
            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<MemoryEntry>>(json, Options) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public void Save(IEnumerable<MemoryEntry> memories)
    {
        var list = memories.Take(120).ToList();
        var json = JsonSerializer.Serialize(list, Options);
        File.WriteAllText(FilePath, json);
    }

    public void Add(PetData data, string text, string? tag = null)
    {
        text = text.Trim();
        if (text.Length < 2) return;
        if (text.Length > 160) text = text[..160] + "…";

        // de-dupe similar recent memory
        if (data.Memories.Take(5).Any(m =>
                string.Equals(m.Text, text, StringComparison.OrdinalIgnoreCase) ||
                (m.Text.Length > 8 && text.Contains(m.Text, StringComparison.OrdinalIgnoreCase))))
            return;

        data.Memories.Insert(0, new MemoryEntry
        {
            At = DateTime.Now,
            Text = string.IsNullOrEmpty(tag) ? text : $"[{tag}] {text}"
        });
        if (data.Memories.Count > 120)
            data.Memories.RemoveRange(120, data.Memories.Count - 120);
        Save(data.Memories);
    }
}
