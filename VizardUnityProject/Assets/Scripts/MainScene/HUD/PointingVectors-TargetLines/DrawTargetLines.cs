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
/// Draws all TargetLines in current VizMessage
/// </summary>
public class DrawTargetLines : MonoBehaviour
{
    private List<Vector3> startPoints = new List<Vector3>();
    private List<Vector3> endPoints = new List<Vector3>();
    private List<Color> lineColors = new List<Color>();
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();
    private bool useLineRenderers = true;
    private float currentWidth = 0.0625f;

    static Material lineMaterial;
    
    void Update()
    {
        startPoints.Clear();
        endPoints.Clear();
        lineColors.Clear();

        VizMessage.Types.LiveVizSettingsPb mySettings = MessageList.CurrentMessage.LiveSettings;

        if (mySettings != null)
        {
            foreach (VizMessage.Types.PointLine line in mySettings.TargetLines)
            {
                GameObject fromBody =
                    CelestialBodyStateUtilities.GetLineTargetGameObjectWithName(line.FromBodyName);
                if (fromBody != null)
                {
                    GameObject toBody =
                        CelestialBodyStateUtilities.GetLineTargetGameObjectWithName(line.ToBodyName);
                    if (toBody != null)
                    {
                        startPoints.Add(fromBody.transform.position);
                        endPoints.Add(toBody.transform.position);
                        lineColors.Add(GetLineColor(line));
                    }
                }
            }

            if (useLineRenderers)
            {
                while (lineRenderers.Count < startPoints.Count)
                {
                    GameObject newLine = Instantiate(Resources.Load("Prefabs/SpacecraftHUD/LineObject") as GameObject, transform);
                    LineRenderer newLineRenderer = newLine.GetComponent<LineRenderer>();
                    newLineRenderer.startWidth = currentWidth;
                    newLineRenderer.endWidth = currentWidth;
                    lineRenderers.Add(newLineRenderer);
                }
                
                for (int i = 0; i < startPoints.Count; i++)
                {
                    lineRenderers[i].enabled = true;
                    lineRenderers[i].startColor = lineColors[i];
                    lineRenderers[i].endColor = lineColors[i];
                    lineRenderers[i].material.color = lineColors[i];
                    lineRenderers[i].SetPositions(new[]{startPoints[i], endPoints[i]});
                }

                for (int i = startPoints.Count; i<lineRenderers.Count; i++)
                {
                    lineRenderers[i].SetPositions(new[]{Vector3.zero, Vector3.zero});
                    lineRenderers[i].enabled = false;
                }
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

    // Will be called after all regular rendering is done
    public void OnRenderObject()
    {
        CreateLineMaterial();
        // Apply the line material
        GL.PushMatrix();
        lineMaterial.SetPass(0);
        // Set transformation matrix for drawing to
        // match our transform
        GL.MultMatrix(transform.localToWorldMatrix);

        // Draw lines
        GL.Begin(GL.LINES);

        for (int i = 0; i < endPoints.Count; i++)
        {
            GL.Color(lineColors[i]);
            // One vertex at transform position
            GL.Vertex3(startPoints[i].x, startPoints[i].y, startPoints[i].z);
            // Another vertex at x
            GL.Vertex3(endPoints[i].x, endPoints[i].y, endPoints[i].z);
        }

        GL.End();
        GL.PopMatrix();
    }

    private Color GetLineColor(VizMessage.Types.PointLine line)
    {
        if (line.LineColor.Count < 3)
        {
            return Color.white;
        }

        float r = (Mathf.Clamp(line.LineColor[0] / 255f, 0f, 1f));
        float g = (Mathf.Clamp(line.LineColor[1] / 255f, 0f, 1f));
        float b = (Mathf.Clamp(line.LineColor[2] / 255f, 0f, 1f));
        float a = 1f;
        if (line.LineColor.Count == 4)
        {
            a = (Mathf.Clamp(line.LineColor[3] / 255f, 0f, 1f));
        }

        return new Color(r, g, b, a);
    }

    public void UpdateLineRendererSettings(bool isOn)
    {
        useLineRenderers = isOn;
        currentWidth = SpacecraftStateUtilities.GetCurrentSpacecraftOrbitLineConstant() * (float) PersistentUserSettings.persistentSettingsFromLastSave.LinesAndFramesLineWidth;
        if (isOn)
        {
            foreach (LineRenderer line in lineRenderers)
            {
                line.startWidth = currentWidth;
                line.endWidth = currentWidth;
            }
        }
        else
        {
            foreach (LineRenderer line in lineRenderers)
            {
                line.enabled = false;
            }
        }
    }
}