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
/// Used on GUI panels to create a drag bar area to move panels around.
/// </summary>
public class DragPanel : MonoBehaviour, IPointerDownHandler, IDragHandler
{
	private Vector2 startingPointerPosition;
	private Vector3 startingPanelPosition;
	public RectTransform panelRectTransform;
	public RectTransform canvasRectTransform;

	//object that is going to be moved
	public GameObject dragContent;
	
	void Start ()
	{
		if (dragContent == null)
		{
			dragContent = transform.parent.gameObject;
		}

		panelRectTransform = dragContent.GetComponent<RectTransform>();
		canvasRectTransform = VizardGUISettings.PanelViewMgr.GetComponent<RectTransform>();
	}
	
	public void OnPointerDown (PointerEventData data) {
		startingPanelPosition = panelRectTransform.localPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle (canvasRectTransform, data.position, data.pressEventCamera, out startingPointerPosition);
	}
	
	public void OnDrag (PointerEventData data) {
		if (panelRectTransform == null || canvasRectTransform == null)
		{
			Debug.Log($"{dragContent.name} Drag Panel not set up correctly.");
			return;
		}

		Vector2 localPointerPosition;
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle (canvasRectTransform, data.position, data.pressEventCamera, out localPointerPosition)) {
			Vector3 offset = localPointerPosition - startingPointerPosition;
			panelRectTransform.localPosition = startingPanelPosition + offset;
		}
	}
}
