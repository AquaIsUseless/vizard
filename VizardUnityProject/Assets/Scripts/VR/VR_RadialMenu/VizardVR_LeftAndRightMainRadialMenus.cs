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
using System.Linq;
using UnityEngine;

/// <summary>
/// Provides the Vizard VR main menu options for the left-hand
/// and right-hand main menus
/// <remarks>Left hand includes Playback Controls, Instructor Mode, and Orthographic Views
/// Right hand includes View Menu options as well as Secondary Camera panels</remarks>
/// </summary>
public class VizardVR_LeftAndRightMainRadialMenus : MonoBehaviour
{
    [Tooltip("Handles user input for all radial menus")]
    public VizardVR_RadialMenuInputManager menuInputManager;
    [Tooltip("This main menu is activated by left controller")]
    public bool isLeftMenu;
    
    [Header("Left Hand Menu Submenus")]
    public VizardVR_RadialMenuMethods viewpointMenuMethods;
    public VizardVR_RadialMenuMethods playbackMenuMethods;
    public VizardVR_RadialMenuMethods instructorModeMenuMethods;
    
    [Header("Right Hand Menu Submenus")]
    public VizardVR_RadialMenuMethods cameraTargetMenuMethods;
    public VizardVR_RadialMenuMethods oscOrbitMenuMethods;
    public VizardVR_RadialMenuMethods truePathMenuMethods;
    public VizardVR_RadialMenuMethods coordinateFramesMenuMethods;
    public VizardVR_RadialMenuMethods cameraPanelsMenuMethods;
    
    private VizardVR_RadialOptionIndicator radialOptionIndicatorMgr; //Manages showing what option is currently active
    
    private bool firstEnable = true; //True if this is the first time this menu has been enabled
    
    private Dictionary<string, string> leftMainMenuOptions = new Dictionary<string, string>()
    {
	    {"Change\nView", "Change Viewpoint"},
	    {"Playback\nControls", "Playback Controls Menu"},
    };
    
    private Dictionary<string, string> rightMainMenuOptions = new Dictionary<string, string>()
    {
        {"Camera\nTarget", "Change Camera Target"},
        {"Coord.\nAxes", "Coordinate Axes Menu"},
        {"Osc.\nOrbit\nLines", "Toggle Osculating Orbit Lines"},
        {"True path\nLines", "True Path Trajectories Menu"},
        {"Satellite\nCameras", "Satellite Cameras Menu"}
    };

    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections for this main menu</remarks>
    /// </summary>
    private void OnEnable()
    {
        if (firstEnable)
        {
            menuInputManager = GetComponentInParent<VizardVR_RadialMenuInputManager>();
            // If live streaming, include the "Instructor Controls" submenu option
            if (DataManager.IsLiveSim)
            {
                leftMainMenuOptions["Instructor\nControls"]="Set Instructor Mode";
            }
            
            //Create the radial sections for those options
            GetComponent<VizardVR_RadialMenuTextOptionsMethods>().InitializeRadialMenuSections(isLeftMenu?leftMainMenuOptions:rightMainMenuOptions);
            
            //Right menu may have some options that should have indicators
            //and those need to be added.
            if (!isLeftMenu)
            {
                radialOptionIndicatorMgr = GetComponent<VizardVR_RadialOptionIndicator>();
                radialOptionIndicatorMgr.CreateOptionIndicators(rightMainMenuOptions.Keys.ToList(),false,false);
            }
            firstEnable = false;
        }
        
        //Set indicators for any active options on this menu
        SetupIndicators();
    }
    
    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection
    /// and opens the desired submenu or activates selected option.
    /// </summary>
    /// <param name="optionName">Name of section option invoked</param>
    public void RadialSectionInvoked(string optionName) //Receives broadcast message from radialMenu
    {
        if (optionName=="Change\nView")
        {
            menuInputManager.SetActiveMenu(viewpointMenuMethods);
        }else if (optionName == "Camera\nTarget")
        {
            menuInputManager.SetActiveMenu(cameraTargetMenuMethods);
            
        }
        else if (optionName == "Playback\nControls")
        {
            menuInputManager.SetActiveMenu(playbackMenuMethods);
            
        }
        else if (optionName == "Coord.\nAxes")
        {
            menuInputManager.SetActiveMenu(coordinateFramesMenuMethods);
        }
        else if (optionName == "Osc.\nOrbit\nLines")
        {
            if (SpacecraftStateUtilities.ParentSpacecraftList.Count > 1)
            {
                menuInputManager.SetActiveMenu(oscOrbitMenuMethods);
            }
            else
            {
                VizardGUISettings.OsculatingOrbitLinesVisible = !VizardGUISettings.OsculatingOrbitLinesVisible;
                radialOptionIndicatorMgr.SetIndicatorActive("Osc.\nOrbit\nLines", VizardGUISettings.OsculatingOrbitLinesVisible);
            }
        }       
        else if (optionName == "True path\nLines")
        {
            menuInputManager.SetActiveMenu(truePathMenuMethods);
        }
        else if (optionName == "Satellite\nCameras")
        {
            menuInputManager.SetActiveMenu(cameraPanelsMenuMethods);
        }
        else if (optionName == "Instructor\nControls")
        {
            menuInputManager.SetActiveMenu(instructorModeMenuMethods);
        }
        else
        {
            Debug.Log($"Did not have a handler for {optionName}.");
        }
    }

    /// <summary>
    /// On right main menu, if only one parent spacecraft is in scenario
    /// the osculating orbit lines menu is unnecessary (no spacecraft relative
    /// orbits can be plotted) so turn on/off osculating orbit line from main menu
    /// </summary>
    private void SetupIndicators()
    {
        if (!isLeftMenu)
        {
            if (SpacecraftStateUtilities.ParentSpacecraftList.Count <= 1)
            {
                radialOptionIndicatorMgr.SetIndicatorActive("Osc.\nOrbit\nLines",
                    VizardGUISettings.OsculatingOrbitLinesVisible);
            }
        }
    }
}
