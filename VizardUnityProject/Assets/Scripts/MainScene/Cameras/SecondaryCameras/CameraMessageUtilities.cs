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
/// <summary>
/// Static class providing methods to load textures, normal maps, and the camera configuration for
/// a given camera ID (not necessarily the message index of the camera)
/// </summary>
public static class CameraMessageUtilities{
	
	public static readonly int MinTextureDimension = 1; //global minimum texture size
	public static readonly int MaxTextureDimension = 8192; //global maximum texture size <- this is probably too big to actually be used.
	public static readonly int DefaultCameraDepth = 24;
	
	public static VizMessage.Types.CameraConfig GetCurrentCameraMessage(int cameraID)
	{
		foreach (VizMessage.Types.CameraConfig thisConfig in MessageList.CurrentMessage.Cameras)
		{
			if (thisConfig.CameraID == cameraID){
				return thisConfig;
			}
		}
		return null;
	}

	public static VizMessage.Types.CameraConfig GetCameraSetup(int cameraID){
		foreach (VizMessage.Types.CameraConfig thisConfig in MessageList.CurrentMessage.Cameras)
		{
			if (thisConfig.CameraID == cameraID){
				return thisConfig;
			}
		}
		//If can't find a configuration message in the current message, try the first message
		foreach (VizMessage.Types.CameraConfig thisConfig in MessageList.FirstMessage.Cameras)
		{
			if (thisConfig.CameraID == cameraID){
				VizardGUISettings.UpdateErrorMessages(
					$"Requested vizMessage camera message with ID: {cameraID} could not be found in Message: {MessageList.CurrentIndex}, but was found in the first message.");

				return thisConfig;
			}
		}
		
		VizardGUISettings.UpdateErrorMessages(
			$"Requested vizMessage camera message with ID: {cameraID} could not be found in messages.");
		return null;
	}

	public static string GetCameraParentName(int cameraID){
		VizMessage.Types.CameraConfig desiredCamConfig = GetCameraSetup(cameraID);
		return desiredCamConfig.ParentName;
	}

	public static Texture2D LoadTextureImage(string filePath) {

		Texture2D tex = null;

		if (File.Exists (filePath)) {
			byte[] fileData = File.ReadAllBytes (filePath);
			tex = new Texture2D (2, 2);
			tex.LoadImage (fileData); //..this will auto-resize the texture dimensions.

		} else {
			VizardGUISettings.UpdateErrorMessages($"Custom texture {filePath} not found.");
		}
		return tex;
	}

	public static Texture2D LoadNormalMap(string filePath){
		//First load in the normal map as a regular texture:
		Texture2D loadedTexture = LoadTextureImage(filePath);

		//Now convert to Unity format Normal map
		Texture2D normalMap = NormalMapToUnityFormat(loadedTexture);

		return normalMap;
	}

	private static Texture2D NormalMapToUnityFormat(Texture2D aTexture) {
		// This method from: https://orbcreation.com/cgi-bin/orbcreation/page.pl?1180
		// which is based on: https://answers.unity.com/questions/47121/runtime-normal-map-import.html
		Texture2D normalTexture = new Texture2D(aTexture.width, aTexture.height, TextureFormat.ARGB32, aTexture.mipmapCount > 1);
		Color[] pixels = aTexture.GetPixels(0);
		Color[] nPixels = new Color[pixels.Length];
		for (int y=0; y<aTexture.height; y++) {
			for (int x=0; x<aTexture.width; x++) {
				Color p = pixels[y * aTexture.width + x];
				p.b = p.g;
				p.a = p.r;  
				p.r = p.g;
				nPixels[(y * aTexture.width) + x] = p;
			}
		}
		normalTexture.SetPixels(nPixels, 0);
		normalTexture.Apply(true);
		return normalTexture;
	}
	
}
