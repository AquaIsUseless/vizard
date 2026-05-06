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
using UnityEngine.UI;
using System;
using TMPro;
/// <summary>
/// Handles user input to the Add Pointing Vector panel,
/// creates new lines, maintains list of created lines,
/// and allows modification to current lines.
/// </summary>
public class AddPointingVectorPanelMethods : MonoBehaviour
{
	[Header("Panel GUI Components")]
	public Button panelOnButton;
	public TMP_Dropdown fromBodyDropdown;
	public TMP_Dropdown toBodyDropdown;
	public Button AddButton;
	public Button ColorButton;
	public TextMeshProUGUI errorText;
	
	[Header("Inventory Manager")]
	public InventoryPanelMethods inventoryPanelMethods;
	[Header("Color Wheel GUI Panel")]
	public GameObject colorWheelPanel;
	//public List<string> bodyList = new List<string> ();

	private List<GameObject> pointingVectorList = new List<GameObject> ();

	private List<GameObject> lineButtons = new List<GameObject> ();

	private string fromBodyEffectorParent;
	private string toBodyEffectorParent;

	private bool firstBuild = true;
	private GameObject openSubMenu;
	GameObject selectedButton;


	void Start ()
	{
		AddButton.onClick.AddListener (AddLineFromPanel);
		ColorButton.onClick.AddListener (ChooseLineColor);
		fromBodyDropdown.onValueChanged.AddListener(MainFromBodyDropdownValueSelected);
		toBodyDropdown.onValueChanged.AddListener(MainToBodyDropdownValueSelected);

		if ((SpacecraftStateUtilities.SpacecraftMsgOnly)&&(MessageList.FirstMessage.Spacecraft.Count<=1)){
			//Don't enable the use of the pointing vector panel if there's nothing to point to.
			panelOnButton.GetComponentInChildren<TextMeshProUGUI> ().color = new Color (0.5f, 0.5f, 0.5f);
			panelOnButton.interactable = false;
		} 
		ColorButton.interactable = true;
	}

	public void OnEnable ()
	{
		if (firstBuild)
		{
			UpdateBodyLists();
		}

		errorText.text = "";
		transform.SetAsLastSibling();
	}

	private void UpdateBodyLists(){
		VizardGUISettings.CreateBodyListForDropdown(fromBodyDropdown, "fromBody", true, true, true, false);
		VizardGUISettings.CreateBodyListForDropdown(toBodyDropdown, "toBody", true, true, true, false);
		firstBuild = false;
	}

	private GameObject CreateLineObject(string fromBody, string toBody, bool createdFromPanel = true){
		string parentBodyEffectorParent = "";
		string targetBodyEffectorParent = "";
		if (createdFromPanel)
		{
			parentBodyEffectorParent = fromBodyEffectorParent;
			targetBodyEffectorParent = toBodyEffectorParent;
		}
		GameObject parentBody = CelestialBodyStateUtilities.GetLineTargetGameObjectWithName(fromBody, parentBodyEffectorParent);
		if (parentBody== null) {
			string errorMsg = String.Format("Line from {0} to {1} can't be initialized because {0} is not in messages. ", fromBody, toBody);
			VizardGUISettings.UpdateErrorMessages((errorMsg), true);
			return null;
		}
		
		GameObject targetBody = CelestialBodyStateUtilities.GetLineTargetGameObjectWithName(toBody, targetBodyEffectorParent);
		if (targetBody ==null){
			string errorMsg = String.Format("Line from {0} to {1} can't be initialized because {1} is not in messages.", fromBody, toBody);
			VizardGUISettings.UpdateErrorMessages((errorMsg), true);
			return null;
		}

		if (!createdFromPanel)
		{
			if (parentBody.CompareTag("Spacecraft")&&parentBody.GetComponent<SpacecraftController>().isEffector)
			{
				parentBodyEffectorParent = parentBody.GetComponent<SpacecraftController>().parentSpacecraftName;
			}

			if (targetBody.CompareTag("Spacecraft")&&targetBody.GetComponent<SpacecraftController>().isEffector)
			{
				targetBodyEffectorParent = targetBody.GetComponent<SpacecraftController>().parentSpacecraftName;
			}
		}

		string nameString =
			$"{((parentBodyEffectorParent != "") ? (parentBodyEffectorParent + ": ") : "")}{fromBody} to {((targetBodyEffectorParent != "") ? (targetBodyEffectorParent + ": ") : "")}{toBody}";

		bool lineExists = false;
		GameObject buttonToReturn = null;
		if (lineButtons.Count > 0) {
			foreach (GameObject button in lineButtons) {
				if(button.name == nameString+"Button"){
					lineExists = true;
					errorText.text = $"Pointing vector {nameString} already exists.";
					buttonToReturn = button;
				}
			}
		}
		if (!lineExists)
		{
			GameObject newPointingVector = Instantiate (Resources.Load ("Prefabs/SpacecraftHUD/PointingVectorTemplate") as GameObject, parentBody.transform, true);
			newPointingVector.transform.localScale = Vector3.one;
			Color lineColor = GetDefaultStartingLineColor(toBody);
			newPointingVector.GetComponent<DrawPointingVector> ().InitializePointingVector (parentBody, targetBody, lineColor, PersistentUserSettings.persistentSettingsFromLastSave.UseLineRenderersForTargetLinesAndFrames==1);
			pointingVectorList.Add (newPointingVector);

			GameObject newPointingVectorButton = Instantiate (Resources.Load ("Prefabs/GUIGenerics/GenericButtonWithLabelAndImage") as GameObject);
			newPointingVectorButton.AddComponent<InventoryButton> ();
			newPointingVectorButton.GetComponent<InventoryButton> ().SetupButton (newPointingVector, newPointingVector, lineColor, transform.gameObject, nameString);
			inventoryPanelMethods.AddItemButtonToInventory(newPointingVectorButton);

			errorText.text = "";
			buttonToReturn = newPointingVectorButton;
		}
		return buttonToReturn;
	}

