using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using ScheduleToolbox.Helpers;
using UnityEngine;
#if MONO
using FishNet;
using Console = ScheduleOne.Console;
using List = System.Collections.Generic.List<string>;
#else
using Il2CppFishNet;
using Console = Il2CppScheduleOne.Console;
using List = Il2CppSystem.Collections.Generic.List<string>;
#endif

namespace ScheduleToolbox.Patches;

[HarmonyPatch(typeof(Console))]
public class LogCommands
{
    [HarmonyPatch(nameof(Console.SubmitCommand), new []{typeof(List)})]
    [HarmonyPrefix]
    public static bool LogConsoleCommands(Console __instance, List args)
    {
        if (args.Count == 0 || (!InstanceFinder.IsHost && !Debug.isDebugBuild && !Application.isEditor)) return true;
        for (var i = 0; i < args.Count; i++)
        {
#if MONO
            args[i] = args[i].ToLower();
#else
            args._items[i] = args._items[i].ToLower();
#endif
        }
            
        var commandWord = args.AsEnumerable().ElementAt(0);
        if (!Console.commands.TryGetValue(commandWord, out var command))
        {
            Console.LogWarning($"Command {commandWord} not found.");
            return false;
        }
        args.RemoveAt(0);
        command.Execute(args);
        // Join args, manually since string.Join doesn't support Il2CppList
        var argsJoined = "";
        for (var i = 0; i < args.Count; i++)
        {
            argsJoined += args.AsEnumerable().ElementAt(i);
            if (i < args.Count - 1) argsJoined += " ";
        }
        
        MelonLogger.Msg($"Executed command: {commandWord} {argsJoined}");
        // Append to file
        var file = Path.Combine(MelonEnvironment.UserDataDirectory, "ScheduleToolbox", "history.log");
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.AppendAllText(file, $"{commandWord} {argsJoined}\n");

        // Trim the file if it exceeds the max lines
        var lines = File.ReadAllLines(file).ToList();
        if (ScheduleToolbox.MaxBufferLines.Value > 0 && lines.Count > ScheduleToolbox.MaxBufferLines.Value)
        {
            lines = lines.Skip(lines.Count - ScheduleToolbox.MaxBufferLines.Value).ToList();
            File.WriteAllLines(file, lines);
        }

        return false;
    }
}