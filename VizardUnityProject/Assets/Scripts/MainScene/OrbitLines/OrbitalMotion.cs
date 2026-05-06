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
using static System.Math;
/// <summary>
/// Math functions for calculating position and velocity from orbital elements
/// and orbital elements from position and velocity.
/// <remarks> Methods copied from Basilisk Utility orbitalMotion.c
/// </remarks>
/// </summary>
public static class OrbitalMotion
{
	public static double[] elem2pos(double desiredTrueAnomaly, double[] OE)
	{
		double a = OE[0];
		double e = OE[1];
		double i = OE[2];
		double OMEGA = OE[3];
		double omega = OE[4];
		double rPeriap = OE[8];
		
		double radius; //km
		double angle = desiredTrueAnomaly;
		double[] rvec;
		if ((Abs(e - 1.0) < OrbitVectorMath.EPS) && (Abs(a) > OrbitVectorMath.EPS))
		{
			/* 1D rectilinear elliptic/hyperbolic orbit case */
			double[] ir;
			if (a > 0.0)
			{
				radius = a * (1.0 - e * Cos(angle));
			}
			else
			{
				radius = a * (1.0 - e * Cosh(angle));
			}

			ir = new []
			{
				Cos(OMEGA) * Cos(omega) - Sin(OMEGA) * Sin(omega) * Cos(i),
				Sin(OMEGA) * Cos(omega) + Cos(OMEGA) * Sin(omega) * Cos(i),
				Sin(omega) * Sin(i)
			};
			rvec = OrbitVectorMath.ScaleVector(ir, radius);
		}
		else
		{
			//general 2D orbit case
			double p;
			if (Abs(a) > OrbitVectorMath.EPS)
			{
				p = (a * (1.0 - e * e)); //elliptic or hyperbolic
			}
			else
			{
				p = 2 * rPeriap; //parabolic
			}

			radius = p / (1 + e * Cos(angle)); //orbit radius
			double theta = omega + angle; //true latitude angle

			rvec = new []
			{
				radius * (Cos(OMEGA) * Cos(theta) - Sin(OMEGA) * Sin(theta) * Cos(i)),
				radius * (Sin(OMEGA) * Cos(theta) + Cos(OMEGA) * Sin(theta) * Cos(i)),
				radius * (Sin(theta) * Sin(i))
			};
		}

		return new [] {rvec[0], rvec[1], rvec[2]};
	}


	public static double[] elem2rv(double desiredTrueAnomaly, double mu, double[] orbElem){
		// a,  e,  i,  OMEGA (AN), omega (AP),  f,  rmag,  alpha,  rPeriap,  rApoap
		double radius; //km
		double angle = desiredTrueAnomaly;
		double[] rvec;
		double[] vvec;

		if((Abs(orbElem[1]-1.0) < OrbitVectorMath.EPS) && (Abs(orbElem[0]) > OrbitVectorMath.EPS)) { /* 1D rectilinear elliptic/hyperbolic orbit case */
			double vel;
			double[] ir;
			if (orbElem[0] >0.0){
				radius = orbElem[0] *(1.0 - orbElem[1]*Cos(angle));
			}else{
				radius = orbElem[0] *(1.0 - orbElem[1]*Cosh(angle));
			}
			vel = Sqrt(2 * mu/radius - mu/orbElem[0]);
			ir = new []{
				Cos(orbElem[3]) * Cos(orbElem[4]) - Sin(orbElem[3]) * Sin(orbElem[4]) * Cos(orbElem[2]),
				Sin(orbElem[3]) * Cos(orbElem[4]) + Cos(orbElem[3]) * Sin(orbElem[4]) * Cos(orbElem[2]),
				Sin(orbElem[4]) * Sin(orbElem[2])
			};
			rvec = OrbitVectorMath.ScaleVector(ir, radius);
			if (Sin(angle)>0){
				vvec = OrbitVectorMath.ScaleVector(ir, vel);
			}else{
				vvec = OrbitVectorMath.ScaleVector(ir, -vel);
			}
		} else { //general 2D orbit case
			double p;
			if (Abs(orbElem[0]) >OrbitVectorMath.EPS){
				p = (orbElem[0] * (1.0 - orbElem[1] * orbElem[1])); //elliptic or hyperbolic
			} else{
				p = 2 * orbElem[8]; //parabolic
			}

			radius = p / (1 + orbElem[1] * Cos(angle)); //orbit radius
			double theta = orbElem[4] + angle; //true latitude angle
			double h = Sqrt(mu * p); //orbit angular momentum magnitude

			rvec = new[]
			{
				radius * (Cos(orbElem[3]) * Cos(theta) - Sin(orbElem[3]) * Sin(theta) * Cos(orbElem[2])),
				radius * (Sin(orbElem[3]) * Cos(theta) + Cos(orbElem[3]) * Sin(theta) * Cos(orbElem[2])),
				radius * (Sin(theta) * Sin(orbElem[2]))
			};
			vvec = new[]
			{
				-mu / h * (Cos(orbElem[3]) * (Sin(theta) + orbElem[1] * Sin(orbElem[4])) +
				           Sin(orbElem[3]) * (Cos(theta) + orbElem[1] * Cos(orbElem[4])) * Cos(orbElem[2])),
				-mu / h * (Sin(orbElem[3]) * (Sin(theta) + orbElem[1] * Sin(orbElem[4])) -
				           Cos(orbElem[3]) * (Cos(theta) + orbElem[1] * Cos(orbElem[4])) * Cos(orbElem[2])),
				-mu / h * (-(Cos(theta) + orbElem[1] * Cos(orbElem[4])) * Sin(orbElem[2]))
			};
		}
		return new []{rvec[0],rvec[1],rvec[2],vvec[0],vvec[1],vvec[2]};
	}

