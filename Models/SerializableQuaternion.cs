using UnityEngine;

namespace ScheduleToolbox.Models;

[Serializable]
public class SerializableQuaternion
{
    public float x, y, z, w;

    public SerializableQuaternion() { }

    public SerializableQuaternion(Quaternion q)
    {
        x = q.x;
        y = q.y;
        z = q.z;
        w = q.w;
    }

    public Quaternion ToUnity() => new Quaternion(x, y, z, w);
}