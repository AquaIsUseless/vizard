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
using VizProtobufferMessage;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Checks every message for EventDialogs that need
/// to be created or updated. Creates new dialogs as needed.
/// Processes updates to EventDialogs.
/// </summary>
public class EventDialogManager : MonoBehaviour
{
    public AlertIconManager alertIconMgr;
    public GameObject eventDialogPanels;
    public GameObject singleConfirmationPanel; //Used in VR
    public InputField testMessageInputField;
    public List<VizEventReply> CurrentReplies = new List<VizEventReply>();

    private Dictionary<string, GameObject> existingPanels = new Dictionary<string, GameObject>();
    private int lastProcessedIndex;
    private List<VizEventReply> lastSyncSettings = new List<VizEventReply>();
    public Dictionary<string, VizEventDialog> CurrentEventDialogs = new Dictionary<string, VizEventDialog>();
    private List<int[]> panelInGridPosition = new List<int[]>();

    private List<Vector2> gridLocations = new List<Vector2>()
    {
        new(0f, 0.25f), new(0f, 0f), new(0f, -0.25f),
        new(0.25f, 0.25f), new(0.25f, 0f), new(0.25f, -0.25f),
        new(-0.25f, 0.25f), new(-0.25f, 0f), new(-0.25f, -0.25f)
    };

    void Awake()
    {
        VizInputUtilities.eventDialogManager = this;
        if (testMessageInputField != null)
        {
            testMessageInputField.onSubmit.AddListener(SendTestMessage);
        }

        if (MessageList.FirstMessage.VizEventDialogs.Count > 0)
        {
            foreach (VizEventDialog dialog in MessageList.FirstMessage.VizEventDialogs)
            {
                BuildEventDialogPanel(dialog, MessageList.FirstMessage.CurrentTime.SimTimeElapsed);
            }
        }

        panelInGridPosition.Add(new int[9]);
    }

    void Update()
    {
        if (lastProcessedIndex != MessageList.CurrentIndex)
        {
            if (DataManager.IsLiveSim && DataManager.SocketIsReceiveOnly &&
                (VizInputUtilities.ForceBroadcastSyncSettings))
            {
                ApplyBroadcastSyncSettings();
            }
            else
            {
                for (int desiredIndex = lastProcessedIndex + 1;
                     desiredIndex <= MessageList.CurrentIndex;
                     desiredIndex++)
                {
                    VizMessage messageToProcess = MessageList.GetMessageAtIndex(desiredIndex);
                    if (messageToProcess != null)
                    {
                        ProcessEventDialogs(messageToProcess);
                    }
                    else
                    {
                        //If buffered playback then desiredIndex may no longer be available 
                        //in the current buffer, so 
                        desiredIndex = MessageList.FirstMessageIndexOfPlottedMessages;
                    }
                }
            }
        }

        lastProcessedIndex = MessageList.CurrentIndex;
    }

    private void ProcessEventDialogs(VizMessage currentMessage)
    {
        foreach (VizEventDialog dialog in currentMessage.VizEventDialogs)
        {
            if (existingPanels.ContainsKey(dialog.EventHandlerID))
            {
                existingPanels[dialog.EventHandlerID].GetComponent<EventDialogHandler>()
                    .UpdateEventDialogPanel(dialog, currentMessage.CurrentTime.SimTimeElapsed);
            }
            else
            {
                if (dialog.DurationOfDisplay != -1)
                {
                    BuildEventDialogPanel(dialog, currentMessage.CurrentTime.SimTimeElapsed);
                }
            }

            CurrentEventDialogs[dialog.EventHandlerID] = dialog.Clone();
        }
    }

