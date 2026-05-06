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
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using VizProtobufferMessage;
/// <summary>
/// Builds and updates a Sun celestial body object
/// </summary>
public class SunBuilder : MonoBehaviour {
	public string sunName;
	public int msgIndex;
	public float solarSystemViewScaleFactor = 2;
	private readonly float apparentSunSizeScaleMultiplier = 0.9726921f; // apparent sun size/ scientific literature sun dia
	public float sunRadius;
	private float distanceSunToEarth;
	
	public GameObject sunMesh;
	public GameObject sunClickableCollider;
	public GameObject sunCoordinateAxes;
	public Light sunPointLight;
	public Light sunDirectionalLight;
	public Light lensFlareLightSource;
	public GameObject shellForwardLighting;
	public GameObject shellBackLighting;
	public ParticleSystem sunParticleSystem;
	private GameObject mainCamera;

	private double ratioProjectionToTrueDistanceFromCam;

	private VizProtobufferMessage.VizMessage.Types.CustomModel myModelSettings;
	public Material defaultMaterial;

	private GameObject nameLabel;
	private List<GameObject> allMyLabels = new List<GameObject>();
	private bool isVisible=true;
	private readonly int layerMask = ((1 << 7)| (1 << 9)|(1 << 11)); //7 = Unlit Spacecraft 9 = True Body Size Colliders, 11 = Spacecraft 

	private bool inNoDisplayMode;
	private float oldScale;

	public void Start()
	{
		inNoDisplayMode = DataManager.InNoDisplayMode;
		myModelSettings = new VizProtobufferMessage.VizMessage.Types.CustomModel{
			ModelPath = "HI_DEF_SPHERE", 
			SimBodiesToModify = {sunName}, 
			Offset = {0,0,0},
			Rotation = {0,0,0},
			Scale = {1,1,1},
		};
		


		defaultMaterial = Instantiate(sunMesh.GetComponent<Renderer>().material);

		sunRadius = CelestialBodyStateUtilities.GetCelestialBodyRadiusInMeters ("sun"); 
		distanceSunToEarth = (float) CelestialBodyStateUtilities.GetAveDistanceToSun("earth")*1000;//get it into meters
		mainCamera = GameObject.FindWithTag ("MainCamera");
		sunMesh.transform.localScale = apparentSunSizeScaleMultiplier*Vector3.one;
		
		CreateLabels();
	}

	public void FixedUpdate(){
		if (!inNoDisplayMode){
			UpdateSun();
		}
	}

