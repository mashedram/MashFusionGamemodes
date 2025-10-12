using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Control;
using MashGamemodeLibrary.networking.Validation;
using UnityEngine;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class Vector3SyncedVariable : SyncedVariable<Vector3>
{
    public Vector3SyncedVariable(string name, Vector3 defaultValue, INetworkRoute? route = null) : base(name, defaultValue, route)
    {
    }

    protected override int? GetSize(Vector3 data)
    {
        return sizeof(float) * 3;
    }

    protected override bool Equals(Vector3 a, Vector3 b)
    {
        return a.Equals(b);
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