    private void ApplyBroadcastSyncSettings()
    {
        if ((MessageList.LatestBroadcastSyncSettings.ForceTrainerSettings) || (VizInputUtilities.FirstSync))
        {
            CurrentEventDialogs = new Dictionary<string, VizEventDialog>();
            //Apply settings for current panels, build missing panels if needed
            foreach (VizEventDialog vizEventDialogMsg in MessageList.LatestBroadcastSyncSettings.CurrentEventDialogs)
            {
                CurrentEventDialogs.Add(vizEventDialogMsg.EventHandlerID, vizEventDialogMsg.Clone());
                if (!existingPanels.ContainsKey(vizEventDialogMsg.EventHandlerID))
                {
                    if (vizEventDialogMsg.DurationOfDisplay != -1)
                    {
                        BuildEventDialogPanel(vizEventDialogMsg, MessageList.CurrentMessage.CurrentTime.SimTimeElapsed);
                    }
                }
                else
                {
                    existingPanels[vizEventDialogMsg.EventHandlerID].GetComponent<EventDialogHandler>()
                        .UpdateEventDialogPanel(vizEventDialogMsg,
                            MessageList.CurrentMessage.CurrentTime.SimTimeElapsed);
                }

                if (VizInputUtilities.FirstSync)
                {
                    if (existingPanels.ContainsKey(vizEventDialogMsg.EventHandlerID))
                    {
                        if (vizEventDialogMsg.HideOnSelection == -100)
                        {
                            existingPanels[vizEventDialogMsg.EventHandlerID].GetComponent<EventDialogHandler>()
                                .BlockButtonAccess(true);
                            alertIconMgr.BlockButtonAccess(true);
                            existingPanels[vizEventDialogMsg.EventHandlerID].GetComponent<EventDialogHandler>()
                                .PanelMinimizeButtonWasClicked();
                        }
                        else if ((vizEventDialogMsg.HideOnSelection == -101) ||
                                 (vizEventDialogMsg.HideOnSelection == 0))
                        {
                            existingPanels[vizEventDialogMsg.EventHandlerID].GetComponent<EventDialogHandler>()
                                .BlockButtonAccess(true);
                            alertIconMgr.BlockButtonAccess(true);
                            existingPanels[vizEventDialogMsg.EventHandlerID].GetComponent<EventDialogHandler>()
                                .eventDialogPanel.SetActive(true);
                            alertIconMgr.RemoveButtonAndPanelFromInventory(
                                existingPanels[vizEventDialogMsg.EventHandlerID]);
                        }
                    }
                }
            }

            VizInputUtilities.FirstSync = false;

            List<VizEventReply> currentSyncSettings = new List<VizEventReply>();
            foreach (VizEventReply reply in MessageList.LatestBroadcastSyncSettings.DialogEvents)
            {
                if (!lastSyncSettings.Contains(reply))
                {
                    if (existingPanels.ContainsKey(reply.EventHandlerID))
                    {
                        existingPanels[reply.EventHandlerID].GetComponent<EventDialogHandler>()
                            .ApplyBroadcastPanelChoice(reply.Reply);
                    }
                }

                currentSyncSettings.Add(reply);
            }

            lastSyncSettings = currentSyncSettings;
        }
    }

    private void BuildEventDialogPanel(VizEventDialog dialogMsg, double simTimeAtDialogMsg)
    {
        string prefabPathToUse = "Prefabs/GUIPanels/EventDialogParent";

        if (DataManager.UseVR)
        {
            prefabPathToUse = "Prefabs/VR/VizardVR_EventDialogParent";
        }

        string formatToUse = dialogMsg.DialogFormat.ToUpper(); //double check that it's all upper case
        if ((formatToUse != "") && (formatToUse != "NONE")) //Think "NONE" is hardcoded into vizInterface as the default
        {
            prefabPathToUse += formatToUse;
        }

        GameObject newPanel;

        try
        {
            newPanel = Instantiate(Resources.Load(prefabPathToUse) as GameObject,
                eventDialogPanels.transform);
        }
        catch
        {
            VizardGUISettings.UpdateErrorMessages(
                $"{dialogMsg.EventHandlerID} EventDialog requested format: {dialogMsg.DialogFormat}. This panel format is not supported.");
            VizardGUISettings.ConsoleLog.SetActive(true);
            formatToUse = "";
            if (DataManager.UseVR)
            {
                newPanel = Instantiate(Resources.Load("Prefabs/VR/VizardVR_EventDialogParent") as GameObject,
                    eventDialogPanels.transform);
            }
            else
            {
                newPanel = Instantiate(Resources.Load("Prefabs/GUIPanels/EventDialogParent") as GameObject,
                    eventDialogPanels.transform);
            }
        }


        newPanel.GetComponent<EventDialogHandler>().InitializePanel(dialogMsg, simTimeAtDialogMsg, this, formatToUse);

        try
        {
            existingPanels.Add(dialogMsg.EventHandlerID, newPanel);
        }
        catch
        {
            Destroy(newPanel);
            VizardGUISettings.UpdateErrorMessages(
                $"Multiple EventDialog messages with EventHandlerID {dialogMsg.EventHandlerID} were sent in Frame Number {MessageList.CurrentMessage.CurrentTime.FrameNumber}.",
                true);
        }
    }
    
