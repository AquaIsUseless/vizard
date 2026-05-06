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
/// Sets up, points, and captures images from attached standard camera
/// </summary>
public class StandardCameraController : MonoBehaviour {

	private GameObject bodyTarget;
	private GameObject myAttachedBody;
	public Transform targetTransform; //Needed for smooth camera views in Along Track body mode
	public SecondaryCameraHUDMethods secondaryCameraHUD;

	private bool inBodyMode=true;

	public bool useCustomCamPosition;
	
	private Vector3 currentCameraOrigin = Vector3.zero;

	private int cameraVectorToPlanetIndex=2;

	public Vector3 pointingVector = Vector3.one;

	void Awake()
	{
		secondaryCameraHUD = GetComponent<SecondaryCameraHUDMethods>();
		MainCameraUtilities.SecondaryCameras.Add(GetComponent<Camera>());
		myAttachedBody = SpacecraftStateUtilities.ParentSpacecraftList[0];
		if (myAttachedBody == null)
		{
			myAttachedBody = CelestialBodyStateUtilities.CelestialBodiesList[0];
		}

		bodyTarget = CelestialBodyStateUtilities.CelestialBodiesList[0];
		if (bodyTarget == null)
		{
			bodyTarget = GameObject.Find("OriginTarget");
		}
		
		ChangeStandardCameraAttachedBody(myAttachedBody);
	}

	// Update is called once per frame
	void Update () {
		
		if (inBodyMode){
			if (bodyTarget != null) {
				Vector3 cameraForward = Vector3.one; //Set a dummy cameraForward vector for help with error detection
				if (cameraVectorToPlanetIndex == 0){ //Nadir View: Camera forward vector points to planet center
					// Have the camera forward vector be toward the planetTarget
					cameraForward = bodyTarget.transform.position - myAttachedBody.transform.position;
					Quaternion cameraRotation = Quaternion.LookRotation (cameraForward, Vector3.up);
					transform.rotation = cameraRotation;
				}
				else
				{
					Vector3 attachedBodyVelocity;
					if (myAttachedBody.CompareTag("Spacecraft")){
						attachedBodyVelocity = OrbitVectorMath.ReturnVector3(SpacecraftStateUtilities.GetAbsSpacecraftVelocityUnityCS(myAttachedBody.GetComponent<SpacecraftController>().spacecraftIndex));
					} 
					else{
						
						if (myAttachedBody.CompareTag("Sun"))
						{
							attachedBodyVelocity= OrbitVectorMath.ReturnVector3(CelestialBodyStateUtilities.GetAbsPlanetVelocityUnityCS(CelestialBodyStateUtilities.SunIndex));
						}
						else
						{
							attachedBodyVelocity = OrbitVectorMath.ReturnVector3(CelestialBodyStateUtilities.GetAbsPlanetVelocityUnityCS(myAttachedBody.GetComponent<PlanetController>().planetIndex));
						}
					}
					if (cameraVectorToPlanetIndex == 1) { //Orbit Normal: Camera points along normal of orbit plane
						//Camera up vector be the vector from the planet to the spacecraft
						Vector3 cameraUp = myAttachedBody.transform.position - bodyTarget.transform.position;
						//Calculate camera forward vector
						cameraForward = Vector3.Cross (attachedBodyVelocity, cameraUp);

						Quaternion cameraRotation = Quaternion.LookRotation (cameraForward, cameraUp);
						transform.rotation = cameraRotation;

					} else if (cameraVectorToPlanetIndex == 2) { //Along Track: Camera points along velocity vector
						cameraForward = attachedBodyVelocity;
						targetTransform.position = transform.position +attachedBodyVelocity;
						transform.LookAt(targetTransform);
					} else {
						Debug.Log ("Invalid planet view camera orientation selection.");
					}
				}
				pointingVector = cameraForward.normalized;
			}//End of body mode
		}
		
		if (CelestialBodyStateUtilities.ViewIsSpacecraftLocal){
			GetComponentInParent<Camera> ().cullingMask |= 1 << LayerMask.NameToLayer("Spacecraft");
			transform.position = transform.parent.transform.position + secondaryCameraHUD.GetMaxExtent()*(float)CelestialBodyStateUtilities.SpacecraftLocalViewScale * pointingVector; //Want camera moved in global coordinates for body mode
		} else{
			GetComponentInParent<Camera> ().cullingMask &= ~(1 << LayerMask.NameToLayer("Spacecraft"));
			transform.position = transform.parent.transform.position + secondaryCameraHUD.GetMaxExtent() * pointingVector; //Want camera moved in global coordinates for body mode
		}
		
		if (useCustomCamPosition){
			transform.localPosition = currentCameraOrigin;
		}else{
			currentCameraOrigin = transform.localPosition;
		}
	}

	public void RequestHUDUpdate()
	{
		ChangeStandardCameraAttachedBody(myAttachedBody);
	}

	public void ChangePointingVector(Vector3 newVector){
		inBodyMode = false;
		pointingVector = new Vector3(newVector.y, newVector.z, -newVector.x); //Converted to Unity CS
		pointingVector = pointingVector.normalized;
		Quaternion directionToPoint = Quaternion.LookRotation (pointingVector); 
		transform.localRotation = directionToPoint;
	}

	public void ChangeStandardCameraAttachedBody(GameObject newParent)
	{
		if (newParent == bodyTarget){
			foreach(GameObject p in CelestialBodyStateUtilities.CelestialBodiesList){
				if (p != newParent){
					bodyTarget = p;
					break;
				}
			}
		}
		myAttachedBody = newParent;
		transform.SetParent(newParent.transform);
		transform.position = Vector3.zero;
		transform.localScale = Vector3.one;
		secondaryCameraHUD.GetAttachedBodyMeshDimensionExtent(myAttachedBody);
	}

	public void ChangeStandardCameraTarget(GameObject newTarget){
		inBodyMode = true;
		bodyTarget = newTarget;
	}

	public void ChangeCameraVectorToPlanet(int vectorToPlanetIndex){
		inBodyMode = true;
		cameraVectorToPlanetIndex = vectorToPlanetIndex -1;
		if (cameraVectorToPlanetIndex < 0) {
			cameraVectorToPlanetIndex = 0;
		}
	}
	
	public Vector3 GetCurrentCameraOrigin(){
		return currentCameraOrigin;
	}

	public void SetCurrentCameraOrigin(Vector3 newOrigin){
		// Change origin coordinate from BSK coordinates to Unity
		currentCameraOrigin = new Vector3(newOrigin.y, newOrigin.z, -newOrigin.x);
	}
	
	public string GetBodyTargetName()
	{
		if (bodyTarget != null)
		{
			return bodyTarget.name;
		}
		return "";
	}

	public GameObject GetAttachedBody()
	{
		return myAttachedBody;
	}

	public void SetBodyMode(bool targetBodyMode)
	{
		inBodyMode = targetBodyMode;
	}
}
