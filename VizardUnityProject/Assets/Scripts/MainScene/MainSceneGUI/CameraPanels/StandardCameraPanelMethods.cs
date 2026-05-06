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
using UnityEngine.UI;
using VizProtobufferMessage;
/// <summary>
/// Handles user input to a standard camera panel and
/// displays the current view from its standard camera
/// </summary>
public class StandardCameraPanelMethods : MonoBehaviour
{
    [Header("Panel GUI Components")]
    public GameObject myCameraImage;
    public GameObject myCamera;
    public Toggle panelToggle;
    public Button closePanelButton;
    public Button verboseButton;
    public GameObject verbosePanel;
    public GameObject VRVerbosePanel;
    public TMP_InputField panelName;
    public TMP_Dropdown attachedBodyDropdown;
    public Button takeScreenshotButton;
    public GameObject fieldOfViewInputField;

    public Toggle pointCameraAtBodyToggle;
    public Toggle pointCameraAlongVectorToggle;

    //At body panel
    public GameObject atBodyPanel;
    public TMP_Dropdown targetDropdown; //Holds the target list options (planet cam only)
    public TMP_Dropdown viewDropdown; //Holds the camera view options

    //Along Vector panel
    public GameObject alongVectorPanel;
    public TMP_InputField xPt;
    public TMP_InputField yPt;
    public TMP_InputField zPt;

    //Show HUD Elements
    public Toggle showHUDElementsToggle;

    //Custom Camera Position panel
    public Toggle useCustomCamPositionToggle;
    public GameObject camPositionPanel;
    public TMP_InputField xPos;
    public TMP_InputField yPos;
    public TMP_InputField zPos;

    private StandardCameraController cameraController;

    private bool inBodyMode = true;
    private bool firstActivation = true;

    public List<string> presetVectorsToPlanet = new List<string>() {"Set View", "Nadir", "Orbit Normal", "Along Track"};

    private string attachedBodyEffectorParent = "";
    private string targetBodyEffectorParent = "";
    private GameObject openSubMenu;


    void Awake()
    {
        //Add the listener methods for the GUI objects
        takeScreenshotButton.onClick.AddListener(CaptureCameraImage);

        pointCameraAtBodyToggle.onValueChanged.AddListener(Toggle_BodyTargetSubpanel);
        panelName.onEndEdit.AddListener(UpdateLabels);
        closePanelButton.onClick.AddListener(ClosePanel);
        verboseButton.onClick.AddListener(ToggleVerbosePanel);

        useCustomCamPositionToggle.onValueChanged.AddListener(Toggle_UserSetCameraPosition);
        xPos.onEndEdit.AddListener(TMP_InputField_ChangeCameraOrigin);
        yPos.onEndEdit.AddListener(TMP_InputField_ChangeCameraOrigin);
        zPos.onEndEdit.AddListener(TMP_InputField_ChangeCameraOrigin);

        showHUDElementsToggle.onValueChanged.AddListener(ToggleHUDElements);

        fieldOfViewInputField.GetComponent<TMP_InputField>().onEndEdit
            .AddListener(TMP_InputField_ChangeCameraFieldOfView);

        xPt.onEndEdit.AddListener(TMP_InputField_ChangePointingVector);
        yPt.onEndEdit.AddListener(TMP_InputField_ChangePointingVector);
        zPt.onEndEdit.AddListener(TMP_InputField_ChangePointingVector);

        attachedBodyDropdown.onValueChanged.AddListener(MainAttachedBodyDropdownValueSelected);
        targetDropdown.onValueChanged.AddListener((MainTargetBodyDropdownValueSelected));

        VizardGUISettings.PopulateList(viewDropdown, presetVectorsToPlanet);
        viewDropdown.onValueChanged.AddListener(Dropdown_ChangeBodyCameraView);

        myCameraImage.GetComponent<CameraViewImageMethods>()
            .InitializeCameraViewImage(myCamera.GetComponent<Camera>(), true, 240, 240);
    }

