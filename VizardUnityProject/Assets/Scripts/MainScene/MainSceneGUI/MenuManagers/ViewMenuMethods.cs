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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ViewMenuMethods : MonoBehaviour
{
	[Header("Coordinate Frame Menu Items")]
	public Toggle cameraTargetCSToggle;
	public Toggle allPlanetsCSToggle;
	public Toggle allSpacecraftCSToggle;
	public Toggle allEffectorsCSToggle;
	
	public Toggle showHillFrameToggle;
	public Toggle showVelFrameToggle;
	
	[Header("Orbit Line Menu Items")]
	public Toggle orbitLinesToggle;
	public Toggle oscRelativeOrbitsToggle;

	[Header("True Path Trajectory Menu Items")]
	public ToggleGroup truePathToggleGroup;
	public Toggle truePathLinesToggle;
	public Toggle trueSpacecraftRelativeOrbitsToggle;
	public Toggle scRelHillFrameToggle;
	public Toggle scRelVelocityFrameToggle;
	public Toggle scRelInertialFrameToggle;
	public Toggle trueBodyRelativeOrbitsToggle;
	public Toggle trueRotatingFramesToggle;
	public Toggle trueFixedFramesToggle;
	
	public Button setChiefSpacecraftPanelButton;
	public Button setSpacecraftRelativeFrameButton;
	public Button setBodyRelativeBodyButton;
	public Button setRotatingFrameBodiesButton;
	public Button setFixedFrameBodyButton;
	
	public GameObject rotatingFrameBodySelectionPanel;
	public GameObject celestialBodyRelativeSelectionPanel;
	public GameObject fixedFrameBodySelectionPanel;
	
	[Header("Ground Track Menu Items")]
	public Toggle oscGroundTracksToggle;
	public Toggle truePathGroundTracksToggle;
	
	[Header("Live Sync Setting")]
	public Toggle forceSyncSettingsToggle;

	void OnEnable()
	{
		CheckForPlanets();
		CheckForEffectors();
		orbitLinesToggle.isOn = VizardGUISettings.OsculatingOrbitLinesVisible;
		oscRelativeOrbitsToggle.isOn = VizardGUISettings.SpacecraftRelativeOsculatingOrbits;

		truePathLinesToggle.isOn = (VizardGUISettings.TruePathLinesVisible);
		scRelHillFrameToggle.isOn = VizardGUISettings.SpacecraftRelativeOrbitMode==1;
		scRelVelocityFrameToggle.isOn = VizardGUISettings.SpacecraftRelativeOrbitMode==2;
		scRelInertialFrameToggle.isOn = VizardGUISettings.SpacecraftRelativeOrbitMode == 3;

		trueSpacecraftRelativeOrbitsToggle.isOn = VizardGUISettings.TruePathLineMode == 2;
		trueBodyRelativeOrbitsToggle.isOn = VizardGUISettings.TruePathLineMode == 3;
		trueRotatingFramesToggle.isOn = VizardGUISettings.TruePathLineMode == 4;
		trueFixedFramesToggle.isOn = VizardGUISettings.TruePathLineMode == 5;

		cameraTargetCSToggle.isOn = VizardGUISettings.CameraTargetCSOn;
		allSpacecraftCSToggle.isOn = VizardGUISettings.AllSpacecraftCSOn;
		allPlanetsCSToggle.isOn = VizardGUISettings.AllPlanetCSOn;
		showHillFrameToggle.isOn = VizardGUISettings.ShowHillFrame;
		showVelFrameToggle.isOn = VizardGUISettings.ShowVelocityFrame;

		oscGroundTracksToggle.isOn = VizardGUISettings.OsculatingGroundTrackOn;
		truePathGroundTracksToggle.isOn = VizardGUISettings.TruePathGroundTrackOn;

		SetRelativeOrbitOptionsInteractable();
		
		if (DataManager.SocketIsReceiveOnly)
		{
			if (MessageList.LatestBroadcastSyncSettings != null)
			{
				BroadcastSyncToggleMode(MessageList.LatestBroadcastSyncSettings.ForceTrainerSettings);
			}
		}
	}
void Start ()
	{
		cameraTargetCSToggle.onValueChanged.AddListener (ToggleCameraCS);
		allPlanetsCSToggle.onValueChanged.AddListener (TogglePlanetsCS);
		allSpacecraftCSToggle.onValueChanged.AddListener (ToggleSpacecraftCS);
		allEffectorsCSToggle.onValueChanged.AddListener(ToggleEffectorCS);
		showHillFrameToggle.onValueChanged.AddListener(ToggleHillFrameCS);
		showVelFrameToggle.onValueChanged.AddListener(ToggleVelFrameCS);
		
		orbitLinesToggle.onValueChanged.AddListener(ToggleOrbitLineVisibility);
		oscRelativeOrbitsToggle.onValueChanged.AddListener(ToggleOsculatingRelativeOrbits);
		
		truePathLinesToggle.onValueChanged.AddListener(ToggleTruePathLinesVisibility);
		trueSpacecraftRelativeOrbitsToggle.onValueChanged.AddListener(ToggleSpacecraftRelativeTrueTrajectory);
		scRelHillFrameToggle.onValueChanged.AddListener(Toggle_SCRelHillFrameMode);
		scRelVelocityFrameToggle.onValueChanged.AddListener(Toggle_SCRelVelocityFrameMode);
		scRelInertialFrameToggle.onValueChanged.AddListener(Toggle_SCRelInertialFrameMode);
		trueRotatingFramesToggle.onValueChanged.AddListener(ToggleRotatingFrameTrueTrajectory);
		trueBodyRelativeOrbitsToggle.onValueChanged.AddListener(ToggleBodyRelativeTrueTrajectory);
		trueFixedFramesToggle.onValueChanged.AddListener(ToggleBodyFixedFrameTrueTrajectory);
		setBodyRelativeBodyButton.onClick.AddListener(ToggleCelestialBodyRelativeSelectionPanel);
		setRotatingFrameBodiesButton.onClick.AddListener(ToggleRotatingFrameBodiesSelectionPanel);
		setFixedFrameBodyButton.onClick.AddListener(ToggleFixedFrameBodySelectionPanel);
		
		oscGroundTracksToggle.onValueChanged.AddListener(ToggleOsculatingGroundTracks);
		truePathGroundTracksToggle.onValueChanged.AddListener(ToggleTruePathGroundTracks);
	}


	public void CheckForPlanets()
	{
		if (SpacecraftStateUtilities.SpacecraftMsgOnly)
		{
			allPlanetsCSToggle.isOn = VizardGUISettings.AllPlanetCSOn;
			//Change text on the all planet cs toggle
			allPlanetsCSToggle.transform.GetComponentInChildren<TextMeshProUGUI>().text = "Show Origin CS";
			orbitLinesToggle.isOn = false;
			orbitLinesToggle.interactable = false;
			orbitLinesToggle.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
			
			truePathLinesToggle.interactable = false;
			truePathLinesToggle.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
			
			oscGroundTracksToggle.interactable = false;
			oscGroundTracksToggle.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
			truePathGroundTracksToggle.interactable = false;
			truePathGroundTracksToggle.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
			
			showHillFrameToggle.interactable = false;
			showHillFrameToggle.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
			showVelFrameToggle.interactable = false;
			showVelFrameToggle.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
			
		}
		else
		{
			if (CelestialBodyStateUtilities.CelestialBodiesList.Count < 2)
			{
				trueRotatingFramesToggle.isOn = false;
				trueRotatingFramesToggle.interactable = false;
				trueRotatingFramesToggle.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
				setRotatingFrameBodiesButton.interactable = false;
			}
		}
	}

	private void CheckForEffectors()
	{
		if (SpacecraftStateUtilities.EffectorList.Count > 0)
		{
			allEffectorsCSToggle.interactable = true;
			allEffectorsCSToggle.isOn = VizardGUISettings.AllEffectorCSOn;
			allEffectorsCSToggle.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.white;
		}
		else
		{
			allEffectorsCSToggle.isOn = false;
			allEffectorsCSToggle.interactable = false;
			allEffectorsCSToggle.transform.GetComponentInChildren<TextMeshProUGUI>().color = Color.gray;
		}
	}

	private void ToggleCameraCS(bool toggleValue)
	{
		VizardGUISettings.CameraTargetCSOn = toggleValue;
		if (MainCameraUtilities.CameraTarget.CompareTag("Spacecraft"))
		{
			if (!MainCameraUtilities.CameraTarget.GetComponent<SpacecraftController>().isEffector)
			{


				if (SpacecraftStateUtilities.SpacecraftList.Count <= 1)
				{
					allSpacecraftCSToggle.isOn = toggleValue;
				}

				MainCameraUtilities.CameraTarget.transform.GetChild(2).gameObject
					.SetActive(VizardGUISettings.CameraTargetCSOn);
				RefFrameCSTogglesActive(true);
			}
			else
			{
				RefFrameCSTogglesActive(false);
					if (SpacecraftStateUtilities.EffectorList.Count <= 1)
					{
						allEffectorsCSToggle.isOn = toggleValue;
					}

					MainCameraUtilities.CameraTarget.transform.GetChild(2).gameObject
						.SetActive(VizardGUISettings.CameraTargetCSOn);
			}
		}
		else
		{
			RefFrameCSTogglesActive(false);
				if (!MainCameraUtilities.CameraTarget.CompareTag("OriginTarget"))
				{
					if (CelestialBodyStateUtilities.CelestialBodiesList.Count <= 1)
					{
						allPlanetsCSToggle.isOn = toggleValue;
					}

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
	

	public void TogglePlanetsCS (bool toggleValue)
	{
		if (VizardGUISettings.AllPlanetCSOn != toggleValue)
		{
			VizardGUISettings.AllPlanetCSOn = toggleValue;
			if ((!MainCameraUtilities.CameraTarget.CompareTag("Spacecraft")))
			{
				VizardGUISettings.CameraTargetCSOn = toggleValue;
				cameraTargetCSToggle.isOn = toggleValue;
			}

			CelestialBodyStateUtilities.UpdatePlanetCSVisibility();
		}
	}

	public void ToggleSpacecraftCS (bool toggleValue)
	{
		if (VizardGUISettings.AllSpacecraftCSOn != toggleValue)
		{
			VizardGUISettings.AllSpacecraftCSOn = toggleValue;
			if (MainCameraUtilities.CameraTarget.CompareTag("Spacecraft"))
			{
				VizardGUISettings.CameraTargetCSOn = toggleValue;
				cameraTargetCSToggle.isOn = toggleValue;
			}

			SpacecraftStateUtilities.UpdateSpacecraftCSVisibility();
		}
	}

	private void ToggleEffectorCS (bool toggleValue)
	{
		if (VizardGUISettings.AllEffectorCSOn != toggleValue)
		{
			VizardGUISettings.AllEffectorCSOn = toggleValue;
			if (MainCameraUtilities.CameraTarget.CompareTag("Spacecraft")&&(MainCameraUtilities.CameraTarget.GetComponent<SpacecraftController>().isEffector))
			{
				VizardGUISettings.CameraTargetCSOn = toggleValue;
				cameraTargetCSToggle.isOn = toggleValue;
			}

			SpacecraftStateUtilities.UpdateEffectorCSVisibility();
		}
	}

	private void ToggleOrbitLineVisibility(bool toggleValue)
	{
		VizardGUISettings.OsculatingOrbitLinesVisible = toggleValue;
	}

	private void ToggleOsculatingRelativeOrbits(bool toggleValue){
		VizardGUISettings.SpacecraftRelativeOsculatingOrbits = toggleValue;
		if (toggleValue)
		{
			orbitLinesToggle.isOn = true;
		}
	}

	public void ToggleTruePathLinesVisibility(bool toggleValue){
		VizardGUISettings.TruePathLinesVisible = toggleValue;
		foreach (GameObject line in SpacecraftStateUtilities.SpacecraftOrbitLinesList)
		{
			line.GetComponent<OsculatingOrbitLine>().ToggleTruePathLineGameObject(toggleValue);
		}
	}

	private void ToggleSpacecraftRelativeTrueTrajectory(bool toggleValue)
	{
		if (toggleValue)
		{
			VizardGUISettings.TruePathLineMode = 2;
			VizardGUISettings.TruePathLinesVisible = true;
			SpacecraftStateUtilities.UpdateChiefSpacecraft(VizardGUISettings.ChiefSpacecraftIndex);
			truePathLinesToggle.isOn = true;
			// if use camera target for chief, set relative body index to -1,
		}
		else
		{
			if (!truePathToggleGroup.AnyTogglesOn())
			{
				VizardGUISettings.TruePathLineMode = 1;
			}
		}
	}

	private void Toggle_SCRelHillFrameMode(bool isOn)
	{
		if (isOn)
		{
			VizardGUISettings.SpacecraftRelativeOrbitMode = 1;
			VizardGUISettings.RelativeTruePathChangeCount++;
			SpacecraftStateUtilities.UpdateChiefSpacecraft(VizardGUISettings.ChiefSpacecraftIndex);
		}
	}
	private void Toggle_SCRelVelocityFrameMode(bool isOn)
	{
		if (isOn)
		{
			VizardGUISettings.SpacecraftRelativeOrbitMode = 2;
			VizardGUISettings.RelativeTruePathChangeCount++;
			SpacecraftStateUtilities.UpdateChiefSpacecraft(VizardGUISettings.ChiefSpacecraftIndex);
		}
	}
	private void Toggle_SCRelInertialFrameMode(bool isOn)
	{
		if (isOn)
		{
			VizardGUISettings.SpacecraftRelativeOrbitMode = 3;
			VizardGUISettings.RelativeTruePathChangeCount++;
		}
	}

	private void ToggleBodyRelativeTrueTrajectory(bool toggleValue)
	{
		if (toggleValue)
		{
			if (!VizardGUISettings.UseSpacecraftParentBodyForRelativeTraj&&(VizardGUISettings.RelativeBodyIndex == -1))
			{
				celestialBodyRelativeSelectionPanel.SetActive(true);
			}
			else
			{
				VizardGUISettings.TruePathLineMode = 3;
				VizardGUISettings.TruePathLinesVisible = true;
				truePathLinesToggle.isOn = true;
			}

			VizardGUISettings.RelativeTruePathChangeCount++;
		}
		else
		{
			celestialBodyRelativeSelectionPanel.SetActive(false);
			if (!truePathToggleGroup.AnyTogglesOn())
			{
				VizardGUISettings.TruePathLineMode = 1;
			}
		}
	}

	private void ToggleRotatingFrameTrueTrajectory(bool toggleValue)
	{
		if (toggleValue)
		{
			if ((VizardGUISettings.RotatingFrameBody1Index != VizardGUISettings.RotatingFrameBody2Index)&&(VizardGUISettings.RotatingFrameBody1Index !=-1)&&(VizardGUISettings.RotatingFrameBody2Index !=-1))
			{
				VizardGUISettings.TruePathLineMode = 4;
				VizardGUISettings.TruePathLinesVisible = true;
				truePathLinesToggle.isOn = true;
			}
			else
			{
				if (CelestialBodyStateUtilities.CelestialBodiesList.Count == 2)
				{
					float mu0 = (float) CelestialBodyStateUtilities.GetMu(CelestialBodyStateUtilities.CelestialBodiesList[0].name);
					float mu1 = (float) CelestialBodyStateUtilities.GetMu(CelestialBodyStateUtilities.CelestialBodiesList[1].name);
					if (mu0 < mu1)
					{
						VizardGUISettings.RotatingFrameBody1Index = 1;
						VizardGUISettings.RotatingFrameBody2Index = 0;
					}
					else
					{
						VizardGUISettings.RotatingFrameBody1Index = 0;
						VizardGUISettings.RotatingFrameBody2Index = 1;
					}

					CelestialBodyStateUtilities.CalculateRotatingFramePositionAndVelocityHistories();
					VizardGUISettings.TruePathLineMode = 4;
					VizardGUISettings.TruePathLinesVisible = true;
					truePathLinesToggle.isOn = true;
				}
				else
				{
					rotatingFrameBodySelectionPanel.SetActive(true);
				}
			}
			
			VizardGUISettings.RelativeTruePathChangeCount++;
		}
		else
		{
			rotatingFrameBodySelectionPanel.SetActive(false);
			if (!truePathToggleGroup.AnyTogglesOn())
			{
				VizardGUISettings.TruePathLineMode = 1;
			}
		}
	}
	
	private void ToggleBodyFixedFrameTrueTrajectory(bool toggleValue)
	{
		if (toggleValue)
		{
			VizardGUISettings.TruePathLineMode = 5;
			VizardGUISettings.TruePathLinesVisible = true;				
			truePathLinesToggle.isOn = true;
			VizardGUISettings.RelativeTruePathChangeCount++;
		}
		else
		{
			fixedFrameBodySelectionPanel.SetActive(false);
			if (!truePathToggleGroup.AnyTogglesOn())
			{
				VizardGUISettings.TruePathLineMode = 1;
			}
		}
	}

	private void ToggleCelestialBodyRelativeSelectionPanel()
	{
		celestialBodyRelativeSelectionPanel.SetActive(!celestialBodyRelativeSelectionPanel.activeSelf);
	}

	private void ToggleRotatingFrameBodiesSelectionPanel()
	{
		rotatingFrameBodySelectionPanel.SetActive(!rotatingFrameBodySelectionPanel.activeSelf);
	}

	private void ToggleFixedFrameBodySelectionPanel()
	{
		fixedFrameBodySelectionPanel.SetActive(!fixedFrameBodySelectionPanel.activeSelf);
	}
	
	public void ToggleHillFrameCS(bool toggleValue){
		VizardGUISettings.ShowHillFrame = toggleValue;
		GameObject cameraTarget = MainCameraUtilities.CameraTarget;
		if(cameraTarget.CompareTag("Spacecraft")){ //Effectors will not define the shown hill frame
			cameraTarget.GetComponent<SpacecraftController>().hillFrameCoordinateAxes.SetActive(toggleValue);
		}
	}

	public void ToggleVelFrameCS(bool toggleValue){
		VizardGUISettings.ShowVelocityFrame = toggleValue;
		GameObject cameraTarget = MainCameraUtilities.CameraTarget;
		if(cameraTarget.CompareTag("Spacecraft")){ //Effectors will not define the displayed velocity frame
			cameraTarget.GetComponent<SpacecraftController>().velocityFrameCoordinateAxes.SetActive(toggleValue);
		}
	}

	private void RefFrameCSTogglesActive(bool setActive){
		VizardGUISettings.SetToggleInteractable(setActive, showHillFrameToggle);
		VizardGUISettings.SetToggleInteractable(setActive, showVelFrameToggle);
	}

	private void ToggleOsculatingGroundTracks(bool isOn)
	{
		VizardGUISettings.OsculatingGroundTrackOn = isOn;
		foreach (GameObject line in SpacecraftStateUtilities.SpacecraftOrbitLinesList)
		{
			line.GetComponentInChildren<GroundTrackOsculating>().ToggleOsculatingGroundTrackLine(isOn);
		}
		
	}

	private void ToggleTruePathGroundTracks(bool isOn)
	{
		VizardGUISettings.TruePathGroundTrackOn = isOn;
		foreach (GameObject line in SpacecraftStateUtilities.SpacecraftOrbitLinesList)
		{
			line.GetComponentInChildren<GroundTrackTruePath>().ToggleTruePathGroundTrackLine(isOn);
		}
	}

	private void SetRelativeOrbitOptionsInteractable()
	{
		bool multipleSpacecraft = SpacecraftStateUtilities.ParentSpacecraftList.Count > 1;
		VizardGUISettings.SetToggleInteractable(multipleSpacecraft, oscRelativeOrbitsToggle);
		
		VizardGUISettings.SetToggleInteractable(multipleSpacecraft, trueSpacecraftRelativeOrbitsToggle);
		VizardGUISettings.SetButtonInteractable(multipleSpacecraft, setSpacecraftRelativeFrameButton, false, true);
		VizardGUISettings.SetButtonInteractable(multipleSpacecraft, setChiefSpacecraftPanelButton);
		
		VizardGUISettings.SetToggleInteractable(!SpacecraftStateUtilities.SpacecraftMsgOnly, trueBodyRelativeOrbitsToggle);
		VizardGUISettings.SetButtonInteractable(!SpacecraftStateUtilities.SpacecraftMsgOnly, setBodyRelativeBodyButton);

		bool multipleCelestialBodies = CelestialBodyStateUtilities.CelestialBodiesList.Count > 1;
		VizardGUISettings.SetToggleInteractable(multipleCelestialBodies, trueRotatingFramesToggle);
		VizardGUISettings.SetButtonInteractable(multipleCelestialBodies, setRotatingFrameBodiesButton);
		
		bool multipleBodies = multipleSpacecraft || !SpacecraftStateUtilities.SpacecraftMsgOnly;
		VizardGUISettings.SetToggleInteractable(multipleBodies, trueFixedFramesToggle);
		VizardGUISettings.SetButtonInteractable(multipleBodies, setFixedFrameBodyButton);
	}
	
	public void BroadcastSyncToggleMode(bool inForcedSync)
	{
		//TODO: Need to add in all the new toggles 
		VizardGUISettings.SetToggleInteractable(!inForcedSync, orbitLinesToggle);
		VizardGUISettings.SetToggleInteractable((!inForcedSync&&(MessageList.FirstMessage.Spacecraft.Count>1)), oscRelativeOrbitsToggle);
		
		VizardGUISettings.SetToggleInteractable(!inForcedSync, truePathLinesToggle);
		VizardGUISettings.SetToggleInteractable((!inForcedSync&&(MessageList.FirstMessage.Spacecraft.Count>1)), trueSpacecraftRelativeOrbitsToggle);
		VizardGUISettings.SetButtonInteractable((!inForcedSync&&(MessageList.FirstMessage.Spacecraft.Count>1)), setSpacecraftRelativeFrameButton, false, true);
		VizardGUISettings.SetButtonInteractable((!inForcedSync&&(MessageList.FirstMessage.Spacecraft.Count>1)),setChiefSpacecraftPanelButton);

		VizardGUISettings.SetToggleInteractable((!inForcedSync&&(MessageList.FirstMessage.CelestialBodies.Count>0)), trueBodyRelativeOrbitsToggle);
		VizardGUISettings.SetButtonInteractable((!inForcedSync&&(MessageList.FirstMessage.CelestialBodies.Count>0)),setBodyRelativeBodyButton);

		VizardGUISettings.SetToggleInteractable((!inForcedSync&&(MessageList.FirstMessage.CelestialBodies.Count>1)), trueRotatingFramesToggle);
		VizardGUISettings.SetButtonInteractable((!inForcedSync&&(MessageList.FirstMessage.CelestialBodies.Count>1)),setRotatingFrameBodiesButton);
		
		VizardGUISettings.SetToggleInteractable((!inForcedSync&&(MessageList.FirstMessage.CelestialBodies.Count>0||MessageList.FirstMessage.Spacecraft.Count>1)), trueFixedFramesToggle);
		VizardGUISettings.SetButtonInteractable((!inForcedSync&&(MessageList.FirstMessage.CelestialBodies.Count>0||MessageList.FirstMessage.Spacecraft.Count>1)), setFixedFrameBodyButton);

		VizardGUISettings.SetToggleInteractable(!inForcedSync, cameraTargetCSToggle);
		VizardGUISettings.SetToggleInteractable(!inForcedSync, allPlanetsCSToggle);
		VizardGUISettings.SetToggleInteractable(!inForcedSync, allSpacecraftCSToggle);
		
		VizardGUISettings.SetToggleInteractable(!inForcedSync, showHillFrameToggle);
		VizardGUISettings.SetToggleInteractable(!inForcedSync, showVelFrameToggle);
		
		if (!DataManager.UseVR)
		{
			forceSyncSettingsToggle.isOn = inForcedSync;
		}
	}


}
