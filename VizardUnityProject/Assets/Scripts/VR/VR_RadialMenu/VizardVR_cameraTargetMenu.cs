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
using System.Collections.Generic;
/// <summary>
/// Provides a radial menu of scenario objects that can
/// be selected as main camera target
/// </summary>
public class VizardVR_CameraTargetMenu : MonoBehaviour
{
    private VizardVR_RadialMenuInputManager vizardVRRadialMenuInputMgr; //Manages user input to the radial menus
    private VizardVR_RadialOptionIndicator radialOptionIndicatorMgr; //Manages showing what option is currently active
    private MainCameraViewManager mainCameraViewMgr; //Handles changes in the main camera target
    
    private Transform sectionLabels; //GameObject to hold the section labels
    private bool firstEnable = true; //True if this is the first time the menu is enabled
    
    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections the to show the available main camera targets</remarks>
    /// </summary>
    void OnEnable()
    {
        if (firstEnable)
        {
            // Get the scene references for the managers
            vizardVRRadialMenuInputMgr = GetComponent<VizardVR_RadialMenuTextOptionsMethods>().vizardVRRadialMenuInput;
            radialOptionIndicatorMgr = GetComponent<VizardVR_RadialOptionIndicator>();
            mainCameraViewMgr = MainCameraUtilities.MainCamera.GetComponent<MainCameraViewManager>();

            //Create a list of all possible main camera targets
            List<string> allCameraTargetOptions = new List<string>();
            //Add all parent spacecraft
            foreach (GameObject spacecraft in SpacecraftStateUtilities.ParentSpacecraftList)
            {
                allCameraTargetOptions.Add(spacecraft.name);
            }
            //Add all celestial bodies
            foreach (GameObject cb in CelestialBodyStateUtilities.CelestialBodiesList)
            {
                allCameraTargetOptions.Add(cb.name);
            }

            //Create the radial sections for those options
            GetComponent<VizardVR_RadialMenuTextOptionsMethods>().InitializeRadialMenuSections(allCameraTargetOptions);
            
            //Create option indicators to show the user what is currently enabled
            radialOptionIndicatorMgr.CreateOptionIndicators(allCameraTargetOptions, true,true);
            
            //Turn on the indicator for the current main camera target
            radialOptionIndicatorMgr.ToggleIndicator(MainCameraUtilities.CameraTargetName);
            firstEnable = false;
        }
    }
    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection and
    /// requests the main camera change its target to the new object
    /// </summary>
    /// <param name="bodyName">Body name of section option invoked</param>
    public void RadialSectionInvoked(string bodyName) //Receives broadcast message from radialMenu
    {
        //Get the scenario object to set as the main camera target
        GameObject bodyToTarget = CelestialBodyStateUtilities.GetGameObjectWithBodyName(bodyName);
        //Set that body as the main camera target
        if (bodyToTarget != null)
        {
            mainCameraViewMgr.SetupChangeOfMainCameraTarget(bodyToTarget);
            radialOptionIndicatorMgr.ToggleIndicator(bodyName);
            vizardVRRadialMenuInputMgr.ToggleMenuMode();
        }
        else
        {
            Debug.Log($"Could not find the game object named \'{bodyName}\' in scene");
        }
    }
}