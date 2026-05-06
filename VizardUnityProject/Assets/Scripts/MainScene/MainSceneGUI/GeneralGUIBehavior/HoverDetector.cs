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
using UnityEngine;
using UnityEngine.EventSystems;
/// <summary>
/// Detects cursor over hover dropdown or sub-dropdown
/// </summary>
public class HoverDetector : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public HoverDropdown hoverDropdown;
    public SubDropdown subDropdown;
    
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (hoverDropdown != null)
        {
            hoverDropdown.HoveringOverThisItem(transform);
        }else if (subDropdown != null)
        {
            subDropdown.HoveringOverThisItem(transform);
        }
        else
        {
            Debug.LogError("No receiver set for HoverDetector");
        }
    }
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (subDropdown != null)
        {
            subDropdown.ExitedThisItem();
        }
    }

    public void SetHoverDropdown(HoverDropdown dropdownToUse)
    {
        hoverDropdown = dropdownToUse;
    }
    
    public void SetSubDropdown(SubDropdown dropdownToUse)
    {
        subDropdown = dropdownToUse;
    }
}
