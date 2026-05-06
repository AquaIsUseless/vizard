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
/// Manages creation and updates of keep-out or keep-in
/// cones from the Keep Out/In Cone Panel
/// </summary>
public class AddKeepOutInConePanelMethods : MonoBehaviour
{
    //The following components are wired in the editor to the button in the View subpanel
    [Header("Panel GUI Components")]
    public Toggle keepOutToggle;
    public Toggle keepInToggle;
    public TMP_InputField nameField;
    public TMP_InputField xCoord;
    public TMP_InputField yCoord;
    public TMP_InputField zCoord;
    public TMP_InputField xNormal;
    public TMP_InputField yNormal;
    public TMP_InputField zNormal;
    public TMP_InputField angle;
    public TMP_InputField height;
    public Button ColorButton;
    public Image colorSample;
    public Slider transparencySlider;
    public TextMeshProUGUI errorText;
    public TMP_Dropdown fromBodyDropdown;
    public TMP_Dropdown toBodyDropdown;
    public Button ApplySettingsButton;
    public Button CancelButton;

    [Header("Support Panels")]
    public GameObject coneInventoryPanel;
    public GameObject colorWheelPanel;
    public bool isColorPanelReturn;
    
    private List<string> bodyList = new List<string>();
    private bool firstClick = true;

    private GameObject selectedButton;
    private int buttonCounter;

    private string fromBodyEffectorParent="";
    private string toBodyEffectorParent="";

    private int fromBodyDropdownOption;
    private int toBodyDropdownOption;
    private GameObject openSubMenu;


    // Use this for initialization
    void Start()
    {
        transparencySlider.onValueChanged.AddListener(ChangeTransparency);
        ColorButton.onClick.AddListener(EnableColorChooser);
        ApplySettingsButton.onClick.AddListener(ApplyConeSettings);
        CancelButton.onClick.AddListener(CancelConeBuild);
        fromBodyDropdown.onValueChanged.AddListener(MainFromBodyDropdownValueSelected);
        toBodyDropdown.onValueChanged.AddListener(MainToBodyDropdownValueSelected);
    }

    void OnEnable()
    {
        bodyList = VizardGUISettings.CreateBodyListForDropdown(fromBodyDropdown, "fromBody", true, true, true, false);
        VizardGUISettings.CreateBodyListForDropdown(toBodyDropdown, "toBody", true, true, true, false);
        if (firstClick)
        {
           PanelStartupTasks();
        }

        errorText.text = "";
        if (isColorPanelReturn)
        {
            isColorPanelReturn = false;

            if (fromBodyDropdownOption != 0)
            {
                fromBodyDropdown.value = fromBodyDropdownOption;
            }

            if (toBodyDropdownOption != 0)
            {
                toBodyDropdown.value = toBodyDropdownOption;
            }
        }
        else
        {
            if (coneInventoryPanel.GetComponent<InventoryPanelMethods>().useDefaultValuesInSettings)
            {
                UseDefaults();
            }
            else
            {
                RestoreConeSettings(coneInventoryPanel.GetComponent<InventoryPanelMethods>().GetSelectedButton());
            }
        }

        coneInventoryPanel.SetActive(false);
        transform.SetAsLastSibling();
    }

    private void PanelStartupTasks()
    {
        if (firstClick)
        {
            //Find the planet manager so that the solar system GameObject list can be accessed when changing camera target

            fromBodyDropdown.gameObject.GetComponent<HoverDropdown>().enabled = true;
            toBodyDropdown.gameObject.GetComponent<HoverDropdown>().enabled = true;
            firstClick = false;
        }
    }

    private void ChangeTransparency(float newValue)
    {
        colorSample.color = new Color(colorSample.color.r, colorSample.color.g, colorSample.color.b, newValue);
    }

