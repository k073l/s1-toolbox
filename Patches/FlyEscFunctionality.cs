using HarmonyLib;
using ScheduleToolbox.Commands;

#if MONO
using ScheduleOne.PlayerScripts;
using ScheduleOne.DevUtilities;
#else
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.PlayerScripts;
#endif

namespace ScheduleToolbox.Patches;

[HarmonyPatch(typeof(PlayerCamera), "Exit")]
public class FlyEscFunctionality
{
    static bool Prefix(PlayerCamera __instance, ExitAction action)
    {
        if (action.Used) return true;
        if (!__instance.FreeCamEnabled || action.exitType != ExitType.Escape) return true;
        action.Used = true;
        FlyCommand.TeleportToCameraPos();
        __instance.SetFreeCam(enable: false);
        return false;
    }
}
