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
using UnityEngine.UI;
/// <summary>
/// Sets up and updates the coarse sun sensor data display panel
/// for a given  spacecraft
/// </summary>
public class CSSPanelMethods : MonoBehaviour
{
	public int spacecraftIndex;
	private GameObject myPanel;
	public GameObject myToggle;
	private Button verboseButton;
	private GameObject verbosePanel;

	private Dictionary<int, List<int>> cssGroups;
	private Dictionary<int, double> maxExpectedValues;

	private int panelWidth;

	public void InitializePanel(GameObject panel, int spacecraftID, GameObject panelToggle){
		myToggle = panelToggle;
		spacecraftIndex = spacecraftID;
		myPanel = panel;
		string spacecraftName = MessageList.CurrentMessage.Spacecraft[spacecraftIndex].SpacecraftName;
		string parentSCName = MessageList.CurrentMessage.Spacecraft[spacecraftIndex].ParentSpacecraftName;
		myPanel.name = spacecraftName + "CSS Panel";
		cssGroups = CSSUtilities.GetCSSGroups(spacecraftIndex);
		
		string panelName = spacecraftName + " CSS";
		if (parentSCName != "")
		{
			panelName = parentSCName+"/"+panelName;
		}
		myPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = panelName;

		int cssGroupCount = cssGroups.Count;
		int maxCSSCount = 0;
		foreach(int groupID in cssGroups.Keys){
			if (cssGroups[groupID].Count>maxCSSCount){
				maxCSSCount = cssGroups[groupID].Count;
				int minPanelWidth = Mathf.Max(96, 25+ panelName.Length * 7);
				panelWidth = Mathf.Max(minPanelWidth, 17*maxCSSCount+62);
				if (panelWidth == minPanelWidth){
					myPanel.transform.GetChild(0).GetComponent<RectTransform>().offsetMax = new Vector2(0,0);
				}
			}
		}
			
		int ypos = -25;
		foreach(int groupID in cssGroups.Keys){
			GameObject cssGroupPanel = Instantiate(Resources.Load("Prefabs/SpacecraftPanels/CSSGroupPanel") as GameObject, panel.transform, true);
			cssGroupPanel.name = "CSS Group ID " + groupID.ToString();
			cssGroupPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(panelWidth,80);
			cssGroupPanel.GetComponent<CSSGroupPanelData>().BuildSubpanel(spacecraftIndex, cssGroups[groupID]);
			cssGroupPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, ypos);
			cssGroupPanel.GetComponent<CSSGroupPanelData>().SetLinesWidth(Mathf.Max(38, maxCSSCount*17+4));
			ypos-=80;
		}

		panel.GetComponent<RectTransform>().sizeDelta= new Vector2(panelWidth, 25+80*cssGroupCount);
	}
}
