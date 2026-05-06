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
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using static System.IO.File;
/// <summary>
/// Sets up the camera render texture for a standard or instrument
/// cameras, captures screen shots to file or buffer (streaming),
/// and panel resizing.
/// </summary>
public class CameraViewImageMethods : MonoBehaviour {
	private Camera sourceCamera;
	private RenderTexture cameraRenderTexture;
	private int imageWidth; 
	private int imageHeight;
	private int myDepth;
	private bool allowTextureResize = true;
	private bool isCameraPanel=true;
	
	public void InitializeCameraViewImage(Camera myCamera, bool textureResizable = true, int texWidth = 190, int texHeight = 170, int texDepth = 16, bool isCamPanel = true){
		sourceCamera = myCamera;
		imageWidth = texWidth;
		imageHeight = texHeight;
		myDepth = texDepth;
		allowTextureResize = textureResizable;
		isCameraPanel = isCamPanel;
		GetComponent<RectTransform>().sizeDelta = new Vector2(imageWidth, imageHeight);

		//Create the textures that the cameras will render to in the panels
		cameraRenderTexture = new RenderTexture (imageWidth, imageHeight, myDepth, RenderTextureFormat.ARGB32); //, RenderTextureFormat.ARGB32);
		cameraRenderTexture.Create ();

		//Assign the textures to their respective cameras
		sourceCamera.targetTexture=cameraRenderTexture;
		
		//Set the cameraImage texture to the camera render texture
		transform.GetComponentInParent<RawImage>().texture=cameraRenderTexture;
	}

	public void CommandSourceCamera(){
		//Assign the textures to their respective cameras
		sourceCamera.targetTexture=cameraRenderTexture;

		//Set the cameraImage texture to the camera render texture
		transform.GetComponentInParent<RawImage>().texture=cameraRenderTexture;
	}
	/// <summary>
	///This method must be implemented for any subpanel component that needs to do something when the panel is resized
	/// Do not delete or make private.
	/// </summary>
	/// <param name="newPanelDimensions">new panel extents</param>
	public void ApplyPanelResize(Vector2 newPanelDimensions){
			imageWidth = (int)transform.gameObject.GetComponent<RectTransform>().rect.width;
			imageHeight = (int)transform.gameObject.GetComponent<RectTransform>().rect.height;

		if (allowTextureResize) {
			InitializeCameraViewImage (sourceCamera, true, imageWidth, imageHeight, myDepth, isCameraPanel);
		}
	}

	//This method is just to allow opNav user to reset the output texture's dimensions from the camera config message
	public void ApplyTextureResize(Vector2 newTextureDimensions){
		imageWidth = (int)newTextureDimensions[0];
		imageHeight = (int)newTextureDimensions[1];
		if (allowTextureResize){
			InitializeCameraViewImage(sourceCamera,true, imageWidth, imageHeight, myDepth);
		}
	}

