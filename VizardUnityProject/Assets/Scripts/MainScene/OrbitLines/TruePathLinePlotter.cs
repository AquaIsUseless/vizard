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
/// <summary>
/// Draws the true path orbit or ground track line with OpenGL and/or Unity LineRenderers
/// </summary>
public class TruePathLinePlotter : MonoBehaviour
{
    [HideInInspector]public bool isOrbitLine;
    private Transform truePathLineGroup;
    private List<LineRenderer> truePathLinePieces = new List<LineRenderer>();
    
    public Color defaultTruePathColor = new Color(0f, (210f / 255f), 1f, 1f);
    public List<Color> lineColors = new List<Color>();
    public List<int> colorChangeIndices;
    [HideInInspector]public bool applyLineColorChanges;
    static Material lineMaterial;
    [HideInInspector] public bool plotWithOpenGL = true;
    [HideInInspector] public bool plotWithLineRenderers = true;
    [HideInInspector] public Vector3[] pointsToDrawOnscreen = new Vector3[] { };
    public void InitializeDrawTruePathLine(bool isUnitTest=false)
    {
        lineColors.Add(defaultTruePathColor);
        if (!isUnitTest)
        {
            truePathLineGroup = transform.GetChild(0);
            truePathLinePieces.Add(truePathLineGroup.GetChild(0).GetComponent<LineRenderer>());
            UpdateLineThickness(SpacecraftStateUtilities.GetCurrentSpacecraftOrbitLineWidth());
        }
    }

    void OnEnable()
    {
        float lineThickness = isOrbitLine
            ? SpacecraftStateUtilities.GetCurrentSpacecraftOrbitLineWidth()
            : SpacecraftStateUtilities.GetCurrentGroundTrackLineWidth();
        UpdateLineThickness(lineThickness);
    }
    
    public void BuildTrajectoryColorHistory(int spacecraftIndex)
    {
        MessageList.GetTruePathColorHistory(this, spacecraftIndex, isOrbitLine);
        applyLineColorChanges = lineColors.Count > 1;
    }

    public void OnRenderObject() 
    {
        if (isOrbitLine) //Ground track lines dont use the OpenGL call
        {
            //This is to draw the OpenGL true path lines
            if (VizardGUISettings.TruePathLinesVisible && plotWithOpenGL)
            {
                if ((pointsToDrawOnscreen.Length > 2) && (Camera.current.CompareTag("MainCamera")))
                {

                    int changeIndex = 1;
                    int startFrameNumber = MessageList.FirstMessageIndexOfPlottedMessages;
                    CreateLineMaterial();

                    GL.PushMatrix();
                    lineMaterial.SetPass(0);
                    GL.MultMatrix(transform.localToWorldMatrix);

                    GL.Begin(GL.LINES);
                    GL.Color(lineColors[0]);
                    for (int i = 0;
                         i < (pointsToDrawOnscreen.Length - 1);
                         i++) // End at less than one because we need two points to draw line
                    {
                        if (applyLineColorChanges)
                        {
                            if (i + startFrameNumber == colorChangeIndices[changeIndex])
                            {
                                GL.Color(lineColors[changeIndex]);
                                changeIndex += 1;
                                if (changeIndex >= colorChangeIndices.Count)
                                {
                                    changeIndex = 0;
                                }
                            }
                        }

                        GL.Vertex3(pointsToDrawOnscreen[i].x, pointsToDrawOnscreen[i].y, pointsToDrawOnscreen[i].z);
                        GL.Vertex3(pointsToDrawOnscreen[i + 1].x, pointsToDrawOnscreen[i + 1].y,
                            pointsToDrawOnscreen[i + 1].z);
                    }

                    GL.End();
                    GL.PopMatrix();
                }
            }
        }
    }

