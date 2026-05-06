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
/// <summary>
/// Handles user input to the rotating frame settings panel
/// </summary>
public class RotatingFrameSettingsMethods : MonoBehaviour
{
    [Header("Panel GUI Elements")]
    public TMP_Dropdown Body1Dropdown;
    public TMP_Dropdown Body2Dropdown;
    public Button selectButton;
    public TextMeshProUGUI errorText;
    
    private List<string> bodyList;
    private bool firstBuild = true;

    void Start()
    {
        selectButton.onClick.AddListener(SelectRotatingFrame);
    }

    public void OnEnable()
    {
        if (firstBuild)
        {
            bodyList = VizardGUISettings.CreateBodyListForDropdown(Body1Dropdown, "RotatingFrameBody1", false, true, false);
            VizardGUISettings.PopulateList(Body2Dropdown, bodyList);
            firstBuild = false;
        }
        errorText.text = "";
        transform.SetAsLastSibling();
        ChangeDropdownChoice();
    }

    private void ChangeDropdownChoice()
    {
        Body1Dropdown.value = VizardGUISettings.RotatingFrameBody1Index + 1;
        Body2Dropdown.value = VizardGUISettings.RotatingFrameBody2Index + 1;
    }

    private void SelectRotatingFrame()
    {
        int currentBody1Value = Body1Dropdown.value;
        int currentBody2Value = Body2Dropdown.value;

        if (currentBody1Value == 0)
        {
            errorText.text = "Please select a body in the Body 1 Dropdown.";
        }else if (currentBody2Value == 0)
        {
            errorText.text = "Please select a body in the Body 2 Dropdown.";
        } else if (currentBody1Value == currentBody2Value)
        {
            errorText.text = "Please select two different bodies in the dropdowns.";
        }else 
        {
            VizardGUISettings.RotatingFrameBody1Index = currentBody1Value - 1;
            VizardGUISettings.RotatingFrameBody2Index = currentBody2Value - 1;
            CelestialBodyStateUtilities.CalculateRotatingFramePositionAndVelocityHistories();
            VizardGUISettings.TruePathLineMode = 4;
            VizardGUISettings.TruePathLinesVisible = true;
            VizardGUISettings.RelativeTruePathChangeCount++;
            gameObject.SetActive(false);
        } 
    }
}
