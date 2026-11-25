using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HeavenStudio.InputSystem.Loaders
{
    public static class InputXInputInitializer
    {
        [LoadOrder(3)]
        public static InputController[] Initialize()
        {
            PlayerInput.PlayerInputRefresh.Add(Refresh);
            return FindControllers();
        }

        public static InputController[] Refresh()
        {
            return FindControllers();
        }

        static InputController[] FindControllers()
        {
            if (InputJoyshockInitializer.jslDeviceConnected)
            {
                Debug.Log("JSL devices connected, skipping other controllers");
                return null;
            }

            var gamepads = Gamepad.all;
            if (gamepads.Count == 0)
            {
                Debug.Log("No remaining controllers found.");
                return null;
            }

            var controllers = new List<InputController>();
            for (int i = 0; i < gamepads.Count; i++)
            {
                var gamepad = gamepads[i];
                var controller = new InputXInput(gamepad);
                controller.SetPlayer(i + 1);
                controller.InitializeController();
                Debug.Log($"Controller \"{controller.GetDeviceName()}\" found (player {i + 1}).");
                controllers.Add(controller);
            }
            return controllers.ToArray();
        }
    }
}

namespace HeavenStudio.InputSystem
{
    public class InputXInput : InputController
    {
        Gamepad pad;

        const float STICK_DEAD = 0.6f;

        enum Buttons
        {
            DPadUp,
            DPadDown,
            DPadLeft,
            DPadRight,
            South,
            East,
            West,
            North,
            L1,
            R1,
            Start,
            Select,
            L2,
            R2
        }

        static ControlBindings defaultBindings => new ControlBindings()
        {
            Pad = new int[]
            {
                (int)Buttons.DPadUp,
                (int)Buttons.DPadDown,
                (int)Buttons.DPadLeft,
                (int)Buttons.DPadRight,
                (int)Buttons.South,
                (int)Buttons.East,
                (int)Buttons.West,
                (int)Buttons.North,
                (int)Buttons.L1,
                (int)Buttons.R1,
                (int)Buttons.Start,
                (int)Buttons.Select,
                (int)Buttons.L2,
                (int)Buttons.R2,
                -1
            },
            PointerSensitivity = 3,
            version = 0,
        };



        public InputXInput() 
        {
            pad = Gamepad.current;
        }

        public InputXInput(Gamepad gamepad)
        {
            pad = gamepad;
        }

        int GetButtonForAction(int action)
        {
            if (currentBindings.Pad == null) return -1;
            if (action < 0 || action >= BINDS_MAX) return -1;
            return currentBindings.Pad[action];
        }

        bool IsButtonDown(int btn)
        {
            if (pad == null) return false;
            switch ((Buttons)btn)
            {
                case Buttons.DPadUp:    return pad.dpad.up.isPressed;
                case Buttons.DPadDown:  return pad.dpad.down.isPressed;
                case Buttons.DPadLeft:  return pad.dpad.left.isPressed;
                case Buttons.DPadRight: return pad.dpad.right.isPressed;
                case Buttons.South:     return pad.buttonSouth.isPressed;
                case Buttons.East:      return pad.buttonEast.isPressed;
                case Buttons.West:      return pad.buttonWest.isPressed;
                case Buttons.North:     return pad.buttonNorth.isPressed;
                case Buttons.L1:        return pad.leftShoulder.isPressed;
                case Buttons.R1:        return pad.rightShoulder.isPressed;
                case Buttons.Start:     return pad.startButton.isPressed;
                case Buttons.Select:    return pad.selectButton.isPressed;
                case Buttons.L2:        return pad.leftTrigger.isPressed;
                case Buttons.R2:        return pad.rightTrigger.isPressed;
                default: return false;
            }
        }

        bool IsButtonPressedThisFrame(int btn)
        {
            if (pad == null) return false;
            switch ((Buttons)btn)
            {
                case Buttons.DPadUp:    return pad.dpad.up.wasPressedThisFrame;
                case Buttons.DPadDown:  return pad.dpad.down.wasPressedThisFrame;
                case Buttons.DPadLeft:  return pad.dpad.left.wasPressedThisFrame;
                case Buttons.DPadRight: return pad.dpad.right.wasPressedThisFrame;
                case Buttons.South:     return pad.buttonSouth.wasPressedThisFrame;
                case Buttons.East:      return pad.buttonEast.wasPressedThisFrame;
                case Buttons.West:      return pad.buttonWest.wasPressedThisFrame;
                case Buttons.North:     return pad.buttonNorth.wasPressedThisFrame;
                case Buttons.L1:        return pad.leftShoulder.wasPressedThisFrame;
                case Buttons.R1:        return pad.rightShoulder.wasPressedThisFrame;
                case Buttons.Start:     return pad.startButton.wasPressedThisFrame;
                case Buttons.Select:    return pad.selectButton.wasPressedThisFrame;
                case Buttons.L2:        return pad.leftTrigger.wasPressedThisFrame;
                case Buttons.R2:        return pad.rightTrigger.wasPressedThisFrame;
                default: return false;
            }
        }