	public void UpdateSun(){
		double[] myPosition = CelestialBodyStateUtilities.GetAbsolutePlanetPositionUnityCS (msgIndex);
		Vector3 lightVectorToTarget = Vector3.zero;
		ratioProjectionToTrueDistanceFromCam = 1f;
		if (CelestialBodyStateUtilities.ViewIsLocal) { 
			
			sunPointLight.transform.gameObject.SetActive (false);
			sunDirectionalLight.transform.gameObject.SetActive (true);
			lensFlareLightSource.transform.gameObject.SetActive (true);
			sunParticleSystem.transform.gameObject.SetActive (false);
			sunClickableCollider.transform.localScale = new Vector3 (2, 2, 2);

			double[] cameraTargetPosition = MainCameraUtilities.GetCameraTargetAbsolutePositionUnityCS (); //Returns raw position of target

			//Calculate relative position:
			myPosition= OrbitVectorMath.Subtract(myPosition,cameraTargetPosition);
			lightVectorToTarget = -OrbitVectorMath.ReturnVector3(myPosition);
			
			//Attenuate the light for distance if turned on
			if (PersistentUserSettings.persistentSettingsFromLastSave.AttenuateSunLightWithDistance==1){
				float sunPositionMagnitude = (float) OrbitVectorMath.Magnitude (myPosition);
				float rDistanceRatio = distanceSunToEarth / sunPositionMagnitude;
				sunDirectionalLight.intensity = (float) PersistentUserSettings.persistentSettingsFromLastSave.SunIntensity * rDistanceRatio*rDistanceRatio;
			}

			if (CelestialBodyStateUtilities.ViewIsSpacecraftLocal)
			{
				//sunPosition = orbitVectorMath.ScaleVector (sunPosition, CelestialBodyStateUtilities.spacecraftLocalViewScale);
				double[] camPositionMeters = MainCameraUtilities.GetAbsoluteMainCameraPositionInMeters();
				double[] truePositionFromCameraMeters = OrbitVectorMath.Subtract(myPosition, camPositionMeters);
				double trueDistanceFromCamMeters = OrbitVectorMath.Magnitude(truePositionFromCameraMeters);
				double distanceToProjectionWallMeters =
					MainCameraUtilities.DistanceToProjectionWallUnityUnits /
					CelestialBodyStateUtilities.SpacecraftLocalViewScale;
				double projectionWallStackingConstantMeters = MainCameraUtilities.ProjectionWallStackingConstantUnityUnits /
				                                              CelestialBodyStateUtilities.SpacecraftLocalViewScale;

				if (trueDistanceFromCamMeters > distanceToProjectionWallMeters){
					if (trueDistanceFromCamMeters >
					    MainCameraUtilities.TrueCameraDistanceToTargetMeters)
					{
						ratioProjectionToTrueDistanceFromCam =
							(distanceToProjectionWallMeters + projectionWallStackingConstantMeters * Math.Log10(trueDistanceFromCamMeters-distanceToProjectionWallMeters)) / trueDistanceFromCamMeters;
					}
					else
					{
						ratioProjectionToTrueDistanceFromCam = distanceToProjectionWallMeters/ trueDistanceFromCamMeters;
					}
					myPosition = OrbitVectorMath.ScaleVector(truePositionFromCameraMeters,
						ratioProjectionToTrueDistanceFromCam); //meters
					myPosition = OrbitVectorMath.ScaleVector(myPosition,
						CelestialBodyStateUtilities.SpacecraftLocalViewScale); //Unity Units
					myPosition = OrbitVectorMath.Add(OrbitVectorMath.ReturnDoubleArray(MainCameraUtilities.MainCamera.transform.position),myPosition); //Unity units
				}
				else
				{
					myPosition = OrbitVectorMath.ScaleVector(myPosition,
						(float) CelestialBodyStateUtilities.SpacecraftLocalViewScale); //Unity units
				}
			}else{
				myPosition = OrbitVectorMath.ScaleVector (myPosition, 1 / CelestialBodyStateUtilities.LocalPlanetViewScale);
			}
		} else { //view is solar system wide
			myPosition =OrbitVectorMath.ScaleVector(myPosition, 1/CelestialBodyStateUtilities.HelioCenteredViewScale);
			sunPointLight.transform.gameObject.SetActive (true);
			sunDirectionalLight.transform.gameObject.SetActive (false);
			lensFlareLightSource.transform.gameObject.SetActive (false);
			sunParticleSystem.transform.gameObject.SetActive (true);
			
			sunClickableCollider.transform.localScale = Vector3.one;
		}


		UpdateVisibilitySettings();


		//Update the position of the sun
		Vector3 scaledPosition = OrbitVectorMath.ReturnVector3(myPosition);
		transform.position = scaledPosition;
		
		if (CelestialBodyStateUtilities.ViewIsLocal) {
			sunDirectionalLight.transform.LookAt(scaledPosition+lightVectorToTarget);
			//sunDirectionalLight.transform.localRotation.SetLookRotation(lightVectorToTarget);
			lensFlareLightSource.transform.LookAt (mainCamera.transform);
		}
		//Update the sun mesh rotation
		sunMesh.transform.localRotation = CelestialBodyStateUtilities.GetPlanetRotationUnityCS(msgIndex);

		SetScale (GetDesiredScale(CelestialBodyStateUtilities.ViewIsLocal, CelestialBodyStateUtilities.ViewIsSpacecraftLocal));
	}

	public float GetScale(){
		return transform.localScale.x;
	}

	private float GetDesiredScale(bool viewIsLocal, bool viewIsSpacecraftLocal){
		double desiredScale;
		if (!viewIsLocal) {
			desiredScale = CelestialBodyStateUtilities.DefaultHelioPlanetScale*solarSystemViewScaleFactor;
		} else {
			if (viewIsSpacecraftLocal)
			{
				desiredScale = sunRadius*CelestialBodyStateUtilities.SpacecraftLocalViewScale;
			}
			else
			{
				desiredScale = sunRadius / CelestialBodyStateUtilities.LocalPlanetViewScale;
			}
			desiredScale *= ratioProjectionToTrueDistanceFromCam;
		}
		return (float) desiredScale; 
	}

	private void SetScale(float newRadius){
		transform.localScale = new Vector3 (newRadius, newRadius, newRadius);
		if (!CelestialBodyStateUtilities.ViewIsLocal)
		{
			if (Math.Abs(newRadius - oldScale) > OrbitVectorMath.EPS)
			{
				sunParticleSystem.Stop();
				sunParticleSystem.Play();
				oldScale = newRadius;
			}
		}
	}

