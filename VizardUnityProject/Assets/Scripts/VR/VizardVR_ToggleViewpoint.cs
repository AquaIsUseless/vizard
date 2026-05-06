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
/// VR: Command main camera position to the orthogonal view selected by user in the Viewpoint Radial menu
/// </summary>
public class VizardVR_ToggleViewpoint : MonoBehaviour
{
    private VizardVR_MainCameraMovementController mainCameraMovementController; //Vizard Main Scene camera controller
    /// <summary>
    /// Monodevelop method called before first update
    /// <remarks>Sets up radial menu options and gets reference to camera controller.</remarks>
    /// </summary>
    void Start()
    {
        mainCameraMovementController = Camera.main.GetComponent<VizardVR_MainCameraMovementController>();

        GetComponent<VizardVR_RadialMenuMethods>().InitializeRadialSectionsDynamically();
    }
/// <summary>
/// Change main camera viewpoint to Top View of current camera target
/// </summary>
    public void setTopView(){
        mainCameraMovementController.SetVRviewpoint(2);
    }
    /// <summary>
    /// Change main camera viewpoint to Front View of current camera target
    /// </summary>
    public void setFrontView(){
        mainCameraMovementController.SetVRviewpoint(0);
    }
    /// <summary>
    /// Change main camera viewpoint to Bottom View of current camera target
    /// </summary>
    public void setBottomView(){
        mainCameraMovementController.SetVRviewpoint(5);
    }
    /// <summary>
    /// Change main camera viewpoint to Rear View of current camera target
    /// </summary>
    public void setRearView(){
        mainCameraMovementController.SetVRviewpoint(3);
    }
    /// <summary>
    /// Change main camera viewpoint to Left View of current camera target
    /// </summary>
    public void setLeftView(){
        mainCameraMovementController.SetVRviewpoint(4);
    }
    /// <summary>
    /// Change main camera viewpoint to Right View of current camera target
    /// </summary>
    public void setRightView(){
        mainCameraMovementController.SetVRviewpoint(1);
    }
}