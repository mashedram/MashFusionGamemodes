using MashGamemodeLibrary.Entities.Interaction.Grabbing;
using MashGamemodeLibrary.Integrations;
using UnityEngine;

namespace MashGamemodeLibrary.Debug;

public class GrapPredicateKeybind : DebugKeybind
{

    protected override KeyCode _key => KeyCode.H;
    protected override Action _onPress => () =>
    {
        if (PlayerGrabManager.GrabPredicate != null)
            PlayerGrabManager.GrabPredicate = null;
        else
            PlayerGrabManager.GrabPredicate = _ => false;
    };
}