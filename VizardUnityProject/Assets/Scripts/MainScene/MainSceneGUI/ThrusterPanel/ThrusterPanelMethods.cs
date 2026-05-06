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
/// Sets up and updates the thruster data display panel for a given spacecraft
/// </summary>
public class ThrusterPanelMethods : MonoBehaviour {
	public int spacecraftIndex;
	public GameObject myToggle;
	private GameObject myPanel;
	private int thrusterGroupCount;

	private int panelWidth = 210;

	private Dictionary<string, List<int>> thrusterGroups;

	private List<List<GameObject>> thrusterGroupsDisplaysList = new List<List<GameObject>> ();
	
	public void InitializePanel(GameObject panel, int spacecraftID, GameObject panelToggle){
		myToggle = panelToggle;
		spacecraftIndex = spacecraftID;
		string spacecraftName = MessageList.CurrentMessage.Spacecraft[spacecraftID].SpacecraftName;
		string parentName = MessageList.FirstMessage.Spacecraft[spacecraftIndex].ParentSpacecraftName;
		
		myPanel = panel;
		if (parentName == "")
		{
			myPanel.name = spacecraftName + " Thrusters Panel";
			myPanel.transform.GetChild (0).GetComponent<TextMeshProUGUI> ().text = spacecraftName+" Thrusters";
		}
		else
		{
			myPanel.name = parentName + "/" + spacecraftName + " Thrusters Panel";
			myPanel.transform.GetChild (0).GetComponent<TextMeshProUGUI> ().text = parentName + "/" + spacecraftName+" Thrusters";
		}
		
		thrusterGroups = ThrusterUtilities.GetThrusterGroups (spacecraftIndex);
		thrusterGroupCount = thrusterGroups.Count;
		
		int maxThrusterCount = 0;
		foreach(string groupName in thrusterGroups.Keys){
			if (thrusterGroups[groupName].Count>maxThrusterCount){
				maxThrusterCount = thrusterGroups[groupName].Count;
				panelWidth = Mathf.Max(155, 60 + maxThrusterCount * 63);
			}
		}
		int i = 0;
		foreach (string groupName in thrusterGroups.Keys) {
			List<int> thrusterGroupList = thrusterGroups[groupName];

			//Add a subpanel for each group of thrusters
			string thrusterTag = MessageList.FirstMessage.Spacecraft[spacecraftIndex].Thrusters[thrusterGroupList[0]].ThrusterTag;
			double thrusterGroupMaxThrust = MessageList.FirstMessage.Spacecraft[spacecraftIndex].Thrusters[thrusterGroupList[0]].MaxThrust;
			List<GameObject> currentThrusterDisplays = new List<GameObject> ();

			GameObject thrusterGroupPanel = Instantiate(Resources.Load("Prefabs/SpacecraftPanels/ThrusterGroupPanel") as GameObject, myPanel.transform, true);
			thrusterGroupPanel.name = thrusterTag != "none" ? $"Thruster Group Panel {thrusterTag}: {thrusterGroupMaxThrust}" : $"Thruster Group Panel: {thrusterGroupMaxThrust}";

			var thrustString = thrusterGroupMaxThrust.ToString(thrusterGroupMaxThrust>=1f ? "#.00" : "E1");

			if (thrusterTag == "none"){
				thrusterGroupPanel.transform.GetChild (0).GetComponent<TextMeshProUGUI> ().text = "  ";
				thrusterGroupPanel.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "(Max Thrust: "+thrustString+" N)";
			}else{
				thrusterGroupPanel.transform.GetChild (0).GetComponent<TextMeshProUGUI> ().text = "Thruster Group: " + thrusterTag;
				thrusterGroupPanel.transform.GetChild(6).GetComponent<TextMeshProUGUI>().text = "(Max Thrust: "+thrustString+" N)";
			}
			thrusterGroupPanel.transform.GetChild (1).GetComponent<TextMeshProUGUI> ().text = thrustString + " N";
			thrusterGroupPanel.GetComponent<RectTransform> ().sizeDelta = new Vector2 (panelWidth, 125);
			thrusterGroupPanel.transform.GetChild (3).GetComponent<RectTransform> ().sizeDelta = new Vector2 (maxThrusterCount * 63, 1);
		

			int xpos = 60;
			int ypos = 35;
			foreach (int t in thrusterGroupList)
			{
				//Create a thruster panel display subunit for each thruster in the current thruster group
				GameObject thisThrusterBar = Instantiate (Resources.Load ("Prefabs/SpacecraftPanels/ThrusterPanelUnit") as GameObject, thrusterGroupPanel.transform, true);
				thisThrusterBar.name =  "Thruster " + t.ToString() + " (Group " + thrusterTag + ")";
				//thisThrusterBar.GetComponent<RectTransform> ().sizeDelta = new Vector2 (60, 40);
				thisThrusterBar.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (xpos, ypos);
				thisThrusterBar.GetComponent<ThrusterPanelUnitData> ().spacecraftID = spacecraftID;
				thisThrusterBar.GetComponent<ThrusterPanelUnitData> ().thrusterTag = thrusterTag;
				thisThrusterBar.GetComponent<ThrusterPanelUnitData> ().thrusterID = t;
				thisThrusterBar.GetComponent<ThrusterPanelUnitData> ().maxThrust = thrusterGroupMaxThrust;
				//thisThrusterBar.GetComponent<ThrusterPanelUnitData> ().parentThrusterPanel = transform.GetComponent<ThrusterPanelMethods>();
				currentThrusterDisplays.Add (thisThrusterBar);
				xpos = xpos + 63;
			}

			thrusterGroupPanel.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, -25 - 125 * i);
			i++;
			thrusterGroupsDisplaysList.Add (currentThrusterDisplays);
		}


		//Add the elapsed time since thruster firing color key
		GameObject thrusterColorKey = Instantiate(Resources.Load("Prefabs/SpacecraftPanels/ThrusterColorKey") as GameObject, myPanel.transform, true);

		thrusterColorKey.GetComponent<RectTransform> ().pivot = new Vector2 (0, 0);
		thrusterColorKey.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, 0);
		if (panelWidth < 210) {
			thrusterColorKey.GetComponent<RectTransform> ().sizeDelta = new Vector2 (panelWidth, 90);
			thrusterColorKey.transform.GetChild (1).GetComponent<RectTransform> ().anchoredPosition = new Vector2 (5, -45);
			myPanel.GetComponent<RectTransform>().sizeDelta= new Vector2 (panelWidth, 115+(thrusterGroupCount * 125));
		} else {
			thrusterColorKey.GetComponent<RectTransform> ().sizeDelta = new Vector2 (panelWidth, 45);
			myPanel.GetComponent<RectTransform>().sizeDelta= new Vector2 (panelWidth, 70+(thrusterGroupCount * 125));
		}
	}
		
}
