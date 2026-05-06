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
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
/// <summary>
/// Sets up and updates a generic sensor HUD element
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class GenericSensorHUDMethods : MonoBehaviour
{

	public GameObject rectFrustum;
	public GameObject visibleScoop;
	public GameObject visibleCone;
	public GameObject edgeCircle;
	public GameObject gsLabel;
	public bool labelSettingOnForThisSC;

	private int spacecraftIndex;
	private int sensorIndex;
	private bool useRectFrustum;
	private List<Color> modeColors = new List<Color>();
	private int colorCount;
	private bool userSetSize;
	private float fov0;

	private bool lastFrameHidden;
	private int lastFrameMode;
	private bool inFade;
	private int frameCount;
	private readonly int fadeLength = 20;
	private Color lastModeColor;
	private readonly Color frustumGray = new(.9f,.9f,.9f,.03f);
	private readonly Color CUgold = new((207f/255f), (184f/255f), (124f/255f),1f);

	private bool inSpriteMode;
	private bool firstUpdate = true;

	public GameObject InitializeGenericSensorHUDUnit(int scIndex, int sensIndex, float maxMeshDimension, bool showLabel)
	{
		spacecraftIndex = scIndex;
		sensorIndex = sensIndex;
		labelSettingOnForThisSC = showLabel;

		VizProtobufferMessage.VizMessage.Types.GenericSensor myMsg = MessageList.FirstMessage
			.Spacecraft[spacecraftIndex].GenericSensors[sensorIndex];

		transform.localPosition = OrbitVectorMath.ReturnVector3(OrbitVectorMath.TransformFromBSKCStoUnity(new[]{myMsg.Position[0], myMsg.Position[1], myMsg.Position[2]}));
		transform.forward = OrbitVectorMath.ReturnVector3(OrbitVectorMath.TransformFromBSKCStoUnity(new[]{myMsg.NormalVector[0], myMsg.NormalVector[1], myMsg.NormalVector[2]}));

		setModeColorsList(scIndex, sensIndex, myMsg);

		float size = maxMeshDimension;
		if (myMsg.Size<=0){
			userSetSize = false;
		}else{
			userSetSize = true;
			size = (float) myMsg.Size;
		}

		if (myMsg.FieldOfView.Count > 1){ //Use rectangular Frustum
			rectFrustum.SetActive(true);
			useRectFrustum = true;
			rectFrustum.GetComponent<DrawSensorFrustum>().InitializeFrustum((float) myMsg.FieldOfView[0], (float) myMsg.FieldOfView[1], size, frustumGray, CUgold);
			Destroy(visibleCone.transform.parent.gameObject);
		}else{ //Use circular cone
			visibleCone.transform.parent.gameObject.SetActive(true);
			edgeCircle.GetComponent<DrawCircle>().enabled = true;
			fov0 = (float) myMsg.FieldOfView[0];
			SetFieldOfViewAndVisibleConeSize(fov0, size);
			Destroy(rectFrustum);
		}
		string scName = MessageList.FirstMessage.Spacecraft[scIndex].SpacecraftName;
		string gsName = myMsg.Label;
		if (gsName == "")
		{
			gsName = $"{scName} GS {sensorIndex}";
		}

		this.name = gsName + "HUD";

		Vector2 gsScreenOffset = new Vector2(10,-10);
		gsLabel=LabelMaker.CreateLabel(gsName, scName, this.gameObject, gsScreenOffset, "GenericSensors");
		if ((!labelSettingOnForThisSC)||(myMsg.IsHidden)){
			gsLabel.SetActive(false);
		}

		lastModeColor= getModeColor(myMsg.ActivityStatus);
		changeActivityColorAndActiveFaceVisibility(lastModeColor, false);
		changeHUDVisibility(myMsg.IsHidden);

		return gsLabel;
	}

	private void SetFieldOfViewAndVisibleConeSize(float FOV, float size){
		float visibleRadius = size;
		float visibleConeHeight = size*Mathf.Cos(FOV/2*Mathf.PI/180); //reduce cone height to compensate for scoop top
		int numRingsToCreate = (int) FOV / 5;
		if (numRingsToCreate <= 0)
		{
			numRingsToCreate = 1;
		}

		Vector3[] edgeCirclePoints =
			CSSUtilities.BuildHemisphereMesh(visibleScoop, numRingsToCreate, 36, FOV, true, true, visibleRadius);
		edgeCircle.GetComponent<DrawCircle>().SetCirclePointsAndColor(edgeCirclePoints, CUgold, size);
		
		Vector3 scaleToGetCorrectFOV = new Vector3(Mathf.Tan(FOV/2*Mathf.PI/180), Mathf.Tan(FOV/2*Mathf.PI/180), 1);
		visibleCone.transform.localScale = scaleToGetCorrectFOV*visibleConeHeight;
		visibleCone.GetComponent<MeshRenderer>().material.color = frustumGray;
	}
		
	private void setModeColorsList(int scIndex, int sensIndex, VizProtobufferMessage.VizMessage.Types.GenericSensor myMsg){
		int colorSize = myMsg.Color.Count;
		modeColors.Add(CUgold); //This is the default active mode color and should never be used unless the user doesn't provide enough colors for all modes
		if (colorSize>=4){
			int i = 3;
			if (colorSize%4!=0){
				string errorString = $"Generic Sensor Color message requires 4 values per color (R, G, B, and A). {colorSize} were provided for spacecraft: {scIndex}, sensor index: {sensIndex}";
				VizardGUISettings.UpdateErrorMessages(errorString);
			}
			while (i<colorSize){
				modeColors.Add(new Color(myMsg.Color[i-3]/255f, myMsg.Color[i-2]/255f, myMsg.Color[i-1]/255f, myMsg.Color[i]/255f));
				i+=4;
			}
			//What do I do for active inactive if only one color is given
		}else{
			if (colorSize>0){
				string errorString = $"Generic Sensor Color message requires 4 values per color (R, G, B, and A). Only {colorSize} were provided for spacecraft index: {scIndex}, sensor index: {sensIndex}";
				VizardGUISettings.UpdateErrorMessages(errorString);
			}
		}
		colorCount = modeColors.Count;
	}

	/// <summary>
	/// This receives a broadcast message from its parent spacecraft when the size of the mesh model changes.
	/// Do not delete or make private.
	/// </summary>
	/// <param name="newDimension">Mesh dimension of spacecraft model</param>
		public void ApplyMeshDimUpdate(float newDimension)
		{
			if (newDimension > 0)
			{
				if (!userSetSize)
				{
					if (useRectFrustum)
					{
						rectFrustum.GetComponent<DrawSensorFrustum>().UpdateSizeForMaxDimChange(newDimension);
					}
					else
					{
						SetFieldOfViewAndVisibleConeSize(fov0, newDimension);
					}
				}
			}
		}

	void OnEnable(){
		if (firstUpdate)
		{
			firstUpdate = false;
		}else
		{
			ApplyMeshDimUpdate(SpacecraftStateUtilities.GetMeshDimension(spacecraftIndex));
		}
		if (gsLabel !=null)
		{
			gsLabel.SetActive(VizardGUISettings.ShowGenericSensorLabels);
		}
	}

	void OnDisable(){
		if (gsLabel != null){
			gsLabel.SetActive(false);
		}
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		VizProtobufferMessage.VizMessage.Types.GenericSensor currentMsg =
			MessageList.CurrentMessage.Spacecraft[spacecraftIndex].GenericSensors[sensorIndex];
		if (!inSpriteMode){
			if (currentMsg.IsHidden!= lastFrameHidden){
				changeHUDVisibility(currentMsg.IsHidden);
			}
			
			if (!currentMsg.IsHidden){
				if (currentMsg.ActivityStatus != lastFrameMode){
					//Debug.LogFormat("Mode change: {0}!", currentMsg.ActivityStatus);
					if (currentMsg.ActivityStatus ==0){
						inFade = true;
						frameCount = 0;
					}else{
						lastModeColor = getModeColor(currentMsg.ActivityStatus);
						changeActivityColorAndActiveFaceVisibility(lastModeColor, true);
					}
				}else{//check for transition back to 0
					if (inFade){
						Color colorToUse = Color.Lerp (lastModeColor, frustumGray,frameCount/(float)fadeLength);
						if (frameCount==fadeLength){
								inFade = false;
							lastModeColor = frustumGray;
						}	
						frameCount++;
						changeActivityColorAndActiveFaceVisibility(colorToUse, inFade);
					}
				}
			}
		}
		lastFrameHidden = currentMsg.IsHidden;
		lastFrameMode = currentMsg.ActivityStatus;
	}


	private void changeActivityColorAndActiveFaceVisibility(Color colorToUse, bool isActiveMode){
		if (useRectFrustum){
			rectFrustum.GetComponent<DrawSensorFrustum>().SetActiveColor(colorToUse);
			rectFrustum.GetComponent<DrawSensorFrustum>().activeFace.SetActive(isActiveMode);
		}else{
			visibleScoop.SetActive(isActiveMode);
			visibleScoop.GetComponent<MeshRenderer>().material.color = colorToUse;
		}
	}

	private Color getModeColor(int mode){
		if (mode < colorCount){
			return modeColors[mode];
		}else{
			if (mode == 0){
				return frustumGray;
			}else{
				return modeColors[0];
			}
		}
	}

	private void changeHUDVisibility(bool isHidden){
		if (useRectFrustum){
			rectFrustum.SetActive(!isHidden);
		}else{
			visibleCone.transform.parent.gameObject.SetActive(!isHidden);
		}
		if (gsLabel != null){
			if ((!isHidden)&&(VizardGUISettings.ShowGenericSensorLabels))
			{
				gsLabel.SetActive(true);
			}
			else
			{
				gsLabel.SetActive(false);
			}
		}
	}
	/// <summary>
	/// This receives a BroadcastMessage from the parent Spacecraft when
	/// going into sprite mode. Don't delete. 
	/// </summary>
	/// <param name="spriteOn">True if attached spacecraft is in sprite mode</param>
	public void ConfigureHUDForSpriteMode(bool spriteOn){
		inSpriteMode = spriteOn;
		changeHUDVisibility(inSpriteMode);
	}

}
