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
/// <summary>
/// Shows or hides the sub-toggles in a panel
/// <remarks>Used to allow granular selection parent spacecraft or effectors
/// for a given spacecraft on HUD or panel menus</remarks>
/// </summary>
public class ShowHideSubToggles : MonoBehaviour
{
    public List<GameObject> mySubToggles = new List<GameObject>();
    public Button myButton;
    public Toggle masterToggle;
    public Transform inventoryContentTransform;
    public bool subTogglesAreShowing;

    void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(ShowHideMyToggles);
    }
    
    void ShowHideMyToggles()
    {
        foreach (GameObject toggle in mySubToggles)
        {
            toggle.SetActive(!subTogglesAreShowing);
        }

        subTogglesAreShowing = !subTogglesAreShowing;
        ReorganizeTogglesInContentInventory();
    }

    void ReorganizeTogglesInContentInventory()
    {
        int yPosition = 0;
        int count = 0;
        foreach (Transform child in inventoryContentTransform)
        {
            if (child.gameObject.activeSelf)
            {
                Vector2 oldTransform = child.GetComponent<RectTransform>().anchoredPosition;
                child.GetComponent<RectTransform>().anchoredPosition = new Vector2(oldTransform.x, yPosition);
                yPosition -= 20;
                count++;
            }
        }
        Vector2 oldSize = inventoryContentTransform.gameObject.GetComponent<RectTransform>().sizeDelta;
		
        inventoryContentTransform.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(oldSize.x, count * 20);
    }

    public void SetMasterToggle(GameObject toggle)
    {
        masterToggle = toggle.GetComponent<Toggle>();
        inventoryContentTransform = toggle.transform.parent;
        
        masterToggle.onValueChanged.AddListener(ToggleAllSubToggles);
    }

    private void ToggleAllSubToggles(bool turnOn)
    {
        foreach (GameObject toggle in mySubToggles)
        {
            if (toggle.GetComponentInChildren<PanelToggle>().enabled)
            {
                toggle.GetComponentInChildren<PanelToggle>().TogglePanel(turnOn);
                toggle.GetComponentInChildren<Toggle>().SetIsOnWithoutNotify(turnOn);
            }
            else
            {
                toggle.GetComponentInChildren<Toggle>().isOn = turnOn;
            }
            
        }
    }

    public void AddSubToggle(GameObject newToggle)
    {
        mySubToggles.Add(newToggle);
        newToggle.GetComponent<RectTransform>().anchoredPosition = new Vector2(20, 0);
        int parentToggleIndex = masterToggle.transform.GetSiblingIndex();
        foreach (GameObject toggle in mySubToggles)
        {
            parentToggleIndex++;
            toggle.transform.SetSiblingIndex(parentToggleIndex+1);
        }
        newToggle.SetActive(false);
        ReorganizeTogglesInContentInventory();
        CheckAllSubTogglesOn();
    }

    private void CheckAllSubTogglesOn()
    {
        bool allTogglesOn = true;
        foreach (GameObject subToggle in mySubToggles)
        {
            if (!subToggle.GetComponentInChildren<Toggle>().isOn)
            {
                allTogglesOn = false;
                break;
            }
        }
        masterToggle.SetIsOnWithoutNotify(allTogglesOn);
    }
}
