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
/// Handles saving off a subset of the current scenario's available messages
/// </summary>
public class SaveMessageSubsetPanel : MonoBehaviour
{
	public TMP_InputField startIndex;
	public TMP_InputField endIndex;
	public TMP_InputField fileName;
	public Button SaveButton;
	public Button OkayButton;
	public TextMeshProUGUI errorText;
	public TextMeshProUGUI currentMsgText;
	public GameObject confirmationPanel;
	public TextMeshProUGUI confirmationText;

	public Toggle saveFileOnExitToggle;
	public TMP_InputField fileNameToUseField;
	public TextMeshProUGUI filepath;

	private int startRange;
	private int endRange;
	private int messageCount;
	private string filenameToUse = "vizSave";
    // Start is called before the first frame update
    void Start()
    {
		SaveButton.onClick.AddListener(ApplyFileSaveOptions);
		OkayButton.onClick.AddListener(OkayDismiss);

		startIndex.text = startRange.ToString();
		messageCount = MessageList.TimestepsTotal;
		endRange = messageCount;
		endIndex.text = endRange.ToString();
		fileName.text = "vizSave";
		startIndex.onEndEdit.AddListener(VerifyValidStart);
		endIndex.onEndEdit.AddListener(VerifyValidEnd);
		fileName.onEndEdit.AddListener(VerifyValidFilename);
		saveFileOnExitToggle.onValueChanged.AddListener(toggleSaveFileOnExit);
		fileNameToUseField.onEndEdit.AddListener(ChangeSaveFileName);
		errorText.text = "";
    }

	void OnEnable(){
		if ((Application.platform == RuntimePlatform.WindowsPlayer)||(Application.platform == RuntimePlatform.WindowsEditor)){
			filepath.text = "user/MyDocuments/VizardData";
		}else{
			filepath.text =  "user/VizardData/";
		}
		messageCount = MessageList.TimestepsTotal;
		endRange = messageCount-1;
		endIndex.text = endRange.ToString();
		fileNameToUseField.text = DataManager.SaveMsgFileName;
		saveFileOnExitToggle.isOn = DataManager.SaveMsgFileOnQuit;
		fileNameToUseField.interactable=DataManager.SaveMsgFileOnQuit;
	}

	void FixedUpdate(){
		messageCount = MessageList.TimestepsTotal;
		currentMsgText.text = $"Index of current message displayed: {MessageList.CurrentIndex}";
	}

	private void VerifyValidStart(string newValue){
		try{
			startRange = int.Parse(newValue);
			if ((startRange<0)||(startRange>messageCount -1)){
				startIndex.text = "0";
				errorText.color = Color.red;
				errorText.text = $"Invalid start index. Value must be between 0 and {messageCount-1}";
			}
		}catch{
			errorText.color = Color.red;
			errorText.text = $"Please provide integer values greater than 0 and less than {messageCount-1} (maximum message index).";
		}
	}

	private void VerifyValidEnd(string newValue){
		try{
			endRange = int.Parse(newValue);
			if ((endRange<0)||(endRange>messageCount -1)){
				endIndex.text = (messageCount -1).ToString();
				errorText.color = Color.red;
				errorText.text = $"Invalid end index. Value must be between 0 and {messageCount-1}";
			} else if (endRange <= startRange){
				endIndex.text = (messageCount -1).ToString();
				errorText.color = Color.red;
				errorText.text = "Please select an end index for the message range that is greater than the start index.";
			}
		}catch{
			errorText.color = Color.red;
			errorText.text = $"Please provide integer values greater than 0 and less than {messageCount-1} (maximum message index).";
		}
	}

	private void VerifyValidFilename(string newValue){
		try{
			if( !string.IsNullOrEmpty(newValue)){
				filenameToUse = newValue;
			}else{
				errorText.color = Color.red;
				errorText.text = "Please provide an alphanumeric string for the filename.";
			}
		}catch{
			errorText.color = Color.red;
				errorText.text = "Please provide an alphanumeric string for the filename.";
		}
	}

	private void ApplyFileSaveOptions(){
		if (endRange - startRange >0){
			if ((filenameToUse != "")&&(filenameToUse!=" ")){
				MessageList.SaveMessageSubset(filenameToUse+".bin", startRange, endRange);
				confirmationPanel.SetActive(true);
				confirmationText.text = $"Messages {startRange} to {endRange} were saved to {filenameToUse}.bin. \nFile is available at {filepath}."; //"\nFile is available in the directory containing Vizard application."
			}else{
				errorText.color = Color.red;
				errorText.text = "Please enter a valid alphanumeric filename string and retry.";
			}
		}else{
			errorText.color = Color.red;
			errorText.text = "Current message range will create an empty file. Please modify range values and retry.";
		}
	}

	private void OkayDismiss(){
		confirmationPanel.SetActive(false);
		transform.gameObject.SetActive(false);
	}

	private void toggleSaveFileOnExit(bool saveOn){
		DataManager.SaveMsgFileOnQuit = saveOn;
		fileNameToUseField.interactable=saveOn;
	}

	private void ChangeSaveFileName(string newName){
		DataManager.SaveMsgFileName=newName;
	}

}
