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
/// <summary>
/// Sets up and updates a reaction wheel HUD element
/// </summary>
public class ReactionWheelHUDMethods : MonoBehaviour {
	public int spacecraftIndex;
	public int rwIndex;
	public GameObject rwLabel;
	public Color currentColor = new Color(0.5f,0.5f,0.5f,1f);
	public Renderer myRenderer;
	private MaterialPropertyBlock props;
	public GameObject spinAxis;

	public Vector3 spinAxisVector;
	private float lastWheelValue;
	private bool inSpriteMode;

	public void InitializeReactionWheelHUDUnit(int scIndex, int myRWIndex, float meshDimension){
		spacecraftIndex = scIndex;
		rwIndex = myRWIndex;

		myRenderer = GetComponent<Renderer>();

		props = new MaterialPropertyBlock();

		myRenderer.SetPropertyBlock (props);

		spinAxisVector = new Vector3(-1f,-1f,-1f);

		spinAxisVector = ReactionWheelUtilities.GetReactionWheelSpinAxis (spacecraftIndex, rwIndex);

		transform.up = spinAxisVector;
		ApplyMeshDimUpdate(meshDimension);
//		transform.position = radiusScaleFactor * spinAxisVector;

	}

	void OnEnable(){
		ApplyMeshDimUpdate(SpacecraftStateUtilities.GetMeshDimension(spacecraftIndex));
		if (rwLabel != null)
		{
			rwLabel.SetActive(VizardGUISettings.ShowRWLabels);
		}
	}

	void OnDisable(){
		if(rwLabel!=null){
			rwLabel.SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!inSpriteMode){
			if (ReactionWheelUtilities.HUDShowSpeed) {
				float currentWheelValue = (float) (ReactionWheelUtilities.GetReactionWheelSpeed (spacecraftIndex, rwIndex) / ReactionWheelUtilities.MaxSpeed[spacecraftIndex]);
				float redValue = Mathf.Clamp(1.5f * Mathf.Cos (Mathf.PI*(Mathf.Abs(currentWheelValue)-1.0f)),0f, 1f);
				float greenValue = Mathf.Clamp(1.5f * Mathf.Cos (Mathf.PI*(Mathf.Abs(currentWheelValue)+1.5f)),0f, 1f);
				float blueValue = Mathf.Clamp(1.5f * Mathf.Cos (Mathf.PI*(Mathf.Abs(currentWheelValue))),0f, 1f);

				currentColor = new Color (redValue, greenValue, blueValue, 0.75f);

				myRenderer.GetPropertyBlock (props);
				props.SetColor ("_Color", currentColor);
				myRenderer.SetPropertyBlock(props);

				if (Math.Abs(Mathf.Sign (currentWheelValue) - Mathf.Sign (lastWheelValue)) > OrbitVectorMath.EPS) {
					if (currentWheelValue >= 0) {
						spinAxis.transform.localPosition = new Vector3 (0, 15, 0);
						spinAxis.transform.localRotation = Quaternion.Euler (0, 0, 0);
					} else {
						spinAxis.transform.localPosition = new Vector3 (0, -15, 0);
						spinAxis.transform.localRotation = Quaternion.Euler (0, 0,180);
					}
				}
				lastWheelValue = currentWheelValue;
			}
		}
	}
	
	/// <summary>
	/// This receives a BroadcastMessage from the parent spacecraft when
	/// going into sprite mode. Don't delete. 
	/// </summary>
	/// <param name="spriteOn">True if attached spacecraft is in sprite mode</param>
	public void ConfigureHUDForSpriteMode(bool spriteOn){

		inSpriteMode = spriteOn;
		ToggleHUDVisibility(!inSpriteMode);
	}

	private void ToggleHUDVisibility(bool showHUD){
		GetComponent<Renderer>().enabled = showHUD;
		transform.GetChild(0).gameObject.SetActive(showHUD);
	}

	/// <summary>
	/// This receives a broadcast message from its parent spacecraft when the size of the mesh model changes.
	/// Do not delete or make private.
	/// </summary>
	/// <param name="newDimension">Mesh dimension of spacecraft model</param>
	public void ApplyMeshDimUpdate(float newDimension){
		if (newDimension > 0)
		{
			transform.localPosition = newDimension * spinAxisVector * 1.5f;
			transform.localScale = new Vector3(1f, 0.025f, 1f) * newDimension;
		}
	}

}
