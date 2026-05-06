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

/// <summary>
/// Provides a radial menu of possible targets to aim secondary camera at
/// and sends user's selection to the root camera panel menu to build desired
/// secondary camera and its view panel.
/// </summary>
public class VizardVR_CameraPanels_TargetBodyMenu : MonoBehaviour
{
    [Tooltip("Secondary Cameras root radial menu")]
    public VizardVR_CameraPanelsMenu camPanelMenu;

    private Transform sectionLabels; //Transform to hold the section labels
    private VizardVR_RadialMenuInputManager vizardVRRadialMenuInputMgr; //Radial menu input manager
    private MainCameraMovementController mainCameraMovementController; //Main camera's movement controller


    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections with options appropriate
    /// to the previously selected attach body for the new secondary camera</remarks>
    /// </summary>
    void OnEnable()
    {
        vizardVRRadialMenuInputMgr = GetComponent<VizardVR_RadialMenuTextOptionsMethods>().vizardVRRadialMenuInput;
        
        //Add all possible spacecraft and effectors (except the one that has been selected as the attach body)
        List<string> allCameraTargetOptions = new List<string>();
        foreach (GameObject spacecraft in SpacecraftStateUtilities.SpacecraftList)
        {
            if (spacecraft.name != camPanelMenu.NewCameraSettings.SpacecraftName)
            {
                allCameraTargetOptions.Add(spacecraft.name);
            }
        }

        //Add all celestial bodies in scenario
        foreach (GameObject body in CelestialBodyStateUtilities.CelestialBodiesList)
        {
            allCameraTargetOptions.Add(body.name);
        }

        //Create the radial sections for these options
        GetComponent<VizardVR_RadialMenuTextOptionsMethods>()
            .InitializeRadialMenuSections(allCameraTargetOptions, true);
    }

    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection
    /// and sends the name of the desired secondary camera target to
    /// the newCameraSettings being populated and invokes the creation of
    /// the new secondary camera and panel.
    /// </summary>
    /// <param name="bodyName">Body name of section option invoked</param>
    public void RadialSectionInvoked(string bodyName) //Receives broadcast message from radialMenu
    {
        camPanelMenu.NewCameraSettings.BodyTarget = bodyName;
        camPanelMenu.AddCamera();
        vizardVRRadialMenuInputMgr.SetActiveMenu(camPanelMenu.GetComponent<VizardVR_RadialMenuMethods>());
    }
}