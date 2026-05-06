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
using UnityEngine.UI;
using TMPro;

/// <summary>
/// VR: Used to pop up the confirmation panel for a user selected event option
/// </summary>
public class VizardVR_ConfirmationOnly : MonoBehaviour
{
    private string confirmEventString; //Choice to confirm

    [Header("Confirmation Panel Components")]
    public Button confirmButton;    //If pressed, user has confirmed choice
    public Button cancelButton;     //If pressed, user has canceled choice
    public TextMeshProUGUI dialogText; //UI Text to display query for confirmation
    
    /// <summary>
    /// Monodevelop method called before first update
    /// <remarks>Here used to add listeners to the confirm and cancel buttons</remarks>
    /// </summary>
    void Start()
    {
        confirmButton.onClick.AddListener(SendConfirmEventReply);
        cancelButton.onClick.AddListener(CloseConfirmationPanel);
    }

    /// <summary>
    /// Sends confirmation of user choice to the VizInputUtilities to be communicated to live Basilisk scenario (if applicable)
    /// </summary>
    void SendConfirmEventReply()
    {
        VizInputUtilities.AddEventHandlerIDOnlyReply(confirmEventString);
        this.gameObject.SetActive(false);
    }

    /// <summary>
    /// Closes the confirmation panel if user selects Cancel button
    /// </summary>
    void CloseConfirmationPanel()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Updates the text of the confirmation panel to reflect the option the user is being requested to confirm or cancel.
    /// </summary>
    /// <param name="eventReplyString">choice to display for confirmation</param>
    public void ShowEventToConfirm(string eventReplyString)
    {
        confirmEventString = eventReplyString;
        dialogText.text = $"Please confirm your choice to {confirmEventString}";
    }
}