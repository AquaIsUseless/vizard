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
using UnityEngine.EventSystems;
/// <summary>
/// Sets up and updates the current value of a bar graph display unit
/// </summary>
public class BarDisplayMethods : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public double maxValue;
	public double currentValue;
	public float barHeight;
	public GameObject myBar;
	public Transform myFillArea;
	public RectTransform myFill;
	public Transform myTextImage;
	
	// Use this for initialization
	void Start () {
		myBar = transform.gameObject;
		barHeight = myBar.GetComponent<RectTransform> ().sizeDelta.y;

		myFillArea = this.gameObject.transform.GetChild(1);
		myFill = myFillArea.GetChild (0).gameObject.GetComponent<RectTransform> ();
		myTextImage = myFillArea.GetChild(1).transform;
		myTextImage.gameObject.SetActive (false);
		ChangeMaxValue (maxValue);
		ChangeCurrentValue (currentValue);
	}

	public void ChangeMaxValue(double newValue){
		maxValue = newValue;
		if (maxValue < 0) {
			maxValue = -maxValue;
		}
	}

	private void ChangeCurrentValue(double newValue){
		currentValue = newValue;
		if (Mathf.Abs ((float)currentValue) > (float)maxValue) {
			ChangeMaxValue (currentValue);
		}

		float fillHeight = Mathf.Abs ((float)currentValue / (float)maxValue)*barHeight/2;
		if (fillHeight < 1) {
			fillHeight = 1;
		}
		myFill.sizeDelta = new Vector2 (myFill.sizeDelta.x, fillHeight);
		if (currentValue >= 0) {
			myFill.pivot = new Vector2 (0.5f, 0);
			myFill.anchoredPosition = new Vector2 (myFill.anchoredPosition.x, 0);

		} else {
			myFill.pivot = new Vector2 (0.5f, 1.0f);
			myFill.anchoredPosition = new Vector2 (myFill.anchoredPosition.x, 0);

		}
	}

	public void OnPointerEnter(PointerEventData data){
		myTextImage.gameObject.SetActive (true);
		myBar.GetComponent<RectTransform> ().SetAsLastSibling ();
		myTextImage.gameObject.GetComponentInChildren<TextMeshProUGUI> ().text = currentValue.ToString("#.000");
		myTextImage.position = data.position;
	}

	public void OnPointerExit(PointerEventData data){
		myTextImage.gameObject.SetActive (false);
	}
		
	// Update is called once per frame
	void Update () {
		ChangeCurrentValue (currentValue);
		if (myTextImage.gameObject.activeInHierarchy) {
			myTextImage.gameObject.GetComponentInChildren<TextMeshProUGUI> ().text = currentValue.ToString("#.000");
		}
	}
}
