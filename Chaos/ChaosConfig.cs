using LabFusion.Network.Serialization;
using MashGamemodeLibrary.Config;

namespace Chaos;

public class ChaosConfig : IConfig
{

    public void Serialize(INetSerializer serializer)
    {
        
    }
    public object Clone()
    {
        return new ChaosConfig();
    }
}