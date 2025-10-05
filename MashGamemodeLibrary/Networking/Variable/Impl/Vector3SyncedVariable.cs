using LabFusion.Network.Serialization;
using UnityEngine;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class Vector3SyncedVariable : SyncedVariable<Vector3>
{
    public Vector3SyncedVariable(string name, Vector3 defaultValue) : base(name, defaultValue)
    {
    }

    protected override int? GetSize(Vector3 data)
    {
        return sizeof(float) * 3;
    }

    protected override Vector3 ReadValue(NetReader reader)
    {
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var z = reader.ReadSingle();
        return new Vector3(x, y, z);
    }

    protected override void WriteValue(NetWriter writer, Vector3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }
}