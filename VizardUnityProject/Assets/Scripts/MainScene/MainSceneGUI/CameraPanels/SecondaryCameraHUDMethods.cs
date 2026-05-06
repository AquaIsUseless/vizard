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
/// Toggles on and off the HUD elements for a secondary camera:
/// frustum, camera view, and boresight
/// </summary>
public class SecondaryCameraHUDMethods : MonoBehaviour
{
	[Header("Camera HUD Components")]
	public LineRenderer boresightLineRenderer;
	public GameObject myFrustum;
	public GameObject cameraLabel;
	public GameObject myCameraPreview;
	public Toggle panelToggle;
	
	private Color boresightColor = new (0.8470588f, 1f, 0.6392157f, 1f);
	private float maxExtent = 1f;
	private GameObject attachedBody;
	
	void OnEnable()
	{
		if (!gameObject.name.Contains("AdjustModelCamera"))
		{
			ConfigureHUDForSpriteMode(false);
			if (cameraLabel != null)
			{
				cameraLabel.SetActive(VizardGUISettings.ShowCameraLabels);
			}
		}
	}
	
	void OnDisable(){
		if (cameraLabel!=null){
			cameraLabel.SetActive(false);
		}
		if ((!VizardGUISettings.ShowCamPreviews)&&(panelToggle!=null))
		{
			panelToggle.isOn = false;
		}
	}

	private void DrawBoresight()
	{
		boresightLineRenderer.gameObject.GetComponent<HUDLineRenderer>().SetMeshDim(maxExtent);
		Vector3[] verticesForFrustum = {Vector3.zero, new Vector3(0,0,maxExtent)};
		boresightLineRenderer.startColor = boresightColor;
		boresightLineRenderer.endColor = boresightColor;
		boresightLineRenderer.material.color = boresightColor;
		boresightLineRenderer.positionCount = 2;
		boresightLineRenderer.SetPositions(verticesForFrustum);
	}
	

	public void ToggleCameraBoresightHUD(bool showHUD)
	{
		boresightLineRenderer.enabled = showHUD;
		if (showHUD){
			DrawBoresight();
		}
	}

	public void ToggleCameraFrustumHUD(bool showHUD){
		
		myFrustum.SetActive(showHUD);
		if (showHUD)
		{
			myFrustum.GetComponent<DrawCameraFrustum>().DrawFrustum(maxExtent);
		}
	}

	public void ToggleCameraPreviewHUD(bool showHUD)
	{
		myCameraPreview.SetActive(showHUD);
		if (showHUD)
		{
			myCameraPreview.GetComponent<FrustumCameraPreviewMethods>().SetCameraPreviewSizeAndLocation(maxExtent);
			
		}

		myFrustum.GetComponent<MeshRenderer>().enabled = !showHUD;
	}
	
	/// <summary>
	/// This receives a broadcast message from its parent spacecraft when the size of the mesh model changes.
	/// Do not delete or make private.
	/// </summary>
	/// <param name="newDimension">Mesh dimension of spacecraft model</param>
	public void ApplyMeshDimUpdate(float newDimension){
		if (newDimension > 0)
		{
			maxExtent = newDimension;
			boresightLineRenderer.gameObject.GetComponent<HUDLineRenderer>().SetMeshDim(maxExtent);
			ToggleCameraBoresightHUD(boresightLineRenderer.enabled);
			ToggleCameraFrustumHUD(myFrustum.activeSelf);
			ToggleCameraPreviewHUD(myCameraPreview.activeSelf);
		}
	}
	/// <summary>
	/// This receives a BroadcastMessage from the parent Spacecraft when
	/// going into sprite mode. Don't delete. 
	/// </summary>
	/// <param name="spriteOn">True if attached spacecraft is in sprite mode</param>
	private void ConfigureHUDForSpriteMode(bool spriteOn){
		if (spriteOn)
		{
			ToggleCameraBoresightHUD(false);
			ToggleCameraFrustumHUD(false);
			ToggleCameraPreviewHUD(false);
		}
		else
		{
			ToggleCameraBoresightHUD(VizardGUISettings.ShowCamBoresights);
			ToggleCameraFrustumHUD(VizardGUISettings.ShowCamFrustums);
			ToggleCameraPreviewHUD(VizardGUISettings.ShowCamPreviews);
		}
	}
	
	public float GetAttachedBodyMeshDimensionExtent(GameObject myAttachedBody){
		if (myAttachedBody.CompareTag("Spacecraft"))
		{
			maxExtent = myAttachedBody.GetComponent<SpacecraftController>().meshDimension;
		}
		else
		{
			Transform clickableCollider = myAttachedBody.transform.GetChild(1);
			Vector3 clickableColliderLocalScale = clickableCollider.localScale;
			maxExtent = Mathf.Max(clickableColliderLocalScale.x, clickableColliderLocalScale.y,
				clickableColliderLocalScale.z);
		}
		ApplyMeshDimUpdate(maxExtent);

		return maxExtent;
	}

	public float GetMaxExtent()
	{
		return maxExtent;
	}
}
