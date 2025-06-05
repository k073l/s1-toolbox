using MelonLoader;
using UnityEngine;

#if MONO
using Console = ScheduleOne.Console;
using ScheduleOne.PlayerScripts;
using List = System.Collections.Generic.List<string>;
#else
using Console = Il2CppScheduleOne.Console;
using Il2CppScheduleOne.PlayerScripts;
using List = Il2CppSystem.Collections.Generic.List<string>;
using Il2CppInterop.Runtime.Injection;
#endif

namespace ScheduleToolbox.Commands;

[RegisterTypeInIl2Cpp]
public class FlyCommand : Console.ConsoleCommand
{
#if !MONO
    public FlyCommand(IntPtr ptr) : base(ptr)
    {
    }

    public FlyCommand() : base(ClassInjector.DerivedConstructorPointer<FlyCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif

    public override string CommandWord => "fly";
    public override string CommandDescription => "Toggles fly mode (enhanced freecam)";
    public override string ExampleUsage => "fly";

    public override void Execute(List args)
    {
        if (PlayerCamera.Instance.FreeCamEnabled)
        {
            TeleportToCameraPos();
            PlayerCamera.Instance.SetFreeCam(enable: false);
        }
        else
        {
            PlayerCamera.Instance.SetFreeCam(enable: true);
        }
    }

    public static void TeleportToCameraPos()
    {
        var cameraPos = PlayerCamera.Instance.transform.position;
        var cameraRot = PlayerCamera.Instance.transform.rotation;
        PlayerCamera.Instance.SetFreeCam(enable: false);
        PlayerMovement.Instance.Teleport(cameraPos);
        Player.Local.transform.rotation = cameraRot;
        Player.Local.transform.forward = Vector3.forward;
        PlayerMovement.Instance.SetResidualVelocity(Vector3.zero, 0, 0);
    }
}