    private void ApplyConeSettings()
    {
        string fromBody = bodyList[fromBodyDropdown.value];
        string toBody = bodyList[toBodyDropdown.value];

        if ((fromBody != "Select Body") && (fromBody != "Select Body") && (fromBody != toBody))
        {
            GameObject parentBody =
                CelestialBodyStateUtilities.GetLineTargetGameObjectWithName(fromBody, fromBodyEffectorParent);

            Vector3 origin = new Vector3(float.Parse(xCoord.text), float.Parse(yCoord.text), float.Parse(zCoord.text));
            Vector3 normal = new Vector3(float.Parse(xNormal.text), float.Parse(yNormal.text),
                float.Parse(zNormal.text));

            if (normal != Vector3.zero)
            {
                if (selectedButton == null)
                {
                    GameObject newKeepOutInCone =
                        Instantiate(Resources.Load("Prefabs/SpacecraftHUD/KeepOutInConeTemplate") as GameObject,
                            parentBody.transform, true);
                    newKeepOutInCone.GetComponent<DrawKeepOutInCone>().InitializeKeepOutInCone(fromBody, toBody,
                         keepOutToggle.isOn, colorSample.color, origin, normal, float.Parse(angle.text),
                        float.Parse(height.text), nameField.text, toBodyEffectorParent);
                    string coneLabel = newKeepOutInCone.GetComponent<DrawKeepOutInCone>().GetConeLabel();
                    newKeepOutInCone.name = coneLabel + " ID: " + buttonCounter;

                    GameObject newKeepOutInConeButton =
                        Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericButtonWithLabelAndImage") as GameObject);
                    newKeepOutInConeButton.AddComponent<InventoryButton>();
                    newKeepOutInConeButton.GetComponent<InventoryButton>().SetupButton(newKeepOutInCone,
                        newKeepOutInCone.transform.GetChild(0).gameObject, colorSample.color, coneInventoryPanel,
                        coneLabel, buttonCounter);
                    buttonCounter += 1;

                    
                    coneInventoryPanel.SetActive(true);
                    coneInventoryPanel.GetComponent<InventoryPanelMethods>()
                        .AddItemButtonToInventory(newKeepOutInConeButton);
                    newKeepOutInCone.GetComponent<DrawKeepOutInCone>().SetMyConeButton(newKeepOutInConeButton);
                    //newKeepOutInCone.GetComponentInChildren<ConeTrigger>().coneTriggerRestart();
                }
                else
                {
                    
                    GameObject coneToModify = selectedButton.GetComponent<InventoryButton>().myGUIObject;
                    coneToModify.transform.SetParent(parentBody.transform);
                    coneToModify.GetComponent<DrawKeepOutInCone>().InitializeKeepOutInCone(fromBody, toBody, 
                        keepOutToggle.isOn, colorSample.color, origin, normal, float.Parse(angle.text),
                        float.Parse(height.text), nameField.text);
                    //coneToModify.GetComponentInChildren<ConeTrigger>().coneTriggerRestart();
                    coneInventoryPanel.SetActive(true);
                    coneInventoryPanel.GetComponent<InventoryPanelMethods>()
                        .UpdateItemButtonInInventory(selectedButton);
                }
            }
            else
            {
                errorText.text = "Please set a non-zero cone normal vector.";
            }
        }
        else
        {
            errorText.text = "Please select two distinct bodies for vector of interest.";
        }
    }

    private void CancelConeBuild()
    {
        coneInventoryPanel.SetActive(true);
        this.gameObject.SetActive(false);
    }

    private void EnableColorChooser()
    {
        colorWheelPanel.SetActive(true);
        colorWheelPanel.GetComponent<ColorWheelMethods>().SetCallerName("coneBuilder");
        isColorPanelReturn = true;
    }

    public void SetConeColor(Color newColor)
    {
        colorSample.color = new Color(newColor.r, newColor.g, newColor.b, transparencySlider.value);
        if (!transparencySlider.IsActive())
        {
            colorSample.color = new Color(newColor.r, newColor.g, newColor.b, 0.16f);
        }
    }

    public Color GetConeColor()
    {
        if (selectedButton != null)
        {
            return selectedButton.GetComponent<InventoryButton>().GetGUIObjectColor();
        }
        else
        {
            return colorSample.color;
        }
    }

