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
/// Connects a toggle to an object to toggle
/// </summary>
public class PanelToggle : MonoBehaviour {
	private Toggle myToggle;
	public GameObject panelToToggle;
	private Camera mainCamera;
	public GameObject cameraToToggle;

	void Start()
	{
		mainCamera = Camera.main;
		myToggle = GetComponent<Toggle> ();
		myToggle.onValueChanged.AddListener(TogglePanel);

		if (cameraToToggle!=null){
			myToggle.onValueChanged.AddListener(ToggleCameraObject);
		}

		if (panelToToggle.GetComponentInChildren<ClosePanelButton>()!=null){
			panelToToggle.GetComponentInChildren<ClosePanelButton>().SetMyToggle(transform.gameObject);
		}
	}

	public void TogglePanel(bool toggleValue){

		panelToToggle.gameObject.SetActive (toggleValue);
	}

	public void SetupCameraObjectToggle(GameObject cameraObject){
		cameraToToggle = cameraObject;
		myToggle = GetComponent<Toggle> ();
		myToggle.onValueChanged.AddListener (ToggleCameraObject);
	}

	private void ToggleCameraObject(bool toggleValue){
		cameraToToggle.gameObject.SetActive (toggleValue);
		cameraToToggle.gameObject.GetComponent<Camera>().enabled = toggleValue;
		if (toggleValue)
		{
			if (VizardGUISettings.SkyboxIsTexture)
			{
				cameraToToggle.GetComponent<Camera>().clearFlags = CameraClearFlags.Skybox;
				try
				{
					cameraToToggle.GetComponent<Skybox>().material = mainCamera.GetComponent<Skybox>().material;
				}
				catch
				{
					//Debug.Log("Main camera material not yet available.");
				}
			}
			else
			{
				cameraToToggle.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
				cameraToToggle.GetComponent<Camera>().backgroundColor = VizardGUISettings.SkyboxColor;
			}
		}
	}

	public void SetupCameraComponentToggle (GameObject cameraObject){
		cameraToToggle = cameraObject;
		myToggle = GetComponent<Toggle> ();
		myToggle.onValueChanged.AddListener (ToggleCameraComponent);
	}

	private void ToggleCameraComponent(bool toggleValue){
		cameraToToggle.GetComponent<Camera> ().enabled = toggleValue;
	}

}
