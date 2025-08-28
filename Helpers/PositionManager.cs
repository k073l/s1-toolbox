using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using Newtonsoft.Json;
#if MONO
using Object = System.Object;
#else
using Object = Il2CppSystem.Object;
#endif

namespace ScheduleToolbox.Helpers;

[Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3() { }

    public SerializableVector3(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToUnity() => new Vector3(x, y, z);
}

[Serializable]
public class SerializableQuaternion
{
    public float x, y, z, w;

    public SerializableQuaternion() { }

    public SerializableQuaternion(Quaternion q)
    {
        x = q.x;
        y = q.y;
        z = q.z;
        w = q.w;
    }

    public Quaternion ToUnity() => new Quaternion(x, y, z, w);
}

[Serializable]
public class SavedPosition
{
    public SerializableVector3 position;
    public SerializableQuaternion rotation;
}

[Serializable]
public class SavedPositionsData
{
    public Dictionary<string, SavedPosition> Positions = new();
}

public static class PositionManager
{
    private static readonly string SaveFilePath = Path.Combine(MelonEnvironment.UserDataDirectory, "ScheduleToolbox" , "ScheduleToolbox.json");
    private static readonly MelonLogger.Instance Logger = new MelonLogger.Instance($"{BuildInfo.Name}-PositionManager");
    private static SavedPositionsData _data;

    static PositionManager()
    {
        Load();
    }

    public static void SavePosition(string name, Vector3 position, Quaternion rotation)
    {
        _data.Positions[name] = new SavedPosition
        {
            position = new SerializableVector3(position),
            rotation = new SerializableQuaternion(rotation)
        };

        Save();
    }

    public static bool TryGetPosition(string name, out Vector3 position, out Quaternion rotation)
    {
        if (_data.Positions.TryGetValue(name, out var saved))
        {
            position = saved.position.ToUnity();
            rotation = saved.rotation.ToUnity();
            return true;
        }

        position = default;
        rotation = default;
        return false;
    }

    public static void Save()
    {
        try
        {
            var json = JsonConvert.SerializeObject(_data, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(SaveFilePath)!);
            File.WriteAllText(SaveFilePath, json);
            Logger.Debug($"Saved {_data.Positions.Count} positions to: {SaveFilePath}");
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
                _data = JsonConvert.DeserializeObject<SavedPositionsData>(json) ?? new SavedPositionsData();
                Logger.Debug($"Loaded {_data.Positions.Count} positions.");
            }
            else
            {
                Logger.Debug("Save file not found. Starting fresh.");
                _data = new SavedPositionsData();
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Load failed: {ex}");
            _data = new SavedPositionsData();
        }
    }
}