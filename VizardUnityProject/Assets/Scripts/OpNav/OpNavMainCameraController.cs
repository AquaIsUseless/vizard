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

using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using VizProtobufferMessage;

/// <summary>
/// Main camera controller for -noDisplay livestreaming
/// </summary>
public class OpNavMainCameraController : MonoBehaviour
{
    [Header("Scene")] [Tooltip("Skybox manager")]
    public SkyboxDropdown skyboxSetter;

    [Tooltip("Applies VizMessage.InstrumentCamera post-processing settings")]
    public PostProcessingManager postProcessingManager;

    //Post processing
    [Tooltip("Available Unity post processing effects")]
    public PostProcessResources postProcessResources; //Unity resource needed to enable post-processing effects

    private PostProcessVolume postProcessVolume; //Unity resource needed to enable post-processing effects
    private PostProcessLayer postProcessLayer; //Unity resource needed to enable post-processing effects

    //Main camera and world center object used as camera rig
    private Camera mainCamera; //Main camera in scene
    private GameObject worldCenterObject; //World center in op-nav no-display mode

    // Main Camera output 
    private RenderTexture cameraRenderTexture; //RenderTexture in which main camera rendering output is stored
    private int desiredTextureWidth; //[pixels] Output texture width
    private int desiredTextureHeight; //[pixels] Output texture height
    private float fieldOfView; //[degrees] Current Main Camera Field of View
    private DepthOfField depthOfField; //[Unity Units] Post-processing depth of field setting

    private readonly bool saveImageToFile = false; //True if saving a copy of image to file (for debugging)

    /// <summary>
    /// Create an empty game object to act as the camera rig
    /// for the main camera
    /// </summary>
    public void SetWorldCenterObject_NoDisplayMode()
    {
        mainCamera = GetComponent<Camera>();
        worldCenterObject = new GameObject
        {
            transform =
            {
                position = Vector3.zero
            }
        };
        mainCamera.transform.SetParent(worldCenterObject.transform);
    }

    /// <summary>
    /// Apply the current camera settings' parent spacecraft as the
    /// Main Camera target in spacecraft local view, forcing all other
    /// objects in the scene to be positioned relative to that spacecraft
    /// </summary>
    /// <param name="spacecraftName">Name of current camera's parent spacecraft</param>
    public void SetSpacecraftAsWorldCenterTarget_NoDisplayMode(string spacecraftName)
    {
        //Keep Vizard in spacecraft local view
        CelestialBodyStateUtilities.ViewIsLocal = true;
        CelestialBodyStateUtilities.ViewIsSpacecraftLocal = true;

        //Get the parent spacecraft for the current camera settings and set it to
        //the main camera target if it has changed
        if (MainCameraUtilities.CameraTarget.name != spacecraftName)
        {
            GameObject scBody = CelestialBodyStateUtilities.GetGameObjectWithBodyName(spacecraftName);
            int scIndex = scBody.GetComponent<SpacecraftController>().spacecraftIndex;
            MainCameraUtilities.CameraTarget = scBody;
            MainCameraUtilities.CameraTargetIndex = scIndex;
            MainCameraUtilities.CameraTargetIsSpacecraftOrEffector = true;
        }

        //Ensure world center camera rig stays at origin
        worldCenterObject.transform.position = Vector3.zero;
        //Rotate the world center camera rig to the orientation of the parent spacecraft
        worldCenterObject.transform.rotation =
            SpacecraftStateUtilities.GetSpacecraftOrientationUnityCS(MainCameraUtilities.CameraTargetIndex);
    }

    /// <summary>
    /// Configure the main camera to the instrument camera settings
    /// requested for the current image capture
    /// </summary>
    public void UpdateCameraAndCaptureImage()
    {
        //Debug.Log($"Capturing image for message {MessageList.CurrentIndex} for camera ID {AtomicImageBuffer.cameraID}");
        ConfigureMainCameraToInstrumentCamera(AtomicImageBuffer.CameraID);
        CaptureScreenshotToBuffer();
    }