	private void AddLineFromPanel (){
		errorText.text = "";
		string fromBodyName = fromBodyDropdown.options[fromBodyDropdown.value].text;
		string toBodyName = toBodyDropdown.options[toBodyDropdown.value].text;

		if ((fromBodyName!= "Select Body")&&(toBodyName!="Select Body")&&(fromBodyName!=toBodyName)){
			lineButtons.Add(CreateLineObject(fromBodyName, toBodyName));
		} else {
			errorText.text = "Please select two distinct bodies to draw a pointing vector.";
		}
	}



	private void ChooseLineColor()
	{
		selectedButton = inventoryPanelMethods.GetSelectedButton();
		if (selectedButton !=null){
			errorText.text = "";
			colorWheelPanel.SetActive (true);
			colorWheelPanel.GetComponent<ColorWheelMethods> ().SetCallerName ("lineBuilder");
		}else
		{
			errorText.text = inventoryPanelMethods.GetItemInventoryCount()>0 ? "Please select a line to modify." : "Please create a line to modify.";
		}
	}

	public void SetLineColor(Color newColor){
		inventoryPanelMethods.ItemButtonSelected(selectedButton);
		inventoryPanelMethods.SetGUIObjectColor(newColor);
	}

	public Color GetLineColor(){
		return inventoryPanelMethods.GetGUIObjectColor();
	}

	public void CloseColorWheelPanel(){
		colorWheelPanel.SetActive (false);
	}

	public void AddLineFromSettingsMessage (string fromBody, string toBody, Color lineColor){
		if (fromBody != toBody){
			GameObject newButton = CreateLineObject(fromBody, toBody, false);
			newButton.GetComponent<InventoryButton>().SetGUIObjectColor(lineColor);
		}
		else{
			VizardGUISettings.UpdateErrorMessages("Error adding pointing vector from message: Please check that two different bodies have been provided.");
		}
	}

	private static Color GetDefaultStartingLineColor(string toBodyName)
	{
		string stringToCheck = toBodyName.ToLower();
		if (stringToCheck.Contains("sun")) {
			return Color.yellow;
		}

		if (stringToCheck.Contains("mars")) {
			return Color.red;
		}

		if (stringToCheck.Contains("earth")){
			return Color.cyan;
		}

		if (stringToCheck.Contains("moon")) {
			return Color.white;
		}

		return UnityEngine.Random.ColorHSV(0,1,0,1,0.75f,1);
	}

	private void MainFromBodyDropdownValueSelected(int optionValue)
	{
		if (optionValue != 0)
		{
			fromBodyEffectorParent = "";
			fromBodyDropdown.options[0].text = "Select Body";
			if (openSubMenu != null)
			{
				openSubMenu.SetActive(false);
			}
		}
	}

	private void MainToBodyDropdownValueSelected(int optionValue)
	{
		if (optionValue != 0)
		{
			toBodyEffectorParent = "";
			toBodyDropdown.options[0].text = "Select Body";
			if (openSubMenu != null)
			{
				openSubMenu.SetActive(false);
			}
		}
	}

	/// <summary>
	/// Handles user input selection of a sub-dropdown option
	/// Only used when effectors are present in scenario
	/// <remarks>DO NOT DELETE</remarks>
	/// </summary>
	/// <param name="dropdownData">sub-dropdown option selected</param>
	public void SubDropdownValueSelected(string[] dropdownData)
	{
		if (dropdownData[0] == "fromBody")
		{
			
			fromBodyDropdown.options[0].text = dropdownData[2];
			fromBodyDropdown.value = 0;
			fromBodyDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dropdownData[2];
			fromBodyEffectorParent = dropdownData[1];

		}else if (dropdownData[0] == "toBody")
		{
			toBodyDropdown.options[0].text = dropdownData[2];
			toBodyDropdown.value = 0;
			toBodyDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dropdownData[2];
			toBodyEffectorParent = dropdownData[1];
		}
	}
	
	/// <summary>
	/// Opens a sub-dropdown menu
	/// Only used when effectors are present in scenario
	/// <remarks>DO NOT DELETE</remarks>
	/// </summary>
	/// <param name="openMenu">open sub-dropdown menu </param>
	public void SetOpenSubMenu(GameObject openMenu)
	{
		openSubMenu = openMenu;
	}

	public void CloseOpenSubMenu()
	{
		if (openSubMenu != null)
		{
			openSubMenu.SetActive(false);
			openSubMenu = null;
		}
	}

	public void UpdateAllPointingLineLineRenderers(bool isOn)
	{
		foreach (GameObject pv in pointingVectorList)
		{
			pv.GetComponent<DrawPointingVector>().UpdateLineRendererSettings(isOn);
		}
	}
	
}
