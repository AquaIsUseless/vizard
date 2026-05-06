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

using System.Collections.Generic;
using UnityEngine;
using VizProtobufferMessage;
using GameObject = UnityEngine.GameObject;

/// <summary>
/// Provides a radial menu of all secondary cameras in scene
/// that can be toggled to show their view panels and
/// the option to create a new secondary camera and panel.
/// </summary>
public class VizardVR_CameraPanelsMenu : MonoBehaviour
{
    [Header("Scene Objects")] [Tooltip("GUI Panel View Manager")]
    public PanelViewManager panelViewMgr;

    [Header("Secondary Camera Setup Submenus")]
    [Tooltip("Submenu to select body on which to attach new secondary camera")]
    public VizardVR_RadialMenuMethods attachBodyMenuMethods; //Provides possible attach bodies for new secondary camera

    [Tooltip("Submenu to select body to aim secondary camera at")]
    public VizardVR_RadialMenuMethods
        targetBodyMenuMethods; //Provides possible camera target bodies for new secondary camera

    private Transform sectionLabels; //Transform to hold the section labels
    private VizardVR_RadialMenuInputManager vizardVRRadialMenuInputMgr; //Radial menu input manager
    private VizardVR_RadialOptionIndicator radialOptionIndicatorMgr; //Manages showing what option is currently active

    private Dictionary<string, GameObject>
        menuOptions; //Dictionary of option names and their associated camera panel (if applicable)

    private StandardCameraPanelMethods[] stdCameras; //Array of all standard secondary cameras in scene
    private InstrumentCameraPanelMethods[] instCameras; //Array of all instrument secondary cameras in scenario

    [HideInInspector]
    public VizMessage.Types.StandardCameraSettings
        NewCameraSettings; //Settings to be applied to create new standard secondary camera

    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections to add a new secondary camera or toggle
    /// on/off the camera and panel of an existing secondary camera</remarks>
    /// </summary>
    void OnEnable()
    {
        // Get the scene references for the managers
        vizardVRRadialMenuInputMgr = GetComponent<VizardVR_RadialMenuTextOptionsMethods>().vizardVRRadialMenuInput;
        radialOptionIndicatorMgr = GetComponent<VizardVR_RadialOptionIndicator>();

        //Clear the new camera settings
        NewCameraSettings = new VizMessage.Types.StandardCameraSettings();

        //Update arrays of standard and instrument cameras in scene
        stdCameras =
            FindObjectsByType<StandardCameraPanelMethods>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        instCameras =
            FindObjectsByType<InstrumentCameraPanelMethods>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        //Create list of all camera panels menu options 
        menuOptions = new Dictionary<string, GameObject>();
        //Add option to add a camera
        menuOptions["Add\nCamera"] = this.gameObject;
        //Add options to toggle on/off all existing standard secondary cameras in scene
        foreach (StandardCameraPanelMethods stdCam in stdCameras)
        {
            menuOptions[stdCam.panelName.text] = stdCam.gameObject;
        }

        //Add options to toggle on/off all existing instrument secondary cameras in scene
        foreach (InstrumentCameraPanelMethods instCam in instCameras)
        {
            menuOptions[instCam.panelName.text] = instCam.gameObject;
        }

        //Create a list of all the dictionary keys to pass as the options for the menu
        List<string> menuStrings = new List<string>();
        foreach (string key in menuOptions.Keys)
        {
            menuStrings.Add(key);
        }

        //Create the radial sections for those options
        GetComponent<VizardVR_RadialMenuTextOptionsMethods>().InitializeRadialMenuSections(menuStrings, true);

        //Create option indicators to show the user what is currently enabled
        radialOptionIndicatorMgr.CreateOptionIndicators(menuStrings, true, false);

        //Set indicators per the current active camera panels
        SetupIndicators();
    }

    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection,
    /// and either starts the process of adding a secondary camera
    /// or toggles the selected existing camera and its panel on or off. 
    /// </summary>
    /// <param name="option">Name of section option invoked</param>
    public void RadialSectionInvoked(string option) //Receives broadcast message from radialMenu
    {
        if (option == "Add\nCamera") //Start to add a camera
        {
            // If there are more than one spacecraft in the scenario, 
            // show all attach body options in the attach body submenu
            if (MessageList.CurrentMessage.Spacecraft.Count > 1)
            {
                vizardVRRadialMenuInputMgr.SetActiveMenu(attachBodyMenuMethods);
            }
            //otherwise, set the attach body to the only spacecraft
            else
            {
                NewCameraSettings.SpacecraftName = MessageList.CurrentMessage.Spacecraft[0].SpacecraftName;
                //If there are multiple camera target bodies, show the target body submenu
                if (MessageList.CurrentMessage.CelestialBodies.Count > 1)
                {
                    vizardVRRadialMenuInputMgr.SetActiveMenu(targetBodyMenuMethods);
                }
                //otherwise, make the camera target the only celestial body in the scene
                else
                {
                    NewCameraSettings.BodyTarget = MessageList.CurrentMessage.CelestialBodies[0].BodyName;
                    AddCamera();
                }
            }
        }
        else //Toggle on or off the chosen secondary camera and its panel
        {
            bool turnOnPanel = !menuOptions[option].activeSelf;
            menuOptions[option].SetActive(turnOnPanel);
            radialOptionIndicatorMgr.SetIndicatorActive(option, turnOnPanel);
        }
    }

    /// <summary>
    /// Add a new secondary camera once NewCameraSettings has been populated
    /// </summary>
    public void AddCamera()
    {
        panelViewMgr.VizardVR_AddStandardCameraPanelFromRadialMenu(NewCameraSettings);
    }

    /// <summary>
    /// Toggle on the indicators for all enabled cameras
    /// </summary>
    private void SetupIndicators()
    {
        foreach (string key in menuOptions.Keys)
        {
            if (key != "Add\nCamera")
            {
                radialOptionIndicatorMgr.SetIndicatorActive(key, menuOptions[key].activeSelf);
            }
        }
    }
}