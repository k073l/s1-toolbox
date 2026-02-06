using UnityEngine;

namespace ScheduleToolbox.Models;

[Serializable]
public class SerializableVector3
{
    public float x, y, z;

    public SerializableVector3() { }

    public SerializableVector3(Vector3 v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToUnity() => new Vector3(x, y, z);
}
