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
using UnityEngine.UI;
/// <summary>
/// Creates inventory buttons for all Locations and handles user input
/// to add, modify, or delete Locations.
/// </summary>
public class LocationInventoryPanelMethods : MonoBehaviour
{
	[Header("Panel GUI Components")]
	public Button removeLocationButton;
	public Toggle showLocationConesToggle;
	public Toggle showLocationLinesToggle;
	public Transform inventoryContentPanel;
	public Scrollbar contentScrollbar;
	
	private int buttonCounter;

    // Start is called before the first frame update
    void Start()
	{
		showLocationConesToggle.onValueChanged.AddListener(ToggleLocationCones);
		showLocationLinesToggle.onValueChanged.AddListener(ToggleLocationLines);  
		removeLocationButton.onClick.AddListener(CleanAllAntennaLists);
    }

    // Update is called once per frame
    void OnEnable()
    {
		showLocationConesToggle.isOn = VizardGUISettings.ShowStationCone;
		showLocationLinesToggle.isOn = VizardGUISettings.ShowStationCommunicationLines;
		foreach (DrawLocationMarker loc in CelestialBodyStateUtilities.LocationsDictionary.Values)
		{
			if (loc.GetInventoryButton() ==null){
				AddInventoryButtonForLocation(loc.gameObject, loc.GetLocationColor());
			}
		}
    }

    private void ToggleLocationCones(bool isOn){
		VizardGUISettings.ShowStationCone = isOn;
		int count = inventoryContentPanel.childCount;
		for (int i =0; i <count; i++){
			GameObject glButton = inventoryContentPanel.GetChild(i).gameObject;
			GameObject location = glButton.GetComponent<InventoryButton>().myGUIObject;
			if (location.GetComponent<DrawLocationMarker>().isFullLocation)
			{
				location.GetComponentInChildren<FullLocationMethods>().ToggleVisibleConeElements(isOn);
			}
		}
	}

	private void ToggleLocationLines(bool isOn){
		VizardGUISettings.ShowStationCommunicationLines = isOn;
	}

	private void CleanAllAntennaLists(){
		 int count = inventoryContentPanel.childCount;
		 GameObject glButton;
		 for (int i =0; i <count; i++){
		 	glButton = inventoryContentPanel.GetChild(i).gameObject;
		 	glButton.GetComponent<InventoryButton>().myGUIObject.GetComponent<DrawLocationMarker>().CleanAntennaInViewList();
		 }
	}

	public void AddInventoryButtonForLocation(GameObject newLocation,Color stationColor)
	{
		GameObject newLocationButton = Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericButtonWithLabelAndImage") as GameObject) as GameObject;
		newLocationButton.AddComponent<InventoryButton> ();
		newLocationButton.GetComponent<InventoryButton> ().SetupButton (newLocation, newLocation, stationColor, this.gameObject, newLocation.name, buttonCounter);
		buttonCounter+=1;
		GetComponent<InventoryPanelMethods> ().AddItemButtonToInventory (newLocationButton);
		newLocation.GetComponent<DrawLocationMarker> ().SetInventoryButton(newLocationButton);
		inventoryContentPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(375, (buttonCounter* 22));
		contentScrollbar.numberOfSteps = Mathf.CeilToInt(buttonCounter / 7f);
	}
}
