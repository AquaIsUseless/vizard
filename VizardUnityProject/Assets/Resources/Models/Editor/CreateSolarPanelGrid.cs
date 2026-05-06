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

public class CreateSolarPanelGrid : ScriptableWizard
{
    public int numCellsLength = 36;
    public int numCellsWidth = 5;
    public float gap = 0.02f;
    public float cellWidth = 0.4f;
    public float cellHeight = 0.4f;
    
    [MenuItem ("GameObject/Create Other/SolarPanelGrid")]
    static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard("Create SolarPanelGrid", typeof(CreateSolarPanelGrid));
	}

	void OnWizardCreate(){
		GameObject newGrid=new GameObject("SolarPanelGrid");
		string meshName = newGrid.name + numCellsLength + "x" +numCellsWidth + "Gap" +gap;
		Debug.Log(meshName);
		string meshPrefabPath = "Assets/Editor/" + meshName + ".asset";
		Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshPrefabPath, typeof(Mesh));
		if(mesh==null){
			mesh=new Mesh();
			mesh.name=meshName;

			int totalVertexCount = 4 * numCellsLength * numCellsWidth;
			Vector3[] vertices=new Vector3[totalVertexCount ];
			Vector3[] normals=new Vector3[totalVertexCount ]; 
			Vector2[] uvs=new Vector2[totalVertexCount]; 
			int[] tris;

			int i = 0;
			for (int j = 0; j < numCellsLength; j++)
			{
				for (int k = 0; k < numCellsWidth; k++)
				{
					float blx = k * cellWidth + k * gap;
					float bly = j * cellHeight + j * gap;

					vertices[i] = new Vector3(blx, bly, 0);
					vertices[i+1] = new Vector3(blx+cellWidth, bly, 0);
					vertices[i+2] = new Vector3(blx+cellWidth, bly+cellHeight, 0);
					vertices[i+3] = new Vector3(blx, bly+cellHeight, 0);
					

					uvs[i] = new Vector2(0, 0);
					uvs[i+1] = new Vector2(1, 0);
					uvs[i+2] = new Vector2(1, 1);
					uvs[i+3] = new Vector2(0, 1);
					
					i += 4;
				}
			}

			for (int j = 0; j < totalVertexCount; j++)
			{
				normals[j] = new Vector3(0, 0, 1);
			}

			
			mesh.vertices = vertices;
			mesh.normals = normals;		
			mesh.uv = uvs;

			
			tris = new int[numCellsLength*numCellsWidth*2*3]; 
			int ct=0;
			Debug.Log(numCellsLength * numCellsWidth * 2);
			for (int k = 0; k < numCellsLength * numCellsWidth; k++)
			{
				Debug.Log(ct);
				//Making two triangles for every cell
				tris[ct] = k*4;
				tris[ct + 1] = k*4 + 1;
				tris[ct + 2] = k*4 + 2;
				tris[ct + 3] = k*4;
				tris[ct + 4] = k*4 + 2;
				tris[ct + 5] = k*4 + 3;
				
				ct += 6;
			}

			mesh.triangles = tris;		
			AssetDatabase.CreateAsset(mesh, meshPrefabPath);
			AssetDatabase.SaveAssets();
		}

		MeshFilter mf=newGrid.AddComponent<MeshFilter>();
		mf.mesh = mesh;

		newGrid.AddComponent<MeshRenderer>();

		// if(addCollider){
		// 	MeshCollider mc=newGrid.AddComponent<MeshCollider>();
		// 	mc.sharedMesh=mf.sharedMesh;
		// }

		Selection.activeObject = newGrid;
	}
}
