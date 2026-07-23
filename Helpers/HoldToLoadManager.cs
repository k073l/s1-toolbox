using MelonLoader;
using UnityEngine;

#if MONO
using ScheduleOne.Persistence;
#else
using Il2CppScheduleOne.Persistence;
#endif

namespace ScheduleToolbox.Helpers;

public class HoldToLoadManager
{
    private static readonly MelonLogger.Instance Logger = new($"{BuildInfo.Name}-HoldToLoad");

    private static HoldToLoadManager _instance;
    public static HoldToLoadManager Instance => _instance ??= new HoldToLoadManager();

    private readonly Dictionary<KeyCode, float> holdTimers = new()
    {
        { KeyCode.Alpha1, 0f },
        { KeyCode.Alpha2, 0f },
        { KeyCode.Alpha3, 0f },
        { KeyCode.Alpha4, 0f },
        { KeyCode.Alpha5, 0f },
    };
    private bool _shouldResetTimers;

    public void Reset()
    {
        _shouldResetTimers = true;
    }

    public void Update()
    {
        if (_shouldResetTimers)
        {
            _shouldResetTimers = false;
            var keysToReset = new List<KeyCode>(holdTimers.Keys);
            foreach (var key in keysToReset)
                holdTimers[key] = 0f;
        }

        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Menu")
            return;

        var keys = new List<KeyCode>(holdTimers.Keys);
        foreach (var key in keys)
        {
            if (Input.GetKey(key))
            {
                holdTimers[key] += Time.deltaTime;

                if (!(holdTimers[key] >= 0.5f)) continue;

                var keyNumber = (int)key - (int)KeyCode.Alpha0;
                Logger.Msg($"Trying to load save in slot: {keyNumber}");
                holdTimers[key] = -999f;

                if (keyNumber <= LoadManager.SaveGames.Length)
                    LoadManager.Instance.StartGame(LoadManager.SaveGames[keyNumber - 1]);
                else
                    Logger.Warning($"Save slot {keyNumber} doesn't exist.");
            }
            else if (holdTimers[key] >= 0f)
            {
                holdTimers[key] = 0f;
            }
        }
    }
}
