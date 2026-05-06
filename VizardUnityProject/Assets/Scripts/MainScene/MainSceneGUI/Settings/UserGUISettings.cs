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
using System;
using UnityEngine;
using UnityEngine.UI;
using VizProtobufferMessage;
/// <summary>
/// Handles implementing settings sent in VizMessage.Settings (first message)
/// and VizMessage.LiveSettings (every message).
/// </summary>
public class UserGUISettings : MonoBehaviour
{
	[Header("GUI Panel References")]
	public PanelViewManager panelViewManager;
	public AddPointingVectorPanelMethods addPointingVector;
	public AddKeepOutInConePanelMethods addKeepOutInCone;
	public ModelDirectoryPanelMethods addCustomModel;
	public SkyboxDropdown skyBoxDropdown;
	public ChangeChiefSpacecraftGUIMethods chiefSettings;
	
	private string lastChiefName;
	private int lastIndexApplied;

	public void ApplyUserSettings()
	{
		ApplyMessageGUISettings(PersistentUserSettings.persistentSettingsFromLastSave, false);
		VizMessage.Types.VizSettingsPb msgSettings = MessageList.FirstMessage.Settings;
		if (msgSettings!=null)
		{
			ApplyMessageGUISettings(msgSettings, true);
		}
		else
		{
			Debug.Log($"Message settings is null.");
		}
	}

	public void ResetUserSettings()
	{
		PersistentUserSettings.RestoreVizDefaultSettings();
		ApplyMessageGUISettings(PersistentUserSettings.persistentSettingsFromLastSave, false);
	}

	private void ApplyMessageGUISettings(VizMessage.Types.VizSettingsPb mySettings, bool currentScenarioSettings){
			ApplyLineRendererSettings(mySettings); //Want this to be set before Coordinate Frames are toggled and any target lines or pointing vectors are created
			ApplyOrbitLinesAndCSSettings(mySettings);
			//applyGroundTrackLineSettings(mySettings); //Applied in SimDataManager
			ApplyAmbientSetting(mySettings);
			ApplySpacecraftShadowBrightnessSetting(mySettings);
			ApplyMainLightSettings(mySettings);
			ApplyGUIResolutionSetting(mySettings);
			ApplyGUIResolutionSetting(mySettings);
			if (currentScenarioSettings)
			{
				CreateUserRequestedPointLines(mySettings);
				CreateUserRequestedKeepOutCones(mySettings);
				ImportCustomModelsAndApply(mySettings);
			}

			ApplyCameraSettings(mySettings);
			ApplySkyboxSetting(mySettings);
			ApplyThrusterDefaultSetting(mySettings);
			ApplySpriteSettings(mySettings);
			ApplyMainCameraSettings(mySettings);
			ApplyDefaultScaleSettings(mySettings);

		VizMessage.Types.LiveVizSettingsPb liveSettings = MessageList.FirstMessage.LiveSettings;
		if (liveSettings != null)
		{
			ApplyRelativeOrbitChiefSetting(liveSettings);
			ApplyPlaybackControlLiveSettings(liveSettings);
			if (liveSettings.TerminateVizard)
			{
				Application.Quit();
			}
		}
#if VIZARD_OPENXR
		// if (DataManager.useVR)
		// {
		// 	ApplyVRDemoSettings(mySettings);
		// }
#endif
		DataManager.FirstMessageDisplayed = true;
		//Note: actuator HUD settings are applied in each spacecraft's SpacecraftPositionHandler script
		//and actuator panel settings are applied in PanelViewManager when building their panels
		//Time Settings are handled by ItsAboutTime script
	}

	void FixedUpdate(){
		VizMessage.Types.LiveVizSettingsPb mySettings = MessageList.CurrentMessage.LiveSettings;
		if (lastIndexApplied != MessageList.CurrentIndex)
		{
			if (mySettings != null)
			{
				ApplyRelativeOrbitChiefSetting(mySettings);
				ApplyPlaybackControlLiveSettings(mySettings);
				if (mySettings.TerminateVizard)
				{
					Application.Quit();
				}
			}

			lastIndexApplied = MessageList.CurrentIndex;
		}
	}

	private void ApplyAmbientSetting(VizMessage.Types.VizSettingsPb mySettings){

			if (mySettings.Ambient >= 0)
			{
				float settingToUse = (float) mySettings.Ambient;
				RenderSettings.ambientIntensity = Mathf.Clamp(settingToUse, 0, 1); //Max possible ambient value is 8. 
			}
	}

