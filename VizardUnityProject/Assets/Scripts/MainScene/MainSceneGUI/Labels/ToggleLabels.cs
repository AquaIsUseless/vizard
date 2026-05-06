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
/// Handles user input to the Label toggle panel to turn groups of labels on or off
/// or change the global size of text in labels. 
/// </summary>
public class ToggleLabels : MonoBehaviour
{
	private GameObject myLabelsHolder;
	private string labelGroup;
	private bool checkParentActive=true;

	public void SetupToggleForLabelGroup(GameObject holder){
		myLabelsHolder = holder;
		//check user settings
		labelGroup = holder.name;
		GetComponent<Toggle>().onValueChanged.AddListener(ToggleAllLabelsInGroup);

		if ((labelGroup == "CelestialBodies")||(labelGroup=="Spacecraft")||(labelGroup=="Cameras")||(labelGroup=="Effectors")){
			checkParentActive = false;
		}
	}

	private void ToggleAllLabelsInGroup(bool isOn){
		foreach(Transform label in myLabelsHolder.transform){
			GameObject controllingParent = label.gameObject.GetComponent<ObjectLabel>().targetTransform.gameObject;
			if (checkParentActive){
				controllingParent = controllingParent.transform.parent.gameObject;
			}
			if (controllingParent.activeInHierarchy){
				label.gameObject.SetActive(isOn);
			}
		}
		if (labelGroup=="Spacecraft"){
			VizardGUISettings.ShowSpacecraftLabels = isOn;
		}else if (labelGroup == "Effectors"){
			VizardGUISettings.ShowEffectorLabels = isOn;
		}else if(labelGroup == "CelestialBodies"){
			VizardGUISettings.ShowCelestialBodyLabels = isOn;
		}else if(labelGroup == "Cameras"){
			VizardGUISettings.ShowCameraLabels = isOn;
		}else if (labelGroup == "CoordinateSystems"){
			VizardGUISettings.ShowCSLabels = isOn;
		}else if (labelGroup == "Thrusters"){
			VizardGUISettings.ShowThrusterLabels = isOn;
		}else if (labelGroup == "ReactionWheels"){
			VizardGUISettings.ShowRWLabels = isOn;
		}else if (labelGroup == "CoarseSunSensors"){
			VizardGUISettings.ShowCSSLabels = isOn;
		}else if (labelGroup == "Locations"){
			VizardGUISettings.ShowLocationLabels = isOn;
		}else if (labelGroup == "GenericSensors"){
			VizardGUISettings.ShowGenericSensorLabels = isOn;
		}else if (labelGroup == "Transceivers"){
			VizardGUISettings.ShowTransceiverLabels = isOn;
		}else if (labelGroup == "Lights"){
			VizardGUISettings.ShowLightLabels = isOn;
		}else if (labelGroup == "MultiShapes") {
			VizardGUISettings.ShowMSMLabels = isOn;
		}else if (labelGroup == "QuadMaps") {
			VizardGUISettings.ShowQuadMapLabels = isOn;
		}else {
			Debug.Log(labelGroup + " is not a handled label group. Fix this.");
		}

		if ((VizardGUISettings.ShowSpacecraftLabels)|(VizardGUISettings.ShowCameraLabels)|(VizardGUISettings.ShowCSLabels)|(VizardGUISettings.ShowThrusterLabels)|(VizardGUISettings.ShowRWLabels)||(VizardGUISettings.ShowCSSLabels)||(VizardGUISettings.ShowGenericSensorLabels)||(VizardGUISettings.ShowTransceiverLabels)||(VizardGUISettings.ShowLightLabels)||(VizardGUISettings.ShowMSMLabels)){
			VizardGUISettings.SomeSpacecraftLabelsAreOn=true;
		}else{
			VizardGUISettings.SomeSpacecraftLabelsAreOn=false;
		}
		if ((VizardGUISettings.ShowCelestialBodyLabels)|(VizardGUISettings.ShowCSLabels)){
			VizardGUISettings.SomeCelestialBodyLabelsAreOn=true;
		}else{
			VizardGUISettings.SomeCelestialBodyLabelsAreOn=false;
		}
	}

}
