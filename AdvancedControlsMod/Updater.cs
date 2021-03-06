﻿using System;
using System.Collections;
using SimpleJSON;
using UnityEngine;
using spaar.ModLoader.UI;
using System.Collections.Generic;
using Lench.AdvancedControls.Resources;

// ReSharper disable UnusedMember.Local

namespace Lench.AdvancedControls
{
    /// <summary>
    /// Overridable update checker.
    /// </summary>
    public class Updater : MonoBehaviour
    {
        /// <summary>
        /// Struct representing a link with a display name and URL.
        /// </summary>
        public struct Link
        {
            /// <summary>
            /// Display name of the link.
            /// </summary>
            public string DisplayName;

            /// <summary>
            /// Link URL.
            /// </summary>
            public string URL;
        }

        /// <summary>
        /// Is update available. Checked on start.
        /// </summary>
        public bool UpdateAvailable { get; private set; }

        /// <summary>
        /// Current installed version.
        /// </summary>
        public Version CurrentVersion { get; private set; } 

        /// <summary>
        /// Latest version available.
        /// </summary>
        public Version LatestVersion { get; private set; }

        /// <summary>
        /// Latest GitHub release name.
        /// </summary>
        public string LatestReleaseName { get; private set; }

        /// <summary>
        /// Latest GitHub release description body.
        /// </summary>
        public string LatestReleaseBody { get; private set; }

        /// <summary>
        /// Window visibility.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// GitHub API URL for checking the latest release.
        /// </summary>
        public string API { get; private set; }

        /// <summary>
        /// Update checker window name.
        /// </summary>
        public string WindowName { get; set; }

        /// <summary>
        /// Links to be displayed below the notification.
        /// </summary>
        public List<Link> Links { get; set; }

        private readonly int _windowID = spaar.ModLoader.Util.GetWindowID();

        /// <summary>
        /// Window Rectangle field for size and position.
        /// </summary>
        public Rect WindowRect = new Rect(300, 300, 320, 100);

        /// <summary>
        /// Check for update.
        /// </summary>
        /// <param name="windowName">Title of the updater window.</param>
        /// <param name="api">GitHub API release url.</param>
        /// <param name="current">Current version.</param>
        /// <param name="links">Links to be displayed.</param>
        /// <param name="verbose">Verbose mode.</param>
        public void Check(string windowName, string api, Version current, List<Link> links, bool verbose = false)
        {
            WindowName = windowName;
            API = api;
            CurrentVersion = current;
            Links = links;
            StartCoroutine(Check(verbose));
        }

        private IEnumerator Check(bool verbose)
        {
            var www = new WWW(API);

            yield return www;

            if (!www.isDone || !string.IsNullOrEmpty(www.error))
            {
                if (verbose) Debug.Log("=> "+Strings.Log_UnableToConnect);
                Destroy(this);
                yield break;
            }  

            string response = www.text;

            var release = JSON.Parse(response);
            LatestVersion = new Version(release["tag_name"].Value.Trim('v'));
            LatestReleaseName = release["name"].Value;
            LatestReleaseBody = release["body"].Value.Replace(@"\r\n", "\n");

            if (LatestVersion > CurrentVersion)
            {
                if (verbose) Debug.Log("=> "+Strings.Log_UpdateAvailable+" v" + LatestVersion+": "+LatestReleaseName);
                UpdateAvailable = true;
                Visible = true;
            }
            else
                if (verbose) Debug.Log("=> "+Strings.Log_ModIsUpToDate);
        }

        private void OnGUI()
        {
            if (Visible)
            {
                GUI.skin = ModGUI.Skin;
                GUI.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 0.7f);
                WindowRect = GUILayout.Window(_windowID, WindowRect, DoWindow, WindowName);
            }
        }

        private void DoWindow(int id)
        {
            // Draw release info
            GUILayout.Label(Strings.Updater_NewUpdateAvailable,
                new GUIStyle(Elements.Labels.Default) { alignment = TextAnchor.MiddleCenter });
            GUILayout.Label("<b>v" + LatestVersion + ": " + LatestReleaseName + "</b>",
                new GUIStyle(Elements.Labels.Default) { alignment = TextAnchor.MiddleCenter, fontSize = 16 });
            GUILayout.Label(LatestReleaseBody,
                new GUIStyle(Elements.Labels.Default) { fontSize = 12, margin = new RectOffset(8, 8, 16, 16) });

            // Draw updater links
            foreach (Link link in Links)
            {
                if (GUILayout.Button(link.DisplayName, Elements.Buttons.ComponentField))
                    Application.OpenURL(link.URL);
            }

            // Draw close button
            if (GUI.Button(new Rect(WindowRect.width - 38, 8, 30, 30),
                Strings.ButtonText_Close, Elements.Buttons.Red))
            {
                Destroy(this);
            }

            // Drag window
            GUI.DragWindow(new Rect(0, 0, WindowRect.width, GUI.skin.window.padding.top));
        }
    }
}