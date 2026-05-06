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

public class LabelPanelMethods : MonoBehaviour
{
	public GameObject labelTogglePanel;
	public GameObject allLabelHolder;
	public TextMeshProUGUI fontSizeText;
	private float currentGUIScale=1f;

	private bool firstTime = true;


	void OnEnable(){

		//Build the panel when someone wants to look at it.
		if (firstTime){
			int count = 0;
			GameObject[] allToggles = new GameObject[allLabelHolder.transform.childCount];
			foreach(Transform child in allLabelHolder.transform){
				string labelText = child.gameObject.name;
				if (labelText == "Spacecraft"){
					labelText = "All Spacecraft";
				}else if (labelText == "Effectors"){
					labelText = "All Effectors";
				}else if (labelText == "CelestialBodies"){
					labelText = "All Planets/Moons";
				}else if (labelText =="CoordinateSystems"){
					labelText = "Coordinate Systems";
				}
				GameObject newToggle = Instantiate (Resources.Load ("Prefabs/GUIGenerics/GenericTogglePanel") as GameObject) as GameObject;
				Destroy(newToggle.GetComponent<PanelToggle>());
				ToggleLabels labelToggler = newToggle.AddComponent<ToggleLabels>();
				newToggle.AddComponent<FocusPanel>();
				newToggle.name = labelText+" Labels Toggle";
				newToggle.GetComponentInChildren<TextMeshProUGUI>().text = labelText;

				labelToggler.SetupToggleForLabelGroup(child.gameObject);

				newToggle.transform.SetParent(labelTogglePanel.transform);
				newToggle.GetComponent<RectTransform>().localScale = Vector3.one;
				newToggle.GetComponent<Toggle>().isOn = CheckGUISettings(child.gameObject.name);
				RectTransformExtensions.SetRight(newToggle.GetComponent<RectTransform>(),0);
				allToggles[count] = newToggle;
				count+=1;
			}
			System.Array.Sort( allToggles,
				(a,b) => { return a.name.CompareTo( b.name); });
			for(int i=0; i< allToggles.Length; i++)
			{
				allToggles[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(5, -20*i -25);
			}
			labelTogglePanel.GetComponent<RectTransform>().sizeDelta = new Vector2(140, 50+count*20);
			firstTime = false;
		}
		UpdateFontSizeDisplay();
	}

	private bool CheckGUISettings(string groupName){
		if (groupName == "Spacecraft"){
			return VizardGUISettings.ShowSpacecraftLabels;
		}else if(groupName == "Effectors"){
			return VizardGUISettings.ShowEffectorLabels;
		}else if (groupName == "CelestialBodies"){
			return VizardGUISettings.ShowCelestialBodyLabels;
		}else if(groupName == "Cameras"){
			return VizardGUISettings.ShowCameraLabels;
		}else if(groupName == "CoordinateSystems"){
			return VizardGUISettings.ShowCSLabels;
		}else if(groupName == "Thrusters"){
			return VizardGUISettings.ShowThrusterLabels;
		}else if(groupName == "ReactionWheels"){
			return VizardGUISettings.ShowRWLabels;
		}else if(groupName == "CoarseSunSensors"){
			return VizardGUISettings.ShowCSSLabels;
		}else if(groupName == "Locations"){
				return VizardGUISettings.ShowLocationLabels;
		}else if(groupName == "GenericSensors"){
			return VizardGUISettings.ShowGenericSensorLabels;
		}else if(groupName == "Transceivers"){
			return VizardGUISettings.ShowTransceiverLabels;
		}else if (groupName == "Lights"){
			return VizardGUISettings.ShowLightLabels;
		}else if (groupName == "MultiShapes"){
			return VizardGUISettings.ShowMSMLabels;
		}else if (groupName == "QuadMaps"){
			return VizardGUISettings.ShowQuadMapLabels;
		}else{
			Debug.LogFormat("Unexpected label group name: {0}.", groupName);
			return false;
		}
	}

	public void IncreaseFontSize(){
		LabelMaker.FontSize+=1;
		UpdateFontSizeDisplay();
		LabelMaker.ChangeFontSize(0f, LabelMaker.FontSize);
	}

	public void DecreaseFontSize(){
		LabelMaker.FontSize-=1;
		UpdateFontSizeDisplay();
		LabelMaker.ChangeFontSize(0f, LabelMaker.FontSize);
	}

	public void UpdateFontSizeDisplay(float changedScale = 0f){
		if (changedScale !=0f){
			currentGUIScale = changedScale;
		}
		int targetedFontSize = (int) ((float) LabelMaker.FontSize/currentGUIScale);
		fontSizeText.text = string.Format("Font Size: {0} pt", targetedFontSize);
	}
}