    public void ReceiveEventReply(VizEventReply reply, GameObject sender = null)
    {
        CurrentReplies.Add(reply);

        if (reply.EventHandlerDestroyed)
        {
            int[] gridMatrixValues = sender.GetComponent<EventDialogHandler>()
                .GetGridLayoutStartingPositionIndices();
            existingPanels.Remove(reply.EventHandlerID);
            CurrentEventDialogs.Remove(reply.EventHandlerID);
            alertIconMgr.RemoveButtonAndPanelFromInventory(sender);
            MakeSpaceInGridAvailable(gridMatrixValues[0], gridMatrixValues[1]);
            Destroy(sender);
        }
    }

    public void SendTestMessage(string testString)
    {
        VizEventReply testReply = new VizEventReply()
        {
            EventHandlerID = testString
        };

        ReceiveEventReply(testReply);
    }

    public void EventDialogMinimized(GameObject eventDialog, string panelType)
    {
        alertIconMgr.AddMinimizedPanel(eventDialog, panelType);
    }


    public void RemoveButtonAndPanelFromInventory(GameObject panel)
    {
        alertIconMgr.RemoveButtonAndPanelFromInventory(panel);
    }

    public void ShowEventConfirmationPanel(string eventReplyString)
    {
#if VIZARD_OPENXR
        singleConfirmationPanel.SetActive(true);
        singleConfirmationPanel.GetComponent<VizardVR_ConfirmationOnly>().ShowEventToConfirm(eventReplyString);
#endif
    }

    public void SetPanelRelayCurrentStatus(string eventHandlerID, int currentStatus)
    {
        if (CurrentEventDialogs.TryGetValue(eventHandlerID, out var dialog))
        {
            dialog.HideOnSelection = currentStatus;
        }
    }

    public void ReleaseButtonsOnAllEventDialogs()
    {
        foreach (string panelKey in existingPanels.Keys)
        {
            existingPanels[panelKey].GetComponent<EventDialogHandler>().BlockButtonAccess(false);
        }

        alertIconMgr.BlockButtonAccess(false);
    }

    public void SetPanelPosition(GameObject eventDialogParent)
    {
        RectTransform eventDialogPanelRect =
            eventDialogParent.GetComponent<EventDialogHandler>().eventDialogPanel.GetComponent<RectTransform>();
        RectTransform confPanelRect = eventDialogParent.GetComponent<EventDialogHandler>().confirmationPanel
            .GetComponent<RectTransform>();
        int loopCount = 0;
        int gridPositionIndex = 0;
        int offsetMultiplier = 0;
        bool foundPosition = false;
        while (!foundPosition)
        {
            int[] currentGridPositions = panelInGridPosition[loopCount];
            for (int i = 0; i < 9; i++)
            {
                if (currentGridPositions[i] == 0)
                {
                    gridPositionIndex = i;
                    offsetMultiplier = loopCount;
                    currentGridPositions[i] = 1;
                    panelInGridPosition[loopCount] = currentGridPositions;
                    foundPosition = true;
                    break;
                }
            }

            loopCount++;
            if (loopCount > panelInGridPosition.Count - 1)
            {
                panelInGridPosition.Add(new int[9]);
            }
        }

        Vector2 canvasDims = new Vector2(Mathf.Abs(eventDialogPanelRect.sizeDelta.x),
            Mathf.Abs(eventDialogPanelRect.sizeDelta.y));
        Vector2 panelPosition = gridLocations[gridPositionIndex];
        panelPosition.x *= canvasDims.x;
        panelPosition.y *= canvasDims.y;

        if (offsetMultiplier > 0)
        {
            panelPosition.x += 50f * offsetMultiplier;
            panelPosition.y -= 50f * offsetMultiplier;
        }

        eventDialogPanelRect.anchoredPosition = panelPosition;
        confPanelRect.anchoredPosition = panelPosition;
        eventDialogParent.GetComponent<EventDialogHandler>()
            .SetGridLayoutStartingPositionIndices(gridPositionIndex, offsetMultiplier);
    }

    private void MakeSpaceInGridAvailable(int gridPositionIndex, int gridLayerIndex)
    {
        int[] layerToChange = panelInGridPosition[gridLayerIndex];
        layerToChange[gridPositionIndex] = 0;
        panelInGridPosition[gridLayerIndex] = layerToChange;
    }
}