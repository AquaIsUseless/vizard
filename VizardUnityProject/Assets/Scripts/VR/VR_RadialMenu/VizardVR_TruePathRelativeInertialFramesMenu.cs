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
using System.Linq;

/// <summary>
/// Provides a radial menu of celestial body objects that can be selected
/// as the relative body for true path line trajectory plotting
/// </summary>
public class VizardVR_TruePathRelativeInertialFramesMenu : MonoBehaviour
{
    private VizardVR_RadialMenuInputManager vizardVRRadialMenuInputMgr; //Manages user input to the radial menus
    private VizardVR_RadialOptionIndicator radialOptionIndicatorMgr; //Manages showing what option is currently active

    //Maps the celestial body scenario objects to their 
    //VizMessage.CelestialBodies[] index
    private Dictionary<string, int[]> bodyOptions = new Dictionary<string, int[]>();

    private bool firstEnable = true; //True if this is the first time this menu has been enabled

    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections the to show the available scenario celestial bodies</remarks>
    /// </summary>
    void OnEnable()
    {
        if (firstEnable)
        {
            // Get the scene references for the managers
            vizardVRRadialMenuInputMgr = GetComponent<VizardVR_RadialMenuTextOptionsMethods>().vizardVRRadialMenuInput;
            radialOptionIndicatorMgr = GetComponent<VizardVR_RadialOptionIndicator>();

            //Create a dictionary of available scenario celestial bodies and maps each
            // celestial body scenario objects to its VizMessage.CelestialBodies[] index
            //AND creates a dictionary of bodies to desired captions
            Dictionary<string, string> allRelativeBodyOptions = new Dictionary<string, string>();
            for (int i = 0; i < CelestialBodyStateUtilities.CelestialBodiesList.Count; i++)
            {
                string bodyName = CelestialBodyStateUtilities.CelestialBodiesList[i].name;
                allRelativeBodyOptions[bodyName] = $"Show relative to {bodyName}";
                int[] bodyIndexAndType = new[] {i, 0};
                bodyOptions[bodyName] = bodyIndexAndType;
            }

            //Add option for user to have Vizard use the camera target's primary body
            string parentBodyOption = "Primary";
            allRelativeBodyOptions["Primary"] = $"Show relative to spacecraft's primary body";
            bodyOptions[parentBodyOption] = new[] {0, 2};

            //Create the radial sections for those options
            GetComponent<VizardVR_RadialMenuTextOptionsMethods>().InitializeRadialMenuSections(allRelativeBodyOptions);

            //Create option indicators to show the user what is currently enabled
            radialOptionIndicatorMgr.CreateOptionIndicators(allRelativeBodyOptions.Keys.ToList(), true, true);
            firstEnable = false;
        }

        //Set indicators per the selection
        SetupIndicators();
    }

    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection,
    /// makes the selected celestial body the relative body for true
    /// path trajectory plotting.
    /// </summary>
    /// <param name="bodyName">Body name of section option invoked</param>
    public void RadialSectionInvoked(string bodyName) //Receives broadcast message from radialMenu
    {
        //Turn on the indicator for the selected option
        radialOptionIndicatorMgr.ToggleIndicator(bodyName);

        //Retrieve the body index and body type for the selected option
        int[] bodyIndexAndType = bodyOptions[bodyName];
        VizardGUISettings.TruePathLinesVisible = true;
        if (bodyIndexAndType[1] == 2) //use parent body
        {
            VizardGUISettings.UseSpacecraftParentBodyForRelativeTraj = true;
            VizardGUISettings.TruePathLineMode = 3;
        }
        else //use the specified celestial body whether it is the parent body or not
        {
            VizardGUISettings.UseSpacecraftParentBodyForRelativeTraj = false;
            VizardGUISettings.TruePathLineMode = 3;
            VizardGUISettings.RelativeBodyIndex = bodyIndexAndType[0];
        }

        //Request recalculation of true path trajectory data
        VizardGUISettings.RelativeTruePathChangeCount++;

        //Return to the main true path trajectory menu
        vizardVRRadialMenuInputMgr.GoToPrevMenu();
    }

    /// <summary>
    /// Show the currently selected option with the option indicators
    /// </summary>
    private void SetupIndicators()
    {
        if (VizardGUISettings.TruePathLineMode != 3)
        {
            radialOptionIndicatorMgr.ToggleAllIndicators(false);
        }
        else
        {
            radialOptionIndicatorMgr.ToggleIndicator(VizardGUISettings.UseSpacecraftParentBodyForRelativeTraj
                ? "Primary"
                : CelestialBodyStateUtilities.CelestialBodiesList[VizardGUISettings.RelativeBodyIndex].name);
        }
    }
}