using LabFusion.SDK.Gamemodes;

namespace MashGamemodeLibrary.Context;

public abstract class GameContext
{
    public virtual void OnUpdate(float delta) {}

    internal void Update(float delta)
    {
        OnUpdate(delta);
    }
}