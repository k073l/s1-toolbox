using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;
using UnityEngine;
using ScheduleToolbox.Commands;

#if MONO
using ScheduleOne.UI;
using Console = ScheduleOne.Console;
#else
using Il2CppScheduleOne.UI;
using Console = Il2CppScheduleOne.Console;
#endif

namespace ScheduleToolbox.Helpers;

public class ConsoleManager
{
    private static readonly MelonLogger.Instance Logger = new($"{BuildInfo.Name}-ConsoleManager");

    private static ConsoleManager _instance;
    public static ConsoleManager Instance => _instance ??= new ConsoleManager();

    private ConsoleUI _consoleUI;
    private int currentBufferLine = -1;

    private List<string> autocompleteMatches = new();
    private int autocompleteIndex = -1;
    private string lastInputText = string.Empty;
    private string autocompletePrefix = string.Empty;
    private bool autocompleteActive = false;

    internal static MelonPreferences_Entry<int> MaxBufferLines;

    // Timewarp keybind preferences
    private MelonPreferences_Entry<KeyCode> _timewarpToggleKey;
    private MelonPreferences_Entry<KeyCode> _timewarpSpeedUpKey;
    private MelonPreferences_Entry<KeyCode> _timewarpSlowDownKey;
    private MelonPreferences_Entry<float> _timewarpDefaultSpeed;
    private MelonPreferences_Entry<float> _timewarpSpeedStep;
    private MelonPreferences_Entry<float> _timewarpMinSpeed;
    private MelonPreferences_Entry<float> _timewarpMaxSpeed;
    private float _currentTimewarpSpeed;

    public bool IsConsoleOpen => _consoleUI != null && _consoleUI.canvas != null && _consoleUI.canvas.enabled;

    public static void Initialize()
    {
        var category = MelonPreferences.CreateCategory("ScheduleToolbox-Settings", "Schedule Toolbox Settings");
        MaxBufferLines = category.CreateEntry("MaxConsoleBufferLines", 0, "Max Console Buffer Lines",
            "Maximum number of lines to keep in the console history file. Set to 0 for unlimited.");

        Instance._timewarpToggleKey = category.CreateEntry("TimewarpToggleKey", KeyCode.KeypadMultiply, "Timewarp Toggle Key",
            "Key to toggle timewarp on/off.");
        Instance._timewarpSpeedUpKey = category.CreateEntry("TimewarpSpeedUpKey", KeyCode.KeypadPlus, "Timewarp Speed Up Key",
            "Key to increase timewarp speed.");
        Instance._timewarpSlowDownKey = category.CreateEntry("TimewarpSlowDownKey", KeyCode.KeypadMinus, "Timewarp Slow Down Key",
            "Key to decrease timewarp speed.");
        Instance._timewarpDefaultSpeed = category.CreateEntry("TimewarpDefaultSpeed", 5f, "Timewarp Default Speed",
            "Default timewarp speed multiplier.", validator: new ValueRange<float>(1f, 100f));
        Instance._timewarpSpeedStep = category.CreateEntry("TimewarpSpeedStep", 1f, "Timewarp Speed Step",
            "Amount to increase/decrease speed per key press.", validator: new ValueRange<float>(0.1f, 50f));
        Instance._timewarpMinSpeed = category.CreateEntry("TimewarpMinSpeed", 2f, "Timewarp Min Speed",
            "Minimum timewarp speed.", validator: new ValueRange<float>(1f, 100f));
        Instance._timewarpMaxSpeed = category.CreateEntry("TimewarpMaxSpeed", 20f, "Timewarp Max Speed",
            "Maximum timewarp speed.", validator: new ValueRange<float>(1f, 100f));

        Instance._currentTimewarpSpeed = Instance._timewarpDefaultSpeed.Value;
    }

    public void SetConsoleUI(ConsoleUI ui)
    {
        _consoleUI = ui;
    }

    public void StopTimewarp()
    {
        if (!TimeWarpCommand.IsActive) return;
        TimeWarpCommand.Stop();
    }

    public void Update()
    {
        UpdateConsoleInput();
        HandleTimewarpKeybinds();
    }

    private void UpdateConsoleInput()
    {
        if (_consoleUI == null || _consoleUI.canvas == null) return;
        if (_consoleUI.canvas.enabled)
        {
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
                    ? ""
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
                    if (autocompleteMatches.Count > 0)
                        autocompleteIndex = (autocompleteIndex + 1) % autocompleteMatches.Count;
                }

                if (autocompleteMatches.Count <= 0) return;

                var match = autocompleteMatches[autocompleteIndex];
                _consoleUI.InputField.SetTextWithoutNotify(match);
                _consoleUI.InputField.caretPosition = match.Length;

                lastInputText = match;
            }
            else
            {
                var currentText = _consoleUI.InputField.text.Trim();

                if (string.Equals(currentText, lastInputText, StringComparison.OrdinalIgnoreCase)) return;

                autocompleteMatches.Clear();
                autocompleteIndex = -1;
                autocompletePrefix = string.Empty;
                autocompleteActive = false;
                lastInputText = currentText;
            }
        }
        else
        {
            currentBufferLine = -1;
        }
    }

    private void HandleTimewarpKeybinds()
    {
        if (IsConsoleOpen) return;

        if (Input.GetKeyDown(_timewarpToggleKey.Value))
        {
            if (TimeWarpCommand.IsActive)
                TimeWarpCommand.Stop();
            else
            {
                _currentTimewarpSpeed = _timewarpDefaultSpeed.Value;
                TimeWarpCommand.Toggle(_currentTimewarpSpeed);
            }
        }

        if (Input.GetKeyDown(_timewarpSpeedUpKey.Value))
        {
            _currentTimewarpSpeed = Mathf.Clamp(
                _currentTimewarpSpeed + _timewarpSpeedStep.Value,
                _timewarpMinSpeed.Value, _timewarpMaxSpeed.Value);
            if (TimeWarpCommand.IsActive)
                TimeWarpCommand.SetSpeed(_currentTimewarpSpeed);
        }

        if (Input.GetKeyDown(_timewarpSlowDownKey.Value))
        {
            _currentTimewarpSpeed = Mathf.Clamp(
                _currentTimewarpSpeed - _timewarpSpeedStep.Value,
                _timewarpMinSpeed.Value, _timewarpMaxSpeed.Value);
            if (TimeWarpCommand.IsActive)
                TimeWarpCommand.SetSpeed(_currentTimewarpSpeed);
        }
    }
}
