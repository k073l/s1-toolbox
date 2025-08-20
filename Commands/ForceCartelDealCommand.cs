#if MONO
using Console = ScheduleOne.Console;
using ScheduleOne.Cartel;
using List = System.Collections.Generic.List<string>;
#else
using Console = Il2CppScheduleOne.Console;
using Il2CppScheduleOne.Cartel;
using List = Il2CppSystem.Collections.Generic.List<string>;
using Il2CppInterop.Runtime.Injection;
#endif
using MelonLoader;
using Object = UnityEngine.Object;

namespace ScheduleToolbox.Commands;

public class ForceCartelDealCommand: Console.ConsoleCommand
{
#if !MONO
    public ForceCartelDealCommand(IntPtr ptr) : base(ptr)
    {
    }

    public ForceCartelDealCommand() : base(ClassInjector.DerivedConstructorPointer<ForceCartelDealCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif
    public override string CommandWord => "forcecarteldeal";
    public override string CommandDescription => "Forces a cartel deal, nullifying the existent deal";
    public override string ExampleUsage => "forcecarteldeal";
    
    public override void Execute(List args)
    {
        // Find CartelDealManager NetworkBehaviour
        var cartelDealManager = Object.FindObjectOfType<CartelDealManager>();
        if (cartelDealManager == null)
        {
            MelonLogger.Error("CartelDealManager not found in the scene.");
            return;
        }
        cartelDealManager.StartDeal();
    }
}