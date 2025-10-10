using LabFusion.Network.Serialization;
using MashGamemodeLibrary.networking.Control;
using UnityEngine;

namespace MashGamemodeLibrary.networking.Variable.Impl;

public class FloatSyncedVariable : SyncedVariable<float>
{
    public FloatSyncedVariable(string name, float defaultValue, CatchupMoment moment = CatchupMoment.Join) : base(name, defaultValue, moment)
    {
    }

    protected override int? GetSize(float data)
    {
        return sizeof(float);
    }

    protected override bool Equals(float a, float b)
    {
        return Mathf.Approximately(a, b);
    }

    protected override float ReadValue(NetReader reader)
    {
        return reader.ReadSingle();
    }

    protected override void WriteValue(NetWriter writer, float value)
    {
        writer.Write(value);
    }
}