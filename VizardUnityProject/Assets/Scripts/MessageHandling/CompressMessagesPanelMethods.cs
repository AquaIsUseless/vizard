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
/// Handles user input to the File>Compress Messages Panel
/// that is available to reduce total message count in
/// a live-streamed scenario
/// </summary>
public class CompressMessagesPanelMethods : MonoBehaviour
{
	[Header("Panel GUI")]
	public InputField numerator;
	public InputField denominator;
	public Button ApplyButton;
	public Button CancelButton;
	public TextMeshProUGUI errorText;
	private float percentDiscard = 0.5f;
	private bool correctInput = true;

    // Start is called before the first frame update
    void Start()
    {
		ApplyButton.onClick.AddListener(ApplyCompression);
		CancelButton.onClick.AddListener(CancelCompression);
		numerator.text = "1";
		denominator.text = "2";
		numerator.onValueChanged.AddListener(UpdateWarningText);
		denominator.onValueChanged.AddListener(UpdateWarningText);

    }
	
	private void ApplyCompression(){
		if ((percentDiscard<1)&&(correctInput)){
			MessageList.CompressLiveMessages(int.Parse(numerator.text), int.Parse(denominator.text));
			SpacecraftStateUtilities.ResetOrbitLines();
			transform.gameObject.SetActive(false);
		}else
		{
			errorText.text = !correctInput ? 
				"Please provide integer values in the input fields." : 
				"Discard percentage must be less than 100%.";
		}
	}

	private void CancelCompression(){
		transform.gameObject.SetActive(false);
	}

	private void UpdateWarningText(string newValue){
		try{
			percentDiscard = float.Parse(numerator.text)/float.Parse(denominator.text);
			correctInput = true;
		} catch{
			errorText.text = "Please provide integer values in the input fields.";
			correctInput = false;
		}
		if (correctInput)
		{
			errorText.text = percentDiscard<1 ?
				$"{100 * percentDiscard:0.0}% of messages will be discarded." :
				"Discard percentage must be less than 100%.";
		}
	}

}
