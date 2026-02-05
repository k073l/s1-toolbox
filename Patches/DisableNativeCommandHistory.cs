using HarmonyLib;

#if MONO
using ScheduleOne.UI;
#else
using Il2CppScheduleOne.UI;
#endif

namespace ScheduleToolbox.Patches;

// We have our own command history implementation, so disable the native one to avoid conflicts
[HarmonyPatch(typeof(ConsoleUI), "UpdateCommandHistory")]
public class DisableNativeCommandHistory
{
    public static bool Prefix(ConsoleUI __instance)
    {
        return false;
    }
}