	public void SetSunMesh(GameObject newMesh){
		sunMesh = newMesh;
	}

	private int CheckForVisibleInCamera(){
		Vector3 origin = mainCamera.transform.position;
		Vector3 direction = transform.position - origin;
		int maxDistance = (int) direction.magnitude*2;
		try
		{
			if (Physics.Raycast(origin, direction, out var hit, maxDistance, layerMask))
			{
				if (hit.collider.gameObject.transform.parent.gameObject.CompareTag("Sun"))
				{
					return 1;
				}

				return 2;
			}
		}
		catch
		{
			return 0;
		}
		return 0;
	}

	public void SetDefaultModel(VizMessage.Types.CustomModel newSettings){
		myModelSettings = newSettings;
	}

	public VizProtobufferMessage.VizMessage.Types.CustomModel GetDefaultModel(){
		return myModelSettings;
	}

	public void SetDefaultMaterial(Material myMaterial){
		defaultMaterial = myMaterial;
	}

	public Material GetDefaultMaterial(){
		return defaultMaterial;
	}

	public void ApplyDefaultMaterial(){
		try{
			sunMesh.GetComponent<Renderer> ().material = defaultMaterial;
		}catch{
			sunMesh.GetComponentInChildren<Renderer> ().material = defaultMaterial;
		}
	}

	private void UpdateVisibilitySettings(){
		if (DataManager.FirstMessageDisplayed)
		{
			int bodyVisible = CheckForVisibleInCamera();
			if ((bodyVisible == 1) | (bodyVisible == 2))
			{
				if ((bodyVisible == 1) && (isVisible != true))
				{
					isVisible = true;
					if (CelestialBodyStateUtilities.ViewIsLocal)
					{
						lensFlareLightSource.transform.gameObject.SetActive(true);
					}
					else
					{
						lensFlareLightSource.transform.gameObject.SetActive(false);
					}
				}
				else if ((bodyVisible == 2))
				{
					isVisible = false;
					lensFlareLightSource.transform.gameObject.SetActive(false);
				}

				if (VizardGUISettings.SomeCelestialBodyLabelsAreOn)
				{
					UpdateLabelVisibility();
				}
			}
		}
	}
		
	private void UpdateLabelVisibility(){
		foreach(GameObject label in allMyLabels){
			label.GetComponent<TextMeshProUGUI>().enabled = isVisible;
		}
	}

	private void CreateLabels(){
		//Body Name
		nameLabel = LabelMaker.CreateLabel(sunName, "Label", transform.gameObject, Vector2.one, "CelestialBodies");
		if(VizardGUISettings.ShowCelestialBodyLabels){
			nameLabel.SetActive(true);
		}else{
			nameLabel.SetActive(false);
		}

		//Coordinate System
		char prefix = '\u0070';
		string xLabel = $"{prefix.ToString()}{LabelMaker.Circumflex.ToString()}<sub>1</sub>";
		string yLabel = $"{prefix.ToString()}{LabelMaker.Circumflex.ToString()}<sub>2</sub>";
		string zLabel = $"{prefix.ToString()}{LabelMaker.Circumflex.ToString()}<sub>3</sub>";
		GameObject x = LabelMaker.CreateLabel(xLabel, sunName, sunCoordinateAxes.transform.GetChild(0).gameObject, Vector2.zero, "CoordinateSystems");
		GameObject y = LabelMaker.CreateLabel(yLabel, sunName, sunCoordinateAxes.transform.GetChild(1).gameObject, Vector2.zero, "CoordinateSystems");
		GameObject z = LabelMaker.CreateLabel(zLabel, sunName, sunCoordinateAxes.transform.GetChild(2).gameObject, Vector2.zero, "CoordinateSystems");
		sunCoordinateAxes.GetComponent<DrawAxes>().AttachCSLabels(x,y,z);
	}

	public void SetSunIntensityAtEarth(float newIntensity){
		sunDirectionalLight.intensity = newIntensity;
	}

	public void UseShellLighting()
	{
		shellForwardLighting.SetActive(true); //Main Shell Lighting
		shellBackLighting.SetActive(true); //Back Shell Lighting
	}
	
	public double GetRatioProjectionToTrueDistanceFromCam()
	{
		return ratioProjectionToTrueDistanceFromCam;
	}
				
}
