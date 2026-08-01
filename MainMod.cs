using System.Collections;
using MelonLoader;
using UnityEngine;
using ScheduleToolbox.Commands;
using ScheduleToolbox.Helpers;
using Object = UnityEngine.Object;

#if MONO
using Console = ScheduleOne.Console;
using ScheduleOne.UI;
#else
using Console = Il2CppScheduleOne.Console;
using Il2CppScheduleOne.UI;
#endif

[assembly: MelonInfo(
    typeof(ScheduleToolbox.ScheduleToolbox),
    ScheduleToolbox.BuildInfo.Name,
    ScheduleToolbox.BuildInfo.Version,
    ScheduleToolbox.BuildInfo.Author
)]
[assembly: MelonColor(1, 255, 0, 0)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace ScheduleToolbox;

public static class BuildInfo
{
    public const string Name = "ScheduleToolbox";
    public const string Description = "Testing tools for Schedule I";
    public const string Author = "k073l";
    public const string Version = "2.1.5";
}

public class ScheduleToolbox : MelonMod
{
    private static MelonLogger.Instance Logger;
    private bool _addedCommands = false;

    public override void OnInitializeMelon()
    {
        Logger = LoggerInstance;
        Logger.Msg("ScheduleToolbox initialized");
        var _ = PersistenceManager.Data; // force load persistence data, i love il2cpp

        ConsoleManager.Initialize();
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        if (sceneName == "Menu")
        {
            HoldToLoadManager.Instance.Reset();
            ConsoleManager.Instance.StopTimewarp();
        }
        if (sceneName == "Main")
        {
            ConsoleManager.Instance.SetConsoleUI(Object.FindObjectOfType<ConsoleUI>());
            Logger.Msg("Main scene loaded, starting console commands coroutine");
            MelonCoroutines.Start(Utils.WaitForSingleton<Console>(ConsoleCoro()));
        }
    }

    public override void OnUpdate()
    {
        ConsoleManager.Instance.Update();
        HoldToLoadManager.Instance.Update();
    }

    private IEnumerator ConsoleCoro()
    {
        if (_addedCommands)
        {
            Logger.Msg("Console commands already added, skipping.");
            yield break;
        }
        yield return new WaitForSeconds(1f);
        var commands = new List<Console.ConsoleCommand>
        {
            new FlyCommand(),
            new TeleportCommand(),
            new SavePosCommand(),
            new PosCommand(),
            new TimeWarpCommand(),
            new ForceCartelDealCommand(),
            new SetCartelInfluenceCommand(),
            new ForceDealCommand(),
            new CopyCommand(),
            new PasteCommand(),
            new ListItemsCommand(),
            new ListNPCsCommand(),
        };
        foreach (var command in commands)
        {
            var commandWord = command.CommandWord;
            Console.commands.Add(commandWord, command);
            Console.Commands.Add(command);
            Logger.Msg($"Registered command: {commandWord}");
        }
        _addedCommands = true;
    }

    public override void OnGUI()
    {
        PosCommand.OnGUI();
        SavePosCommand.OnGUI();
        TimeWarpCommand.OnGUI();
    }
}
