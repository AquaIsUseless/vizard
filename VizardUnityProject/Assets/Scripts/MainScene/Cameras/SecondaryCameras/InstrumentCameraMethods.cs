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
using System.IO;
using UnityEngine;
using VizProtobufferMessage;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.ResourceManagement.AsyncOperations;
/// <summary>
/// Controls the attached instrument camera and takes
/// images for screenshots or for streaming on command.
/// </summary>
public class InstrumentCameraMethods : MonoBehaviour {
	
	public int cameraID;
	public GameObject myAttachedBody;
	public SecondaryCameraHUDMethods secondaryCameraHUD;

	public float fov;
	public int reqWidth;
	public int reqHeight;

	private Vector3 cameraPosition;
	private Quaternion cameraOrientation;

	private double renderRate;

	public Camera myCamera;
	public GameObject myPanelTexture;
	public GameObject myOutputTexture;
	public bool usingCustomSkyboxForCamera;
	private int maxTextureDimension = 8192; //Update this if larger max texture is required
	private readonly int minTextureDimension = 1;
	
	private AsyncOperationHandle<Material> skyboxHandle;

	public PostProcessVolume postProcessVolume;
	private DepthOfField dof;
	public PostProcessResources postProcessResources;
	private bool updateConfig;
	private float scLocalScaleInUse = 1f;

	private bool takeDepthImage;
	private float nearClipPlane = 0.1f;
	private float farClipPlane = 100.0f;
	private bool isInstrumentCamera;
	private bool firstUpdate = true;

	public bool isTestCamera;
	public bool takeTestImage;

	void Awake()
	{
		secondaryCameraHUD = GetComponent<SecondaryCameraHUDMethods>();
		MainCameraUtilities.SecondaryCameras.Add(myCamera);
		if (isTestCamera)
		{
			reqWidth = 128;
			reqHeight = 128;
			farClipPlane = 10f;
			takeDepthImage = true;
			isInstrumentCamera = true;
		}
		
	}

	void Update()
	{
		if (isInstrumentCamera) //otherwise is being used as AdjustModelPanel's camera and does not require update
		{
			if (firstUpdate&&DataManager.UseVR)
			{
				RectTransform myPanelTextureParent = myPanelTexture.transform.parent.GetComponent<RectTransform>();
				Vector3 panelPosition = myPanelTextureParent.localPosition;
				panelPosition.z = 12f;
				myPanelTextureParent.localPosition = panelPosition;

				RectTransform camImage = myPanelTexture.GetComponent<RectTransform>();
				Vector3 imagePos = camImage.localPosition;
				imagePos.z = -1f;
				camImage.localPosition = imagePos;
				firstUpdate = false;
			}

			if (!isTestCamera)
			{
				VizMessage.Types.CameraConfig currentConfig = CameraMessageUtilities.GetCurrentCameraMessage(cameraID);
				if ((currentConfig != null) && (currentConfig.UpdateCameraParameters))
				{
					ConfigureInstrumentCamera(currentConfig);
				}
			}


			if (takeDepthImage)
			{
				myCamera.farClipPlane = farClipPlane * scLocalScaleInUse;
				myCamera.nearClipPlane = nearClipPlane * scLocalScaleInUse;
			}
			
			if (takeTestImage)
			{

				CaptureImageFromOutputTexture(true, true); //Time to take a picture
				takeTestImage = false;
			}

			if (!DataManager.InNoDisplayMode)
			{
				//Don't use the render rate in op-nav mode, even if it's set - this may need to be readdressed later
				if (renderRate > 0)
				{
					if (MessageList.CurrentMessage.CurrentTime.SimTimeElapsed % renderRate == 0)
					{
						CaptureImageFromOutputTexture(true); //Time to take a picture
						takeTestImage = false;
					}
				}
			}

			if ((VizardGUISettings.AssetLoadingComplete)&&(AtomicImageBuffer.IsRequestPending))
			{
				if (AtomicImageBuffer.CameraID == cameraID)
				{
					CaptureImageFromOutputTexture(false);
				}
			}
		}
	}

