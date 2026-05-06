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
/// Handles inputs to the Sprite Settings tab of the File>Settings panel
/// </summary>
public class SpritePanelMethods : MonoBehaviour
{
	public Button ApplyButton;
	public Button SelectColorButton;
	public Image spriteSample;
	public TMP_Dropdown spritesSelectDropdown;
	public GameObject objectInventory;
	public Slider planetSizeSlider;
	public Slider spacecraftSizeSlider;
	public TextMeshProUGUI planetSizeText;
	public TextMeshProUGUI spacecraftSizeText;
	public Toggle showPlanetSprites;
	public Toggle showSpacecraftSprites;

	public GameObject colorWheelPanel;

	private List<GameObject> objectToggles = new List<GameObject>();
	private List<GameObject> selectedObjects = new List<GameObject>();
	private List<GameObject> allSpacecraftToggles = new List<GameObject>();
	private List<string> spriteList = new List<string>(){"Circle", "Square", "Star", "Triangle","bskSat"};

	private bool firstClick = true;

	private Sprite currentSprite;
	private string currentSpriteName;

    // Start is called before the first frame update
    void Start()
    {
		ApplyButton.onClick.AddListener (ApplySpriteToObjects);
		SelectColorButton.onClick.AddListener (EnableColorChooser);
		spritesSelectDropdown.onValueChanged.AddListener (UpdateSampleSprite);
		planetSizeSlider.onValueChanged.AddListener (ChangePlanetSpriteSize);
		spacecraftSizeSlider.onValueChanged.AddListener (ChangeSpacecraftSpriteSize);
		showPlanetSprites.onValueChanged.AddListener(TogglePlanetSprites);
		showSpacecraftSprites.onValueChanged.AddListener(ToggleSpacecraftSprites);
        
    }

	void OnEnable(){
		if (firstClick) {
			VizardGUISettings.PopulateList (spritesSelectDropdown, spriteList);

			CreateSimulatedObjectsToggles ();

			if (VizardGUISettings.ShowSpritesForSpacecraft){
				showSpacecraftSprites.isOn = true;
			}
			if (VizardGUISettings.ShowSpritesForPlanets){
				showPlanetSprites.isOn = true;
			}

			currentSprite = VizardGUISettings.GetSprite("CIRCLE");
			firstClick = false;
		}
	}