    void OnEnable()
    {
        //Locations may be added dynamically, so these lists need to be refreshed
        VizardGUISettings.CreateBodyListForDropdown(attachedBodyDropdown, "attachBody", false, true, true, false);
        VizardGUISettings.CreateBodyListForDropdown(targetDropdown, "targetBody", true, true, true, false);

        if (firstActivation)
        {
            alongVectorPanel.SetActive(false);

            //Create quick reference to the panel's camera's controller
            cameraController = myCamera.GetComponent<StandardCameraController>();

            //Set the FOV to the camera's current field of view
            fieldOfViewInputField.GetComponent<TMP_InputField>().text =
                $"{myCamera.GetComponent<Camera>().fieldOfView}";

            myCamera.SetActive(true);

            if (DataManager.UseVR)
            {
                RectTransform camPanel = GetComponent<RectTransform>();
                Vector3 panelPosition = camPanel.localPosition;
                panelPosition.z = 12f;
                camPanel.localPosition = panelPosition;

                RectTransform camImage = myCameraImage.GetComponent<RectTransform>();
                Vector3 imagePos = camImage.localPosition;
                imagePos.z = -1f;
                camImage.localPosition = imagePos;

                //Set the UI elements raycast needs to interact with to VR_UI_Raycast layer
                closePanelButton.gameObject.layer = VizardGUISettings.VRUIRaycastLayer;
                transform.GetChild(0).gameObject.layer = VizardGUISettings.VRUIRaycastLayer; //Drag Bar
                verboseButton.gameObject.layer = VizardGUISettings.VRUIRaycastLayer;
            }

            ToggleVerbosePanel();
            firstActivation = false;
        }
    }

    private void CaptureCameraImage()
    {
        myCameraImage.GetComponent<CameraViewImageMethods>().CaptureScreenshot(myCamera.name);
    }

    private void Toggle_BodyTargetSubpanel(bool newValue)
    {
        inBodyMode = newValue;
        cameraController.SetBodyMode(newValue);
        if (inBodyMode)
        {
            atBodyPanel.SetActive(true);
            alongVectorPanel.SetActive(false);
        }
        else
        {
            atBodyPanel.SetActive(false);
            alongVectorPanel.SetActive(true);
            Vector3 pointingVector = cameraController.pointingVector.normalized;
            xPt.text = $"{-pointingVector[2]}";
            yPt.text = $"{pointingVector[0]}";
            zPt.text = $"{pointingVector[1]}";
            TMP_InputField_ChangePointingVector("0.00");
        }
    }

    private void TMP_InputField_ChangeCameraFieldOfView(string newValue)
    {
        fieldOfViewInputField.GetComponent<TMP_InputField>().text = newValue;

        ChangeCameraFieldOfView(float.Parse(newValue));
    }

    public void ChangeCameraFieldOfView(float fovValue)
    {
        //Camera Field of View is limited to between 1 and 179 degrees
        if (fovValue <= 0)
        {
            fovValue = 0.0001f;
            fieldOfViewInputField.GetComponent<TMP_InputField>().text = "0.0001";
            Debug.Log("Camera is limited by the visualization's code to a minimum FOV of 0.0001 degrees.");
        }
        else if (fovValue >= 180f)
        {
            fovValue = 179.9999f;
            fieldOfViewInputField.GetComponent<TMP_InputField>().text = "179.9999";
            Debug.Log("Camera is limited by the visualization's code to a maximum FOV of 179.9999 degrees.");
        }

        myCamera.GetComponent<Camera>().fieldOfView = fovValue;
        myCamera.GetComponent<StandardCameraController>().RequestHUDUpdate();
    }

