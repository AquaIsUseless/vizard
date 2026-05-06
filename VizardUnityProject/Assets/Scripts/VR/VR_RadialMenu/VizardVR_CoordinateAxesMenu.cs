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
/// Provides a radial menu with options to turn on/off coordinate frames
/// </summary>
public class VizardVR_CoordinateAxesMenu : MonoBehaviour
{
    private VizardVR_RadialOptionIndicator radialOptionIndicatorMgr; //Manages showing what option is currently active

    //List of the available coordinate frame choices
    private Dictionary<string, string> allCoordinateAxesOptions = new Dictionary<string, string>()
    {
	    {"Cam Tgt\nInertial", "Show Camera Target Inertial Coordinate Axes"},
	    {"Hill\nFrame", "Show Spacecraft Hill Frame Coordinate Axes"},
	    {"Velocity\nFrame", "Show Spacecraft Velocity Frame Coordinate Axes"},
	    {"All\nSpacecraft\nInertial", "Show All Spacecraft Inertial Coordinate Axes"},
	    {"All\nBodies\nInertial", "Show All Celestial Bodies Inertial Coordinate Axes"}
    };
    
    private bool firstEnable = true; //True if this is the first time this menu has been enabled

    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections the available coordinate frame options</remarks>
    /// </summary>
    private void OnEnable()
    {
        if (firstEnable)
        {
	        // Get the scene references for the manager
	        radialOptionIndicatorMgr = GetComponent<VizardVR_RadialOptionIndicator>();
	        
	        //Create the radial sections for the coordinate frame options
            GetComponent<VizardVR_RadialMenuTextOptionsMethods>().InitializeRadialMenuSections(allCoordinateAxesOptions);
            
            //Create option indicators to show the user what is currently enabled
            radialOptionIndicatorMgr.CreateOptionIndicators(allCoordinateAxesOptions.Keys.ToList(), true,false);
            
            //Set indicators per the current active coordinate frames
            SetupIndicators();
            firstEnable = false;
        }
    }
    
    /// <summary>
    /// Receives the RadialSectionInvoked message on user selection and
    /// toggles on or off the coordinate frame of that option.
    /// </summary>
    /// <param name="optionName">Name of section option invoked</param>
    public void RadialSectionInvoked(string optionName) //Receives broadcast message from radialMenu
    {
	    //Turn on the indicator for the selected option
	    radialOptionIndicatorMgr.ToggleIndicator(optionName);
	    
	    // Toggle the appropriate coordinate axes on or off
        if (optionName=="Cam Tgt\nInertial")
        {
            ToggleCameraCS(!VizardGUISettings.CameraTargetCSOn);
        }else if (optionName == "Hill\nFrame")
        {
            VizardGUISettings.ShowHillFrame = !VizardGUISettings.ShowHillFrame;
        }
        else if (optionName == "Velocity\nFrame")
        {
            VizardGUISettings.ShowVelocityFrame = !VizardGUISettings.ShowVelocityFrame;
        }
        else if (optionName == "All\nSpacecraft\nInertial")
        {
            ToggleSpacecraftCS(!VizardGUISettings.AllSpacecraftCSOn);
        }
        else if (optionName == "All\nBodies\nInertial")
        {
            TogglePlanetsCS(!VizardGUISettings.AllPlanetCSOn);
        }
        else
        {
            Debug.Log($"Did not have a handler for {optionName}.");
        }
    }
    
