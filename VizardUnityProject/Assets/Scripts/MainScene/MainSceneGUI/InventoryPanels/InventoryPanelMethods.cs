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
/// Generic inventory management for an inventory panel
/// containing a list of buttons or toggles
/// </summary>
public class InventoryPanelMethods : MonoBehaviour
{
	//The following components are wired in the editor to the button in the inventory panel itself
	[Header("Inventory Panel GUI Components")]
	public Button AddItemButton;
	public Button ModifyItemButton;
	public Button HideItemButton;
	public Button RemoveItemButton;
	public GameObject myInventoryPanel;
	public Transform contentPanel;
	public Scrollbar contentScrollBar;
	public GameObject mySettingsPanel;

	public bool useDefaultValuesInSettings = true;

	//The rest of these variables are defined internally in the script

	private List<GameObject> itemButtons = new List<GameObject> ();

	private GameObject selectedButton;


	void Start ()
	{
		AddItemButton.onClick.AddListener (AddItem);
		if (ModifyItemButton != null){
			ModifyItemButton.onClick.AddListener (ModifyItem);
		}
		HideItemButton.onClick.AddListener (HideShowItem);
		RemoveItemButton.onClick.AddListener (RemoveItem);

	}

	void OnEnable(){
			
		transform.SetAsLastSibling();
		selectedButton = null;
	}
	private void AddItem()
	{
		selectedButton = null;
		useDefaultValuesInSettings = true;
		if (mySettingsPanel!=myInventoryPanel){
			mySettingsPanel.SetActive (true);
		}
	}

	private void ModifyItem(){
		useDefaultValuesInSettings = false;
		mySettingsPanel.SetActive (true);
		SetButtonState (false);
	}

	public GameObject GetSelectedButton(){
		return selectedButton;
	}

	public void ItemButtonSelected (GameObject inventoryButton)
	{
		selectedButton = inventoryButton;

		if (selectedButton.GetComponent<InventoryButton> ().showGUIObject) {
			HideItemButton.GetComponentInChildren<TextMeshProUGUI> ().text = "Hide";
		} else {
			HideItemButton.GetComponentInChildren<TextMeshProUGUI> ().text = "Show";
		}
		SetButtonState (true);


	}

	public void SetGUIObjectColor(Color newColor){
		selectedButton.GetComponent<InventoryButton> ().SetGUIObjectColor (newColor);
	}

	public Color GetGUIObjectColor(){
		return selectedButton.GetComponent<InventoryButton>().GetGUIObjectColor();
	}


	private void HideShowItem ()
	{
		selectedButton.GetComponent<InventoryButton> ().HideOrShowAssociatedGUIObject ();
		SetButtonState (false);
	}

	private void RemoveItem ()
	{
		List<GameObject> remainingButtons = new List<GameObject> ();
		int buttonCount = 0;
		foreach (GameObject button in itemButtons) {
			if (button != selectedButton) {
				button.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, buttonCount * -22);
				buttonCount += 1;
				remainingButtons.Add (button);
			}
		}

		Destroy (selectedButton.GetComponent<InventoryButton> ().myGUIObject);
		Destroy (selectedButton);

		itemButtons = remainingButtons;

		SetButtonState (false);
	}

	private void SetButtonState(bool isInteractable){
		if (ModifyItemButton != null){
			ModifyItemButton.interactable = isInteractable;
		}
		HideItemButton.interactable = isInteractable;
		RemoveItemButton.interactable = isInteractable;
	}

	public void AddItemButtonToInventory (GameObject newButton){
		if (mySettingsPanel!= myInventoryPanel){
			mySettingsPanel.SetActive (false);
		}
		//Add button to inventory list
		itemButtons.Add (newButton);
		newButton.transform.SetParent (contentPanel);
		newButton.GetComponent<RectTransform>().localScale = Vector3.one;
		newButton.GetComponent<RectTransform> ().anchoredPosition = new Vector2 (0, (itemButtons.Count-1) *- 22);
		contentScrollBar.value = 1;
	}

	public void UpdateItemButtonInInventory (GameObject selectedButton){
		if (mySettingsPanel!= myInventoryPanel){
			mySettingsPanel.SetActive (false);
		}
		//Make sure the button's color and name are up to date

		selectedButton.transform.GetChild(0).gameObject.GetComponent<Image>().color = selectedButton.GetComponent<InventoryButton>().myGUIObject.GetComponent<DrawKeepOutInCone>().GetConeColor();
		string newName = selectedButton.GetComponent<InventoryButton>().myGUIObject.GetComponent<DrawKeepOutInCone>().GetConeLabel();

		selectedButton.GetComponentInChildren<TextMeshProUGUI> ().text = newName;
		selectedButton.name = newName + " Button " + selectedButton.GetComponent<InventoryButton> ().GetObjectID ();
		selectedButton.GetComponent<InventoryButton>().myGUIObject.name = newName + " ID: " + selectedButton.GetComponent<InventoryButton> ().GetObjectID ();
	}

	public int GetItemInventoryCount(){
		return itemButtons.Count;
	}

}
