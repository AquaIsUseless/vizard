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
using UnityEngine;
using UnityEngine.UI;
using TMPro;
/// <summary>
/// Handles inputs to Light panel to create and update lights,
/// builds lights from VizMessages.
/// </summary>
public class LightPanelMethods : MonoBehaviour
{
    public Button ApplyButton;
    public TextMeshProUGUI ApplyButtonText;
    public Button CancelButton;
    public Button AddNewLightButton;
    public Button RemoveButton;

    public GameObject lightInventory;
    public GameObject lightSettingsSubpanel;


    public TMP_InputField nameField;

    private List<string> bodyList = new List<string>();
    public TMP_Dropdown parentBodyDropdown;

    public TMP_InputField xPos;
    public TMP_InputField yPos;
    public TMP_InputField zPos;

    public TMP_InputField xNorm;
    public TMP_InputField yNorm;
    public TMP_InputField zNorm;

    public TMP_InputField FOV;
    public TMP_InputField range;
    public TMP_InputField Intensity;

    public Toggle showMarker;
    public TMP_InputField markerDia;

    public Toggle showFlare;
    public TMP_InputField flareSpeed;
    public TMP_InputField flareBrightness;
    public TMP_InputField gammaSetting;

    public Button ColorButton;
    public Image colorSample;
    public GameObject colorWheelPanel;

    public TextMeshProUGUI errorText;

    private List<GameObject> itemButtons = new List<GameObject>();
    public int lightCounter;

    public GameObject selectedButton;

    private string parentBodyEffectorParent = "";
    private GameObject openSubMenu;

    // Start is called before the first frame update
    void Start()
    {
        AddNewLightButton.onClick.AddListener(AddNewLight);
        ApplyButton.onClick.AddListener(ApplySettings);
        RemoveButton.onClick.AddListener(RemoveLight);
        CancelButton.onClick.AddListener(CancelChanges);
        ColorButton.onClick.AddListener(EnableColorChooser);
        VizardGUISettings.CreateBodyListForDropdown(parentBodyDropdown, "parentBody", false, false, true, false);
        if (!itemButtons.Contains(AddNewLightButton.transform.gameObject))
        {
            itemButtons.Add(AddNewLightButton.transform.gameObject);
        }

        showFlare.onValueChanged.AddListener(ToggleLensFlareFromLightPanel);
        showMarker.onValueChanged.AddListener(ToggleMarkerFromLightPanel);
        parentBodyDropdown.onValueChanged.AddListener(MainParentBodyDropdownValueSelected);
    }

    public void OnClose()
    {
        selectedButton = null;
        nameField.text = "";
        errorText.text = "";
        lightSettingsSubpanel.SetActive(false);
    }

    private void AddNewLight()
    {
        selectedButton = null;
        lightSettingsSubpanel.SetActive(true);
        ApplyButtonText.text = "Add Light";
        errorText.text = "";
        nameField.text = "";

        if (Intensity.text == "")
        {
            Intensity.text = "1.0";
        }

        if (markerDia.text == "")
        {
            markerDia.text = "0.01";
        }

        if (flareSpeed.text == "")
        {
            flareSpeed.text = "10.0";
        }

        if (flareBrightness.text == "")
        {
            flareBrightness.text = "0.3";
        }

        if (gammaSetting.text == "")
        {
            gammaSetting.text = "0.8";
        }

        if (xPos.text == "")
        {
            xPos.text = "0.0";
            yPos.text = "0.0";
            zPos.text = "0.0";
        }

        if (xNorm.text == "")
        {
            xNorm.text = "0.0";
            yNorm.text = "0.0";
            zNorm.text = "0.0";
        }

        if (FOV.text == "")
        {
            FOV.text = "5.0";
        }

        if (range.text == "")
        {
            range.text = "100.0";
        }
    }

    private void ApplySettings()
    {
        string lightName = nameField.text;
        if (lightName == "")
        {
            lightName = "Light " + lightCounter;
        }

        bool appliedSettings;
        if (selectedButton == null)
        {
            GameObject newLight =
                Instantiate(Resources.Load("Prefabs/SpacecraftHUD/LightHud") as GameObject);

            appliedSettings = ApplyCurrentSettingsToLight(newLight, lightName);
            if (appliedSettings)
            {
                CreateButtonForLight(lightName, newLight);
                CreateLightLabel(lightName, newLight.name, newLight);
                lightSettingsSubpanel.SetActive(false);
            }
            else
            {
                Destroy(newLight);
            }
        }
        else
        {
            selectedButton.GetComponentInChildren<TextMeshProUGUI>().text = lightName;
            ApplyCurrentSettingsToLight(selectedButton.GetComponent<InventoryButton>().myGUIObject, lightName);
        }
    }

