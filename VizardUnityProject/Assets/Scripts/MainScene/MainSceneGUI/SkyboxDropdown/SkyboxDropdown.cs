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
using TMPro;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
/// <summary>
/// Handles user input to the skybox selection dropdown
/// to change the current main camera skybox
/// and importing user supplied skybox textures
/// </summary>
public class SkyboxDropdown : MonoBehaviour {

	public TMP_Dropdown skyboxDropdown;
	public TextMeshProUGUI dropdownTitle;

	private List<string> skyboxNames = new List<string> {"Skybox","NASA SVS Star Map", "ESO Milky Way", "Black"};
	private Dictionary<string, string> skyboxOptions = new Dictionary<string, string>()
	{
		{"NASA SVS Star Map", "Materials/Skyboxes/NASA_SVS_StarMap"},
		{"ESO Milky Way", "MilkyWay"},
		{"Black", "solid"}
	};
	private AsyncOperationHandle<Material> skyboxHandle;

	void Start(){
		
		VizardGUISettings.PopulateList(skyboxDropdown, skyboxNames, dropdownTitle, "Skybox");
		skyboxDropdown.onValueChanged.AddListener (SkyboxDropdownValueChanged);
		SkyboxDropdownValueChanged(0);
		VizardGUISettings.CurrentSkybox = "NASA SVS Star Map";
		//skyboxList = new List<Material> (){ NASA_svs, milkyWay};
		dropdownTitle.text = "Skybox";

	}

	void SkyboxDropdownValueChanged(int value){
		dropdownTitle.text = "Skybox";
		if (skyboxNames[value] != VizardGUISettings.CurrentSkybox)
		{
			VizardGUISettings.CurrentSkybox = skyboxNames[value];
			switch (value)
			{
				case 0:

					break;
				case 1:

					UseResourcesSkyboxTexture(skyboxOptions[skyboxNames[value]]);
					break;
				case 2:

					UseAddressableSkyboxTexture(skyboxOptions[skyboxNames[value]]);
					break;
				case 3:
					ChangeCameraBackgroundToSolidColor(Color.black);
					break;
				default:
					UseImportedSkyboxTexture(skyboxOptions[skyboxNames[value]]);

					break;
			}
		}
	}

	void AddListOptionToDropdown()
	{
		VizardGUISettings.PopulateList(skyboxDropdown, skyboxNames);
		dropdownTitle.text = "Skybox";
	}

	private void UseResourcesSkyboxTexture(string location)
	{
		Debug.Log("Trying to load " + location);
		Material newMaterial =
			Instantiate(Resources.Load(location) as Material);

		VizardGUISettings.SkyboxIsTexture = true;
		foreach (Camera currentCamera in Camera.allCameras){
			currentCamera.clearFlags = CameraClearFlags.Skybox;
			currentCamera.GetComponent<Skybox> ().material = newMaterial;
		}

		VizardGUISettings.CurrentSkybox = name;

		ReleaseHandle();
	}
	
	private void UseAddressableSkyboxTexture(string address){
		VizardGUISettings.SkyboxIsTexture = true;
		skyboxHandle = Addressables.LoadAssetAsync<Material>(address);
		skyboxHandle.Completed += SkyboxHandleLoaded;

	}

	private static void SkyboxHandleLoaded(AsyncOperationHandle<Material> operation)
	{
		if (operation.Status == AsyncOperationStatus.Succeeded)
		{
			foreach (Camera currentCamera in Camera.allCameras)
			{
				currentCamera.clearFlags = CameraClearFlags.Skybox;
				currentCamera.GetComponent<Skybox>().material = Instantiate(operation.Result);
			}
		}
		else
		{
			Debug.Log($"Asset for skybox material failed to load.");
		}
	}
	private void ChangeCameraBackgroundToSolidColor(Color newColor){
		VizardGUISettings.SkyboxIsTexture = false;
		VizardGUISettings.SkyboxColor = newColor;
		foreach (Camera currentCamera in Camera.allCameras){
			currentCamera.clearFlags = CameraClearFlags.SolidColor;
			currentCamera.backgroundColor = newColor;
		}

		ReleaseHandle();
	}

	private bool UseImportedSkyboxTexture(string filepath){
		string skyboxName = Path.GetFileName(filepath);
		string fullPath = filepath;
		if ((!DataManager.IsLiveSim)&&(fullPath.StartsWith(".")))
		{
			fullPath = Path.GetFullPath(fullPath, Path.GetDirectoryName(DataManager.FilePath));
		}
		Texture2D newTex = CameraMessageUtilities.LoadTextureImage (fullPath);
		if (newTex != null)
		{
			Material newMaterial =
				Instantiate(Resources.Load("Materials/Skyboxes/InstrumentCamera") as Material);
			newMaterial.mainTexture = newTex;
			newMaterial.name = skyboxName;
			if (!skyboxOptions.ContainsKey(skyboxName))
			{
				skyboxNames.Add(skyboxName);
				skyboxOptions.Add(skyboxName, filepath);
				AddListOptionToDropdown();
			}

			VizardGUISettings.SkyboxIsTexture = true;
			foreach (Camera currentCamera in Camera.allCameras)
			{
				bool applyForThisCamera = true;
				InstrumentCameraMethods instrumentCameraMethods = currentCamera.GetComponent<InstrumentCameraMethods>();
				if ((instrumentCameraMethods != null)&&(instrumentCameraMethods.usingCustomSkyboxForCamera))
				{
					applyForThisCamera = false;
				}
				if (applyForThisCamera)
				{
					currentCamera.clearFlags = CameraClearFlags.Skybox;
					currentCamera.GetComponent<Skybox>().material = newMaterial;
				}
			}

			VizardGUISettings.CurrentSkybox = skyboxName;
			ReleaseHandle();
			return true;
		}

		string errorString = $"Could not load skybox from {filepath}.";
		VizardGUISettings.UpdateErrorMessages(errorString);
		return false;
	}

	public  void ApplyUserSkyboxSettings(string skyboxSetting){
		if (skyboxSetting != ""){
			if (skyboxSetting == "black"){
				skyboxDropdown.value = 3;
			}else if (skyboxSetting == "ESO"){
				skyboxDropdown.value = 2;
			}else if (skyboxSetting == "NASA_SVS")
			{
				skyboxDropdown.value = 1;
			}
			else
			{
				UseImportedSkyboxTexture(skyboxSetting);
			}
		}
		dropdownTitle.text = "Skybox";
	}

	public void SetSkyboxToBlack(){
		VizardGUISettings.SkyboxIsTexture = false;
		VizardGUISettings.SkyboxColor = Color.black;
		foreach (Camera currentCamera in Camera.allCameras){
			currentCamera.clearFlags = CameraClearFlags.SolidColor;
			currentCamera.backgroundColor = Color.black;
		}

		ReleaseHandle();
	}

	private void ReleaseHandle()
	{
		if (skyboxHandle.IsValid())
		{
			Addressables.Release(skyboxHandle);
		}
	}
	private void OnDestroy()
	{
		ReleaseHandle();
	}

}
