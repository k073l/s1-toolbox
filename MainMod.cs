using System.Collections;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
using ScheduleToolbox.Commands;
using ScheduleToolbox.Helpers;
using Object = UnityEngine.Object;

#if MONO
using Console = ScheduleOne.Console;
using ScheduleOne.Persistence;
using ScheduleOne.UI;
#else
using Console = Il2CppScheduleOne.Console;
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.UI;
using Il2CppObject = Il2CppSystem.Object;
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
    public const string Version = "2.1.3";
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
    
    private ConsoleUI _consoleUI;
    private int currentBufferLine = -1;
    
    private static List<string> autocompleteMatches = new();
    private static int autocompleteIndex = -1;
    private static string lastInputText = string.Empty;
    private static string autocompletePrefix = string.Empty;
    private static bool autocompleteActive = false;
    
    private static MelonPreferences_Category _settingsCategory;
    internal static MelonPreferences_Entry<int> MaxBufferLines;
    
    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;
        Logger.Msg("ScheduleToolbox initialized");
        
        _settingsCategory = MelonPreferences.CreateCategory("ScheduleToolbox-Settings", "Schedule Toolbox Settings");
        MaxBufferLines = _settingsCategory.CreateEntry("MaxConsoleBufferLines", 0, "Max Console Buffer Lines",
            "Maximum number of lines to keep in the console history file. Set to 0 for unlimited.");
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == "Menu")
        {
            _shouldResetTimers = true;
        }
        if (sceneName == "Main") 
        {
            // grab console UI reference
            if (_consoleUI == null)
                _consoleUI = Object.FindObjectOfType<ConsoleUI>();
            
            Logger.Msg("Main scene loaded, starting console commands coroutine");
            MelonCoroutines.Start(Utils.WaitForSingleton<Console>(ConsoleCoro()));
        }
    }

    public override void OnUpdate()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Main")
        {
            ConsoleImprovements();
        }
        
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

    private void ConsoleImprovements()
    {
        if (_consoleUI == null || _consoleUI.canvas == null) return;
        if (_consoleUI.canvas.enabled)
        {
            // Console is open, able to scroll through buffer
            string[] buffer;
            try
            {
                buffer = File.ReadAllLines(Path.Combine(MelonEnvironment.UserDataDirectory,
                    "ScheduleToolbox", "history.log"));
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case DirectoryNotFoundException _:
                    case FileNotFoundException _:
                        // No history file or directory
                        return;

                    default:
                        Logger.Error($"Error reading history file: {ex}");
                        return;
                }
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                if (buffer.Length <= 0) return;
                currentBufferLine = Mathf.Clamp(currentBufferLine + 1, 0, buffer.Length - 1);
                _consoleUI.InputField.SetTextWithoutNotify(buffer[^(currentBufferLine + 1)]);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (buffer.Length <= 0) return;
                currentBufferLine = Mathf.Clamp(currentBufferLine - 1, -1, buffer.Length - 1);
                _consoleUI.InputField.SetTextWithoutNotify(currentBufferLine == -1
                    ? "" // clear input if at the bottom of the buffer
                    : buffer[^(currentBufferLine + 1)]);
            }
            else if (Input.GetKeyDown(KeyCode.Tab))
            {
                var currentText = _consoleUI.InputField.text.Trim();

                if (!autocompleteActive)
                {
                    autocompletePrefix = currentText;
                    autocompleteActive = true;

#if MONO
                    autocompleteMatches = Console.commands.Keys
                        .Where(cmd => cmd.StartsWith(autocompletePrefix, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(cmd => cmd)
                        .ToList();
#else
                    // il2cpp pain, gets confused by .Keys
                    var keysList = new List<string>();
                    foreach (var kv in Console.commands)
                        keysList.Add(kv.Key);

                    autocompleteMatches = keysList
                        .Where(cmd => cmd.StartsWith(autocompletePrefix, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(cmd => cmd)
                        .ToList();
#endif

                    autocompleteIndex = 0;
                }
                else
                {
                    // cycle through matches
                    if (autocompleteMatches.Count > 0)
                        autocompleteIndex = (autocompleteIndex + 1) % autocompleteMatches.Count;
                }

                if (autocompleteMatches.Count <= 0) return;

                var match = autocompleteMatches[autocompleteIndex];
                _consoleUI.InputField.SetTextWithoutNotify(match);
                _consoleUI.InputField.caretPosition = match.Length;

                lastInputText = match; // track what was inserted
            }
            else
            {
                var currentText = _consoleUI.InputField.text.Trim();

                if (string.Equals(currentText, lastInputText, StringComparison.OrdinalIgnoreCase)) return;
                // input changed, reset autocomplete state
                autocompleteMatches.Clear();
                autocompleteIndex = -1;
                autocompletePrefix = string.Empty;
                autocompleteActive = false;
                lastInputText = currentText;
            }

        }
        else
        {
            // Console is closed, reset buffer index
            currentBufferLine = -1;
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
            new ForceCartelDealCommand(),
            new SetCartelInfluenceCommand(),
            new ForceDealCommand(),
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
