using System;
using UnityEngine;
using System.Linq;
using VizProtobufferMessage;
/// <summary>
/// Calculates the fixed frame position history for the
/// true path ground track line of a spacecraft
/// </summary>
public class GroundTrackTruePath : MonoBehaviour
{
    public GameObject markerSphere;
    private TruePathLinePlotter truePathLinePlotter;
    private SpacecraftController sc;    
    private int scIndex;
    private int bodyIndex=-1;
    private string lastParentBodySetting;
    private GameObject parentBody;
    private int visibleHistoryUpdateCount = -1;

    private double[,] mySpacecraftPositionHistoryBSK;
    private double[,] fixedFrameDCMs;
    private double[,] fixedFrameRelativePositions_BSK;

    private bool alreadyInAppend;
    private int lastIndex;
    private bool forceGroundTrackUpdate;
    
    private readonly float bodyOffsetFactor = 1.001f;
    private readonly float helioMarkerScale = 0.02f;
    private readonly float planetMarkerScale = 0.01f;
    private readonly float scMarkerScale = 0.005f;
    

    void Awake()
    {
        truePathLinePlotter = GetComponent<TruePathLinePlotter>();
        truePathLinePlotter.isOrbitLine = false;
    }
    void FixedUpdate()
    {
        if (VizardGUISettings.TruePathGroundTrackOn)
        {
            bool forceUpdateOfParentBodyData = false;
            string currentBodyNameSetting = MessageList.CurrentMessage.Spacecraft[scIndex].GroundTrackBodyName;

            if (String.IsNullOrEmpty(currentBodyNameSetting))
            {
                if (sc.spacecraftParentBodyIndex != bodyIndex)
                {
                    bodyIndex = sc.spacecraftParentBodyIndex;
                    forceUpdateOfParentBodyData = true;
                }
            }
            else if (currentBodyNameSetting != lastParentBodySetting)
            {
                int newParentBodyIndex = CelestialBodyStateUtilities.GetCelestialBodyIndex(currentBodyNameSetting);
                if (newParentBodyIndex >= 0)
                {
                    lastParentBodySetting = MessageList.CurrentMessage.Spacecraft[scIndex].GroundTrackBodyName;
                    bodyIndex = newParentBodyIndex;
                    forceUpdateOfParentBodyData = true;
                }
            }

            lastParentBodySetting = currentBodyNameSetting;

            if (visibleHistoryUpdateCount!=MessageList.VisibleHistoryUpdateCount)
            {
                if (!MessageList.InBufferLoad)
                {
                    BuildPositionHistoryBSK();
                    SetParentBody();
                    BuildFixedFrameData();
                    truePathLinePlotter.BuildTrajectoryColorHistory(scIndex);
                    visibleHistoryUpdateCount = MessageList.VisibleHistoryUpdateCount;
                }
            }else if (forceUpdateOfParentBodyData)
            {
                    SetParentBody();
                    BuildFixedFrameData();
                    forceUpdateOfParentBodyData = false;
            }else if ((DataManager.IsLiveSim) &&
                      (mySpacecraftPositionHistoryBSK.GetLength(0) != MessageList.TimestepsTotal))
            {
                if (!alreadyInAppend)
                {
                    AppendNewMessageData();
                }
            }
            
            transform.position = parentBody.transform.position;
            transform.localScale = parentBody.transform.localScale;
            if ((lastIndex != MessageList.CurrentIndex) || (forceGroundTrackUpdate))
            {
                lastIndex = MessageList.CurrentIndex;
                UpdatePointsToDraw();
                forceGroundTrackUpdate = false;
            }
            
        }
    }
    public void InitializeTruePathGroundTrack(int mySCIndex)
    {
        scIndex = mySCIndex;
        sc = SpacecraftStateUtilities.GetSpacecraftObject(scIndex).GetComponent<SpacecraftController>();
        truePathLinePlotter.InitializeDrawTruePathLine();
        
        string bodySetting = MessageList.CurrentMessage.Spacecraft[scIndex].GroundTrackBodyName;
        bodyIndex = sc.spacecraftParentBodyIndex;
        if (!String.IsNullOrEmpty(bodySetting))
        {
            int bodyToDrawOn = CelestialBodyStateUtilities.GetCelestialBodyIndex(bodySetting);
            if (bodyToDrawOn >= 0)
            {
                bodyIndex = bodyToDrawOn;
            }
        }

        SetParentBody();
    }

