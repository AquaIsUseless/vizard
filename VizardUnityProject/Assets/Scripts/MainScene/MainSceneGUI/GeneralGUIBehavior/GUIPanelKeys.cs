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
using UnityEngine.EventSystems;
using System;
using TMPro;
/// <summary>
/// Compiles a list of all input fields on the currently active and in focus
/// panel. Allows the user press tab key to move through them and to press
/// enter to complete an entry.
/// <remarks>Help for this was found at these threads:
///	https://forum.unity.com/threads/tab-between-input-fields.263779/
/// https://answers.unity.com/questions/1004722/how-to-get-an-array-of-all-buttons-attached-to-a-p.html
/// </remarks>
/// </summary>
public class GUIPanelKeys : MonoBehaviour
{
	public Button buttonToClickOnEnter;
	public TMP_InputField[] panelInputFields;
	private EventSystem system;
	
	void Start()
	{
		system = EventSystem.current;// EventSystemManager.currentSystem;
		panelInputFields = GetComponentsInChildren<TMP_InputField>();

	}
	// Update is called once per frame
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Tab))
		{

			TMP_InputField current = system.currentSelectedGameObject.GetComponent<TMP_InputField>();
			if (current!=null)
			{
				int index = Array.IndexOf(panelInputFields, current);
				TMP_InputField nextInputField = panelInputFields[0];
				if (index+1<panelInputFields.Length){
					nextInputField = panelInputFields[index+1];
				}
				nextInputField.OnPointerClick(new PointerEventData(system));  //if it's an input field, also set the text caret
				
				system.SetSelectedGameObject(nextInputField.gameObject, new BaseEventData(system));
			}
		}else if (Input.GetKeyDown(KeyCode.Return)){
			if (buttonToClickOnEnter!=null){
				ExecuteEvents.Execute(buttonToClickOnEnter.gameObject, new BaseEventData(system), ExecuteEvents.submitHandler);
			}
		}
	}
}
