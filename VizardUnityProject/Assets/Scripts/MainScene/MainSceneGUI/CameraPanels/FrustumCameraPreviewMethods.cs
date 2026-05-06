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
/// Displays the view of a source camera inside that camera's frustum HUD
/// </summary>
public class FrustumCameraPreviewMethods : MonoBehaviour
{
    [Tooltip("Camera view to show in frustum")]
    public Camera sourceCamera;
    [Tooltip("Is the camera a standard camera or an instrument camera?")]
    public bool standardCamera;
    private Material frontMaterial;
    private Material backMaterial;
    private float length = 1f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void OnEnable()
    {
        frontMaterial = transform.GetChild(0).GetComponent<MeshRenderer>().material;
        backMaterial = transform.GetChild(1).GetComponent<MeshRenderer>().material;
        frontMaterial.mainTexture = sourceCamera.targetTexture;
        backMaterial.mainTexture = sourceCamera.targetTexture;
    }

    public void SetCameraPreviewSizeAndLocation(float maxExtent)
    {
        length = maxExtent;
        float distance = length * Mathf.Cos(sourceCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumHeight = 2.0f * distance * Mathf.Tan(sourceCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float frustumWidth;
        if (standardCamera)
        {
            frustumWidth = frustumHeight * sourceCamera.aspect;
        }
        else
        {
            //use the output image resolution to calculate aspect ratio
            float outputWidth = sourceCamera.GetComponent<InstrumentCameraMethods>().reqWidth;
            float outputHeight = sourceCamera.GetComponent<InstrumentCameraMethods>().reqHeight;
            frustumWidth = frustumHeight * (outputWidth / outputHeight);
        }

        transform.localPosition = new Vector3(0, 0, distance);
        transform.localScale = new Vector3(frustumWidth, frustumWidth, 1f);

    }

}
