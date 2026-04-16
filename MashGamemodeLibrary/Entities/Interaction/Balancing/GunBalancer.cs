namespace MashGamemodeLibrary.Entities.Interaction.Balancing;

public class GunBalancer
{
    public readonly IGunModifier[] Modifiers;

    public GunBalancer(params IGunModifier[] modifiers)
    {
        Modifiers = modifiers;
    }

    public float GetDamage(GunInfo gunInfo)
    {
        var modifiers = new float[Modifiers.Length];
        for (var i = 0; i < Modifiers.Length; i++)
        {
            // We want to accumulate the modifiers multiplicatively, so we take 1 - modifier to get the "remaining" damage after the modifier is applied
            var modifier = Modifiers[i].GetModifier(gunInfo);
            modifiers[i] = 1f - modifier;
        }

        // This should accumulate the modifiers additively
        // All damage is based around 1 so no need to multiply later
        return modifiers.Aggregate(1f, (current, modifier) => current + modifier);
    }
}