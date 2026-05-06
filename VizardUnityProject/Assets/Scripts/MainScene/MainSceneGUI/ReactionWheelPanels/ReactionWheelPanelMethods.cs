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
/// Sets up and updates a spacecraft's reaction wheel display panel
/// </summary>
public class ReactionWheelPanelMethods : MonoBehaviour {
	public int spacecraftIndex;
	private GameObject myPanel;
	private Button verboseButton;
	private GameObject verbosePanel;
	private bool verboseOn;
	private int wheelCount;

	private float ypos = -35;
	private float xpos = 30;


	private List<GameObject> speedBarsList = new List<GameObject> ();
	private List<GameObject> torqueBarsList = new List<GameObject> ();
	private List<GameObject> speedLabelsList = new List<GameObject>();
	private List<GameObject> torqueLabelsList = new List<GameObject>();
	

	public void InitializePanel (GameObject parentPanel, int spacecraftID, string spacecraftName){
		spacecraftIndex = spacecraftID;
		myPanel = parentPanel;
		string parentName = MessageList.FirstMessage.Spacecraft[spacecraftIndex].ParentSpacecraftName;
		if (parentName == "")
		{
			myPanel.name = spacecraftName + " Reaction Wheels Panel";
			myPanel.transform.GetChild (0).GetComponent<TextMeshProUGUI> ().text = spacecraftName+" RW";
		}
		else
		{
			myPanel.name = parentName + "/" + spacecraftName + " Reaction Wheels Panel";
			myPanel.transform.GetChild (0).GetComponent<TextMeshProUGUI> ().text = parentName + "/" + spacecraftName+" RW";
		}

		wheelCount = MessageList.FirstMessage.Spacecraft[spacecraftIndex].ReactionWheels.Count;

		Color blueColor = new Vector4 (0.32f, 0.73f, 0.93f, 1f);
		Color yellowColor = new Vector4(0.93f,0.84f,0.32f,1f);

		myPanel.GetComponent<RectTransform> ().sizeDelta = new Vector2 ((42*wheelCount+66), 170);
		UpdateScales ();
		myPanel.transform.GetChild (5).GetComponent<RectTransform> ().anchoredPosition = new Vector2 (42 * wheelCount + 30, -25);
		myPanel.transform.GetChild (6).GetComponent<RectTransform> ().anchoredPosition = new Vector2 (42 * wheelCount + 30, -115);
		myPanel.transform.GetChild (9).GetComponent<RectTransform> ().anchoredPosition = new Vector2 (42 * wheelCount + 27, -105);
		myPanel.transform.GetChild (7).GetComponent<RectTransform> ().sizeDelta = new Vector2 (42 * wheelCount+5, 1);

		verboseButton = myPanel.transform.GetChild (10).GetComponent<Button> ();
		verboseButton.onClick.AddListener (ToggleVerboseMode);
		verbosePanel = myPanel.transform.GetChild (11).gameObject;
		verbosePanel.GetComponent<RectTransform> ().sizeDelta = new Vector2 ((42*wheelCount+66), 40);

		for (int wheel=0; wheel<wheelCount ; wheel++){
			//Add the reaction wheel speed bar
			GameObject speedBar = Instantiate(Resources.Load("Prefabs/GUIGenerics/BarDisplay")as GameObject, myPanel.transform, true);
			speedBar.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (xpos, ypos);
			speedBar.name = $"Speed RW{wheel}";
			speedBar.GetComponent<BarDisplayMethods> ().ChangeMaxValue (ReactionWheelUtilities.MaxSpeed[spacecraftIndex]);
			speedBar.transform.GetChild (1).transform.GetChild (0).GetComponent<Image> ().color = blueColor;

			//Add the label for the speed/torque bar pair
			GameObject rwLabel = Instantiate(Resources.Load("Prefabs/GUIGenerics/GUILabel") as GameObject, myPanel.transform, true);
			rwLabel.name = $"RW{wheel}";
			rwLabel.GetComponent<TextMeshProUGUI>().text = $"RW{wheel}";
			rwLabel.GetComponent<TextMeshProUGUI> ().alignment = TextAlignmentOptions.MidlineLeft;
			rwLabel.GetComponent<RectTransform> ().anchoredPosition = new Vector2 ((xpos+5), -130);

			//Add the reaction wheel's speed verbose label
			GameObject speedLabel = Instantiate(Resources.Load("Prefabs/GUIGenerics/GUILabel") as GameObject, verbosePanel.transform, true);
			speedLabel.name = $"Speed Verbose RW{wheel}";
			speedLabel.GetComponent<TextMeshProUGUI> ().alignment = TextAlignmentOptions.MidlineLeft;
			speedLabel.GetComponent<TextMeshProUGUI> ().fontSize = 9;
			speedLabel.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (xpos+7, 0);

			//Add the reaction wheel's torque verbose label
			GameObject torqueLabel = Instantiate(Resources.Load("Prefabs/GUIGenerics/GUILabel") as GameObject, verbosePanel.transform, true);
			torqueLabel.name = $"Torque Verbose RW{wheel}";
			torqueLabel.GetComponent<TextMeshProUGUI> ().alignment = TextAlignmentOptions.MidlineLeft;
			torqueLabel.GetComponent<TextMeshProUGUI> ().fontSize = 9;
			torqueLabel.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (xpos+7, -15);
			
			xpos += 15;
			//Add the reaction wheel torque bar
			GameObject torqueBar = Instantiate(Resources.Load("Prefabs/GUIGenerics/BarDisplay") as GameObject, myPanel.transform, true);
			torqueBar.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (xpos, ypos);
			torqueBar.name = $"Torque RW{wheel}";
			torqueBar.GetComponent<BarDisplayMethods> ().ChangeMaxValue (ReactionWheelUtilities.MaxTorque[spacecraftIndex]);
			torqueBar.transform.GetChild (1).transform.GetChild (0).GetComponent<Image> ().color = yellowColor;
			
			xpos += 27;
			speedBarsList.Add (speedBar);
			torqueBarsList.Add (torqueBar);
			torqueLabelsList.Add (torqueLabel);
			speedLabelsList.Add (speedLabel);
		}
			
		verbosePanel.SetActive (false);

	}