	private void ApplySpacecraftShadowBrightnessSetting(VizMessage.Types.VizSettingsPb mySettings){

			if (mySettings.SpacecraftShadowBrightness is >= 0 and <= 1){
				PersistentUserSettings.SetSpacecraftShaderEmissive((float) mySettings.SpacecraftShadowBrightness, false);//Max possible ambient value is 8. 
			}
	}

	private static void ApplyMainLightSettings(VizMessage.Types.VizSettingsPb mySettings)
	{
		if (mySettings.AttenuateSunLightWithDistance != 0)
		{
			if (mySettings.AttenuateSunLightWithDistance == 1)
				PersistentUserSettings.persistentSettingsFromLastSave.AttenuateSunLightWithDistance = 1;
			else if (mySettings.AttenuateSunLightWithDistance == -1)
				PersistentUserSettings.persistentSettingsFromLastSave.AttenuateSunLightWithDistance = -1;
		}
		PersistentUserSettings.SetSunOrMainLightIntensity((float) mySettings.SunIntensity > 0 ? (float) mySettings.SunIntensity : (float) PersistentUserSettings.persistentSettingsFromLastSave.SunIntensity, false);
	}

	private void ApplyGUIResolutionSetting(VizMessage.Types.VizSettingsPb mySettings)
	{
		Vector2 oldReferenceResolution = GetComponent<CanvasScaler>().referenceResolution;
		double newHeight = mySettings.CustomGUIReferenceHeight>300?mySettings.CustomGUIReferenceHeight:oldReferenceResolution.y;
		
		GetComponent<CanvasScaler>().referenceResolution = new Vector2(oldReferenceResolution.x, (float) newHeight);
	}