    public void PlotPointsTruePathLineRenderers()
    {
        if (plotWithLineRenderers)
        {
            int additionalLinesNeededCount = colorChangeIndices.Count - truePathLinePieces.Count;
            if (additionalLinesNeededCount > 0)
            {
                for (int i = 0; i < additionalLinesNeededCount; i++)
                {
                    GameObject extraLine = Instantiate(Resources.Load("Prefabs/TruePathLineRenderer") as GameObject,
                        Vector3.zero, Quaternion.identity, truePathLineGroup);
                    extraLine.GetComponent<LineRenderer>().useWorldSpace = isOrbitLine;
                    
                    if(!isOrbitLine){
                        extraLine.transform.localPosition = Vector3.zero;
                    }
                    truePathLinePieces.Add(extraLine.GetComponent<LineRenderer>());
                }

                float lineThickness = isOrbitLine
                    ? SpacecraftStateUtilities.GetCurrentSpacecraftOrbitLineWidth()
                    : SpacecraftStateUtilities.GetCurrentGroundTrackLineWidth();
                UpdateLineThickness(lineThickness);
            }

            int changeIndex = 1;
            int lineRendererInUse = 0;
            Color currentColor = lineColors[0];
            int startFrameNumber = MessageList.FirstMessageIndexOfPlottedMessages;
            int startIndexCurrentLine = 0;
            if (pointsToDrawOnscreen.Length == 0)
            {
                DisableExcessLineRenderers(-1);
            }
            else
            {
                if (applyLineColorChanges)
                {
                    for (int i = 0; i < pointsToDrawOnscreen.Length; i++)
                    {
                        int endIndexCurrentLine = i;
                        if (i + startFrameNumber == colorChangeIndices[changeIndex])
                        {
                            SetCurrentLineRenderer(truePathLinePieces[lineRendererInUse], currentColor,
                                startIndexCurrentLine,
                                (endIndexCurrentLine - startIndexCurrentLine) + 1);
                            currentColor = lineColors[changeIndex];
                            changeIndex++;
                            startIndexCurrentLine = endIndexCurrentLine;
                            lineRendererInUse++;
                            if (changeIndex == colorChangeIndices.Count)
                            {
                                break;
                            }
                        }
                    }

                    SetCurrentLineRenderer(truePathLinePieces[lineRendererInUse], currentColor, startIndexCurrentLine,
                        pointsToDrawOnscreen.Length-startIndexCurrentLine);
                    
                    DisableExcessLineRenderers(lineRendererInUse);
                }
                else
                {
                    SetCurrentLineRenderer(truePathLinePieces[0], lineColors[0], 0,
                        pointsToDrawOnscreen.Length);
                    DisableExcessLineRenderers(0);
                }
            }
        }
    }

    private void SetCurrentLineRenderer(LineRenderer currentLine, Color currentColor, int startIndex, int count)
    {
        currentLine.startColor = currentColor;
        currentLine.endColor = currentColor;
        currentLine.positionCount = count;

        Vector3[] linePositionsForCurrentLine = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            linePositionsForCurrentLine[i] = pointsToDrawOnscreen[i + startIndex];
        }
        currentLine.SetPositions(linePositionsForCurrentLine);
    }

    private void DisableExcessLineRenderers(int lastUsedIndex)
    {
        for (int i = lastUsedIndex + 1; i < truePathLinePieces.Count; i++)
        {
            truePathLinePieces[i].positionCount = 0;
        }
    }

    public void UpdateLineThickness(float newWidth)
    {
        foreach (LineRenderer line in truePathLinePieces)
        {
            line.startWidth = newWidth;
            line.endWidth = newWidth;
        }
    }
    
    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int) UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }
    
    public void ToggleLinePlotters(bool showOpenGLLines, bool showLineRendererLines)
    {
        plotWithOpenGL = showOpenGLLines;
        if (isOrbitLine)
        {
            plotWithLineRenderers = showLineRendererLines && VizardGUISettings.TruePathLinesVisible;
        }
        else
        {
            plotWithLineRenderers = showLineRendererLines && VizardGUISettings.TruePathGroundTrackOn;
        }

        transform.GetChild(0).gameObject.SetActive(plotWithLineRenderers);
    }
    
}