using MelonLoader;
using UnityEngine;

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
public class PosCommand : Console.ConsoleCommand
{
#if !MONO
    public PosCommand(IntPtr ptr) : base(ptr)
    {
    }

    public PosCommand() : base(ClassInjector.DerivedConstructorPointer<PosCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif

    private static readonly MelonLogger.Instance Logger = new MelonLogger.Instance($"{BuildInfo.Name}-PosCommand");

    public override string CommandWord => "pos";
    public override string CommandDescription => "Logs the player's current position and rotation";
    public override string ExampleUsage => "pos";

    private static string _guiMessage = "";
    private static float _guiMessageEndTime = 5f;

    public override void Execute(List args)
    {
        var cameraPos = PlayerCamera.Instance.transform.position;
        var cameraRot = PlayerCamera.Instance.transform.rotation;
        var formattedMessage =
            $"Pos: ({cameraPos.x:F2}, {cameraPos.y:F2}, {cameraPos.z:F2}); Rot: ({cameraRot.eulerAngles.x:F1}, {cameraRot.eulerAngles.y:F1}, {cameraRot.eulerAngles.z:F1})";
        Logger.Msg(formattedMessage);

        _guiMessage = formattedMessage;
        _guiMessageEndTime = Time.time + 3f;
    }

    public static void OnGUI()
    {
        if (Time.time < _guiMessageEndTime)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                normal = { textColor = Color.white }
            };

            GUI.Label(new Rect(10, Screen.height - 30, Screen.width - 20, 25), _guiMessage, style);
        }
    }
}