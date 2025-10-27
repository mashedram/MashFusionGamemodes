namespace MashGamemodeLibrary.Vision.Holster;

internal interface IReceiverHider
{
    bool SetHidden(bool hidden);
    bool Update(bool? hidden = null);
}