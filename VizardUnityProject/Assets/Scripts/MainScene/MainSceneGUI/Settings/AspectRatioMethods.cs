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
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// Supports the Aspect Ratio panel (under the View menu)
/// Allows the user to set a desired aspect ratio and will
/// force the resized Vizard window's width to meet the desired aspect
/// ratio for the window's height.
/// </summary>
public class AspectRatioMethods : UIBehaviour
{
    [Header("Aspect Ratio Option Toggles")]
    public Toggle oneToOne;         // Toggle the 1:1 Aspect Ratio
    public Toggle fourToThree;      // Toggle the 4:3 Aspect Ratio   
    public Toggle sixteenToNine;    // Toggle the 16:9 Aspect Ratio
    public Toggle freeAspect;       // Toggle to allow any aspect ratio (default)

    private float widthToHeightRatio; // Desired width to height ratio for a given aspect ratio setting
    private bool forceAspectRatio;    // True if the window should be forced to a user chosen aspect ratio
    private DateTime lastDimUpdateApplied;  //Last clock time the window resize values were applied 

    /// <summary>
    /// Start is called once before the first execution of Update after the MonoBehaviour is created
    /// <remarks>Add listeners to UI Toggles</remarks>
    /// </summary>
    protected override void Start()
    {
        base.Start();
        oneToOne.onValueChanged.AddListener(ToggleOneToOneMode);
        fourToThree.onValueChanged.AddListener(ToggleFourToThreeMode);
        sixteenToNine.onValueChanged.AddListener(ToggleSixteenToNineMode);
        freeAspect.onValueChanged.AddListener(ToggleFreeAspect);
        
        lastDimUpdateApplied = DateTime.Now;
    }
    
    /// <summary>
    /// Event override for UIBehavior.OnRectTransformDimensionsChange()
    /// <remarks>Calls ForceAspectRatio on window resize</remarks>
    /// </summary>
    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        ForceAspectRatio();
    }
    /// <summary>
    /// Forces the width of the Vizard window to the desired aspect ratio for the Vizard window's height
    /// </summary>
    private void ForceAspectRatio()
    {
        if ((DateTime.Now - lastDimUpdateApplied).TotalSeconds > 0.2)
        {
            if (!Screen.fullScreen) //Do not resize if the Screen is in fullscreen mode
            {
                if (forceAspectRatio) //Calculate new width if user has chosen a specific aspect ratio
                {
                    Screen.SetResolution(Mathf.RoundToInt(widthToHeightRatio * Screen.height), Screen.height, false);
                }
            }
            else //If in full screen mode, turn off forceAspectRatio
            {
                forceAspectRatio = false; 
            }

            lastDimUpdateApplied = DateTime.Now;
        }
    }
    
/// <summary>
/// Force Vizard window aspect ratio to 1:1 scale
/// </summary>
/// <param name="isOn">True if this option is selected</param>
    private void ToggleOneToOneMode(bool isOn)
    {
        if (isOn)
        {
            forceAspectRatio = true;
            widthToHeightRatio = 1f;
            ForceAspectRatio();
        }
    }
    /// <summary>
    /// Force Vizard window aspect ratio to 4:3 scale
    /// </summary>
    /// <param name="isOn">True if this option is selected</param>
    private void ToggleFourToThreeMode(bool isOn)
    {
        if (isOn)
        {
            forceAspectRatio = true;
            widthToHeightRatio = (4f/3f);
            ForceAspectRatio();
        }
    }
    /// <summary>
    /// Force Vizard window aspect ratio to 16:9 scale
    /// </summary>
    /// <param name="isOn">True if this option is selected</param>
    private void ToggleSixteenToNineMode(bool isOn)
    {
        if (isOn)
        {
            forceAspectRatio = true;
            widthToHeightRatio = (16f/9f);
            ForceAspectRatio();
        }
    }
    /// <summary>
    /// Allow Vizard window to be resized to any dimensions by user
    /// </summary>
    /// <param name="isOn">True if this option is selected</param>
    private void ToggleFreeAspect(bool isOn)
    {
        if (isOn)
        {
            forceAspectRatio = false;
        }
    }
}