    private void TMP_InputField_ChangePointingVector(string newValue)
    {
        try
        {
            Vector3 pointingVector =
                new Vector3(float.Parse(xPt.text), float.Parse(yPt.text), float.Parse(zPt.text)); //Still in BSK CS
            cameraController.ChangePointingVector(pointingVector);
        }
        catch
        {
            Debug.Log("Incorrect input string format for pointing vector.");
        }
    }

    private void Dropdown_ChangeBodyCameraView(int listOption)
    {
        if (listOption != 0)
        {
            cameraController.ChangeCameraVectorToPlanet(listOption);
        }
    }

    private void ClosePanel()
    {
        if (!VizardGUISettings.ShowCamPreviews)
        {
            panelToggle.isOn = false;
        }

        gameObject.SetActive(false);
    }

    private void Toggle_UserSetCameraPosition(bool isOn)
    {
        Debug.Log("Toggle user set camera position.");
        if (isOn)
        {
            Vector3 currentCamPosition = cameraController.GetCurrentCameraOrigin(); //Unity CS
            //Convert to BSK coordinate system for display
            xPos.text = $"{-currentCamPosition.z}";
            yPos.text = $"{currentCamPosition.x}";
            zPos.text = $"{currentCamPosition.y}";
            camPositionPanel.SetActive(true);
            cameraController.useCustomCamPosition = true;
            if (verbosePanel.activeSelf)
            {
                Vector2 currentSize = GetComponent<RectTransform>().sizeDelta;
                GetComponent<RectTransform>().sizeDelta = new Vector2(currentSize.x, currentSize.y + 45);
            }
        }
        else
        {
            cameraController.useCustomCamPosition = false;
            camPositionPanel.SetActive(false);
            Vector2 currentSize = GetComponent<RectTransform>().sizeDelta;
            GetComponent<RectTransform>().sizeDelta = new Vector2(currentSize.x,currentSize.y-45);
        }
    }

    private void TMP_InputField_ChangeCameraOrigin(string newValue)
    {
        try
        {
            Vector3 cameraPosition =
                new Vector3(float.Parse(xPos.text), float.Parse(yPos.text), float.Parse(zPos.text)); //Still in BSK CS
            cameraController.SetCurrentCameraOrigin(cameraPosition);
        }
        catch
        {
            Debug.Log("Incorrect input string format for camera position.");
        }
    }

    public void SetupCameraPanelWithUserSettings(VizMessage.Types.StandardCameraSettings mySettings, int index)
    {
        myCamera.SetActive(true);

        bool foundAttachBody = attachedBodyDropdown.GetComponent<HoverDropdown>()
            .SetOptionFromMessages(mySettings.SpacecraftName);
        if (!foundAttachBody)
        {
            VizardGUISettings.UpdateErrorMessages(
                $"Could not find body with name matching string \"{mySettings.SpacecraftName}\"provided in Standard Camera Settings [{index}] ParentBody field.",
                true);
        }

        if (mySettings.SetMode == 0)
        {
            pointCameraAtBodyToggle.isOn = true;
            inBodyMode = true;

            bool foundTargetBody = targetDropdown.GetComponent<HoverDropdown>()
                .SetOptionFromMessages(mySettings.BodyTarget);
            if (!foundTargetBody)
            {
                VizardGUISettings.UpdateErrorMessages(
                    $"Could not find body with name matching string \"{mySettings.SpacecraftName}\"provided in Standard Camera Settings [{index}] BodyTarget field.",
                    true);
            }
            else
            {
                GameObject cameraTarget =
                    CelestialBodyStateUtilities.GetGameObjectWithBodyName(mySettings.BodyTarget,
                        targetBodyEffectorParent);
                cameraController.ChangeStandardCameraTarget(cameraTarget);
            }

            int viewValue = mySettings.SetView;
            viewDropdown.value = viewValue + 1;
        }
        else
        {
            pointCameraAtBodyToggle.isOn = false;
            Toggle_BodyTargetSubpanel(false);
            pointCameraAlongVectorToggle.isOn = true;
            alongVectorPanel.SetActive(true);
            inBodyMode = false;
            xPt.text = $"{mySettings.PointingVector[0]}";
            yPt.text = $"{mySettings.PointingVector[1]}";
            zPt.text = $"{mySettings.PointingVector[2]}";
            TMP_InputField_ChangePointingVector("0");
        }

        if (mySettings.FieldOfView > 0)
        {
            TMP_InputField_ChangeCameraFieldOfView((mySettings.FieldOfView).ToString());
        }

        if (mySettings.Position.Count == 3)
        {
            useCustomCamPositionToggle.isOn = true;
            camPositionPanel.SetActive(true);
            xPos.text = $"{mySettings.Position[0]}";
            yPos.text = $"{mySettings.Position[1]}";
            zPos.text = $"{mySettings.Position[2]}";
            cameraController.useCustomCamPosition = true;
            TMP_InputField_ChangeCameraOrigin("0");
        }

        if (mySettings.DisplayName != "")
        {
            panelName.text = mySettings.DisplayName;
            UpdateLabels(mySettings.DisplayName);
        }

        VizardGUISettings.SetSecondaryCameraLayerMask(myCamera.GetComponent<Camera>(),
            mySettings.ShowHUDElementsInImage == 1);
        showHUDElementsToggle.isOn = mySettings.ShowHUDElementsInImage == 1;
    }