    public void CloseColorWheelPanel()
    {
        colorWheelPanel.SetActive(false);
    }

    private void UseDefaults()
    {
        selectedButton = null;
        keepOutToggle.isOn = true;
        keepInToggle.isOn = false;
        nameField.text = "autogeneratenameifblank";
        xCoord.text = "0.0";
        yCoord.text = "0.0";
        zCoord.text = "0.0";
        xNormal.text = "0.0";
        yNormal.text = "0.0";
        zNormal.text = "0.0";
        angle.text = "45.0";
        height.text = "1.0";

        colorSample.color = new Color(1f, (float) 168 / 255, (float) 38 / 255, 0.35f);
        transparencySlider.value = 0.16f;

        fromBodyDropdown.value = 0;
        toBodyDropdown.value = 0;
    }

    private void RestoreConeSettings(GameObject inventoryButton)
    {
        if (firstClick)
        {
            PanelStartupTasks();
            firstClick = false;
        }
        selectedButton = inventoryButton;
        DrawKeepOutInCone myConeSettings = selectedButton.GetComponent<InventoryButton>().myGUIObject
            .GetComponent<DrawKeepOutInCone>();
        if (myConeSettings.GetIsKeepOut())
        {
            keepOutToggle.isOn = true;
        }
        else
        {
            keepInToggle.isOn = true;
        }

        Vector3 coneOrigin = myConeSettings.GetConeOrigin();
        Vector3 coneNormal = myConeSettings.GetConeNormal();
        float coneAngle = myConeSettings.GetConeAngle();
        float coneHeight = myConeSettings.GetConeHeight();
        bool regenerateName = myConeSettings.IsAutogeneratedName();
        nameField.text = regenerateName ? "autogeneratenameifblank" : myConeSettings.GetConeLabel();

        xCoord.text = $"{coneOrigin.x}";
        yCoord.text =  $"{coneOrigin.y}";
        zCoord.text =  $"{coneOrigin.z}";
        xNormal.text =  $"{coneNormal.x}";
        yNormal.text = $"{coneNormal.y}";
        zNormal.text = $"{coneNormal.z}";
        angle.text = $"{coneAngle}";
        height.text = $"{coneHeight}";

        Color myConeColor = myConeSettings.GetConeColor();
        colorSample.color = myConeColor;
        transparencySlider.value = myConeColor.a;

        string fromBodyName = myConeSettings.GetFromBody();
        string toBodyName = myConeSettings.GetToBody();

        fromBodyDropdown.GetComponent<HoverDropdown>().SetForOptionWithDropdownLockout(fromBodyName);
        toBodyDropdown.GetComponent<HoverDropdown>().SetForOptionWithDropdownLockout(toBodyName);
    }

