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
using System.Linq;
using UnityEngine;
using VizProtobufferMessage;

/// <summary>
/// Accumulates the most recent user inputs to EventDialogs
/// and key inputs to provide to live Basilisk sim
/// </summary>
public class VizInputAccumulator : MonoBehaviour
{
    [Header("Event Dialog Manager Reference")]
    public EventDialogManager eventDialogManager;
    private string listenerKeys = "";
    private string currentHotKeys = "cvqtdroahlsf";
    [HideInInspector] public string currentKeys = "";
    private List<VizEventReply> lastFrameReplies = new List<VizEventReply>();
    private int repetitionCount;
    private int maxRepeatCount = 5;

    void Start()
    {
        VizInputUtilities.keyboardAccumulator = this;
    }
    // Update is called once per frame
    void Update()
    {
        if (UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject == null)
        {
            for (int i = 0; i < listenerKeys.Length; i++)
            {
                string lKey = listenerKeys.Substring(i, 1);
                if (Input.GetKey(lKey))
                {
                    if (!currentKeys.Contains(lKey))
                    {
                        currentKeys += lKey;
                    }
                }
            }
        }
    }

    private string GetCurrentKeys()
    {
        string keysToSend = currentKeys;
        currentKeys = "";
        return keysToSend;
    }

    public void SetListenerStringForKeyboard(string stringToUse)
    {
        string doubleBookedKeys = "";
        string keysToUse = "";
        
        if (stringToUse.Length > 0)
        {
            for (int i = 0; i < stringToUse.Length; i++)
            {
                string keyToCheck = stringToUse.Substring(i, 1);
                if ((currentHotKeys.Contains(keyToCheck)) && (!doubleBookedKeys.Contains(keyToCheck)))
                {
                    doubleBookedKeys += keyToCheck + ", ";
                }

                if (!keysToUse.Contains(keyToCheck))
                {
                    keysToUse += keyToCheck;
                }
            }
        }

        if (doubleBookedKeys.Length > 0)
        {
            VizardGUISettings.UpdateErrorMessages($"Keyboard input listeners have been set for the characters in this string: {stringToUse}. The following keys are already in use as hot keys in Vizard: {doubleBookedKeys} and using them for live input may result in unexpected behavior.", true);
        }
        listenerKeys = keysToUse;
        
    }

    public VizInput GetInputResponseMessage()
    {
        VizInput vizInputMessage = new VizInput()
        {
            FrameNumber = MessageList.CurrentIndex + 1,
            KeyInputs = new VizProtobufferMessage.VizInput.Types.KeyboardInput()
            {
                Keys = GetCurrentKeys()
            }
        };

        vizInputMessage.PlaybackState = MessageList.PlaybackPaused ? PlaybackState.PlaybackPaused : PlaybackState.PlaybackPlaying;

        if (eventDialogManager != null) //Because still in start-up screen
        {
            vizInputMessage.BroadcastSyncSettings = GetCurrentBroadcastSyncSettings();
            foreach (VizEventReply reply in eventDialogManager.CurrentReplies)
            {
                if (reply != null)
                {
                    vizInputMessage.Replies.Add(reply);
                    lastFrameReplies.Add(reply.Clone());
                    repetitionCount = 0;
                }
                //Dialog events are being relayed in Basilisk to the broadcast stream and do not have to be included here
                // vizInputMessage.BroadcastSyncSettings.DialogEvents.Add(reply);
            }
            
            if (repetitionCount<maxRepeatCount)
            {
                foreach (VizEventReply reply in lastFrameReplies)
                {
                    vizInputMessage.Replies.Add(reply);
                }
                
                repetitionCount += 1;
            }
            else
            {
                repetitionCount = 0;
                lastFrameReplies.Clear();
            }

            eventDialogManager.CurrentReplies.Clear();
        }
        return vizInputMessage;
    }

    private VizBroadcastSyncSettings GetCurrentBroadcastSyncSettings()
    {
        VizBroadcastSyncSettings syncSettings = new VizBroadcastSyncSettings() { };

        int oscOrbitsSetting=-1;
        if (VizardGUISettings.OsculatingOrbitLinesVisible)
        {
            oscOrbitsSetting = 1+(VizardGUISettings.SpacecraftRelativeOsculatingOrbits? 1 : 0);
        }

        int trueTrajSetting = -1;
        if (VizardGUISettings.TruePathLinesVisible)
        {
            trueTrajSetting = VizardGUISettings.TruePathLineMode;

            switch (VizardGUISettings.TruePathLineMode)
            {
                case 2: //Spacecraft relative
                    syncSettings.TruePathBodySetting.Add(1);
                    syncSettings.TruePathBodySetting.Add(VizardGUISettings.SetChiefToCamTgt?-1: VizardGUISettings.ChiefSpacecraftIndex);
                    break;
                case 3: //celestial body relative
                    syncSettings.TruePathBodySetting.Add(-1);
                    syncSettings.TruePathBodySetting.Add(VizardGUISettings.UseSpacecraftParentBodyForRelativeTraj?-1:VizardGUISettings.RelativeBodyIndex);
                    break;
                case 4: //rotating frame
                    syncSettings.TruePathBodySetting.Add(-1);
                    syncSettings.TruePathBodySetting.Add(VizardGUISettings.RotatingFrameBody1Index);
                    syncSettings.TruePathBodySetting.Add(VizardGUISettings.RotatingFrameBody2Index);
                    break;
                case 5: // fixed frame
                    syncSettings.TruePathBodySetting.Add(VizardGUISettings.FixedBodyIsSpacecraft?1:-1);
                    syncSettings.TruePathBodySetting.Add(VizardGUISettings.FixedBodyIndex);
                    break;
            }

        }

        syncSettings.ForceTrainerSettings = VizInputUtilities.ForceBroadcastSyncSettings;
        syncSettings.OrbitLinesOn = oscOrbitsSetting;
        syncSettings.TrueTrajectoryLinesOn = trueTrajSetting;
        syncSettings.SpacecraftCSon = (VizardGUISettings.AllSpacecraftCSOn ? 1: -1);
        syncSettings.PlanetCSon = (VizardGUISettings.AllPlanetCSOn ? 1: -1);
        syncSettings.ShowHillFrame = (VizardGUISettings.ShowHillFrame ? 1 : -1);
        syncSettings.ShowVelocityFrame = (VizardGUISettings.ShowVelocityFrame ? 1 : -1);
        foreach (string panelKey in eventDialogManager.CurrentEventDialogs.Keys.ToArray())
        {
            syncSettings.CurrentEventDialogs.Add(eventDialogManager.CurrentEventDialogs[panelKey]);
        }
        return syncSettings;
    }

}
