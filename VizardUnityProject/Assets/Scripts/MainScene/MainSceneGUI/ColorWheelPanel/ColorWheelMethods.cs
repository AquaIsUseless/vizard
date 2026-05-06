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
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
/// <summary>
/// Handles user input to the color wheel panel and
/// returns the chosen color to the calling panel. 
/// </summary>
public class ColorWheelMethods : MonoBehaviour, IPointerClickHandler {

	[Header("Calling Panels")]
	public AddPointingVectorPanelMethods addPointingVectorPanelMethods;
	public AddKeepOutInConePanelMethods addKeepOutInConePanelMethods;
	public AddLocationPanelMethods addLocationPanelMethods;
	public SpritePanelMethods spritePanelMethods;
	public Image defaultThrusterColorImage;
	public LightPanelMethods lightPanelMethods;

	[Header ("Color Wheel Panel GUI Components")]
	public Texture2D colorPicker;
	public Image colorWheelImage;
	public Image colorSample; 
	public Button applyButton;
	public Button cancelButton;
	public TMP_InputField redInput;
	public TMP_InputField greenInput;
	public TMP_InputField blueInput;

	private string callerName = "lineBuilder";
	private RectTransform colorWheelRect;
	private Vector2 pickPosition;
	private int redValue;
	private int greenValue;
	private int blueValue;

	void Start ()
	{
		applyButton.onClick.AddListener (ApplyColorChange);
		cancelButton.onClick.AddListener (CancelColorSelection);
		redInput.onEndEdit.AddListener (UpdateColorWithRedValue);
		greenInput.onEndEdit.AddListener (UpdateColorWithGreenValue);
		blueInput.onEndEdit.AddListener (UpdateColorWithBlueValue);

	}

	void OnEnable(){
		transform.SetAsLastSibling();
	}

	void Awake(){

		colorWheelRect = colorWheelImage.rectTransform;

	}

	public void SetCallerName(string callString){
		callerName = callString;
		if (callString =="lineBuilder"){
			SetPanelToObjectColor(addPointingVectorPanelMethods.GetLineColor());
			addPointingVectorPanelMethods.gameObject.SetActive(false);
		}else if (callString == "coneBuilder"){
			SetPanelToObjectColor(addKeepOutInConePanelMethods.GetConeColor());
			addKeepOutInConePanelMethods.gameObject.SetActive(false);
		}else if (callString == "spritePanel"){
			SetPanelToObjectColor(spritePanelMethods.GetSpriteColor());
		}else if (callString == "thrusterSettingsPanel"){
			SetPanelToObjectColor(ThrusterUtilities.GetDefaultThrusterColor());
		}else if (callString == "stationBuilder"){
			SetPanelToObjectColor(addLocationPanelMethods.GetLocationColor());
			addLocationPanelMethods.gameObject.SetActive(false);
		}else if (callString == "lightBuilder"){
			SetPanelToObjectColor(lightPanelMethods.GetLightColor());
			lightPanelMethods.gameObject.SetActive(false);
		}
	}

	public void OnPointerClick(PointerEventData data){
		RectTransformUtility.ScreenPointToLocalPointInRectangle (colorWheelRect, data.position, data.pressEventCamera, out pickPosition);

		Color col = colorPicker.GetPixel ((int)pickPosition.x*205/150,(150+(int)pickPosition.y)*202/150);
		colorSample.color = col;

		redValue = (int)(col.r * 255);
		greenValue = (int)(col.g * 255);
		blueValue = (int)(col.b * 255);
		redInput.text = redValue.ToString();
		greenInput.text = greenValue.ToString ();
		blueInput.text = blueValue.ToString ();
		applyButton.interactable = true;	
	}

	private void SetPanelToObjectColor(Color oldColor){
		colorSample.color = oldColor;
		redValue = (int)(oldColor.r * 255);
		greenValue = (int)(oldColor.g * 255);
		blueValue = (int)(oldColor.b * 255);
		redInput.text = redValue.ToString();
		greenInput.text = greenValue.ToString ();
		blueInput.text = blueValue.ToString ();
	}

	private void ApplyColorChange(){
		if (callerName =="lineBuilder") {
			addPointingVectorPanelMethods.gameObject.SetActive (true);
			addPointingVectorPanelMethods.SetLineColor (colorSample.color);
			addPointingVectorPanelMethods.CloseColorWheelPanel ();
		} else if (callerName == "coneBuilder"){
			addKeepOutInConePanelMethods.gameObject.SetActive (true);
			addKeepOutInConePanelMethods.SetConeColor (colorSample.color);
			addKeepOutInConePanelMethods.CloseColorWheelPanel ();
		}else if (callerName == "spritePanel"){
			spritePanelMethods.gameObject.SetActive (true);
			spritePanelMethods.UpdateSpriteColor (colorSample.color);
		}else if (callerName == "thrusterSettingsPanel"){
			defaultThrusterColorImage.color = colorSample.color;
			ThrusterUtilities.SetDefaultThrusterColorSetting(colorSample.color, true);
		}else if (callerName == "stationBuilder"){
			addLocationPanelMethods.gameObject.SetActive (true);
			addLocationPanelMethods.SetLocationColor (colorSample.color);
			addLocationPanelMethods.CloseColorWheelPanel ();
		}else if (callerName == "lightBuilder"){
			lightPanelMethods.gameObject.SetActive (true);
			lightPanelMethods.SetLightColor (colorSample.color);
			lightPanelMethods.CloseColorWheelPanel ();
		}
	}

	private void CancelColorSelection(){
		if (callerName =="lineBuilder") {
			addPointingVectorPanelMethods.gameObject.SetActive (true);
			addPointingVectorPanelMethods.CloseColorWheelPanel ();
		} else if (callerName == "coneBuilder"){
			addKeepOutInConePanelMethods.gameObject.SetActive (true);
			addKeepOutInConePanelMethods.CloseColorWheelPanel ();
		}else if (callerName == "spritePanel"){
			spritePanelMethods.gameObject.SetActive (true);
		}else if (callerName == "thrusterSettingsPanel"){
		}else if (callerName == "stationBuilder"){
			addLocationPanelMethods.gameObject.SetActive (true);
			addLocationPanelMethods.CloseColorWheelPanel ();
		}else if (callerName == "lightBuilder"){
			lightPanelMethods.gameObject.SetActive (true);
			lightPanelMethods.CloseColorWheelPanel ();
		}
	}

//	public void CancelColorSelection(){
//		addPointingVectorPanelMethods.CloseColorWheelPanel ();
//	}

private void UpdateColorWithRedValue(string newValue){
		redValue = int.Parse (newValue);
		colorSample.color = new Color (((float)redValue )/ 255, ((float)greenValue )/ 255, ((float)blueValue )/ 255,1f);
		applyButton.interactable = true;
	}

	private void UpdateColorWithGreenValue(string newValue){
		greenValue = int.Parse (newValue);
		colorSample.color = new Color (((float)redValue )/ 255, ((float)greenValue )/ 255, ((float)blueValue )/ 255,1f);
		applyButton.interactable = true;
	}

	private void UpdateColorWithBlueValue(string newValue){
		blueValue = int.Parse (newValue);
		colorSample.color = new Color (((float)redValue )/ 255, ((float)greenValue )/ 255, ((float)blueValue )/ 255,1f);
		applyButton.interactable = true;
	}

}
