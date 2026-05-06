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
/// Builds a camera frustum HUD for a secondary camera
/// </summary>
public class DrawCameraFrustum : MonoBehaviour
{
	public bool standardCamera;
	public LineRenderer frustumLineRenderer;
	public Camera myCamera;
	private float length = 1f;
	private readonly Color stdCamColor = new Color(0.957f, 0.663f, 0.035f, 0.8F);
	private readonly Color instCamColor = new Color(0.925f, 0.957f, 0.039f, 0.8F);
	private Vector3 v0;
	private Vector3 v1;
	private Vector3 v2;
	private Vector3 v3;
	private Vector3 v4;
	
	public void DrawFrustum(float maxExtent)
	{
		length = maxExtent;
		frustumLineRenderer.GetComponent<HUDLineRenderer>().SetMeshDim(length);
		DrawFrustumSides();
		DrawFrustumEdges();
	}

	private void DrawFrustumSides(){

		Mesh mesh = GetComponent<MeshFilter>().mesh;

		mesh.Clear();
		//The following calculations followed Unity's Frustum Calculation
		//https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
		// but were updated to keep the hypotenuse of the frustum constant
		// instead of "distance"
		float distance = length*Mathf.Cos(myCamera.fieldOfView *0.5f*Mathf.Deg2Rad);
		float frustumHeight = 2.0f * distance *Mathf.Tan(myCamera.fieldOfView *0.5f*Mathf.Deg2Rad);
		float frustumWidth;
		if (standardCamera){
			frustumWidth = frustumHeight * myCamera.aspect;
		}else{ //use the output image resolution to calculate aspect ratio
			float outputWidth = myCamera.GetComponent<InstrumentCameraMethods>().reqWidth;
			float outputHeight = myCamera.GetComponent<InstrumentCameraMethods>().reqHeight;
			frustumWidth = frustumHeight *(outputWidth/outputHeight);
		}

		v0 = Vector3.zero;
		v1 = new Vector3(frustumWidth*0.5f, frustumHeight*0.5f, distance);
		v2 = new Vector3(frustumWidth*0.5f, -frustumHeight*0.5f, distance);
		v3 = new Vector3(-frustumWidth*0.5f, -frustumHeight*0.5f, distance);
		v4 = new Vector3(-frustumWidth*0.5f, frustumHeight*0.5f, distance);
		
		// make changes to the Mesh by creating arrays which contain the new values
		mesh.vertices = new[] {v0,v1,v2,v3,v4};

		Vector2[] uvs = new Vector2[mesh.vertices.Length];
		for (int i = 0; i < uvs.Length; i++){
			uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
		}
		mesh.uv = uvs;
		
		mesh.triangles =  new[] {0, 1, 2,  0,2,3,  0,3,4, 0,4,1};
		
		GetComponent<MeshRenderer>().material.color = standardCamera?stdCamColor:instCamColor;
	}

	private void DrawFrustumEdges()
	{
		Vector3[] verticesForFrustum = new Vector3[] {v1, v0,v2, v0,v3, v0, v4, v1, v2, v3, v4};
		Color colorToUse = standardCamera ? stdCamColor : instCamColor;
		frustumLineRenderer.startColor = colorToUse;
		frustumLineRenderer.endColor = colorToUse;
		frustumLineRenderer.material.color = colorToUse;
		frustumLineRenderer.positionCount = verticesForFrustum.Length;
		frustumLineRenderer.SetPositions(verticesForFrustum);
	}
}

