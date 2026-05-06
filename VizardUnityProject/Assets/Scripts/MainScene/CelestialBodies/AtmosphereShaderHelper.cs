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
//Adapted from Planet.cs script from "Planet Shader and Shadowing System" Unity asset by Muntadas Quentin
// Version 1.2
// Last release date Feb 22, 2019
using UnityEngine;

public class AtmosphereShaderHelper : MonoBehaviour
{
	public bool HDatmosphere = false;
	public bool atmosphereUpdatesOn = true;
	public Material PlanetMaterial;

	public float hdrExposure = 1.0f;
	public Color atmoColor = new Color(0, 0, 0);
	public float atmoStrength = 10.0f;

	private float kr = 0.0025f;
	private float km = 0.0010f;
	public float outerScaleFactor = 1.015f;
	private float innerRadius;
	private float outerRadius;
	private float scaleDepth = 0.25f;
	private float scale;
	private float gamma = 1.0f;
	private float attenuationAngleSin;
	private float averageDistToSun; //km
	private float planetRadius; //km
	private float sunRadius; //km
	private float umbraExtent; // km

	public float cloudSpeed = 120f; //Earth default km/hr
	public float secondsForCloudOrbit;
	private float[] atmosphereSettings;

	private int shadowNumber = 0;


	void Start()
	{
		if (QualitySettings.activeColorSpace == ColorSpace.Gamma)
		{
			gamma = 2.2f;
			#if UNITY_EDITOR_OSX
			gamma = 1.8f;
			#endif
			#if UNITY_STANDALONE_OSX
			gamma = 1.8f;
			#endif
		}

	}

	void FixedUpdate()
	{
		if (atmosphereUpdatesOn){
			if (HDatmosphere){
				innerRadius = transform.parent.transform.localScale.x;
				InitHDMaterial(PlanetMaterial);
			}
			float currentOffset = ((float) (MessageList.CurrentMessage.CurrentTime.SimTimeElapsed/1e9))/secondsForCloudOrbit;
			PlanetMaterial.SetFloat("_CloudOffset", currentOffset);
		}
	}

	public void InitHDMaterial(Material mat)
	{
		ToggleHDAtmosphere(true);
		PlanetMaterial = mat;
		hdrExposure = atmosphereSettings[0];
		atmoColor = new Color(atmosphereSettings[1]/255f,atmosphereSettings[2]/255f,atmosphereSettings[3]/255f,atmosphereSettings[4]/255f);
		atmoStrength = atmosphereSettings[5];
		outerScaleFactor = atmosphereSettings[6];
		cloudSpeed = atmosphereSettings[7];

		float scaledUmbraDist = umbraExtent;
		if (!CelestialBodyStateUtilities.ViewIsSpacecraftLocal){
			scaledUmbraDist/=(float) CelestialBodyStateUtilities.LocalPlanetViewScale;
		}
		else
		{
			scaledUmbraDist /= (float) CelestialBodyStateUtilities.SpacecraftLocalViewScale;
		}

		float planetRadius = transform.parent.gameObject.GetComponent<PlanetController>().planetRadius/1000f; //km
		float planetCircumference = 2f*Mathf.PI*planetRadius;
		secondsForCloudOrbit = (planetCircumference/cloudSpeed)*60f*60f; // [seconds] =km/(km/hr)*(60m/hr)*(60s/m)

		innerRadius = transform.localScale.x*transform.parent.transform.localScale.x;

		outerRadius = outerScaleFactor * innerRadius;
		scale = 1.0f / (outerRadius - innerRadius);
		Vector3 invWL4 = new Vector3(1 - atmoColor.linear.r, 1 - atmoColor.linear.g, 1 - atmoColor.linear.b);
		invWL4 = new Vector3(1.0f / Mathf.Pow(invWL4.x, 4),
			1.0f / Mathf.Pow(invWL4.y, 4),
			1.0f / Mathf.Pow(invWL4.z, 4));
		mat.SetFloat("_Gamma", gamma);
		mat.SetVector("v3InvWavelength", invWL4);
		mat.SetFloat("fOuterRadius", outerRadius);
		mat.SetFloat("fInnerRadius", innerRadius);
		mat.SetFloat("fKrESun", kr * atmoStrength);
		mat.SetFloat("fKmESun", km * atmoStrength);
		mat.SetFloat("fKr4PI", kr * 4.0f * Mathf.PI);
		mat.SetFloat("fKm4PI", km * 4.0f * Mathf.PI);
		mat.SetFloat("fScale", scale);
		mat.SetFloat("fScaleDepth", scaleDepth);
		mat.SetFloat("fScaleOverScaleDepth", scale / scaleDepth);
		mat.SetFloat("fHdrExposure", hdrExposure);
		mat.SetVector("v3Translate", transform.position);
		mat.SetFloat("shadowNumber", shadowNumber);
		mat.SetFloat("fSpacecraftLocalView", (CelestialBodyStateUtilities.ViewIsSpacecraftLocal? 1 : 0));
		mat.SetFloat("_UserAlbedoSetting", (RenderSettings.ambientIntensity*0.2f));
		mat.SetFloat("fUmbraLengthUnityUnits",  scaledUmbraDist);
		mat.SetFloat("fSinAttenAngle", attenuationAngleSin);
	}

	public void ToggleHDAtmosphere(bool isOn){
		HDatmosphere = isOn;
	}

	public void SetAtmosphereSettings(float[] settings){
		atmosphereSettings = settings;
	}

	public void SetPlanetValues(string dictionaryKey){
		averageDistToSun = (float)CelestialBodyStateUtilities.GetAveDistanceToSun(dictionaryKey);
		planetRadius = (float)CelestialBodyStateUtilities.GetCelestialBodyRadiusInMeters(dictionaryKey)/1000f;
		attenuationAngleSin = Mathf.Sin(Mathf.Atan(planetRadius/averageDistToSun));
		sunRadius = (float)CelestialBodyStateUtilities.GetCelestialBodyRadiusInMeters("sun")/1000f;
		umbraExtent = planetRadius*averageDistToSun/(sunRadius-planetRadius);
	}

	public float[] GetAtmosphereSettings()
	{
		return atmosphereSettings;
	}

}

