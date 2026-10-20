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

            InputXInput gamepad = new InputXInput();
            gamepad.SetPlayer(1);
            gamepad.InitializeController();

            if (InputJoyshockInitializer.jslDeviceConnected)
            {
                Debug.Log($"Controller \"{gamepad.GetDeviceName()}\" found; JSL used instead.");
                return null;
            }
            if (!gamepad.GetIsConnected())
            {
                Debug.Log("No remaining controllers found.");
                return null;
            }

            Debug.Log($"Controller \"{gamepad.GetDeviceName()}\" found.");
            return new InputController[] { gamepad };
        }

        public static InputController[] Refresh()
        {
            InputXInput gamepad = new InputXInput();
            gamepad.SetPlayer(1);
            gamepad.InitializeController();

            if (InputJoyshockInitializer.jslDeviceConnected)
            {
                Debug.Log($"Controller \"{gamepad.GetDeviceName()}\" found; JSL used instead.");
                return null;
            }
            if (!gamepad.GetIsConnected())
            {
                Debug.Log("No remaining controllers found.");
                return null;
            }

            Debug.Log($"Controller \"{gamepad.GetDeviceName()}\" found.");
            return new InputController[] { gamepad };
        }
    }
}

namespace HeavenStudio.InputSystem
{
    public class InputXInput : InputController
    {
        Gamepad pad;

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
            Start
        }

        static ControlBindings defaultBindings = new ControlBindings()
        {
            Pad = new int[]
            {
                (int)Buttons.DPadUp,  // DPad Up
                (int)Buttons.DPadDown,  // DPad Down
                (int)Buttons.DPadLeft,  // DPad Left
                (int)Buttons.DPadRight,  // DPad Right
                (int)Buttons.South,  // South (A)
                (int)Buttons.East,  // East (B)
                (int)Buttons.West,  // West (X)
                (int)Buttons.North,  // North (Y)
                (int)Buttons.L1,  // Left Shoulder
                (int)Buttons.R1,  // Right Shoulder
                (int)Buttons.Start, // Start
                -1  
            },

            PointerSensitivity = 3,
        };

        InputDirection hatDirectionCurrent;
        InputDirection hatDirectionLast;


        public override void InitializeController()
        {
            pad = Gamepad.current; 
            LoadBindings();
        }

        public override void UpdateState()
        {
            pad = Gamepad.current; 
        }

        public override void OnSelected()
        {
            Task.Run(() => SelectionVibrate());
        }

        async void SelectionVibrate()
        {
            pad.SetMotorSpeeds(0.5f, 0.5f);
            await Task.Delay(100);
            pad.SetMotorSpeeds(0f, 0f);
        }

        public override string GetDeviceName()
        {
            return String.IsNullOrEmpty(pad?.displayName) ? "Unknown" : pad.displayName;
        }

        public override string[] GetButtonNames()
        {
            return new string[]
            {
                "Up",
                "Down",
                "Left",
                "Right",
                "A",
                "B",
                "X",
                "Y",
                "LB",
                "RB",
                "Start"
            };
        }

        public override InputFeatures GetFeatures()
        {
            return InputFeatures.Readable_StringInput | InputFeatures.Style_Pad | InputFeatures.Style_Baton;
        }

        public override bool GetIsConnected()
        {
            return !String.IsNullOrEmpty(pad?.displayName);
        }

        public override bool GetIsPoorConnection()
        {
            return false;
        }

        public override ControlBindings GetDefaultBindings()
        {
            return defaultBindings;
        }

        public override void ResetBindings()
        {
            currentBindings = GetDefaultBindings();
        }

        public override ControlBindings GetCurrentBindings()
        {
            return currentBindings;
        }

        public override void SetCurrentBindings(ControlBindings newBinds)
        {
            currentBindings = newBinds;
        }

        public override ControlBindings UpdateBindings(ControlBindings lastBinds)
        {
            return lastBinds;
        }

        public override int GetBindingsVersion()
        {
            return 0;
        }

        public override bool GetIsActionUnbindable(int action, ControlStyles style)
        {
            return false;
        }

        public override int GetLastButtonDown(bool strict = false)
        {
            if (pad == null) return -1;

            if (pad.dpad.up.wasPressedThisFrame) return 0;
            if (pad.dpad.down.wasPressedThisFrame) return 1;
            if (pad.dpad.left.wasPressedThisFrame) return 2;
            if (pad.dpad.right.wasPressedThisFrame) return 3;

            if (pad.buttonSouth.wasPressedThisFrame) return 4;
            if (pad.buttonEast.wasPressedThisFrame) return 5;
            if (pad.buttonWest.wasPressedThisFrame) return 6;
            if (pad.buttonNorth.wasPressedThisFrame) return 7;

            if (pad.leftShoulder.wasPressedThisFrame) return 8;
            if (pad.rightShoulder.wasPressedThisFrame) return 9;

            if (pad.startButton.wasPressedThisFrame) return 10;

            return -1;
        }

        public override int GetLastActionDown()
        {
            for (int i = 0; i < currentBindings.Pad.Length; i++)
            {
                if (GetActionDown(ControlStyles.Pad, i, out double dt))
                    return i;
            }
            return -1;
        }