    /// <summary>
    /// Configure the main camera to the most recent VizMessage's camera settings
    /// for the camera requested by cameraID in the AtomicImageBuffer image request
    /// </summary>
    /// <param name="instCameraID">camera ID of instrument camera for requested image</param>
    private void ConfigureMainCameraToInstrumentCamera(int instCameraID)
    {
        //Get the VizMessage.CameraConfig message for the requested instrument camera
        VizMessage.Types.CameraConfig thisConfigMessage = CameraMessageUtilities.GetCameraSetup(instCameraID);
        if (thisConfigMessage == null)
        {
            //If there was no configuration message included in the most recent message
            //use the camera configuration for that ID from the first VizMessage
            if (MessageList.FirstMessage.Cameras.Count > 0)
            {
                thisConfigMessage = MessageList.FirstMessage.Cameras[instCameraID];
                Debug.Log(
                    "REQUEST_IMAGE opNav message did not include the camera ID. The first instrument camera configuration message will be applied.");
            }
            else //No configuration available for requested camera ID
            {
                Debug.Log(
                    "No camera config messages were found in messages. Vizard cannot fulfill the REQUEST_IMAGE request.");
            }
        }

        //Apply the instrument camera configuration to the main camera
        if (thisConfigMessage != null)
        {
            skyboxSetter.ApplyUserSkyboxSettings(thisConfigMessage.Skybox);

            //Set the camera field of view to what was requested
            fieldOfView = (float) thisConfigMessage.FieldOfView;
            if (fieldOfView < 0.0001)
            {
                fieldOfView = 0.0001f;
                Debug.Log("Camera is limited by Vizard to a minimum FOV of 0.0001 degrees.");
            }
            else if (fieldOfView >= 180)
            {
                fieldOfView = 179.9999f;
                Debug.Log("Camera is limited by Vizard to a maximum FOV of 179.9999 degrees.");
            }

            mainCamera.fieldOfView = fieldOfView;

            //Set the output image width and height in pixels
            desiredTextureWidth = Mathf.Clamp((int) thisConfigMessage.Resolution[0],
                CameraMessageUtilities.MinTextureDimension, CameraMessageUtilities.MaxTextureDimension);
            desiredTextureHeight = Mathf.Clamp((int) thisConfigMessage.Resolution[1],
                CameraMessageUtilities.MinTextureDimension, CameraMessageUtilities.MaxTextureDimension);


            //Spacecraft relative camera position requires transformation to Left-handed Unity frame!
            transform.localPosition = new Vector3((float) thisConfigMessage.CameraPosB[1],
                (float) thisConfigMessage.CameraPosB[2], (float) -thisConfigMessage.CameraPosB[0]);

            //Camera orientation provided in MRP, convert to quaternion in left-handed unity frame
            double[] providedCameraMRP =
                {thisConfigMessage.CameraDirB[0], thisConfigMessage.CameraDirB[1], thisConfigMessage.CameraDirB[2]};
            transform.localRotation = OrbitVectorMath.ConvertRightHandedMRPtoLeftHandedQuaternion(providedCameraMRP);

            //Add post-processing settings, if enabled in current instrument camera settings
            if (thisConfigMessage.PostProcessingOn == 1)
            {
                if (postProcessVolume == null)
                {
                    ConnectPostProcessingToMainCamera();
                }

                postProcessVolume.enabled = true;
                postProcessLayer.enabled = true;


                if (thisConfigMessage.PpFocusDistance < 0.1)
                {
                    depthOfField.focusDistance.value = 10;
                    depthOfField.focusDistance.overrideState = false;
                }
                else
                {
                    depthOfField.focusDistance.overrideState = true;
                    depthOfField.focusDistance.value = (float) thisConfigMessage.PpFocusDistance;
                }

                if (thisConfigMessage.PpAperture < 0.05)
                {
                    depthOfField.aperture.value = 5.6f;
                    depthOfField.aperture.overrideState = false;
                }
                else
                {
                    depthOfField.aperture.overrideState = true;
                    depthOfField.aperture.value = (float) thisConfigMessage.PpAperture;
                }

                if (thisConfigMessage.PpFocalLength < 1)
                {
                    depthOfField.focalLength.value = 50f;
                    depthOfField.focalLength.overrideState = false;
                }
                else
                {
                    depthOfField.focalLength.overrideState = true;
                    depthOfField.focalLength.value = (float) thisConfigMessage.PpFocalLength;
                }

                int blurSize = (int) thisConfigMessage.PpMaxBlurSize;
                if (blurSize <= 0)
                {
                    depthOfField.kernelSize.value = KernelSize.Medium;
                    depthOfField.kernelSize.overrideState = false;
                }
                else
                {
                    depthOfField.kernelSize.overrideState = true;
                    if (blurSize == 1)
                    {
                        depthOfField.kernelSize.value = KernelSize.Small;
                    }
                    else if (blurSize == 3)
                    {
                        depthOfField.kernelSize.value = KernelSize.Large;
                    }
                    else if (blurSize == 4)
                    {
                        depthOfField.kernelSize.value = KernelSize.VeryLarge;
                    }
                    else
                    {
                        depthOfField.kernelSize.value = KernelSize.Medium;
                    }
                }
            }
        }
        else
        {
            Debug.Log("Custom camera config message was null.");
        }

        // Create a render texture of the desired dimensions
        InitializeOpNavCameraTexture();
    }

