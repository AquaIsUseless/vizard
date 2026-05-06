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
/// Calculates the instantaneous osculating orbit line for a scenario object
/// </summary>
public class OsculatingOrbitLine : MonoBehaviour
{
	public int bodyIndex;
	public int parentBodyIndex=0;
	public double parentBodyMu=1000000;
	public bool isSpacecraft;
	public bool updatePrimaryBody = false;
	
	private int segmentsPer360;
	private float pastOrbitDegreeRangeRadians=-Mathf.PI;
	private float futureOrbitDegreeRangeRadians=Mathf.PI;
	private float fullRange=2*Mathf.PI;
	private int segmentCount = 180;
	private int pastSegmentCount = 90;
	private double TAincrementRad;
	private bool currentTAInsideRange = true;
	
	private double currentScale = 1;
	private double[] OE = new double[10]; //Orbit elements returned from orbitalMotion.rv2elem
	private double[] bodyPosition = new double[]{0,0,0};
	private double[] parentPosition = new double[]{0,0,0};
	private double[] bodyVelocity = new double[]{0,0,0};
	private double[] parentVelocity = new double[]{0,0,0};

	private Vector3[] pointsToPlot = new Vector3[1];
	private double[,] chief_dT_Pos_Vel = new double[1, 7];
	private double[] chief0 = new double[7];
	
	private const double PI = System.Math.PI;

	private bool drawThisOrbitLine = true;
	private bool plotRelativeOrbitLine = false;
	
	public OsculatingOrbitLinePlotter osculatingOrbitLinePlotter;
	public TruePathOrbitLine truePathOrbitLine;
	public GroundTrackOsculating groundTrackOsculating;
	public GroundTrackTruePath groundTrackLineTruePath;

	private double[] saveRvec;
	private double[] saveVvec;
	
	public void InitializeOrbitLine(GameObject body, int BodyIndex, bool bodyIsSpacecraft){//string name, string parentName, bool bodyIsSpacecraft, bool bodyIsMoon, bool updateParentBody){
		isSpacecraft = bodyIsSpacecraft;
		gameObject.name = body.name+"OrbitLine";
		bodyIndex = BodyIndex;
		updatePrimaryBody = (isSpacecraft) || (body.GetComponent<PlanetController>().updateParentBody);
		parentBodyIndex = updatePrimaryBody
			? OrbitVectorMath.FindPrimaryBody(bodyIndex, isSpacecraft)[0]
			: parentBodyIndex = body.GetComponent<PlanetController>().parentBodyIndex;

		if (isSpacecraft)
		{
			truePathOrbitLine.InitializeTruePathLine(body, bodyIndex);
			groundTrackOsculating.InitializeGroundTrackLine(parentBodyIndex, bodyIndex, this);
			groundTrackLineTruePath.InitializeTruePathGroundTrack(bodyIndex);
		}
		else
		{
			//Remove orbit line prefab components that are not used by celestial bodies
			Destroy(truePathOrbitLine.transform.gameObject);
			Destroy(groundTrackOsculating.gameObject);
			Destroy(groundTrackLineTruePath.gameObject);
		}
		
		parentBodyMu = CelestialBodyStateUtilities.GetMu (CelestialBodyStateUtilities.CelestialBodiesList[parentBodyIndex].name);
		
		UpdateOrbitLineSegmentCountAndOrbitRange();
	}

	void Update () {
		if (VizardGUISettings.OsculatingOrbitLinesVisible || VizardGUISettings.OsculatingGroundTrackOn)
		{
			CalculateOrbitalElements();
		}
		drawThisOrbitLine = false;
		osculatingOrbitLinePlotter.enabled = false;
		if (VizardGUISettings.OsculatingOrbitLinesVisible){ //if orbit lines are turned off, don't go any further
			SetOrbitLineVisibility();
			if (isSpacecraft)
			{
				Color newLineColor = SpacecraftStateUtilities.GetOscOrbitColor(bodyIndex);
				if (newLineColor != Color.black)
				{
					osculatingOrbitLinePlotter.lineColor = newLineColor;
				}
			}

			osculatingOrbitLinePlotter.enabled = drawThisOrbitLine;
			if (drawThisOrbitLine){
				if (plotRelativeOrbitLine){
					DrawSpacecraftRelativeOrbitLine();
				}else{
					DrawParentBodyRelativeOrbitLine();
				}
				osculatingOrbitLinePlotter.PlotLine(pointsToPlot,pastSegmentCount);
			}
		}
	}

