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
/// Provides a radial menu of possible bodies to attach a new secondary camera to
/// and sends user's selection to the root camera panel menu before activating
/// the target body selection radial menu
/// </summary>
public class VizardVR_CameraPanels_AttachBodyMenu : MonoBehaviour
{
    [Tooltip("Secondary Cameras root radial menu")]
    public VizardVR_CameraPanelsMenu camPanelMenu;

    private Transform sectionLabels; //Transform to hold the section labels
    private VizardVR_RadialMenuInputManager vizardVRRadialMenuInputMgr; //Radial menu input manager
    private MainCameraMovementController mainCameraMovementController; //Main camera's movement controller

    private bool firstEnable = true; //True if this is the first time the menu has been enabled


    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections with all the spacecraft
    /// and effector scenario objects as possible attach bodies for the
    /// new secondary camera</remarks>
    /// </summary>
    void OnEnable()
    {
        if (firstEnable)
        {
            vizardVRRadialMenuInputMgr = GetComponent<VizardVR_RadialMenuTextOptionsMethods>().vizardVRRadialMenuInput;

            //Add all spacecraft and effectors in current scenario
            List<string> allCameraAttachBodyOptions = new List<string>();
            foreach (GameObject spacecraft in SpacecraftStateUtilities.SpacecraftList)
            {
                allCameraAttachBodyOptions.Add(spacecraft.name);
            }

            //Create the radial sections for these options
            GetComponent<VizardVR_RadialMenuTextOptionsMethods>()
                .InitializeRadialMenuSections(allCameraAttachBodyOptions);

            firstEnable = false;
        }
    }

    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection
    /// and sends the name of the desired secondary camera attach body to
    /// the newCameraSettings being populated and activates the target
    /// body radial menu.
    /// </summary>
    /// <param name="bodyName">Body name of section option invoked</param>
    public void RadialSectionInvoked(string bodyName) //Receives broadcast message from radialMenu
    {
        camPanelMenu.NewCameraSettings.SpacecraftName = bodyName;
        //Check if there are multiple target bodies available
        //and if so, activates the target body submenu
        if ((MessageList.CurrentMessage.CelestialBodies.Count > 1) || (MessageList.CurrentMessage.Spacecraft.Count > 1))
        {
            vizardVRRadialMenuInputMgr.SetActiveMenu(camPanelMenu.targetBodyMenuMethods);
        }
        //Otherwise, select the only available target object and create the secondary camera
        else
        {
            camPanelMenu.NewCameraSettings.BodyTarget = MessageList.CurrentMessage.CelestialBodies[0].BodyName;
            camPanelMenu.AddCamera();
            vizardVRRadialMenuInputMgr.SetActiveMenu(camPanelMenu.GetComponent<VizardVR_RadialMenuMethods>());
        }
    }
}