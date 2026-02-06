namespace ScheduleToolbox.Models;

[Serializable]
public class SavedPositionsData
{
    public Dictionary<string, SavedPosition> Positions = new();
}