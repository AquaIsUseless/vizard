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
using TMPro;
using VizProtobufferMessage;
/// <summary>
/// Handles applying trainer updates on user settings and panel inputs
/// to subscribed broadcast (receive-only) Vizard instances. Also triggers
/// fading status alert if trainer broadcast sync forcing is turned on or off. 
/// </summary>
public class BroadcastSyncSettingsManager : MonoBehaviour
{
	public ViewMenuMethods viewMenuMethods;

	public GameObject forceSyncSettingsAlert;
    // Start is called before the first frame update

	void FixedUpdate()
	{
		if (DataManager.SocketIsReceiveOnly)
		{
			if (MessageList.LatestBroadcastSyncSettings != null)
			{
				if (MessageList.LatestBroadcastSyncSettings.ForceTrainerSettings)
				{
					if (!VizInputUtilities.ForceBroadcastSyncSettings)
					{
						if (DataManager.UseVR)
						{
#if VIZARD_OPENXR
							MainCameraUtilities.MainCamera.GetComponent<VizardVR_MainCameraMovementController>().SetStatusPanelText("Live Settings Sync Resumed");
#endif
						}
						else{

							forceSyncSettingsAlert.SetActive(true);
							forceSyncSettingsAlert.GetComponentInChildren<TextMeshProUGUI>().text =
								"Live Settings Sync Resumed";
						}

						viewMenuMethods.BroadcastSyncToggleMode(true);
						VizInputUtilities.FirstSync = true;
					}
					VizInputUtilities.ForceBroadcastSyncSettings = true;
					ApplyBroadcastSyncSettings();
				}
				else
				{
					if (VizInputUtilities.ForceBroadcastSyncSettings)
					{

						if (DataManager.UseVR)
						{
#if VIZARD_OPENXR
							MainCameraUtilities.MainCamera.GetComponent<VizardVR_MainCameraMovementController>().SetStatusPanelText("Live Settings Sync Ended");
#endif
						}
						else
						{
							forceSyncSettingsAlert.SetActive(true);
							forceSyncSettingsAlert.GetComponentInChildren<TextMeshProUGUI>().text =
								"Live Settings Sync Ended";
						}

						viewMenuMethods.BroadcastSyncToggleMode(false);
						VizInputUtilities.eventDialogManager.ReleaseButtonsOnAllEventDialogs();
					}
					VizInputUtilities.ForceBroadcastSyncSettings = false;
				}
			}
		}
	}


	private void ApplyBroadcastSyncSettings()
	{
		VizBroadcastSyncSettings syncSettings = MessageList.LatestBroadcastSyncSettings;
		if (syncSettings.OrbitLinesOn >= 0)
		{
			VizardGUISettings.OsculatingOrbitLinesVisible = true;
			VizardGUISettings.SpacecraftRelativeOsculatingOrbits = (syncSettings.OrbitLinesOn == 2);
		}
		else
		{
			VizardGUISettings.OsculatingOrbitLinesVisible = false;
		}

		if (syncSettings.TrueTrajectoryLinesOn >= 1)
		{
			VizardGUISettings.TruePathLinesVisible = true;
			if (syncSettings.TrueTrajectoryLinesOn > 0)
			{
				VizardGUISettings.TruePathLineMode = syncSettings.TrueTrajectoryLinesOn;
				switch (VizardGUISettings.TruePathLineMode)
				{
					case 2: //Spacecraft relative
						VizardGUISettings.SetChiefToCamTgt = syncSettings.TruePathBodySetting[1] == -1;
						VizardGUISettings.ChiefSpacecraftIndex = VizardGUISettings.SetChiefToCamTgt
							? MainCameraUtilities.CameraTargetIndex
							: syncSettings.TruePathBodySetting[1];
						break;
					case 3: //celestial body relative
						VizardGUISettings.UseSpacecraftParentBodyForRelativeTraj=syncSettings.TruePathBodySetting[1] == -1;
						VizardGUISettings.RelativeBodyIndex = syncSettings.TruePathBodySetting[1];
						break;
					case 4: //rotating frame
						VizardGUISettings.RotatingFrameBody1Index = syncSettings.TruePathBodySetting[1];
						VizardGUISettings.RotatingFrameBody1Index = syncSettings.TruePathBodySetting[2];
						break;
					case 5: // fixed frame
						VizardGUISettings.FixedBodyIsSpacecraft=syncSettings.TruePathBodySetting[0] == 1;
						VizardGUISettings.UseSpacecraftParentBodyForFixedFrameTraj =
							syncSettings.TruePathBodySetting[1] == -1;
						VizardGUISettings.FixedBodyIndex = syncSettings.TruePathBodySetting[1];
						break;
				}
				VizardGUISettings.TruePathLinesVisible = true;
			}
			else
			{
				VizardGUISettings.TruePathLinesVisible = false;
			}
		}
		else
		{
			VizardGUISettings.TruePathLinesVisible = false;
		}
		foreach (GameObject line in SpacecraftStateUtilities.SpacecraftOrbitLinesList)
		{
			line.GetComponent<OsculatingOrbitLine>().ToggleTruePathLineGameObject(VizardGUISettings.TruePathLinesVisible);
		}

		if (DataManager.FirstMessageDisplayed)
		{
			viewMenuMethods.ToggleSpacecraftCS(syncSettings.SpacecraftCSon > 0);
			viewMenuMethods.TogglePlanetsCS(syncSettings.PlanetCSon > 0);
			viewMenuMethods.ToggleHillFrameCS(syncSettings.ShowHillFrame > 0);
			viewMenuMethods.ToggleVelFrameCS(syncSettings.ShowVelocityFrame > 0);
		}
	}
}
