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
using Utils = ScheduleToolbox.Helpers.Utils;

namespace ScheduleToolbox.Commands;

[RegisterTypeInIl2Cpp]
public class CopyCommand : Console.ConsoleCommand
{
#if !MONO
    public CopyCommand(IntPtr ptr) : base(ptr)
    {
    }

    public CopyCommand() : base(ClassInjector.DerivedConstructorPointer<CopyCommand>())
    {
        ClassInjector.DerivedConstructorBody(this);
    }
#endif

    public override string CommandWord => "copyhand";
    public override string CommandDescription => "Copies the item held in hand";
    public override string ExampleUsage => "copyhand";

    internal static ItemInstance EquippedItem { get; set; }

    public override void Execute(List args)
    {
        if (!PlayerInventory.Instance.isAnythingEquipped)
        {
            Melon<ScheduleToolbox>.Logger.Warning("Copy command executed while nothing is held in hand!");
            return;
        }

        var item = PlayerInventory.Instance.EquippedItem;
        if (Utils.Is<ProductItemInstance>(item, out var productItem))
        {
            EquippedItem = productItem.GetCopy();
            Melon<ScheduleToolbox>.Logger.Msg($"Copied ProductItem {productItem.ID}");
            return;
        }

        if (Utils.Is<QualityItemInstance>(item, out var qualityItem))
        {
            EquippedItem = qualityItem.GetCopy();
            Melon<ScheduleToolbox>.Logger.Msg($"Copied QualityItem {qualityItem.ID}");
            return;
        }

        if (Utils.Is<WaterContainerInstance>(item, out var waterContainer))
        {
            EquippedItem = waterContainer.GetCopy();
            Melon<ScheduleToolbox>.Logger.Msg($"Copied WaterContainer {waterContainer.ID}");
            return;
        }

        if (Utils.Is<CashInstance>(item, out var cashInstance))
        {
            EquippedItem = cashInstance.GetCopy();
            Melon<ScheduleToolbox>.Logger.Msg($"Copied CashInstance {cashInstance.ID}");
            return;
        }

        if (Utils.Is<IntegerItemInstance>(item, out var integerItem))
        {
            EquippedItem = integerItem.GetCopy();
            Melon<ScheduleToolbox>.Logger.Msg($"Copied IntegerItem {integerItem.ID}");
            return;
        }

        if (Utils.Is<StorableItemInstance>(item, out var storableItem))
        {
            EquippedItem = storableItem.GetCopy();
            Melon<ScheduleToolbox>.Logger.Msg($"Copied StorageItem {storableItem.ID}");
            return;
        }

        EquippedItem = item.GetCopy();
    }
}