	public void CaptureScreenshot(string cameraName, string fileName = "none", bool inDepthTestMode=false){
		//Helpful code bits from: https://answers.unity.com/questions/37134/is-it-possible-to-save-rendertextures-into-png-fil.html
		DateTime renderStart = DateTime.Now;
		sourceCamera.Render ();
		DateTime renderEnd = DateTime.Now;
		RenderTexture.active = cameraRenderTexture; 
		Texture2D screenshot = new Texture2D (imageWidth, imageHeight, TextureFormat.RGB24, false);
		screenshot.ReadPixels (new Rect(0,0, imageWidth, imageHeight),0,0);

		if (inDepthTestMode)
		{
			Debug.Log("I am running the depth test.");
			SetGUIText resultText = GameObject.Find("TestOutput").GetComponent<SetGUIText>();
			Color[] checkColors = screenshot.GetPixels();
			bool allChecksPassed = true;
			for (int i = 0; i < checkColors.Length; i++)
			{
				if (checkColors[i] != Color.white)
				{
					Debug.Log($"{i}: {checkColors[i].r}, {checkColors[i].g}, {checkColors[i].b}, {checkColors[i].a}");
				}
			}

			float testCamFarClippingPlane = 10f;
			Dictionary<int, float> testPixelsAndDepths = new Dictionary<int, float>()
			{
				{8210, 9.9999f}, {8220, 7.0000f}, {8230, 5.00000f}, {8235, 2.00000f}, {8243, 1.00000f},
				{8250, 0.50000f}, {8255, 0.11f}
			};
			foreach (int pixelToCheck in testPixelsAndDepths.Keys)
			{
				float expectedDepth = testPixelsAndDepths[pixelToCheck];
				float calculatedDepth = testCamFarClippingPlane*(checkColors[pixelToCheck].r + (checkColors[pixelToCheck].g / 255f) +
				                             (checkColors[pixelToCheck].b / (255f * 255f)));
				float difference = Mathf.Abs(expectedDepth - calculatedDepth);
				if (difference > 0.01)
				{
					string errorString =
						$"At pixel {pixelToCheck}, expect depth: {expectedDepth}, calculated depth: {calculatedDepth} -> difference greater than 0.01";
					Debug.Log(errorString);
					resultText.AddToText(errorString+"\n");
					allChecksPassed = false;
				}
			}

			if (allChecksPassed)
			{
				Debug.Log("All pixel color depth checks passed.");
				resultText.AddToText("All pixel color depth checks passed.");
			}
		}

		RenderTexture.active = null;
		sourceCamera.targetTexture = cameraRenderTexture;
		byte[] bytes = screenshot.EncodeToPNG ();

		string filename = fileName;
		if (filename == "none") {
			filename = ScreenShotName (cameraName, imageWidth, imageHeight, inDepthTestMode);

			if (!Directory.Exists(Path.GetDirectoryName(filename))){
				Directory.CreateDirectory(Path.GetDirectoryName(filename));
			}
		}

		WriteAllBytes(filename, bytes);
		DateTime endWrite = DateTime.Now;
		TimeSpan renderInterval = renderEnd-renderStart;
		TimeSpan saveInterval = endWrite - renderEnd;
		if (DataManager.SaveFPSMetricsToFile) 
			DataManager.SaveMetrics(renderInterval.TotalSeconds+", "+saveInterval.TotalSeconds);
		Destroy(screenshot);
		Debug.Log($"Took screenshot to: {filename}");
		
	}

	public void CaptureScreenshotToBuffer()
	{
		sourceCamera.Render ();
		RenderTexture.active = cameraRenderTexture; 
		Texture2D screenshot = new Texture2D (imageWidth, imageHeight, TextureFormat.RGB24, false);
		screenshot.ReadPixels (new Rect(0,0, imageWidth, imageHeight),0,0); 

		RenderTexture.active = null;

		sourceCamera.targetTexture = cameraRenderTexture;

		AtomicImageBuffer.LockBuffer();
		AtomicImageBuffer.ImageBuffer = screenshot.EncodeToPNG ();
		AtomicImageBuffer.ReleaseBuffer();
		AtomicImageBuffer.SignalScreenshotFulfilled();
		Destroy(screenshot);
	}

	private static string ScreenShotName(string cameraName, int width, int height, bool inDepthTestMode=false) {
		string filename;
		if (Application.isEditor)
		{
			filename = Application.dataPath;
		}
		else
		{
			filename = string.Format("{0}/{1}",
				Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "VizardData");
		}

		filename += $"/Screenshots/{cameraName}_{width}x{height}";

		if (inDepthTestMode)
		{
			filename += "_DEPTH_TEST.png";
		}
		else
		{
			filename += $"_{MessageList.CurrentMessage.CurrentTime.SimTimeElapsed}.png";
		}

		return filename;
	}

	public RenderTexture GetTargetTexture()
	{
		return cameraRenderTexture;
	}
}

