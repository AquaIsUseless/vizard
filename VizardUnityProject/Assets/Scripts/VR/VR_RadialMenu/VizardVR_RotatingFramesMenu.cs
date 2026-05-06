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
using TMPro;
using UnityEngine;

/// <summary>
/// Provides a radial menu for selection of celestial bodies
/// to set up a two body rotating frame for true path trajectory plotting
/// </summary>
public class VizardVR_RotatingFramesMenu : MonoBehaviour
{
    [Header("Radial Menu")] [Tooltip("Text field for menu instructions")]
    public TextMeshProUGUI instructions;

    private VizardVR_RadialMenuInputManager vizardVRRadialMenuInputMgr; //Manages user input to the radial menus
    private VizardVR_RadialOptionIndicator radialOptionIndicatorMgr; //Manages showing what option is currently active

    //Maps the name of celestial bodies in the scene to their
    //VizMessage.CelestialBodies[] index 
    private Dictionary<string, int> bodyOptions = new Dictionary<string, int>();

    private bool firstEnable = true; //True if this is the first time this menu has been enabled
    private int bodiesSelected; //Number of bodies selected for two body frame

    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections the to show the available celestial bodies</remarks>
    /// </summary>
    void OnEnable()
    {
        if (firstEnable)
        {
            // Get the scene references for the managers
            vizardVRRadialMenuInputMgr = GetComponent<VizardVR_RadialMenuTextOptionsMethods>().vizardVRRadialMenuInput;
            radialOptionIndicatorMgr = GetComponent<VizardVR_RadialOptionIndicator>();

            //Create a dictionary of celestial bodies in the current scenario and
            //map to their respective VizMessage.CelestialBodies[] index
            List<string> allBodies = new List<string>();
            for (int i = 0; i < CelestialBodyStateUtilities.CelestialBodiesList.Count; i++)
            {
                string bodyName = CelestialBodyStateUtilities.CelestialBodiesList[i].name;
                allBodies.Add(bodyName);
                bodyOptions[bodyName] = i;
            }

            //Create the radial sections for those options
            GetComponent<VizardVR_RadialMenuTextOptionsMethods>().InitializeRadialMenuSections(allBodies);

            //Create option indicators to show the user what is currently enabled
            radialOptionIndicatorMgr.CreateOptionIndicators(allBodies, true, false);

            firstEnable = false;
        }

        //Set indicators to show current rotating frame selections
        SetupIndicators();
        bodiesSelected = 0; //reset the number of selected bodies
        instructions.text = "Select first body."; //set the instructions
    }

    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection.
    /// If this is the first body selected, changes the instructions and
    /// asks user to select second body.
    /// If this is the second body selected, sets true path rotating frame
    /// to selected bodies and returns to true path main menu.
    /// </summary>
    /// <param name="bodyName">Body name of section option invoked</param>
    public void RadialSectionInvoked(string bodyName) //Receives broadcast message from radialMenu
    {
        switch (bodiesSelected)
        {
            case 0: //no bodies have been previously selected, set the first selected body
                radialOptionIndicatorMgr.ToggleAllIndicators(false);
                radialOptionIndicatorMgr.ToggleIndicator(bodyName);
                VizardGUISettings.RotatingFrameBody1Index = bodyOptions[bodyName];
                instructions.text = "Select second body.";
                bodiesSelected = 1;
                return;
            case 1: //one body of the two body frame has been selected previously, set the second
                int secondBody = bodyOptions[bodyName];
                if (secondBody != VizardGUISettings.RotatingFrameBody1Index)
                {
                    radialOptionIndicatorMgr.ToggleIndicator(bodyName);
                    VizardGUISettings.TruePathLinesVisible = true;
                    VizardGUISettings.RotatingFrameBody2Index = secondBody;
                    VizardGUISettings.TruePathLineMode = 4;
                    CelestialBodyStateUtilities.CalculateRotatingFramePositionAndVelocityHistories();
                    VizardGUISettings.RelativeTruePathChangeCount++;
                    vizardVRRadialMenuInputMgr.GoToPrevMenu();
                    return;
                }

                instructions.text = "Cannot select the same body for frame.";
                bodiesSelected = 1;
                return;
        }
    }

    /// <summary>
    /// Indicate which bodies are being used to calculate a two body rotating frame
    /// for the true path trajectory
    /// </summary>
    private void SetupIndicators()
    {
        radialOptionIndicatorMgr.ToggleAllIndicators(false);
        if (VizardGUISettings.TruePathLineMode == 4)
        {
            radialOptionIndicatorMgr.ToggleIndicator(CelestialBodyStateUtilities
                .CelestialBodiesList[VizardGUISettings.RotatingFrameBody1Index].name);
            radialOptionIndicatorMgr.ToggleIndicator(CelestialBodyStateUtilities
                .CelestialBodiesList[VizardGUISettings.RotatingFrameBody2Index].name);
        }
    }
}