        public override bool GetAction(ControlStyles style, int button)
        {
            if (pad == null) return false;

            switch ((Buttons)currentBindings.Pad[button])
            {
                case Buttons.DPadUp:     return pad.dpad.up.isPressed;
                case Buttons.DPadDown:   return pad.dpad.down.isPressed;
                case Buttons.DPadLeft:   return pad.dpad.left.isPressed;
                case Buttons.DPadRight:  return pad.dpad.right.isPressed;

                case Buttons.South:      return pad.buttonSouth.isPressed;
                case Buttons.East:       return pad.buttonEast.isPressed;
                case Buttons.West:       return pad.buttonWest.isPressed;
                case Buttons.North:      return pad.buttonNorth.isPressed;

                case Buttons.L1:         return pad.leftShoulder.isPressed;
                case Buttons.R1:         return pad.rightShoulder.isPressed;

                case Buttons.Start:      return pad.startButton.isPressed;

                default: return false;
            }
        }

        public override bool GetActionDown(ControlStyles style, int button, out double dt)
        {
            dt = 0;
            if (pad == null) return false;

            switch ((Buttons)currentBindings.Pad[button])
            {
                case Buttons.DPadUp:     return pad.dpad.up.wasPressedThisFrame;
                case Buttons.DPadDown:   return pad.dpad.down.wasPressedThisFrame;
                case Buttons.DPadLeft:   return pad.dpad.left.wasPressedThisFrame;
                case Buttons.DPadRight:  return pad.dpad.right.wasPressedThisFrame;

                case Buttons.South:      return pad.buttonSouth.wasPressedThisFrame;
                case Buttons.East:       return pad.buttonEast.wasPressedThisFrame;
                case Buttons.West:       return pad.buttonWest.wasPressedThisFrame;
                case Buttons.North:      return pad.buttonNorth.wasPressedThisFrame;

                case Buttons.L1:         return pad.leftShoulder.wasPressedThisFrame;
                case Buttons.R1:         return pad.rightShoulder.wasPressedThisFrame;

                case Buttons.Start:      return pad.startButton.wasPressedThisFrame;

                default: return false;
            }
        }

        public override bool GetActionUp(ControlStyles style, int button, out double dt)
        {
            dt = 0;
            if (pad == null) return false;

            switch ((Buttons)currentBindings.Pad[button])
            {
                case Buttons.DPadUp:     return pad.dpad.up.wasReleasedThisFrame;
                case Buttons.DPadDown:   return pad.dpad.down.wasReleasedThisFrame;
                case Buttons.DPadLeft:   return pad.dpad.left.wasReleasedThisFrame;
                case Buttons.DPadRight:  return pad.dpad.right.wasReleasedThisFrame;

                case Buttons.South:      return pad.buttonSouth.wasReleasedThisFrame;
                case Buttons.East:       return pad.buttonEast.wasReleasedThisFrame;
                case Buttons.West:       return pad.buttonWest.wasReleasedThisFrame;
                case Buttons.North:      return pad.buttonNorth.wasReleasedThisFrame;

                case Buttons.L1:         return pad.leftShoulder.wasReleasedThisFrame;
                case Buttons.R1:         return pad.rightShoulder.wasReleasedThisFrame;

                case Buttons.Start:      return pad.startButton.wasReleasedThisFrame;

                default: return false;
            }
        }

        public override float GetAxis(InputAxis axis)
        {
            switch (axis)
            {
                case InputAxis.AxisLTrigger:
                    return pad.leftTrigger.ReadValue();
                case InputAxis.AxisRTrigger:
                    return pad.rightTrigger.ReadValue();
                case InputAxis.AxisLStickX:
                    return pad.leftStick.x.ReadValue();
                case InputAxis.AxisLStickY:
                    return pad.leftStick.y.ReadValue();
                case InputAxis.AxisRStickX:
                    return pad.rightStick.x.ReadValue();
                case InputAxis.AxisRStickY:
                    return pad.rightStick.y.ReadValue();
                case InputAxis.PointerX:   //isn't updated for now, so always returns 0f
                //return joyTouchStateCurrent.t0X;
                case InputAxis.PointerY:
                //return joyTouchStateCurrent.t0Y;
                default:
                    return 0f;
            }
        }
        public override Vector3 GetVector(InputVector vec)
        {
            if (pad == null) return Vector3.zero;

            return pad.leftStick.ReadValue();
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
            if (playerNum == -1 || playerNum == null)
            {
                this.playerNum = null;
                return;
            }
            this.playerNum = (int)playerNum;
        }

        public override int? GetPlayer()
        {
            return playerNum;
        }

        public override bool GetFlick(out double dt)
        {
            dt = 0;
            return false;
        }

        public override bool GetSlide(out double dt)
        {
            dt = 0;
            return false;
        }

        public override void SetMaterialProperties(Material m)
        {
            bool b = ColorUtility.TryParseHtmlString("#F4F4F4", out Color colour);
            m.SetColor("_BodyColor", b ? colour : Color.white);
        }

        public override bool GetCurrentStyleSupported()
        {
            return PlayerInput.CurrentControlStyle is ControlStyles.Pad; // or ControlStyles.Baton
        }

        public override ControlStyles GetDefaultStyle()
        {
            return ControlStyles.Pad;
        }

        public override bool GetSqueezeDown(out double dt)
        {
            dt = 0;
            return false;
        }

        public override bool GetSqueezeUp(out double dt)
        {
            dt = 0;
            return false;
        }

        public override bool GetSqueeze()
        {
            return false;
        }

        public override void TogglePointerLock(bool locked)
        {
        }

        public override void RecentrePointer()
        {
        }

        public override bool GetPointerLeftRight()
        {
            return false;
        }
    }
}