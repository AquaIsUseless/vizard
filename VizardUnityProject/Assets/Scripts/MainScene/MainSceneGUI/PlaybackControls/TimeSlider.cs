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

using UnityEngine.UI;
using UnityEngine;
/// <summary>
/// Handles user input to the TimeSlider in the PlaybackControlsBar
/// </summary>
public class TimeSlider : MonoBehaviour {
	private Slider slider;
	UISelectHandler handleSelect;

	[Header("Required Reference to GameController")]
	public ItsAboutTime timeController;

	void Start(){
		slider = GetComponent<Slider> ();
		handleSelect = slider.GetComponentInChildren<UISelectHandler> ();
	}
		
	// Update is called once per frame
	void Update () {
		if (handleSelect.IsSelected) {
				float timeValue = slider.value;
				timeController.SetArchiveFraction (timeValue);
		} else {
				slider.value = timeController.GetArchiveFraction ();
		}
	}
}
