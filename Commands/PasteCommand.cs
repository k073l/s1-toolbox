#if MONO
using Console = ScheduleOne.Console;
using ScheduleOne.PlayerScripts;
using List = System.Collections.Generic.List<string>;
using ScheduleOne.ItemFramework;
using ScheduleOne.Product;
using ScheduleOne.Storage;
#else
using Console = Il2CppScheduleOne.Console;
using Il2CppScheduleOne.PlayerScripts;
using List = Il2CppSystem.Collections.Generic.List<string>;
using Il2CppInterop.Runtime.Injection;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Product;
using Il2CppScheduleOne.Storage;
#endif
using MelonLoader;

namespace ScheduleToolbox.Commands;

[RegisterTypeInIl2Cpp]
public class PasteCommand : Console.ConsoleCommand
{
#if !MONO
    public PasteCommand(IntPtr ptr) : base(ptr)
    {
    }

    public PasteCommand() : base(ClassInjector.DerivedConstructorPointer<PasteCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif

    public override string CommandWord => "pastehand";
    public override string CommandDescription => "Gives the copied item with all its properties";
    public override string ExampleUsage => "pastehand";

    public override void Execute(List args)
    {
        if (CopyCommand.EquippedItem == null)
        {
            Melon<ScheduleToolbox>.Logger.Warning("Copied item is null!");
            return;
        }

        var item = CopyCommand.EquippedItem.GetCopy();
        if (!PlayerInventory.Instance.CanItemFitInInventory(item))
        {
            Melon<ScheduleToolbox>.Logger.Warning("Insufficient inventory space");
        }

        // check current slot first
        if (!PlayerInventory.Instance.isAnythingEquipped)
        {
            var slot = PlayerInventory.Instance.equippedSlot;
            slot.InsertItem(item);
            Melon<ScheduleToolbox>.Logger.Msg($"Pasted in {item.ID} into equipped slot");
            return;
        }

        PlayerInventory.Instance.AddItemToInventory(item);
    }
}