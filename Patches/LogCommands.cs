using FishNet;
using HarmonyLib;
using MelonLoader;
using MelonLoader.Utils;
using UnityEngine;
#if MONO
using Console = ScheduleOne.Console;
using List = System.Collections.Generic.List<string>;
#else
using Console = Il2CppScheduleOne.Console;
using List = Il2CppSystem.Collections.Generic.List<string>;
#endif

namespace ScheduleToolbox.Patches;

[HarmonyPatch(typeof(Console))]
public class LogCommands
{
    [HarmonyPatch("SubmitCommand", new []{typeof(List<string>)})]
    [HarmonyPrefix]
    public static bool LogConsoleCommands(Console __instance, List args)
    {
        if (args.Count == 0 || (!InstanceFinder.IsHost && !Debug.isDebugBuild && !Application.isEditor)) return true;
        for (var i = 0; i < args.Count; i++)
        {
            args[i] = args[i].ToLower();
        }
            
        var commandWord = args.ElementAt(0);
        if (!Console.commands.TryGetValue(commandWord, out var command))
        {
            Console.LogWarning($"Command {commandWord} not found.");
            return false;
        }
        args.RemoveAt(0);
        command.Execute(args);
        var argsJoined = string.Join(" ", args);
        MelonLogger.Msg($"Executed command: {commandWord} {argsJoined}");
        // Append to file
        var file = Path.Combine(MelonEnvironment.UserDataDirectory, "ScheduleToolbox", "history.log");
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.AppendAllText(file, $"{commandWord} {argsJoined}\n");
        return false;
    }
}