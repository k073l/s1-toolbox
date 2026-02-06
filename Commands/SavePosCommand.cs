using MelonLoader;
using UnityEngine;
using ScheduleToolbox.Helpers;

#if MONO
using Console = ScheduleOne.Console;
using ScheduleOne.PlayerScripts;
using List = System.Collections.Generic.List<string>;
#else
using Il2CppInterop.Runtime.Injection;
using Console = Il2CppScheduleOne.Console;
using Il2CppScheduleOne.PlayerScripts;
using List = Il2CppSystem.Collections.Generic.List<string>;
#endif

namespace ScheduleToolbox.Commands;

[RegisterTypeInIl2Cpp]
public class SavePosCommand : Console.ConsoleCommand
{
#if !MONO
    public SavePosCommand(IntPtr ptr) : base(ptr)
    {
    }

    public SavePosCommand() : base(ClassInjector.DerivedConstructorPointer<SavePosCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif

    private static readonly MelonLogger.Instance Logger = new($"{BuildInfo.Name}-SavePosCommand");

    public override string CommandWord => "savepos";

    public override string CommandDescription =>
        "Saves the player's current position and rotation to a named location.";

    public override string ExampleUsage => "savepos home";

    private static string _guiMessage = "";
    private static float _guiMessageEndTime;

    public override void Execute(List args)
    {
        if (args.Count != 1)
        {
            Logger.Warning("Usage: savepos <name>");
            return;
        }

        string name = args.AsEnumerable().ElementAt(0);
        var pos = PlayerCamera.Instance.transform.position;
        var rot = PlayerCamera.Instance.transform.rotation;

        PersistenceManager.SavePosition(name, pos, rot);

        Logger.Msg($"Saved position '{name}': {pos.x},{pos.y},{pos.z} | {rot.x},{rot.y},{rot.z}");
        _guiMessage = $"Saved '{name}' at {pos.x}, {pos.y}, {pos.z}!";
        _guiMessageEndTime = Time.time + 3f;
    }

    public static void OnGUI()
    {
        if (!string.IsNullOrEmpty(_guiMessage) && Time.time < _guiMessageEndTime)
        {
            GUI.Label(new Rect(10, Screen.height - 60, 300, 45), _guiMessage);
        }
    }
}