    public void CreateConeFromSettingsMessage(VizMessage.Types.KeepOutInCone newCone)
    {
        if (newCone.FromBodyName != newCone.ToBodyName)
        {
            GameObject parentBody = CelestialBodyStateUtilities.GetGameObjectWithBodyName(newCone.FromBodyName);
            GameObject toBody = CelestialBodyStateUtilities.GetGameObjectWithBodyName(newCone.ToBodyName);
            if (parentBody == null)
            {
                string errMsg =
                    System.String.Format(
                        "Cone from {0} to {1} could not be added because {0} could not be matched to a simulated body name. ",
                        newCone.FromBodyName, newCone.ToBodyName);
                VizardGUISettings.UpdateErrorMessages(errMsg, true);
                return;
            }

            if (toBody == null)
            {
                string errMsg =
                    System.String.Format(
                        "Cone from {0} to {1} could not be added because {1} could not be matched to a simulated body name.",
                        newCone.FromBodyName, newCone.ToBodyName);
                VizardGUISettings.UpdateErrorMessages(errMsg, true);
                return;
            }

            //Conversion from Basilisk coordinate frame to Unity CS for origin and normal vector happens in DrawKeepOutCone.cs
            Vector3 origin = new Vector3((float) newCone.Position[0], (float) newCone.Position[1],
                (float) newCone.Position[2]);
            Vector3 normal = new Vector3((float) newCone.NormalVector[0], (float) newCone.NormalVector[1],
                (float) newCone.NormalVector[2]);
            if (normal != Vector3.zero)
            {
                GameObject newKeepOutInCone =
                    Instantiate(Resources.Load("Prefabs/SpacecraftHUD/KeepOutInConeTemplate") as GameObject, parentBody.transform, true);
                Color coneColor = new Color(1f, .5f, 0, 1f);
                if (newCone.ConeColor.Count == 3)
                {
                    coneColor = new Color(newCone.ConeColor[0] / 255f, newCone.ConeColor[1] / 255f,
                        newCone.ConeColor[2] / 255f, 1f);
                }else if (newCone.ConeColor.Count > 3)
                {
                    coneColor = new Color(newCone.ConeColor[0] / 255f, newCone.ConeColor[1] / 255f,
                        newCone.ConeColor[2] / 255f, newCone.ConeColor[3] / 255f);
                }

                newKeepOutInCone.GetComponent<DrawKeepOutInCone>().InitializeKeepOutInCone(newCone.FromBodyName,
                    newCone.ToBodyName, !newCone.IsKeepIn, coneColor, origin, normal,
                    (float) newCone.IncidenceAngle, (float) newCone.ConeHeight, newCone.ConeName);
                string coneLabel = newKeepOutInCone.GetComponent<DrawKeepOutInCone>().GetConeLabel();
                newKeepOutInCone.name = coneLabel + " ID: " + buttonCounter;

                GameObject newKeepOutInConeButton =
                    Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericButtonWithLabelAndImage") as GameObject);
                newKeepOutInConeButton.AddComponent<InventoryButton>();
                newKeepOutInConeButton.GetComponent<InventoryButton>().SetupButton(newKeepOutInCone,
                    newKeepOutInCone.transform.GetChild(0).gameObject, coneColor, coneInventoryPanel, coneLabel,
                    buttonCounter);
                buttonCounter += 1;

                coneInventoryPanel.SetActive(true);
                coneInventoryPanel.GetComponent<InventoryPanelMethods>()
                    .AddItemButtonToInventory(newKeepOutInConeButton);
                newKeepOutInCone.GetComponent<DrawKeepOutInCone>().SetMyConeButton(newKeepOutInConeButton);
                coneInventoryPanel.SetActive(false);
            }
            else
            {
                VizardGUISettings.UpdateErrorMessages("Please set a non-zero keep out/in cone normal vector.");
            }
        }
        else
        {
            VizardGUISettings.UpdateErrorMessages(
                "Keep Out cone body selections invalid.Please ensure that two different bodies have been specified in settings message.", true);
        }
    }

    private void MainFromBodyDropdownValueSelected(int optionValue)
    {
        fromBodyDropdownOption = optionValue;
        if (optionValue != 0)
        {
            fromBodyEffectorParent = "";
            fromBodyDropdown.options[0].text = "Select Body";
            if (openSubMenu != null)
            {
                openSubMenu.SetActive(false);
            }
        }
    }

    private void MainToBodyDropdownValueSelected(int optionValue)
    {
        toBodyDropdownOption = optionValue;
        if (optionValue != 0)
        {
            toBodyEffectorParent = "";
            toBodyDropdown.options[0].text = "Select Body";
            if (openSubMenu != null)
            {
                openSubMenu.SetActive(false);
            }
        }
    }

    public void SubDropdownValueSelected(string[] dropdownData)
    {
        if (dropdownData[0] == "fromBody")
        {
            fromBodyDropdown.options[0].text = dropdownData[2];
            fromBodyDropdown.value = 0;
            fromBodyDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dropdownData[2];
            fromBodyEffectorParent = dropdownData[1];
        }
        else if (dropdownData[0] == "toBody")
        {
            toBodyDropdown.options[0].text = dropdownData[2];
            toBodyDropdown.value = 0;
            toBodyDropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = dropdownData[2];
            toBodyEffectorParent = dropdownData[1];
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
