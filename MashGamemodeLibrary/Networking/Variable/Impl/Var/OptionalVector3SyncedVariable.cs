using LabFusion.Network.Serialization;
using UnityEngine;

namespace MashGamemodeLibrary.networking.Variable.Impl.Var;

public class OptionalVector3SyncedVariable : SyncedVariable<Vector3?>
{

    public OptionalVector3SyncedVariable(string name, Vector3? defaultValue) : base(name, defaultValue)
    {
    }
    protected override int? GetSize(Vector3? data)
    {
        return sizeof(float) * 3;
    }
    protected override bool Equals(Vector3? a, Vector3? b)
    {
        return a.Equals(b);
    }
    protected override Vector3? ReadValue(NetReader reader)
    {
        if (!reader.ReadBoolean()) return null;
        
        var x = reader.ReadSingle();
        var y = reader.ReadSingle();
        var z = reader.ReadSingle();
        return new Vector3(x, y, z);
    }
    protected override void WriteValue(NetWriter writer, Vector3? value)
    {
        if (value == null)
        {
            writer.Write(false);
            return;
        }

        writer.Write(true);
        
        writer.Write(value.x);
        writer.Write(value.y);
        writer.Write(value.z);
    }
}