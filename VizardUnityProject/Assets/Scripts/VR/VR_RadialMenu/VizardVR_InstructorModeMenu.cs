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
/// Provides the Vizard VR main menu options for instructor mode
/// if live streaming a scenario
/// </summary>
public class VizardVR_InstructorModeMenu : MonoBehaviour
{
    private bool firstEnable = true; //True if this is the first time this menu has been enabled

    /// <summary>
    /// Monodevelop method that is called when attached gameObject is enabled
    /// <remarks>Used to populate the radial sections for this menu</remarks>
    /// </summary>
    void OnEnable()
    {
        if (firstEnable)
        {
            //Create the radial sections for those options
            GetComponent<VizardVR_RadialMenuMethods>().InitializeRadialSectionsDynamically();
            firstEnable = false;
        }
    }

    /// <summary>
    /// Force the instructors view menu settings to be synchronized
    /// with the trainees' Vizard instances
    /// </summary>
    public void InstructorControlMode_BroadcastSyncOn()
    {
        if (!DataManager.SocketIsReceiveOnly)
        {
            VizInputUtilities.ForceBroadcastSyncSettings = true;
        }
    }

    /// <summary>
    /// Release the trainees' Vizard instances to set their
    /// view menu options locally
    /// </summary>
    public void StudentControlMode_BroadCastSyncOff()
    {
        if (!DataManager.SocketIsReceiveOnly)
        {
            VizInputUtilities.ForceBroadcastSyncSettings = false;
        }
    }
}