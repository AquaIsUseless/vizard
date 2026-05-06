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

public class VizardVR_RadialOptionIndicator : MonoBehaviour
{
    public Transform indicatorHolder;
    private bool isToggleGroup;
    private float menuRadius=1.8f;
    private Dictionary<string, GameObject> optionIndicators;

    public void CreateOptionIndicators(List<string> options,bool hasBackButton, bool setAsToggleGroup)
    {
        optionIndicators = new Dictionary<string, GameObject>();
        isToggleGroup = setAsToggleGroup;
        float degreeIncrement;
        int optionsCount = options.Count;
        if (options.Count > 0)
        {
            int offsetBackButton = 0;
            if (hasBackButton)
            {
                optionsCount += 1;
                offsetBackButton = 1;
                AddIndicator("back", 0);
            }
            degreeIncrement = 360.0f / optionsCount;
            for (int i = 0 ; i < options.Count; i++)
            {
                AddIndicator(options[i], degreeIncrement*(i+offsetBackButton));
            }
        }
        ToggleAllIndicators(false);
    }

    private void AddIndicator(string option, float rotationAngle)
    {
        GameObject indicator =
            Instantiate(Resources.Load("Prefabs/VR/VizardVR_RadialOptionIndicator") as GameObject);
        indicator.name = option + "Indicator";
        indicator.transform.SetParent(indicatorHolder);
        indicator.transform.localEulerAngles = Vector3.zero;
        indicator.transform.localPosition =
            new Vector3(menuRadius * Mathf.Sin(rotationAngle * Mathf.PI / 180),
                menuRadius * Mathf.Cos(rotationAngle * Mathf.PI / 180), 0);
        optionIndicators[option] = indicator;
    }

    public void ToggleIndicator(string option)
    {
        if (isToggleGroup)
        {
            ToggleAllIndicators(false);
        }

        GameObject indicator = optionIndicators[option];
        indicator.SetActive(!indicator.activeSelf);
    }

    public void ToggleAllIndicators(bool isOn)
    {
        for (int i = 0; i < indicatorHolder.childCount; i++)
        {
            indicatorHolder.GetChild(i).gameObject.SetActive(isOn);
        }
    }

    public void SetIndicatorActive(string option, bool isOn)
    {
        optionIndicators[option].SetActive(isOn);
    }
    
}
