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
/// <summary>
/// Draws a circle with a line renderer
/// for circular generic sensors
/// </summary>
public class DrawCircle : MonoBehaviour
{
	public LineRenderer lineRenderer;
	private HUDLineRenderer HUDLine;
	
	public void SetCirclePointsAndColor(Vector3[] pointsToDraw, Color colorToUse, float newMeshDim)
	{
		if (HUDLine == null)
		{
			HUDLine= GetComponent<HUDLineRenderer>();
			HUDLine.InitializeHUDLine(newMeshDim);
		}
		else
		{
			HUDLine.SetMeshDim(newMeshDim);
		}

		lineRenderer.startColor = colorToUse;
		lineRenderer.endColor = colorToUse;
		lineRenderer.loop = true;
		lineRenderer.material.color = colorToUse;
		lineRenderer.positionCount = pointsToDraw.Length;
		lineRenderer.SetPositions(pointsToDraw);
	}

}
