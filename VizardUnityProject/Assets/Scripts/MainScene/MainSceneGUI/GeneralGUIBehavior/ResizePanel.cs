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
/// Handles user input to resize a GUI panel by dragging the bottom right corner
/// </summary>
public class ResizePanel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {

	public Vector2 minSize;
	public Vector2 maxSize;
	public Vector2 currentSize;
	private RectTransform rectTransform;
	private Vector2 currentPointerPosition;
	private Vector2 previousPointerPosition;

	void Awake () {
		rectTransform = transform.parent.GetComponent<RectTransform>();
	}

	public void OnPointerDown (PointerEventData data) {
		rectTransform.SetAsLastSibling();
		RectTransformUtility.ScreenPointToLocalPointInRectangle (rectTransform, data.position, data.pressEventCamera, out previousPointerPosition);
	}

	public void OnDrag (PointerEventData data) {
		if (rectTransform == null)
			return;

		Vector2 sizeDelta = rectTransform.sizeDelta;

		RectTransformUtility.ScreenPointToLocalPointInRectangle (rectTransform, data.position, data.pressEventCamera, out currentPointerPosition);
		Vector2 resizeValue = currentPointerPosition - previousPointerPosition;

		sizeDelta += new Vector2 (resizeValue.x, -resizeValue.y);
		sizeDelta = new Vector2 (
			Mathf.Clamp (sizeDelta.x, minSize.x, maxSize.x),
			Mathf.Clamp (sizeDelta.y, minSize.y, maxSize.y)
		);

		rectTransform.sizeDelta = sizeDelta;
		currentSize = sizeDelta;

		previousPointerPosition = currentPointerPosition;
	}

	public void OnPointerUp(PointerEventData data){
		transform.GetComponentInParent<SubpanelMethods> ().UpdateComponentsForPanelResize (currentSize);		
	}

	public void SetPanelSize(int height, int width){
		Vector2 sizeDelta = new Vector2 (width, height);
		sizeDelta = new Vector2 (
			Mathf.Clamp (sizeDelta.x, minSize.x, maxSize.x),
			Mathf.Clamp (sizeDelta.y, minSize.y, maxSize.y)
		);

		rectTransform.sizeDelta = sizeDelta;
		currentSize = sizeDelta;

		transform.GetComponentInParent<SubpanelMethods> ().UpdateComponentsForPanelResize (currentSize);
	}
}
