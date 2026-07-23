using MelonLoader;
using ScheduleToolbox.Helpers;

#if MONO
using Console = ScheduleOne.Console;
using List = System.Collections.Generic.List<string>;
using ScheduleOne.ItemFramework;
#else
using Console = Il2CppScheduleOne.Console;
using List = Il2CppSystem.Collections.Generic.List<string>;
using Il2CppInterop.Runtime.Injection;
using Il2CppScheduleOne.ItemFramework;
#endif

namespace ScheduleToolbox.Commands;

[RegisterTypeInIl2Cpp]
public class ListItemsCommand : Console.ConsoleCommand
{
#if !MONO
    public ListItemsCommand(IntPtr ptr) : base(ptr)
    {
    }

    public ListItemsCommand() : base(ClassInjector.DerivedConstructorPointer<ListItemsCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif

    private static readonly MelonLogger.Instance Logger = new($"{BuildInfo.Name}-ListItems");

    public override string CommandWord => "listitems";
    public override string CommandDescription => "Lists all item definitions with optional filter";
    public override string ExampleUsage => "listitems [filter]";

    public override void Execute(List args)
    {
        var filter = args.Count > 0 ? args.AsEnumerable().ElementAt(0) : null;
        var items = Utils.GetAllStorableItemDefinitions();

        var matched = 0;
        foreach (var item in items)
        {
            var id = item.ID;
            var displayName = item.Name;
            if (filter != null &&
                (!displayName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                 !id.Contains(filter, StringComparison.OrdinalIgnoreCase)))
                continue;

            Logger.Msg($"  [{displayName}] ({id})");
            matched++;
        }

        Logger.Msg(
            $"Matched {matched} item definitions{(filter != null ? $" for '{filter}'" : "")} ({items.Count} total)");
    }
}