	private void SetOrbitLineVisibility(){
		if (isSpacecraft)
		{
			drawThisOrbitLine = true;
			plotRelativeOrbitLine = VizardGUISettings.SpacecraftRelativeOsculatingOrbits;
			if((plotRelativeOrbitLine)&&(VizardGUISettings.ChiefSpacecraftIndex == bodyIndex))
			{
				drawThisOrbitLine = false;
				chief_dT_Pos_Vel = new double[1, 7];
				pointsToPlot = new Vector3[1];
			}
		}else{ // it's a celestial body (not sun, because no orbit line for sun)
			plotRelativeOrbitLine = false;
			if (CelestialBodyStateUtilities.ViewIsSpacecraftLocal){
				drawThisOrbitLine = false;
			}else{
				if (!CelestialBodyStateUtilities.ViewIsLocal){ //draw everybody's orbit line if in solar system view
					drawThisOrbitLine = true;
				}else{
					drawThisOrbitLine = false;
					//check to see if camera target's parent body and my parent body are the same: if yes, show my orbit line
					if (MainCameraUtilities.CameraTarget.CompareTag("Spacecraft"))
					{
						if (MainCameraUtilities.CameraTarget.GetComponent<SpacecraftController> ().spacecraftParentBodyIndex == parentBodyIndex) { //local moon(s) turned on
							drawThisOrbitLine = true;
						}
					} else {
						if (MainCameraUtilities.CameraTarget.GetComponent<PlanetController> ().planetIndex == parentBodyIndex) {
							drawThisOrbitLine = true;
						}
					}
				}
			}
		}
	}

