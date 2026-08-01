using MelonLoader;
using ScheduleToolbox.Helpers;
using UnityEngine;

#if MONO
using Console = ScheduleOne.Console;
using List = System.Collections.Generic.List<string>;
using ScheduleOne.NPCs;
#else
using Console = Il2CppScheduleOne.Console;
using List = Il2CppSystem.Collections.Generic.List<string>;
using Il2CppInterop.Runtime.Injection;
using Il2CppScheduleOne.NPCs;
#endif

namespace ScheduleToolbox.Commands;

[RegisterTypeInIl2Cpp]
public class ListNPCsCommand : Console.ConsoleCommand
{
#if !MONO
    public ListNPCsCommand(IntPtr ptr) : base(ptr)
    {
    }

    public ListNPCsCommand() : base(ClassInjector.DerivedConstructorPointer<ListNPCsCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif

    private static readonly MelonLogger.Instance Logger = new($"{BuildInfo.Name}-ListNPCs");

    public override string CommandWord => "listnpcs";
    public override string CommandDescription => "Lists all loaded NPCs with optional filter";
    public override string ExampleUsage => "listnpcs [filter]";

    public override void Execute(List args)
    {
        var filter = args.Count > 0 ? args.AsEnumerable().ElementAt(0) : null;

        var matched = 0;
        foreach (var npc in NPCManager.NPCRegistry)
        {
            var displayName = npc.FullName;
            var id = npc.ID;
            var pos = npc.Movement.FootPosition;

            if (filter != null && (!displayName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                                   !id.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                continue;

            Logger.Msg($"  {displayName} ({id}) at ({pos.x:F1}, {pos.y:F1}, {pos.z:F1})");
            matched++;
        }

        Logger.Msg(
            $"Matched {matched} NPCs{(filter != null ? $" for '{filter}'" : "")} (total: {NPCManager.NPCRegistry.Count})");
    }
}