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
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VizProtobufferMessage;

/// <summary>
/// Updates the VizMessage Logging Panel to display the current message
/// </summary>
public class MessageLoggingPanelMethods : MonoBehaviour
{
    private bool panelIsOpen;

    [Header("Panel GUI Components")] public Transform toggleInventory;
    public RectTransform displayContent;
    public TextMeshProUGUI msgDisplay;
    public TMP_InputField currentIndexInput;
    public Button minusCurrentIndexButton;
    public Button plusCurrentIndexButton;

    private List<GameObject> contentButtons = new List<GameObject>();
    private InventoryPanelMethods invMethods;

    private Toggle showAllMsgToggle;
    private List<Toggle> allGroupToggles = new List<Toggle>();
    private bool toggleAllGroupToggles = true;
    private int messageGroupsToShowCount;

    private int celestialBodiesToShowCount;
    private int totalCelestialBodiesCount;
    private Dictionary<string, Toggle> celestialBodyToggles = new Dictionary<string, Toggle>();
    private Toggle allCelestialBodiesToggle;
    private bool toggleAllCelestialBodies = true;

    private Toggle timeStampToggle;
    private Toggle epochDateTimeToggle;
    private Toggle settingsToggle;
    private Toggle liveSettingsToggle;
    private Toggle eventDialogsToggle;


    private int spacecraftToShowCount;
    private int totalSpacecraftCount;
    private Dictionary<string, Toggle> spacecraftToggles = new Dictionary<string, Toggle>();
    private Toggle allSpacecraftToggle;
    private bool toggleAllSpacecraft = true;

    private int locationsToShowCount;
    private int totalLocationsCount;
    private Dictionary<string, Toggle> locationToggles = new Dictionary<string, Toggle>();
    private Toggle allLocationsToggle;
    private bool toggleAllLocations = true;

    private int camerasToShowCount;
    private int totalCamerasCount;
    private Dictionary<int, Toggle> cameraToggles = new Dictionary<int, Toggle>();
    private Toggle allCamerasToggle;
    private bool toggleAllCameras = true;

    private int quadMapsToShowCount;
    private int totalQuadMapsCount;
    private Dictionary<int, Toggle> quadMapToggles = new Dictionary<int, Toggle>();
    private Toggle allQuadMapsToggle;
    private bool toggleAllQuadMaps = true;
    private bool showAllQuadMapToggles = true;


    public float heightMultiplier = 10f;
    public float fontPixelWidth = 10f;
    private Vector2 displayWindowSize = new Vector2(305, 345);

    private int lastProcessedMessageIndex = -1;
    private bool toggleChange;
    private bool disableInputUpdate;


    // Start is called before the first frame update
    void Start()
    {
        invMethods = GetComponent<InventoryPanelMethods>();
        AddSubMessageSelectionTogglesToInventory();
        plusCurrentIndexButton.onClick.AddListener(RequestNextMessage);
        minusCurrentIndexButton.onClick.AddListener(RequestLastMessage);
        currentIndexInput.onSubmit.AddListener(RequestMessageAtIndex);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (panelIsOpen)
        {
            TranscribeCurrentMessage();

            if (EventSystem.current.currentSelectedGameObject != currentIndexInput.gameObject)
            {
                currentIndexInput.text = $"{MessageList.CurrentIndex + 1}";
            }
        }
    }

    public void TogglePanelOpen()
    {
        panelIsOpen = !panelIsOpen;
        transform.GetChild(0).gameObject.SetActive(panelIsOpen);
    }

    /// <summary>
    ///This method must be implemented for any subpanel component that needs to do something when the panel is resized
    /// Do not delete or make private.
    /// </summary>
    /// <param name="newDims">new panel extents</param>
    public void ApplyPanelResize(Vector2 newDims)
    {
        displayWindowSize = new Vector2(newDims.x - 195, newDims.y - 35);
    }

