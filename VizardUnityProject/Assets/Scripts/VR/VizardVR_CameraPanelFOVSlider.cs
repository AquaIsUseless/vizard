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
using UnityEngine.UI;

/// <summary>
/// Provides a UI Slider for Standard Camera Panels created in VR
/// to allow user to set the Field of View of the camera with a controller
/// </summary>
public class VizardVR_CameraPanelFOVSlider : MonoBehaviour
{
    public TextMeshProUGUI fovText; // UI text field showing current Field of View Setting in degrees
    private Slider mySlider; // UI Slider 
    private RectTransform mySliderHandle; // Rect Transform of Handle of UI Slider
    private StandardCameraPanelMethods cameraPanelMethods; //Reference to the attached Standard Camera Panel's methods

    /// <summary>
    /// Monodevelop method called before any Update calls
    /// <remarks>Used here to get necessary references on Standard Camera Panel creation</remarks>
    /// </summary>
    void Start()
    {
        mySlider = GetComponent<Slider>();
        mySlider.onValueChanged.AddListener(ChangeFOV);
        cameraPanelMethods = GetComponentInParent<StandardCameraPanelMethods>();
    }

    /// <summary>
    /// Called when the user changes the slider's value
    /// and passes the new field of view value to the attached camera.
    /// Updates the field of view value displayed in the text field
    /// </summary>
    /// <param name="value">[degrees] Desired Field of View of attached camera </param>
    private void ChangeFOV(float value)
    {
        cameraPanelMethods.ChangeCameraFieldOfView(value);
        fovText.text = $"{value}";
    }
}