	private void UpdateScales(){
		if (ReactionWheelUtilities.MaxSpeed[spacecraftIndex]<1){
			myPanel.transform.GetChild (3).GetComponent<TextMeshProUGUI> ().text = ReactionWheelUtilities.MaxSpeed[spacecraftIndex].ToString("F1");//("#.");
			myPanel.transform.GetChild (4).GetComponent<TextMeshProUGUI> ().text = "-" +ReactionWheelUtilities.MaxSpeed[spacecraftIndex].ToString("F1"); //("#.");
		}else{
			myPanel.transform.GetChild (3).GetComponent<TextMeshProUGUI> ().text = ReactionWheelUtilities.MaxSpeed[spacecraftIndex].ToString("F1");
			myPanel.transform.GetChild (4).GetComponent<TextMeshProUGUI> ().text = "-" +ReactionWheelUtilities.MaxSpeed[spacecraftIndex].ToString("F1"); 
		}
		myPanel.transform.GetChild (5).GetComponent<TextMeshProUGUI> ().text = ReactionWheelUtilities.MaxTorque[spacecraftIndex].ToString("F1");
		myPanel.transform.GetChild (6).GetComponent<TextMeshProUGUI> ().text = "-" + ReactionWheelUtilities.MaxTorque[spacecraftIndex].ToString("F1");
	}

	// Update is called once per frame
	void Update () {

		double[] updatedSpeeds = ReactionWheelUtilities.GetReactionWheelSpeeds(spacecraftIndex);
		double[] updatedTorques = ReactionWheelUtilities.GetReactionWheelTorques (spacecraftIndex);

		for (int wheel = 0; wheel < wheelCount; wheel++) {
			if (ReactionWheelUtilities.MaxSpeedChange) {
				UpdateMaxSpeed ((float)ReactionWheelUtilities.MaxSpeed[spacecraftIndex]);
				ReactionWheelUtilities.MaxSpeedChange = false;
			}
				
			if (ReactionWheelUtilities.MaxTorqueChange) {
				UpdateMaxTorque((float)ReactionWheelUtilities.MaxTorque[spacecraftIndex]);
				ReactionWheelUtilities.MaxTorqueChange = false;
			}
			speedBarsList [wheel].GetComponent<BarDisplayMethods> ().currentValue = (float)updatedSpeeds[wheel];
			torqueBarsList [wheel].GetComponent<BarDisplayMethods> ().currentValue = (float)updatedTorques [wheel];

			if (verboseOn) {
				speedLabelsList [wheel].GetComponent<TextMeshProUGUI> ().text = ((float)updatedSpeeds[wheel]).ToString ("F1");
				torqueLabelsList [wheel].GetComponent<TextMeshProUGUI> ().text = ((float)updatedTorques[wheel]).ToString ("F1");
			}
		}
	}

	private void UpdateMaxSpeed(float newSpeed){
		for (int wheel = 0; wheel < wheelCount; wheel++) {
			speedBarsList [wheel].GetComponent<BarDisplayMethods> ().ChangeMaxValue(newSpeed);
		}
		UpdateScales ();
	}

	private void UpdateMaxTorque(float newTorque){
		for (int wheel = 0; wheel < wheelCount; wheel++) {
			torqueBarsList [wheel].GetComponent<BarDisplayMethods> ().ChangeMaxValue(newTorque);
		}
		UpdateScales ();
	}

	private void ToggleVerboseMode(){
		verboseOn = !verboseOn;

		if (verboseOn) {
			myPanel.GetComponent<RectTransform> ().sizeDelta = new Vector2 ((42*wheelCount+66), 205);
			verbosePanel.SetActive (true);

		} else{
			verbosePanel.SetActive (false);
			myPanel.GetComponent<RectTransform> ().sizeDelta = new Vector2 ((42*wheelCount+66), 170);

		}
			
	}
}
