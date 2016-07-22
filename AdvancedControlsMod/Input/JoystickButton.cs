﻿using System;

namespace Lench.AdvancedControls.Input
{
    /// <summary>
    /// Joystick button for mapping in input axes.
    /// </summary>
    public class JoystickButton : Button
    {
        private Controller controller;
        private int index;
        private Guid guid;

        private bool down = false;
        private bool pressed = false;
        private bool released = false;

        /// <summary>
        /// Joystick button identifying string of the following format:
        /// joy:[index]:[device_guid]
        /// </summary>
        public string ID { get { return "joy:" + index + ":" + guid; } }

#pragma warning disable CS1591
        public bool IsDown { get { return down; } }
        public bool Pressed { get { return pressed; } }
        public bool Released { get { return released; } }
        public float Value { get { return down ? 1 : 0; } }
        public string Name { get { return controller != null ? controller.ButtonNames[index] : "<color=#FF0000>Unknown button</color>"; } }
        public bool Connected { get { return controller != null && controller.Connected; } }
#pragma warning restore CS1591

        /// <summary>
        /// Creates a joystick button for a given controller.
        /// </summary>
        /// <param name="controller">Controller object.</param>
        /// <param name="index">Index of the button.</param>
        public JoystickButton(Controller controller, int index)
        {
            this.controller = controller;
            this.index = index;
            this.guid = controller.GUID;

            ACM.Instance.DeviceManager.OnButton += HandleEvent;
            ACM.Instance.DeviceManager.OnDeviceAdded += UpdateDevice;
        }

        /// <summary>
        /// Creates a joystick button from an identifier string.
        /// </summary>
        /// <param name="id"></param>
        public JoystickButton(string id)
        {
            var args = id.Split(':');
            if (args[0].Equals("joy"))
            {
                index = int.Parse(args[1]);
                guid = new Guid(args[2]);
                controller = Controller.Get(guid);
            }
            else
                throw new FormatException("Specified ID does not represent a joystick button.");

            ACM.Instance.DeviceManager.OnButton += HandleEvent;
            ACM.Instance.DeviceManager.OnDeviceAdded += UpdateDevice;
        }

        private void HandleEvent(SDL.SDL_Event e, bool down)
        {
            if (controller == null) return;
            if (e.cdevice.which != controller.Index &&
                e.jdevice.which != controller.Index)
                return;
            if (controller.IsGameController)
            {
                var button = SDL.SDL_GameControllerGetBindForButton(controller.game_controller, (SDL.SDL_GameControllerButton)index).button;
                if (e.cbutton.button == button)
                {
                    pressed = this.down != down && down;
                    released = this.down != down && !down;
                    this.down = down;
                }
            }
            else
            {
                if (e.jbutton.button == index)
                {
                    pressed = this.down != down && down;
                    released = this.down != down && !down;
                    this.down = down;
                }
            }
        }

        private void UpdateDevice(SDL.SDL_Event e)
        {
            controller = Controller.Get(guid);
        }
    }
}
