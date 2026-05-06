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
using UnityEngine;
/// <summary>
/// Static class providing methods and object references for spacecraft thrusters instantiated for the current scenario.
/// </summary>
public static class ThrusterUtilities {
	private static Color defaultThrusterColor = Color.white;
	private static float particleLifeTimeScalar = 1f;

	public static Dictionary<string, List<int>> GetThrusterGroups(int spacecraftIndex){
		Dictionary<string, List<int>> finalThrusterGroupsForDisplay = new Dictionary<string, List<int>>();
		//First divide up the spacecraft's thrusters by the group ID and also by maxThrust
		for(int i = 0; i< MessageList.FirstMessage.Spacecraft[spacecraftIndex].Thrusters.Count;i++){
			string currentThrusterTag = MessageList.CurrentMessage.Spacecraft [spacecraftIndex].Thrusters [i].ThrusterTag;
			if ((currentThrusterTag == "")||(currentThrusterTag == null)){
				currentThrusterTag = "none";
			}
			double currentMaxThrust = MessageList.CurrentMessage.Spacecraft [spacecraftIndex].Thrusters [i].MaxThrust;
			//Sort into sets by thrusterTag and maxThrust
			string myGroupName = $"Spacecraft{spacecraftIndex}{currentThrusterTag}{currentMaxThrust}";
			if(finalThrusterGroupsForDisplay.ContainsKey(myGroupName)){
				List<int> thrustersInGroup = finalThrusterGroupsForDisplay[myGroupName];
				thrustersInGroup.Add(i);
				finalThrusterGroupsForDisplay[myGroupName] = thrustersInGroup;
			}else{
				List<int> thrustersInGroup = new List<int> {i};
				finalThrusterGroupsForDisplay[myGroupName] = thrustersInGroup;
			}
		}

		return finalThrusterGroupsForDisplay;
	}

	public static Color GetDefaultThrusterColor(){

		return defaultThrusterColor;

	}

	public static void SetDefaultThrusterColorSetting(Color newColor, bool setByUser){
		defaultThrusterColor = newColor;
		
		PersistentUserSettings.persistentSettingsFromLastSave.DefaultThrusterColor[0] = Mathf.RoundToInt(defaultThrusterColor.r * 255);
		PersistentUserSettings.persistentSettingsFromLastSave.DefaultThrusterColor[1] = Mathf.RoundToInt(defaultThrusterColor.g * 255);
		PersistentUserSettings.persistentSettingsFromLastSave.DefaultThrusterColor[2] = Mathf.RoundToInt(defaultThrusterColor.b * 255);
		PersistentUserSettings.persistentSettingsFromLastSave.DefaultThrusterColor[3] = Mathf.RoundToInt(defaultThrusterColor.a * 255);

		if (setByUser)
		{
			for (int i = 0; i < 4; i++)
			{
				PersistentUserSettings.currentSessionUserAppliedSettings.DefaultThrusterColor[i] =
					PersistentUserSettings.persistentSettingsFromLastSave.DefaultThrusterColor[i];
			}
		}

		UpdateDefaultColorForAllThrusters();
	}

	public static float GetParticleLifeUserSettingScalar(){
		return particleLifeTimeScalar;
	}

	public static void SetParticleLifeUserSettingScalar(float newValue){
		if (newValue > 0)
		{
			particleLifeTimeScalar = newValue;
		}
	}

	public static void ResetThrusterUtilities()
	{
		particleLifeTimeScalar = (float) PersistentUserSettings.persistentSettingsFromLastSave.DefaultThrusterPlumeLifeScalar;
	}

	private static void UpdateDefaultColorForAllThrusters()
	{
		foreach (GameObject spacecraft in SpacecraftStateUtilities.SpacecraftList)
		{
			GameObject thrusterGroup = spacecraft.GetComponent<SpacecraftController>().GetHUDContainer("Thrusters");
			if (thrusterGroup != null)
			{
				ThrusterHUDMethods[] allThrusters = thrusterGroup.GetComponentsInChildren<ThrusterHUDMethods>();
				foreach (ThrusterHUDMethods thruster in allThrusters)
				{
					thruster.UpdateDefaultThrusterColor(defaultThrusterColor);
				}
			}
		}
	}
}
