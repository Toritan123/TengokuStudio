using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HeavenStudio.Util;
using HeavenStudio;
using HeavenStudio.Common;
using HeavenStudio.InputSystem;


public class CustomInput : MonoBehaviour
{
    protected const int IAEmptyCat = -1;
    protected const int IAPressCat = 0;
    protected const int IAReleaseCat = 1;
    protected const int IAPressingCat = 2;
    protected const int IAFlickCat = 3;
    protected const int IAMAXCAT = 4;

    protected static bool IA_Empty(out double dt)
    {
        dt = 0;
        return false;
    }

    protected static bool IA_PadBasicPress(out double dt)
    {
        return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt);
    }
    protected static bool IA_TouchBasicPress(out double dt)
    {
        return PlayerInput.GetTouchDown(InputController.ActionsTouch.Tap, out dt);
    }
    protected static bool IA_BatonBasicPress(out double dt)
    {
        return PlayerInput.GetBatonDown(InputController.ActionsBaton.Face, out dt);
    }


    protected static bool IA_PadAltPress(out double dt)
    {
        return PlayerInput.GetPadDown(InputController.ActionsPad.South, out dt);
    }
    protected static bool IA_BatonAltPress(out double dt)
    {
        return PlayerInput.GetSqueezeDown(out dt);
    }


    protected static bool IA_PadBasicRelease(out double dt)
    {
        return PlayerInput.GetPadUp(InputController.ActionsPad.East, out dt);
    }
    protected static bool IA_TouchBasicRelease(out double dt)
    {
        return PlayerInput.GetTouchUp(InputController.ActionsTouch.Tap, out dt) && !PlayerInput.GetFlick(out _);
    }
    protected static bool IA_BatonBasicRelease(out double dt)
    {
        return PlayerInput.GetBatonUp(InputController.ActionsBaton.Face, out dt);
    }

    protected static bool IA_PadAltRelease(out double dt)
    {
        return PlayerInput.GetPadUp(InputController.ActionsPad.South, out dt);
    }
    protected static bool IA_BatonAltRelease(out double dt)
    {
        return PlayerInput.GetSqueezeUp(out dt);
    }

    protected static bool IA_PadBasicPressing(out double dt)
    {
        dt = 0;
        return PlayerInput.GetPad(InputController.ActionsPad.East);
    }
    protected static bool IA_TouchBasicPressing(out double dt)
    {
        dt = 0;
        return PlayerInput.GetTouch(InputController.ActionsTouch.Tap);
    }
    protected static bool IA_BatonBasicPressing(out double dt)
    {
        dt = 0;
        return PlayerInput.GetBaton(InputController.ActionsBaton.Face);
    }

    protected static bool IA_PadAltPressing(out double dt)
    {
        dt = 0;
        return PlayerInput.GetPad(InputController.ActionsPad.South);
    }
    protected static bool IA_BatonAltPressing(out double dt)
    {
        dt = 0;
        return PlayerInput.GetSqueeze();
    }

    protected static bool IA_TouchFlick(out double dt)
    {
        return PlayerInput.GetFlick(out dt);
    }

    protected static bool IA_PadAnyDownEast(out double dt)
    {
        return PlayerInput.GetPadDown(InputController.ActionsPad.East, out dt)
                || PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
    }

    protected static bool IA_PadAnyDown(out double dt)
    {
        return PlayerInput.GetPadDown(InputController.ActionsPad.Up, out dt)
                || PlayerInput.GetPadDown(InputController.ActionsPad.Down, out dt)
                || PlayerInput.GetPadDown(InputController.ActionsPad.Left, out dt)
                || PlayerInput.GetPadDown(InputController.ActionsPad.Right, out dt);
    }

    protected static bool IA_PadAnyUp(out double dt)
    {
        bool face = PlayerInput.GetPadUp(InputController.ActionsPad.East, out dt)
            && !(PlayerInput.GetPad(InputController.ActionsPad.Up)
                || PlayerInput.GetPad(InputController.ActionsPad.Down)
                || PlayerInput.GetPad(InputController.ActionsPad.Left)
                || PlayerInput.GetPad(InputController.ActionsPad.Right));
        bool pad = (PlayerInput.GetPadUp(InputController.ActionsPad.Up)
                || PlayerInput.GetPadUp(InputController.ActionsPad.Down)
                || PlayerInput.GetPadUp(InputController.ActionsPad.Left)
                || PlayerInput.GetPadUp(InputController.ActionsPad.Right))
            && !PlayerInput.GetPad(InputController.ActionsPad.East);
        return face || pad;
    }

    public static PlayerInput.InputAction InputAction_BasicPress =
        new("BasicPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
        IA_PadBasicPress, IA_TouchBasicPress, IA_BatonBasicPress);

    public static PlayerInput.InputAction InputAction_AltPress =
        new("AltPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
        IA_PadAltPress, IA_TouchBasicPress, IA_BatonAltPress);

    public static PlayerInput.InputAction InputAction_DPadPress =
        new("DPadPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
        IA_PadAnyDown, IA_TouchBasicPress, IA_BatonBasicPress);

    public static PlayerInput.InputAction InputAction_BasicDPadPress =
        new("DPadPress", new int[] { IAPressCat, IAPressCat, IAPressCat },
        IA_PadAnyDownEast, IA_TouchBasicPress, IA_BatonBasicPress);

    public static PlayerInput.InputAction InputAction_BasicRelease =
        new("BasicRelease", new int[] { IAReleaseCat, IAReleaseCat, IAReleaseCat },
        IA_PadBasicRelease, IA_TouchBasicRelease, IA_BatonBasicRelease);
    public static PlayerInput.InputAction InputAction_BasicPressing =
        new("BasicRelease", new int[] { IAReleaseCat, IAReleaseCat, IAReleaseCat },
        IA_PadBasicPressing, IA_TouchBasicPressing, IA_BatonBasicPressing);

    public static PlayerInput.InputAction InputAction_AltRelease =
        new("AltRelease", new int[] { IAReleaseCat, IAReleaseCat, IAReleaseCat },
        IA_PadAltRelease, IA_TouchBasicRelease, IA_BatonAltRelease);
    public static PlayerInput.InputAction InputAction_AltPressing =
        new("AltRelease", new int[] { IAReleaseCat, IAReleaseCat, IAReleaseCat },
        IA_PadAltPressing, IA_TouchBasicPressing, IA_BatonAltPressing);

    public static PlayerInput.InputAction InputAction_FlickPress =
        new("FlickPress", new int[] { IAPressCat, IAFlickCat, IAPressCat },
        IA_PadBasicPress, IA_TouchFlick, IA_BatonBasicPress);
    public static PlayerInput.InputAction InputAction_FlickRelease =
        new("FlickRelease", new int[] { IAReleaseCat, IAFlickCat, IAReleaseCat },
        IA_PadBasicRelease, IA_TouchFlick, IA_BatonBasicRelease);

    public static PlayerInput.InputAction InputAction_FlickAltPress =
        new("FlickAltPress", new int[] { IAPressCat, IAFlickCat, IAPressCat },
        IA_PadAltPress, IA_TouchFlick, IA_BatonAltPress);
    public static PlayerInput.InputAction InputAction_FlickAltRelease =
        new("FlickAltRelease", new int[] { IAReleaseCat, IAFlickCat, IAReleaseCat },
        IA_PadAltRelease, IA_TouchFlick, IA_BatonAltRelease);

    public static List<PlayerInput.InputAction> allInputActions = new List<PlayerInput.InputAction>()
    {
        InputAction_BasicPress,
        InputAction_AltPress,
        InputAction_DPadPress,
        InputAction_BasicRelease,
        InputAction_AltRelease,
        InputAction_FlickPress,
        InputAction_FlickRelease,
        InputAction_FlickAltPress,
        InputAction_FlickAltRelease,
        InputAction_BasicDPadPress
    };

}
