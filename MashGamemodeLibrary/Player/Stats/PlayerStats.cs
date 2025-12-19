using LabFusion.Network.Serialization;

namespace MashGamemodeLibrary.Player;

public struct PlayerStats : INetSerializable
{
    public float Vitality;
    public float Speed;
    public float UpperStrength;
    public float Agility;
    public float LowerStrength;

    public void Serialize(INetSerializer serializer)
    {
        serializer.SerializeValue(ref Vitality);
        serializer.SerializeValue(ref Speed);
        serializer.SerializeValue(ref UpperStrength);
        serializer.SerializeValue(ref Agility);
        serializer.SerializeValue(ref LowerStrength);
    }

    public PlayerStats MulitplyHealth(float mult)
    {
        return this with
        {
            Vitality = Vitality * mult
        };
    }
}