    private void TranscribeCurrentMessage()
    {
        if (toggleChange || (lastProcessedMessageIndex != MessageList.CurrentIndex))
        {
            VizMessage currentMessage = MessageList.CurrentMessage;
            string displayText = "";
            //displayText+=string.Format("EpochDateTime: \n\t{0}Y {1}M {2}D {3}h {4}m {5:0.00}s\n", epoch.Year, epoch.Month, epoch.Day, epoch.Hours, epoch.Minutes, epoch.Seconds);
            displayText += TranscribeTimeStampMessage(currentMessage);
            displayText += TranscribeEpochDateTimeMessage(currentMessage);
            displayText += TranscribeCelestialBodyMessages(currentMessage);
            displayText += TranscribeSpacecraftMessages(currentMessage);
            displayText += TranscribeCameraMessages(currentMessage);
            displayText += TranscribeLocationMessages(currentMessage);
            displayText += TranscribeSettingsMessages(currentMessage);
            displayText += TranscribeLiveSettingsMessages(currentMessage);
            displayText += TranscribeEventDialogMessages(currentMessage);
            displayText += TranscribeQuadMapMessages(currentMessage);

            float newHeight =
                Mathf.Max(displayText.Length * fontPixelWidth * heightMultiplier / displayWindowSize.x,
                    displayWindowSize.y);
            displayContent.sizeDelta = new Vector2(5, newHeight);
            msgDisplay.text = displayText;
            lastProcessedMessageIndex = MessageList.CurrentIndex;
            toggleChange = false;
        }
    }

    private void AddSubMessageSelectionTogglesToInventory()
    {
        VizMessage msg = MessageList.FirstMessage;

        showAllMsgToggle = (CreateToggleForSubMessage("Show All")).GetComponent<Toggle>();
        showAllMsgToggle.onValueChanged.AddListener(ToggleAllMessages);

        if (msg.CurrentTime != null)
        {
            timeStampToggle = (CreateToggleForSubMessage("Time Stamp")).GetComponent<Toggle>();
            allGroupToggles.Add(timeStampToggle);
            timeStampToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
        }

        if (msg.Epoch != null)
        {
            epochDateTimeToggle = (CreateToggleForSubMessage("EpochDateTime")).GetComponent<Toggle>();
            allGroupToggles.Add(epochDateTimeToggle);
            epochDateTimeToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
        }

        if (msg.CelestialBodies != null)
        {
            CreateTogglesForCelestialBodySubMessage(msg);
        }

        if (msg.Spacecraft != null)
        {
            CreateTogglesForSpacecraftSubMessage(msg);
        }

        if (msg.Cameras != null)
        {
            CreateTogglesForCameras(msg);
        }

        if (msg.Locations != null)
        {
            CreateTogglesForLocations(msg);
        }

        if (msg.Settings != null)
        {
            settingsToggle = (CreateToggleForSubMessage("Settings")).GetComponent<Toggle>();
            allGroupToggles.Add(settingsToggle);
            settingsToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
        }

        if (msg.LiveSettings != null)
        {
            liveSettingsToggle = (CreateToggleForSubMessage("LiveSettings")).GetComponent<Toggle>();
            allGroupToggles.Add(liveSettingsToggle);
            liveSettingsToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
        }

        //Have to add the following because they may not show up in first message, but could show up at any time
        eventDialogsToggle = (CreateToggleForSubMessage("EventDialogs")).GetComponent<Toggle>();
        allGroupToggles.Add(eventDialogsToggle);
        eventDialogsToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);