	public void ApplyOrbitLinesAndCSSettings(VizMessage.Types.VizSettingsPb mySettings)
	{
			if (mySettings.OrbitLinesOn == -1){
				VizardGUISettings.OsculatingOrbitLinesVisible = false;
			}else if (mySettings.OrbitLinesOn == 0)
			{
				VizardGUISettings.OsculatingOrbitLinesVisible = !DataManager.InNoDisplayMode;
			}else if(mySettings.OrbitLinesOn ==1){
				VizardGUISettings.OsculatingOrbitLinesVisible = true;
			}else if (mySettings.OrbitLinesOn == 2){
				VizardGUISettings.OsculatingOrbitLinesVisible = true;
				if (SpacecraftStateUtilities.SpacecraftList.Count>1){
					VizardGUISettings.SpacecraftRelativeOsculatingOrbits = true;
				}
			}
			
			PersistentUserSettings.persistentSettingsFromLastSave.SpacecraftOrbitLineWidth = (mySettings.SpacecraftOrbitLineWidth > 0)?(float) mySettings.SpacecraftOrbitLineWidth: (float) PersistentUserSettings.persistentSettingsFromLastSave.SpacecraftOrbitLineWidth ;
			PersistentUserSettings.persistentSettingsFromLastSave.CelestialBodyOrbitLineWidth = (mySettings.CelestialBodyOrbitLineWidth > 0)?(float) mySettings.CelestialBodyOrbitLineWidth:PersistentUserSettings.persistentSettingsFromLastSave.CelestialBodyOrbitLineWidth;
			SpacecraftStateUtilities.UpdateSpacecraftOrbitLineWidth();
			CelestialBodyStateUtilities.UpdateCelestialBodyOrbitLineWidth();

			VizardGUISettings.AllSpacecraftCSOn = mySettings.SpacecraftCSon == 1;
			VizardGUISettings.AllPlanetCSOn = mySettings.PlanetCSon == 1;
			VizardGUISettings.ShowHillFrame = mySettings.ShowHillFrame == 1;
			VizardGUISettings.ShowVelocityFrame = mySettings.ShowVelocityFrame == 1;
			
			PersistentUserSettings.SetOrbitLineSegmentsPer360(((mySettings.OrbitLineSegments >= 4)
				? mySettings.OrbitLineSegments
				: PersistentUserSettings.persistentSettingsFromLastSave.OrbitLineSegments),false );
			
			if (mySettings.OsculatingOrbitLineRange.Count >= 2)
			{
				if (mySettings.OsculatingOrbitLineRange[0] < mySettings.OsculatingOrbitLineRange[1])
				{
					PersistentUserSettings.SetOsculatingOrbitDegreeRange(mySettings.OsculatingOrbitLineRange[0],mySettings.OsculatingOrbitLineRange[1], false);
				}
			}
			else
			{
				if (mySettings.RelativeOrbitRange > 0)
				{
					PersistentUserSettings.SetOsculatingOrbitDegreeRange(-mySettings.RelativeOrbitRange,
						mySettings.RelativeOrbitRange, false);
				}
			}

			if (mySettings.RelativeOrbitFrame is < 3 and >= 1)
			{
				VizardGUISettings.SpacecraftRelativeOrbitMode = mySettings.RelativeOrbitFrame;
			}

			if (CelestialBodyStateUtilities.CelestialBodiesList.Count > 0)
			{
				VizardGUISettings.TruePathLineMode =
					mySettings.TrueTrajectoryLinesOn > 0 ? mySettings.TrueTrajectoryLinesOn : 1;
				VizardGUISettings.TruePathLinesVisible = mySettings.TrueTrajectoryLinesOn > 0;

				if (mySettings.TruePathRelativeBody.Length > 0)
				{
					bool matchedBody = false;
					foreach (int key in CelestialBodyStateUtilities.IndexToBodyDictionary.Keys)
					{
						if (CelestialBodyStateUtilities.IndexToBodyDictionary[key] == mySettings.TruePathRelativeBody)
						{
							VizardGUISettings.RelativeBodyIndex = key;
							VizardGUISettings.UseSpacecraftParentBodyForRelativeTraj = false;
							matchedBody = true;
							break;
						}
					}

					if (!matchedBody)
					{
						VizardGUISettings.UpdateErrorMessages(
							$"TruePathPrimaryBody setting of {mySettings.TruePathRelativeBody} could not be matched to body in scene.",
							true);
						if ((VizardGUISettings.TruePathLineMode == 3) && (SpacecraftStateUtilities.SpacecraftMsgOnly))
						{
							VizardGUISettings.TruePathLineMode =
								1; //If there is no celestial body to be the parent body, don't let the mode be set to 3
						}
					}
				}

				if (mySettings.TruePathRotatingFrame.Length > 0)
				{
					string[] rotatingFrameBodiesPrelim = mySettings.TruePathRotatingFrame.Split(new char[] {' ', ','});
					string[] rotatingFrameBodies = new string[2];
					int j = 0;
					foreach (var bodyName in rotatingFrameBodiesPrelim)
					{
						if (bodyName != String.Empty)
						{
							rotatingFrameBodies[j] = bodyName;
							j++;
						}
					}

					int matchedBodies = 0;
					if (rotatingFrameBodies.Length >= 2)
					{
						foreach (int key in CelestialBodyStateUtilities.IndexToBodyDictionary.Keys)
						{
							if (CelestialBodyStateUtilities.IndexToBodyDictionary[key] == rotatingFrameBodies[0])
							{
								VizardGUISettings.RotatingFrameBody1Index = key;
								matchedBodies++;
							}

							if ((rotatingFrameBodies[1] == " ") && (rotatingFrameBodies.Length > 2))
							{
								if (CelestialBodyStateUtilities.IndexToBodyDictionary[key] == rotatingFrameBodies[2])
								{
									VizardGUISettings.RotatingFrameBody2Index = key;
									matchedBodies++;
								}
							}
							else
							{
								if (CelestialBodyStateUtilities.IndexToBodyDictionary[key] == rotatingFrameBodies[1])
								{
									VizardGUISettings.RotatingFrameBody2Index = key;
									matchedBodies++;
								}
							}

							if (matchedBodies == 2)
							{
								CelestialBodyStateUtilities.CalculateRotatingFramePositionAndVelocityHistories();
								break;
							}
						}
					}

					bool overrideModeSetting = false;
					if (matchedBodies != 2)
					{
						VizardGUISettings.UpdateErrorMessages(
							$"TruePathRotatingFrame setting of \"{mySettings.TruePathRotatingFrame}\" could not be parsed and matched to bodies in scene.",
							true);
						overrideModeSetting = true;
					}
					else
					{
						if (VizardGUISettings.RotatingFrameBody1Index == VizardGUISettings.RotatingFrameBody2Index)
						{
							VizardGUISettings.UpdateErrorMessages(
								$"TruePathRotatingFrame setting of \"{mySettings.TruePathRotatingFrame}\" is invalid as it would set both bodies of rotating frame to the same body",
								true);
							overrideModeSetting = true;
						}
					}

					if (overrideModeSetting && (VizardGUISettings.TruePathLineMode == 4))
					{
						VizardGUISettings.TruePathLineMode = 1;
					}
				}

				if (mySettings.TruePathFixedFrame.Length > 0)
				{
					bool matchedBody = false;
					foreach (int key in CelestialBodyStateUtilities.IndexToBodyDictionary.Keys)
					{
						if (CelestialBodyStateUtilities.IndexToBodyDictionary[key] == mySettings.TruePathFixedFrame)
						{
							VizardGUISettings.FixedBodyIndex = key;
							VizardGUISettings.FixedBodyIsSpacecraft = false;
							VizardGUISettings.UseSpacecraftParentBodyForFixedFrameTraj = false;
							matchedBody = true;
							break;
						}
					}

					if (!matchedBody)
					{
						foreach (GameObject sc in SpacecraftStateUtilities.SpacecraftList)
						{
							if (sc.name == mySettings.TruePathFixedFrame)
							{
								VizardGUISettings.FixedBodyIndex =
									sc.GetComponent<SpacecraftController>().spacecraftIndex;
								VizardGUISettings.FixedBodyIsSpacecraft = true;
								VizardGUISettings.UseSpacecraftParentBodyForFixedFrameTraj = false;
								matchedBody = true;
								break;
							}
						}

						if (!matchedBody)
						{
							VizardGUISettings.UpdateErrorMessages(
								$"TruePathFixedFrame setting of {mySettings.TruePathFixedFrame} could not be matched to body in scene.",
								true);
							if (VizardGUISettings.TruePathLineMode == 5)
							{
								VizardGUISettings.TruePathLineMode = 1;
							}
						}
					}
				}
			}
	}

