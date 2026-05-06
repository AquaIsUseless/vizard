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
/// Builds a sensor frustum HUD
/// </summary>
public class DrawSensorFrustum : MonoBehaviour
{
	public LineRenderer lineRenderer;
	public GameObject activeFace;
	private float widthFOV;
	private float heightFOV;
	private float size = 5f; 
	private Vector3 v0 =Vector3.zero;
	private Vector3 v1;
	private Vector3 v2;
	private Vector3 v3;
	private Vector3 v4;
	private Color activeColor;
	private Color frustumWallsColor;
	private Color frustumEdgeColor;
	
	public void InitializeFrustum(float width, float height, float sizeToUse, Color wallColor, Color edgeColor){
		widthFOV = width;
		heightFOV = height;
		size = sizeToUse;
		GetComponentInChildren<HUDLineRenderer>().InitializeHUDLine(size);
		frustumWallsColor= wallColor;
		frustumEdgeColor=edgeColor;
		CalculateFrustumVertices();
		DrawFrustumSides();
		DrawFrustumActiveFace();
		DrawFrustumEdges();
		GetComponent<MeshRenderer>().material.color = frustumWallsColor;
		activeFace.GetComponent<MeshRenderer>().material.color = frustumEdgeColor;
	}

	public void SetActiveColor(Color newColor){
		activeColor = newColor;
		activeFace.GetComponent<MeshRenderer>().material.color = activeColor;
	}

	private void CalculateFrustumVertices(){
		//The following calculations followed Unity's Frustum Calculation
		//https://docs.unity3d.com/Manual/FrustumSizeAtDistance.html
		// but were updated to keep the hypotenuse of the frustum constant
		// instead of "distance"
		float distance;
		if (widthFOV <= heightFOV){
			distance = size*Mathf.Cos(widthFOV *0.5f*Mathf.Deg2Rad);
		}else{
			distance = size*Mathf.Cos(heightFOV*0.5f*Mathf.Deg2Rad);
		}
		float frustumHeight = 2.0f * distance *Mathf.Tan(heightFOV *0.5f*Mathf.Deg2Rad);
		float frustumWidth = 2.0f * distance *Mathf.Tan(widthFOV *0.5f*Mathf.Deg2Rad);

		v0 = Vector3.zero;
		v1 = new Vector3(frustumWidth*0.5f, frustumHeight*0.5f, distance);
		v2 = new Vector3(frustumWidth*0.5f, -frustumHeight*0.5f, distance);
		v3 = new Vector3(-frustumWidth*0.5f, -frustumHeight*0.5f, distance);
		v4 = new Vector3(-frustumWidth*0.5f, frustumHeight*0.5f, distance);
	}

	private void DrawFrustumSides(){
		Mesh mesh = GetComponent<MeshFilter>().mesh;

		mesh.Clear();

		// make changes to the Mesh by creating arrays which contain the new values
		mesh.vertices = new[] {v0,v1,v2,v3,v4};

		Vector2[] uvs = new Vector2[mesh.vertices.Length];
		for (int i = 0; i < uvs.Length; i++){
			uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
		}
		mesh.uv = uvs;
		mesh.triangles =  new[] {0, 1, 2,  0,2,3,  0,3,4, 0,4,1};
	}

	private void DrawFrustumActiveFace(){
		Mesh mesh =  activeFace.GetComponent<MeshFilter>().mesh;

		mesh.Clear();

		// make changes to the Mesh by creating arrays which contain the new values
		mesh.vertices = new[] {v0,v1,v2,v3,v4};

		Vector2[] uvs = new Vector2[mesh.vertices.Length];
		for (int i = 0; i < uvs.Length; i++){
			uvs[i] = new Vector2(mesh.vertices[i].x, mesh.vertices[i].z);
		}
		mesh.uv = uvs;
		mesh.triangles =  new[] { 1,2,3,  3,4,1};
	}

	private void DrawFrustumEdges()
	{
		Vector3[] verticesForFrustum = new Vector3[] {v1, v2, v3, v4};
		lineRenderer.startColor = frustumEdgeColor;
		lineRenderer.endColor = frustumEdgeColor;
		lineRenderer.material.color = frustumEdgeColor;
		lineRenderer.loop = true;
		lineRenderer.positionCount = verticesForFrustum.Length;
		lineRenderer.SetPositions(verticesForFrustum);
	}

	public void UpdateSizeForMaxDimChange(float newDimension)
	{
		lineRenderer.gameObject.GetComponent<HUDLineRenderer>().SetMeshDim(newDimension);
		size = newDimension;
		if (newDimension <1 ){
			size =1f;
		}
		CalculateFrustumVertices();
		DrawFrustumSides();
		DrawFrustumActiveFace();
		DrawFrustumEdges();
	}
}
