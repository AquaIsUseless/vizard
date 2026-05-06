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
/// for a given group of Coarse Sun Sensors on a spacecraft
/// </summary>
public class CSSGroupPanelData : MonoBehaviour
{
	public int spacecraftIndex;
	public int groupID;
	private List<int> cssGroupList = new List<int>();
	private List<GameObject> signalBars = new List<GameObject>();
	private double maxOfAll;
	private List<double> expectedMax = new List<double>();
	private List<double> expectedMin = new List<double>();

	public GameObject expValueBar;
	public TextMeshProUGUI expMaxText;
	public TextMeshProUGUI expMinText;
	public TextMeshProUGUI groupTitle;
	public RectTransform zeroLine;
	public RectTransform maxLine;

	private readonly Color nomOrange = new(1f, 0.757f, 0.102f, 1f);
	private readonly Color alertOrange = new(1f, 0.453f, 0.102f, 1f);

	public void BuildSubpanel(int spacecraftID, List<int> cssIDs){
		cssGroupList =cssIDs;
		spacecraftIndex = spacecraftID;
		groupID = MessageList.FirstMessage.Spacecraft[spacecraftIndex].CSS[cssGroupList[0]].CSSGroupID;
		maxOfAll = 0;
		foreach(int cssID in cssGroupList){
			double currentMax = MessageList.FirstMessage.Spacecraft[spacecraftIndex].CSS[cssID].MaxMsmt;
			double currentMin = MessageList.FirstMessage.Spacecraft[spacecraftIndex].CSS[cssID].MinMsmt;
			if (currentMax > maxOfAll){
				maxOfAll = currentMax;
			}
			expectedMax.Add(currentMax);
			expectedMin.Add(currentMin);
		}

		int xPos = 2;

		int i = 0;
		foreach(int cssID in cssGroupList){
			GameObject cssUnit = Instantiate(Resources.Load("Prefabs/SpacecraftPanels/CSSPanelUnit") as GameObject, expMinText.transform.GetChild(0).transform, true);
			double currentMeasurement = MessageList.FirstMessage.Spacecraft[spacecraftIndex].CSS[cssID].CurrentMsmt;
			int mySize = UpdateBarSize(currentMeasurement);
			cssUnit.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(15, mySize);
			cssUnit.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (int)(35f*(float) (expectedMin[i]/maxOfAll)));
			cssUnit.transform.GetChild(2).GetComponent<RectTransform>().anchoredPosition = new Vector2(0, (int) (35f*(float) (expectedMax[i]/maxOfAll)));
			cssUnit.GetComponent<RectTransform>().anchoredPosition = new Vector2(xPos, 0);
			cssUnit.GetComponentInChildren<TextMeshProUGUI>().text = cssID.ToString();
			signalBars.Add(cssUnit);
			xPos+=17;
			i++;
		}

		groupTitle.text = $"Group {groupID}";
		expMaxText.text = maxOfAll.ToString("#.00");
		expMinText.text = "0.00";
	}

	void FixedUpdate(){
		int i = 0;
		foreach(int cssID in cssGroupList){
			double currentMeasurement = MessageList.FirstMessage.Spacecraft[spacecraftIndex].CSS[cssID].CurrentMsmt;
			if ((currentMeasurement >expectedMax[i])||(currentMeasurement < expectedMin[i])){
				signalBars[i].transform.GetChild(0).GetComponent<Image>().color = alertOrange;
			}else{
				signalBars[i].transform.GetChild(0).GetComponent<Image>().color = nomOrange;
			}
			int mySize = UpdateBarSize(currentMeasurement);

			signalBars[i].transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(15, mySize);
			signalBars[i].GetComponent<HoveringLabel>().labelText.text = currentMeasurement.ToString("#.000");
			i++;
		}
		
	}

	private int UpdateBarSize(double measurement){
		return (int) Mathf.Min(40f, (35f*(float)(measurement/maxOfAll)));
	}

	public void SetLinesWidth(int newWidth){
		zeroLine.sizeDelta = new Vector2(newWidth,1);
		maxLine.sizeDelta = new Vector2(newWidth,1);
	}


}
