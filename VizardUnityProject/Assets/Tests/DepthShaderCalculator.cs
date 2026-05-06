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
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DepthShaderCalculator : MonoBehaviour, IPointerClickHandler
{
    public Image depthColor;
    public Camera mainCamera;
    public TMP_Text redInput;
    public TMP_Text greenInput;
    public TMP_Text blueInput;
    public TMP_Text clippingPlanes;
    [FormerlySerializedAs("depthLayer")] public EnableDepthLayer enableDepthLayer;
    public Button runTestButton;
    public InstrumentCameraMethods testCamera;
    private Color currentColor=Color.white;


    void Start()
    {
        runTestButton.onClick.AddListener(TriggerDepthTest);
    }
    // Update is called once per frame
    void Update()
    {
        currentColor = depthColor.color;
        float farClippingPlane = mainCamera.farClipPlane;
        float depth = farClippingPlane * (((((currentColor.b * 255f) / 255f + currentColor.g * 255f) / 255f) + currentColor.r * 255f) / 255f);
        //float depth = farClippingPlane * currentColor.r;
        GetComponent<TMP_Text>().text = $"{depth}";
        redInput.text = $"Red: {currentColor.r * 255f}";
        greenInput.text = $"Green: {currentColor.g * 255f}";
        blueInput.text = $"Blue: {currentColor.b * 255f}";
        clippingPlanes.text = $"Near: {mainCamera.nearClipPlane} Far: {farClippingPlane}";
    }
    
    public void OnPointerClick(PointerEventData data)
    {
        //Texture2D _texture2D = depthLayer.camOutput.
       //currentColor = depthLayer.camOutput.GetPixel ((int)((int)pickpos.x*205/150),(int) ((150+(int)pickpos.y)*202/150));

    }

    public void TriggerDepthTest()
    {
        testCamera.takeTestImage = true;
    }
}
