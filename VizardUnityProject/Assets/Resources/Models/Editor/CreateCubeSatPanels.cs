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

public class CreateCubeSatPanels : ScriptableWizard
{
    public int numCellsLength = 8;
    public int numCellsWidth = 1;
    public float gap = 0.02f;
    public float cellWidth = 0.9f;
    public float cellHeight = 0.3f;
    public float tabWidth = 0.1f;
    public float tabHeight = 0.05f;
    
    [MenuItem ("GameObject/Create Other/CubeSatPanels")]
    static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard("Create CubeSatPanels", typeof(CreateCubeSatPanels));
	}

	void OnWizardCreate(){
		GameObject newGrid=new GameObject("CubeSatPanels");
		string meshName = newGrid.name + numCellsLength + "x" +numCellsWidth + "Gap" +gap;
		Debug.Log(meshName);
		string meshPrefabPath = "Assets/Editor/" + meshName + ".asset";
		Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshPrefabPath, typeof(Mesh));
		if(mesh==null){
			mesh=new Mesh();
			mesh.name=meshName;

			int totalVertexCount = 8 * numCellsLength * numCellsWidth;
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
					vertices[i+1] = new Vector3(blx+tabWidth, bly, 0);
					vertices[i+2] = new Vector3(blx+cellWidth-tabWidth, bly, 0);
					vertices[i+3] = new Vector3(blx+cellWidth, bly, 0);
					
					vertices[i+4] = new Vector3(blx+cellWidth, bly+cellHeight-tabHeight, 0);
					vertices[i+5] = new Vector3(blx+cellWidth-tabWidth, bly+cellHeight, 0);
					vertices[i+6] = new Vector3(blx+tabWidth, bly+cellHeight, 0);
					vertices[i+7] = new Vector3(blx, bly+cellHeight-tabHeight, 0);
					
					

					uvs[i] = new Vector2(0, 0);
					uvs[i+1] = new Vector2(tabWidth/cellWidth, 0);
					uvs[i + 2] = new Vector2((cellWidth - tabWidth) / cellWidth, 0);
					uvs[i+3] = new Vector2(1, 0);
					uvs[i+4] = new Vector2(1, (cellHeight-tabHeight)/cellHeight);
					uvs[i+5] = new Vector2((cellWidth - tabWidth) / cellWidth, cellHeight);
					uvs[i+6] = new Vector2(tabWidth/cellWidth, cellHeight);
					uvs[i+7] = new Vector2(0, (cellHeight-tabHeight)/cellHeight);
					
					i += 8;
				}
			}

			for (int j = 0; j < totalVertexCount; j++)
			{
				normals[j] = new Vector3(0, 0, 1);
			}

			
			mesh.vertices = vertices;
			mesh.normals = normals;		
			mesh.uv = uvs;

			
			tris = new int[numCellsLength*numCellsWidth*6*3]; 
			int ct=0;
			Debug.Log(numCellsLength * numCellsWidth * 6);
			for (int k = 0; k < numCellsLength * numCellsWidth; k++)
			{
				Debug.Log(ct);
				//Making six triangles for every cell
				int z = k * 8;
				//Tri 1
				tris[ct] = z;
				tris[ct + 1] = z + 1;
				tris[ct + 2] = z + 7;
				
				//Tri 2
				tris[ct + 3] = z+1;
				tris[ct + 4] = z+2;
				tris[ct + 5] = z+5;
				
				//Tri 3
				tris[ct + 6] = z+2;
				tris[ct + 7] = z+3;
				tris[ct + 8] = z+4;
				
				//Tri 4
				tris[ct + 9] = z+2;
				tris[ct + 10] = z+4;
				tris[ct + 11] = z+5;
				
				//Tri 5
				tris[ct + 12] = z+1;
				tris[ct + 13] = z+5;
				tris[ct + 14] = z+6;
				
				//Tri 6
				tris[ct + 15] = z+1;
				tris[ct + 16] = z+6;
				tris[ct + 17] = z+7;
				
				ct += 18;
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