	private void CreateSimulatedObjectsToggles(){
		if (SpacecraftStateUtilities.ParentSpacecraftList.Count>1){
			CreateAllSpacecraftToggle();
		}
		foreach(GameObject sc in SpacecraftStateUtilities.ParentSpacecraftList){
			GameObject newObjectToggle = CreateObjectToggle(sc);
			allSpacecraftToggles.Add(newObjectToggle);
		}
		foreach(GameObject cb in CelestialBodyStateUtilities.CelestialBodiesList){
			CreateObjectToggle(cb);
		}
		if (objectToggles.Count>8){
			float width = objectInventory.GetComponent<RectTransform>().rect.width; 
			objectInventory.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 25*objectToggles.Count);
		}
	}

	private GameObject CreateObjectToggle(GameObject simulatedBody){

		GameObject newObjectToggle = Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericToggle") as GameObject, objectInventory.transform, true);
		newObjectToggle.name = simulatedBody.name;
		newObjectToggle.GetComponent<Toggle>().isOn = false;
		newObjectToggle.AddComponent<InventoryToggle>();
		newObjectToggle.GetComponent<InventoryToggle>().SetupToggleWithGUIObject(simulatedBody, transform.gameObject, "SPRITE");

		objectToggles.Add(newObjectToggle);

		newObjectToggle.GetComponent<RectTransform>().localScale = Vector3.one;
		newObjectToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(5,-(objectToggles.Count-1)*25-5);

		return newObjectToggle;
	}

	private void CreateAllSpacecraftToggle(){
		GameObject newObjectToggle = Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericToggle") as GameObject, objectInventory.transform, true);
		newObjectToggle.name = "AllSpacecraftToggle";
		newObjectToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Select All Spacecraft";
		newObjectToggle.GetComponent<Toggle>().isOn = false;
		newObjectToggle.GetComponent<Toggle>().onValueChanged.AddListener(SelectAllSpacecraft);
		objectToggles.Add(newObjectToggle);

		newObjectToggle.GetComponent<RectTransform>().localScale = Vector3.one;
		newObjectToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(5,-(objectToggles.Count-1)*25-5);
	}

	private void SelectAllSpacecraft(bool isOn){
		foreach(GameObject scToggle in allSpacecraftToggles){
			scToggle.GetComponent<Toggle>().isOn = isOn;
		}
	}

	private void ApplySpriteToObjects(){
		if (currentSprite != null) {
			foreach (GameObject toggle in objectToggles) {
				if (toggle.name != "AllSpacecraftToggle") {
					if (toggle.GetComponent<Toggle> ().isOn) {
						GameObject simBodyToChange = toggle.GetComponent<InventoryToggle> ().myGUIObject;
						GameObject spriteToUpdate = null;
						if (simBodyToChange.CompareTag("Spacecraft")){ //Effectors do not have sprites
							spriteToUpdate = simBodyToChange.GetComponent<SpacecraftController> ().spacecraftSprite;
						}else if (simBodyToChange.CompareTag("Planet")){
							spriteToUpdate = simBodyToChange.GetComponent<PlanetController> ().planetSprite;
						}

						if (spriteToUpdate != null)
						{
							SpriteRenderer spriteRenderer = spriteToUpdate.GetComponent<SpriteRenderer>();
							spriteRenderer.sprite = currentSprite;
							spriteRenderer.color = spriteSample.color;
						}
					}
				}
				else
				{
					if (toggle.GetComponent<Toggle>().isOn)
					{//Save off new default for all spacecraft sprites
						Color spriteColor = spriteSample.color;
						string newDefaultSpriteSetting =
							$"{currentSpriteName} {Mathf.RoundToInt(spriteColor.r * 255)} {Mathf.RoundToInt(spriteColor.g * 255)} {Mathf.RoundToInt(spriteColor.b * 255)} {Mathf.RoundToInt(spriteColor.a * 255)}";
						PersistentUserSettings.persistentSettingsFromLastSave.DefaultSpacecraftSprite=newDefaultSpriteSetting;
						PersistentUserSettings.currentSessionUserAppliedSettings.DefaultSpacecraftSprite =
							newDefaultSpriteSetting;

					}

				}
			}
		}
	}

	private void UpdateSampleSprite(int value){
		currentSpriteName = spriteList [value];
		currentSprite = VizardGUISettings.GetSprite(currentSpriteName);
		spriteSample.sprite = currentSprite;		
	}

	public void UpdateSpriteColor(Color newColor){
		spriteSample.color = newColor;
		colorWheelPanel.SetActive (false);
	}

	private void EnableColorChooser ()
	{
		colorWheelPanel.SetActive (true);
		colorWheelPanel.GetComponent<ColorWheelMethods> ().SetCallerName("spritePanel");
	}

	public Color GetSpriteColor(){
		return spriteSample.color;
	}

	public void ObjectToggleSelected(GameObject toggleSelected, bool toggledOn)
	{
		if (toggledOn){
			selectedObjects.Add(toggleSelected);
		}else{
			selectedObjects.Remove(toggleSelected);
		}

		ApplyButton.interactable = selectedObjects.Count >= 1;
	}

	private void ChangePlanetSpriteSize(float newValue){
		planetSizeText.text = $"{newValue}";
		VizardGUISettings.PlanetSpriteSize = 0.02f*newValue;
	}

	private void ChangeSpacecraftSpriteSize(float newValue){
		spacecraftSizeText.text = $"{newValue}";
		VizardGUISettings.SpacecraftSpriteSize = 0.02f*newValue;
	}

	private void ToggleSpacecraftSprites(bool isOn){
		VizardGUISettings.ShowSpritesForSpacecraft = isOn;
	}

	private void TogglePlanetSprites(bool isOn){
		VizardGUISettings.ShowSpritesForPlanets = isOn;
	}
}
