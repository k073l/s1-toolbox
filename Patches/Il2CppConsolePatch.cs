#if !MONO
using HarmonyLib;
using Console = Il2CppScheduleOne.Console;
using List = Il2CppSystem.Collections.Generic.List<string>;

namespace ScheduleToolbox.Patches;

[HarmonyPatch(typeof(Console.ConsoleCommand), "Execute")]
public static class Il2CppConsolePatch
{
    public static bool Prefix(Console.ConsoleCommand __instance, List args)
    {
        // call directly, it gets confused in Il2Cpp due to virtual/abstract methods
        __instance.Execute(args);
        return false;
    }
}
#endif