using System.Collections;
using HarmonyLib;
using MelonLoader;
using ScheduleToolbox.Helpers;
using UnityEngine;

#if MONO
using Console = ScheduleOne.Console;
using List = System.Collections.Generic.List<string>;
#else
using Console = Il2CppScheduleOne.Console;
using List = Il2CppSystem.Collections.Generic.List<string>;
using Il2CppInterop.Runtime.Injection;
#endif

namespace ScheduleToolbox.Commands;

[RegisterTypeInIl2Cpp]
public class TimeWarpCommand : Console.ConsoleCommand
{
#if !MONO
    public TimeWarpCommand(IntPtr ptr) : base(ptr)
    {
    }

    public TimeWarpCommand() : base(ClassInjector.DerivedConstructorPointer<TimeWarpCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif
    public override string CommandWord => "timewarp";
    public override string CommandDescription => "Temporarily speeds up time in the game world.";
    public override string ExampleUsage => "timewarp [seconds] | timewarp stop";
    
    private readonly Console.ConsoleCommand _timeScaleCommand = Console.commands["settimescale"];

#if !MONO
    private static List _warpDefault = new[] { "10" }.ToIl2CppList();
    private static List _warpStop = new[] { "1" }.ToIl2CppList();
#else
    private static List _warpDefault = ["10"];
    private static List _warpStop = ["1"];
#endif
    
    public override void Execute(List args)
    {
        switch (args.Count)
        {
            case 0:
            {
                // if no args supplied, default to 10 seconds
                MelonLogger.Msg("No duration specified. Defaulting to 10 seconds.");
                this.Execute(_warpDefault);
                break;
            }
            case 1:
            {
                if (args.AsEnumerable().ElementAt(0) == "stop")
                {
                    MelonCoroutines.Start(ResetTimeWarp(0));
                    return;
                }
                if (float.TryParse(args.AsEnumerable().ElementAt(0), out var seconds))
                {
                    if (seconds <= 0)
                    {
                        MelonLogger.Warning("Time warp duration must be a positive number.");
                        return;
                    }

                    // Set timescale to speed up time
                    _timeScaleCommand.Execute(_warpDefault);
                    MelonLogger.Msg($"Time warp started for {seconds} seconds. Time scale set to 10.");
                    
                    // Wait for the specified duration
                    MelonCoroutines.Start(ResetTimeWarp(seconds));
                }
                else
                {
                    MelonLogger.Warning("Invalid argument. Use 'timewarp stop' to stop or 'timewarp <seconds>' to start.");
                }
                break;
            }
            default:
            {
                MelonLogger.Warning("Usage: timewarp [seconds] | timewarp stop");
                break;
            }
        }
    }

    private IEnumerator ResetTimeWarp(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        _timeScaleCommand.Execute(_warpStop);
        MelonLogger.Msg($"Time warp {(seconds > 0 ? "ended" : "stopped")}. Time scale reset to 1.");
    }
}