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
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// Casts a shadow from planet onto the attached rings if HD material is available
/// Adapts Planet.cs script from "Planet Shader and Shadowing System" Unity asset by Muntadas Quentin
/// Version 1.2
/// Last release date Feb 22, 2019
/// </summary>
public class RingsHelper : MonoBehaviour
{
	public GameObject planet;
	private Material ringHDMaterial;
	private Material ringMaterial;
	private bool useHDRings;
	private bool HDmatAvailable;
	private AsyncOperationHandle<Material> matHandle;
	private bool needToLoadHDRings = true;
	private bool inMatLoad;
    void Start()
    {
		planet = transform.parent.gameObject.transform.parent.gameObject; 
		ringMaterial = GetComponent<Renderer>().material;
    }
    
    void FixedUpdate()
    {
	    if (GoodEnoughAddressables.AllRemoteCatalogsLoaded)
	    {
		    if (needToLoadHDRings&&!inMatLoad)
		    {
			    LoadHDRingMaterial();
		    }
	    }
	    
	    if (HDmatAvailable&&useHDRings)
	    {
		    SetRingShaderProperties();
	    }
    }

    private void LoadHDRingMaterial()
    {
	    string HDRingkey = "MeshRingsMaterialHD";
	    inMatLoad = true;
	    if (CelestialBodyStateUtilities.RemoteModelKeyValid(HDRingkey))
	    {
		    VizardGUISettings.AddRemoteAssetLoadToList(HDRingkey, 3);
		    matHandle = Addressables.LoadAssetAsync<Material>(HDRingkey);
		    matHandle.Completed += HDRingMaterialLoad;
	    }
	    else
	    {
		    needToLoadHDRings = false;
		    HDmatAvailable = false;
		    inMatLoad = false;
		    string errMsg = $"Celestial body material key: MeshRingsMaterialHD not found in Addressables bundles.";
		    VizardGUISettings.UpdateErrorMessages(errMsg);
		    GetComponent<Renderer>().material = ringMaterial;
	    }
    }

	private void SetRingShaderProperties(){
		float scaledRadius = transform.parent.transform.localScale.x*planet.transform.localScale.x;
		ringHDMaterial.SetVector("_SpherePosition", planet.transform.position);
		ringHDMaterial.SetFloat("_SphereRadius", scaledRadius);
		ringHDMaterial.SetFloat("_LightDistance", 0.0025f);
	}

	public void UseHDRings(bool HDon){
		if (HDon != useHDRings)
		{
			useHDRings = HDon;
			if (useHDRings && HDmatAvailable)
			{
				GetComponent<Renderer>().material = ringHDMaterial;
			}
			else
			{
				GetComponent<Renderer>().material = ringMaterial;
			}
		}
	}

	private void HDRingMaterialLoad(AsyncOperationHandle<Material> operation)
	{
		needToLoadHDRings = false;
		inMatLoad = false;
		if (operation.Status == AsyncOperationStatus.Succeeded)
		{
			HDmatAvailable = true;
			VizardGUISettings.PopRemoteAssetLoadFromList("MeshRingsMaterialHD", true);
			ringHDMaterial = Instantiate(operation.Result);
			GetComponent<Renderer>().material = ringHDMaterial;
			SetRingShaderProperties();

		}
		else
		{
			VizardGUISettings.PopRemoteAssetLoadFromList("MeshRingsMaterialHD",false);
			Debug.Log($"\"MeshRingsMaterialHD for Saturn rings failed to load.");
			HDmatAvailable = false;
			GetComponent<Renderer>().material = ringMaterial;
		}
	}
	
	private void OnDestroy()
	{
		if (matHandle.IsValid())
		{
			Addressables.Release(matHandle);
		}
	}
}
