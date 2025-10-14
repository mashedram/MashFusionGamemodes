namespace MashGamemodeLibrary.Vision.Holster;

internal interface IReceiverHider
{
    void SetHidden(bool hidden);
    void Update(bool? hidden = null);
}