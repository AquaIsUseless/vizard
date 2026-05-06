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
/// Provides a radial menu of spacecraft relative
/// true path trajectory options
/// </summary>
public class VizardVR_TruePathSpacecraftRelativeMenu : MonoBehaviour
{
    [Tooltip("Submenu to select the chief spacecraft")]
    public VizardVR_RadialMenuMethods chiefSpacecraftMenuMethods;

    private VizardVR_RadialMenuInputManager vizardVRRadialMenuInputMgr; //Manages user input to the radial menus
    private VizardVR_RadialOptionIndicator radialOptionIndicatorMgr; //Manages showing what option is currently active

    private bool firstEnable = true; //True if this is the first time this menu has been enabled

    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections with the spacecraft relative true path trajectory options</remarks>
    /// </summary>
    void OnEnable()
    {
        if (firstEnable)
        {
            // Get the scene references for the managers
            vizardVRRadialMenuInputMgr = GetComponent<VizardVR_RadialMenuTextOptionsMethods>().vizardVRRadialMenuInput;
            radialOptionIndicatorMgr = GetComponent<VizardVR_RadialOptionIndicator>();

            //Create a dictionary of spacecraft relative frame options
            Dictionary<string, string> options = new Dictionary<string, string>();
            options.Add("Set Chief\nSpacecraft", "Set the chief spacecraft");
            options.Add("Hill", "Show in the Hill Frame");
            options.Add("Velocity", "Show in the Velocity Frame");
            options.Add("Inertial", "Show in the relative Inertial Frame");

            //Create the radial sections for those options
            GetComponent<VizardVR_RadialMenuTextOptionsMethods>().InitializeRadialMenuSections(options);

            //Create option indicators to show the user what is currently enabled
            radialOptionIndicatorMgr.CreateOptionIndicators(options.Keys.ToList(), true, true);
            firstEnable = false;
        }

        //Set indicators per the current menu option selection
        SetupIndicators();
    }

    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection and
    /// opens the appropriate submenu or sets the true path trajectory
    /// state for the selected option
    /// </summary>
    /// <param name="option">Name of section option invoked</param>
    public void RadialSectionInvoked(string option) //Receives broadcast message from radialMenu
    {
        //Enable the true path trajectory lines
        VizardGUISettings.TruePathLinesVisible = true;

        //Show the chief spacecraft selection submenu
        if (option == "Set Chief\nSpacecraft") // spacecraft for relative body
        {
            vizardVRRadialMenuInputMgr.SetActiveMenu(chiefSpacecraftMenuMethods);
        }
        //Set the true path trajectory to the desired spacecraft relative frame
        else
        {
            radialOptionIndicatorMgr.ToggleIndicator(option);
            VizardGUISettings.TruePathLineMode = 2;
            if (option == "Hill")
            {
                VizardGUISettings.SpacecraftRelativeOrbitMode = 1;
            }
            else if (option == "Velocity")
            {
                VizardGUISettings.SpacecraftRelativeOrbitMode = 2;
            }
            else if (option == "Inertial")
            {
                VizardGUISettings.SpacecraftRelativeOrbitMode = 3;
            }

            //Update the true path position and DCM histories for the current chief spacecraft
            SpacecraftStateUtilities.UpdateChiefSpacecraft(VizardGUISettings.ChiefSpacecraftIndex);

            //Request recalculation of true path trajectory data for current options
            VizardGUISettings.RelativeTruePathChangeCount++;
        }
    }

    /// <summary>
    /// Turn on the indicator for the current spacecraft relative frame
    /// </summary>
    private void SetupIndicators()
    {
        if (VizardGUISettings.TruePathLineMode != 2)
        {
            radialOptionIndicatorMgr.ToggleAllIndicators(false);
        }
        else
        {
            switch (VizardGUISettings.SpacecraftRelativeOrbitMode)
            {
                case 1:
                    radialOptionIndicatorMgr.ToggleIndicator("Hill");
                    break;
                case 2:
                    radialOptionIndicatorMgr.ToggleIndicator("Velocity");
                    break;
                case 3:
                    radialOptionIndicatorMgr.ToggleIndicator("Inertial");
                    break;
            }
        }
    }
}