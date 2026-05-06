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
/// Detects user cursor hovering over dropdown and
/// sets up and handles input to any sub-dropdowns
/// </summary>
public class HoverDropdown : MonoBehaviour
{
    private Dictionary<int, GameObject> subDropdownsDictionary = new Dictionary<int, GameObject>();
    private GameObject openSubMenu;
    private TMP_Dropdown myDropdown;
    public Sprite arrowSprite;
    public Image captionImage;

    void Start()
    {
        myDropdown.onValueChanged.AddListener(RemoveSpriteArrow);
        GameObject itemTemplate = transform.GetChild(2).transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        HoverDetector hd = itemTemplate.AddComponent<HoverDetector>();
        hd.SetHoverDropdown(this);
    }

    void OnEnable()
    {
        myDropdown = GetComponent<TMP_Dropdown>();
    }

    void Update()
    {
        captionImage.enabled = false;
    }

    public void HoveringOverThisItem(Transform dropdownItem)
    {
        int optionValue = dropdownItem.GetSiblingIndex();
        if (subDropdownsDictionary.ContainsKey(optionValue))
        {
            openSubMenu = subDropdownsDictionary[optionValue];
            Vector2 parentDropdownDims = this.GetComponent<RectTransform>().sizeDelta;
            this.transform.SetAsLastSibling();
            openSubMenu.GetComponent<RectTransform>().anchoredPosition =
                new Vector2(parentDropdownDims.x, -20 * (optionValue) - 4);
            openSubMenu.SetActive(true);

            gameObject.SendMessageUpwards("SetOpenSubMenu", openSubMenu);
        }
        else
        {
            if (openSubMenu != null)
            {
                openSubMenu.SetActive(false);
            }
        }
    }

    public GameObject AddSubDropdownMenu(int optionValue, List<string> optionNames, string parentOptionName,
        string mainDropdownID)
    {
        GameObject newDropdown = Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericSubDropdown") as GameObject,
            transform);
        newDropdown.name = parentOptionName + "EffectorsDropdown";
        SubDropdown subDropdown = newDropdown.AddComponent<SubDropdown>();
        subDropdown.InitializeSubDropdown(parentOptionName, mainDropdownID);

        VizardGUISettings.PopulateList(newDropdown.GetComponent<TMP_Dropdown>(), optionNames);
        if (subDropdownsDictionary.ContainsKey(optionValue))
        {
            subDropdownsDictionary.Remove(optionValue);
        }

        subDropdownsDictionary.Add(optionValue, newDropdown);
        return newDropdown;
    }

    private void RemoveSpriteArrow(int optionValue)
    {
        captionImage.enabled = false;
    }

    public bool SetForOptionWithDropdownLockout(string optionName)
    {
        if (myDropdown == null)
        {
            myDropdown = GetComponent<TMP_Dropdown>();
        }

        for (int i = 1; i < myDropdown.options.Count; i++)
        {
            string mainEntryString = myDropdown.options[i].text;
            if (mainEntryString == optionName)
            {
                myDropdown.value = i;

                return true;
            }

            if (subDropdownsDictionary.ContainsKey(i))
            {
                TMP_Dropdown subDropdown = subDropdownsDictionary[i].GetComponent<TMP_Dropdown>();
                for (int j = 1; j < subDropdown.options.Count; j++)
                {
                    if (subDropdown.options[j].text == optionName)
                    {
                        subDropdown.GetComponent<SubDropdown>().SendOptionTextToDropdownOwner(j);
                        return true;
                    }
                }
            }
        }

        myDropdown.value = 0;
        return false;
    }

    public bool SetOptionFromMessages(string optionName)
    {
        for (int i = 1; i < myDropdown.options.Count; i++)
        {
            string mainEntryString = myDropdown.options[i].text;
            if (mainEntryString == optionName)
            {
                myDropdown.value = i;
                return true;
            }

            if (subDropdownsDictionary.ContainsKey(i))
            {
                TMP_Dropdown subDropdown = subDropdownsDictionary[i].GetComponent<TMP_Dropdown>();
                for (int j = 1; j < subDropdown.options.Count; j++)
                {
                    if (subDropdown.options[j].text == optionName)
                    {
                        subDropdown.GetComponent<SubDropdown>().SendOptionTextToDropdownOwner(j);
                        return true;
                    }
                }
            }
        }

        myDropdown.value = 0;
        return false;
    }
}