	public static double CalculateEccentricAnomaly(double trueAnom, double e){
		return 2*Atan2(Sqrt(1-e)*Sin(trueAnom/2),Sqrt(1+e)*Cos(trueAnom/2)); //BSK orbitalMotion.c which checks with Vallado p.56 EQN 2-14
	}

	public static double CalculateTrueAnomaly(double eccAnom, double e){
		return 2*Atan2(Sqrt(1+e)*Sin(eccAnom/2), Sqrt(1-e)*Cos(eccAnom/2));//BSK orbitalMotion.c which checks with Vallado p.56 EQN 2-13
	}

	public static double CalculateHyperbolicAnomaly(double trueAnom, double e){
		double H;
		if (e >1){
			H= 2 * Atanh(Sqrt((e-1)/(e+1))*Tan(trueAnom/2)); //THIS DOESN'T WORK FOR A LARGE RANGE OF TRUE ANOMALY VALUES
			//H = Asinh((Sin(trueAnom)*Sqrt(e*e-1))/(1+e*Cos(trueAnom)));
		}
		else{
			H = double.NaN;
			Debug.Log("Hyperbolic anomaly cannot be calculated for eccentricities less than 1");
		}
		return H;
	}

	private static double Atanh(double value){
		if (Abs(value)<1){
			return (0.5*Log((1+value)/(1-value)));
		}else{
			Debug.Log("Absolute value of input argument to Atanh must be less than 1");
			return double.NaN;
		}
	}

	public static double Asinh(double value){
		return Log(value+Sqrt(1+value*value));
	}
	
