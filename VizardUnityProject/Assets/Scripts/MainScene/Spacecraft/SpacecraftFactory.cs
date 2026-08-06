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
/// Builds scenario objects for all VizMessage.Spacecraft in current scenario
/// </summary>
public class SpacecraftFactory : MonoBehaviour
{
	void Awake ()
	{
		SpacecraftStateUtilities.SpacecraftList = new List<GameObject>();
		SpacecraftStateUtilities.ParentSpacecraftList = new List<GameObject>();
		SpacecraftStateUtilities.EffectorList = new List<GameObject>();
		SpacecraftStateUtilities.ParentAndEffectorDictionary = new Dictionary<string, List<int>>();
	}

	public void CreateAvailableSpacecraft(){
		if (MessageList.FirstMessage.CelestialBodies.Count == 0) {
			SpacecraftStateUtilities.SpacecraftMsgOnly = true;
			string errorString = "Only spacecraft messages present.";
			VizardGUISettings.UpdateErrorMessages(errorString);
		}
		//Create all spacecraft that have sim data available
		for (int i = 0; i< MessageList.FirstMessage.Spacecraft.Count; i++){
			string scName = MessageList.FirstMessage.Spacecraft[i].SpacecraftName;
			if (scName == "")
			{
				VizardGUISettings.UpdateErrorMessages($"Spacecraft[{i}] SpacecraftName field was not populated. Spacecraft will be named SC{i}.");
				scName = "SC" + i;
			}
			CreateSpacecraft (i, scName);

			//Have GUI Manager build any actuator/sensor panels there are messages for:
			VizardGUISettings.PanelViewMgr.AddActuatorPanels(SpacecraftStateUtilities.ActuatorsList, i, scName);
			VizardGUISettings.PanelViewMgr.AddInstrumentPanels (SpacecraftStateUtilities.InstrumentsList, i, scName);
		}
	}

	private void CreateSpacecraft(int spacecraftIndex, string scName)
	{
		// Create a new GameObject using the Spacecraft template
		GameObject spacecraft = Instantiate (Resources.Load ("Prefabs/basiliskSpacecraftTemplate") as GameObject, 
			DataManager.ScenarioObjectsContainer);

		spacecraft.name = scName;


        string parentSpacecraftName = spacecraft.GetComponent<SpacecraftController>().InitializeSpacecraft(spacecraftIndex);
		
		//Add the current spacecraft to the list of all spacecraft in the current viz
		SpacecraftStateUtilities.SpacecraftList.Add (spacecraft);

		//Create a trajectory line object for the spacecraft only if there are celestial body messages present
		//Do not create trajectory line object if the created spacecraft was not a parent spacecraft (i.e. was a solar panel)
		if (parentSpacecraftName==""){
			if (!SpacecraftStateUtilities.SpacecraftMsgOnly)
			{
				//Register the spacecraft with the MapManager to display its position on the map
                MapManager.Instance.RegisterSpacecraft(spacecraftIndex);


                // Create a new GameObject using the OrbitLine Template
                GameObject orbitLine =
					Instantiate(Resources.Load("Prefabs/OrbitLineTemplate") as GameObject, DataManager.ScenarioObjectsContainer);

				orbitLine.GetComponent<OsculatingOrbitLine>().InitializeOrbitLine(spacecraft, spacecraftIndex, true);
				orbitLine.name = scName + "OrbitLine";

				orbitLine.SetActive(true);
				spacecraft.GetComponent<SpacecraftController>().orbitLine = orbitLine;
				SpacecraftStateUtilities.SpacecraftOrbitLinesList.Add(orbitLine);
			}

			SpacecraftStateUtilities.ParentSpacecraftList.Add(spacecraft);
		}
		else
		{
			SpacecraftStateUtilities.EffectorList.Add(spacecraft);
			if (SpacecraftStateUtilities.ParentAndEffectorDictionary.ContainsKey(parentSpacecraftName))
			{
				List<int> effList = SpacecraftStateUtilities.ParentAndEffectorDictionary[parentSpacecraftName];
				effList.Add(spacecraftIndex);
				SpacecraftStateUtilities.ParentAndEffectorDictionary[parentSpacecraftName] = effList;

			}
			else
			{
				SpacecraftStateUtilities.ParentAndEffectorDictionary[parentSpacecraftName] =
					new List<int>() {spacecraftIndex};
			}
		}
	}
	
	public void UpdateAllSpacecraft(){
		foreach(GameObject sc in SpacecraftStateUtilities.SpacecraftList){
			sc.GetComponent<SpacecraftController>().UpdateSpacecraft();
		}
	}
	
}

