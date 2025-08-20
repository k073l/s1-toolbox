#if MONO
using Console = ScheduleOne.Console;
using ScheduleOne.Cartel;
using ScheduleOne.Map;
using List = System.Collections.Generic.List<string>;
#else
using Console = Il2CppScheduleOne.Console;
using Il2CppScheduleOne.Cartel;
using Il2CppScheduleOne.Map;
using List = Il2CppSystem.Collections.Generic.List<string>;
using Il2CppInterop.Runtime.Injection;
#endif
using MelonLoader;
using ScheduleToolbox.Helpers;
using Object = UnityEngine.Object;

namespace ScheduleToolbox.Commands;

[RegisterTypeInIl2Cpp]
public class SetCartelInfluenceCommand: Console.ConsoleCommand
{
#if !MONO
    public SetCartelInfluenceCommand(IntPtr ptr) : base(ptr)
    {
    }

    public SetCartelInfluenceCommand() : base(ClassInjector.DerivedConstructorPointer<SetCartelInfluenceCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif
    public override string CommandWord => "setcartelinfluence";
    public override string CommandDescription => "Sets the cartel influence level for the region.";
    public override string ExampleUsage => "setcartelinfluence westville 1";
    
    public override void Execute(List args)
    {
        if (args.Count != 2)
        {
            MelonLogger.Warning("Usage: setcartelinfluence <region> <influence>");
            return;
        }

        var regionName = args.AsEnumerable().ElementAt(0);
        if (!Enum.TryParse(regionName, true, out EMapRegion region))
        {
            MelonLogger.Warning($"Invalid region name '{regionName}'. Valid regions are: {string.Join(", ", Enum.GetNames(typeof(EMapRegion)))}");
            return;
        }
        
        if (!float.TryParse(args.AsEnumerable().ElementAt(1), out var influence))
        {
            MelonLogger.Warning("Invalid influence value. Please provide a valid number.");
            return;
        }

        // Find CartelInfluence MonoBehaviour
        var cartelInfluence = Object.FindObjectOfType<CartelInfluence>();
        if (cartelInfluence == null)
        {
            MelonLogger.Error("CartelInfluence not found in the scene.");
            return;
        }

        cartelInfluence.SetInfluence(null, region, influence);
        MelonLogger.Msg($"Set cartel influence for {regionName} to {influence}.");
    }
}