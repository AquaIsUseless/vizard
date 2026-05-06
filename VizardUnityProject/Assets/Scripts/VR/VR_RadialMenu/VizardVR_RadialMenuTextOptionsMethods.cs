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
using TMPro;

/// <summary>
/// Sets up radial menu for a given list of string options
/// <remarks>Inherits from VizardVR_RadialMenuMethods to set radial sections,
/// select current section, and activate section on user input.</remarks>
/// </summary>
public class VizardVR_RadialMenuTextOptionsMethods : VizardVR_RadialMenuMethods
{
    [Header("Scene Objects")] [Tooltip("Holds all labels for this menu")]
    public Transform LabelsFolderTransform;

    [Tooltip("Menu input manager for all radial menus")]
    public VizardVR_RadialMenuInputManager vizardVRRadialMenuInput;

    [Tooltip("True if this is a root main menu")]
    public bool isMainMenu;

    /// <summary>
    /// For a given list of string options, set up the radial menu
    /// sections.
    /// </summary>
    /// <param name="options">List of string options to be provided in menu</param>
    /// <param name="reinitializeSections">True if rebuilding menu to show additional options</param>
    public void InitializeRadialMenuSections(List<string> options, bool reinitializeSections = false)
    {
        numberOfSections = options.Count;
        //Remove old option sections to allow a clean rebuild of the menu
        if (reinitializeSections)
        {
            radialSections.Clear();
            int count = LabelsFolderTransform.childCount;
            for (int i = count; i > 1; i--) //Leave the "back" button alone
            {
                //Destroy all labels (except back button)
                Destroy(LabelsFolderTransform.GetChild(i - 1).gameObject);
            }

            count = transform.childCount;
            for (int i = count; i > 0; i--)
            {
                //Destroy all the section divider "donut_lines"
                if (transform.GetChild(i - 1).gameObject.name == "donut_line(Clone)")
                {
                    Destroy(transform.GetChild(i - 1).gameObject);
                }
                else
                {
                    break;
                }
            }
        }

        //If this is not a main menu, include a "Back" button to allow return to root menu 
        if (!isMainMenu)
        {
            VizardVR_RadialSection backButton = new VizardVR_RadialSection();
            backButton.name = "Back";
            backButton.onPress.AddListener(vizardVRRadialMenuInput.GoToPrevMenu);
            backButton.icon = LabelsFolderTransform.GetChild(0).gameObject;
            radialSections.Add(backButton);
        }

        //Create a text label radial section for each option
        foreach (string op in options)
        {
            GameObject labelGameObject =
                Instantiate(Resources.Load("Prefabs/VR/VizardVR_RadialSectionTextLabel") as GameObject,
                    LabelsFolderTransform);
            labelGameObject.name = op;
            labelGameObject.GetComponentInChildren<TextMeshProUGUI>().text = op;
            VizardVR_RadialSection newOption = new VizardVR_RadialSection();
            newOption.name = op;
            newOption.icon = labelGameObject;
            newOption.onPress.AddListener(() => OnOptionPress(op));
            labelGameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
            radialSections.Add(newOption);
        }

        //Call the parent class to set up the radial menu for the created radial sections
        CreateAndSetupRadialSectionsDynamically();
    }

    /// <summary>
    /// For a given dictionary of string options and their string captions, set up the radial menu
    /// sections.
    /// </summary>
    /// <param name="optionNamesAndCaptions">Dictionary of string options and their captions to be provided in menu</param>
    public void InitializeRadialMenuSections(Dictionary<string, string> optionNamesAndCaptions)
    {
        numberOfSections = optionNamesAndCaptions.Count;
        //Include a "back" button to return to root main menu if this menu is not a main menu
        if (!isMainMenu)
        {
            VizardVR_RadialSection backButton = new VizardVR_RadialSection();
            backButton.name = "Back";
            backButton.onPress.AddListener(vizardVRRadialMenuInput.GoToPrevMenu);
            backButton.icon = LabelsFolderTransform.GetChild(0).gameObject;
            radialSections.Add(backButton);
        }

        //Create a text label radial section for each option
        foreach (string op in optionNamesAndCaptions.Keys)
        {
            GameObject labelGameObject =
                Instantiate(Resources.Load("Prefabs/VR/VizardVR_RadialSectionTextLabel") as GameObject,
                    LabelsFolderTransform);
            labelGameObject.name = op;
            labelGameObject.GetComponentInChildren<TextMeshProUGUI>().text = op;
            VizardVR_RadialSection newOption = new VizardVR_RadialSection();
            newOption.name = optionNamesAndCaptions[op];
            newOption.icon = labelGameObject;
            newOption.onPress.AddListener(() => OnOptionPress(op));
            labelGameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
            radialSections.Add(newOption);
        }
        
        //Call the parent class to set up the radial menu for the created radial sections
        CreateAndSetupRadialSectionsDynamically();
    }

    /// <summary>
    /// Broadcast message announcing the option the user has selected
    /// Menu-specific script will receive the option and take action
    /// </summary>
    /// <param name="option">User selected section's option</param>
    public void OnOptionPress(string option)
    {
        BroadcastMessage("RadialSectionInvoked", option, SendMessageOptions.DontRequireReceiver);
    }
}