    /// <summary>
    /// Configure any post-processing requested for current image capture
    /// </summary>
    private void ConnectPostProcessingToMainCamera()
    {
        VizardGUISettings.PostProcessingMgr = postProcessingManager;
        postProcessVolume = postProcessingManager.GetPostProcessingVolume(0).GetComponent<PostProcessVolume>();
        postProcessLayer = mainCamera.transform.gameObject.AddComponent<PostProcessLayer>();
        postProcessLayer.volumeLayer = LayerMask.GetMask(postProcessVolume.name, "Spacecraft", "Gravity");
        postProcessLayer.Init(postProcessResources);
        postProcessVolume.profile.TryGetSettings(out depthOfField);
        postProcessVolume.enabled = false;
        postProcessLayer.enabled = false;
    }

    /// <summary>
    /// Create a render texture of the instrument camera settings
    /// specified width and height
    /// </summary>
    private void InitializeOpNavCameraTexture()
    {
        if (cameraRenderTexture == null)
        {
            cameraRenderTexture = new RenderTexture(desiredTextureWidth, desiredTextureHeight,
                CameraMessageUtilities.DefaultCameraDepth);
            cameraRenderTexture.Create();
        }
    }

    /// <summary>
    /// Capture the output of the main camera to the cameraRenderTexture
    /// and send it to Basilisk sim through AtomicImageBuffer
    /// </summary>
    /// <exception cref="InvalidOperationException">[Editor Testing Only] Path to save screenshot could not be created </exception>
    private void CaptureScreenshotToBuffer()
    {
        //Tell the main camera to render current scene to the cameraRenderTexture
        mainCamera.Render();

        //Set output of main camera to the cameraRenderTexture
        RenderTexture.active = cameraRenderTexture;

        //Create Texture 2D
        Texture2D screenshot =
            new Texture2D(desiredTextureWidth, desiredTextureHeight, TextureFormat.RGB24, false); //RGB24

        //read pixels from the current render texture and write them into the Texture2D
        screenshot.ReadPixels(new Rect(0, 0, desiredTextureWidth, desiredTextureHeight), 0, 0);

        // Stop sending render texture commands to the cameraRenderTexture
        RenderTexture.active = null;

        //Restore output of the main camera to cameraRenderTexture
        mainCamera.targetTexture = cameraRenderTexture;

        //Prepare to stream image to Basilisk
        AtomicImageBuffer.LockBuffer();

        //Enable this if you want to save a file copy of the
        //rendered image about to be streamed to Basilisk
        if (saveImageToFile)
        {
            byte[] bytes = screenshot.EncodeToPNG();

            string filename = ScreenShotName($"Main{AtomicImageBuffer.CameraID}", desiredTextureWidth,
                desiredTextureHeight);
            if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(filename)))
            {
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filename) ??
                                                    throw new InvalidOperationException());
            }

            System.IO.File.WriteAllBytes(filename, bytes);
        }

        //Encode the image to PNG and send it through AtomicImageBuffer
        AtomicImageBuffer.ImageBuffer = screenshot.EncodeToPNG();

        //Release lock on AtomicImageBuffer
        AtomicImageBuffer.ReleaseBuffer();

        //Send message to Basilisk that the requested screenshot has been sent
        AtomicImageBuffer.SignalScreenshotFulfilled();

        //Release the Texture2D
        Destroy(screenshot);
    }

    /// <summary>
    /// Provide a name for the screenshot file
    /// </summary>
    /// <param name="cameraName">Name of the instrument camera taking the image</param>
    /// <param name="width">[pixels] Width of the requested image</param>
    /// <param name="height">[pixels] Height of the requested image</param>
    /// <returns></returns>
    private static string ScreenShotName(string cameraName, int width, int height)
    {
        return string.Format("{0}/Screenshots/{1}_{2}x{3}_{4}.png",
            Application.dataPath, cameraName,
            width, height,
            MessageList.CurrentMessage.CurrentTime.SimTimeElapsed);
    }
}