    /// <summary>
    /// Toggle on or off the coordinate axes of the current camera target
    /// </summary>
    /// <param name="toggleValue">True if the coordinate axes should be turned on</param>
    	private void ToggleCameraCS(bool toggleValue)
	{
		VizardGUISettings.CameraTargetCSOn = toggleValue;
		if (MainCameraUtilities.CameraTarget.CompareTag("Spacecraft"))
		{
			if (!MainCameraUtilities.CameraTarget.GetComponent<SpacecraftController>().isEffector)
			{
				if (SpacecraftStateUtilities.SpacecraftList.Count <= 1)
				{
					VizardGUISettings.AllSpacecraftCSOn = toggleValue;
				}

				MainCameraUtilities.CameraTarget.transform.GetChild(2).gameObject
					.SetActive(VizardGUISettings.CameraTargetCSOn);
			}
			else
			{
				MainCameraUtilities.CameraTarget.transform.GetChild(2).gameObject
					.SetActive(VizardGUISettings.CameraTargetCSOn);
			}
		}
		else
		{
				if (!MainCameraUtilities.CameraTarget.CompareTag("OriginTarget"))
				{
					if (MainCameraUtilities.CameraTarget.CompareTag("Sun"))
					{
						MainCameraUtilities.CameraTarget.GetComponent<SunBuilder>().sunCoordinateAxes
							.SetActive(VizardGUISettings.CameraTargetCSOn);
					}
					else
					{
						MainCameraUtilities.CameraTarget.GetComponent<PlanetController>().coordinateAxes
							.SetActive(VizardGUISettings.CameraTargetCSOn);
					}
				}
				else
				{
					MainCameraUtilities.CameraTarget.transform.GetChild(2).gameObject
						.SetActive(VizardGUISettings.CameraTargetCSOn);
				}
			
		}
	}
	
/// <summary>
/// Toggle on or off the coordinate axes of all celestial bodies in the scene
/// </summary>
/// <param name="toggleValue">True if the coordinate axes should be turned on</param>
	private void TogglePlanetsCS (bool toggleValue)
	{
		if (VizardGUISettings.AllPlanetCSOn != toggleValue)
		{
			VizardGUISettings.AllPlanetCSOn = toggleValue;
			if ((!MainCameraUtilities.CameraTarget.CompareTag("Spacecraft")) &&
			    (!MainCameraUtilities.CameraTarget.CompareTag("OriginTarget")))
			{
				VizardGUISettings.CameraTargetCSOn = toggleValue;
			}

			CelestialBodyStateUtilities.UpdatePlanetCSVisibility();
		}
	}

	/// <summary>
	/// Toggle on or off the coordinate axes of all spacecraft bodies in the scene
	/// </summary>
	/// <param name="toggleValue">True if the coordinate axes should be turned on</param>
	private void ToggleSpacecraftCS (bool toggleValue)
	{
		if (VizardGUISettings.AllSpacecraftCSOn != toggleValue)
		{
			VizardGUISettings.AllSpacecraftCSOn = toggleValue;
			if (MainCameraUtilities.CameraTarget.CompareTag("Spacecraft"))
			{
				VizardGUISettings.CameraTargetCSOn = toggleValue;
			}

			SpacecraftStateUtilities.UpdateSpacecraftCSVisibility();
		}
	}
	
	/// <summary>
	/// Turn on the option indicators for all coordinate axes options
	/// currently turned on.
	/// </summary>
	private void SetupIndicators() //Receives broadcast message from radialMenu
	{
		if (VizardGUISettings.CameraTargetCSOn)
		{
			radialOptionIndicatorMgr.ToggleIndicator("Cam Tgt\nInertial");
		}

		if (VizardGUISettings.ShowHillFrame)
		{
			radialOptionIndicatorMgr.ToggleIndicator("Hill\nFrame");
		}

		if (VizardGUISettings.ShowVelocityFrame)
		{
			radialOptionIndicatorMgr.ToggleIndicator("Velocity\nFrame");
		}

		if (VizardGUISettings.AllSpacecraftCSOn)
		{
			radialOptionIndicatorMgr.ToggleIndicator("All\nSpacecraft\nInertial");
		}

		if (VizardGUISettings.AllPlanetCSOn)
		{
			radialOptionIndicatorMgr.ToggleIndicator("All\nBodies\nInertial");
		}
	}
}
