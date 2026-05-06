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
using UnityEditor;
using System.Collections;


public class CreateTrianglePanel : ScriptableWizard {
	

	[MenuItem ("GameObject/Create Other/TrianglePanel")]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard("Create Triangle Panel", typeof(CreateTrianglePanel));
	}

	void OnWizardCreate(){
		GameObject trianglePanel=new GameObject("TriPanel");
		string meshName = trianglePanel.name;
		string meshPrefabPath = "Assets/Editor/" + meshName + ".asset";
		Mesh mesh = new Mesh(); // = (Mesh)AssetDatabase.LoadAssetAtPath(meshPrefabPath, typeof(Mesh));
			mesh=new Mesh();
			mesh.name=meshName;

			Vector3[] vertices=new Vector3[18]; // 0..n-1: top, n..2n-1: bottom
			Vector3[] normals=new Vector3[18];
			Vector2[] uvs=new Vector2[18];
			int[] tris;

			//Face 1
			vertices[0] = new Vector3(0, 0, 0);
			vertices[1] = new Vector3(.5f, 1f, 0);
			vertices[2] = new Vector3(-.5f, 1f, 0);
			normals[0] = new Vector3(0, 0, -1);
			normals[1] = new Vector3(0, 0, -1);
			normals[2] = new Vector3(0, 0, -1);
			uvs[0] = new Vector2(.5f, 0);
			uvs[1] = new Vector2(1f, 1f);
			uvs[2] = new Vector2(0, 1f);
			
			//Face 2
			vertices[3] = new Vector3(0, 0, 1f);
			vertices[4] = new Vector3(.5f, 1f, 1f);
			vertices[5] = new Vector3(-.5f, 1f, 1f);
			normals[3] = new Vector3(0, 0, 1f);
			normals[4] = new Vector3(0, 0, 1);
			normals[5] = new Vector3(0, 0, 1);
			uvs[3] = new Vector2(.5f, 0);
			uvs[4] = new Vector2(1f, 1f);
			uvs[5] = new Vector2(0, 1f);
			
			//Face 3/4
			vertices[6] = new Vector3(0, 0, 0);
			vertices[7] = new Vector3(.5f, 1f, 0);
			vertices[8] = new Vector3(.5f, 1f, 1f);
			vertices[9] = new Vector3(0, 0, 1);
			normals[6] = new Vector3(1, -1, 0);
			normals[7] = new Vector3(1, -1, 0);
			normals[8] = new Vector3(1, -1, 0);
			normals[9] = new Vector3(1, -1, 0);
			uvs[6] = new Vector2(0f, 0);
			uvs[7] = new Vector2(1f, 0f);
			uvs[8] = new Vector2(1f, 1f);
			uvs[9] = new Vector2(0f, 1f);
			
			//Face 5/6
			vertices[10] = new Vector3(0, 0, 0);
			vertices[11] = new Vector3(0, 0, 1f);
			vertices[12] = new Vector3(-0.5f, 1, 1f);
			vertices[13] = new Vector3(-0.5f, 1, 0f);
			normals[10] = new Vector3(-1, -1, 0);
			normals[11] = new Vector3(-1, -1, 0);
			normals[12] = new Vector3(-1, -1, 0);
			normals[13] = new Vector3(-1, -1, 0);
			uvs[10] = new Vector2(1f, 0);
			uvs[11] = new Vector2(1f, 1f);
			uvs[12] = new Vector2(0f, 1f);
			uvs[13] = new Vector2(0, 0);
			
			//Face 7/8
			vertices[14] = new Vector3(-.5f, 1, 0);
			vertices[15] = new Vector3(-0.5f, 1, 1f);
			vertices[16] = new Vector3(0.5f, 1, 1f);
			vertices[17] = new Vector3(0.5f, 1, 0);
			normals[14] = new Vector3(0,1,0);
			normals[15] = new Vector3(0,1,0);
			normals[16] = new Vector3(0,1,0);
			normals[17] = new Vector3(0,1,0);
			uvs[14] = new Vector2(1f, 0);
			uvs[15] = new Vector2(1f, 1f);
			uvs[16] = new Vector2(0f, 1f);
			uvs[17] = new Vector2(0f, 0f);

			mesh.vertices = vertices;
			mesh.normals = normals;		
			mesh.uv = uvs;

			tris = new int[24];
			//Face 1
			tris[0] = 0;
			tris[1] = 2;
			tris[2] = 1;
			//Face 2
			tris[3] = 3;
			tris[4] = 4;
			tris[5] = 5;
			//Face 3/4
			tris[6] = 7;
			tris[7] = 8;
			tris[8] = 6;
			tris[9] = 6;
			tris[10] = 8;
			tris[11] = 9;
			//Face 5/6
			tris[12] = 13;
			tris[13] = 10;
			tris[14] = 11;
			tris[15] = 13;
			tris[16] = 11;
			tris[17] = 12;
			//Face 7/8
			tris[18] = 17;
			tris[19] = 14;
			tris[20] = 15;
			tris[21] = 17;
			tris[22] = 15;
			tris[23] = 16;
			
			// create triangles
			
			mesh.triangles = tris;		
			AssetDatabase.CreateAsset(mesh, meshPrefabPath);
			AssetDatabase.SaveAssets();
		

		MeshFilter mf=trianglePanel.AddComponent<MeshFilter>();
		mf.mesh = mesh;

		trianglePanel.AddComponent<MeshRenderer>();


		MeshCollider mc=trianglePanel.AddComponent<MeshCollider>();
		mc.sharedMesh=mf.sharedMesh;

		Selection.activeObject = trianglePanel;
	}
}
