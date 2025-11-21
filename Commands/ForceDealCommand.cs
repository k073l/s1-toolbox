using MelonLoader;
using ScheduleToolbox.Helpers;
#if MONO
using Console = ScheduleOne.Console;
using List = System.Collections.Generic.List<string>;
using ScheduleOne.Economy;
using ScheduleOne.NPCs;
#else
using Il2CppInterop.Runtime.Injection;
using Console = Il2CppScheduleOne.Console;
using List = Il2CppSystem.Collections.Generic.List<string>;
using Il2CppScheduleOne.Economy;
using Il2CppScheduleOne.NPCs;
#endif

namespace ScheduleToolbox.Commands;

[RegisterTypeInIl2Cpp]
public class ForceDealCommand: Console.ConsoleCommand
{
#if !MONO
    public ForceDealCommand(IntPtr ptr) : base(ptr)
    {
    }

    public ForceDealCommand() : base(ClassInjector.DerivedConstructorPointer<ForceDealCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif
    private static readonly MelonLogger.Instance Logger = new MelonLogger.Instance($"{BuildInfo.Name}-ForceDeal");
    
    public override string CommandWord => "forcedeal";
    public override string CommandDescription => "Forces a customer deal to occur.";
    public override string ExampleUsage => "forcedeal kyle_cooley";
    
    public override void Execute(List args)
    {
        if (args.Count != 1)
        {
            Logger.Warning("Usage: forcedeal <customer_name>");
            return;
        }

        var customerName = args.AsEnumerable().ElementAt(0);
        Logger.Msg($"Forcing deal with customer: {customerName}");
        
        var customer = NPCManager.GetNPC(customerName);
        if (customer == null)
        {
            Logger.Error($"Customer '{customerName}' not found.");
            return;
        }

        var customerComp = customer.transform.GetComponent<Customer>();
        if (customerComp == null)
        {
            Logger.Error($"Customer component not found on NPC '{customerName}'.");
            return;
        }
        
        customerComp.ForceDealOffer();
        Logger.Msg($"Forced deal with customer: {customerName}");
    }
}