        bool IsButtonReleasedThisFrame(int btn)
        {
            if (pad == null) return false;
            switch ((Buttons)btn)
            {
                case Buttons.DPadUp:    return pad.dpad.up.wasReleasedThisFrame;
                case Buttons.DPadDown:  return pad.dpad.down.wasReleasedThisFrame;
                case Buttons.DPadLeft:  return pad.dpad.left.wasReleasedThisFrame;
                case Buttons.DPadRight: return pad.dpad.right.wasReleasedThisFrame;
                case Buttons.South:     return pad.buttonSouth.wasReleasedThisFrame;
                case Buttons.East:      return pad.buttonEast.wasReleasedThisFrame;
                case Buttons.West:      return pad.buttonWest.wasReleasedThisFrame;
                case Buttons.North:     return pad.buttonNorth.wasReleasedThisFrame;
                case Buttons.L1:        return pad.leftShoulder.wasReleasedThisFrame;
                case Buttons.R1:        return pad.rightShoulder.wasReleasedThisFrame;
                case Buttons.Start:     return pad.startButton.wasReleasedThisFrame;
                case Buttons.Select:    return pad.selectButton.wasReleasedThisFrame;
                case Buttons.L2:        return pad.leftTrigger.wasReleasedThisFrame;
                case Buttons.R2:        return pad.rightTrigger.wasReleasedThisFrame;
                default: return false;
            }
        }

        public override void InitializeController()
        {
            LoadBindings();
        }

        public override void UpdateState()
        {
            if (pad == null) return;

            float xAxis = pad.leftStick.x.ReadValue();
            float yAxis = pad.leftStick.y.ReadValue();

            directionStateLast    = directionStateCurrent;
            directionStateCurrent = 0;
            directionStateCurrent |= (yAxis >=  STICK_DEAD) ? (1 << (int)ActionsPad.Up)    : 0;
            directionStateCurrent |= (yAxis <= -STICK_DEAD) ? (1 << (int)ActionsPad.Down)  : 0;
            directionStateCurrent |= (xAxis <= -STICK_DEAD) ? (1 << (int)ActionsPad.Left)  : 0;
            directionStateCurrent |= (xAxis >=  STICK_DEAD) ? (1 << (int)ActionsPad.Right) : 0;
        }

        public override void OnSelected()
        {
            Rumble(0.1f);
        }

        public override string GetDeviceName()
        {
            return String.IsNullOrEmpty(pad?.displayName) ? "Unknown" : pad.displayName;
        }

        public override string[] GetButtonNames()
        {
            return new string[]
            {
                "Up", "Down", "Left", "Right",
                "A", "B", "X", "Y",
                "LB", "RB", "Start", "Back", "LT", "RT",
            };
        }

        public override InputFeatures GetFeatures()
        {
            return InputFeatures.Readable_StringInput | InputFeatures.Style_Pad | InputFeatures.Style_Baton;
        }

        public override bool GetIsConnected()
        {
            return pad != null && pad.enabled && !String.IsNullOrEmpty(pad.displayName);
        }

        public override bool GetIsPoorConnection() => false;

        public override ControlBindings GetDefaultBindings()   => defaultBindings;
        public override ControlBindings GetCurrentBindings()   => currentBindings;
        public override void ResetBindings()                   => currentBindings = GetDefaultBindings();
        public override void SetCurrentBindings(ControlBindings newBinds) => currentBindings = newBinds;
        public override ControlBindings UpdateBindings(ControlBindings lastBinds) => lastBinds;
        public override int GetBindingsVersion() => 0;
        public override bool GetIsActionUnbindable(int action, ControlStyles style) => false;

        public override int GetLastButtonDown(bool strict = false)
        {
            if (pad == null) return -1;
            if (pad.dpad.up.wasPressedThisFrame)         return (int)Buttons.DPadUp;
            if (pad.dpad.down.wasPressedThisFrame)       return (int)Buttons.DPadDown;
            if (pad.dpad.left.wasPressedThisFrame)       return (int)Buttons.DPadLeft;
            if (pad.dpad.right.wasPressedThisFrame)      return (int)Buttons.DPadRight;
            if (pad.buttonSouth.wasPressedThisFrame)     return (int)Buttons.South;
            if (pad.buttonEast.wasPressedThisFrame)      return (int)Buttons.East;
            if (pad.buttonWest.wasPressedThisFrame)      return (int)Buttons.West;
            if (pad.buttonNorth.wasPressedThisFrame)     return (int)Buttons.North;
            if (pad.leftShoulder.wasPressedThisFrame)    return (int)Buttons.L1;
            if (pad.rightShoulder.wasPressedThisFrame)   return (int)Buttons.R1;
            if (pad.startButton.wasPressedThisFrame)     return (int)Buttons.Start;
            if (pad.selectButton.wasPressedThisFrame)    return (int)Buttons.Select;
            if (pad.leftTrigger.wasPressedThisFrame)     return (int)Buttons.L2;
            if (pad.rightTrigger.wasPressedThisFrame)    return (int)Buttons.R2;
            return -1;
        }

