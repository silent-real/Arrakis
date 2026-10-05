/*
 * Arrakis | Managers/InputManager.cs
 *
 * Copyright (C) 2026 Arrakis
 * https://github.com/silent-real/Arrakis
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using GorillaNetworking;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using Valve.VR;
using CommonUsages = UnityEngine.XR.CommonUsages;

namespace Arrakis.Managers
{
    public class InputManager
    {
        public static bool GetInput(InputType type, Hand hand, bool pc = false, bool forpcgun = false)
        {
            if (pc && !CheckPC())
                return false;
            return pc ? PCControl(hand, type, forpcgun) : GetHandControl(hand, type);
        }

        private static bool CheckPC()
        {
            return !XRSettings.isDeviceActive && UnityEngine.Application.platform != RuntimePlatform.Android;
        }

        private static bool PCControl(Hand hand, InputType type, bool pcgun = false)
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;
            if (keyboard == null)
                return false;
            switch (type)
            {
                case InputType.Joystick:
                    return hand == Hand.Left ? keyboard.leftAltKey.isPressed : keyboard.rightAltKey.isPressed;
                case InputType.Trigger:
                    if (pcgun)
                        return mouse != null && mouse.leftButton.isPressed;
                    return hand == Hand.Left ? keyboard.minusKey.isPressed : keyboard.equalsKey.isPressed;
                case InputType.Grip:
                    if (pcgun)
                        return mouse != null && mouse.rightButton.isPressed;
                    return hand == Hand.Left ? keyboard.leftBracketKey.isPressed : keyboard.rightBracketKey.isPressed;
                case InputType.Secondary:
                    return hand == Hand.Left ? keyboard.rKey.isPressed : keyboard.tKey.isPressed;
                case InputType.Primary:
                    return hand == Hand.Left ? keyboard.cKey.isPressed : keyboard.vKey.isPressed;
                default:
                    return false;
            }
        }

        private static bool GetHandControl(Hand hand, InputType type)
        {
            bool isSteam = PlayFabAuthenticator.instance.platform;
            switch (type)
            {
                case InputType.Joystick:
                    if (hand == Hand.Left)
                    {
                        if (isSteam)
                        {
                            return SteamVR_Actions.gorillaTag_LeftJoystickClick.GetState(SteamVR_Input_Sources.LeftHand);
                        }
                        return ControllerInputPoller.instance.leftControllerDevice.TryGetFeatureValue(CommonUsages.secondary2DAxisClick, out bool leftJoystick) && leftJoystick;
                    }
                    if (hand == Hand.Right)
                    {
                        if (isSteam)
                        {
                            return SteamVR_Actions.gorillaTag_RightJoystickClick.GetState(SteamVR_Input_Sources.RightHand);
                        }
                        return ControllerInputPoller.instance.rightControllerDevice.TryGetFeatureValue(CommonUsages.secondary2DAxisClick, out bool rightJoystick) && rightJoystick;
                    }
                    break;
                case InputType.Trigger:
                    if (hand == Hand.Left)
                    {
                        return ControllerInputPoller.instance.leftControllerTriggerButton;
                    }
                    if (hand == Hand.Right)
                    {
                        return ControllerInputPoller.instance.rightControllerTriggerButton;
                    }
                    break;
                case InputType.Grip:
                    if (hand == Hand.Left)
                    {
                        return ControllerInputPoller.instance.leftGrab;
                    }
                    if (hand == Hand.Right)
                    {
                        return ControllerInputPoller.instance.rightGrab;
                    }
                    break;
                case InputType.Secondary:
                    if (hand == Hand.Left)
                    {
                        return ControllerInputPoller.instance.leftControllerSecondaryButton;
                    }
                    if (hand == Hand.Right)
                    {
                        return ControllerInputPoller.instance.rightControllerSecondaryButton;
                    }
                    break;
                case InputType.Primary:
                    if (hand == Hand.Left)
                    {
                        return ControllerInputPoller.instance.leftControllerPrimaryButton;
                    }
                    if (hand == Hand.Right)
                    {
                        return ControllerInputPoller.instance.rightControllerPrimaryButton;
                    }
                    break;
            }
            return false;
        }

        public static Vector2 GetAxis(Hand hand)
        {
            Vector2 axis = Vector2.zero;
            bool isSteam = PlayFabAuthenticator.instance.platform.PlatformTag == "Steam";
            switch (hand)
            {
                case Hand.Left:
                    if (isSteam)
                    {
                        axis = SteamVR_Actions.gorillaTag_LeftJoystick2DAxis.GetAxis(SteamVR_Input_Sources.LeftHand);
                    }
                    else
                    {
                        ControllerInputPoller.instance.leftControllerDevice.TryGetFeatureValue(CommonUsages.secondary2DAxis, out axis);
                    }
                    break;
                case Hand.Right:
                    if (isSteam)
                    {
                        axis = SteamVR_Actions.gorillaTag_RightJoystick2DAxis.GetAxis(SteamVR_Input_Sources.RightHand);
                    }
                    else
                    {
                        ControllerInputPoller.instance.rightControllerDevice.TryGetFeatureValue(CommonUsages.secondary2DAxis, out axis);
                    }
                    break;
            }
            return axis;
        }

        public enum Hand
        {
            Left,
            Right
        }
        public enum InputType
        {
            JoystickAxis,
            Joystick,
            Trigger,
            Grip,
            Secondary,
            Primary
        }
    }
}