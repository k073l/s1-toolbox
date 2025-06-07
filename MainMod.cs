using System.Collections;
using MelonLoader;
using UnityEngine;
using ScheduleToolbox.Commands;
using ScheduleToolbox.Helpers;

#if MONO
using Console = ScheduleOne.Console;
using ScheduleOne.Persistence;
#else
using Console = Il2CppScheduleOne.Console;
using Il2CppScheduleOne.Persistence;
#endif

[assembly: MelonInfo(
    typeof(ScheduleToolbox.ScheduleToolbox),
    ScheduleToolbox.BuildInfo.Name,
    ScheduleToolbox.BuildInfo.Version,
    ScheduleToolbox.BuildInfo.Author
)]
[assembly: MelonColor(1, 255, 0, 0)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace ScheduleToolbox;

public static class BuildInfo
{
    public const string Name = "ScheduleToolbox";
    public const string Description = "Testing tools for Schedule I";
    public const string Author = "k073l";
    public const string Version = "1.1.1";
}

public class ScheduleToolbox : MelonMod
{
    private static MelonLogger.Instance Logger;

    // keycode timers
    private readonly Dictionary<KeyCode, float> holdTimers = new()
    {
        { KeyCode.Alpha1, 0f },
        { KeyCode.Alpha2, 0f },
        { KeyCode.Alpha3, 0f },
        { KeyCode.Alpha4, 0f },
        { KeyCode.Alpha5, 0f },
    };
    private bool _shouldResetTimers = false;
    private bool _addedCommands = false;
    
    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;
        Logger.Msg("ScheduleToolbox initialized");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == "Menu")
        {
            _shouldResetTimers = true;
        }
        if (sceneName == "Main") 
        {
            Logger.Msg("Main scene loaded, starting console commands coroutine");
            MelonCoroutines.Start(Utils.WaitForSingleton<Console>(ConsoleCoro()));
        }
    }

    public override void OnUpdate()
    {
        // Hold timers for keys 1-5
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
    
    private IEnumerator ConsoleCoro()
    {
        if (_addedCommands)
        {
            Logger.Msg("Console commands already added, skipping.");
            yield break;
        }
        yield return new WaitForSeconds(1f);
        var commands = new List<Console.ConsoleCommand>
        {
            new FlyCommand(),
            new TeleportCommand(),
            new SavePosCommand(),
            new PosCommand(),
            new TimeWarpCommand(),
        };
        foreach (var command in commands)
        {
            var commandWord = command.CommandWord;
            Console.commands.Add(commandWord, command);
            Console.Commands.Add(command);
            Logger.Msg($"Registered command: {commandWord}");
        }
        _addedCommands = true;
    }

    public override void OnGUI()
    {
        PosCommand.OnGUI();
        SavePosCommand.OnGUI();
    }
}