    private void SetParentBody()
    {
        parentBody = CelestialBodyStateUtilities.GetCelestialBodyObject(bodyIndex);
    }

    private void BuildPositionHistoryBSK()
    {
        mySpacecraftPositionHistoryBSK = MessageList.GetPositionHistoryBSK(true, scIndex);
    }

    private void BuildFixedFrameData()
    {
        fixedFrameRelativePositions_BSK = MessageList.GetPositionHistoryBSK(false, bodyIndex);
        fixedFrameDCMs = MessageList.GetRotationHistoryDCM_BSK(VizardGUISettings.FixedBodyIsSpacecraft, bodyIndex);

        double[] relativePositionBSK = {0, 0, 0};
        double[] fixedFrameDCM = new double[9];
        for (int i = 0; i < Mathf.Min(fixedFrameRelativePositions_BSK.GetLength(0), mySpacecraftPositionHistoryBSK.GetLength(0)); i++)
        {
            for (int j = 0; j < 9; j++)
            {
                fixedFrameDCM[j] = fixedFrameDCMs[i, j];
            }
            for (int j = 0; j < 3; j++)
            {
                relativePositionBSK[j] =
                    mySpacecraftPositionHistoryBSK[i, j] - fixedFrameRelativePositions_BSK[i, j];
            }
            
            relativePositionBSK = OrbitVectorMath.ApplyTransformationMatrixToVector(fixedFrameDCM, relativePositionBSK);
           
            for (int j = 0; j < 3; j++)
            {
                fixedFrameRelativePositions_BSK[i, j] = relativePositionBSK[j];
            }
        }
    }

