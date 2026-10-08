using System.IO;
using System.Text.Json;
using MaomaoDesktopPet.Models;

namespace MaomaoDesktopPet.Services;

public sealed class SaveService
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public string DataDirectory { get; }
    public string SavePath { get; }
    public MemoryStore Memory { get; }

    public SaveService()
    {
        DataDirectory = ResolveDataDirectory();
        Directory.CreateDirectory(DataDirectory);
        SavePath = Path.Combine(DataDirectory, "save.json");
        Memory = new MemoryStore(DataDirectory);
        TryMigrateFromAppData();
    }

    private static string ResolveDataDirectory()
    {
        // Prefer folder next to the exe so portable installs keep memory/save together.
        var exeDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return exeDir;
    }

    private void TryMigrateFromAppData()
    {
        if (File.Exists(SavePath)) return;
        var legacy = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "MaomaoDesktopPet", "save.json");
        try
        {
            if (File.Exists(legacy))
                File.Copy(legacy, SavePath, overwrite: false);
        }
        catch { /* ignore */ }
    }

    public PetData LoadOrCreate()
    {
        PetData data;
        try
        {
            if (File.Exists(SavePath))
            {
                var json = File.ReadAllText(SavePath);
                data = JsonSerializer.Deserialize<PetData>(json, Options) ?? new PetData();
            }
            else
            {
                data = new PetData();
            }
        }
        catch
        {
            data = new PetData();
        }

        // Merge dedicated memory.json (source of truth for long-term memory)
        var diskMemories = Memory.Load();
        if (diskMemories.Count > 0)
        {
            data.Memories = diskMemories;
        }
        else if (data.Memories.Count > 0)
        {
            Memory.Save(data.Memories);
        }

        return data;
    }

    public void Save(PetData data)
    {
        data.LastSeenAt = DateTime.Now;
        var json = JsonSerializer.Serialize(data, Options);
        File.WriteAllText(SavePath, json);
        Memory.Save(data.Memories);
    }
}
