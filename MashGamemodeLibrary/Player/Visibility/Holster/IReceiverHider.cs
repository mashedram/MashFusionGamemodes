namespace MashGamemodeLibrary.Vision.Holster;

internal interface IReceiverHider
{
    bool SetHidden(bool hidden);
    bool FetchRenderers(bool? hidden = null);
}