using LabFusion.Network.Serialization;
using UnityEngine;

namespace MashGamemodeLibrary.networking.Variable.Encoder.Impl;

public class Vector3Encoder : IEncoder<Vector3>
{

    public int GetSize(Vector3 value)
    {
        return sizeof(float) * 3;
    }
    public Vector3 Read(NetReader reader)
    {
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var z = reader.ReadSingle();

        return new Vector3(x, y, z);
    }
    public void Write(NetWriter writer, Vector3 value)
    {
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }
}