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
/// Handles input to the chief spacecraft selection panel
/// </summary>
public class ChangeChiefSpacecraftGUIMethods : MonoBehaviour {
	[Header("Panel GUI Elements")]
	public TMP_Dropdown bodyDropdown;
	public GameObject myPanel;
	public TextMeshProUGUI errorText;

	private readonly Vector2 noErrorDims = new Vector2( 170f, 60f);
	private readonly Vector2 withErrorDims = new Vector2(170f, 80f);

	private bool firstClick = true;
	private List<string> bodyList;
	

	void Start(){
		bodyDropdown.onValueChanged.AddListener (ChangeChiefSpacecraft);
	}

	void OnEnable ()
	{
		if (firstClick)
		{
			bodyList = VizardGUISettings.CreateBodyListForDropdown(bodyDropdown, "Auto: Use Camera Target", false,
				false);
			firstClick = false;
		} 

		ChangeDropdownChoice();
	}

	public void ChangeDropdownChoice(){
		if(VizardGUISettings.SetChiefToCamTgt){
			bodyDropdown.value = 0;
		}else{
			for(int i = 0; i<bodyList.Count; i++)
			{
				string chiefSpacecraftName =
					SpacecraftStateUtilities.SpacecraftList[VizardGUISettings.ChiefSpacecraftIndex].name;
				if (bodyList[i]==chiefSpacecraftName){
					bodyDropdown.value = i;
					break;
				}
			}
		}
	}

	private void ChangeChiefSpacecraft(int listValue)
	{
		int chiefIndex = VizardGUISettings.ChiefSpacecraftIndex;
		if (listValue > 0) {
	
			chiefIndex = SpacecraftStateUtilities.GetSpacecraftIndex(bodyList [listValue]);
			VizardGUISettings.SetChiefToCamTgt = false;
		}else
		{
			// chief = "use cam target";
			VizardGUISettings.SetChiefToCamTgt = true;
			if(MainCameraUtilities.CameraTarget.CompareTag("Spacecraft")){ //Effectors cannot be chief spacecraft
				chiefIndex = MainCameraUtilities.CameraTargetIndex;
			}
		}

		VizardGUISettings.RelativeTruePathChangeCount++;
		SpacecraftStateUtilities.UpdateChiefSpacecraft(chiefIndex);
		if (errorText.transform.gameObject.activeSelf){
			myPanel.GetComponent<RectTransform>().sizeDelta = noErrorDims;
			errorText.transform.gameObject.SetActive(false);
		}
	}

	public void SpacecraftNameNotFound(string badName){
		errorText.text = "No spacecraft of name "+badName+" found.";
		myPanel.GetComponent<RectTransform>().sizeDelta = withErrorDims;
		errorText.transform.gameObject.SetActive(true);
	}

}
