using MelonLoader;
using ScheduleToolbox.Helpers;
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
public class TeleportCommand : Console.ConsoleCommand
{
#if !MONO
    public TeleportCommand(IntPtr ptr) : base(ptr)
    {
    }

    public TeleportCommand() : base(ClassInjector.DerivedConstructorPointer<TeleportCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif

    private static readonly MelonLogger.Instance Logger = new($"{BuildInfo.Name}-TeleportCommand");

    public override string CommandWord => "tp";
    public override string CommandDescription => "Teleports the player to a specified location.";
    public override string ExampleUsage => "(tp <x> [y] <z>) or (tp location)";

    public override void Execute(List args)
    {
        switch (args.Count)
        {
            case 1:
            {
                var locationName = args.AsEnumerable().ElementAt(0);

                if (PersistenceManager.TryGetPosition(locationName, out var position, out var rotation))
                {
                    PlayerMovement.Instance.Teleport(position);
                    Player.Local.transform.rotation = rotation;
                    Player.Local.transform.forward = Vector3.forward;
                    Logger.Msg($"Teleported to '{locationName}' at {position}");
                    return;
                }

                // fallback to base game teleport command for built-in locations
                
                var comm = Console.commands["teleport"];
                comm.Execute(args);
                break;
            }

            case 2:
            {
                bool validX = float.TryParse(args.AsEnumerable().ElementAt(0), out var x1);
                bool validZ = float.TryParse(args.AsEnumerable().ElementAt(1), out var z1);
                if (validX && validZ)
                {
                    const float startY = 1000f;
                    var origin = new Vector3(x1, startY, z1);
                    var ray = new Ray(origin, Vector3.down);
                    if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity))
                    {
                        var targetPosition = hitInfo.point;
                        PlayerMovement.Instance.Teleport(targetPosition + Vector3.up);
                    }
                    else
                    {
                        Logger.Error("Could not find ground below given X/Z coordinates.");
                    }
                }
                else
                {
                    Logger.Error("Invalid coordinates. Please provide valid numbers.");
                }

                break;
            }

            case 3:
            {
                bool validX = float.TryParse(args.AsEnumerable().ElementAt(0), out var x2);
                bool validY = float.TryParse(args.AsEnumerable().ElementAt(1), out var y2);
                bool validZ = float.TryParse(args.AsEnumerable().ElementAt(2), out var z2);
                if (validX && validY && validZ)
                {
                    PlayerMovement.Instance.Teleport(new Vector3(x2, y2, z2));
                }
                else
                {
                    Logger.Error("Invalid coordinates. Please provide valid numbers.");
                }

                break;
            }

            default:
                Logger.Warning("Invalid usage. Use: tp <x> [y] <z> or tp <location>");
                break;
        }
    }
}