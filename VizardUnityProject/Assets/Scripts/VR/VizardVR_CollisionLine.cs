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
using UnityEngine;
using System;
/// <summary>
/// VR: Draws a line along the osculating orbit line that is inside a Keep Out ellipsoid
/// <remarks>Created for SpaceWERX VR operator monitoring experiments and requires a spacecraft named "Servicer" as written </remarks>
/// </summary>
public class VizardVR_CollisionLine : MonoBehaviour {

    public LineRenderer relativeLine; 

    public GameObject collisionZone; 

    public LineRenderer myLine;

    public Vector3[] pointsToPlot; 
  

    public Vector3 semiMajorAxis;

    public Quaternion rot;
   private int connectAttempts = 0;
  
   /// <summary>
   /// Set up the collision line
   /// </summary>
    void InitializeServicerCollisionLine()
    {
        GameObject servicer = SpacecraftStateUtilities.GetSpacecraftObject("Servicer");
        if(servicer == null){
            this.gameObject.SetActive(false);
        }

        if (servicer != null)
        {
            GameObject servicerOrbitLine = servicer.GetComponent<SpacecraftController>().orbitLine;
            relativeLine = servicerOrbitLine.transform.GetChild(0).GetComponent<LineRenderer>();

            collisionZone = GameObject.Find("Ellipsoid");

            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 1.0f);
            curve.AddKey(1.0f, 1.0f);
            curve.AddKey(0.0f, 1.0f);
            curve.AddKey(1.0f, 1.0f);
            myLine.widthCurve = curve;
            myLine.widthMultiplier = 0.2f;
        }
    }
/// <summary>
/// Monodevelop method called each frame
/// <remarks>Checks for points of osculating orbit line inside keep-out ellipsoid and
/// updates the collision line points if any points are found.</remarks>
/// </summary>
    void Update(){
        if (relativeLine == null)
        {
            if (connectAttempts <= 10)
            {
                InitializeServicerCollisionLine();
                connectAttempts++;
            }
        }
        else
        {
            if (VizardGUISettings.OsculatingOrbitLinesVisible)
            {
                myLine.gameObject.SetActive(true);
                if (collisionZone == null)
                {
                    GameObject servicerOrbitLine = GameObject.Find("servicerOrbitLine");
                    relativeLine = servicerOrbitLine.transform.GetChild(0).GetComponent<LineRenderer>();
                    collisionZone = GameObject.Find("Ellipsoid");
                }

                pointsToPlot = getPointsInCollision();
                myLine.positionCount = pointsToPlot.Length;
                myLine.SetPositions(pointsToPlot);


                Color ellipseColor = collisionZone.GetComponent<Renderer>().material.color;
                myLine.startColor = new Color(ellipseColor[0], ellipseColor[1], ellipseColor[2], 1);
                myLine.endColor = new Color(ellipseColor[0], ellipseColor[1], ellipseColor[2], 1);
            }
            else
            {
                myLine.gameObject.SetActive(false);
            }
        }
    }

/// <summary>
/// Check if points of future osculating orbit line renderer are inside keep-out ellipsoid 
/// </summary>
/// <returns></returns>
    public Vector3[] getPointsInCollision(){
        bool prevInCollision = false; 
        semiMajorAxis = collisionZone.transform.localScale; 
        rot = Quaternion.Inverse(collisionZone.transform.rotation); 
        List<Vector3> pointsToPlot2 = new List<Vector3>();
        Vector3 rotatedPoint; 
        
        for (int i = 1; i < relativeLine.positionCount ; i++) {
            Vector3 linePosition = relativeLine.GetPosition(i);
            rotatedPoint = (rot) * linePosition;
            double point = Math.Pow(2*rotatedPoint.x/semiMajorAxis.x,2) +  Math.Pow(2*rotatedPoint.y/semiMajorAxis.y,2) + Math.Pow(2*rotatedPoint.z/semiMajorAxis.z,2);
            if (point <= 1){
                prevInCollision = true; 
                pointsToPlot2.Add(linePosition);
            } else if (prevInCollision){
                break;
            }
        }
        Vector3[] points = pointsToPlot2.ToArray();
        return points;
    }


}