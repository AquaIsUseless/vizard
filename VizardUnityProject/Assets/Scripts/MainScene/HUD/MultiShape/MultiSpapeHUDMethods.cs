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
/// Sets up and updates a multi-shape HUD element
/// </summary>
public class MultiShapeHUDMethods : MonoBehaviour
{
    public GameObject myShape;
    private int myIndex;
    private int scIndex;
    private bool inSpriteMode;
    private Color neutralColor;
    private Color positiveColor = new(1f,0f,0f, 0.294f);
    private Color negativeColor = new(0f,1f,0f,0.294f);
    private double maxChargeValue;
    private float defaultAlphaValue = 0.039f;
    private GameObject myLabel;
    private bool labelSettingOnForThisSC;
    private bool mainLabelToggleActive;

    // Update is called once per frame
    void FixedUpdate()
    {
        if ((MessageList.CurrentMessage.Spacecraft[scIndex].MultiShapes[myIndex].IsOn == 1)&&(!inSpriteMode))
        {
            myShape.SetActive(true);
            if (VizardGUISettings.ShowMSMLabels)
            {
                mainLabelToggleActive = true;
            }

            if (mainLabelToggleActive)
            {
                labelSettingOnForThisSC = VizardGUISettings.ShowMSMLabels;
            }
            myLabel.SetActive(labelSettingOnForThisSC);
            SetColorForCurrentCharge(MessageList.CurrentMessage.Spacecraft[scIndex].MultiShapes[myIndex].CurrentValue);
        }
        else
        {
            myShape.SetActive(false);
            myLabel.SetActive(false);
        }
    }
    
    void OnDisable()
    {
        if (myLabel != null)
        {
            myLabel.SetActive(false);
        }
    }

    public void InitializeMSM(int msmIndex, GameObject parent, int spacecraftIndex)
    {
        myIndex = msmIndex;
        scIndex = spacecraftIndex;

        transform.parent = parent.transform;

        VizMessage.Types.MultiShape myMsg = MessageList.FirstMessage.Spacecraft[scIndex].MultiShapes[myIndex];

        if (myMsg.Shape != "")
        {
            if (myMsg.Shape == "CAPSULE")
            {
                GameObject newShape = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                newShape.transform.parent = transform;
                newShape.transform.SetAsFirstSibling();
                newShape.layer = 22;
                Destroy(myShape);
                myShape = newShape;
            }else if (myMsg.Shape == "CYLINDER")
            {
                GameObject newShape = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                newShape.transform.parent = transform;
                newShape.transform.SetAsFirstSibling();
                newShape.layer = 22;
                Destroy(myShape);
                myShape = newShape;
            }else if (myMsg.Shape == "CUBE")
            {
                GameObject newShape = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newShape.transform.parent = transform;
                newShape.transform.SetAsFirstSibling();
                newShape.layer = 22;
                Destroy(myShape);
                myShape = newShape;
            }
        }
        
        double[] myPosition = {0, 0, 0};
        if (myMsg.Position.Count >= 3)
        {
            myPosition = OrbitVectorMath.TransformFromBSKCStoUnity(new[] {myMsg.Position[0], myMsg.Position[1], myMsg.Position[2]});
        }

        myShape.transform.localPosition = OrbitVectorMath.ReturnVector3(myPosition);

        if (myMsg.Radius>0)
        {
            myShape.transform.localScale = Vector3.one * (2f * (float)myMsg.Radius);
        }

        if (myMsg.Dimensions.Count == 3)
        {
            Vector3 desiredDimensions = OrbitVectorMath.ReturnVector3(
                OrbitVectorMath.TransformFromBSKCStoUnity(new[]
                    {-myMsg.Dimensions[0], myMsg.Dimensions[1], myMsg.Dimensions[2]}));
            if (desiredDimensions != Vector3.zero)
            {
                myShape.transform.localScale = desiredDimensions;
            }
        }

        if (myMsg.Rotation.Count == 3)
        {
            myShape.transform.localRotation = OrbitVectorMath.ConvertRightHandedMRPtoLeftHandedQuaternion(new[]
                {myMsg.Rotation[0], myMsg.Rotation[1], myMsg.Rotation[2]});
        }
        

        Color nullColor = new Color(0, 0, 0, 0);
        if (myMsg.PositiveColor.Count >= 3)
        {
            if (myMsg.PositiveColor.Count >= 4)
            {
                 Color testColor = new Color(myMsg.PositiveColor[0] / 255f, myMsg.PositiveColor[1] / 255f, myMsg.PositiveColor[2] / 255f, myMsg.PositiveColor[3]/255f);
                if (testColor != nullColor)
                {
                    positiveColor = testColor;
                }
            }
            else
            {
                positiveColor = new Color(myMsg.PositiveColor[0] / 255f, myMsg.PositiveColor[1] / 255f, myMsg.PositiveColor[2] / 255f, positiveColor.a);
            }
        }
        
        if (myMsg.NegativeColor.Count >= 3)
        {
            if (myMsg.NegativeColor.Count >= 4)
            {
                Color testColor = new Color(myMsg.NegativeColor[0] / 255f, myMsg.NegativeColor[1] / 255f, myMsg.NegativeColor[2] / 255f, myMsg.NegativeColor[3]/255f);
                if (testColor != nullColor)
                {
                    negativeColor = testColor;
                }
            }
            else
            {
                negativeColor = new Color(myMsg.NegativeColor[0] / 255f, myMsg.NegativeColor[1] / 255f, myMsg.NegativeColor[2] / 255f, negativeColor.a);
            }
        }

        if (myMsg.NeutralOpacity >= 0)
        {
            defaultAlphaValue = myMsg.NeutralOpacity;
        }
        
        neutralColor = new Color(1f, 1f, 1f, defaultAlphaValue / 255f);
        
        maxChargeValue = myMsg.MaxValue;

        // Debug.Log($"Positive color set to: {positiveColor.r}, {positiveColor.g}, {positiveColor.b},{positiveColor.a}/n" +
        //           $"Negative color set to: {negativeColor.r}, {negativeColor.g}, {negativeColor.b},{negativeColor.a}/n" +
        //           $"Neutral color set to: {neutralColor.r}, {neutralColor.g}, {neutralColor.b},{neutralColor.a}");
        SetColorForCurrentCharge(myMsg.CurrentValue);

    }

    private void SetColorForCurrentCharge(double currentValue)
    {
        float currentChargeToMaxRatio = (float) (currentValue / maxChargeValue);

        myShape.GetComponent<Renderer>().material.color = currentChargeToMaxRatio >= 0 ? 
            Color.Lerp(neutralColor, positiveColor, currentChargeToMaxRatio) : 
            Color.Lerp(neutralColor, negativeColor, -currentChargeToMaxRatio);
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

    public void SetLabelAndLabelState(GameObject label, bool labelOn)
    {
        myLabel = label;
        labelSettingOnForThisSC = labelOn;
        myLabel.SetActive(labelSettingOnForThisSC);
    }
}
