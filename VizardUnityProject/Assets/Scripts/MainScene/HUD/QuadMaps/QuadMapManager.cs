/*
// ISC License

// Copyright (c) 2025, Autonomous Vehicle Systems Lab, University of Colorado at Boulder

// Permission to use, copy, modify, and/or distribute this software for any
// purpose with or without fee is hereby granted, provided that the above
// copyright notice and this permission notice appear in all copies.

// THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
// WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
// MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
// ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
// WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
// ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
// OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.

// */
using System.Collections.Generic;
using UnityEngine;
using VizProtobufferMessage;
/// <summary>
/// Handles QuadMap messages in the current VizMessage
/// and orders creation or update of QuadMap objects
/// </summary>
public class QuadMapManager : MonoBehaviour
{
    private Dictionary<int, QuadMapMethods> allQuadMaps = new Dictionary<int, QuadMapMethods>();
    private int lastProcessedIndex=-1;
    private MessageLoggingPanelMethods msgPanel;

    void Start()
    {
        msgPanel = FindFirstObjectByType<MessageLoggingPanelMethods>();
        if (MessageList.FirstMessage.Settings!=null)
        {
            if (MessageList.FirstMessage.Settings.ShowQuadMapLabels == -1)
            {
                VizardGUISettings.ShowQuadMapLabels = false;
            }
            
        }
    }
    
    void FixedUpdate()
    {
        if (lastProcessedIndex != MessageList.CurrentIndex)
        {
                for (int desiredIndex = lastProcessedIndex + 1;
                     desiredIndex <= MessageList.CurrentIndex;
                     desiredIndex++)
                {
                    VizMessage messageToProcess = MessageList.GetMessageAtIndex(desiredIndex);
                    if (messageToProcess != null)
                    {
                        ProcessQuadMaps(messageToProcess, desiredIndex);
                    }
                    else
                    {
                        //If buffered playback then desiredIndex may no longer be available 
                        //in the current buffer, so 
                        desiredIndex = MessageList.FirstMessageIndexOfPlottedMessages; 
                    }
                }
        }

        lastProcessedIndex = MessageList.CurrentIndex;
    }

    private void ProcessQuadMaps(VizMessage msg, int msgIndex)
    {
        if (msg.QuadMaps.Count > 0)
        {
            foreach (VizMessage.Types.QuadMap qm in msg.QuadMaps)
            {
                if (allQuadMaps.ContainsKey(qm.ID))
                {
                    allQuadMaps[qm.ID].UpdateQuadMapSettings(qm, msgIndex);
                }
                else if (qm.Vertices.Count > 3)
                {
                    BuildQuadMap(qm, msgIndex);
                }
            }
        }
    }

    private void BuildQuadMap(VizMessage.Types.QuadMap newMap, int msgIndex)
    {
        GameObject quadMapObject = Instantiate(Resources.Load("Prefabs/QuadMap")as GameObject);
        QuadMapMethods quadMap = quadMapObject.GetComponent<QuadMapMethods>();
        bool buildSuccess = quadMap.InitializeQuadMap(newMap, msgIndex);
        if (buildSuccess){
            allQuadMaps[newMap.ID] = quadMap;
            msgPanel.CreateToggleForQuadMapSubMessage(newMap);
        }
        else
        {
            Destroy(quadMapObject);
        }
    }

}
