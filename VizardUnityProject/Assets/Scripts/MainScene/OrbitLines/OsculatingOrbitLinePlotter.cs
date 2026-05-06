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
using UnityEngine;
/// <summary>
/// Draws the osculating orbit line with OpenGL and/or Unity LineRenderers
/// </summary>
public class OsculatingOrbitLinePlotter : MonoBehaviour
{
    static Material lineMaterial;
    public LineRenderer oscOrbitFutureLineRenderer;
    public LineRenderer oscOrbitPastLineRenderer;
    public TruePathLinePlotter truePathOrbitLinePlotter;
    public Color lineColor;
    private Vector3[] lineVertices;
    private int indexOfCurrentTimestepPoint;
    private readonly float minValue = 0.2f;
    private readonly float maxValue = 0.9f;
    private readonly float midValue = 0.55f;
    [HideInInspector] public bool plotWithOpenGL = true;
    [HideInInspector] public bool plotWithLineRenderer = true;

    void OnEnable()
    {
        CreateLineMaterial();
        oscOrbitFutureLineRenderer.enabled = plotWithLineRenderer;
        oscOrbitPastLineRenderer.enabled = plotWithLineRenderer;
    }

    void OnDisable()
    {
        oscOrbitFutureLineRenderer.enabled = false;
        oscOrbitPastLineRenderer.enabled = false;
    }

    //Plot an array of points as a continuous line
    public void PlotLine(Vector3[] pointsToPlot, int currentTimestepIndex)
    {
        indexOfCurrentTimestepPoint = currentTimestepIndex;
        Color startColor = lineColor;
        startColor.a = maxValue;
        Color endColor = lineColor;
        endColor.a = minValue;
        Color midColor = lineColor;
        midColor.a = midValue;


        int futureLength = pointsToPlot.Length - indexOfCurrentTimestepPoint;
        int pastLength = indexOfCurrentTimestepPoint + 1;
        if (indexOfCurrentTimestepPoint == 0)
        {
            pastLength = 0;
            midColor = endColor;
        }
        else if (indexOfCurrentTimestepPoint == pointsToPlot.Length - 1)
        {
            futureLength = 0;
            midColor = startColor;
        }

        Vector3[] futureSegment = new Vector3[futureLength];
        Vector3[] pastSegment = new Vector3[indexOfCurrentTimestepPoint + 1];

        Array.Copy(pointsToPlot, indexOfCurrentTimestepPoint, futureSegment, 0, futureLength);
        Array.Copy(pointsToPlot, 0, pastSegment, 0, pastLength);

        SetPointsAndColor(futureSegment, oscOrbitFutureLineRenderer, startColor, midColor);
        SetPointsAndColor(pastSegment, oscOrbitPastLineRenderer, midColor, endColor);

        lineVertices = pointsToPlot;

    }

    private void SetPointsAndColor(Vector3[] pointsToDraw, LineRenderer lineRendererToUse, Color colorToUseStart,
        Color colorToUseEnd) //, bool connectEndsInLoop)
    {
        if (plotWithLineRenderer)
        {
            lineRendererToUse.enabled = true;
            lineRendererToUse.startColor = colorToUseStart;
            lineRendererToUse.endColor = colorToUseEnd;
            lineRendererToUse.positionCount = pointsToDraw.Length;
            lineRendererToUse.SetPositions(pointsToDraw);
        }
        else
        {
            lineRendererToUse.positionCount = 0;
        }
    }

    public void UpdateLineRendererLineThickness(float newValue, bool isSpacecraftLine = false)
    {
        oscOrbitFutureLineRenderer.startWidth = newValue;
        oscOrbitFutureLineRenderer.endWidth = newValue;
        oscOrbitPastLineRenderer.startWidth = newValue;
        oscOrbitPastLineRenderer.endWidth = newValue;
        if (isSpacecraftLine)
        {
            truePathOrbitLinePlotter.GetComponent<TruePathLinePlotter>().UpdateLineThickness(newValue);
        }
    }

    public void UpdateLinePlotters(bool useOpenGL, bool useLineRenderer, bool isSpacecraftLine = false)
    {
        plotWithOpenGL = useOpenGL;
        plotWithLineRenderer = useLineRenderer;

        if (isSpacecraftLine)
        {
            truePathOrbitLinePlotter.ToggleLinePlotters(useOpenGL, useLineRenderer);
        }
    }

    public void OnRenderObject()
    {
        if (plotWithOpenGL)
        {
            if (Camera.current.CompareTag("MainCamera"))
            {
                lineMaterial.SetPass(0);

                GL.PushMatrix();
                GL.MultMatrix(transform.localToWorldMatrix);

                int vertexCount = lineVertices.Length;
                float startValue = midValue;
                float endValue = minValue;
                if (indexOfCurrentTimestepPoint == vertexCount - 1)
                {
                    startValue = maxValue;
                }

                //First draw the mostly transparent past trajectory of the parabolic/hyperbolic orbit
                //Plot orbit line
                GL.Begin(GL.LINES);
                GL.Color(new Color(lineColor.r, lineColor.g, lineColor.b, startValue));
                for (int i = 0; i < indexOfCurrentTimestepPoint; i++)
                {
                    GL.Vertex3(lineVertices[i].x, lineVertices[i].y, lineVertices[i].z);
                    GL.Vertex3(lineVertices[i + 1].x, lineVertices[i + 1].y, lineVertices[i + 1].z);
                    GL.Color(new Color(lineColor.r, lineColor.g, lineColor.b,
                        startValue - (1 - endValue) * ((float) i / vertexCount)));
                }

                GL.End();
                startValue = maxValue;
                endValue = midValue;
                if (indexOfCurrentTimestepPoint == 0)
                {
                    endValue = minValue;
                }

                //Now draw the future trajectory with the shading (most opaque closest to body)
                GL.Begin(GL.LINES);
                GL.Color(new Color(lineColor.r, lineColor.g, lineColor.b, maxValue));
                for (int i = indexOfCurrentTimestepPoint; i < vertexCount - 1; i++)
                {
                    GL.Vertex3(lineVertices[i].x, lineVertices[i].y, lineVertices[i].z);
                    GL.Vertex3(lineVertices[i + 1].x, lineVertices[i + 1].y, lineVertices[i + 1].z);
                    GL.Color(new Color(lineColor.r, lineColor.g, lineColor.b, startValue - (1 - endValue) *
                        ((float) (i -indexOfCurrentTimestepPoint)/vertexCount)));
                }
                GL.End();
                GL.PopMatrix();
            }
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
    
    
}