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
/// Create radial sections for the provided menu options
/// and handle menu option selections by user
/// </summary>
public class VizardVR_RadialMenuMethods : MonoBehaviour
{
    [Header("Scene")] [Tooltip("Highlights Radial Section in focus")]
    public GameObject highlightSection;

    [Header("Events")] public List<VizardVR_RadialSection> radialSections = null;
    public TextMeshProUGUI label;

    private float menuDistanceLimit = 150f; //Radius of radial menu on canvas (cursor positions outside this radius will not activate radial buttons)

    private bool enableSelectedAction;
    
    protected Vector2 cursorPosition = Vector2.zero;    //Current position of cursor (thumbstick or laser pointer)
    protected VizardVR_RadialSection sectionInFocus;    //Current radial section in user focus

    protected int numberOfSections; // Number of menu options (sections) in the radial menu
    protected float degreeIncrement; // [degrees] Degrees per section
    protected float offset = -45.0f; //offset since image starts at top center and proceeds clockwise
    protected float iconRadius = 1.2f; //Desired radius of section icon 
    protected float sectionDivisionLineScale = 3.5f; //Desired length of section division line

    protected bool sectionsComplete; //True if the sections have been created and initialized for the attached menu

    public VizardVR_RadialMenuMethods
        prevMenuMethods; //Parent menu that should be returned to after selection or cancel

    /// <summary>
    /// Used if the radial sections are created at runtime
    /// instead of being a static part of the menu
    /// </summary>
    public void InitializeRadialSectionsDynamically()
    {
        CreateAndSetupRadialSectionsDynamically();
        //No option should be highlighted at start
        ShowHighlightSection(false);
    }

    /// <summary>
    /// Used to create the radial sections at runtime.
    /// Calculates how many sections are required for the menu's options,
    /// how big each section will be (degrees), and adds the section divider lines.
    /// </summary>
    protected void CreateAndSetupRadialSectionsDynamically()
    {
        numberOfSections = radialSections.Count;
        if (numberOfSections != 0)
        {
            degreeIncrement = 360.0f / numberOfSections;
            highlightSection.GetComponent<Image>().fillAmount = 1.0f / numberOfSections;
            offset = -degreeIncrement / 2;
            highlightSection.GetComponent<Transform>().localEulerAngles = new Vector3(0, 0, offset);
            int i = 0;
            foreach (VizardVR_RadialSection section in radialSections)
            {
                section.icon.transform.localPosition = new Vector3(
                    iconRadius * Mathf.Sin(degreeIncrement * i * Mathf.PI / 180),
                    iconRadius * Mathf.Cos(degreeIncrement * i * Mathf.PI / 180), 0);
                GameObject line =
                    Instantiate(Resources.Load("Sprites/GUIIcons/Menu/donut_line", typeof(GameObject))) as GameObject;
                line.transform.SetParent(this.transform);
                line.transform.localPosition = new Vector3(0, 0, 0);
                line.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, (offset + degreeIncrement * i)));
                line.transform.localScale = new Vector3(1, 1, 1);
                line.GetComponent<RectTransform>().sizeDelta =
                    new Vector2(sectionDivisionLineScale, sectionDivisionLineScale);
                i += 1;
            }
        }

        sectionsComplete = true;
    }

    /// <summary>
    /// Enable or disable the radial menu and update
    /// VizardGUISettings.isVRMenuActive
    /// </summary>
    /// <param name="isEnabled">True if the radial menu is enabled</param>
    public void EnableRadialMenu(bool isEnabled)
    {
        gameObject.SetActive(isEnabled);
        VizardGUISettings.SetVRRadialMenuActive(isEnabled);
    }

    /// <summary>
    /// Monodevelop method called on every frame
    /// <remarks>Update the sim elapsed time display and the current playback rate</remarks>
    /// </summary>
    void FixedUpdate()
    {
        if (sectionsComplete)
        {
            float cursorAngle = GetDegree();

            //If the cursor is over the radial menu circle allow selections
            //otherwise, do not allow option selection (prevents accidental selections)
            if (cursorPosition.magnitude < menuDistanceLimit)
            {
                ShowHighlightSection(true);
                SetSectionAtCursor(cursorAngle);
                SetInFocusSection(cursorAngle);
                enableSelectedAction = true;
            }
            else
            {
                ShowHighlightSection(false);
                enableSelectedAction = false;
            }
        }
    }

    /// <summary>
    /// Current angle of the cursor to the center of the radial menu
    /// </summary>
    /// <param name="cursorPosition">Current position of cursor relative to center of radial menu</param>
    /// <returns></returns>
    private float GetDegree()
    {
        float value = Mathf.Atan2(cursorPosition.x, cursorPosition.y);
        value = Mathf.Rad2Deg * value;
        if (value < 0)
        {
            value += 360;
        }

        return value;
    }

    /// <summary>
    /// Sets where the user input is placing the cursor relative to the
    /// center of the radial menu
    /// </summary>
    /// <param name="position">Current position of the cursor</param>
    public void SetCursorPositionOnRadialMenu(Vector2 position)
    {
        if (Mathf.Abs(position.x) > .2 | Mathf.Abs(position.y) > .2)
        {
            cursorPosition = position;
        }
    }

    /// <summary>
    /// Set the rotation of the selection transform 
    /// </summary>
    /// <param name="newRotation">Angle of cursor location relative to center of radial circle</param>
    private void SetSectionAtCursor(float newRotation)
    {
        float snappedRotation = SnapRotation(newRotation);
        highlightSection.GetComponent<Transform>().localEulerAngles = new Vector3(0, 0, -snappedRotation);
    }

    /// <summary>
    /// Get the closest section's rotation for a given angle plus the half section offset
    /// </summary>
    /// <param name="rotation">Angle of cursor location relative to center of radial circle</param>
    /// <returns></returns>
    private float SnapRotation(float rotation)
    {
        return GetNearestIncrement(rotation) * degreeIncrement + offset;
    }

    /// <summary>
    /// Get the closest section's rotation for a given angle
    /// </summary>
    /// <param name="rotation"></param>
    /// <returns></returns>
    private int GetNearestIncrement(float rotation)
    {
        return Mathf.RoundToInt(rotation / degreeIncrement);
    }

    /// <summary>
    /// Set the radial section currently in the user's focus and show its option text in the label
    /// </summary>
    /// <param name="currentRotation">Angle of the cursor relative to the center of the radial menu</param>
    private void SetInFocusSection(float currentRotation)
    {
        int index = GetNearestIncrement(currentRotation);
        if (index == numberOfSections)
        {
            index = 0;
        }

        sectionInFocus = radialSections[index];
        label.text = sectionInFocus.name;
    }

    /// <summary>
    /// Activate the in-focus section's option
    /// </summary>
    public void ActivateInFocusSection()
    {
        if (enableSelectedAction)
        {
            sectionInFocus.onPress.Invoke();
        }
    }

    /// <summary>
    /// Show the blue highlight section over the current option in the user's focus
    /// </summary>
    /// <param name="isOn">True if the highlight section should be visible</param>
    private void ShowHighlightSection(bool isOn)
    {
        highlightSection.SetActive(isOn);
    }
}