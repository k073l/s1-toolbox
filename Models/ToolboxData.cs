using UnityEngine;

namespace ScheduleToolbox.Models;

[Serializable]
public class ToolboxData
{
    public Dictionary<string, string> Keybindings = new();
    public Dictionary<string, SavedPosition> Positions = new();
}