    public GameObject AddLightFromMessage(VizProtobufferMessage.VizMessage.Types.Light lightMsg, GameObject scBody,
        int scIndex, int lightIndex)
    {
        string lightName = lightMsg.Label;
        if (lightName == "")
        {
            lightName = "Light" + lightCounter;
        }

        GameObject newLight = Instantiate(Resources.Load("Prefabs/SpacecraftHUD/LightHud") as GameObject);
        bool settingsWorked = newLight.GetComponent<LightHUDMethods>()
            .InitializeLightFromMessage(lightName, lightMsg, scBody, scIndex, lightIndex);
        if (settingsWorked)
        {
            if (itemButtons.Count == 0)
            {
                itemButtons.Add(AddNewLightButton.transform.gameObject);
            }

            CreateButtonForLight(newLight.name, newLight);
            CreateLightLabel(newLight.name, scBody.name, newLight);
            return newLight;
        }

        Destroy(newLight);
        return null;
    }

    private GameObject CreateButtonForLight(string lightName, GameObject newLight)
    {
        GameObject newButton =
            Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericSmallToggleWithButton") as GameObject, lightInventory.transform, true);
        newButton.name = lightName;
        newButton.AddComponent<InventoryButton>();
        newButton.GetComponent<InventoryButton>().SetupButton(newLight, newLight, colorSample.color, this.gameObject,
            lightName, lightCounter, false);
        newButton.GetComponent<InventoryButton>()
            .AddHideShowListenerToToggle(newLight.GetComponent<LightHUDMethods>().myLight.enabled);
        newLight.GetComponent<LightHUDMethods>().inventoryButtonLightOnToggle = newButton.GetComponent<Toggle>();

        itemButtons.Add(newButton);
        newButton.GetComponent<RectTransform>().localScale = Vector3.one;
        newButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -25 - (itemButtons.Count - 2) * 20);
        RectTransformExtensions.SetRight(newButton.GetComponent<RectTransform>(), 0);