        public override int GetLastActionDown()
        {
            for (int i = 0; i < currentBindings.Pad.Length; i++)
            {
                if (GetActionDown(ControlStyles.Pad, i, out double _))
                    return i;
            }
            return -1;
        }

        public override bool GetAction(ControlStyles style, int action)
        {
            if (action == -1) return false;
            if (action is 0 or 1 or 2 or 3)
                return IsButtonDown(GetButtonForAction(action)) || (directionStateCurrent & (1 << action)) != 0;
            return IsButtonDown(GetButtonForAction(action));
        }

        public override bool GetActionDown(ControlStyles style, int action, out double dt)
        {
            dt = 0;
            if (action == -1) return false;
            if (action is 0 or 1 or 2 or 3)
                return IsButtonPressedThisFrame(GetButtonForAction(action))
                    || ((directionStateCurrent & (1 << action)) != 0 && (directionStateLast & (1 << action)) == 0);
            return IsButtonPressedThisFrame(GetButtonForAction(action));
        }

        public override bool GetActionUp(ControlStyles style, int action, out double dt)
        {
            dt = 0;
            if (action == -1) return false;
            if (action is 0 or 1 or 2 or 3)
                return IsButtonReleasedThisFrame(GetButtonForAction(action))
                    || ((directionStateCurrent & (1 << action)) == 0 && (directionStateLast & (1 << action)) != 0);
            return IsButtonReleasedThisFrame(GetButtonForAction(action));
        }

        public override float GetAxis(InputAxis axis)
        {
            if (pad == null) return 0f;
            switch (axis)
            {
                case InputAxis.AxisLTrigger: return pad.leftTrigger.ReadValue();
                case InputAxis.AxisRTrigger: return pad.rightTrigger.ReadValue();
                case InputAxis.AxisLStickX:  return pad.leftStick.x.ReadValue();
                case InputAxis.AxisLStickY:  return pad.leftStick.y.ReadValue();
                case InputAxis.AxisRStickX:  return pad.rightStick.x.ReadValue();
                case InputAxis.AxisRStickY:  return pad.rightStick.y.ReadValue();
                default: return 0f;
            }
        }

        public override Vector3 GetVector(InputVector vec)
        {
            if (pad == null) return Vector3.zero;
            switch (vec)
            {
                case InputVector.LStick:  return new Vector3(pad.leftStick.x.ReadValue(),  pad.leftStick.y.ReadValue(),  0f);
                case InputVector.RStick:  return new Vector3(pad.rightStick.x.ReadValue(), pad.rightStick.y.ReadValue(), 0f);
                default: return Vector3.zero;
            }
        }

        public override Vector2 GetPointer()
        {
            Camera cam = GameManager.instance.CursorCam;
            Vector3 rawPointerPos = Input.mousePosition;
            rawPointerPos.z = Mathf.Abs(cam.gameObject.transform.position.z);
            return cam.ScreenToWorldPoint(rawPointerPos);
        }

        public override void SetPlayer(int? playerNum)
        {
            if (playerNum == -1 || playerNum == null) { this.playerNum = null; return; }
            this.playerNum = (int)playerNum;
        }

        public override int? GetPlayer() => playerNum;

        public override bool GetFlick(out double dt)    { dt = 0; return false; }
        public override bool GetSlide(out double dt)    { dt = 0; return false; }

        public override void SetMaterialProperties(Material m)
        {
            bool b = ColorUtility.TryParseHtmlString("#F4F4F4", out Color colour);
            m.SetColor("_BodyColor", b ? colour : Color.white);
        }

        public override bool GetCurrentStyleSupported() =>
            PlayerInput.CurrentControlStyle is ControlStyles.Pad;

        public override ControlStyles GetDefaultStyle() => ControlStyles.Pad;

        public override bool GetSqueezeDown(out double dt) { dt = 0; return false; }
        public override bool GetSqueezeUp(out double dt)   { dt = 0; return false; }
        public override bool GetSqueeze()                  => false;
        public override void TogglePointerLock(bool locked) { }
        public override void RecentrePointer()              { }
        public override bool GetPointerLeftRight()          => false;

        public override void Rumble(float seconds = 0.1F)
        {
            Task.Run(() => SelectionVibrate(0.1f));
        }

        async void SelectionVibrate(float seconds)
        {
            if (pad == null) return;

            pad.SetMotorSpeeds(1f, 1f);
            await Task.Delay((int)seconds * 1000);
            pad.SetMotorSpeeds(0f, 0f);
        }
    }
}