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
using VizProtobufferMessage;
/// <summary>
/// Process current VizMessage (and any skipped over messages) Locations messages
/// Add locations that have not been created yet and send updates on to
/// existing locations. 
/// </summary>
public class LocationManager : MonoBehaviour
{
    private int lastProcessedIndex = -1;
    private bool firstUpdate = true;
    private bool useFullLocations;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (MessageList.FirstMessage.Settings!=null)
        {
            VizardGUISettings.ShowStationCommunicationLines = MessageList.FirstMessage.Settings.ShowLocationCommLines != -1;
            VizardGUISettings.ShowStationCone = MessageList.FirstMessage.Settings.ShowLocationCones != -1;
            VizardGUISettings.UseSimpleMarkersForLocations = ((MessageList.FirstMessage.Locations.Count > 100)||(MessageList.FirstMessage.Settings.UseSimpleLocationMarkers==1))&&(MessageList.FirstMessage.Settings.UseSimpleLocationMarkers!=-1);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (firstUpdate)
        {
            useFullLocations = MessageList.FirstMessage.Locations.Count <= 100;
				    
            if (MessageList.FirstMessage.Settings != null)
            {
                if (MessageList.FirstMessage.Settings.UseSimpleLocationMarkers == 1)
                {
                    useFullLocations = false;
                }
                else if (MessageList.FirstMessage.Settings.UseSimpleLocationMarkers == -1)
                {
                    useFullLocations = true;
                }
            }

            firstUpdate = false;
        }else{
            if (lastProcessedIndex != MessageList.CurrentIndex)
            {

                for (int desiredIndex = lastProcessedIndex + 1;
                     desiredIndex <= MessageList.CurrentIndex;
                     desiredIndex++)
                {
                    VizMessage messageToProcess = MessageList.GetMessageAtIndex(desiredIndex);
                    if (messageToProcess != null)
                    {
                        ProcessLocations(messageToProcess, desiredIndex);
                    }
                    else
                    {
                        //If buffered playback then desiredIndex may no longer be available 
                        //in the current buffer, so 
                        desiredIndex = MessageList.FirstMessageIndexOfPlottedMessages;
                    }
                }
                lastProcessedIndex = MessageList.CurrentIndex;
            }
        }
    }

    private void ProcessLocations(VizMessage msg, int msgIndex)
    {
	    
        foreach (VizMessage.Types.Location station in msg.Locations)
        {
            if (CelestialBodyStateUtilities.LocationsDictionary.ContainsKey(station.StationName))
            {
                CelestialBodyStateUtilities.LocationsDictionary[station.StationName].GetComponent<DrawLocationMarker>().ProcessLocationMessage(station, msgIndex);
            }
            else
            {
                if (CheckLocationMessage(station))
                {
                    AddLocation(station, msgIndex, useFullLocations);
                }
            }
        }
    }

    private bool CheckLocationMessage(VizMessage.Types.Location newStation)
    {
        string stationName = newStation.StationName;
        if (stationName == "")
        {
            VizardGUISettings.UpdateErrorMessages($"Location does not include StationName: {newStation} ");
            return false;
        }

        string parentName = newStation.ParentBodyName;
        if (parentName == "")
        {
            VizardGUISettings.UpdateErrorMessages(
                $"Location does not include ParentBodyName: {newStation} ");
            return false;
        }

        if (newStation.RGPP.Count != 3)
        {
            VizardGUISettings.UpdateErrorMessages(
                $"Location does not include x, y, and z coordinates for the relative position.{newStation}");
            return false;
        }

        if (newStation.GHatP.Count >= 3)
        {
            Vector3 normal = new Vector3((float) newStation.GHatP[0], (float) newStation.GHatP[1], (float) newStation.GHatP[2]);
            if (normal == Vector3.zero)
            {
                VizardGUISettings.UpdateErrorMessages(
                    $"Location does not include a non-zero normal vector.{newStation}");
                return false;
            }
        }
        else
        {
            VizardGUISettings.UpdateErrorMessages($"Location does not include normal vector.{newStation}");
            return false;
        }

        float fov = (float) newStation.FieldOfView;

        if ((fov < 0.0001f) || (fov > 179.9999f))
        {
            VizardGUISettings.UpdateErrorMessages(
                $"Location Field Of View must be within 0.0001 to 179.9999 degrees. {newStation}");
        }
        return true;
    }
    
    public GameObject AddLocation(VizMessage.Types.Location newStation, int msgIndex, bool isFullLocation){
        GameObject newLocMarker = Instantiate (Resources.Load("Prefabs/LocationMarkerTemplate")as GameObject);
        DrawLocationMarker newLoc = newLocMarker.GetComponent<DrawLocationMarker>();
        bool buildSuccess = newLoc.InitializeLocationMarker(newStation, msgIndex, isFullLocation);
        if (buildSuccess)
        {
            CelestialBodyStateUtilities.LocationsDictionary.Add(newLocMarker.name, newLoc);
            return newLocMarker;
        }
        else
        {
            Destroy(newLocMarker);
            return null;
        }
    }
}