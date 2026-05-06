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
/// Provides a radial menu of scenario objects that can
/// be selected as the chief spacecraft for spacecraft relative
/// orbit lines and true path trajectories
/// </summary>
public class VizardVR_TruePathChiefSpacecraftMenu : MonoBehaviour
{
    private VizardVR_RadialMenuInputManager vizardVRRadialMenuInputMgr; //Manages user input to the radial menus
    private VizardVR_RadialOptionIndicator radialOptionIndicatorMgr; //Manages showing what option is currently active
    
    //Maps the name of a scenario object to (1) its VizMessage.Spacecraft[]
    //index and (2) if the option is a specific 
    //spacecraft (=1) or the user wants the current camera target to be
    //the chief spacecraft (=2)
    private Dictionary<string, int[]> bodyOptions = new Dictionary<string, int[]>();
    
    private bool firstEnable = true; //True if this is the first time this menu has been enabled
    
    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections the to show the available scenario spacecraft</remarks>
    /// </summary>
    void OnEnable()
    {
        if (firstEnable)
        {
            // Get the scene references for the managers
            vizardVRRadialMenuInputMgr = GetComponent<VizardVR_RadialMenuTextOptionsMethods>().vizardVRRadialMenuInput;
            radialOptionIndicatorMgr = GetComponent<VizardVR_RadialOptionIndicator>();
            
            //Create a dictionary of available scenario parent spacecraft and specifies a caption for each
            Dictionary<string, string> allChiefSpacecraftOptions = new Dictionary<string,string>();
            if (SpacecraftStateUtilities.ParentSpacecraftList.Count > 1)
            {
                foreach (GameObject spacecraft in SpacecraftStateUtilities.ParentSpacecraftList)
                {
                    allChiefSpacecraftOptions[spacecraft.name] = $"Set chief spacecraft to {spacecraft.name}";
                    int[] bodyIndexAndType = new[] {SpacecraftStateUtilities.GetSpacecraftIndex(spacecraft.name), 1};
                    bodyOptions[spacecraft.name] = bodyIndexAndType;
                }
            }
            //Add the option to have the current camera target
            //(if it is a parent spacecraft) be the chief spacecraft 
            string parentBodyOption = "Camera\nTarget";
            allChiefSpacecraftOptions[parentBodyOption] = $"Show relative to current camera target spacecraft";
            bodyOptions[parentBodyOption] = new[] {0, 2};
            
            //Create the radial sections for those options
            GetComponent<VizardVR_RadialMenuTextOptionsMethods>().InitializeRadialMenuSections(allChiefSpacecraftOptions);
            
            //Create option indicators to show the user what is currently enabled
            radialOptionIndicatorMgr.CreateOptionIndicators(allChiefSpacecraftOptions.Keys.ToList(), true, true);
            firstEnable = false;
        }
        //Set indicators per the current chief spacecraft selection
        SetupIndicators();
    }

    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection,
    /// makes the selected parent spacecraft or the current
    /// camera target (if it is a parent spacecraft) the chief spacecraft
    /// for relative orbit lines or true path trajectories.
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
        if (bodyIndexAndType[1] == 2) //if the current camera target should be the chief spacecraft
        {
            VizardGUISettings.SetChiefToCamTgt = true;
            if (MainCameraUtilities.CameraTarget.CompareTag("Spacecraft"))
            {
                SpacecraftController sc = MainCameraUtilities.CameraTarget.GetComponent<SpacecraftController>();
                VizardGUISettings.ChiefSpacecraftIndex = sc.isEffector ? sc.GetParentSpacecraftIndex() : sc.spacecraftIndex;
            }
        }
        else //a specific parent spacecraft is being set as the chief spacecraft
        {
            VizardGUISettings.SetChiefToCamTgt = false;
            VizardGUISettings.ChiefSpacecraftIndex = bodyIndexAndType[0];
            if(VizardGUISettings.TruePathLineMode !=2)
            {
                VizardGUISettings.TruePathLineMode = 2; //Set to spacecraft relative
                VizardGUISettings.SpacecraftRelativeOrbitMode = 1; //Set to Hill Frame
            }
        }
        //Request recalculation of the chief spacecraft position and DCM history
        SpacecraftStateUtilities.UpdateChiefSpacecraft(VizardGUISettings.ChiefSpacecraftIndex);
        
        //Request the true path trajectory script(s) update their data
        //for the changes to the true path trajectory state
        VizardGUISettings.RelativeTruePathChangeCount++;
    }

    /// <summary>
    /// Turn off all option selection indicators and turn on
    /// only the currently selected option
    /// </summary>
    private void SetupIndicators()
    {
        radialOptionIndicatorMgr.ToggleIndicator(VizardGUISettings.SetChiefToCamTgt?"Camera\nTarget":SpacecraftStateUtilities.ParentSpacecraftList[VizardGUISettings.ChiefSpacecraftIndex].name);
    }
}