	public void CaptureImageFromOutputTexture(bool saveToFile, bool isDepthTestImage=false){
		myOutputTexture.SetActive(true);
		myPanelTexture.SetActive(false);
		myOutputTexture.GetComponent<CameraViewImageMethods>().CommandSourceCamera();
		if (saveToFile)
		{
			string nameToUse = myCamera.name;
			myOutputTexture.GetComponent<CameraViewImageMethods>().CaptureScreenshot (nameToUse, "none", isDepthTestImage);
		}else{//send over socket
			myOutputTexture.GetComponent<CameraViewImageMethods>().CaptureScreenshotToBuffer ();
		}
		myPanelTexture.SetActive(true);
		myOutputTexture.SetActive(false);
		myPanelTexture.GetComponent<CameraViewImageMethods>().CommandSourceCamera();
	}

	public void ConfigureInstrumentCamera(VizProtobufferMessage.VizMessage.Types.CameraConfig thisConfigMessage){
		if (thisConfigMessage != null)
		{
			isInstrumentCamera = true;
			cameraID = (int) thisConfigMessage.CameraID;

			transform.gameObject.name = "InstrumentCamera" + cameraID;
			//Set the camera field of view to what was requested, not sure this will end up doing anything if you are using sensor size and focal length, it may be overwritten.
			fov = (float) thisConfigMessage.FieldOfView;
			if (fov < 0.0001)
			{
				fov = 0.0001f;
				Debug.Log("Camera is limited by the visualization's code to a minimum FOV of 0.0001 degrees.");
			}
			else if (fov >= 180)
			{
				fov = 179;
				Debug.Log("Camera is limited by the visualization's code to a maximum FOV of 179.999 degrees.");
			}

			myCamera.fieldOfView = fov;

			reqWidth = Mathf.Clamp((int) thisConfigMessage.Resolution[0], minTextureDimension, maxTextureDimension);
			reqHeight = Mathf.Clamp((int) thisConfigMessage.Resolution[1], minTextureDimension, maxTextureDimension);

			//Camera position is in mm so divide by 1000 to get it into meters
			// also have to perform the transformation to Left-handed Unity frame!
			cameraPosition = new Vector3((float) thisConfigMessage.CameraPosB[1],
				(float) thisConfigMessage.CameraPosB[2], (float) -thisConfigMessage.CameraPosB[0]);
			//Camera orientation provided in MRP, convert to quaternion in left-handed unity frame
			double[] providedCameraMRP = new double[] {thisConfigMessage.CameraDirB[0],
				thisConfigMessage.CameraDirB[1], thisConfigMessage.CameraDirB[2]};
			
			cameraOrientation = OrbitVectorMath.ConvertRightHandedMRPtoLeftHandedQuaternion(providedCameraMRP);

			renderRate = thisConfigMessage.RenderRate;
			string skyboxPath = thisConfigMessage.Skybox;
			if ((skyboxPath != "")&&(skyboxPath != "NASA_SVS"))
			{
				ApplySkyboxSetting(skyboxPath);
			}

			if (thisConfigMessage.ParentName == "")
			{
				string errMsg = $"No parent body name was provided for Custom Camera {thisConfigMessage.CameraID}.Check the CameraConfig message";
				VizardGUISettings.UpdateErrorMessages(errMsg, true);

			}
			
			VizardGUISettings.SetSecondaryCameraLayerMask(myCamera, thisConfigMessage.ShowHUDElementsInImage==1);
			PositionCameraRelativeToSpacecraft(thisConfigMessage.ParentName);
			
			if (thisConfigMessage.PostProcessingOn == 1)
			{
				ApplyPostProcessingSettings(thisConfigMessage);
			}

			ApplyDepthModeSettings(thisConfigMessage);
		}
	}

