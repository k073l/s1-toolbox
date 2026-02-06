using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using Newtonsoft.Json;
using ScheduleToolbox.Models;
#if MONO
using Object = System.Object;
#else
using Object = Il2CppSystem.Object;
#endif

namespace ScheduleToolbox.Helpers;

public static class PersistenceManager
{
    private static readonly string SaveFilePath = Path.Combine(MelonEnvironment.UserDataDirectory, "ScheduleToolbox" , "ScheduleToolbox.json");
    private static readonly MelonLogger.Instance Logger = new MelonLogger.Instance($"{BuildInfo.Name}-PositionManager");
    public static ToolboxData Data { get; private set; }

    static PersistenceManager()
    {
        Load();
    }

    public static void SavePosition(string name, Vector3 position, Quaternion rotation)
    {
        Data.Positions[name] = new SavedPosition
        {
            position = new SerializableVector3(position),
            rotation = new SerializableQuaternion(rotation)
        };

        Save();
    }

    public static bool TryGetPosition(string name, out Vector3 position, out Quaternion rotation)
    {
        if (Data.Positions.TryGetValue(name, out var saved))
        {
            position = saved.position.ToUnity();
            rotation = saved.rotation.ToUnity();
            return true;
        }

        position = default;
        rotation = default;
        return false;
    }

    public static void SaveKeybind(KeyCode key, string command)
    {
        if (Data.Keybindings.TryGetValue(key.ToString(), out var saved) && saved != command)
            Logger.Msg($"Overwriting keybind for {key}: '{saved}' -> '{command}'");
        Data.Keybindings[key.ToString()] = command;
        Save();
    }
    
    public static void RemoveKeybind(KeyCode key)
    {
        Data.Keybindings.Remove(key.ToString());
        Save();
    }

    public static void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(Data, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath)!);
            File.WriteAllText(SaveFilePath, json);
            Logger.Debug($"Saved {Data.Positions.Count} positions to: {SaveFilePath}");
        }
        catch (Exception ex)
        {
            Logger.Error($"Save failed: {ex}");
        }
    }

    public static void Load()
    {
        try
        {
            if (File.Exists(SaveFilePath))
            {
                var json = File.ReadAllText(SaveFilePath);
                Data = JsonConvert.DeserializeObject<ToolboxData>(json) ?? new ToolboxData();
                Logger.Debug($"Loaded {Data.Positions.Count} positions.");
            }
            else
            {
                Logger.Debug("Save file not found. Starting fresh.");
                Data = new ToolboxData();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Load failed: {ex}");
            Data = new ToolboxData();
        }
    }
}