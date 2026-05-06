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
/// <summary>
/// Handles user input to the Change Main Camera Target panel
/// </summary>
public class ChangeMainCameraTargetGUIMethods : MonoBehaviour {
	[Header("Panel GUI Elements")]
	public TMP_Dropdown bodyDropdown;
	public GameObject myPanel;

	private bool firstClick = true;
	private List<string> bodyList;
	
	private MainCameraViewManager mainCameraViewMgr;
	
	private string targetBodyEffectorParent="";
	private GameObject openSubMenu;

	void Awake(){
		bodyDropdown.onValueChanged.AddListener (MainDropdownValueSelected);
	}

	void Start()
	{
		mainCameraViewMgr = MainCameraUtilities.MainCamera.GetComponent<MainCameraViewManager>();
	}

	void OnEnable ()
	{
		if (firstClick)
		{
			bodyList = VizardGUISettings.CreateBodyListForDropdown(bodyDropdown, "cameraTarget", false, true, true, false);
			firstClick = false;
		} else {
			bodyDropdown.value = 0;
		}
	}

	private void ChangeCameraTarget(string newTargetName)
	{
		if (newTargetName != "Select Body")
		{
			GameObject target = CelestialBodyStateUtilities.GetGameObjectWithBodyName(newTargetName, targetBodyEffectorParent);
			mainCameraViewMgr.SetupChangeOfMainCameraTarget(target);
			myPanel.SetActive(false);
		}
	}
	
	public void MainDropdownValueSelected(int optionValue)
	{
		if (optionValue != 0)
		{
			targetBodyEffectorParent = "";
			bodyDropdown.options[0].text = "Select Target";
			string newTargetName = bodyDropdown.options[bodyDropdown.value].text;
			ChangeCameraTarget(newTargetName);
			if (openSubMenu != null)
			{
				openSubMenu.SetActive(false);
			}
		}
	}

	public void SubDropdownValueSelected(string[] dropdownData)
	{
		if (dropdownData[0] == "cameraTarget")
		{
			bodyDropdown.options[0].text = dropdownData[2];
			bodyDropdown.value = 0;
			bodyDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dropdownData[2];
			targetBodyEffectorParent = dropdownData[1];
			ChangeCameraTarget(dropdownData[2]);
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
