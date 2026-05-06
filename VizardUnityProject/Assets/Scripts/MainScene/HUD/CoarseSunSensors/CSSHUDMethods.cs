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
using System.Linq;
using UnityEngine;
using VizProtobufferMessage;
/// <summary>
/// Sets up and updates a coarse sun sensor (CSS) HUD element
/// </summary>
public class CSSHUDMethods : MonoBehaviour
{
	public GameObject cssLabel;
	public GameObject cssCoverageMesh;
	public GameObject boresight;
	private bool inSpriteMode;
	private int spacecraftIndex;
	private int cssID;
	private readonly Color minSignalColor = new(0.608f, 0f, 1f,0.1f);
	private readonly Color maxSignalColor = new(1f,1f,0f,0.1f);
	private readonly Color alertColor = new(1f, 0.453f, 0.102f, 0.1f);

	private float maxMeasurement;
	private float minMeasurement;
	private Vector3 cssPosition;
	private Vector3 cssNormal; 
	private bool allCSSPositionsOnThisSpacecraftZeroed;

	private bool boresightIsOn;
	private bool coverageIsOn;

    // Start is called before the first frame update
	public void InitializeCSSHUDUnit(int scID, int cssIndex, float meshDimension, bool boresightOn, bool coverageOn, bool allPositionsZeroed)
    {
		spacecraftIndex = scID;
		cssID = cssIndex;
		allCSSPositionsOnThisSpacecraftZeroed= allPositionsZeroed;
		boresightIsOn = boresightOn;
		coverageIsOn = coverageOn;
		VizMessage.Types.CoarseSunSensor myMsg = MessageList.FirstMessage.Spacecraft[spacecraftIndex].CSS[cssID];
		CSSUtilities.BuildHemisphereMesh(cssCoverageMesh, 8, 36, (float) myMsg.FieldOfView, true, true);
		maxMeasurement = (float) myMsg.MaxMsmt;
		minMeasurement = (float) myMsg.MinMsmt;
		cssNormal = OrbitVectorMath.ReturnVector3(OrbitVectorMath.TransformFromBSKCStoUnity(myMsg.NormalVector.ToArray()));
		if (allCSSPositionsOnThisSpacecraftZeroed){//Place CSS along its normal vector as no positions were provided
			cssPosition = (meshDimension*0.6f)*cssNormal.normalized;
		}else{
			cssPosition = OrbitVectorMath.ReturnVector3(OrbitVectorMath.TransformFromBSKCStoUnity(myMsg.Position.ToArray()));
		}
		cssCoverageMesh.transform.up = cssNormal;
		ApplyMeshDimUpdate(meshDimension);
		boresight.transform.localPosition = cssPosition;
		boresight.transform.up = cssNormal;

		cssCoverageMesh.SetActive(coverageIsOn);
		boresight.SetActive(boresightIsOn);
	}

	void OnEnable(){
		ApplyMeshDimUpdate(SpacecraftStateUtilities.GetMeshDimension(spacecraftIndex));
		if (cssLabel != null)
		{
			cssLabel.SetActive(VizardGUISettings.ShowCSSLabels);
		}
	}

	void OnDisable(){
		if(cssLabel!=null){
			cssLabel.SetActive(false);
		}
	}

    // Update is called once per frame
    void FixedUpdate()
    {
		if (!inSpriteMode){
			float currentMeasurement = (float) MessageList.CurrentMessage.Spacecraft[spacecraftIndex].CSS[cssID].CurrentMsmt;
			Color currentColor = alertColor;
			if ((currentMeasurement <= maxMeasurement)||(currentMeasurement >= minMeasurement)){
				float measurementRatio = (currentMeasurement-minMeasurement)/(maxMeasurement-minMeasurement);
				currentColor = Color.Lerp(minSignalColor, maxSignalColor, measurementRatio);
			}
			cssCoverageMesh.GetComponent<MeshRenderer>().material.color = currentColor;
			currentColor.a = 1f;
			boresight.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = currentColor;
			boresight.transform.GetChild(0).GetChild(0).gameObject.GetComponent<MeshRenderer>().material.color = currentColor;
		}
    }
    
    /// <summary>
    /// This receives a BroadcastMessage from the parent Spacecraft when
    /// going into sprite mode. Don't delete. 
    /// </summary>
    /// <param name="spriteOn">True if attached spacecraft is in sprite mode</param>
    public void ConfigureHUDForSpriteMode(bool spriteOn)
    {
	    inSpriteMode = spriteOn;
	    cssCoverageMesh.SetActive(coverageIsOn&&!inSpriteMode);
	    boresight.SetActive(boresightIsOn&&!inSpriteMode);
    }
    
    public void ToggleCSSCoverageHUD(bool isOn)
	{
		coverageIsOn = isOn;
		cssCoverageMesh.SetActive(isOn);
	}

	public void ToggleCSSNormalHUD(bool isOn)
	{
		boresightIsOn = isOn;
		boresight.SetActive(isOn);
		SetLabelActive();
	}

	private void SetLabelActive(){
		if (cssLabel != null){ 
			if ((VizardGUISettings.ShowCSSLabels)&&(boresight.activeSelf)){
				cssLabel.SetActive(true);	
			}else{
				cssLabel.SetActive(false);
			}
		}
	}
	
	/// <summary>
	/// This receives a broadcast message from its parent spacecraft when the size of the mesh model changes.
	/// Do not delete or make private.
	/// </summary>
	/// <param name="newDimension">Mesh dimension of spacecraft model</param>
	public void ApplyMeshDimUpdate(float newDimension){
		if (newDimension > 0)
		{
			float sizeUpdate = newDimension * 1.5f;
			cssCoverageMesh.transform.localScale = new Vector3(sizeUpdate, sizeUpdate, sizeUpdate);
			if (allCSSPositionsOnThisSpacecraftZeroed)
			{
				//Place CSS along its normal vector as no positions were provided
				cssPosition = sizeUpdate * cssNormal.normalized;
				boresight.transform.localPosition = cssPosition;
			}

			boresight.transform.localScale = new Vector3(sizeUpdate, sizeUpdate, sizeUpdate);
		}
	}
}
