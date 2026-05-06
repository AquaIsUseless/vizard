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

#if VIZARD_OPENXR
using UnityEngine.InputSystem;
#endif
using UnityEngine;

/// <summary>
/// Takes VR controller inputs and maps them to radial menu actions
/// </summary>
public class VizardVR_RadialMenuInputManager : MonoBehaviour
{
    [Header("VR Radial Menu Methods")] [Tooltip("Current Main Radial Menu")]
    public VizardVR_RadialMenuMethods mainMenuMethods; //Current main menu (left or right)

    [Tooltip("Left Controller Main Radial Menu")]
    public VizardVR_RadialMenuMethods vizardLeftMainMenuMethods; //Main menu activated by left hand controller

    [Tooltip("Right Controller Main Radial Menu")]
    public VizardVR_RadialMenuMethods vizardRightMainMenuMethods; //Main menu activated by right hand controller

    [Tooltip("Current Active Radial Menu")]
    public VizardVR_RadialMenuMethods activeMenuMethods; //Currently active menu's methods

#if VIZARD_OPENXR
    private InputActionAsset vizardInputActionAsset; //Input action map for Vizard

    // Input actions required for input to radial menus
    private InputAction rightPrimaryButton;
    private InputAction rightSecondaryButton;
    private InputAction rightThumbstick;
    private InputAction rightTrigger;
    private InputAction rightGripPress;
    private InputAction leftPrimaryButton;
    private InputAction leftSecondaryButton;
    private InputAction leftThumbstick;
    private InputAction leftTrigger;
    private InputAction leftGripPress;

#endif

    private Transform leftEndMarker; //Left controller forward marker
    private Transform rightEndMarker; //Right controller forward marker

    private RectTransform radialMenuRect; //Rect transform of the radial menu

    private Camera mainCamera; // Scene Main Camera 


    /// <summary>
    /// Monodevelop method called before any Update calls
    /// <remarks>Here used to populate references and add listeners for input actions.</remarks>
    /// </summary>
    void Start()
    {
        radialMenuRect = GetComponent<RectTransform>();
        mainCamera = MainCameraUtilities.MainCamera;

        //Set the active main menu methods to the right menu by default
        mainMenuMethods = vizardRightMainMenuMethods;

        //Set the active menu to be the main menu
        activeMenuMethods = mainMenuMethods;

#if VIZARD_OPENXR
        // Left and right markers are used to what user is pointing at in the radial menu
        leftEndMarker = mainCamera.GetComponent<VizardVR_MainCameraMovementController>().leftEndMarker;
        rightEndMarker = mainCamera.GetComponent<VizardVR_MainCameraMovementController>().rightEndMarker;

        vizardInputActionAsset = mainCamera.GetComponent<VizardVR_MainCameraMovementController>().inputActionAsset;
        leftPrimaryButton = vizardInputActionAsset.FindAction("LeftPrimaryButton");
        leftSecondaryButton = vizardInputActionAsset.FindAction("LeftSecondaryButton");
        leftThumbstick = vizardInputActionAsset.FindAction("LeftThumbstick");
        leftGripPress = vizardInputActionAsset.FindAction("LeftGripPress");
        leftTrigger = vizardInputActionAsset.FindAction("LeftTriggerPress");
        rightPrimaryButton = vizardInputActionAsset.FindAction("RightPrimaryButton");
        rightSecondaryButton = vizardInputActionAsset.FindAction("RightSecondaryButton");
        rightThumbstick = vizardInputActionAsset.FindAction("RightThumbstick");
        rightGripPress = vizardInputActionAsset.FindAction("RightGripPress");
        rightTrigger = vizardInputActionAsset.FindAction("RightTriggerPress");

        //Register the listeners for the applicable Input Actions
        leftGripPress.performed += ToggleLeftMenuModeOpenXR;
        rightGripPress.performed += ToggleRightMenuModeOpenXR;
        rightPrimaryButton.performed += PrimaryButton_OpenXR;
        rightSecondaryButton.performed += SecondaryButton_OpenXR;
        leftPrimaryButton.performed += PrimaryButton_OpenXR;
        leftSecondaryButton.performed += SecondaryButton_OpenXR;

#endif
    }

    /// <summary>
    /// Close the old active radial menu, set the new menu to be active,
    /// and show it onscreen.
    /// </summary>
    /// <param name="menuMethods">Desired menu to activate</param>
    public void SetActiveMenu(VizardVR_RadialMenuMethods menuMethods)
    {
        //lets the menu charge
        activeMenuMethods.EnableRadialMenu(false);
        activeMenuMethods = menuMethods;
        activeMenuMethods.EnableRadialMenu(true);
    }

    /// <summary>
    /// Activate the current main menu (left or right)
    /// </summary>
    public void ActivateMainMenu()
    {
        SetActiveMenu(mainMenuMethods);
    }

    /// <summary>
    /// Open a main radial menu (left for left press) if no radial menus are open
    /// If left main radial menu open and right press, show left radial menu
    /// If right main radial menu open and left press, show right radial menu
    /// </summary>
    /// <param name="isLeftPress">True if user input from left controller</param>
    public void ToggleMenuMode(bool isLeftPress = false)
    {
        if (!VizardGUISettings.GetVRMenuActive()) //no radial menu is currently active
        {
            //set up a main menu to be activated (as no menus are currently active)
            //left press will activate left main menu, right press the right main menu
            mainMenuMethods = isLeftPress ? vizardLeftMainMenuMethods : vizardRightMainMenuMethods;
            ActivateMainMenu();
        }
        else //there is a radial menu currently open
        {
            //if left press and right main menu is the current main menu, open left main menu
            if ((isLeftPress) && (mainMenuMethods == vizardRightMainMenuMethods))
            {
                SetActiveMenu(vizardLeftMainMenuMethods);
                return;
            }

            //if right press and left main menu is the current main menu, open right main menu
            if ((!isLeftPress) && (mainMenuMethods == vizardLeftMainMenuMethods))
            {
                SetActiveMenu(vizardRightMainMenuMethods);
                return;
            }

            // close the open radial menu
            activeMenuMethods.EnableRadialMenu(false);
        }
    }