	public void ApplyGroundTrackLineSettings(VizMessage.Types.VizSettingsPb mySettings)
	{
		VizardGUISettings.OsculatingGroundTrackOn = (mySettings.ShowOsculatingGroundTrackLines == 1);
		VizardGUISettings.TruePathGroundTrackOn = (mySettings.ShowTruePathGroundTrackLines == 1);
		
		if (mySettings.OsculatingGroundTrackRange.Count >= 2)
		{
			int pastRange = mySettings.OsculatingGroundTrackRange[0];
			int futureRange = mySettings.OsculatingGroundTrackRange[1];
			if (pastRange < futureRange)
			{
				PersistentUserSettings.SetOsculatingGroundTrackDegreeRange(pastRange, futureRange, false);
			}
		}
	}

	private void CreateUserRequestedPointLines(VizMessage.Types.VizSettingsPb mySettings)
	{
		if (mySettings.PointLines != null)
		{
			foreach (VizMessage.Types.PointLine newLine in mySettings.PointLines)
			{
				Color lineColor = new Color(newLine.LineColor[0] / 255f, newLine.LineColor[1] / 255f,
					newLine.LineColor[2] / 255f, newLine.LineColor[3] / 255f);
				if (newLine.LineColor.Count >= 4)
				{
					lineColor.a = newLine.LineColor[3] / 255f;
				}
				addPointingVector.AddLineFromSettingsMessage(newLine.FromBodyName, newLine.ToBodyName, lineColor);
			}
		}
	}

	private void CreateUserRequestedKeepOutCones(VizMessage.Types.VizSettingsPb mySettings){
		if (mySettings.KeepOutInCones!= null){
			addKeepOutInCone.transform.gameObject.SetActive(true);
			addKeepOutInCone.transform.gameObject.SetActive(false);
			foreach(VizMessage.Types.KeepOutInCone newCone in mySettings.KeepOutInCones){
				addKeepOutInCone.CreateConeFromSettingsMessage(newCone);
			}
		}
	}

	private void ApplyCameraSettings(VizMessage.Types.VizSettingsPb mySettings){
		if (mySettings.StandardCameraSettings != null){
			panelViewManager.ConfigureStandardCameraPanelsToUserSettings(mySettings);
		}
	}


	private void ApplySkyboxSetting(VizMessage.Types.VizSettingsPb mySettings){
		string skyboxToUse = (mySettings.Skybox != "")?mySettings.Skybox:"NASA_SVS";
		skyBoxDropdown.ApplyUserSkyboxSettings(skyboxToUse);
	}

	private void ImportCustomModelsAndApply(VizMessage.Types.VizSettingsPb mySettings){
			if (mySettings.CustomModels != null){
				foreach(VizMessage.Types.CustomModel newModelSettings in mySettings.CustomModels){
					addCustomModel.ApplyCustomModelMessageSettings(newModelSettings, true);
				}
			}
	}

