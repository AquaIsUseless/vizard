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

/// <summary>
/// VR Radial menu class to allow user to access playback controls
/// </summary>
public class VizardVR_PlaybackControls : MonoBehaviour
{
    public ItsAboutTime timeMgr; //Vizard Time/Message Incrementing Class
    public TextMeshProUGUI simTime; //Text display of current message simElapsedTime
    public TextMeshProUGUI playbackSpeed; //[xRealTime] Test display of current playback rate

    /// <summary>
    /// Monodevelop method called before any Update calls
    /// </summary>
    void Start()
    {
        GetComponent<VizardVR_RadialMenuMethods>().InitializeRadialSectionsDynamically();
    }

    /// <summary>
    /// Monodevelop method called on every frame
    /// <remarks>Update the sim elapsed time display and the current playback rate</remarks>
    /// </summary>
    void FixedUpdate()
    {
        UpdateSimElapsedTimeDisplay();
        playbackSpeed.text = timeMgr.VR_RadialMenuGetPlaybackSpeed();
    }

    /// <summary>
    /// Toggles play/pause of message playback
    /// </summary>
    public void PlayPausePressed()
    {
        timeMgr.TogglePlaybackPause();
    }

    /// <summary>
    /// Jumps ahead in the messages by a tenth of the total messages
    /// </summary>
    public void JumpForwardPressed()
    {
        if (!MessageList.InBufferLoad)
        {
            float fractionToRequest = (float) MessageList.CurrentIndex / MessageList.TimestepsTotal + 0.1f;
            if (fractionToRequest > 1)
            {
                fractionToRequest = 1;
            }

            timeMgr.SetArchiveFraction(fractionToRequest);
        }
    }

    /// <summary>
    /// Jumps backwards in the messages by a tenth of the total messages
    /// </summary>
    public void JumpBackwardPressed()
    {
        if (!MessageList.InBufferLoad)
        {
            float fractionToRequest = (float) MessageList.CurrentIndex / MessageList.TimestepsTotal - 0.1f;
            if (fractionToRequest < 0)
            {
                fractionToRequest = 0;
            }

            timeMgr.SetArchiveFraction(fractionToRequest);
        }
    }

    /// <summary>
    /// Doubles the current rate of message playback
    /// </summary>
    public void FastForwardPressed()
    {
        if (!MessageList.InBufferLoad)
        {
            timeMgr.IncreasePlaybackSpeed();
        }
    }

    /// <summary>
    /// Halves the current rate of message playback
    /// </summary>
    public void SlowPlaybackPressed()
    {
        if (!MessageList.InBufferLoad)
        {
            timeMgr.DecreasePlaybackSpeed();
        }
    }

    /// <summary>
    /// Converts the VizMessage.SimElapsedTime in nanoseconds to Days:Hours:Minutes:Seconds
    /// and displays the resulting string in the SimElapsedTime text field of the radial menu
    /// </summary>
    private void UpdateSimElapsedTimeDisplay()
    {
        double currentSimElapsedSeconds =
            MessageList.CurrentMessage.CurrentTime.SimTimeElapsed / (1e9); //Gets sim time in seconds

        int simDays = (int) currentSimElapsedSeconds / 86400;
        string simDstr = simDays.ToString();
        if (simDays < 10)
        {
            simDstr = "0" + simDstr;
        }

        //Calculate how many hours in day fraction:
        int simHours = (int) (currentSimElapsedSeconds - simDays * 86400) / 3600;
        string simHstr = simHours.ToString();
        if (simHours < 10)
        {
            simHstr = "0" + simHstr;
        }

        //Calculate how many minutes in hour fraction:
        int simMins = (int) (currentSimElapsedSeconds - simDays * 86400 - simHours * 3600) / 60;
        string simMstr = simMins.ToString();
        if (simMins < 10)
        {
            simMstr = "0" + simMstr;
        }

        //Calculate how many seconds in minute fraction:
        double simSecs = currentSimElapsedSeconds - simDays * 86400 - simHours * 3600 - simMins * 60;
        string simSstr = simSecs.ToString("F1");
        if (simSecs < 10)
        {
            simSstr = "0" + simSstr;
        }

        simTime.text =
            string.Format("Sim Time Elapsed: {0}:{1}:{2}:{3}", simDstr, simHstr, simMstr, simSstr);
    }
}