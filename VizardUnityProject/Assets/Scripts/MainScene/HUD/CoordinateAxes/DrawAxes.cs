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
/// Draws the coordinate frame axes with both
/// line renderers and OpenGL
/// </summary>
public class DrawAxes : MonoBehaviour
{
    public float lineScale = 5.0f;
    public bool modelAttached;
    public bool isAttachedToSpacecraft;
    static Material lineMaterial;

    private GameObject xLabel;
    private GameObject yLabel;
    private GameObject zLabel;

    public GameObject xPosition;
    public GameObject yPosition;
    public GameObject zPosition;

    public Vector3 axis1 = new Vector3(0, 0, -5);
    public Vector3 axis2 = new Vector3(5, 0, 0);
    public Vector3 axis3 = new Vector3(0, 5, 0);

    private Vector3 centralStartingPoint;
    private Vector3 xPt;
    private Vector3 yPt;
    private Vector3 zPt;

    public bool localFrame = true;
    public Transform lineRendererParent;
    public LineRenderer[] lineRenderers = new LineRenderer[3];
    private bool isModelTuningCS;

    void Start()
    {
        for (int i = 0; i < 3; i++)
        {
            LineRenderer line = lineRenderers[i];
            Color colorToUse = Color.black;
            colorToUse[i] = 1f;
            line.material.color = colorToUse;
            line.startColor = colorToUse;
            line.endColor = colorToUse;
        }

        UpdateLineRendererSettings(PersistentUserSettings.persistentSettingsFromLastSave
            .UseLineRenderersForTargetLinesAndFrames == 1);
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

    public void Update()
    {
        centralStartingPoint = Vector3.zero;
        xPt = new Vector3(axis1.x, axis1.y, axis1.z);
        yPt = new Vector3(axis2.x, axis2.y, axis2.z);
        zPt = new Vector3(axis3.x, axis3.y, axis3.z);

        if (!localFrame)
        {
            centralStartingPoint = transform.position;
            xPt += centralStartingPoint;
            yPt += centralStartingPoint;
            zPt += centralStartingPoint;
            xPosition.transform.position = xPt;
            yPosition.transform.position = yPt;
            zPosition.transform.position = zPt;
        }
    }

    // Will be called after all regular rendering is done
    public void OnRenderObject()
    {
        if (!isModelTuningCS) //Don't draw OpenGL Lines for model tuning (can't keep them out of main camera view)
        {
            // This draws the Basilisk right-handed coordinate axes where red = x, green = y, and blue = z
            // onto the screen using the Unity left-handed coordinate system internally
            CreateLineMaterial();
            // Apply the line material
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            // Set transformation matrix for drawing to
            // match our transform
            if (localFrame)
            {
                GL.MultMatrix(transform.localToWorldMatrix);
            }
            // The Basilisk coordinate frame is right-handed with z up. Unity uses a left-handed coordinate frame with y up.
            // To change to right handed with y up, Basilisk position <p0,p1,p2> becomes the intermediate  right-handed frame <p1 ,p2, p0>
            // To change that intermediate frame to a left-handed frame with y up, x right,z into screen: 
            // the z component must be made negative leaving us with: <p1, p2, -p0>

            // Draw lines
            GL.Begin(GL.LINES);
            // Vertex color to red 
            GL.Color(new Color(1, 0, 0, 0.8F));
            // One vertex at transform position
            GL.Vertex3(centralStartingPoint.x, centralStartingPoint.y, centralStartingPoint.z);
            // Another vertex at Basilisk +x (Unity -z)
            //GL.Vertex3(axis1.x,axis1.y,axis1.z);
            GL.Vertex3(xPt.x, xPt.y, xPt.z);

            // Vertex colors change from red to green
            GL.Color(new Color(0, 1, 0, 0.8F));
            // One vertex at transform position
            GL.Vertex3(centralStartingPoint.x, centralStartingPoint.y, centralStartingPoint.z);
            // Another vertex at Basilisk +y (Unity +x)
            //GL.Vertex3(axis2.x,axis2.y,axis2.z);
            GL.Vertex3(yPt.x, yPt.y, yPt.z);

            // Vertex colors change from green to blue
            GL.Color(new Color(0, 0, 1, 0.8F));
            // One vertex at transform position
            GL.Vertex3(centralStartingPoint.x, centralStartingPoint.y, centralStartingPoint.z);
            // Another vertex at Basilisk +z (Unity +y)
            //GL.Vertex3(axis3.x,axis3.y,axis3.z);
            GL.Vertex3(zPt.x, zPt.y, zPt.z);
            GL.End();
            GL.PopMatrix();
        }
    }

    void OnEnable()
    {
        if (modelAttached)
        {
            CalculateLineScale();
        }

        if (VizardGUISettings.ShowCSLabels)
        {
            if (xLabel != null)
            {
                xLabel.SetActive(true);
                yLabel.SetActive(true);
                zLabel.SetActive(true);
            }
        }
    }

    void OnDisable()
    {
        if (xLabel != null)
        {
            xLabel.SetActive(false);
            yLabel.SetActive(false);
            zLabel.SetActive(false);
        }
    }

    public void AttachCSLabels(GameObject x, GameObject y, GameObject z)
    {
        xLabel = x;
        yLabel = y;
        zLabel = z;

        xLabel.SetActive(false);
        yLabel.SetActive(false);
        zLabel.SetActive(false);
    }

    public void ChangeAxes(Vector3 a1, Vector3 a2, Vector3 a3)
    {
        float scaleToUse = lineScale * transform.parent.localScale.x;
        axis1 = scaleToUse * a1.normalized;
        axis2 = scaleToUse * a2.normalized;
        axis3 = scaleToUse * a3.normalized;
        lineRendererParent.rotation = Quaternion.LookRotation(-axis1, axis3);
    }

    private void ApplyAverageMeshDimUpdate(float newDimension)
    {
        lineScale = newDimension / transform.parent.localScale.x;
#if VIZARD_OPENXR
		lineScale = 10;
#endif
        axis1 = lineScale * axis1.normalized;
        axis2 = lineScale * axis2.normalized;
        axis3 = lineScale * axis3.normalized;
        if (localFrame)
        {
            xPosition.transform.localPosition = new Vector3(axis1.x, axis1.y, axis1.z);
            yPosition.transform.localPosition = new Vector3(axis2.x, axis2.y, axis2.z);
            zPosition.transform.localPosition = new Vector3(axis3.x, axis3.y, axis3.z);
        }

        if (PersistentUserSettings.persistentSettingsFromLastSave.UseLineRenderersForTargetLinesAndFrames == 1)
        {
            UpdateLineRendererSettings(true);
            lineRendererParent.localScale = lineScale * Vector3.one;
        }
    }

    public void CalculateLineScale()
    {
        Vector3 size = (SpacecraftStateUtilities.CalculateModelBounds(transform.parent.GetChild(0).gameObject)).size;
        float newLineScale = (size.x + size.y + size.z) * 0.75f / 3f;
        ApplyAverageMeshDimUpdate(newLineScale);
    }

    public void UpdateLineRendererSettings(bool isOn)
    {
        foreach (LineRenderer line in lineRenderers)
        {
            line.enabled = isOn;
        }

        if (isOn)
        {
            float lineThickness;
            if (isAttachedToSpacecraft)
            {
                lineThickness = SpacecraftStateUtilities.GetCurrentSpacecraftOrbitLineWidth()
                                * (float) PersistentUserSettings.persistentSettingsFromLastSave
                                    .LinesAndFramesLineWidth /
                                (float) PersistentUserSettings.persistentSettingsFromLastSave.SpacecraftOrbitLineWidth;
            }
            else
            {
                lineThickness = 0.2f * CelestialBodyStateUtilities.GetCurrentCelestialBodyOrbitLineWidth()
                                     * (float) PersistentUserSettings.persistentSettingsFromLastSave
                                         .LinesAndFramesLineWidth /
                                (float) PersistentUserSettings.persistentSettingsFromLastSave
                                    .CelestialBodyOrbitLineWidth;
            }

            foreach (LineRenderer line in lineRenderers)
            {
                line.startWidth = lineThickness;
                line.endWidth = lineThickness;
            }
        }
    }

    public void SetUpForModelTuningPanel()
    {
        isModelTuningCS = true;
        UpdateLineRendererSettings(true);
    }
}