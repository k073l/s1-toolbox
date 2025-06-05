using HarmonyLib;
using UnityEngine;

#if MONO
using ScheduleOne.UI.MainMenu;
#else
using Il2CppScheduleOne.UI.MainMenu;
#endif

namespace ScheduleToolbox.Patches;
[HarmonyPatch(typeof(Disclaimer), "Awake")]
public class DisclaimerSkip
{
    public static bool Prefix(MonoBehaviour __instance)
    {
        __instance.gameObject.SetActive(false);
        return false;
    }
}