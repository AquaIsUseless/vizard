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
using TMPro;
using UnityEngine;
/// <summary>
/// Sets up and handles user input to a sub-dropdown
/// (used mainly to access effectors for body selections)
/// </summary>
public class SubDropdown : MonoBehaviour
{
    private TMP_Dropdown myDropdown;

    private string parentOptionName;//Name of the main dropdown option that is the parent to this dropdown

    private string mainDropdownName; // Needed if the GUI panel contains more than one dropdown
    
    private bool pointerHasExited;
    private float timeDownMark = 3000000f;
    public float timeToWait = 0.8f;
    void Update()
    {
        if (pointerHasExited)
        {
            if (Time.time - timeDownMark > timeToWait)
            {
                timeDownMark = Time.time+3000f;
                pointerHasExited = false;
                this.gameObject.SetActive(false);
            }
        }
    }

    void OnEnable()
    {
        transform.SetAsLastSibling();
       
    }

    private void OnGUI()
    {
        myDropdown.SetValueWithoutNotify(0);
    }

    public void InitializeSubDropdown(string optionParent, string mainDropdownID)
    {
        myDropdown = GetComponent<TMP_Dropdown>();
        myDropdown.onValueChanged.AddListener(SendOptionTextToDropdownOwner);
        // HoverDetector hD1 = myDropdown.transform.gameObject.AddComponent<HoverDetector>();
        // hD1.SetSubDropdown(this);
        parentOptionName = optionParent;
        mainDropdownName = mainDropdownID;
        GameObject itemTemplate = transform.GetChild(2).transform.GetChild(0).GetChild(0).GetChild(0).gameObject;
        HoverDetector hD2 = itemTemplate.AddComponent<HoverDetector>();
        hD2.SetSubDropdown(this);
        
    }
    public void SendOptionTextToDropdownOwner(int option)
    {
        if (option != 0)
        {
            string optionString = myDropdown.options[option].text;
            string[] dropdownData = new string[] {mainDropdownName, parentOptionName, optionString};
            gameObject.SendMessageUpwards("SubDropdownValueSelected", dropdownData,SendMessageOptions.DontRequireReceiver);
            transform.gameObject.SetActive(false);
        }
    }

    public void HoveringOverThisItem(Transform dropdownItem)
    {
        pointerHasExited = false;
        timeDownMark = Time.time + 3000f;
    }

    public void ExitedThisItem()
    {
        pointerHasExited = true;
        timeDownMark = Time.time;
    }
    

}
