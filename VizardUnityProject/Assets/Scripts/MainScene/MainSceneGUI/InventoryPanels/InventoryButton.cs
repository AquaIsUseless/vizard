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
/// <summary>
/// Sets up an inventory button with an scenario or GUI object to which it
/// provides a reference. 
/// </summary>
public class InventoryButton : MonoBehaviour {

	public Button buttonComponent;
	public TextMeshProUGUI textComponent;
	public Color myGUIObjectColor;

	public GameObject myGUIObject; //Set this to the GUI component to be turned on and off 
	public GameObject myGUIObjectPartToToggle;
	public GameObject myParentInventory;
	public bool showGUIObject = true;
	private Image colorThumbnail;

	private GameObject xOutImage;
	private int objectID; //used to avoid duplicate names, default is 0

	public void SetupButton(GameObject guiObject, GameObject guiObjectPartToToggle, Color guiObjectColor, GameObject inventoryPanel, string guiName, int newID=0, bool useColorChip = true)
	{
		buttonComponent = useColorChip ? GetComponentInParent<Button> () : GetComponentInChildren<Button>();
		textComponent = GetComponentInChildren<TextMeshProUGUI> ();

		buttonComponent.onClick.AddListener (ButtonSelected);

		myGUIObject = guiObject;
		myGUIObjectPartToToggle = guiObjectPartToToggle;
		myGUIObjectColor = guiObjectColor;
		objectID = newID;

		if(useColorChip){
			colorThumbnail = transform.GetChild (0).GetComponent<Image> ();
			colorThumbnail.color = guiObjectColor;
			xOutImage = transform.GetChild (1).gameObject;
		}

		string nameString = guiName;
		myParentInventory = inventoryPanel;
		textComponent.text = nameString;
		transform.name = nameString+"Button";
	}


	private void ButtonSelected()
	{
		myParentInventory.SendMessage("ItemButtonSelected", this.gameObject);
	}

	public void HideOrShowAssociatedGUIObject(){
		if (showGUIObject) {
			showGUIObject = false;
			xOutImage.SetActive (true);
			SetGUIObjectVisibility (false);
		} else {
			showGUIObject = true;
			xOutImage.SetActive (false);
			SetGUIObjectVisibility (true);
		}
	}

	public void AddHideShowListenerToToggle(bool startOutOn=true){
		GetComponent<Toggle>().isOn = startOutOn;
		GetComponent<Toggle>().onValueChanged.AddListener(ToggleGUIObjectAction);
	}
	public void ToggleGUIObject(bool isOn){
		myGUIObject.SetActive(isOn);
	}

	private void ToggleGUIObjectAction(bool isOn){
		myGUIObject.SendMessage("ToggleGUIObjectFromPanel", isOn);
	}


	// THIS METHOD NEEDS TO GO OUT INTO THE 
	private void SetGUIObjectVisibility(bool isVisible){
		myGUIObjectPartToToggle.gameObject.SetActive(isVisible);
	}

	public Color GetGUIObjectColor(){
		return myGUIObjectColor;
	}

	public void SetGUIObjectColor(Color newColor){
		myGUIObjectColor = newColor;
		if (colorThumbnail != null){
			colorThumbnail.color = newColor;
		}
		if (myGUIObject.activeSelf){
			myGUIObject.SendMessage("SetColor", newColor);
		}else{
			myGUIObject.SetActive(true);
			myGUIObject.SendMessage("SetColor", newColor);
			myGUIObject.SetActive(false);
		}
	}

	public int GetObjectID(){
		return objectID;
	}
		
	public void SetButtonText(string newName){
		textComponent.text = newName;
	}

	public string GetButtonText()
	{
		return textComponent.text;
	}
		
}
