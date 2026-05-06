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
using VizProtobufferMessage;
/// <summary>
/// Handles user input to the Add Location Panel
/// to create or modify a Location
/// </summary>
public class AddLocationPanelMethods : MonoBehaviour
{
	//The following components are wired in the editor to the button in the View subpanel

	
	[Header("Panel GUI Components")]
	public TMP_InputField nameField;
	public Toggle enableFullLocationToggle;
	public List<TextMeshProUGUI> fullLocTextGroup;
	public TMP_InputField xCoord;
	public TMP_InputField yCoord;
	public TMP_InputField zCoord;
	public TMP_InputField xNormal;
	public TMP_InputField yNormal;
	public TMP_InputField zNormal;
	public TMP_InputField FOV;
	public TMP_InputField range;
	public Button colorButton;
	public Image colorSample;
	public TextMeshProUGUI errorText;
	public TMP_Dropdown parentBodyDropdown;
	public Button applySettingsButton;
	public Button cancelButton;
	public Toggle useUserRangeToggle;
	
	[Header("Required References")]
	public LocationManager locationManager;
	public GameObject locationInventoryPanel;
	public GameObject colorWheelPanel;
	
	private bool firstBuild = true;
	private bool goToColorChooser;
	private bool canChangeThisLocation = true;
	private string parentBodyEffectorParent=""; //Used if sub menu of effectors is in-use
	private GameObject openSubMenu;
	private GameObject selectedButton;

    // Start is called before the first frame update
    void Awake()
    {
		colorButton.onClick.AddListener (EnableColorChooser);
		applySettingsButton.onClick.AddListener (ApplyPanelSettingsToCreateLocation);
		cancelButton.onClick.AddListener (CancelBuild);
		parentBodyDropdown.onValueChanged.AddListener(MainParentBodyDropdownValueSelected);
		enableFullLocationToggle.onValueChanged.AddListener(ToggleFullLocation);
    }

	void OnEnable ()
	{
		transform.SetAsLastSibling();

		if (firstBuild) {
			VizardGUISettings.CreateBodyListForDropdown(parentBodyDropdown, "parentBody", false, true, true, false);
			firstBuild = false;
		}
		errorText.text = "";
		
		selectedButton = locationInventoryPanel.GetComponent<InventoryPanelMethods>().GetSelectedButton();
		if (!goToColorChooser){
			if (selectedButton ==null){
				UseDefaultSettings();
			}else{
				RestoreLocationSettings();
			}
		}else{
			goToColorChooser = false;
		}
		locationInventoryPanel.SetActive (false);
	}

	private void EnableColorChooser ()
	{
		goToColorChooser = true;
		colorWheelPanel.SetActive (true);
		colorWheelPanel.GetComponent<ColorWheelMethods> ().SetCallerName("stationBuilder");
	}

	private void ToggleFullLocation(bool isOn)
	{
		Color textColor = isOn ? Color.white:Color.gray;

		foreach (TextMeshProUGUI txt in fullLocTextGroup)
		{
			txt.color = textColor;
		}
		
		xNormal.interactable=isOn;
		yNormal.interactable=isOn;
		zNormal.interactable=isOn;
		FOV.interactable=isOn;
		range.interactable=isOn;
		useUserRangeToggle.interactable = isOn;
	}

	public void SetLocationColor(Color newColor){
			colorSample.color = new Color (newColor.r, newColor.g, newColor.b, 1f);
	}
	public Color GetLocationColor(){
		if (selectedButton!=null){
			 return selectedButton.GetComponent<InventoryButton>().GetGUIObjectColor();
		}else{
			return colorSample.color;
		}
	}

	public void CloseColorWheelPanel(){
		colorWheelPanel.SetActive (false);
	}


