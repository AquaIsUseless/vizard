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
using VizProtobufferMessage;
/// <summary>
/// Sets up and updates an ellipsoid HUD element
/// </summary>
public class EllipsoidHUDMethods : MonoBehaviour
{
    public GameObject myEllipsoid;
    public GameObject myEllipsoidInner;
    private int eIndex;
    private int scIndex;
    private SpacecraftController mySC;
    private bool inSpriteMode;
    private readonly float alphaValue = 0.6f;
    // Start is called before the first frame update
    public void InitializeEllipsoid(int ellipsoidIndex, GameObject parent, int spacecraftIndex, int useGrid)
    {
        eIndex = ellipsoidIndex;
        mySC = parent.GetComponent<SpacecraftController>();
        scIndex = spacecraftIndex;

        if (useGrid == 1)
        {
            myEllipsoid.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Materials/Spacecraft_HUD/EllipsoidMatWithGrid", typeof(Material)) as Material);
            myEllipsoidInner.GetComponent<MeshRenderer>().material = Instantiate(Resources.Load("Materials/Spacecraft_HUD/EllipsoidMatWithGrid", typeof(Material)) as Material);
        }

        transform.parent = parent.transform;
    }

    // Update is called once per frame
    void Update()
    {

        VizMessage.Types.Ellipsoid myMsg = MessageList.CurrentMessage.Spacecraft[scIndex].Ellipsoids[eIndex];
        
        if ((myMsg.IsOn==1)&&(!inSpriteMode))
        {
            myEllipsoid.SetActive(true);
 
            //Get position ready 
            double[] myPosition = {0, 0, 0};
            if (myMsg.Position.Count >= 3)
            {
                myPosition = new[] {myMsg.Position[0], myMsg.Position[1], myMsg.Position[2]};
            }

            //Now set up the correct rotation for the ellipsoid HUD main object
            if (myMsg.UseBodyFrame == 1)
            {
                transform.localRotation = Quaternion.Euler(0,0,0);
                //Apply position only to the primitive sphere object, not the entire HUD
                myPosition = OrbitVectorMath.TransformFromBSKCStoUnity(myPosition);
                myEllipsoid.transform.localPosition = OrbitVectorMath.ReturnVector3(myPosition);
                //Then apply any scaling only to the primitive sphere object, not the entire HUD
                Vector3 locScale = OrbitVectorMath.ReturnVector3(OrbitVectorMath.TransformFromBSKCStoUnity(new[]
                    {myMsg.SemiMajorAxes[0], myMsg.SemiMajorAxes[1], myMsg.SemiMajorAxes[2]}));
                myEllipsoid.transform.localScale = new Vector3(2f * Mathf.Abs(locScale[0]), 2f * Mathf.Abs(locScale[1]),
                    2f * Mathf.Abs(locScale[2]));
            }
            else
            {
                List<Vector3> hillFrameAxes = mySC.hillFrameAxes; //These are in the Unity coordinate frame
                if (mySC.hillFrameAxes.Count == 3)
                {
                    transform.rotation = Quaternion.LookRotation(hillFrameAxes[2], hillFrameAxes[1]);
                }
                //Apply position only to the primitive sphere object, not the entire HUD
                Vector3 myHillPositionUnityFrame = OrbitVectorMath.ReturnVector3(myPosition);
                myHillPositionUnityFrame.x = -myHillPositionUnityFrame.x; //Because Unity's CS are left handed
                myEllipsoid.transform.localPosition = myHillPositionUnityFrame;
                //Then apply any scaling only to the primitive sphere object, not the entire HUD
                Vector3 locScale = 2f* OrbitVectorMath.ReturnVector3(new[]
                    {myMsg.SemiMajorAxes[0], myMsg.SemiMajorAxes[1], myMsg.SemiMajorAxes[2]});
                myEllipsoid.transform.localScale = locScale;
            }

            //Lastly apply color
            if (myMsg.Color.Count >= 3)
            {
                if (myMsg.Color.Count >= 4)
                {
                    myEllipsoid.GetComponent<Renderer>().material.color = new Color(myMsg.Color[0] / 255f, myMsg.Color[1] / 255f, myMsg.Color[2] / 255f, myMsg.Color[3]/255f);
                    myEllipsoidInner.GetComponent<Renderer>().material.color = new Color(myMsg.Color[0] / 255f, myMsg.Color[1] / 255f, myMsg.Color[2] / 255f, myMsg.Color[3]/255f);
                }
                else
                {
                   myEllipsoid.GetComponent<Renderer>().material.color = new Color(myMsg.Color[0] / 255f, myMsg.Color[1] / 255f, myMsg.Color[2] / 255f, alphaValue);
                   myEllipsoidInner.GetComponent<Renderer>().material.color = new Color(myMsg.Color[0] / 255f, myMsg.Color[1] / 255f, myMsg.Color[2] / 255f, alphaValue);
                }
            }
        }
        else
        {
            myEllipsoid.SetActive(false);
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
    }

}
