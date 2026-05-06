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
/// <summary>
/// Sets up and updates a Transceiver HUD
/// </summary>
public class TransceiverHUDMethods : MonoBehaviour
{
	public GameObject visibleCone;
	public GameObject transmitCircle;
	public ParticleSystem transmitParticles;
	public ParticleSystem receiveParticles;
	public GameObject txLabel;
	public bool labelSettingOnForThisSC;

	private int spacecraftIndex;
	private int txIndex;
	private Color transmitColor;
	private readonly Color frustumGray = new (.9f,.9f,.9f,.03f);
	private readonly Color CUgold = new ((207f/255f), (184f/255f), (124f/255f),1f);
	private float fov;
	private Vector3 scaleToGetCorrectFOV;


	private bool lastFrameHidden;
	private int lastFrameSpeed = 5;
	private int lastFrameMode;
	public int baseRate = 10;

	private float particleSpeed=3.5f;
	private float height;
	private float width;
	private bool inSpriteMode;



	public GameObject InitializeTransceiverHUDUnit(int scIndex, string scName, int transIndex, float maxMeshDimension, bool frustumOn, bool showLabel){
		spacecraftIndex = scIndex;
		txIndex = transIndex;

		VizProtobufferMessage.VizMessage.Types.Transceiver myMsg = MessageList.FirstMessage.Spacecraft[spacecraftIndex]
			.Transceivers[txIndex];
		transform.localPosition = OrbitVectorMath.ReturnVector3(OrbitVectorMath.TransformFromBSKCStoUnity(new[]{myMsg.Position[0], myMsg.Position[1], myMsg.Position[2]}));
		transform.forward = OrbitVectorMath.ReturnVector3(OrbitVectorMath.TransformFromBSKCStoUnity(new[]{myMsg.NormalVector[0], myMsg.NormalVector[1], myMsg.NormalVector[2]}));

		transmitColor = myMsg.Color.Count>=4 ? new Color(myMsg.Color[0]/255f, myMsg.Color[1]/255f, myMsg.Color[2]/255f, myMsg.Color[3]/255f) : CUgold;
		height = maxMeshDimension;
		if (myMsg.AnimationSpeed is > 0 and <= 10){
			lastFrameSpeed = myMsg.AnimationSpeed;

		}
		visibleCone.transform.parent.gameObject.SetActive(true);
		fov = (float) myMsg.FieldOfView;
		SetFieldOfViewAndVisibleConeHeight(fov, height);
		SetParticleSystemsVariables();
		
		string txName = myMsg.Label;
		if (txName == ""){
			txName = scName + " tx "+txIndex;
		}
		this.name = txName+"HUD";
		
		Vector2 txScreenOffset = new Vector2(10,-10);
		txLabel=LabelMaker.CreateLabel(txName, scName, this.gameObject, txScreenOffset, "Transceivers");
		labelSettingOnForThisSC = showLabel;
		if ((!labelSettingOnForThisSC)||(myMsg.IsHidden)){
			txLabel.SetActive(false);
		}
		
		visibleCone.GetComponent<MeshRenderer>().enabled=frustumOn;

		return txLabel;
	}

	public void ToggleTransceiverFrustumHUD(bool isOn){
		visibleCone.GetComponent<MeshRenderer>().enabled = isOn;
	}

	public void ToggleTransceiverCommHUD(bool isOn){
		transmitCircle.SetActive(isOn);
	}

	void OnEnable(){
		if (txLabel !=null)
		{
			txLabel.SetActive(VizardGUISettings.ShowTransceiverLabels);
		}
	}

	void OnDisable(){
		if (txLabel != null){
			txLabel.SetActive(false);
		}
	}

