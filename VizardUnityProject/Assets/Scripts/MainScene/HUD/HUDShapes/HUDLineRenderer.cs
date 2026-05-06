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
/// Sets up and updates the line renderer drawing
/// the HUD elements for sensor and camera frustums
/// and boresights.
/// </summary>
public class HUDLineRenderer : MonoBehaviour
{
    private float currentLineWidth;
    public LineRenderer myLine;
    private float meshDim = 3f;
    public bool updateLineWidth;
    private bool firstUpdate = true;

    void Start()
    {
        VizardGUISettings.AddHUDLine(this.GetComponent<HUDLineRenderer>());
    }
    void Update()
    {
        if (firstUpdate)
        {
            VizardGUISettings.AddHUDLine(this);
            SetMeshDim(meshDim);
            firstUpdate = false;
        }
        if (updateLineWidth)
        {
            Debug.Log($"Current line width is {currentLineWidth}");
            updateLineWidth = false;
        }
    }
     public void InitializeHUDLine(float modelDim)
    {
        
        SetMeshDim(modelDim);
    }

    public void SetLineWidth(float newWidth)
    {
        currentLineWidth = newWidth;
        
        if (CelestialBodyStateUtilities.ViewIsSpacecraftLocal)
        {
            if (meshDim > 1f)
            {
                currentLineWidth *= (float) CelestialBodyStateUtilities.SpacecraftLocalViewScale;
            }
        }
        else
        {
            if (meshDim < 1f)
            {
                currentLineWidth *= meshDim;
            }
        }
        myLine.startWidth = currentLineWidth;
        myLine.endWidth = currentLineWidth;
    }

    public void SetMeshDim(float newDim)
    {
        meshDim = newDim;
        SetLineWidth(SpacecraftStateUtilities.GetCurrentSpacecraftOrbitLineConstant());
    }
}