	/*
	 * Function: rv2elem
	 * Ported from: Basilisk/SimCode/Utilities/orbitalMotion.c
	 * Purpose: Translates the orbit elements inertial Cartesian position
	 *   vector rVec and velocity vector vVec into the corresponding
	 *   classical orbit elements where
	 *           a   - semi-major axis           (km)
	 *          orbElem[1]  - eccentricity
	 *           i   - inclination               (rad)
	 *           AN  - ascending node            (rad)
	 *           AP  - argument of periapses     (rad)
	 *           f   - true anomaly angle        (rad)
	 *                 if the orbit is rectilinear, then this will be the
	 *                 eccentric or hyperbolic anomaly
	 *   The attracting body is specified through the supplied
	 *   gravitational constant mu (units of km^3/s^2).
	 * Inputs:
	 *   mu = gravitational parameter
	 *   rVec = position vector
	 *   vVec = velocity vector
	 * Outputs:
	 *   elements = orbital elements
	 */
	public static double[] rv2elem(double mu, double[] rVec, double[] vVec)
	{
		double a;		//[km] semi-major axis
		double e;		// eccentricity
		double i;		//[rad] inclination
		//double AN;		//[rad] ascending node
		//double AP;		//[rad] argument of periapses
		double f;		//[rad] true anomaly angle (if the orbit is rectilinear, then this will be the eccentric or hyperbolic anomaly)
		double OMEGA;	//[rad] longitude of the ascending node (rad)
		double omega;	//[rad] argument of perigee (rad)
		double rmag;	// [km] magnitude of position vector
		double alpha;	// [1/km] inverse of the semi-major axis
		double rPeriap;	// [km] radius of periapsis
		double rApoap;	// [km] radius of apoapsis

		double[] hVec;             /* orbit angular momentum vector */
		double[] ihHat;            /* normalized orbit angular momentum vector */
		double h;                   /* orbit angular momentum magnitude */
		double[] v3;               /* temp vector */
		double[] n1Hat;            /* 1st inertial frame base vector */
		double[] n3Hat;            /* 3rd inertial frame base vector */
		double[] nVec;             /* line of nodes vector */
		double[] inHat;            /* normalized line of nodes vector */
		double[] irHat;            /* normalized position vector */
		double r;                   /* current orbit radius */
		double v;                   /* orbit velocity magnitude */
		double[] eVec;             /* eccentricity vector */
		double[] ieHat;            /* normalized eccentricity vector */
		double p;                   /* the parameter, also called semi-latus rectum */

		double M_PI = PI;
		/* define what is a small numerical value */


		/* define inertial frame axes */
		n1Hat = new []{1.0, 0.0, 0.0};
		n3Hat = new []{0.0, 0.0, 1.0};

		/* norms of position and velocity vectors */
		r =OrbitVectorMath.Magnitude(rVec);
		rmag = r;
		v = OrbitVectorMath.Magnitude(vVec);
		
		irHat = OrbitVectorMath.Normalized(rVec);

		/* Calculate the specific angular momentum and its magnitude */
		hVec = OrbitVectorMath.Cross(rVec, vVec);
		h = OrbitVectorMath.Magnitude(hVec);
		ihHat = OrbitVectorMath.Normalized(hVec);
		p = h*h / mu;

		/* Calculate the line of nodes */
		nVec = OrbitVectorMath.Cross(n3Hat, hVec);
		if (OrbitVectorMath.Magnitude(nVec) < OrbitVectorMath.EPS) {
			/* near equatorial orbits */
			inHat = n1Hat;
		} else {
			inHat = OrbitVectorMath.Normalized(nVec);
		}

		/* Orbit eccentricity vector */
		eVec = OrbitVectorMath.ScaleVector(rVec, v * v / mu - 1.0 / r);
		v3 = OrbitVectorMath.ScaleVector(vVec, OrbitVectorMath.Dot(rVec, vVec) / mu);
		eVec = OrbitVectorMath.Subtract(eVec, v3);
		e = OrbitVectorMath.Magnitude(eVec);
		rPeriap = p / (1.0 + e);

		/* Orbit eccentricity unit vector */
		if (e > OrbitVectorMath.EPS) {
			ieHat = OrbitVectorMath.Normalized(eVec);
		} else {
			/* near circular orbits, make i_e_hat equal to line of nodes */
			ieHat = inHat;
		}

		/* compute semi-major axis */
		alpha = 2.0 / r - v*v / mu;
		if(Abs(alpha) > OrbitVectorMath.EPS) {
			/* elliptic or hyperbolic case */
			a = 1.0 / alpha;
			rApoap = p / (1.0 - e);
		} else {
			/* parabolic case */
			a = 0.0;
			rApoap = 0.0;
		}

		/* Calculate the inclination */
		i = Acos(hVec[2] / h);

		/* Calculate Ascending Node Omega */
		v3= OrbitVectorMath.Cross(n1Hat, inHat);
		OMEGA = Atan2(v3[2], inHat[0]);
		if (OMEGA < 0.0) {
			OMEGA += 2*M_PI;
		}

		/* Calculate Argument of Periapses omega */
		v3 = OrbitVectorMath.Cross(inHat, ieHat);
		omega = Atan2(OrbitVectorMath.Dot(ihHat,v3), OrbitVectorMath.Dot(inHat, ieHat));
		if (omega < 0.0) {
			omega += 2*M_PI;
		}

		/* Calculate true anomaly angle f */
		v3 = OrbitVectorMath.Cross(ieHat, irHat);
		f = Atan2(OrbitVectorMath.Dot(ihHat,v3), OrbitVectorMath.Dot(ieHat, irHat));
		if (f < 0.0) {
			f  += 2*M_PI;
		}

		//Try some stuff to fix hyperbolic true anomaly
		if (1/a<0){
			if (Abs(f) > 2*PI){
				f = f%(2*PI);
			}
			if (f > PI){
				f-=2*PI;
			}
			if (f < - PI){
				f+=2*PI;
			}
		}

		return new []{a, e, i, OMEGA,omega, f, rmag, alpha, rPeriap, rApoap};
	}

