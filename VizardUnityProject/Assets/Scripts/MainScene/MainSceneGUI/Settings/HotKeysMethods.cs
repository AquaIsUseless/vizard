/*
 ISC License

 Copyright (c) 2025, Autonomous Vehicle Systems Lab, University of Colorado at Boulder

 Permission to use, copy, modify, and/or distribute this software for any
 purpose with or without fee is hereby granted, provided that the above
 copyright notice and this permission notice appear in all copies.

 THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
 WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
 MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
 ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
 WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
 ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
 OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

 */

using UnityEngine;
using UnityEngine.UI;
using System;
#if VIZARD_OPENXR
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Keyboard input listener that supports use of hot keys in Vizard
/// </summary>
public class HotKeysMethods : MonoBehaviour
{
    [Header("Vizard Main Objects")] public GameObject fileMenu; // Vizard Main Scene File Submenu game object
    public ItsAboutTime timeMgr; // Vizard Main Scene playback manager
    public ViewMenuMethods viewMgr; // Vizard Main Scene View Submenu manager
    public GameObject hotkeyPanel; // Vizard Main Scene Hot Keys Info panel
    public GameObject consolePanel; // Vizard Main Scene Console Log panel
    public GameObject vizMessagePanel; // Vizard Main Scene VizMessage Display panel
    public GameObject rangeToTargetDisplay; // Vizard Main Scene Range to Target display
    public LoadScenarioFile fileLoader; // Vizard Main Scene methods to activate file browser

    /// <summary>
    /// Monodevelop method called every frame
    /// <remarks> Listens for the assigned hot keys and triggers desired action for given input</remarks>
    /// </summary>
    void Update()
    {
        if ((Input.GetKeyDown("left ctrl")) || (Input.GetKeyDown("right ctrl")))
        {
            if (Input.GetKeyDown("q"))
            {
                //launch confirm quit
                fileMenu.GetComponent<FileMenuMethods>().ShowQuitConfirmationPanel();
            }
        }

        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null)
        {
            //Only allow hotkey keyboard input when the user isn't in a GUI Field to prevent unintended hot key use
            if (Input.GetKeyDown("t"))
            {
                timeMgr.ToggleTimeDisplay();
            }

            if (Input.GetKeyDown("d"))
            {
                //toggle data rate display
                timeMgr.dataRateDisplayToggle.isOn = !timeMgr.dataRateDisplayToggle.isOn;
            }

            if (Input.GetKeyDown("r")) 
            {
                //toggle range to target display
                rangeToTargetDisplay.SetActive(!rangeToTargetDisplay.activeSelf);
            }

            if (Input.GetKeyDown("o"))
            {
                //toggle orbit lines
                VizardGUISettings.OsculatingOrbitLinesVisible = !VizardGUISettings.OsculatingOrbitLinesVisible;
            }

            if (Input.GetKeyDown("a"))
            {
                //toggle true path trajectory lines
                viewMgr.ToggleTruePathLinesVisibility(!VizardGUISettings.TruePathLinesVisible);
            }

            if (Input.GetKeyDown("h"))
            {
                //toggle hot key panel
                hotkeyPanel.SetActive(!hotkeyPanel.activeSelf);
            }

            if (Input.GetKeyDown("c"))
            {
                //toggle console log panel
                consolePanel.SetActive(!consolePanel.activeSelf);
            }

            if (Input.GetKeyDown("v"))
            {
                //toggle VizMessage display panel
                vizMessagePanel.GetComponent<MessageLoggingPanelMethods>().TogglePanelOpen();
            }

            if (Input.GetKeyDown("l"))
            {
                //toggle flashlight
                MainCameraUtilities.ToggleFlashlight();
            }

            if (Input.GetKeyDown("f"))
            {
                //Show file browser
                if (!DataManager.IsLiveSim)
                {
                    fileLoader.SelectFile();
                }
            }

            //Movie camera motion keys
            if (Input.GetKeyDown("s"))
            {
                //stop all keyboard camera motion
                MainCameraUtilities.KeyVertPan = 0;
                MainCameraUtilities.KeyHorizPan = 0;
                MainCameraUtilities.KeyRoll = 0;
                MainCameraUtilities.KeyZoom = 0;
            }

            if (!VizardGUISettings.GetVRMenuActive()) //trigger vertical pan upward
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    MainCameraUtilities.KeyVertPan += 1;
                }

                if (Input.GetKeyDown(KeyCode.DownArrow)) //trigger vertical pan downward
                {
                    MainCameraUtilities.KeyVertPan -= 1;
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow)) //trigger horizontal pan left
                {
                    MainCameraUtilities.KeyHorizPan += 1;
                }

                if (Input.GetKeyDown(KeyCode.RightArrow)) //trigger horizontal pan right
                {
                    MainCameraUtilities.KeyHorizPan -= 1;
                }

                if (Input.GetKey(KeyCode.LeftShift) || (Input.GetKey(KeyCode.RightShift)))
                {
                    if (Input.GetKeyDown(KeyCode.Period)) //trigger camera clockwise roll
                    {
                        MainCameraUtilities.KeyRoll += 1;
                    }
                    else if (Input.GetKeyDown(KeyCode.Comma)) //trigger camera counterclockwise roll
                    {
                        MainCameraUtilities.KeyRoll -= 1;
                    }
                }

                if (Input.GetKeyDown(KeyCode.LeftBracket)) //trigger main camera zoom in
                {
                    MainCameraUtilities.KeyZoom += 1;
                }

                if (Input.GetKeyDown(KeyCode.RightBracket)) //trigger main camera zoom out
                {
                    MainCameraUtilities.KeyZoom -= 1;
                }
            }
        }
    }
}