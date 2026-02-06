using HarmonyLib;
using MelonLoader;
using Newtonsoft.Json;
using ScheduleToolbox.Helpers;
using UnityEngine;
#if MONO
using Console = ScheduleOne.Console;
#else
using Console = Il2CppScheduleOne.Console;
#endif


namespace ScheduleToolbox.Patches;

// i hate il2cpp
// if you're wondering why not just patch AddBinding, RemoveBinding, etc. directly:
// il2cpp.
[HarmonyPatch(typeof(Console.Bind), nameof(Console.Bind.Execute))]
internal static class Console_AddBinding
{
    public static bool Prefix(Console.Bind __instance, 
#if MONO
        System.Collections.Generic.List<string> args
#else
        Il2CppSystem.Collections.Generic.List<string> args
#endif
        )
    {
        if (args.Count > 1)
        {
            var text = args[0].ToLower();
            if (!Enum.TryParse<KeyCode>(text, ignoreCase: true, out var result))
            {
                Console.LogCommandError("Unrecognized keycode '" + text + "'");
            }
            var command = string.Join(" ", args.ToArray()).Substring(text.Length + 1);
            AddBinding(result, command);
        }
        else
        {
            Console.LogUnrecognizedFormat(new string[1] { __instance.ExampleUsage });
        }

        return false;
    }

    private static void AddBinding(KeyCode key, string command)
    {
        command = PersistKeybindings.SaveBinding(key, command);
        PersistenceManager.SaveKeybind(key, command);
    }
}

[HarmonyPatch(typeof(Console.Unbind), nameof(Console.Unbind.Execute))]
internal static class Console_RemoveBinding
{
    public static bool Prefix(Console.Unbind __instance, 
#if MONO
        System.Collections.Generic.List<string> args
#else
        Il2CppSystem.Collections.Generic.List<string> args
#endif
        )
    {
        if (args.Count > 0)
        {
            string text = args[0].ToLower();
            if (!Enum.TryParse<KeyCode>(text, ignoreCase: true, out var result))
            {
                Console.LogCommandError("Unrecognized keycode '" + text + "'");
            }
            RemoveBinding(result);
        }
        else
        {
            Console.LogUnrecognizedFormat(new string[1] { __instance.ExampleUsage });
        }

        return false;
    }
    
    private static void RemoveBinding(KeyCode key)
    {
        Console.Log($"Unbinding {key}");
        Console.Instance.keyBindings.Remove(key);
        PersistenceManager.RemoveKeybind(key);
    }
}

[HarmonyPatch(typeof(Console.ClearBinds), nameof(Console.ClearBinds.Execute))]
internal static class Console_ClearBindings
{
    public static bool Prefix(Console.ClearBinds __instance, 
#if MONO
        System.Collections.Generic.List<string> args
#else
        Il2CppSystem.Collections.Generic.List<string> args
#endif
        )
    {
        Console.Log("Clearing all key bindings");
        Console.Instance.keyBindings.Clear();
        PersistenceManager.Data.Keybindings.Clear();
        PersistenceManager.Save();
        return false;
    }
}

[HarmonyPatch(typeof(Console))]
public class PersistKeybindings
{

    public static string SaveBinding(KeyCode key, string command)
    {
        command = StripOuterSingleQuotes(command);
        Console.Log("Binding " + key.ToString() + " to " + command);
        Console.Instance.keyBindings[key] = command;
        return command;
    }

    [HarmonyPatch("Awake")]
    [HarmonyPostfix]
    public static void LoadBindings(Console __instance)
    {
        foreach (var keybind in PersistenceManager.Data.Keybindings)
        {
            Melon<ScheduleToolbox>.Logger.Msg($"Restoring keybind: {keybind.Key} -> {keybind.Value}");
            if (!Enum.TryParse<KeyCode>(keybind.Key, true, out var key))
            {
                Melon<ScheduleToolbox>.Logger.Warning($"Unknown keycode '{keybind.Key}'");
                continue;
            }
            SaveBinding(key, keybind.Value);
        }
    }

    private static string StripOuterSingleQuotes(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        if (s.Length >= 2 && s[0] == '\'' && s[^1] == '\'')
            return s[1..^1];

        return s;
    }
}