    private void UpdateLabels(string nameToUse)
    {
        panelToggle.GetComponentInChildren<TextMeshProUGUI>().text = nameToUse;
        transform.name = nameToUse + "Panel";
        myCamera.name = nameToUse + "Camera";
        myCamera.GetComponent<SecondaryCameraHUDMethods>().cameraLabel.GetComponent<TextMeshProUGUI>().text = nameToUse;
    }

    /// <summary>
    ///This method must be implemented for any subpanel component that needs to do something when the panel is resized
    /// Do not delete or make private.
    /// </summary>
    /// <param name="newPanelDimensions">new panel extents</param>
    public void ApplyPanelResize(Vector2 newPanelDimensions)
    {
        Debug.Log($"Panel resize {newPanelDimensions}");
        int imageWidth;
        int imageHeight;
        if (verbosePanel.activeSelf)
        {
            imageWidth = (int) newPanelDimensions.x - 10;
            imageHeight = (int) newPanelDimensions.y - 192;
            if (cameraController.useCustomCamPosition)
            {
                imageHeight = (int) newPanelDimensions.y - 242;
            }
        }
        else
        {
            imageWidth = (int) newPanelDimensions.x - 10;
            imageHeight = (int) newPanelDimensions.y - 37;
        }

        GetComponentInChildren<CameraViewImageMethods>()
            .InitializeCameraViewImage(myCamera.GetComponent<Camera>(), true, imageWidth, imageHeight);
        myCamera.GetComponent<StandardCameraController>().RequestHUDUpdate();
    }

    private void ToggleVerbosePanel()
    {
        Vector2 oldSize = GetComponent<RectTransform>().sizeDelta;
        Vector2 imageSize = myCameraImage.GetComponent<RectTransform>().sizeDelta;
        if (DataManager.UseVR)
        {
            verbosePanel.SetActive(false);
            if (VRVerbosePanel.activeSelf)
            {
                VRVerbosePanel.SetActive(false);
                GetComponent<RectTransform>().sizeDelta = new Vector2(oldSize.x, imageSize.y + 37);
                RestoreSettings();
            }
            else
            {
                VRVerbosePanel.SetActive(true);
                GetComponent<RectTransform>().sizeDelta = new Vector2(oldSize.x, imageSize.y + 90);
            }
        }
        else
        {
            VRVerbosePanel.SetActive(false);
            if (verbosePanel.activeSelf)
            {
                verbosePanel.SetActive(false);
                GetComponent<RectTransform>().sizeDelta = new Vector2(oldSize.x, imageSize.y + 37);
                RestoreSettings();
            }
            else
            {
                verbosePanel.SetActive(true);
                GetComponent<RectTransform>().sizeDelta = cameraController.useCustomCamPosition ? 
                    new Vector2(oldSize.x, imageSize.y + 242) : 
                    new Vector2(oldSize.x, imageSize.y + 192);
            }
        }
    }