	public static void ApplyLabelSettings(VizMessage.Types.VizSettingsPb mySettings){
		VizardGUISettings.ShowSpacecraftLabels = (mySettings.ShowSpacecraftLabels == 1);
		VizardGUISettings.ShowCSLabels = (mySettings.ShowCSLabels == 1);
		VizardGUISettings.ShowCelestialBodyLabels = (mySettings.ShowCelestialBodyLabels == 1);
		VizardGUISettings.ShowCameraLabels = (mySettings.ShowCameraLabels ==1);
		VizardGUISettings.ShowLocationLabels = (mySettings.ShowLocationLabels != -1);
		VizardGUISettings.ShowLightLabels = (mySettings.ShowLightLabels ==1);
	}

	private static void ApplyThrusterDefaultSetting(VizMessage.Types.VizSettingsPb mySettings)
	{
		if (mySettings.DefaultThrusterColor.Count >= 3)
		{
			Color myThrusterColor = new Color(mySettings.DefaultThrusterColor[0] / 255f,
				mySettings.DefaultThrusterColor[1] / 255f,
				mySettings.DefaultThrusterColor[2] / 255f, 1f);
			if (mySettings.DefaultThrusterColor.Count >= 4)
			{
				myThrusterColor.a = mySettings.DefaultThrusterColor[3] / 255f;
			}
			ThrusterUtilities.SetDefaultThrusterColorSetting(myThrusterColor, false);
		}

		float lifeScalar = (float) mySettings.DefaultThrusterPlumeLifeScalar;
		if ( lifeScalar> 0)
		{
			ThrusterUtilities.SetParticleLifeUserSettingScalar(lifeScalar);
		}
	}

	private static void ApplySpriteSettings(VizMessage.Types.VizSettingsPb mySettings){
		
			if (!String.IsNullOrEmpty(mySettings.DefaultSpacecraftSprite))
			{
				if (mySettings.DefaultSpacecraftSprite != "Circle 255 255 255 255")
				{
					Debug.Log($"I am seeing {mySettings.DefaultSpacecraftSprite}");
					PersistentUserSettings.SetDefaultSpacecraftSprite(mySettings.DefaultSpacecraftSprite, false);
				}
			}

			int value = mySettings.ShowSpacecraftAsSprites;
			if (value == -1)
			{
				VizardGUISettings.ShowSpritesForSpacecraft = (mySettings.ShowSpacecraftAsSprites!=-1);
			}
			else if (value == 1)
			{
				VizardGUISettings.ShowSpritesForSpacecraft = true;
			}
			else
			{
				//Use default settings of true if multiple spacecraft, false if single spacecraft
				VizardGUISettings.ShowSpritesForSpacecraft = SpacecraftStateUtilities.ParentSpacecraftList.Count > 1;
			}
			
			VizardGUISettings.ShowSpritesForPlanets = (mySettings.ShowCelestialBodiesAsSprites==1);
	}

	private static void ApplyMainCameraSettings(VizMessage.Types.VizSettingsPb mySettings){
			MainCameraUtilities.ApplyVizMessageKeyboardRateSettings((float) mySettings.KeyboardAngularRate, (float) mySettings.KeyboardZoomRate);
		if (!DataManager.UseVR)
		{
			MainCameraUtilities.ApplyVizMessageKeyboardRateSettings((float) mySettings.KeyboardAngularRate,
				(float) mySettings.KeyboardZoomRate);
		}

			MainCameraUtilities.ForceSpacecraftLocalView = (mySettings.ForceStartAtSpacecraftLocalView == 1);
		
			int userSetting = mySettings.ScViewToPlanetViewBoundaryMultiplier;
			if (userSetting is > 0 and <= 10){
				MainCameraUtilities.SpacecraftLocalTransitionBoundaryUnityUnits = userSetting*1000f;
			}
			else
			{
				MainCameraUtilities.SpacecraftLocalTransitionBoundaryUnityUnits = 5000f;
			}
		
			userSetting = mySettings.PlanetViewToHelioViewBoundaryMultiplier;
			if (userSetting is > 0 and <= 10)
			{
				MainCameraUtilities.PlanetLocalTransitionBoundaryUnityUnits = userSetting * 10000f;
			}
			else
			{
				MainCameraUtilities.PlanetLocalTransitionBoundaryUnityUnits = 20000f;
			}
	}