	private void ApplyPanelSettingsToCreateLocation(){
		errorText.text = "";
		if (canChangeThisLocation)
		{
			bool createOrModifyLocationAndClosePanel = true;
			string stationName = nameField.text;
			if (stationName == "")
			{
				errorText.text = "Please provide a name for this location.";
				createOrModifyLocationAndClosePanel = false;
			}else if ((selectedButton==null)&&(CelestialBodyStateUtilities.LocationsDictionary.ContainsKey(stationName)))
			{
				errorText.text = "Please provide a name for this location that is not already in use.";
				createOrModifyLocationAndClosePanel = false;
			}

			string parentName = parentBodyDropdown.options[parentBodyDropdown.value].text;
			if (parentName == "Select Body")
			{
				errorText.text = "Please select a parent body from the dropdown.";
				createOrModifyLocationAndClosePanel = false;
			}

			double[] origin = new double[]
				{double.Parse(xCoord.text), double.Parse(yCoord.text), double.Parse(zCoord.text)};

			Vector3 normal = Vector3.one;
			float fov = 135f;
			float userRange = -1f;
			if (enableFullLocationToggle.isOn)
			{
				normal = new Vector3(float.Parse(xNormal.text), float.Parse(yNormal.text),
					float.Parse(zNormal.text));
				if (normal == Vector3.zero)
				{
					errorText.text = "Please provide a non-zero normal vector.";
					createOrModifyLocationAndClosePanel = false;
				}

				fov = float.Parse(FOV.text);
				if ((fov < 0.0001) || (fov > 179.9999))
				{
					errorText.text = "Field Of View must be within 0.0001 to 179.9999 degrees.";
					createOrModifyLocationAndClosePanel = false;
				}

				userRange = -1f;
				if (useUserRangeToggle.isOn)
				{
					userRange = float.Parse(range.text);
					if (userRange <= 0)
					{
						errorText.text = "Location range must be greater than zero meters.";
						createOrModifyLocationAndClosePanel = false;
					}
				}
			}

			if (createOrModifyLocationAndClosePanel)
			{
				VizMessage.Types.Location newLoc = new VizMessage.Types.Location()
				{
					StationName = stationName,
					ParentBodyName = parentName,
					RGPP = {origin[0], origin[1], origin[2]},
					GHatP = {normal[0], normal[1], normal[2]},
					FieldOfView = fov,
					Range = userRange,
					IsHidden = false,
					MarkerScale = 1,
					Color =
					{
						Mathf.RoundToInt(colorSample.color.r * 255), Mathf.RoundToInt(colorSample.color.g * 255),
						Mathf.RoundToInt(colorSample.color.b * 255), Mathf.RoundToInt(colorSample.color.a * 255)
					}
				};
				locationInventoryPanel.SetActive(true);
				if (selectedButton == null)
				{
					GameObject newLocation = locationManager.AddLocation(newLoc, 0, enableFullLocationToggle.isOn);
					if (newLocation != null)
					{
						locationInventoryPanel.GetComponent<LocationInventoryPanelMethods>()
							.AddInventoryButtonForLocation(newLocation, colorSample.color);
					}
				}
				else
				{
					selectedButton.GetComponent<InventoryButton>().myGUIObject.GetComponent<DrawLocationMarker>()
						.ApplyLocationSettings(newLoc, true);
					selectedButton.GetComponent<InventoryButton>().SetGUIObjectColor(colorSample.color);
					string oldButtonText = selectedButton.GetComponent<InventoryButton>().GetButtonText();
					if (stationName != oldButtonText)
					{
						selectedButton.GetComponent<InventoryButton>().SetButtonText(stationName);
						CelestialBodyStateUtilities.LocationsDictionary.Remove(oldButtonText);
						CelestialBodyStateUtilities.LocationsDictionary[stationName] = selectedButton
							.GetComponent<InventoryButton>()
							.myGUIObject.GetComponent<DrawLocationMarker>();
					}


				}
				transform.gameObject.SetActive(false);
			}
		}
		else
		{
			errorText.text =
				"This location's settings are continuously refreshed from messages and cannot be changed here.";
		}
	}

	private void CancelBuild(){
		locationInventoryPanel.SetActive (true);
		this.gameObject.SetActive (false);
	}

	private void RestoreLocationSettings(){
		//TODO: This whole thing is a problem because I need to maybe only modify locations that don't get made in messages?
		DrawLocationMarker currentLocation = selectedButton.GetComponent<InventoryButton>().myGUIObject
			.GetComponent<DrawLocationMarker>();
		VizMessage.Types.Location mySettings = currentLocation.GetCurrentLocationSettings();
		nameField.text = mySettings.StationName;
		parentBodyDropdown.GetComponent<HoverDropdown>().SetForOptionWithDropdownLockout(mySettings.ParentBodyName);
		
		xCoord.text = $"{mySettings.RGPP[0]}";
		yCoord.text = $"{mySettings.RGPP[1]}";
		zCoord.text = $"{mySettings.RGPP[2]}";
		xNormal.text = $"{mySettings.GHatP[0]}";
		yNormal.text = $"{mySettings.GHatP[1]}";
		zNormal.text = $"{mySettings.GHatP[2]}";
		FOV.text = $"{mySettings.FieldOfView}";
		useUserRangeToggle.isOn = mySettings.Range>0;
		range.text = (mySettings.Range).ToString ("E");
		colorSample.color = currentLocation.GetLocationColor();
		canChangeThisLocation = !currentLocation.updateLocationFromMessages;
	}

	private void UseDefaultSettings()
	{
		canChangeThisLocation = true;
		nameField.text = "";
		parentBodyDropdown.value = 0;
		xCoord.text = "0";
		yCoord.text = "0";
		zCoord.text = "0";
		xNormal.text = "0";
		yNormal.text = "0";
		zNormal.text = "0";
		FOV.text = "135";
		useUserRangeToggle.isOn = false;
		range.text = "";
		colorSample.color = Color.blue;
	}


	private void MainParentBodyDropdownValueSelected(int optionValue)
	{
		if (optionValue != 0)
		{
			parentBodyEffectorParent = "";
			parentBodyDropdown.options[0].text = "Select Body";
			if (openSubMenu != null)
			{
				openSubMenu.SetActive(false);
			}
		}
	}

	public void SubDropdownValueSelected(string[] dropdownData)
	{
		if (dropdownData[0] == "parentBody")
		{
			parentBodyDropdown.options[0].text = dropdownData[2];
			parentBodyDropdown.value = 0;
			parentBodyDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dropdownData[2];
			parentBodyEffectorParent = dropdownData[1];
		}
	}

	public void SetOpenSubMenu(GameObject openMenu)
	{
		openSubMenu = openMenu;
	}

	public void CloseOpenSubMenu()
	{
		openSubMenu.SetActive(false);
		openSubMenu = null;
	}
}
