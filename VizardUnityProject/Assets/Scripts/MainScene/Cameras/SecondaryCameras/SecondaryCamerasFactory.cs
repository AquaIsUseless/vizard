using UnityEngine;
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
using VizProtobufferMessage;
/// <summary>
/// Builds secondary camera prefabs and their associated display panels
/// for all VizMessage.CameraConfig (instrument cameras)  and
/// VizMessage.Settings.StandardCameraSettings (standard cameras) in current scenario
/// </summary>
public class SecondaryCamerasFactory : MonoBehaviour
{
    public void CreateInstrumentCameras(){
        //Create any custom cameras requested by the sim if not in OpNav mode
        if ((!DataManager.InNoDisplayMode)&&(MessageList.CurrentMessage.Cameras.Count > 0))
        {
            foreach (VizMessage.Types.CameraConfig thisConfig in MessageList.FirstMessage.Cameras)
            {
                CreateInstrumentCamera(thisConfig);
            }
        }
    }

    private static void CreateInstrumentCamera(VizMessage.Types.CameraConfig thisConfig)
    {
        GameObject myCamera = Instantiate (Resources.Load ("Prefabs/SpacecraftHUD/InstrumentCamera") as GameObject);

        myCamera.GetComponent<InstrumentCameraMethods> ().ConfigureInstrumentCamera(thisConfig);

        string camLabelText = $"InstrCam {thisConfig.CameraID}";
        GameObject camLabel = LabelMaker.CreateLabel(camLabelText, thisConfig.ParentName, myCamera, Vector2.zero, "Cameras");
        myCamera.GetComponent<SecondaryCameraHUDMethods> ().cameraLabel = camLabel;
        camLabel.SetActive(VizardGUISettings.ShowCameraLabels);
        MainCameraUtilities.InstrumentCameras.Add(myCamera);
    }
}