	void CalculateOrbitalElements()
	{
		currentScale = CelestialBodyStateUtilities.GetCurrentScale ();
		if (CelestialBodyStateUtilities.ViewIsSpacecraftLocal)
		{
			currentScale = 1f/currentScale;
		}

		if (isSpacecraft) {
			bodyPosition = SpacecraftStateUtilities.GetAbsSpacecraftPositionUnityCS (bodyIndex);
			bodyVelocity = SpacecraftStateUtilities.GetAbsSpacecraftVelocityUnityCS (bodyIndex);
			parentBodyIndex = (OrbitVectorMath.FindPrimaryBody (bodyIndex, true))[0];
			parentBodyMu = CelestialBodyStateUtilities.GetMu (CelestialBodyStateUtilities.CelestialBodiesList[parentBodyIndex].name);
		} else {
			if (updatePrimaryBody){
				parentBodyIndex = OrbitVectorMath.FindPrimaryBody(bodyIndex, false)[0];
			}
			bodyPosition = CelestialBodyStateUtilities.GetAbsolutePlanetPositionUnityCS (bodyIndex);
			bodyVelocity = CelestialBodyStateUtilities.GetAbsPlanetVelocityUnityCS(bodyIndex);
		}

		parentPosition = CelestialBodyStateUtilities.GetAbsolutePlanetPositionUnityCS (parentBodyIndex);
		parentVelocity = CelestialBodyStateUtilities.GetAbsPlanetVelocityUnityCS (parentBodyIndex);

		//Note that I am also putting these vectors back into BSK CS and  changing them into km instead of meters at the same time
		double[] rvec = new double[]{
			(-bodyPosition [2] + parentPosition [2])/1000,
			(bodyPosition [0] - parentPosition [0])/1000,
			(bodyPosition [1] - parentPosition [1])/1000};
		saveRvec = rvec;
		double[] vvec = new double[]{
			(-bodyVelocity [2] + parentVelocity [2])/1000,
			(bodyVelocity [0] - parentVelocity [0])/1000,
			(bodyVelocity [1] - parentVelocity [1])/1000};
		saveVvec = vvec;
		//Calculate the orbital elements
		OE = OrbitalMotion.rv2elem (parentBodyMu, saveRvec, saveVvec);
	}
	void DrawParentBodyRelativeOrbitLine(){
		if (pointsToPlot.Length != segmentCount + 1)
		{
			pointsToPlot = new Vector3[segmentCount + 1];
		}
		double[] parentPVec = new double[]{ parentPosition [0] / currentScale, parentPosition [1] / currentScale, parentPosition [2] / currentScale};
		if (CelestialBodyStateUtilities.ViewIsLocal) { 
			parentPVec = MainCameraUtilities.GetScaledObjectPositionRelToCamTgt (parentBodyIndex);
		} 
		//Now use the orbital elements to calculate line positions for the line renderer
		// Feed r,v into orbitalMotion script
		//starting true anomaly
		double currentTA = OE[5];
		double startTA = currentTA + pastOrbitDegreeRangeRadians;
		double endTA = currentTA + futureOrbitDegreeRangeRadians;
		double eccentricity = OE[1];
		double a = OE[0];
		bool setPastSegmentCount = true;
		if ((1/a) > 0.0) {//Closed orbit
			double CTA = startTA;
			for (int i = 0; i <= segmentCount; i++) {
				if ((setPastSegmentCount)&& (currentTAInsideRange)&&(CTA>=currentTA))
				{
					pastSegmentCount = i;
					CTA = currentTA;
					setPastSegmentCount = false;
				}
				if (i == segmentCount)
				{
					CTA = endTA;
				}
				//Calculate orbit position relative to center of parent body
				double[] positionforCurrentTA = OrbitVectorMath.ScaleVector(OrbitalMotion.elem2pos (CTA, OE),1000.0f / currentScale);

				//Offset the position with the position of the parent body in Unity space (will be relative to the current camera target)
				double[] positionUnityCF =OrbitVectorMath.Add(new double[] {positionforCurrentTA [1], positionforCurrentTA [2], -positionforCurrentTA [0]}, parentPVec);

				pointsToPlot[i]=new Vector3((float)positionUnityCF[0],(float) positionUnityCF[1],(float)positionUnityCF[2]);
				CTA += TAincrementRad;
			}
		} else{//Orbit is parabolic or hyperbolic and true anomaly range is limited
			double minTALimit = .99*(-PI+System.Math.Acos((1.0/eccentricity))); //radians
			double maxTALimit = .99*(PI-System.Math.Acos(1.0/eccentricity));
			if (startTA < minTALimit)
			{
				startTA = minTALimit;
			}

			bool endTAGreaterThanTALimit = false;
			if (endTA > maxTALimit)
			{
				endTA = maxTALimit;
				endTAGreaterThanTALimit = true;
			}
			double totalRange = endTA - startTA;
			double totalIncrement = totalRange / segmentCount;
			currentTAInsideRange = ((startTA < currentTA) && (endTA > currentTA));
			
			double CTA = startTA;
			setPastSegmentCount = true;
			for (int i=0; i<=segmentCount;i++)
			{
				if ((setPastSegmentCount)&& (currentTAInsideRange)&&(CTA>=currentTA))
				{
					pastSegmentCount = i;
					CTA = currentTA;
					setPastSegmentCount = false;
				}
				if ((endTAGreaterThanTALimit) && (i == segmentCount))
				{
					CTA = endTA;
				}
				double[] positionforCurrentTA = OrbitVectorMath.ScaleVector(OrbitalMotion.elem2pos (CTA, OE),1000/currentScale);
				double[] positionUnityCF = OrbitVectorMath.Add(new double[] {positionforCurrentTA [1], positionforCurrentTA [2], -positionforCurrentTA [0]}, parentPVec);
				pointsToPlot[i]=new Vector3((float)positionUnityCF[0],(float) positionUnityCF[1],(float)positionUnityCF[2]);
				CTA += totalIncrement;
			}
		}

		if (CelestialBodyStateUtilities.ViewIsSpacecraftLocal)
		{	//Absolute Osc. Orbit
			GameObject parentBody = CelestialBodyStateUtilities.GetCelestialBodyObject(parentBodyIndex);
			float ratioWallDistToTrueDist = 0;
			if (parentBodyIndex != CelestialBodyStateUtilities.SunIndex)
			{
				ratioWallDistToTrueDist = (float) parentBody.GetComponent<PlanetController>()
					.GetRatioProjectionToTrueDistanceFromCam();
			}
			else
			{
				ratioWallDistToTrueDist = (float) parentBody.GetComponent<SunBuilder>()
					.GetRatioProjectionToTrueDistanceFromCam();
			}

			Vector3 camPositionUU = OrbitVectorMath.ReturnVector3(MainCameraUtilities.GetAbsoluteMainCameraPositionInMeters());
			float camDistance = camPositionUU.magnitude;
			
			Vector3 planetCenterCoordAbsUnityUnits = OrbitVectorMath.ReturnVector3(OrbitVectorMath.ScaleVector(
				OrbitVectorMath.Subtract(CelestialBodyStateUtilities.GetAbsolutePlanetPositionUnityCS(parentBodyIndex),
					MainCameraUtilities.GetCameraTargetAbsolutePositionUnityCS()), CelestialBodyStateUtilities.SpacecraftLocalViewScale));
			Vector3 asDrawnPlanetCenterUnityUnits = parentBody.transform.position;
			if (camDistance>MainCameraUtilities.LineAndSpriteProjectionCorrectionThreshold*CelestialBodyStateUtilities.SpacecraftLocalViewScale)
			{
				for (int i = 0; i < (pointsToPlot.Length); i++)
				{
					Vector3 vectorFromPlanetCenterToPoint = pointsToPlot[i] - planetCenterCoordAbsUnityUnits;
					pointsToPlot[i] = asDrawnPlanetCenterUnityUnits +
					                  vectorFromPlanetCenterToPoint * ratioWallDistToTrueDist;
				}
			}
		}
	}