	private void ApplySkyboxSetting(string skyboxPath)
	{
		usingCustomSkyboxForCamera = true;
		if (skyboxPath.ToLower() == "black")
		{
			//Apply the black skybox
			myCamera.clearFlags = CameraClearFlags.SolidColor;
			myCamera.backgroundColor = Color.black;
			ReleaseHandle();
		}
		else if ((skyboxPath == "ESO")||(skyboxPath == "ESO Milky Way"))
		{
			myCamera.clearFlags = CameraClearFlags.Skybox;
			UseAddressableSkyboxTexture("MilkyWay");
		}
		else if (skyboxPath == "NASA_SVS")
		{
			Material newMaterial = Instantiate(Resources.Load("Materials/Skyboxes/NASA_SVS_StarMap") as Material);
			myCamera.clearFlags = CameraClearFlags.Skybox;
			myCamera.GetComponent<Skybox> ().material = newMaterial;
			ReleaseHandle();
		}
		else
		{
			string pathToTry = skyboxPath;
			if ((!DataManager.IsLiveSim)&&(pathToTry.StartsWith(".")))
			{
				pathToTry = Path.GetFullPath(pathToTry, Path.GetDirectoryName(DataManager.FilePath));
			}
			Texture2D newTex = CameraMessageUtilities.LoadTextureImage(pathToTry);
			if (newTex != null)
			{
				Material newMaterial = Instantiate(Resources.Load("Materials/Skyboxes/InstrumentCamera") as Material);
				newMaterial.mainTexture = newTex;
				myCamera.clearFlags = CameraClearFlags.Skybox;
				myCamera.GetComponent<Skybox>().material = newMaterial;
			}
		}
	}

	private void ApplyDepthModeSettings(VizMessage.Types.CameraConfig thisConfigMessage)
	{
		if (thisConfigMessage.RenderMode == 1)
		{
			takeDepthImage = true;
			GetComponent<EnableDepthLayer>().enabled = true;
			//myDepthLens.SetActive(true);
			if (thisConfigMessage.DepthMapClippingPlanes.Count >= 2)
			{
				float proposedNearClip = (float) thisConfigMessage.DepthMapClippingPlanes[0];
				float proposedFarClip = (float) thisConfigMessage.DepthMapClippingPlanes[1];
				
				if ((proposedNearClip > proposedFarClip) || (proposedNearClip <= 0) || (proposedFarClip <= 0))
				{
					VizardGUISettings.UpdateErrorMessages(
						$"Instrument Camera {cameraID} depth map clipping planes must be non-zero and must be in order of near to far in message.");
				}else
				{
					nearClipPlane = proposedNearClip;
					farClipPlane = proposedFarClip;
				}
			}
		}
	}

	private void PositionCameraRelativeToSpacecraft(string spacecraftName){
		//Find the spacecraft manager so that the spacecraft GameObject list can be accessed when changing camera target
		if (spacecraftName == "")
		{
			spacecraftName = MessageList.CurrentMessage.Spacecraft[0].SpacecraftName;
		}
		myAttachedBody = SpacecraftStateUtilities.GetSpacecraftObject(spacecraftName);
		if (myAttachedBody == null){
			myAttachedBody = SpacecraftStateUtilities.SpacecraftList[0];
		}
		secondaryCameraHUD.GetAttachedBodyMeshDimensionExtent(myAttachedBody);
		if (!updateConfig)
		{
			//Find the spacecraft manager so that the spacecraft GameObject list can be accessed when changing camera target
			VizardGUISettings.PanelViewMgr.CreateInstrumentCameraToggleAndPanel(myAttachedBody, transform.gameObject);
			updateConfig = true;
		}

		transform.SetParent (myAttachedBody.transform);
		transform.localScale = Vector3.one;
		transform.localPosition = cameraPosition;
		transform.localRotation = cameraOrientation;//Per Thibaud Teil's direction via Slack 6/26/19, apply the custom camera rotation relative to the spacecraft
	}
	

	
	private void UseAddressableSkyboxTexture(string address){
		skyboxHandle = Addressables.LoadAssetAsync<Material>(address);
		skyboxHandle.Completed += SkyboxHandleLoaded;

	}

	private void SkyboxHandleLoaded(AsyncOperationHandle<Material> operation)
	{
		if (operation.Status == AsyncOperationStatus.Succeeded)
		{
			myCamera.clearFlags = CameraClearFlags.Skybox;
			myCamera.GetComponent<Skybox>().material = Instantiate(operation.Result);
		}
		else
		{
			Debug.Log($"Asset for skybox material failed to load.");
		}
	}

