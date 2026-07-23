using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;
using UnityEngine;

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

    public bool IsConsoleOpen => _consoleUI != null && _consoleUI.canvas != null && _consoleUI.canvas.enabled;

    public static void Initialize()
    {
        var category = MelonPreferences.CreateCategory("ScheduleToolbox-Settings", "Schedule Toolbox Settings");
        MaxBufferLines = category.CreateEntry("MaxConsoleBufferLines", 0, "Max Console Buffer Lines",
            "Maximum number of lines to keep in the console history file. Set to 0 for unlimited.");
    }

    public void SetConsoleUI(ConsoleUI ui)
    {
        _consoleUI = ui;
    }

    public void Update()
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
}