	public static double CalculateTrueAnomalyFromH(double H, double e){
		if (e > 1){
			return 2*Atan(Sqrt((e+1)/(e-1))*Tanh(H/2));
		}
		Debug.Log("Eccentricity must be greater than 1 to calculate true anomaly from hyperbolic");
		return double.NaN;
	}
	
	public static double KepEqnElliptical(double M, double e){
		//From p. 73 in Vallado
		double eccAnom;
		double eccAnomOld = 0;
		if (-PI<M && M<0 || M>PI){
			eccAnom = M -e;
		}else{
			eccAnom = M + e;
		}
		while (Abs(eccAnom - eccAnomOld) > OrbitVectorMath.EPS){
			eccAnomOld = eccAnom;
			eccAnom += (M-eccAnom+e*Sin(eccAnom))/(1-e*Cos(eccAnom));
		}
		return eccAnom;
	}	

	public static double MeanAnomalyToHyperbolicAnomaly(double meanAnom, double e){
		//From Basilisk orbitalMotion.c N2H function
		double dH = OrbitVectorMath.EPS;
		double H1 = meanAnom;
		int max =400;
		int count = 0;

		if (Abs(H1)>7.0){
			H1 = meanAnom/Abs(meanAnom)*7.0;
		}

		if (e> 1){
			while (Abs(dH) > OrbitVectorMath.EPS/10){
				dH = (e* Sinh(H1)-H1-meanAnom)/(e+Cosh(H1)-1);
				H1 -= dH;
				if(++count > max){
					Debug.LogFormat("Iteration Error in MeanAnomalyToHyperbolicAnomaly function. N = {0}, e = {1}", meanAnom, e);
					dH = 0;
				}
			}
		}else{
			Debug.LogFormat("MeanAnomalyToHyperbolicAnomaly function received e of {0}. The value of e must be > 1.", e);
			H1 = double.NaN;
		}
		return H1;
	}

	public static double HyperbolicAnomalyToMeanAnomaly(double H, double e){
		double N;
		if (e>1){
			N = e* Sinh(H) - H;
		}else{
			Debug.LogFormat("HyperbolicAnomalyToMeanAnomaly received e < 1 (e equaled {0})", e);
			N = double.NaN;
		}
		return N;
	}

	public static double HyperbolicAnomalyToTrueAnomaly(double H, double e){
		double f;
		if (e>1){
			f = 2*Atan(Sqrt((e+1)/(e-1))*Tanh(H/2));
		}else{
			Debug.LogFormat("HyperbolicAnomalyToTrueAnomaly received e < 1 (e equaled {0})", e);
			f= double.NaN;
		}
		return f;
	}

	public static double[] DCM2PRV(double[] diffDCM)
	{
		double cp = (diffDCM[0] + diffDCM[4] + diffDCM[8] - 1) / 2;
		double p = Acos(cp);
		double sp = p / 2.0 / Sin(p);
		double[] q = new[]
		{
			(diffDCM[5]-diffDCM[7])*sp,
			(diffDCM[6]-diffDCM[2])*sp,
			(diffDCM[1]-diffDCM[3])*sp
		};
		return q;
	}

	public static double[] PRV2DCM(double[] PRV)
	{
		double norm = Sqrt(PRV[0] * PRV[0] + PRV[1] * PRV[1] + PRV[2] * PRV[2]);

		double[] q = PRV;
		if (norm > 0.0)
		{
			q[0] /= norm;
			q[1] /= norm;
			q[2] /= norm;
		}

		double cp = Cos(norm);
		double sp = Sin(norm);
		double d1 = 1 - cp;
		double[] C = new[]
		{
			q[0] * q[0] * d1 + cp,
			q[0] * q[1] * d1 + q[2] * sp,
			q[0] * q[2] * d1 - q[1] * sp,
			q[1] * q[0] * d1 - q[2] * sp,
			q[1] * q[1] * d1 + cp,
			q[1] * q[2] * d1 + q[0] * sp,
			q[2] * q[0] * d1 + q[1] * sp,
			q[2] * q[1] * d1 - q[0] * sp,
			q[2] * q[2] * d1 + cp
		};
		return C;
	}

}
