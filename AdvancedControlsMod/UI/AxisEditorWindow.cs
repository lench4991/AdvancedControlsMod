﻿using System;
using Lench.AdvancedControls.Axes;
using Lench.AdvancedControls.Resources;
using spaar.ModLoader.UI;
using Steamworks;
using UnityEngine;
// ReSharper disable UnusedMember.Local

namespace Lench.AdvancedControls.UI
{
    internal interface IAxisEditor
    {
        void DrawAxis(Rect windowRect);
        void Open();
        void Close();
        string GetHelpURL();
        string GetNote();
        string GetError();
    }

    internal class AxisEditorWindow : MonoBehaviour
    {
        internal bool ShowHelp { get; set; } = false;

        private readonly int _windowID = spaar.ModLoader.Util.GetWindowID();
        private Rect _windowRect = new Rect(0, 0, 320, 100);

        private string _windowName = Strings.AxisEditorWindow_WindowTitle_CreateNewAxis;
        private string _saveName = string.Empty;
        private InputAxis _axis;

        public Vector2 Position
        {
            get { return _windowRect.position; }
            set { _windowRect.position = value; }
        }

        public bool ContainsMouse
        {
            get
            {
                var mousePos = UnityEngine.Input.mousePosition;
                mousePos.y = Screen.height - mousePos.y;
                return _windowRect.Contains(mousePos);
            }
        }

        public Action<InputAxis> OnAxisSelect;

        public void SaveAxis()
        {
            if (_axis.Name == _saveName || !AxisManager.LocalAxes.ContainsKey(_axis.Name))
                _axis.Dispose();
            _axis = _axis.Clone();
            _axis.Editor.Open();
            _axis.Name = _saveName;
            AxisManager.AddLocalAxis(_axis);
            OnAxisSelect?.Invoke(_axis);
            Destroy(this);
        }

        public static AxisEditorWindow CreateAxis(Action<InputAxis> selectAxis = null)
        {
            var window = Mod.Controller.AddComponent<AxisEditorWindow>();
            window.OnAxisSelect = selectAxis;
            window._windowName = Strings.AxisEditorWindow_WindowTitle_CreateNewAxis;
            window._axis = null;
            return window;
        }

        public static AxisEditorWindow EditAxis(InputAxis axis, Action<InputAxis> selectAxis = null)
        {
            var window = Mod.Controller.AddComponent<AxisEditorWindow>();
            window.OnAxisSelect = selectAxis;
            window._windowName = string.Format(Strings.AxisEditorWindow_WindowTitle_Edit, axis.Name);
            window._saveName = axis.Name;
            window._axis = axis;
            window._axis.Editor.Open();
            return window;
        }

        /// <summary>
        /// Render window.
        /// </summary>
        private void OnGUI()
        {
            GUI.skin = Util.Skin;
            _windowRect = GUILayout.Window(_windowID, _windowRect, DoWindow, _windowName,
                GUILayout.Width(320),
                GUILayout.Height(100));
        }

        private void OnDestroy()
        {
            _axis?.Editor.Close();
        }

        private void DoWindow(int id)
        {
            if(_axis == null)
            {
                // Draw add buttons
                if (GUILayout.Button(Strings.AxisEditorWindow_ButtonText_ControllerAxis, Elements.Buttons.ComponentField))
                {
                    _axis = new ControllerAxis(Strings.AxisEditorWindow_DefaultAxisName_NewControllerAxis);
                    _windowName = Strings.AxisEditorWindow_WindowTitle_CreateNewControllerAxis;
                }
                if (GUILayout.Button(Strings.AxisEditorWindow_ButtonText_KeyAxis, Elements.Buttons.ComponentField))
                {
                    _axis = new KeyAxis(Strings.AxisEditorWindow_DefaultAxisName_NewKeyAxis);
                    _windowName = Strings.AxisEditorWindow_WindowTitle_CreateNewKeyAxis;
                }
                if (GUILayout.Button(Strings.AxisEditorWindow_ButtonText_MouseAxis, Elements.Buttons.ComponentField))
                {
                    _axis = new MouseAxis(Strings.AxisEditorWindow_DefaultAxisName_NewMouseAxis);
                    _windowName = Strings.AxisEditorWindow_WindowTitle_CreateNewMouseAxis;
                }
                if (GUILayout.Button(Strings.AxisEditorWindow_ButtonText_ChainAxis, Elements.Buttons.ComponentField))
                {
                    _axis = new ChainAxis(Strings.AxisEditorWindow_DefaultAxisName_NewChainAxis);
                    _windowName = Strings.AxisEditorWindow_WindowTitle_CreateNewChainAxis;
                }
                if (GUILayout.Button(Strings.AxisEditorWindow_ButtonText_CustomAxis, Elements.Buttons.ComponentField))
                {
                    _axis = new CustomAxis(Strings.AxisEditorWindow_DefaultAxisName_NewCustomAxis);
                    _windowName = Strings.AxisEditorWindow_WindowTitle_CreateNewCustomAxis;
                }
                if (_axis != null)
                {
                    _saveName = _axis.Name;
                    _axis.Editor.Open();
                }
            }
            else
            {
                // Draw save text field and save button
                if (_axis.Saveable)
                {
                    GUILayout.BeginHorizontal();
                    _saveName = GUILayout.TextField(_saveName,
                        Elements.InputFields.Default);

                    if (GUILayout.Button(Strings.ButtonText_Save,
                        Elements.Buttons.Default,
                        GUILayout.Width(80))
                        && _saveName != string.Empty)
                    {
                        SaveAxis();
                    }
                    GUILayout.EndHorizontal();
                }

                // Draw axis editor
                _axis.GetEditor().DrawAxis(_windowRect);

                // Draw error message
                if (_axis.GetEditor().GetError() != null)
                {
                    GUILayout.Label(_axis.GetEditor().GetError(), new GUIStyle(Elements.Labels.Default) { margin = new RectOffset(8, 8, 12, 8) });
                }

                // Draw note message
                if (_axis.GetEditor().GetNote() != null)
                {
                    GUILayout.Label(_axis.GetEditor().GetNote(), new GUIStyle(Elements.Labels.Default) { margin = new RectOffset(8, 8, 12, 8) });
                }

                // Draw help button
                if (_axis.GetEditor().GetHelpURL() != null)
                    if (GUI.Button(new Rect(_windowRect.width - 76, 8, 30, 30),
                        Strings.ButtonText_Help, Elements.Buttons.Red))
                    {
                        try
                        {
                            SteamFriends.ActivateGameOverlayToWebPage(_axis.GetEditor().GetHelpURL());
                        }
                        catch
                        {
                            Application.OpenURL(_axis.GetEditor().GetHelpURL());
                        }
                    }
            }


            // Draw close button
            if (GUI.Button(new Rect(_windowRect.width - 38, 8, 30, 30),
                Strings.ButtonText_Close, Elements.Buttons.Red))
            {
                OnAxisSelect = null;
                Destroy(this);
            }

            // Drag window
            GUI.DragWindow(new Rect(0, 0, _windowRect.width, GUI.skin.window.padding.top));
        }
    }
}
