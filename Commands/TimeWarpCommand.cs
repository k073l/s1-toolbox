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
    public override string ExampleUsage => "timewarp [seconds] [timescale] | timewarp stop";

    private readonly Console.ConsoleCommand _timeScaleCommand = Console.commands["settimescale"];

#if !MONO
    private static List _timeScaleDefault = new[] { "5" }.ToIl2CppList();
    private static List _warpDefault = new[] { "10" }.ToIl2CppList();
    private static List _warpStop = new[] { "1" }.ToIl2CppList();
#else
    private static List _timeScaleDefault = ["5"];
    private static List _warpDefault = ["10"];
    private static List _warpStop = ["1"];
#endif

    // Static toggle state for keybind-driven timewarp
    public static bool IsActive { get; private set; }
    public static float CurrentSpeed { get; private set; }

    public static void Toggle(float speed)
    {
        if (IsActive)
        {
            Stop();
            return;
        }

        SetSpeed(speed);
        IsActive = true;
        MelonLogger.Msg($"Timewarp toggled ON at {CurrentSpeed}x.");
    }

    public static void Stop()
    {
        if (!IsActive) return;
        var cmd = Console.commands["settimescale"];
#if MONO
        cmd.Execute(new List { "1" });
#else
        cmd.Execute(new[] { "1" }.ToIl2CppList());
#endif
        IsActive = false;
        MelonLogger.Msg("Timewarp toggled OFF.");
    }

    public static void SetSpeed(float speed)
    {
        CurrentSpeed = speed;
        var cmd = Console.commands["settimescale"];
#if MONO
        cmd.Execute(new List { speed.ToString("F1") });
#else
        cmd.Execute(new[] { speed.ToString("F1") }.ToIl2CppList());
#endif
        if (IsActive)
            MelonLogger.Msg($"Timewarp speed changed to {CurrentSpeed}x.");
    }

    private static GUIStyle _hudStyle;
    private static GUIStyle _hudBgStyle;

    public static void OnGUI()
    {
        if (!IsActive) return;

        _hudStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = new Color(1f, 0.85f, 0.2f) }
        };

        _hudBgStyle ??= new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter
        };

        var text = $"TIMEWARP: {CurrentSpeed}x";
        var width = 200f;
        var height = 30f;
        var rect = new Rect((Screen.width - width) / 2f, 10f, width, height);

        GUI.Box(rect, "", _hudBgStyle);
        GUI.Label(rect, text, _hudStyle);
    }

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
                    _timeScaleCommand.Execute(_timeScaleDefault);
                    MelonLogger.Msg($"Time warp started for {seconds} seconds. Timescale set to 5.");

                    // Wait for the specified duration
                    MelonCoroutines.Start(ResetTimeWarp(seconds));
                }
                else
                {
                    MelonLogger.Warning("Invalid argument. Use 'timewarp stop' to stop or 'timewarp <seconds>' to start.");
                }
                break;
            }
            case 2:
            {
                if (!float.TryParse(args.AsEnumerable().ElementAt(0), out var seconds))
                    MelonLogger.Warning("Invalid time value. Please provide a valid number of seconds.");
                if (seconds <= 0)
                {
                    MelonLogger.Warning("Time warp duration must be a positive number.");
                    return;
                }
                var timeScale = args.AsEnumerable().ElementAt(1);
                // construct list
                #if MONO
                var timeScaleArgs = new List() { timeScale };
                #else
                var timeScaleArgs = new[] { timeScale }.ToIl2CppList();
                #endif
                _timeScaleCommand.Execute(timeScaleArgs);
                MelonLogger.Msg($"Time warp started for {seconds} seconds. Time scale set to {timeScale}.");
                MelonCoroutines.Start(ResetTimeWarp(seconds));
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