    // Update is called once per frame
    void FixedUpdate()
    {

		VizProtobufferMessage.VizMessage.Types.Transceiver currentMsg = MessageList.CurrentMessage.Spacecraft[spacecraftIndex].Transceivers[txIndex];
		if (!inSpriteMode){
			if (currentMsg.IsHidden!= lastFrameHidden){
				ChangeHUDVisibility(currentMsg.IsHidden);
			}
			if ((currentMsg.AnimationSpeed is > 0 and <= 10) && (currentMsg.AnimationSpeed!= lastFrameSpeed)){
				lastFrameSpeed = currentMsg.AnimationSpeed;
				SetParticleSystemsVariables();
			}
			if (currentMsg.TransmitStatus!=lastFrameMode){
				SetTransmissionParticlesMode(currentMsg.TransmitStatus);
			}
		}
		lastFrameHidden = currentMsg.IsHidden;
		lastFrameMode = currentMsg.TransmitStatus;
	}
    /// <summary>
    /// This receives a BroadcastMessage from the parent Spacecraft when
    /// going into sprite mode. Don't delete. 
    /// </summary>
    /// <param name="spriteOn">True if attached spacecraft is in sprite mode</param>
	public void ConfigureHUDForSpriteMode(bool spriteOn){
		inSpriteMode = spriteOn;

		int mode = lastFrameMode;
		if (inSpriteMode){
			mode = 0;
		}
		SetTransmissionParticlesMode(mode);
		visibleCone.SetActive(!inSpriteMode);
	}

	private void SetFieldOfViewAndVisibleConeHeight(float coneFOV, float coneHeight){
		scaleToGetCorrectFOV = new Vector3(Mathf.Tan(coneFOV/2*Mathf.PI/180), Mathf.Tan(coneFOV/2*Mathf.PI/180), 1);
		visibleCone.transform.localScale = scaleToGetCorrectFOV*coneHeight;
		width = visibleCone.transform.localScale.x;
		visibleCone.GetComponent<MeshRenderer>().material.color = frustumGray;
	}

	private Vector3[] CreateTransmitCirclePoints(){
		int numVertices = 36;
		Vector3[] vertices = new Vector3[numVertices]; //Divide circle into 36 segments

		for(int i = 0; i <numVertices; i++){
			float theta = 2f*Mathf.PI/36f * i;
			vertices[i] = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 1f);
		}
		return vertices;
	}

	private void ChangeHUDVisibility(bool isHidden){
		visibleCone.transform.parent.gameObject.SetActive(!isHidden);
		if (txLabel != null){
			txLabel.SetActive(!isHidden);
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
			height = newDimension;
			SetFieldOfViewAndVisibleConeHeight(fov, newDimension);
			SetParticleSystemsVariables();
		}
	}

	// public void CalculateFramesPerSpeed(int newSpeed){
	// 	framesPerSpeed= baseRate*(12-newSpeed);
	// }

	private void SetParticleSystemsVariables(){
		var txMain = transmitParticles.main;
		var rxMain = receiveParticles.main;
		var rxShape = receiveParticles.shape;
		var txSize = transmitParticles.sizeOverLifetime;

		txMain.startColor = transmitColor;
		rxMain.startColor = transmitColor;

		particleSpeed = 1f+lastFrameSpeed/2f;
		txMain.simulationSpeed = particleSpeed;
		rxMain.simulationSpeed = particleSpeed;


		float startSpeed = Mathf.Clamp(0.5f+10f*Mathf.Sin(Mathf.PI/100f*height), 0.5f, 10f);
		if (height >100f){
			startSpeed = height/10f;
		}

		float startLife = height/startSpeed;

		//Transmit settings
		txMain.startLifetime = startLife;
		txMain.startSpeed=startSpeed;
		txSize.sizeMultiplier = width*10f;

		//Receive settings
		rxMain.startLifetime = startLife;
		rxMain.startSpeed = startSpeed;
		rxShape.position = new Vector3(0,0,height);
		rxMain.startSize = width;
	}

	private void SetTransmissionParticlesMode(int mode){
//		Debug.Log(mode);
		if (mode ==1){
			transmitParticles.transform.gameObject.SetActive(true);
			receiveParticles.transform.gameObject.SetActive(false);
		}else if (mode == 2){
			transmitParticles.transform.gameObject.SetActive(false);
			receiveParticles.transform.gameObject.SetActive(true);
		}else if (mode ==3){
			transmitParticles.transform.gameObject.SetActive(true);
			receiveParticles.transform.gameObject.SetActive(true);
		}else{
			transmitParticles.transform.gameObject.SetActive(false);
			receiveParticles.transform.gameObject.SetActive(false);
		}
	}
}