        lightCounter += 1;
        return newButton;
    }

    private void RemoveLight()
    {
        List<GameObject> remainingButtons = new List<GameObject>();
        int buttonCount = 0;
        foreach (GameObject button in itemButtons)
        {
            if (button != selectedButton)
            {
                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, buttonCount * -22);
                buttonCount += 1;
                remainingButtons.Add(button);
            }
        }

        Destroy(selectedButton.GetComponent<InventoryButton>().myGUIObject);
        Destroy(selectedButton);

        itemButtons = remainingButtons;
        lightSettingsSubpanel.SetActive(false);
    }

    private void CancelChanges()
    {
        selectedButton = null;
        lightSettingsSubpanel.SetActive(false);
    }

    private void EnableColorChooser()
    {
        colorWheelPanel.SetActive(true);
        colorWheelPanel.GetComponent<ColorWheelMethods>().SetCallerName("lightBuilder");
    }

    public void CloseColorWheelPanel()
    {
        colorWheelPanel.SetActive(false);
    }

    public void SetLightColor(Color newColor)
    {
        colorSample.color = newColor;
    }

    public Color GetLightColor()
    {
        return colorSample.color;
    }

    private void ToggleLensFlareFromLightPanel(bool isOn)
    {
        if (selectedButton != null)
        {
            selectedButton.GetComponent<InventoryButton>().myGUIObject.GetComponent<LightHUDMethods>()
                .ToggleLensFlareFromPanel(isOn);
        }
    }

    private void ToggleMarkerFromLightPanel(bool isOn)
    {
        if (selectedButton != null)
        {
            selectedButton.GetComponent<InventoryButton>().myGUIObject.GetComponent<LightHUDMethods>()
                .ToggleMarkerFromPanel(isOn);
        }
    }

    public void ItemButtonSelected(GameObject button)
    {
        selectedButton = button;
        lightSettingsSubpanel.SetActive(true);
        ApplyButtonText.text = "Apply Changes";
        errorText.text = "";
        PopulateFieldsWithLightSettings(button.GetComponent<InventoryButton>().myGUIObject);
    }

    private bool ApplyCurrentSettingsToLight(GameObject lightToSet, string lightName)
    {
        errorText.text = "";
        string parentBodyName = parentBodyDropdown.options[parentBodyDropdown.value].text;
        if (parentBodyName == "Select Body")
        {
            errorText.text = "Please select a parent body from the dropdown.";
            return false;
        }

        GameObject parentBody =
            CelestialBodyStateUtilities.GetGameObjectWithBodyName(parentBodyName, parentBodyEffectorParent);
        Vector3 origin = new Vector3(float.Parse(xPos.text), float.Parse(yPos.text), float.Parse(zPos.text)); //BSK CS
        Vector3 normal =
            new Vector3(float.Parse(xNorm.text), float.Parse(yNorm.text), float.Parse(zNorm.text)); //BSK CS
        if (normal != Vector3.zero)
        {
            float fov = float.Parse(FOV.text);
            if (fov is > 0f and < 180f)
            {
                float rangeVal = float.Parse(range.text);
                if (rangeVal > 0)
                {
                    float intensityVal = float.Parse(Intensity.text);
                    if (intensityVal > 0)
                    {
                        float markerDiaVal = float.Parse(markerDia.text);
                        if (markerDiaVal >= 0)
                        {
                            float flareSpeedVal = float.Parse(flareSpeed.text);
                            float flareBrightnessVal = float.Parse(flareBrightness.text);
                            bool flareOn = showFlare.isOn;
                            bool markerOn = showMarker.isOn;
                            lightToSet.GetComponent<LightHUDMethods>().InitializeLightFromPanel(lightName, parentBody,
                                origin, normal, fov, rangeVal, intensityVal, markerDiaVal, colorSample.color, flareOn,
                                flareSpeedVal, flareBrightnessVal, float.Parse(gammaSetting.text), markerOn);
                            return true;
                        }
                        else
                        {
                            errorText.text = "Set marker diameter greater than or equal to 0.";
                        }
                    }
                    else
                    {
                        errorText.text = "Set intensity greater than 0.";
                    }
                }
                else
                {
                    errorText.text = "Set range greater than 0.";
                }
            }
            else
            {
                errorText.text = "Field Of View be a value between 0 and 180 degrees.";
            }
        }
        else
        {
            errorText.text = "Please enter a non-zero normal vector";
        }

        return false;
    }

    private void PopulateFieldsWithLightSettings(GameObject lightToSet)
    {
        LightHUDMethods myLight = lightToSet.GetComponent<LightHUDMethods>();
        nameField.text = lightToSet.name;
        Vector3 pos = myLight.GetBSKPosition();
        Vector3 norm = myLight.GetBSKNormal();

        xPos.text = $"{pos.x}";
        yPos.text = $"{pos.y}";
        zPos.text = $"{pos.z}";
        xNorm.text = $"{norm.x}";
        yNorm.text = $"{norm.y}";
        zNorm.text = $"{norm.z}";
        FOV.text = $"{myLight.GetFOV()}";
        range.text = $"{myLight.GetRange()}";
        Intensity.text = $"{myLight.GetIntensity()}";
        markerDia.text = $"{myLight.GetMarkerDiameter()}";

        colorSample.color = myLight.myLight.color;
        gammaSetting.text = $"{myLight.GetGammaSetting()}";
        showMarker.isOn = myLight.GetMarkerOn();

        flareSpeed.text = $"{myLight.myFlare.fadeSpeed}";
        flareBrightness.text = $"{myLight.myFlare.brightness}";
        showFlare.isOn = myLight.myFlare.enabled;

        parentBodyDropdown.GetComponent<HoverDropdown>().SetForOptionWithDropdownLockout(myLight.GetParentBodyName());
    }

    private void CreateLightLabel(string lightName, string scName, GameObject newLight)
    {
        Vector2 lightScreenOffset = new Vector2(10, 10);
        GameObject lightLabel = LabelMaker.CreateLabel(lightName, scName, newLight, lightScreenOffset, "Lights");
        newLight.GetComponent<LightHUDMethods>().lightLabel = lightLabel;
        lightLabel.SetActive(VizardGUISettings.ShowLightLabels);
    }

    private void MainParentBodyDropdownValueSelected(int optionValue)
    {
        if (optionValue != 0)
        {
            parentBodyEffectorParent = "";
            parentBodyDropdown.options[0].text = "Select Body";
            if (openSubMenu != null)
            {
                openSubMenu.SetActive(false);
            }
        }
    }

    public void SubDropdownValueSelected(string[] dropdownData)
    {
        if (dropdownData[0] == "parentBody")
        {
            parentBodyDropdown.options[0].text = dropdownData[2];
            parentBodyDropdown.value = 0;
            parentBodyDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dropdownData[2];
            parentBodyEffectorParent = dropdownData[1];
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
}