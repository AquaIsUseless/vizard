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
/// <summary>
/// Methods called by buttons in the Vizard Main Scene File Submenu
/// </summary>
public class FileMenuMethods : MonoBehaviour
{
	public Button quitVizardButton;		//UI Button to Quit Vizard instance
	public GameObject quitConfirmation; //Confirmation panel for quitting Vizard instance

	/// <summary>
	/// Monodevelop method called before first update
	/// <remarks>Add Show Confirmation Panel Listener to Quit Vizard button</remarks>
	/// </summary>
    void Start()
    {
		quitVizardButton.onClick.AddListener(ShowQuitConfirmationPanel);
    }
    
    /// <summary>
    /// Show the Quit Vizard Confirmation Panel
    /// </summary>
	public void ShowQuitConfirmationPanel(){
		quitConfirmation.SetActive(true);
	}

    /// <summary>
    /// Hide the Quit Vizard Confirmation Panel if user selects Cancel option
    /// </summary>
	public void CancelQuit(){
		quitConfirmation.SetActive(false);
	}
/// <summary>
/// Quit the Vizard application if the user selects Confirm option
/// </summary>
	public void ConfirmQuit(){
		Application.Quit();
	}

}
