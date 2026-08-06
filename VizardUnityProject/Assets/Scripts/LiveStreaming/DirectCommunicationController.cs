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

using System;
using System.Collections.Generic;
using NetMQ;
using UnityEngine;
using Google.Protobuf;
using VizProtobufferMessage;

/// <summary>
/// Handles communication with live Basilisk simulation
/// </summary>
public class DirectCommunicationController : MonoBehaviour
{
    private ResSocket resSocket; //Response socket, used for two-way communication
    private SubSocket subSocket; //Subscribe socket, used for receive only communication
    private VizInputAccumulator vizInputs; //Vizard user inputs to live Basilisk sim to be communicated in next message

    //Timing data for measuring livestreaming metrics
    private DateTime imageRequestStartTime; //System time image request received from Basilisk sim

    private DateTime
        imageTransmitStartTime; //System time Vizard began to transmit the requested image back to Basilisk sim

    private DateTime imageTransmitEndTime; //System time Vizard finished transmitting the requested image

    public List<string> messageSubscriptions = new List<string>(); //Message types that socket is subscribed to

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject); //Keep this instance alive from StartupScene to be used in Main Scene
        vizInputs = this.GetComponent<VizInputAccumulator>(); //Track Vizard user inputs to be communicated to Basilisk
    }

    /// <summary>
    /// Connect the Event Dialog Manager to the DirectComm controller
    /// to allow user choices on Event Dialogs to be communicated to Basilisk
    /// </summary>
    /// <param name="eventDialogMgr">Vizard Main Scene instance of EventDialogManager</param>
    public void ConnectEventDialogManager(EventDialogManager eventDialogMgr)
    {
        vizInputs.eventDialogManager = eventDialogMgr;

        //If the socket is for two-way communication, set any Basilisk specified hot keys 
        //that should be listened for and reported back to Basilisk
        if (!DataManager.SocketIsReceiveOnly)
        {
            VizProtobufferMessage.VizMessage.Types.VizSettingsPb settings = MessageList.FirstMessage.Settings;
            if (settings != null)
            {
                vizInputs.SetListenerStringForKeyboard(settings.KeyboardLiveInput);
            }
        }
    }

    /// <summary>
    /// Start the correct socket for the type of streaming communication chosen by the user
    /// </summary>
    /// <param name="address">Socket address to connect Vizard</param>
    /// <returns></returns>
    public bool StartCommunication(string address)
    {
        if (DataManager.SocketIsReceiveOnly) //Set up receive only socket
        {
            //Add only the NetMQ message types that can be handled in receive only
            messageSubscriptions.Add("SIM_UPDATE");
            messageSubscriptions.Add("SYNC_SETTINGS");
            //Create the subscription socket
            subSocket = new SubSocket(ReceiveVizMessageSubSocket, address, messageSubscriptions);
            //Start the socket
            return subSocket.Start();
        }

        //Create the response socket for two-way communication
        resSocket = new ResSocket(address, RequestCallback);
        //Start the socket
        return resSocket.Start();
    }

    /// <summary>
    /// Stop communication with Basilisk by stopping the socket in use
    /// </summary>
    public void StopSocket()
    {
        if (DataManager.SocketIsReceiveOnly)
        {
            subSocket.Stop();
        }
        else
        {
            resSocket.Stop();
        }

        NetMQConfig.Cleanup();
    }

    private readonly MessageLogger _logger = new MessageLogger();

    /// <summary>
    /// Returns the correct response for a Basilisk request message
    /// </summary>
    /// <param name="request">Current Basilisk request message</param>
    /// <returns></returns>
    private NetMQMessage RequestCallback(NetMQMessage request)
    {
        NetMQMessage response = new NetMQMessage();
        //Parse the request to pull out the substring of the 
        //request type
        string requestString = "PING";
        if (request.FrameCount > 0)
        {
            requestString = request[0].ConvertToString();
        }

        if (requestString.Contains("REQUEST_IMAGE"))
        {
            requestString = "REQUEST_IMAGE";
        }
        else if (requestString.Contains("REQUEST_INPUT"))
        {
            requestString = "REQUEST_INPUT";
        }
        else if (requestString.Contains("PING"))
        {
            requestString = "PING";
        }
        else if (requestString.Contains("SIM_UPDATE"))
        {
            requestString = "SIM_UPDATE";
        }

        _logger.LogReceived(requestString, response);

        //Take the correct action for the Basilisk request
        switch (requestString)
        {
            case "PING":
                //Keep the socket alive
                response.Append("PONG");
                break;
            case "SIM_UPDATE":
                //Receive a VizMessage containing an update on all the scenario objects in the scene
                ReceiveVizMessageResSocket(request);
                response.Append("OK");
                break;
            case "REQUEST_INPUT":
                //Send all listened for user input that occurred since last request
                VizInput inputResponse = vizInputs.GetInputResponseMessage();
                response.Append("VIZARD_INPUT");
                response.Append(inputResponse.ToByteArray());
                break;
            case "REQUEST_IMAGE":
                //Take an image with the requested camera and 
                //stream the image back to Basilisk
                imageRequestStartTime = DateTime.Now;
                RequestImage(request);
                imageTransmitStartTime = DateTime.Now;

                AtomicImageBuffer.LockBuffer();
                response.Append(AtomicImageBuffer.ImageBuffer.Length);
                response.Append(AtomicImageBuffer.ImageBuffer);
                AtomicImageBuffer.ReleaseBuffer();
                imageTransmitEndTime = DateTime.Now;
                if (DataManager.SaveFPSMetricsToFile)
                {
                    TimeSpan renderInterval = imageTransmitStartTime - imageRequestStartTime;
                    double renderSeconds = renderInterval.TotalSeconds;
                    TimeSpan transmitInterval = imageTransmitEndTime - imageTransmitStartTime;
                    double timeToTransmit = transmitInterval.TotalSeconds;
                    DataManager.SaveMetrics($"{renderSeconds}, {timeToTransmit}");
                }

                break;
            default:
                response.Append("ERROR");
                break;
        }

        return response;
    }

    /// <summary>
    /// Basilisk sim has requested an image from an instrument camera (cameraID provided)
    /// for the most recent VizMessage received
    /// </summary>
    /// <param name="message">NetMQ image request message</param>
    private void RequestImage(NetMQMessage message)
    {
        //Set cameraID to message once that information is being sent in the message
        string requestString = message[0].ConvertToString();
        int cameraID = -1;
        if (requestString.Length > 13)
        {
            string cameraString = requestString.Substring(14);
            cameraID = Int32.Parse(cameraString);
        }

        //Do not advance playback of vizMessages until image has been rendered and transmitted
        MessageList.PlaybackPaused = true;
        //Set the state of the scenario objects to the most recent VizMessage
        MessageList.CurrentIndex = MessageList.TimestepsTotal - 1;
        //Set flag to request camera image for instrument camera matching cameraID
        AtomicImageBuffer.RequestScreenshot(cameraID);
    }

    /// <summary>
    /// Shut down communication and save accumulated VizMessages to file
    /// </summary>
    private void OnApplicationQuit()
    {
        if (DataManager.IsLiveSim)
        {
            MessageList.SaveMessages("last_run.bin");

            if (!DataManager.SocketIsReceiveOnly)
            {
                resSocket.Stop();
            }
            else
            {
                if (subSocket != null)
                {
                    subSocket.Stop();
                }
            }

            NetMQConfig.Cleanup();
        }
    }

    /// <summary>
    /// Handles receiving a VizMessage in two-way communication
    /// </summary>
    /// <param name="message">NetMQ SIM_UPDATE message</param>
    private void ReceiveVizMessageResSocket(NetMQMessage message)
    {
        //For backward compatibility, vizInterface is sending two empty frames between the header and the protobuffer message
        byte[] data = message[3].ToByteArray();
        //Parse the vizMessage from the third frame
        VizMessage vizMessage = VizMessage.Parser.ParseFrom(data);
        _logger.Log(vizMessage.CurrentTime.FrameNumber.ToString());
        //Add it to the message dictionary in MessageList
        MessageList.AddLiveMessage(vizMessage);
    }

    /// <summary>
    /// Handles receiving subscribed to messages from Basilisk in receive-only communication
    /// </summary>
    /// <param name="message">NetMQ SIM_UPDATE message</param>
    private void ReceiveVizMessageSubSocket(NetMQMessage message)
    {
        string messageTopicReceived = message[0].ConvertToString();
        //If receiving a VizMessage protobuffer message:
        if (messageTopicReceived == "SIM_UPDATE")
        {
            //Basilisk broadcast socket doesn't bother with empty frames between header and VizMessage
            byte[] data = message[1].ToByteArray();
            //Parse the vizMessage from the frame
            VizMessage vizMessage = VizMessage.Parser.ParseFrom(data);
            //Add it to the message dictionary in MessageList
            MessageList.AddLiveMessage(vizMessage);
            //If the settings message has not been received yet and this message includes VizMessage.Settings
            if ((!MessageList.SettingsMessageReceived) && (vizMessage.Settings != null))
            {
                //Set the included Settings to be part of the first message in the dictionary
                MessageList.AddSettingsMessageToFirstMessage(vizMessage);
            }
        }
        //If receiving settings that should be applied to keep broadcast viewers in-sync with the
        //current Vizard view settings of the instructor
        else if (messageTopicReceived == "SYNC_SETTINGS")
        {
            byte[] data = message[1].ToByteArray();
            //Parse the sync settings from the frame
            VizBroadcastSyncSettings syncSettings = VizBroadcastSyncSettings.Parser.ParseFrom(data);
            //Apply the latest sync settings to the broadcast viewer's Vizard instance
            MessageList.LatestBroadcastSyncSettings = syncSettings;
        }
    }
}