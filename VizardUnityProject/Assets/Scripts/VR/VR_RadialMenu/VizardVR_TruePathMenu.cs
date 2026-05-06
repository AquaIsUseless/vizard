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
/// Provides a radial menu of the true path trajectory
/// modes available for the current scenario
/// </summary>
public class VizardVR_TruePathMenu : MonoBehaviour
{
    [Header("True Path Trajectory Submenus")]
    [Tooltip("Two body rotating frame submenu")]
    public VizardVR_RadialMenuMethods rotatingFrameMenuMethods;
    [Tooltip("Body Fixed frame submenu")]
    public VizardVR_RadialMenuMethods fixedFrameMenuMethods;
    [Tooltip("Celestial Body Relative frame submenu")]
    public VizardVR_RadialMenuMethods bodyRelativeFrameMenuMethods;
    [Tooltip("Spacecraft Relative frame submenu")]
    public VizardVR_RadialMenuMethods spacecraftRelativeFrameMenuMethods;
    [Header("Scene Objects")]
    [Tooltip("Vizard View Menu methods")]
    public ViewMenuMethods viewMenuMethods;
    
    private VizardVR_RadialMenuInputManager vizardVRRadialMenuInputMgr; //Manages user input to the radial menus
    private VizardVR_RadialOptionIndicator radialOptionIndicatorMgr; //Manages showing what option is currently active
    private bool firstEnable = true; //True if this is the first time this menu has been enabled
    
    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections with the available frames
    /// in which the true path trajectory can be plotted</remarks>
    /// </summary>
    void OnEnable()
    {
        if (firstEnable)
        {
            // Get the scene references for the managers
            vizardVRRadialMenuInputMgr = GetComponent<VizardVR_RadialMenuTextOptionsMethods>().vizardVRRadialMenuInput;
            radialOptionIndicatorMgr = GetComponent<VizardVR_RadialOptionIndicator>();
            
            //Create a dictionary of available true path trajectory frame options
            Dictionary<string, string> options = new Dictionary<string, string>();
            options.Add("Off", "Hide true path line");
            options.Add("Inertial", "Show in BSK Inertial Frame");
            //If there are multiple parent spacecraft, spacecraft relative is available
            if (SpacecraftStateUtilities.ParentSpacecraftList.Count > 1)
            {
                options.Add("Spacecraft\nRelative", "Show relative to selected spacecraft");
                //options.Add("Hill", "Show in Chief Spacecraft Hill Frame ");
                //options.Add("Velocity", "Show in Chief Spacecraft Velocity Frame");
            }

            // Add the celestial body relative inertial frame option
            options.Add("Body\nRelative", "Show relative to selected body");
            
            //If there a multiple celestial bodies, a two body rotating frame submenu is added
            if (CelestialBodyStateUtilities.CelestialBodiesList.Count >= 2)
            {
                options.Add("Rotating", "Show in Two-Body Rotating Frame");
            }
            
            //Add the body fixed frame submenu option
            options.Add("Body\nFixed", "Show in Body-Fixed Frame");

            //Create the radial sections for those options
            GetComponent<VizardVR_RadialMenuTextOptionsMethods>().InitializeRadialMenuSections(options);
            
            //Create option indicators to show the user what is currently enabled
            radialOptionIndicatorMgr.CreateOptionIndicators(options.Keys.ToList(), true, true);
            firstEnable = false;
        }

        //Set indicators per the current true path trajectory state
        SetupIndicators();
    }

    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection and opens
    /// up the applicable submenu or (if no submenu required) updates the
    /// true path trajectory state to be the selected option.
    /// </summary>
    /// <param name="option">Name of section option invoked</param>
    public void RadialSectionInvoked(string option) //Receives broadcast message from radialMenu
    {
        //Turn on the indicator for the selected option
        radialOptionIndicatorMgr.ToggleIndicator(option);

        if (option == "Off")
        {
            //Turn off the true path trajectory line(s)
            viewMenuMethods.ToggleTruePathLinesVisibility(false);
        }
        else //Open the appropriate submenu or set the true path state if possible
        {
            if (option == "Inertial")
            {
                VizardGUISettings.TruePathLineMode = 1;
                VizardGUISettings.TruePathLinesVisible = true;
            }
            else if (option == "Spacecraft\nRelative")
            {
                vizardVRRadialMenuInputMgr.SetActiveMenu(spacecraftRelativeFrameMenuMethods);
            }
            else if (option == "Body\nRelative")
            {
                vizardVRRadialMenuInputMgr.SetActiveMenu(bodyRelativeFrameMenuMethods);
            }
            else if (option == "Rotating")
            {
                if (CelestialBodyStateUtilities.CelestialBodiesList.Count == 2)
                {
                    VizardGUISettings.RotatingFrameBody1Index = 0;
                    VizardGUISettings.RotatingFrameBody2Index = 1;
                    CelestialBodyStateUtilities.CalculateRotatingFramePositionAndVelocityHistories();
                    VizardGUISettings.TruePathLineMode = 4;
                    VizardGUISettings.TruePathLinesVisible = true;
                    VizardGUISettings.RelativeTruePathChangeCount++;
                }
                else
                {
                    vizardVRRadialMenuInputMgr.SetActiveMenu(rotatingFrameMenuMethods);
                }
            }
            else if (option == "Body\nFixed")
            {
                vizardVRRadialMenuInputMgr.SetActiveMenu(fixedFrameMenuMethods);
            }
            else
            {
                Debug.Log($"No support implemented for option '{option}'.");
            }

            viewMenuMethods.ToggleTruePathLinesVisibility(true);
        }
    }

    /// <summary>
    /// Indicate the currently active true path trajectory option
    /// </summary>
    private void SetupIndicators()
    {
        if (!VizardGUISettings.TruePathLinesVisible)
        {
            radialOptionIndicatorMgr.ToggleIndicator("Off");
        }
        else
        {
            switch (VizardGUISettings.TruePathLineMode)
            {
                case 1:
                    radialOptionIndicatorMgr.ToggleIndicator("Inertial");
                    break;
                case 2:
                    radialOptionIndicatorMgr.ToggleIndicator("Spacecraft\nRelative");
                    break;
                case 3:
                    radialOptionIndicatorMgr.ToggleIndicator("Body\nRelative");
                    break;
                case 4:
                    radialOptionIndicatorMgr.ToggleIndicator("Rotating");
                    break;
                case 5:
                    radialOptionIndicatorMgr.ToggleIndicator("Body\nFixed");
                    break;
            }
        }
    }
}