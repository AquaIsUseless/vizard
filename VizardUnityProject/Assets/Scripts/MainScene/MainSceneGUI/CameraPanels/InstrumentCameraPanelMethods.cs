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
/// Displays Instrument Camera view from
/// its assigned VizMessage.Cameras settings
/// and allows users to request screen shots
/// </summary>
public class InstrumentCameraPanelMethods : MonoBehaviour
{
	public GameObject myDisplayImage;
	public GameObject myOutputImage;
	public GameObject myCamera;
	public Button closePanelButton;
	public Toggle panelToggle;

	public Button takeScreenshotButton;
	public TextMeshProUGUI panelName;
	public TextMeshProUGUI displayImageDimensions;
	public TextMeshProUGUI outputImageDimensions;

	public bool isTestCamera;
	public bool takePicture;

	void Update()
	{
		if (takePicture)
		{
			CaptureCameraImage();
			takePicture = false;
		}
	}

	// Start is called before the first frame update
    void Start()
    {
	    closePanelButton.onClick.AddListener(ClosePanel);
	    if (DataManager.UseVR)
	    {
		    closePanelButton.gameObject.layer = VizardGUISettings.VRUIRaycastLayer;
		    takeScreenshotButton.gameObject.layer = VizardGUISettings.VRUIRaycastLayer;
		    transform.GetChild(0).gameObject.layer = VizardGUISettings.VRUIRaycastLayer; //Drag Bar

	    }
		takeScreenshotButton.onClick.AddListener (CaptureCameraImage);
		transform.eulerAngles = Vector3.zero;

		if (isTestCamera)
		{
			SetupPanel("Test", myCamera, null);
		}
    }

	public void SetupPanel(string cameraName, GameObject instCamera, GameObject toggle){
		panelName.text = cameraName;
		myCamera = instCamera;
		if (!isTestCamera)
		{
			panelToggle = toggle.GetComponent<Toggle>();
		}

		instCamera.GetComponent<InstrumentCameraMethods>().myPanelTexture=myDisplayImage;
		instCamera.GetComponent<InstrumentCameraMethods>().myOutputTexture = myOutputImage;
		int reqWidth = instCamera.GetComponent<InstrumentCameraMethods>().reqWidth;
		int reqHeight = instCamera.GetComponent<InstrumentCameraMethods>().reqHeight;

		myOutputImage.SetActive(true);
		myOutputImage.GetComponent<CameraViewImageMethods>().InitializeCameraViewImage(myCamera.GetComponent<Camera>(), false,  reqWidth, reqHeight, 24);
		outputImageDimensions.text = $"{reqWidth} x {reqHeight}";
		myOutputImage.SetActive(false);
		GetComponentInChildren<ResizePanel>().maxSize= new Vector2(reqWidth+10, reqHeight+75);

		ApplyPanelResize(GetComponent<RectTransform>().sizeDelta);
	}

	private void ClosePanel()
	{
		if ((!VizardGUISettings.ShowCamPreviews)&&(!isTestCamera))
		{
			panelToggle.isOn = false;
		}
		gameObject.SetActive(false);
	}

	private void CaptureCameraImage ()
	{
		//To capture from on the screen display image
		//myDisplayImage.GetComponent<CameraViewImageMethods> ().CaptureScreenshot (myCamera.name);
		//To capture the user specified size image and save to file:
		myCamera.GetComponent<InstrumentCameraMethods>().CaptureImageFromOutputTexture(true);
	}

	public void UpdateDisplayImageDimensions(int width, int height){
		displayImageDimensions.text = $"{width} x {height}";
	}

	/// <summary>
	///This method must be implemented for any subpanel component that needs to do something when the panel is resized
	/// Do not delete or make private.
	/// </summary>
	/// <param name="newPanelDimensions">new panel extents</param>
	public void ApplyPanelResize(Vector2 newPanelDimensions){
		Vector2 displayDims= new Vector2(newPanelDimensions.x-10, newPanelDimensions.y-75);
		myDisplayImage.GetComponent<CameraViewImageMethods> ().InitializeCameraViewImage (myCamera.GetComponent<Camera>(), true, (int) displayDims.x, (int) displayDims.y);
	}
}