    private void RestoreSettings()
    {
        attachedBodyDropdown.GetComponent<HoverDropdown>()
            .SetForOptionWithDropdownLockout(cameraController.GetAttachedBody().name);
        if (atBodyPanel.activeSelf)
        {
            targetDropdown.GetComponent<HoverDropdown>()
                .SetForOptionWithDropdownLockout(cameraController.GetBodyTargetName());
        }
    }

    private void SetAttachBody(string attachBodyName)
    {
        if (attachBodyName != "Select Body")
        {
            GameObject newAttachBody =
                CelestialBodyStateUtilities.GetGameObjectWithBodyName(attachBodyName, attachedBodyEffectorParent);
            if (attachBodyName == cameraController.GetBodyTargetName())
            {
                cameraController.SetBodyMode(false);
                targetDropdown.value = 0;
            }

            cameraController.ChangeStandardCameraAttachedBody(newAttachBody);
        }
    }

    private void SetCameraTargetBody(string targetBody)
    {
        if (targetBody != "Select Body")
        {
            if (targetBody != myCamera.transform.parent.name)
            {
                GameObject targetToUse =
                    CelestialBodyStateUtilities.GetLineTargetGameObjectWithName(targetBody, targetBodyEffectorParent);
                cameraController.ChangeStandardCameraTarget(targetToUse);
            }
            else
            {
                targetBodyEffectorParent = "";
                targetDropdown.options[0].text = "Select Body";
                targetDropdown.value = 0;
            }
        }
    }

    private void MainAttachedBodyDropdownValueSelected(int optionValue)
    {
        if (optionValue != 0)
        {
            attachedBodyEffectorParent = "";
            attachedBodyDropdown.options[0].text = "Select Body";
            string attachBody = attachedBodyDropdown.options[optionValue].text;
            SetAttachBody(attachBody);

            if (openSubMenu != null)
            {
                openSubMenu.SetActive(false);
            }
        }
    }

    private void MainTargetBodyDropdownValueSelected(int optionValue)
    {
        if (optionValue != 0)
        {
            targetBodyEffectorParent = "";
            targetDropdown.options[0].text = "Select Body";
            string targetBody = targetDropdown.options[optionValue].text;
            SetCameraTargetBody(targetBody);
            if (openSubMenu != null)
            {
                openSubMenu.SetActive(false);
            }
        }
    }

    public void SubDropdownValueSelected(string[] dropdownData)
    {
        Debug.Log("I received a sub dropdown selection.");
        if (dropdownData[0] == "attachBody")
        {
            attachedBodyDropdown.options[0].text = dropdownData[2];
            attachedBodyDropdown.value = 0;
            attachedBodyDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dropdownData[2];
            attachedBodyEffectorParent = dropdownData[1];
            SetAttachBody(dropdownData[2]);
        }
        else if (dropdownData[0] == "targetBody")
        {
            targetDropdown.options[0].text = dropdownData[2];
            targetDropdown.value = 0;
            targetDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dropdownData[2];
            targetBodyEffectorParent = dropdownData[1];
            SetCameraTargetBody(dropdownData[2]);
        }
    }

    public void SetOpenSubMenu(GameObject openMenu)
    {
        openSubMenu = openMenu;
    }

    public void CloseOpenSubMenu()
    {
        openSubMenu.SetActive(false);
        openSubMenu = null;
    }

    private void ToggleHUDElements(bool isOn)
    {
        VizardGUISettings.SetSecondaryCameraLayerMask(myCamera.GetComponent<Camera>(), isOn);
    }
}