	public bool CalculateChiefSpacecraftPositionsAndVelocities(){
		
		if (chief_dT_Pos_Vel.GetLength(0) != segmentCount + 1)
		{
			chief_dT_Pos_Vel = new double[segmentCount + 1, 7];
		}
		int chiefIndex = VizardGUISettings.ChiefSpacecraftIndex;
		double[] chiefBodyPosition = SpacecraftStateUtilities.GetAbsSpacecraftPositionUnityCS (chiefIndex);
		double[] chiefBodyVelocity = SpacecraftStateUtilities.GetAbsSpacecraftVelocityUnityCS (chiefIndex);

		int chiefParentBodyIndex = (OrbitVectorMath.FindPrimaryBody (chiefIndex, true))[0];
		double chiefParentBodyMu = CelestialBodyStateUtilities.GetMu (CelestialBodyStateUtilities.CelestialBodiesList[chiefParentBodyIndex].name);

		double[] chiefParentPosition = CelestialBodyStateUtilities.GetAbsolutePlanetPositionUnityCS (chiefParentBodyIndex);
		double[] chiefParentVelocity = CelestialBodyStateUtilities.GetAbsPlanetVelocityUnityCS (chiefParentBodyIndex);

		//Note that I am also putting these vectors back into BSK CS and  changing them into km instead of meters at the same time
		double[] chiefToParentRelativePosition = new double[]{
			(-chiefBodyPosition [2] + chiefParentPosition [2])/1000,
			(chiefBodyPosition [0] - chiefParentPosition [0])/1000,
			(chiefBodyPosition [1] - chiefParentPosition [1])/1000};

		double[] chiefToParentRelativeVelocity = new double[]{
			(-chiefBodyVelocity [2] + chiefParentVelocity [2])/1000,
			(chiefBodyVelocity [0] - chiefParentVelocity [0])/1000,
			(chiefBodyVelocity [1] - chiefParentVelocity [1])/1000};

		//Now get the orbital elements calculated
		// order in returned array: a,  e,  i,  OMEGA, omega,  f,  rmag,  alpha,  rPeriap,  rApoap
		double[] chiefElem =  OrbitalMotion.rv2elem(chiefParentBodyMu, chiefToParentRelativePosition, chiefToParentRelativeVelocity);

		double currentTA = chiefElem[5];
		double eccentricity = chiefElem[1];
		
		chief0 = new double[] {
			0,
			chiefToParentRelativePosition[0],
			chiefToParentRelativePosition[1],
			chiefToParentRelativePosition[2],
			chiefToParentRelativeVelocity[0],
			chiefToParentRelativeVelocity[1],
			chiefToParentRelativeVelocity[2]
		};
		
		//Check for ellipticity
		if (1/chiefElem[0]>0){ //Elliptic chief orbit
			double aCubedOverMuSqrtd = System.Math.Sqrt(chiefElem[0]*chiefElem[0]*chiefElem[0]/chiefParentBodyMu);
			double eccentricAnomT0 = OrbitalMotion.CalculateEccentricAnomaly(currentTA, eccentricity);
			double meanAnomT0 = eccentricAnomT0 - eccentricity*System.Math.Sin(eccentricAnomT0);
			double totalPeriod= 2*PI*aCubedOverMuSqrtd;
			//Calculate time of flight, parent position, parent velocity
			double CTA = currentTA + pastOrbitDegreeRangeRadians;
			for (int i = 0; i <= segmentCount; i++)
			{
				if ((currentTAInsideRange)&&(i==pastSegmentCount))
				{
					CTA = currentTA;
				}
				double[] rvForCurrentTA =
					OrbitalMotion.elem2rv(CTA, chiefParentBodyMu, chiefElem);
				chiefToParentRelativePosition = new double[] {rvForCurrentTA[0], rvForCurrentTA[1], rvForCurrentTA[2]};
				chiefToParentRelativeVelocity = new double[] {rvForCurrentTA[3], rvForCurrentTA[4], rvForCurrentTA[5]};
				double currentTACalc = CTA;
				int addLoop = 0;
				while (System.Math.Abs(currentTACalc) > 2.0 * PI)
				{
					currentTACalc -= System.Math.Sign(CTA) * 2.0 * PI;
					addLoop += System.Math.Sign(CTA)*1;
				}
				double eccentricAnom = OrbitalMotion
					.CalculateEccentricAnomaly(currentTACalc, eccentricity);
				double meanAnom = eccentricAnom - eccentricity * System.Math.Sin(eccentricAnom);

				double deltaT = aCubedOverMuSqrtd * (meanAnom - meanAnomT0) + addLoop * totalPeriod;

				chief_dT_Pos_Vel[i, 0] = deltaT;
				chief_dT_Pos_Vel[i, 1] = chiefToParentRelativePosition[0];
				chief_dT_Pos_Vel[i, 2] = chiefToParentRelativePosition[1];
				chief_dT_Pos_Vel[i, 3] = chiefToParentRelativePosition[2];
				chief_dT_Pos_Vel[i, 4] = chiefToParentRelativeVelocity[0];
				chief_dT_Pos_Vel[i, 5] = chiefToParentRelativeVelocity[1];
				chief_dT_Pos_Vel[i, 6] = chiefToParentRelativeVelocity[2];
				CTA += TAincrementRad;
			}
		}
		else
		{
			// hyperbolic orbit
			double aCubedOverMuSqrtd =
				System.Math.Sqrt(-chiefElem[0] * chiefElem[0] * chiefElem[0] / chiefParentBodyMu);

			double minTALimit = -0.9 * PI + System.Math.Acos((1.0 / eccentricity)); //radians
			if (minTALimit < currentTA + pastOrbitDegreeRangeRadians)
			{
				minTALimit = currentTA + pastOrbitDegreeRangeRadians;
			}

			double maxTALimit = 0.9 * (PI - System.Math.Acos(1 / eccentricity));
			if (maxTALimit > currentTA + futureOrbitDegreeRangeRadians)
			{
				maxTALimit = currentTA + futureOrbitDegreeRangeRadians;
			}

			double Hp90 = OrbitalMotion.CalculateHyperbolicAnomaly(maxTALimit, eccentricity);
			double H0 = OrbitalMotion.CalculateHyperbolicAnomaly(currentTA, eccentricity);
			double Hm90 = OrbitalMotion.CalculateHyperbolicAnomaly(minTALimit, eccentricity);

			//Calculate time of flight for hyperbolic anomaly at specified range around current TA
			double timeHp90 = aCubedOverMuSqrtd * (eccentricity * System.Math.Sinh(Hp90) - Hp90);
			double time0 = aCubedOverMuSqrtd * (eccentricity * System.Math.Sinh(H0) - H0);
			double timeHm90 = aCubedOverMuSqrtd * (eccentricity * System.Math.Sinh(Hm90) - Hm90);

			double M0 = OrbitalMotion.HyperbolicAnomalyToMeanAnomaly(H0, eccentricity);

			//Need to account for if time0 is not inside of range and carry that through for loop
			double timeIncrement;
			if (currentTAInsideRange)
			{
				timeIncrement = (time0 - timeHm90) / (pastSegmentCount);
			}
			else
			{
				timeIncrement = (timeHp90 - timeHm90) / segmentCount;
			}

			double deltaT = timeHm90 - time0;
			double CTA = minTALimit;
			for (int i = 0; i <= segmentCount; i++)
			{
				//marching forward user through 90 degrees of true anomaly with time increment
				if ((currentTAInsideRange)&&(i==pastSegmentCount))
				{
					CTA = currentTA;
					deltaT = 0;
					timeIncrement = (timeHp90 - time0) / (segmentCount - pastSegmentCount);
				}

				double[] rvForCurrentTA = OrbitalMotion.elem2rv(CTA, chiefParentBodyMu, chiefElem);
				chiefToParentRelativePosition = new double[] {rvForCurrentTA[0], rvForCurrentTA[1], rvForCurrentTA[2]};
				chiefToParentRelativeVelocity = new double[] {rvForCurrentTA[3], rvForCurrentTA[4], rvForCurrentTA[5]};

				chief_dT_Pos_Vel[i, 0] = deltaT;
				chief_dT_Pos_Vel[i, 1] = chiefToParentRelativePosition[0];
				chief_dT_Pos_Vel[i, 2] = chiefToParentRelativePosition[1];
				chief_dT_Pos_Vel[i, 3] = chiefToParentRelativePosition[2];
				chief_dT_Pos_Vel[i, 4] = chiefToParentRelativeVelocity[0];
				chief_dT_Pos_Vel[i, 5] = chiefToParentRelativeVelocity[1];
				chief_dT_Pos_Vel[i, 6] = chiefToParentRelativeVelocity[2];

				//Calculate next time step's TA:
				deltaT += timeIncrement;
				//Calculate mean anomaly at new time (Vallado eqn 2-38):
				double M1 = (deltaT / aCubedOverMuSqrtd) + M0;
				//Calculate the hyperbolic anomaly at new time
				double H1 = OrbitalMotion.MeanAnomalyToHyperbolicAnomaly(M1, eccentricity);
				//Calculate the true anomaly at new time
				CTA = OrbitalMotion.HyperbolicAnomalyToTrueAnomaly(H1, eccentricity);
			}
		}
		return true;
	}

	
	private void DrawSpacecraftRelativeOrbitLine(){
		bool useHillFrame = VizardGUISettings.SpacecraftRelativeOrbitMode==1;
		bool competeChiefUpdate = CalculateChiefSpacecraftPositionsAndVelocities();
		
		if (pointsToPlot.Length != chief_dT_Pos_Vel.GetLength(0))
		{
			pointsToPlot = new Vector3[chief_dT_Pos_Vel.GetLength(0)];
		}
		
		double[] DCM_T0;
		double[] DCMTranspose_T0;
		if (useHillFrame){
			DCM_T0 = OrbitVectorMath.CalculateHillFrame(new double[]{chief0[1], chief0[2], chief0[3]}, new double[]{chief0[4], chief0[5], chief0[6]});
		}else{
			DCM_T0 = OrbitVectorMath.CalculateVelocityFrame(new double[]{chief0[1], chief0[2], chief0[3]}, new double[]{chief0[4], chief0[5], chief0[6]});
		}

		DCMTranspose_T0 = OrbitVectorMath.TransposeMatrix(DCM_T0);

		double[] relativeChiefPosToCamTgt = OrbitVectorMath.TransformFromUnityCStoBSK(OrbitVectorMath.Subtract(SpacecraftStateUtilities.GetAbsSpacecraftPositionUnityCS(VizardGUISettings.ChiefSpacecraftIndex), MainCameraUtilities.GetCameraTargetAbsolutePositionUnityCS()));

		//TODO: Figure out why this has to be recalculated here. Execution order indicates CalculateOrbitalElements is happening first, so OE should be good, 
		//but if it isn't recalculated here the relative line draws through chief instead of through deputy.
		OE = OrbitalMotion.rv2elem(parentBodyMu, saveRvec, saveVvec);
		
		double aCubedOverMuSqrtd;
		double eccentricAnomAtT0;
		double hyperbolicAnomAtT0;
		double meanAnomAtT0;

		double currentTA = OE[5];
		double eccentricity = OE[1];
		double a = OE[0];

		if (1/a > 0){
			//elliptic deputy orbit
			aCubedOverMuSqrtd = System.Math.Sqrt(a*a*a/parentBodyMu);
			eccentricAnomAtT0= OrbitalMotion.CalculateEccentricAnomaly(currentTA, eccentricity);
			meanAnomAtT0 = eccentricAnomAtT0-eccentricity*System.Math.Sin(eccentricAnomAtT0);
		}else{
			aCubedOverMuSqrtd = System.Math.Sqrt(-a*a*a/parentBodyMu);
			hyperbolicAnomAtT0 = OrbitalMotion.CalculateHyperbolicAnomaly(currentTA, eccentricity);
			meanAnomAtT0 = OrbitalMotion.HyperbolicAnomalyToMeanAnomaly(hyperbolicAnomAtT0 , eccentricity);
		}
		
		double deltaT;
		double meanAnom;
		double[] chief = new double[7];
		double CTA = currentTA + pastOrbitDegreeRangeRadians;
		for(int i = 0; i<chief_dT_Pos_Vel.GetLength(0); i++)
		{
			for (int j = 0; j < 7; j++)
			{
				chief[j] = chief_dT_Pos_Vel[i, j];
			}; 
			deltaT = chief[0];
			int revCount = 0;
			if (1/a > 0){
				meanAnom = meanAnomAtT0+deltaT/aCubedOverMuSqrtd;
				while (meanAnom > 2 * PI)
				{
					meanAnom -= 2 * PI;
					revCount += 1;
				}
				while (meanAnom < -2 * PI)
				{
					meanAnom += 2 * PI;
					revCount -= 1;
				}
				double eccentricAnom = OrbitalMotion.KepEqnElliptical(meanAnom, eccentricity);
				CTA = 2*PI*revCount + OrbitalMotion.CalculateTrueAnomaly(eccentricAnom, eccentricity);
			}else{
				meanAnom = (deltaT/aCubedOverMuSqrtd)+meanAnomAtT0;
				//Calculate the hyperbolic anomaly at new time
				double H1 = OrbitalMotion.MeanAnomalyToHyperbolicAnomaly(meanAnom, eccentricity);
				//Calculate the true anomaly at new time
				CTA = OrbitalMotion.HyperbolicAnomalyToTrueAnomaly(H1, eccentricity);
			}
			double[] DCM;
			if (useHillFrame){
				DCM = OrbitVectorMath.CalculateHillFrame(new double[]{chief[1], chief[2], chief[3]}, new double[]{chief[4], chief[5], chief[6]});
			}else{
				DCM = OrbitVectorMath.CalculateVelocityFrame(new double[]{chief[1], chief[2], chief[3]}, new double[]{chief[4], chief[5], chief[6]});
			}

			double[] current_rvec = OrbitalMotion.elem2pos(CTA, OE);

			//Calculate the relative position of the deputy wrt the chief at this time step
			double[] rrel = OrbitVectorMath.Subtract(new double[] {current_rvec[0], current_rvec[1], current_rvec[2]}, new double[] {chief[1], chief[2], chief[3]});

			//Transform into Hill Frame
			double[] relPos = OrbitVectorMath.ApplyTransformationMatrixToVector(DCM, rrel);

			// This gets you the relative position of the deputy to the chief
			double[] relInertialFromInitialDCM = OrbitVectorMath.ApplyTransformationMatrixToVector(DCMTranspose_T0, relPos);

			//Get it back into meters, add the offset from the camera target to chief
			double[] positionRelativeToCamTgt = OrbitVectorMath.Add(OrbitVectorMath.ScaleVector(relInertialFromInitialDCM, 1000), relativeChiefPosToCamTgt);

			//Scale it by the current world scale and convert to Vector3
			Vector3 inertialPositionToSave = OrbitVectorMath.ReturnVector3(OrbitVectorMath.ScaleVector(positionRelativeToCamTgt, (1/currentScale))); //still in BSK coordinates
			//Lastly rotate into unity frame
			pointsToPlot[i]=new Vector3(inertialPositionToSave.y, inertialPositionToSave.z, -inertialPositionToSave.x);
		}

		if (CelestialBodyStateUtilities.ViewIsSpacecraftLocal)
		{
			//RelativeOrbitLine
			GameObject parentBody = CelestialBodyStateUtilities.GetCelestialBodyObject(parentBodyIndex);
			float ratioWallDistToTrueDist =
				(float) parentBody.GetComponent<PlanetController>().GetRatioProjectionToTrueDistanceFromCam();

			Vector3 planetCenterCoordAbsUnityUnits = OrbitVectorMath.ReturnVector3(OrbitVectorMath.ScaleVector(
				OrbitVectorMath.Subtract(CelestialBodyStateUtilities.GetAbsolutePlanetPositionUnityCS(parentBodyIndex),
					MainCameraUtilities.GetCameraTargetAbsolutePositionUnityCS()), CelestialBodyStateUtilities.SpacecraftLocalViewScale));
			Vector3 asDrawnPlanetCenterUnityUnits = parentBody.transform.position;
			if (MainCameraUtilities.TrueCameraDistanceToTargetMeters>(MainCameraUtilities.LineAndSpriteProjectionCorrectionThreshold*CelestialBodyStateUtilities.SpacecraftLocalViewScale))
			{
				for (int i = 0; i < (pointsToPlot.Length); i++)
				{
					Vector3 vectorFromPlanetCenterToPoint = pointsToPlot[i] - planetCenterCoordAbsUnityUnits;
					pointsToPlot[i] = asDrawnPlanetCenterUnityUnits +
					                  vectorFromPlanetCenterToPoint * ratioWallDistToTrueDist;
				}
			}
		}
	}