    /// <summary>
    /// Activate the current menu's designated previous menu
    /// </summary>
    public void GoToPrevMenu()
    {
        VizardVR_RadialMenuMethods prevMenuMethods =
            activeMenuMethods.GetComponent<VizardVR_RadialMenuMethods>().prevMenuMethods;

        // if the open menu does not designate a previous menu, close open menu
        if (prevMenuMethods == null)
        {
            activeMenuMethods.EnableRadialMenu(false);
        }
        else //show the desired previous menu
        {
            SetActiveMenu(prevMenuMethods);
        }
    }
#if VIZARD_OPENXR
    /// <summary>
    /// Monodevelop method called at every frame
    /// <remarks>Read the current thumbstick and laser pointer values to
    /// determine which radial menu options should be active.</remarks>
    /// </summary>
    public void FixedUpdate()
    {
        //If a radial menu is active
        if (VizardGUISettings.GetVRMenuActive())
        {
            //Get the current position of the cursor relative to the center of the radial menu
            Vector2 cursorPosition = GetCursorPosition();

            //Pass that information to the active menu to set the currently highlighted radial section
            activeMenuMethods.SetCursorPositionOnRadialMenu(cursorPosition);
        }
    }

    /// <summary>
    /// Check for thumbstick and laser pointer (raycast) in progress
    /// and determine the position of the cursor relative to the center of
    /// the radial menu
    /// </summary>
    /// <returns>Current position of the cursor relative to the center of the radial menu</returns>
    private Vector2 GetCursorPosition()
    {
        Vector2 cursorPosition = Vector2.zero;
        //Check if a thumbstick is in use
        if (rightThumbstick.IsInProgress())
        {
            cursorPosition = rightThumbstick.ReadValue<Vector2>();
        }
        else if (leftThumbstick.IsInProgress())
        {
            cursorPosition = leftThumbstick.ReadValue<Vector2>();
        }

        //Check if a trigger button is being pressed, indicated laser pointer use
        if (rightTrigger.IsInProgress())
        {
            cursorPosition = CalculateEndMarkerPositionOnCanvas(true);
        }
        else if (leftTrigger.IsInProgress() && !rightTrigger.IsInProgress())
        {
            cursorPosition = CalculateEndMarkerPositionOnCanvas(false);
        }

        return cursorPosition;
    }

    /// <summary>
    /// Monodevelop method executed when object is destroyed
    /// <remarks>Deregister the input action listeners</remarks>
    /// </summary>
    private void OnDestroy()
    {
        leftGripPress.performed -= ToggleLeftMenuModeOpenXR;
        rightGripPress.performed -= ToggleRightMenuModeOpenXR;
        rightPrimaryButton.performed -= PrimaryButton_OpenXR;
        rightSecondaryButton.performed -= SecondaryButton_OpenXR;
        leftPrimaryButton.performed -= PrimaryButton_OpenXR;
        leftSecondaryButton.performed -= SecondaryButton_OpenXR;
    }

    /// <summary>
    /// Calculate the screen position of the controller marker
    /// </summary>
    /// <param name="useRightMarker">True if the right marker position is used</param>
    /// <returns>Vector2 position of the controller marker on the screen</returns>
    private Vector2 CalculateEndMarkerPositionOnCanvas(bool useRightMarker)
    {
        Vector2 localPoint;
        Vector2 screenPointOfMarker = useRightMarker
            ? RectTransformUtility.WorldToScreenPoint(mainCamera, rightEndMarker.position)
            : RectTransformUtility.WorldToScreenPoint(mainCamera, leftEndMarker.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(radialMenuRect, screenPointOfMarker, mainCamera,
            out localPoint);
        return localPoint;
    }

    /// <summary>
    /// Left controller press action to open/close radial menu
    /// </summary>
    /// <param name="value">Input Action press details</param>
    private void ToggleLeftMenuModeOpenXR(InputAction.CallbackContext value)
    {
        ToggleMenuMode(true);
    }

    /// <summary>
    /// Right controller press action to open/close radial menu
    /// </summary>
    /// <param name="value">Input Action press details</param>
    private void ToggleRightMenuModeOpenXR(InputAction.CallbackContext value)
    {
        ToggleMenuMode(false);
    }

    /// <summary>
    /// Controller secondary button press action to return to previous menu
    /// </summary>
    /// <param name="value">Input Action press details</param>
    private void SecondaryButton_OpenXR(InputAction.CallbackContext value)
    {
        if (VizardGUISettings.GetVRMenuActive())
        {
            GoToPrevMenu();
        }
    }

    /// <summary>
    /// Controller primary button press action to activate currently
    /// highlighted radial section of current menu
    /// </summary>
    /// <param name="value">Input Action press details</param>
    private void PrimaryButton_OpenXR(InputAction.CallbackContext value)
    {
        if (VizardGUISettings.GetVRMenuActive())
        {
            activeMenuMethods.ActivateInFocusSection();
        }
    }

#endif
}