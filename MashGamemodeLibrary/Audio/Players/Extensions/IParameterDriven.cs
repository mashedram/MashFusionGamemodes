namespace MashGamemodeLibrary.Audio.Players.Extensions;

public interface IParameterDriven<in TParameter> : ISyncedAudioPlayer, IRandomAudioPlayer<TParameter>
{
    
}