	public void UpdateOrbitLineSegmentCountAndOrbitRange()
	{
		segmentsPer360 = PersistentUserSettings.persistentSettingsFromLastSave.OrbitLineSegments;
		if (isSpacecraft)
		{
			if (PersistentUserSettings.persistentSettingsFromLastSave.OsculatingOrbitLineRange.Count >= 2)
			{
				
				pastOrbitDegreeRangeRadians =
					(float) (PersistentUserSettings.persistentSettingsFromLastSave.OsculatingOrbitLineRange[0] * PI /
					         180);
				futureOrbitDegreeRangeRadians =
					(float) (PersistentUserSettings.persistentSettingsFromLastSave.OsculatingOrbitLineRange[1] * PI /
					         180);
			}
			else
			{
				pastOrbitDegreeRangeRadians =
					(float) (-PersistentUserSettings.persistentSettingsFromLastSave.RelativeOrbitRange*PI/180);
				futureOrbitDegreeRangeRadians= (float)(PersistentUserSettings.persistentSettingsFromLastSave.RelativeOrbitRange*PI/180);
			}
		}

		fullRange = futureOrbitDegreeRangeRadians - pastOrbitDegreeRangeRadians;
		segmentCount = Mathf.CeilToInt(fullRange * segmentsPer360 / (2 * Mathf.PI));
		if (pastOrbitDegreeRangeRadians < 0)
		{
			if (futureOrbitDegreeRangeRadians < 0)
			{
				pastSegmentCount = segmentCount;
				currentTAInsideRange = false;
			}
			else
			{
				currentTAInsideRange = true;
				pastSegmentCount = Mathf.CeilToInt(-pastOrbitDegreeRangeRadians * segmentsPer360 / (2 * Mathf.PI));
			}
		}
		else
		{
			pastSegmentCount = 0;
			currentTAInsideRange = false;
		}	

		TAincrementRad = fullRange / segmentCount;
	}

	public double[] GetOrbitalElements()
	{
		return OE;
	}
	
	public void ToggleTruePathLineGameObject(bool toggleOn)
	{
		truePathOrbitLine.gameObject.SetActive(toggleOn);
	}
	
}