	private void ReleaseHandle()
		{
			if (skyboxHandle.IsValid())
			{
				Addressables.Release(skyboxHandle);
			}
		}
	private void OnDestroy(){
		{
			ReleaseHandle();
		}
	}

	private void ApplyPostProcessingSettings(VizProtobufferMessage.VizMessage.Types.CameraConfig thisConfigMessage)
	{
		// double ppFocusDistance = 12; // (Optional) Distance to the point of focus, minimum value of 0.1, Value of 0 to leave this option off
		// double ppAperture = 13; //  (Optional) Ratio of the aperture (known as f-stop or f-number). The smaller the value is, the shallower the depth of field is. Valid Setting Range: 0.05 to 32. Value of 0 to leave this option off.
		// double ppFocalLength = 14; // [mm] (Optional) Value of -1 to calculate the focal length automatically from the field-of-view value set on the camera, otherwise provide positive non-zero value in range. Valid setting range: 1mm to 300mm. Value of 0 to leave this option off.
		// int64 ppMaxBlurSize = 15; //(Optional) Convolution kernel size of the bokeh filter, which determines the maximum radius of bokeh. It also affects the performance (the larger the kernel is, the longer the GPU time is required). Depth textures Value of 1 for Small, 2 for Medium, 3 for Large, 4 for Extra Large. Value of 0 to leave this option off.

		GameObject postProcessVolumeObject = VizardGUISettings.PostProcessingMgr.GetPostProcessingVolume(cameraID);
		if (postProcessVolumeObject != null)
		{
			scLocalScaleInUse = (float)CelestialBodyStateUtilities.SpacecraftLocalViewScale;
			postProcessVolume = postProcessVolumeObject.GetComponent<PostProcessVolume>();

			postProcessVolume.enabled = true;
			postProcessVolume.profile.TryGetSettings(out dof);
			PostProcessLayer postProcessLayer = myCamera.transform.gameObject.GetComponent<PostProcessLayer>();
			postProcessLayer.enabled = true;
			postProcessLayer.volumeLayer = LayerMask.GetMask(postProcessVolume.name);
			postProcessLayer.Init(postProcessResources);
			//dof.focusDistance.overrideState = true;
			
			if (thisConfigMessage.PpFocusDistance < 0.1)
			{
				dof.focusDistance.value = 10;
				dof.focusDistance.overrideState = false;
			}
			else
			{
				dof.focusDistance.overrideState = true;
				dof.focusDistance.value = (float) thisConfigMessage.PpFocusDistance;
			}
			

			if (thisConfigMessage.PpAperture < 0.05)
			{
				dof.aperture.value = 5.6f;
				dof.aperture.overrideState = false;
			}
			else
			{
				dof.aperture.overrideState = true;
				dof.aperture.value = (float) thisConfigMessage.PpAperture;
			}

			if (thisConfigMessage.PpFocalLength < 1)
			{
				dof.focalLength.value = 50f;
				dof.focalLength.overrideState = false;
			}
			else
			{
				dof.focalLength.overrideState = true;
				dof.focalLength.value = (float) thisConfigMessage.PpFocalLength;
			}

			int blurSize = (int) thisConfigMessage.PpMaxBlurSize;
			if (blurSize <= 0)
			{
				dof.kernelSize.value = KernelSize.Medium;
				dof.kernelSize.overrideState = false;
			}
			else
			{
				dof.kernelSize.overrideState = true;
				if (blurSize == 1)
				{
					dof.kernelSize.value = KernelSize.Small;
				}
				else if (blurSize == 3)
				{
					dof.kernelSize.value = KernelSize.Large;
				}
				else if (blurSize == 4)
				{
					dof.kernelSize.value = KernelSize.VeryLarge;
				}
				else
				{
					dof.kernelSize.value = KernelSize.Medium;
				}
			}
			// Multiply focal distance and focal length by this value,
			// aperture (f-stop) is ratio of focal length/lens diameter and is unitless and need not be scaled
			dof.focalLength.value *= scLocalScaleInUse;
			dof.focusDistance.value *= scLocalScaleInUse;

		}
	}

	public void UpdateCameraParametersForScaleChange()
	{
		scLocalScaleInUse = (float) CelestialBodyStateUtilities.SpacecraftLocalViewScale;
	}
}