	private static void ApplyPlaybackControlLiveSettings(VizMessage.Types.LiveVizSettingsPb mySettings)
	{
		if (mySettings.PlaybackPaused)
		{
			VizardGUISettings.PlaybackManager.ApplyLiveSettingsPause();
		}
		
		if (mySettings.PlaybackInRealTime!=0)
		{
			VizardGUISettings.PlaybackManager.TogglePlaybackMode(mySettings.PlaybackInRealTime>0);
		}

		if (mySettings.PlaybackMultiplier != 0)
		{
			VizardGUISettings.PlaybackManager.SetPlaybackControlMultiplier(mySettings.PlaybackMultiplier);
		}
	}


	private void ApplyRelativeOrbitChiefSetting(VizMessage.Types.LiveVizSettingsPb mySettings){

			string chiefName = mySettings.RelativeOrbitChief;
			if (!string.IsNullOrEmpty(chiefName))
			{
				int chiefIndex = SpacecraftStateUtilities.GetSpacecraftIndex(chiefName);
				if(lastChiefName != chiefName){
					lastChiefName = chiefName;
					if ((chiefName == "Auto")||(chiefName == "AUTO")||(chiefName == "auto")){
						VizardGUISettings.SetChiefToCamTgt = true;
						chiefSettings.ChangeDropdownChoice();
						return;
					}
					for(int i =0; i<SpacecraftStateUtilities.SpacecraftList.Count;i++){
						if (i== chiefIndex){
							VizardGUISettings.SetChiefToCamTgt = false;
							VizardGUISettings.ChiefSpacecraftIndex = chiefIndex;
							chiefSettings.ChangeDropdownChoice();
							return;
						}
					}
					chiefSettings.SpacecraftNameNotFound(chiefName);
				}
			}
	}

	private static void ApplyDefaultScaleSettings(VizMessage.Types.VizSettingsPb mySettings){

		SpacecraftStateUtilities.DefaultLocalViewSpacecraftScale = (mySettings.SpacecraftSizeMultiplier>0) 
			? (float)mySettings.SpacecraftSizeMultiplier: SpacecraftStateUtilities.DefaultLocalViewSpacecraftScale;
		SpacecraftStateUtilities.DefaultHelioViewSpacecraftScale = (mySettings.SpacecraftHelioViewSizeMultiplier > 0)
			? (float) mySettings.SpacecraftHelioViewSizeMultiplier
			: SpacecraftStateUtilities.DefaultHelioViewSpacecraftScale;
		
		CelestialBodyStateUtilities.DefaultHelioPlanetScale = (mySettings.CelestialBodyHelioViewSizeMultiplier > 0)
			?(float) mySettings.CelestialBodyHelioViewSizeMultiplier:CelestialBodyStateUtilities.DefaultHelioPlanetScale;
	}

	private static void ApplyLineRendererSettings(VizMessage.Types.VizSettingsPb mySettings)
	{
		PersistentUserSettings.persistentSettingsFromLastSave.LinesAndFramesLineWidth =
			mySettings.LinesAndFramesLineWidth > 0
				? mySettings.LinesAndFramesLineWidth
				: PersistentUserSettings.persistentSettingsFromLastSave.LinesAndFramesLineWidth;

		PersistentUserSettings.persistentSettingsFromLastSave.UseLineRenderersForTargetLinesAndFrames =
			mySettings.UseLineRenderersForTargetLinesAndFrames != 0
				? mySettings.UseLineRenderersForTargetLinesAndFrames
				: PersistentUserSettings.persistentSettingsFromLastSave.UseLineRenderersForTargetLinesAndFrames;
	}

	public void ApplyAtmosphereSetting(VizMessage.Types.VizSettingsPb mySettings)
	{
		VizardGUISettings.UseAtmosphereShaderIfAvailable = mySettings.AtmospheresOff != 1;
	}

	private void ApplyVRDemoSettings(VizMessage.Types.VizSettingsPb settings)
	{
		if (SpacecraftStateUtilities.ParentSpacecraftList.Count > 1)
		{
			VizardGUISettings.TruePathLineMode = 2;
			VizardGUISettings.SpacecraftRelativeOsculatingOrbits = true;
		}
		else if ((settings!=null)&&(MessageList.FirstMessage.Settings.MainCameraTarget.ToLower()=="sun"))
		{
			VizardGUISettings.TruePathLineMode = 1;
		}
		else
		{
			VizardGUISettings.TruePathLineMode = 3;
		}

		VizardGUISettings.TruePathLinesVisible = true;
		VizardGUISettings.OsculatingOrbitLinesVisible = true;

		VizardGUISettings.ShowSpritesForSpacecraft = false; //Sprite mode in VR needs work.
	}
}
