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
/// Provides a radial menu of osculating orbit line frame options
/// available for current scenario
/// </summary>
public class VizardVR_OscOrbitLinesMenu : MonoBehaviour
{
    private VizardVR_RadialOptionIndicator radialOptionIndicatorMgr; //Manages showing what option is currently active
    private bool firstEnable = true; //True if this is the first time this menu has been enabled

    //Maps the available spacecraft relative osculating orbit 
    //line options to the desired captions.
    private readonly Dictionary<string, string> allOscOrbitOptions = new Dictionary<string, string>()
    {
        {"Inertial\nFrame", "Show in Chief Spacecraft Inertial Frame"},
        {"Hill Frame", "Show in Chief Spacecraft Hill Frame"},
        {"Velocity\nFrame", "Show in Chief Spacecraft Velocity Frame"}
    };

    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections the to show the spacecraft relative
    /// orbit line options</remarks>
    /// </summary>
    void OnEnable()
    {
        if (firstEnable)
        {
            // Get the scene reference for the manager
            radialOptionIndicatorMgr = GetComponent<VizardVR_RadialOptionIndicator>();

            //Create the radial sections for those options
            GetComponent<VizardVR_RadialMenuTextOptionsMethods>().InitializeRadialMenuSections(allOscOrbitOptions);

            //Create option indicators to show the user what is currently enabled
            radialOptionIndicatorMgr.CreateOptionIndicators(allOscOrbitOptions.Keys.ToList(), true, true);

            firstEnable = false;
        }

        //Set indicators per the osculating orbit frame selection
        SetupIndicators();
    }

    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection and
    /// sets the spacecraft relative orbit line frame settings
    /// </summary>
    /// <param name="option">Name of section option invoked</param>
    public void RadialSectionInvoked(string option) //Receives broadcast message from radialMenu
    {
        //Turn on the indicator for the selected option
        radialOptionIndicatorMgr.ToggleIndicator(option);
        //Set the spacecraft relative osculating orbits settings per the option
        if (option == "Inertial\nFrame")
        {
            if (VizardGUISettings.OsculatingOrbitLinesVisible)
            {
                if (VizardGUISettings.SpacecraftRelativeOsculatingOrbits)
                {
                    VizardGUISettings.SpacecraftRelativeOsculatingOrbits = false;
                }
                else
                {
                    VizardGUISettings.OsculatingOrbitLinesVisible = false;
                }
            }
            else
            {
                VizardGUISettings.OsculatingOrbitLinesVisible = true;
                VizardGUISettings.SpacecraftRelativeOsculatingOrbits = false;
            }
        }
        else if (option == "Hill Frame")
        {
            if (!VizardGUISettings.SpacecraftRelativeOsculatingOrbits)
            {
                VizardGUISettings.OsculatingOrbitLinesVisible = true;
                VizardGUISettings.SpacecraftRelativeOsculatingOrbits = true;
                VizardGUISettings.SpacecraftRelativeOrbitMode = 1;
            }
            else
            {
                if (VizardGUISettings.SpacecraftRelativeOrbitMode != 1)
                {
                    VizardGUISettings.OsculatingOrbitLinesVisible = true;
                    VizardGUISettings.SpacecraftRelativeOsculatingOrbits = true;
                    VizardGUISettings.SpacecraftRelativeOrbitMode = 1;
                }
                else
                {
                    VizardGUISettings.OsculatingOrbitLinesVisible = false;
                    VizardGUISettings.SpacecraftRelativeOsculatingOrbits = false;
                    VizardGUISettings.SpacecraftRelativeOrbitMode = 1;
                }
            }
        }
        else if (option == "Velocity\nFrame")
        {
            if (!VizardGUISettings.SpacecraftRelativeOsculatingOrbits)
            {
                VizardGUISettings.OsculatingOrbitLinesVisible = true;
                VizardGUISettings.SpacecraftRelativeOsculatingOrbits = true;
                VizardGUISettings.SpacecraftRelativeOrbitMode = 2;
            }
            else
            {
                if (VizardGUISettings.SpacecraftRelativeOrbitMode != 2)
                {
                    VizardGUISettings.OsculatingOrbitLinesVisible = true;
                    VizardGUISettings.SpacecraftRelativeOsculatingOrbits = true;
                    VizardGUISettings.SpacecraftRelativeOrbitMode = 2;
                }
                else
                {
                    VizardGUISettings.OsculatingOrbitLinesVisible = false;
                    VizardGUISettings.SpacecraftRelativeOsculatingOrbits = false;
                    VizardGUISettings.SpacecraftRelativeOrbitMode = 1;
                }
            }
        }
        else
        {
            Debug.Log($"No support implemented for option '{option}'.");
        }
    }

    /// <summary>
    /// Show which spacecraft relative osculating orbit mode is in use (if any)
    /// </summary>
    private void SetupIndicators()
    {
        if (VizardGUISettings.OsculatingOrbitLinesVisible)
        {
            if (VizardGUISettings.SpacecraftRelativeOsculatingOrbits)
            {
                radialOptionIndicatorMgr.ToggleIndicator((VizardGUISettings.SpacecraftRelativeOrbitMode == 1)
                    ? "Hill Frame"
                    : "Velocity\nFrame");
            }
            else
            {
                radialOptionIndicatorMgr.ToggleIndicator("Inertial\nFrame");
            }
        }
    }
}