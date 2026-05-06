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
using VizProtobufferMessage;
/// <summary>
/// Builds scenario objects for all VizMessage.CelestialBodies in current scenario
/// </summary>
public class CelestialBodyFactory : MonoBehaviour
{
	public void CreateCelestialBodies(){

		CelestialBodyStateUtilities.DefaultModelImportMtl = Instantiate(Resources.Load("Materials/ImportModelDefault", typeof(Material))as Material);
		CelestialBodyStateUtilities.SunIndex = CelestialBodyStateUtilities.FindSimulatedBodyWithCelestialBodyKey("sun");
		if (CelestialBodyStateUtilities.SunIndex != -1){
			CelestialBodyStateUtilities.SunMsgAvailable = true;
		}

		int index = 0;
		foreach(VizMessage.Types.CelestialBody cb in MessageList.FirstMessage.CelestialBodies){
			
			//Check if this body is supported in Viz defaults
			string bodyDictionaryKey = CelestialBodyStateUtilities.FindCelestialBodyInDictionary(cb.BodyName);
			if (bodyDictionaryKey != "")
			{
				if (bodyDictionaryKey == "sun")
				{
					CelestialBodyStateUtilities.CelestialBodiesList.Add(CreateSun(cb.BodyName, index));
				}
				else
				{
					CreateBody(index, bodyDictionaryKey);
				}
			}else{
				CreateCustomBody(index);
			}

			CelestialBodyStateUtilities.IndexToBodyDictionary[index] = cb.BodyName;
			index += 1;
		}
		//The AddOrbitLineToBody method checks to see if there is a sun before creating planet orbit lines
		foreach(GameObject cb in CelestialBodyStateUtilities.CelestialBodiesList)
		{
			if (!cb.CompareTag("Sun")){
				AddOrbitLineToBody(cb);
			}
		}
		
	}

	private void CreateBody(int bodyIndex, string dictionaryKey, bool isCustom=false){
		GameObject newBody = Instantiate(Resources.Load("Prefabs/CelestialBodyTemplate") as GameObject, DataManager.ScenarioObjectsContainer);
		newBody.GetComponent<PlanetController>().InitializeCelestialBody(bodyIndex, dictionaryKey, isCustom); 
	}

	private void CreateCustomBody(int bodyIndex){
		//First add the body properties to the dictionary
		VizMessage.Types.CelestialBody newBodyMsg = MessageList.FirstMessage.CelestialBodies[bodyIndex];//CelestialBodyStateUtilities.getPlanet(bodyIndex);

		string bodyName = newBodyMsg.BodyName;//Check that real values were provided for the radius and mu
		if (newBodyMsg.Mu <=0){
			VizardGUISettings.UpdateErrorMessages($"Please provide non-zero value for mu for {bodyName}");
		}
		else
		{
			double bodyRadius = newBodyMsg.RadiusEq;
	
			if (bodyRadius <= 0)
			{
				bodyRadius = 0;
				VizardGUISettings.UpdateErrorMessages($"Radius of 0 was provided for {bodyName}.");

			}

			double ellipticity = 1 - newBodyMsg.RadiusRatio;
			if (ellipticity >= 1)
			{
				VizardGUISettings.UpdateErrorMessages($"Invalid radiusRatio provide for {bodyName}. Ellipticity set to 0");
				ellipticity = 0;
			}

			double[] bodyConstants = {bodyRadius, newBodyMsg.Mu, -1, ellipticity};
			CelestialBodyStateUtilities.AddToCelestialBodyDictionary(bodyName, bodyConstants);
			//Second create the new custom body
			CreateBody(bodyIndex, bodyName,true);
		}
	}

	private void AddOrbitLineToBody(GameObject body){
		if (CelestialBodyStateUtilities.SunMsgAvailable||body.GetComponent<PlanetController>().isMoon)
		{
			int bodyIndex = body.GetComponent<PlanetController>().planetIndex;
			// Create a new GameObject using the OrbitLine Template
			GameObject orbitLine = Instantiate(Resources.Load("Prefabs/OrbitLineTemplate") as GameObject, DataManager.ScenarioObjectsContainer);

			orbitLine.GetComponent<OsculatingOrbitLine>().InitializeOrbitLine(body, bodyIndex, false);

			body.GetComponent<PlanetController>().orbitLine = orbitLine;
			CelestialBodyStateUtilities.CelestialBodyOrbitLines.Add(orbitLine);
		}
	}


	private GameObject CreateSun(string nameToUse, int bodyIndex){
		// Create a new GameObject using the Sun Template prefab
		GameObject sun = Instantiate (Resources.Load ("Prefabs/Sun") as GameObject, DataManager.ScenarioObjectsContainer);
		sun.name = nameToUse;
		sun.tag = "Sun";
		sun.layer = 8;
		sun.GetComponent<SunBuilder> ().sunName = nameToUse;
		sun.GetComponent<SunBuilder>().msgIndex = bodyIndex;
		CelestialBodyStateUtilities.SunIndex = bodyIndex;
		CelestialBodyStateUtilities.SunTransform = sun.transform;
		sun.GetComponent<SunBuilder> ().sunClickableCollider.name = nameToUse + "ClickableCollider";
		return sun;
	}

	public void UpdateCelestialBodies(){
		foreach(GameObject p in CelestialBodyStateUtilities.CelestialBodiesList){
			if (p.CompareTag("Sun")){
				p.GetComponent<SunBuilder>().UpdateSun();
			}else{
				p.GetComponent<PlanetController>().UpdateCelestialBody();
			}
		}
	}
}