    private void AppendNewMessageData()
    {
        int currentMsgCount = MessageList.TimestepsTotal;
        double[,] copySpacecraftPositionHistoryBSK = new double[currentMsgCount, 3];
        double[,] copyScRelativeToFixedBodyPositions_fixedBSK = new double[currentMsgCount, 3];
        double[,] copyFixedFrameDCMs = new double[currentMsgCount, 9];
        for (int i = 0; i < mySpacecraftPositionHistoryBSK.GetLength(0); i++)
        {
            for (int j = 0; j < 3; j++)
            {
                copySpacecraftPositionHistoryBSK[i, j] = mySpacecraftPositionHistoryBSK[i, j];
                copyScRelativeToFixedBodyPositions_fixedBSK[i, j] = fixedFrameRelativePositions_BSK[i, j];
            }
            for (int j = 0; j < 9; j++)
            {
                copyFixedFrameDCMs[i, j] = fixedFrameDCMs[i, j];
            }
        }

        for (int k = mySpacecraftPositionHistoryBSK.GetLength(0); k < currentMsgCount; k++)
        {
            alreadyInAppend = true;
            VizMessage newMsg = MessageList.GetMessageAtIndex(k);
            if (newMsg != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    copySpacecraftPositionHistoryBSK[k, i] = newMsg.Spacecraft[scIndex].Position[i];
                }
                
                double[] fixedBodyPosition_inertialBSK = newMsg.CelestialBodies[bodyIndex].Position.ToArray();
                double[] fixedFrameDCM = newMsg.CelestialBodies[bodyIndex].Rotation.ToArray();
                double[] relativePositionBSK =
                    OrbitVectorMath.Subtract(newMsg.Spacecraft[scIndex].Position.ToArray(),
                        fixedBodyPosition_inertialBSK);
                double[] relativePositionFixedFrameRighthanded =
                    OrbitVectorMath.ApplyTransformationMatrixToVector(fixedFrameDCM, relativePositionBSK);
                for (int j = 0; j < 3; j++)
                {
                    copyScRelativeToFixedBodyPositions_fixedBSK[k, j] = relativePositionFixedFrameRighthanded[j];
                }

                for (int j = 0; j < 9; j++)
                {
                    copyFixedFrameDCMs[k, j] = fixedFrameDCM[j];
                }
            }
        }
        mySpacecraftPositionHistoryBSK = copySpacecraftPositionHistoryBSK;
        fixedFrameRelativePositions_BSK = copyScRelativeToFixedBodyPositions_fixedBSK;
        fixedFrameDCMs = copyFixedFrameDCMs;
        alreadyInAppend = false;
    }

    private void UpdatePointsToDraw()
    {
        if (truePathLinePlotter.pointsToDrawOnscreen.Length != fixedFrameRelativePositions_BSK.GetLength(0))
        {
            truePathLinePlotter.pointsToDrawOnscreen = new Vector3[fixedFrameRelativePositions_BSK.GetLength(0)];
        }

        int currentIndex = MessageList.CurrentIndex - MessageList.FirstMessageIndexOfPlottedMessages;
        double[] DCM_T_CurrentIndex = new double[]{1,0,0,0,1,0,0,0,1};
        try
        {
            for (int i = 0; i < 9; i++)
            {
                DCM_T_CurrentIndex[i] = fixedFrameDCMs[currentIndex,i];
            }

            DCM_T_CurrentIndex = OrbitVectorMath.TransposeMatrix(DCM_T_CurrentIndex);
        }
        catch
        {
            Debug.Log(
                $"Broken for current Index of {currentIndex} given MessageList.CurrentIndex: {MessageList.CurrentIndex} and FirstIndex of Plotted messages ({MessageList.FirstMessageIndexOfPlottedMessages}");
        }

        double[] fixedPosition = {0, 0, 0};
        for (int i = 0; i < truePathLinePlotter.pointsToDrawOnscreen.Length; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                fixedPosition[j] = fixedFrameRelativePositions_BSK[i, j];
            }

            fixedPosition = OrbitVectorMath.ApplyTransformationMatrixToVector(DCM_T_CurrentIndex, fixedPosition);
            fixedPosition = OrbitVectorMath.TransformFromBSKCStoUnity(fixedPosition);
            truePathLinePlotter.pointsToDrawOnscreen[i] = OrbitVectorMath.ReturnVector3(fixedPosition);
            truePathLinePlotter.pointsToDrawOnscreen[i] = bodyOffsetFactor*truePathLinePlotter.pointsToDrawOnscreen[i].normalized;
        }

        markerSphere.transform.localPosition = truePathLinePlotter.pointsToDrawOnscreen[currentIndex];
        truePathLinePlotter.PlotPointsTruePathLineRenderers();
    }
    
    public void ToggleTruePathGroundTrackLine(bool isOn)
    {
        truePathLinePlotter.ToggleLinePlotters(false,isOn);
        markerSphere.SetActive(isOn);
        if (isOn)
        {
            UpdateMarkerAppearance();
        }
    }

    public void UpdateMarkerAndLineRendererLineThickness(float newValue)
    {
        UpdateMarkerAppearance();
        truePathLinePlotter.UpdateLineThickness(newValue);
        forceGroundTrackUpdate = true;
    }

    private void UpdateMarkerAppearance()
    {
        markerSphere.transform.localScale = (CelestialBodyStateUtilities.ViewIsLocal?(CelestialBodyStateUtilities.ViewIsSpacecraftLocal?scMarkerScale:planetMarkerScale):helioMarkerScale)*Vector3.one;
    }
}
