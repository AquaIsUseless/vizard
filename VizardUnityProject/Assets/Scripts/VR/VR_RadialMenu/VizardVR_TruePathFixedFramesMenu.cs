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
/// Provides a radial menu of scenario objects that can
/// be selected as the body frame in which to plot the
/// true path trajectory 
/// </summary>
public class VizardVR_TruePathFixedFramesMenu : MonoBehaviour
{
    private VizardVR_RadialMenuInputManager vizardVRRadialMenuInputMgr; //Manages user input to the radial menus
    private VizardVR_RadialOptionIndicator radialOptionIndicatorMgr; //Manages showing what option is currently active

    //Maps the name of a scenario object to (1) its VizMessage.Spacecraft[]
    //or VizMessage.CelestialBodies[] index and (2) if it is a spacecraft
    //or a celestial body
    private Dictionary<string, int[]> bodyOptions = new Dictionary<string, int[]>();

    private bool firstEnable = true; //True if this is the first time this menu has been enabled

    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections the available scenario objects</remarks>
    /// </summary>
    void OnEnable()
    {
        if (firstEnable)
        {
            // Get the scene references for the managers
            vizardVRRadialMenuInputMgr = GetComponent<VizardVR_RadialMenuTextOptionsMethods>().vizardVRRadialMenuInput;
            radialOptionIndicatorMgr = GetComponent<VizardVR_RadialOptionIndicator>();

            //Create a dictionary of available scenario objects and specifies a caption for each
            Dictionary<string, string> allFixedFrameBodyOptionsAndCaptions = new Dictionary<string, string>();
            if (SpacecraftStateUtilities.ParentSpacecraftList.Count > 1)
            {
                foreach (GameObject spacecraft in SpacecraftStateUtilities.ParentSpacecraftList)
                {
                    allFixedFrameBodyOptionsAndCaptions[spacecraft.name] = $"Show in {spacecraft.name} Fixed Frame";
                    int[] bodyIndexAndType = new[] {SpacecraftStateUtilities.GetSpacecraftIndex(spacecraft.name), 1};
                    bodyOptions[spacecraft.name] = bodyIndexAndType;
                }
            }

            //Create a dictionary of available scenario objects and their VizMessage indices and if they are spacecraft or celestial bodies
            for (int i = 0; i < CelestialBodyStateUtilities.CelestialBodiesList.Count; i++)
            {
                string bodyName = CelestialBodyStateUtilities.CelestialBodiesList[i].name;
                allFixedFrameBodyOptionsAndCaptions[bodyName] = $"Show in {bodyName} Fixed Frame";
                int[] bodyIndexAndType = new[] {i, 0};
                bodyOptions[bodyName] = bodyIndexAndType;
            }

            //Create the radial sections for those options
            GetComponent<VizardVR_RadialMenuTextOptionsMethods>()
                .InitializeRadialMenuSections(allFixedFrameBodyOptionsAndCaptions);

            //Create option indicators to show the user what is currently enabled
            radialOptionIndicatorMgr.CreateOptionIndicators(allFixedFrameBodyOptionsAndCaptions.Keys.ToList(), true,
                true);

            //Set indicators per the current true path trajectory state
            SetupIndicators();
            firstEnable = false;
        }
    }

    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection,
    /// turns on the fixed frame option for the true path trajectory,
    /// and sets the selected object's body frame as the fixed frame.
    /// </summary>
    /// <param name="bodyName">Body name of section option invoked</param>
    public void RadialSectionInvoked(string bodyName) //Receives broadcast message from radialMenu
    {
        //Turn on the indicator for the selected option
        radialOptionIndicatorMgr.ToggleIndicator(bodyName);
        
        //Retrieve the body index and body type for the selected option
        int[] bodyIndexAndType = bodyOptions[bodyName];
        
        //Set the true path trajectory state
        VizardGUISettings.TruePathLinesVisible = true;
        VizardGUISettings.UseSpacecraftParentBodyForFixedFrameTraj = false;
        VizardGUISettings.FixedBodyIndex = bodyIndexAndType[0];
        VizardGUISettings.FixedBodyIsSpacecraft = (bodyIndexAndType[1] == 1);
        VizardGUISettings.TruePathLineMode = 5;

        //Request the true path trajectory script(s) update their data
        //for the changes to the true path trajectory state
        VizardGUISettings.RelativeTruePathChangeCount++;

        //Return to the root menu
        vizardVRRadialMenuInputMgr.GoToPrevMenu();
    }

    /// <summary>
    /// Turn off all option selection indicators and turn on
    /// only the currently selected option
    /// </summary>
    private void SetupIndicators()
    {
        radialOptionIndicatorMgr.ToggleAllIndicators(false);

        if ((VizardGUISettings.TruePathLineMode == 5) && (VizardGUISettings.FixedBodyIndex != -1))
        {
            if (VizardGUISettings.FixedBodyIsSpacecraft)
            {
                radialOptionIndicatorMgr.ToggleIndicator(SpacecraftStateUtilities
                    .ParentSpacecraftList[VizardGUISettings.FixedBodyIndex].name);
            }
            else
            {
                radialOptionIndicatorMgr.ToggleIndicator(CelestialBodyStateUtilities
                    .CelestialBodiesList[VizardGUISettings.FixedBodyIndex].name);
            }
        }
    }
}