using HarmonyLib;
#if MONO
using System.Globalization;
using ScheduleOne.DevUtilities;
#else
using Il2CppSystem.Globalization;
using Il2CppScheduleOne.DevUtilities;
#endif

namespace ScheduleToolbox.Patches;

[HarmonyPatch(typeof(Settings), "GetDefaultUnitTypeForPlayer")]
public class SettingsDefaultUnit
{
    public static bool Prefix(ref Settings.EUnitType __result)
    {
        if (!Equals(CultureInfo.CurrentCulture, CultureInfo.InvariantCulture)) return true;
        __result = Settings.EUnitType.Metric;
        return false;
    }
}