        messageGroupsToShowCount = allGroupToggles.Count;
    }

    private void ToggleUpdate(bool isOn)
    {
        toggleChange = true;
    }

    private Toggle CreateToggleForSubMessage(string labelText, int childDepth = 0)
    {
        GameObject newToggle = Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericSmallToggle") as GameObject);

        newToggle.name = labelText;
        newToggle.GetComponentInChildren<TextMeshProUGUI>().text = labelText;
        Toggle myToggle = newToggle.GetComponent<Toggle>();
        myToggle.isOn = true;
        myToggle.onValueChanged.AddListener(ToggleUpdate);

        AddToggleToInventory(newToggle, childDepth);

        return myToggle;
    }

    private Toggle CreateToggleWithButtonForSubMessage(string labelText, int childDepth = 0)
    {
        GameObject newToggle =
            Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericSmallToggleWithButton") as GameObject);

        newToggle.name = labelText;
        newToggle.GetComponentInChildren<TextMeshProUGUI>().text = labelText;
        Toggle myToggle = newToggle.GetComponent<Toggle>();
        myToggle.isOn = true;
        myToggle.onValueChanged.AddListener(ToggleUpdate);

        AddToggleToInventory(newToggle, childDepth);

        return myToggle;
    }

    private void AddToggleToInventory(GameObject toggle, int childDepth)
    {
        toggle.transform.SetParent(toggleInventory);
        toggle.GetComponent<RectTransform>().localScale = Vector3.one;
        toggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(3 + childDepth * 17, 0);

        if (toggleInventory.childCount > 13)
        {
            float width = toggleInventory.GetComponent<RectTransform>().rect.width;
            toggleInventory.GetComponent<RectTransform>().sizeDelta =
                new Vector2(width, 20 * toggleInventory.childCount + 5);
        }

        int positionY = -5;
        foreach (Transform child in toggleInventory.transform)
        {
            float childXPos = child.gameObject.GetComponent<RectTransform>().anchoredPosition.x;
            child.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(childXPos, positionY);
            positionY -= 20;
        }
    }

    private void RedoAllToggleLayout()
    {
        int activeChildCount = 0;
        int positionY = -5;
        foreach (Transform child in toggleInventory.transform)
        {
            if (child.gameObject.activeSelf)
            {
                activeChildCount += 1;
                float childXPos = child.gameObject.GetComponent<RectTransform>().anchoredPosition.x;
                child.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(childXPos, positionY);
                positionY -= 20;
            }
        }

        float width = toggleInventory.GetComponent<RectTransform>().rect.width;
        toggleInventory.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 20 * activeChildCount + 5);
    }

    private void ToggleAllMessages(bool allOn)
    {
        if (toggleAllGroupToggles)
        {
            foreach (Toggle gt in allGroupToggles)
            {
                gt.isOn = allOn;
            }

            messageGroupsToShowCount = allOn ? allGroupToggles.Count : 0;
        }
        else
        {
            toggleAllGroupToggles = true;
        }
    }

    private void UpdateGroupsToShowCount(bool groupOn)
    {
        if (groupOn)
        {
            messageGroupsToShowCount += 1;
            if (messageGroupsToShowCount == allGroupToggles.Count)
            {
                showAllMsgToggle.isOn = true;
            }
        }
        else
        {
            messageGroupsToShowCount -= 1;
            toggleAllGroupToggles = false;
            showAllMsgToggle.isOn = false;
            toggleAllGroupToggles = true;
        }
    }

    //**************************************CURRENT TIME********************************************
    private string TranscribeTimeStampMessage(VizMessage currentMessage)
    {
        if (timeStampToggle != null)
        {
            if (timeStampToggle.isOn)
            {
                if (currentMessage.CurrentTime != null)
                {
                    return $"Time Stamp:\n\t{currentMessage.CurrentTime}\n\n";
                }
            }
        }

        return "";
    }

    //**************************************EPOCH DATE TIME********************************************
    private string TranscribeEpochDateTimeMessage(VizMessage currentMessage)
    {
        if (epochDateTimeToggle != null)
        {
            if (epochDateTimeToggle.isOn)
            {
                if (currentMessage.Epoch != null)
                {
                    return $"Epoch:\n\t{currentMessage.Epoch}\n\n";
                }
            }
        }

        return "";
    }

    //**************************************CELESTIAL BODIES********************************************
    private void CreateTogglesForCelestialBodySubMessage(VizMessage msg)
    {
        Toggle masterToggle;
        totalCelestialBodiesCount = msg.CelestialBodies.Count;
        if (totalCelestialBodiesCount > 1)
        {
            if (totalCelestialBodiesCount <= 3)
            {
                masterToggle = CreateToggleForSubMessage("Show All Celestial Bodies");
            }
            else
            {
                masterToggle = CreateToggleWithButtonForSubMessage("Show All Celestial Bodies");
                masterToggle.GetComponentInChildren<Button>().onClick.AddListener(ShowHideAllCelestialBodyToggles);
                masterToggle.GetComponent<RectTransform>().sizeDelta = new Vector2(166, 20);
            }

            allGroupToggles.Add(masterToggle);
            masterToggle.onValueChanged.AddListener(ToggleAllCelestialBodies);
            masterToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
            allCelestialBodiesToggle = masterToggle;

            foreach (VizMessage.Types.CelestialBody cb in msg.CelestialBodies)
            {
                Toggle cbToggle = CreateToggleForSubMessage(cb.BodyName, 1);
                cbToggle.GetComponent<Toggle>().onValueChanged.AddListener(UpdateCelestialBodyCount);
                celestialBodyToggles.Add(cb.BodyName, cbToggle);
                celestialBodiesToShowCount += 1;
            }
        }
        else
        {
            if (totalCelestialBodiesCount == 1)
            {
                masterToggle = CreateToggleForSubMessage("Show Celestial Body");
                allGroupToggles.Add(masterToggle);
                masterToggle.GetComponent<Toggle>().onValueChanged.AddListener(UpdateCelestialBodyCount);
                masterToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
                celestialBodiesToShowCount = 1;
            }
            else
            {
                celestialBodiesToShowCount = 0;
            }
        }
    }

    private void ShowHideAllCelestialBodyToggles()
    {
        foreach (KeyValuePair<string, Toggle> kvp in celestialBodyToggles)
        {
            kvp.Value.transform.gameObject.SetActive(!kvp.Value.transform.gameObject.activeSelf);
        }

        RedoAllToggleLayout();
    }

    private void ToggleAllCelestialBodies(bool allOn)
    {
        if (toggleAllCelestialBodies)
        {
            foreach (KeyValuePair<string, Toggle> kvp in celestialBodyToggles)
            {
                kvp.Value.isOn = allOn;
            }

            celestialBodiesToShowCount = allOn ? totalCelestialBodiesCount : 0;
        }
        else
        {
            toggleAllCelestialBodies = true;
        }
    }

    private void UpdateCelestialBodyCount(bool bodyOn)
    {
        if (bodyOn)
        {
            celestialBodiesToShowCount += 1;
            if (celestialBodiesToShowCount == totalCelestialBodiesCount)
            {
                if (allCelestialBodiesToggle != null)
                {
                    allCelestialBodiesToggle.isOn = true;
                }
            }
        }
        else
        {
            celestialBodiesToShowCount -= 1;
            toggleAllCelestialBodies = false;
            if (allCelestialBodiesToggle != null)
            {
                allCelestialBodiesToggle.isOn = false;
            }

            toggleAllCelestialBodies = true;
        }
    }

    private string TranscribeCelestialBodyMessages(VizMessage currentMessage)
    {
        string bodyText = "";
        if ((celestialBodiesToShowCount == 0))
        {
            return bodyText;
        }

        if (currentMessage.CelestialBodies.Count == 1)
        {
            if (celestialBodiesToShowCount == 1)
            {
                bodyText += $"Celestial Bodies:\n\t{currentMessage.CelestialBodies[0]}\n";
            }

            bodyText += "\n";
            return bodyText;
        }

        bodyText += "Celestial Bodies:\n";

        if (celestialBodiesToShowCount == currentMessage.CelestialBodies.Count + 1)
        {
            foreach (VizMessage.Types.CelestialBody cb in currentMessage.CelestialBodies)
            {
                bodyText += $"\t{cb}\n";
            }

            bodyText += "\n";
            return bodyText;
        }

        foreach (VizMessage.Types.CelestialBody cb in currentMessage.CelestialBodies)
        {
            Toggle cbToggle = celestialBodyToggles[cb.BodyName];
            if (cbToggle.isOn)
            {
                bodyText += $"\t{cb}\n";
            }
        }

        bodyText += "\n";
        return bodyText;
    }

    //**************************************SPACECRAFT********************************************
    private void CreateTogglesForSpacecraftSubMessage(VizMessage msg)
    {
        Toggle masterToggle;
        totalSpacecraftCount = msg.Spacecraft.Count;
        if (totalSpacecraftCount > 1)
        {
            if (totalSpacecraftCount <= 3)
            {
                masterToggle = CreateToggleForSubMessage("Show All Spacecraft");
            }
            else
            {
                masterToggle = CreateToggleWithButtonForSubMessage("Show All Spacecraft");
                masterToggle.GetComponentInChildren<Button>().onClick.AddListener(ShowHideAllSpacecraftToggles);
                masterToggle.GetComponent<RectTransform>().sizeDelta = new Vector2(166, 20);
            }

            allGroupToggles.Add(masterToggle);
            masterToggle.onValueChanged.AddListener(ToggleAllSpacecraft);
            masterToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
            allSpacecraftToggle = masterToggle.GetComponent<Toggle>();

            foreach (VizMessage.Types.Spacecraft sc in msg.Spacecraft)
            {
                if (spacecraftToggles.ContainsKey(sc.SpacecraftName))
                {
                    VizardGUISettings.UpdateErrorMessages(
                        $"Multiple spacecraft messages for {sc.SpacecraftName} were passed in the first message.");
                }
                else
                {
                    Toggle scToggle = CreateToggleForSubMessage(sc.SpacecraftName, 1);
                    scToggle.onValueChanged.AddListener(UpdateSpacecraftCount);
                    spacecraftToggles.Add(sc.SpacecraftName, scToggle);
                    spacecraftToShowCount += 1;
                }
            }
        }
        else
        {
            if (totalSpacecraftCount == 1)
            {
                masterToggle = CreateToggleForSubMessage("Show Spacecraft");
                allGroupToggles.Add(masterToggle);
                masterToggle.GetComponent<Toggle>().onValueChanged.AddListener(UpdateSpacecraftCount);
                masterToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
                spacecraftToShowCount = 1;
            }
            else
            {
                spacecraftToShowCount = 0;
            }
        }
    }

    private void ShowHideAllSpacecraftToggles()
    {
        foreach (KeyValuePair<string, Toggle> kvp in spacecraftToggles)
        {
            kvp.Value.transform.gameObject.SetActive(!kvp.Value.transform.gameObject.activeSelf);
        }

        RedoAllToggleLayout();
    }

    private void ToggleAllSpacecraft(bool allOn)
    {
        if (toggleAllSpacecraft)
        {
            foreach (KeyValuePair<string, Toggle> kvp in spacecraftToggles)
            {
                kvp.Value.isOn = allOn;
            }

            spacecraftToShowCount = allOn ? totalSpacecraftCount : 0;
        }
        else
        {
            toggleAllSpacecraft = true;
        }
    }

    private void UpdateSpacecraftCount(bool bodyOn)
    {
        if (bodyOn)
        {
            spacecraftToShowCount += 1;
            if (spacecraftToShowCount == totalSpacecraftCount)
            {
                if (allSpacecraftToggle != null)
                {
                    allSpacecraftToggle.isOn = true;
                }
            }
        }
        else
        {
            spacecraftToShowCount -= 1;
            toggleAllSpacecraft = false;
            if (allSpacecraftToggle != null)
            {
                allSpacecraftToggle.isOn = false;
            }

            toggleAllSpacecraft = true;
        }
    }

    private string TranscribeSpacecraftMessages(VizMessage currentMessage)
    {
        string bodyText = "";
        if ((spacecraftToShowCount == 0))
        {
            return bodyText;
        }

        if (currentMessage.Spacecraft.Count == 1)
        {
            if (spacecraftToShowCount == 1)
            {
                bodyText += $"Spacecraft:\n\t{currentMessage.Spacecraft[0]}\n";
            }

            bodyText += "\n";
            return bodyText;
        }

        bodyText += "Spacecraft:\n";
        if (spacecraftToShowCount == currentMessage.Spacecraft.Count + 1)
        {
            foreach (VizMessage.Types.Spacecraft sc in currentMessage.Spacecraft)
            {
                bodyText += $"\t{sc}\n";
            }

            bodyText += "\n";
            return bodyText;
        }

        foreach (VizMessage.Types.Spacecraft sc in currentMessage.Spacecraft)
        {
            Toggle scToggle = spacecraftToggles[sc.SpacecraftName];
            if (scToggle.isOn)
            {
                bodyText += $"\t{sc}\n";
            }
        }

        bodyText += "\n";
        return bodyText;
    }

    //**************************************LOCATIONS********************************************
    private void CreateTogglesForLocations(VizMessage msg)
    {
        Toggle masterToggle;
        totalLocationsCount = msg.Locations.Count;
        if (totalLocationsCount > 1)
        {
            if (totalLocationsCount <= 3)
            {
                masterToggle = CreateToggleForSubMessage("Show All Locations");
            }
            else
            {
                masterToggle = CreateToggleWithButtonForSubMessage("Show All Locations");
                masterToggle.GetComponentInChildren<Button>().onClick.AddListener(ShowHideAllLocationToggles);
                masterToggle.GetComponent<RectTransform>().sizeDelta = new Vector2(166, 20);
            }

            allGroupToggles.Add(masterToggle);
            masterToggle.onValueChanged.AddListener(ToggleAllLocations);
            masterToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
            allLocationsToggle = masterToggle;

            foreach (VizMessage.Types.Location lc in msg.Locations)
            {
                Toggle lcToggle = CreateToggleForSubMessage(lc.StationName, 1);
                lcToggle.onValueChanged.AddListener(UpdateLocationCount);
                locationToggles.Add(lc.StationName, lcToggle);
                locationsToShowCount += 1;
            }
        }
        else
        {
            if (totalLocationsCount == 1)
            {
                masterToggle = CreateToggleForSubMessage("Show Location");
                allGroupToggles.Add(masterToggle);
                masterToggle.onValueChanged.AddListener(UpdateLocationCount);
                masterToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
                locationsToShowCount = 1;
            }
            else
            {
                locationsToShowCount = 0;
            }
        }
    }

    private void ShowHideAllLocationToggles()
    {
        foreach (KeyValuePair<string, Toggle> kvp in locationToggles)
        {
            kvp.Value.transform.gameObject.SetActive(!kvp.Value.transform.gameObject.activeSelf);
        }

        RedoAllToggleLayout();
    }

    private void ToggleAllLocations(bool allOn)
    {
        if (toggleAllLocations)
        {
            foreach (KeyValuePair<string, Toggle> kvp in locationToggles)
            {
                kvp.Value.isOn = allOn;
            }

            locationsToShowCount = allOn ? totalLocationsCount : 0;
        }
        else
        {
            toggleAllLocations = true;
        }
    }

    private void UpdateLocationCount(bool bodyOn)
    {
        if (bodyOn)
        {
            locationsToShowCount += 1;
            if (locationsToShowCount == totalLocationsCount)
            {
                if (allLocationsToggle != null)
                {
                    allLocationsToggle.isOn = true;
                }
            }
        }
        else
        {
            locationsToShowCount -= 1;
            toggleAllLocations = false;
            if (allLocationsToggle != null)
            {
                allLocationsToggle.isOn = false;
            }

            toggleAllLocations = true;
        }
    }

    private string TranscribeLocationMessages(VizMessage currentMessage)
    {
        string bodyText = "";
        if ((locationsToShowCount == 0))
        {
            return bodyText;
        }

        if (currentMessage.Locations.Count == 1)
        {
            if (locationsToShowCount == 1)
            {
                bodyText += $"Locations:\n\t{currentMessage.Locations[0]}\n";
            }

            bodyText += "\n";
            return bodyText;
        }

        bodyText += "Locations:\n";
        if (locationsToShowCount == currentMessage.Locations.Count + 1)
        {
            foreach (VizMessage.Types.Location lc in currentMessage.Locations)
            {
                bodyText += $"\t{lc}\n";
            }

            bodyText += "\n";
            return bodyText;
        }

        foreach (VizMessage.Types.Location lc in currentMessage.Locations)
        {
            Toggle lcToggle = locationToggles[lc.StationName];
            if (lcToggle.isOn)
            {
                bodyText += $"\t{lc}\n";
            }
        }

        bodyText += "\n";
        return bodyText;
    }

    //**************************************CAMERAS********************************************
    private void CreateTogglesForCameras(VizMessage msg)
    {
        Toggle masterToggle;
        totalCamerasCount = msg.Cameras.Count;
        if (totalCamerasCount > 1)
        {
            if (totalCamerasCount <= 3)
            {
                masterToggle = CreateToggleForSubMessage("Show All Cameras");
            }
            else
            {
                masterToggle = CreateToggleWithButtonForSubMessage("Show All Cameras");
                masterToggle.GetComponentInChildren<Button>().onClick.AddListener(ShowHideAllCameraToggles);
                masterToggle.GetComponent<RectTransform>().sizeDelta = new Vector2(166, 20);
            }

            allGroupToggles.Add(masterToggle);
            masterToggle.onValueChanged.AddListener(ToggleAllCameras);
            masterToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
            allCamerasToggle = masterToggle;

            int i = 0;
            foreach (VizMessage.Types.CameraConfig cam in msg.Cameras)
            {
                Toggle camToggle = CreateToggleForSubMessage(cam.ParentName + " Camera " + cam.CameraID, 1);
                camToggle.onValueChanged.AddListener(UpdateCameraCount);
                cameraToggles.Add(i, camToggle);
                i++;
                camerasToShowCount += 1;
            }
        }
        else
        {
            if (totalCamerasCount == 1)
            {
                masterToggle = CreateToggleForSubMessage("Show Camera");
                allGroupToggles.Add(masterToggle);
                masterToggle.onValueChanged.AddListener(UpdateCameraCount);
                masterToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
                camerasToShowCount = 1;
            }
            else
            {
                camerasToShowCount = 0;
            }
        }
    }

    private void ShowHideAllCameraToggles()
    {
        foreach (KeyValuePair<int, Toggle> kvp in cameraToggles)
        {
            kvp.Value.transform.gameObject.SetActive(!kvp.Value.transform.gameObject.activeSelf);
        }

        RedoAllToggleLayout();
    }

    private void ToggleAllCameras(bool allOn)
    {
        if (toggleAllCameras)
        {
            foreach (KeyValuePair<int, Toggle> kvp in cameraToggles)
            {
                kvp.Value.isOn = allOn;
            }

            camerasToShowCount = allOn ? totalCamerasCount : 0;
        }
        else
        {
            toggleAllCameras = true;
        }
    }

    private void UpdateCameraCount(bool bodyOn)
    {
        if (bodyOn)
        {
            camerasToShowCount += 1;
            if (camerasToShowCount == totalCamerasCount)
            {
                if (allCamerasToggle != null)
                {
                    allCamerasToggle.isOn = true;
                }
            }
        }
        else
        {
            camerasToShowCount -= 1;
            toggleAllCameras = false;
            if (allCamerasToggle != null)
            {
                allCamerasToggle.isOn = false;
            }

            toggleAllCameras = true;
        }
    }

    private string TranscribeCameraMessages(VizMessage currentMessage)
    {
        string bodyText = "";
        if ((camerasToShowCount == 0))
        {
            return bodyText;
        }

        if (currentMessage.Cameras.Count == 1)
        {
            if (camerasToShowCount == 1)
            {
                bodyText += $"Cameras:\n\t{currentMessage.Cameras[0]}\n";
            }

            bodyText += "\n";
            return bodyText;
        }

        bodyText += "Cameras:\n";
        if (camerasToShowCount == currentMessage.Cameras.Count + 1)
        {
            foreach (VizMessage.Types.CameraConfig cam in currentMessage.Cameras)
            {
                bodyText += $"\t{cam}\n";
            }

            bodyText += "\n";
            return bodyText;
        }

        int i = 0;
        foreach (VizMessage.Types.CameraConfig cam in currentMessage.Cameras)
        {
            Toggle camToggle = cameraToggles[i];
            if (camToggle.isOn)
            {
                bodyText += $"\t{cam}\n";
            }

            i++;
        }

        bodyText += "\n";
        return bodyText;
    }

    //**************************************Settings********************************************
    private string TranscribeSettingsMessages(VizMessage currentMessage)
    {
        if (settingsToggle != null)
        {
            if (settingsToggle.isOn)
            {
                if (currentMessage.Settings != null)
                {
                    return $"Settings:\n\t{currentMessage.Settings}\n\n";
                }
            }
        }

        return "";
    }

    //**************************************LiveSettings********************************************
    private string TranscribeLiveSettingsMessages(VizMessage currentMessage)
    {
        if (liveSettingsToggle != null)
        {
            if (liveSettingsToggle.isOn)
            {
                if (currentMessage.LiveSettings != null)
                {
                    return $"Live Settings:\n\t{currentMessage.LiveSettings}\n\n";
                }
            }
        }

        return "";
    }

    //**************************************EventDialogs********************************************
    private string TranscribeEventDialogMessages(VizMessage currentMessage)
    {
        if (eventDialogsToggle != null)
        {
            if (eventDialogsToggle.isOn)
            {
                if (currentMessage.VizEventDialogs != null)
                {
                    string returnString = "Event Dialogs:\n";
                    foreach (VizEventDialog dialog in currentMessage.VizEventDialogs)
                    {
                        returnString += $"{dialog}\n\n";
                    }

                    returnString += "\n";
                    return returnString;
                }
            }
        }

        return "";
    }

    //**************************************QuadMaps********************************************
    public void CreateToggleForQuadMapSubMessage(VizMessage.Types.QuadMap qm)
    {
        if (quadMapToggles.Keys.Count == 0) //Build the basic toggle
        {
            allQuadMapsToggle = CreateToggleForSubMessage("Show All QuadMaps");
            allGroupToggles.Add(allQuadMapsToggle);
            allQuadMapsToggle.onValueChanged.AddListener(ToggleAllQuadMaps);
            allQuadMapsToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
        }
        else if
            (quadMapToggles.Keys.Count ==
             3) // Build the fancy toggle button that will expand all the quad map toggles, delete the basic toggle
        {
            GameObject oldAllQuadMapsToggle = allQuadMapsToggle.gameObject;
            int siblingIndex = oldAllQuadMapsToggle.transform.GetSiblingIndex();
            allGroupToggles.Remove(allQuadMapsToggle);

            allQuadMapsToggle = CreateToggleWithButtonForSubMessage("Show All QuadMaps").GetComponent<Toggle>();
            allQuadMapsToggle.GetComponentInChildren<Button>().onClick.AddListener(ShowHideAllQuadMapToggles);
            allQuadMapsToggle.transform.SetSiblingIndex(siblingIndex);
            allQuadMapsToggle.GetComponent<RectTransform>().sizeDelta = new Vector2(166, 20);
            allGroupToggles.Add(allQuadMapsToggle);
            Destroy(oldAllQuadMapsToggle);

            allQuadMapsToggle.onValueChanged.AddListener(ToggleAllQuadMaps);
            allQuadMapsToggle.onValueChanged.AddListener(UpdateGroupsToShowCount);
        }

        Toggle qmToggle = CreateToggleForSubMessage("ID: " + qm.ID.ToString(), 1);
        qmToggle.GetComponent<Toggle>().onValueChanged.AddListener(UpdateQuadMapCount);
        quadMapToggles.Add(qm.ID, qmToggle);
        quadMapsToShowCount += 1;
        totalQuadMapsCount += 1;

        messageGroupsToShowCount = allGroupToggles.Count;
    }

    private void ShowHideAllQuadMapToggles()
    {
        showAllQuadMapToggles = !showAllQuadMapToggles;
        foreach (KeyValuePair<int, Toggle> kvp in quadMapToggles)
        {
            kvp.Value.transform.gameObject.SetActive(showAllQuadMapToggles);
        }

        RedoAllToggleLayout();
    }

    private void ToggleAllQuadMaps(bool allOn)
    {
        if (toggleAllQuadMaps)
        {
            foreach (KeyValuePair<int, Toggle> kvp in quadMapToggles)
            {
                kvp.Value.isOn = allOn;
            }

            quadMapsToShowCount = allOn ? totalQuadMapsCount : 0;
        }
        else
        {
            toggleAllQuadMaps = true;
        }
    }

    private void UpdateQuadMapCount(bool bodyOn)
    {
        if (bodyOn)
        {
            quadMapsToShowCount += 1;
            if (quadMapsToShowCount == totalQuadMapsCount)
            {
                if (allQuadMapsToggle != null)
                {
                    allQuadMapsToggle.isOn = true;
                }
            }
        }
        else
        {
            quadMapsToShowCount -= 1;
            toggleAllQuadMaps = false;
            if (allQuadMapsToggle != null)
            {
                allQuadMapsToggle.isOn = false;
            }

            toggleAllQuadMaps = true;
        }
    }

    private string TranscribeQuadMapMessages(VizMessage currentMessage)
    {
        string returnString = "";
        if (allQuadMapsToggle != null)
        {
            if (allQuadMapsToggle.isOn)
            {
                if (currentMessage.QuadMaps.Count > 0)
                {
                    returnString = "Quad Maps:\n";
                    foreach (VizMessage.Types.QuadMap map in currentMessage.QuadMaps)
                    {
                        returnString += $"{map}\n\n";
                    }
                }
            }
            else
            {
                returnString = "Quad Maps: \n";
                foreach (VizMessage.Types.QuadMap qm in currentMessage.QuadMaps)
                {
                    Toggle qmToggle = quadMapToggles[qm.ID];
                    if (qmToggle.isOn)
                    {
                        returnString += $"\t{qm}\n\n";
                    }
                }
            }

            returnString += "\n";
        }

        return returnString;
    }

    private void RequestNextMessage()
    {
        VizardGUISettings.PlaybackManager.GoToMessage(
            $"{MessageList.CurrentIndex + 2}"); //Because time steps are indexed at 1, but messages at 0
    }

    private void RequestLastMessage()
    {
        VizardGUISettings.PlaybackManager.GoToMessage(
            $"{MessageList.CurrentIndex}"); //Because time steps are indexed at 1, but messages at 0
    }

    private void RequestMessageAtIndex(string goToID)
    {
        VizardGUISettings.PlaybackManager.GoToMessage(goToID);
    }
}