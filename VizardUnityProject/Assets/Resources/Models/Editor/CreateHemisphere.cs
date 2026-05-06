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

// an Editor method to create a hemisphere primitive 
// center of sphere at  (0/0/0)
// center of hemisphere surface at (0,radius,0)
// note the resulting mesh will be created as an asset in Assets/Editor
// Author: Jennifer Wood

public class CreateHemisphere : ScriptableWizard {

	public int numVertices = 10;
	public int numRings = 3;
	public float sphereRadius = 1f;
	public float angle = 45f; // degrees
	public bool outside = true;
	public bool inside = true;
	public bool addCollider = false;

	[MenuItem ("GameObject/Create Other/Hemisphere")]
	static void CreateWizard()
	{
		ScriptableWizard.DisplayWizard("Create Hemisphere", typeof(CreateHemisphere));
	}

	void OnWizardCreate(){
		GameObject newHemisphere=new GameObject("Hemisphere");
//		if(angle>0&&openingAngle<=180){
//			radiusTop=0;
//			radiusBottom=length*Mathf.Tan(openingAngle*Mathf.Deg2Rad/2);
//		}
		string meshName = newHemisphere.name + numVertices + "Rings" +numRings + "angle" + angle + (outside?"o":"") + (inside?"i":"");
		string meshPrefabPath = "Assets/Editor/" + meshName + ".asset";
		Mesh mesh = (Mesh)AssetDatabase.LoadAssetAtPath(meshPrefabPath, typeof(Mesh));
		if(mesh==null){
			mesh=new Mesh();
			mesh.name=meshName;
			// can't access Camera.current
			//newCone.transform.position = Camera.current.transform.position + Camera.current.transform.forward * 5.0f;
			int multiplier=(outside?1:0)+(inside?1:0);
			int offset=(outside&&inside?(numRings*numVertices+1):0);
			Vector3[] vertices=new Vector3[multiplier*(numVertices*numRings+1)]; 
			Vector3[] normals=new Vector3[multiplier*(numVertices*numRings+1)];
			Vector2[] uvs=new Vector2[multiplier*(numVertices*numRings+1)];
			int[] tris;
			float sweepAngle = Mathf.PI*2f/numVertices;
			float ringWidth =sphereRadius*Mathf.Sin(angle/2f*Mathf.PI/180f)/numRings;
//			float slope=Mathf.Atan((radiusBottom-radiusTop)/length); // (rad difference)/height
//			float slopeSin=Mathf.Sin(slope);
//			float slopeCos=Mathf.Cos(slope);

			vertices[0] = new Vector3(0, sphereRadius, 0);
			normals[0] = new Vector3(0,-1,0);
			uvs[0] = new Vector2(0,0); //I'm guessing here.
			if (outside){
				int index =0;
				if (inside&&outside){
					index = offset;
				}
				vertices[index] = new Vector3(0, sphereRadius, 0);
				normals[index] = new Vector3(0, 1,0); //this is for the outside face
				uvs[index] = new Vector2(1,1); //I'm guessing here.
			}
			int i=1;
			for(int k= 0; k<numRings; k++){
				float rc = ringWidth*(k+1);
				float yc =sphereRadius*Mathf.Cos(Mathf.Asin(rc/sphereRadius));
				for(int j=0; j<numVertices;j++){
					vertices[i] = new Vector3(rc*Mathf.Cos(sweepAngle*j), yc, rc*Mathf.Sin(sweepAngle*j));
					if (outside&&inside){
						vertices[i+offset] = vertices[i];
					}
					normals[i] = Vector3.Normalize(vertices[i]); //outside
					if (outside&&inside){
						normals[i+offset] = normals[i];//outside normal
						normals[i] = new Vector3(-normals[i].x, -normals[i].y, -normals[i].z); //inside normal
					}else{
						if(inside){
							normals[i] = new Vector3(-normals[i].x, -normals[i].y, -normals[i].z); //inside normal
						}
					}
						uvs[i] = new Vector2(0.25f*Mathf.Atan2(normals[i].x,normals[i].z)/Mathf.PI,0.25f-Mathf.Asin(-normals[i].y)/Mathf.PI);
					if(outside&&inside){
						uvs[i+offset] = new Vector2(0.25f*Mathf.Atan2(normals[i+offset].x,normals[i+offset].z)/Mathf.PI,0.25f-Mathf.Asin(-normals[i+offset].y)/Mathf.PI);
					}
						i++;
				}
			}
			mesh.vertices = vertices;
			mesh.normals = normals;		
			mesh.uv = uvs;

			// create triangles
			// here we need to take care of point order, depending on inside and outside
			tris = new int[numVertices*(numRings*2-1)*3*multiplier]; 
			int ct=0;
			//This does inside faces
			if (inside){
				//Do the top of the hemisphere
				for (int j=0; j<numVertices; j++){
					tris[ct] = 0;
					tris[ct+1]=j+1;
					if ((j+2)<=numVertices){
						tris[ct+2] = j+2;
					}else{
						tris[ct+2] = 1;
					}
					ct+=3;
				}
				//Do the rest, 2*numVertices faces per ring
				for(int k=0; k < numRings-1; k++){
					for (int j=0; j<numVertices; j++){
						int a = j+1+k*numVertices;
						int b =a+1;
						int c = a+ numVertices;
						if (b==1+numVertices*(k+1)){
							b = 1+numVertices*k;
						}
						int d = b+numVertices;

						tris[ct] = a;
						tris[ct+1] = d;
						tris[ct+2] = b;
						tris[ct+3] = a;
						tris[ct+4] = c;
						tris[ct+5] = d;
						ct+=6;	
					}
				}
			}
			if(outside){
				int startIndex = 0;
				if (inside){
					startIndex = offset;
				}
				//Do the center of the hemisphere
				for (int j=0; j<numVertices; j++){
					tris[ct] = startIndex;
					tris[ct+2]=startIndex+j+1;
					if ((j+2)<=numVertices){
						tris[ct+1] = startIndex+j+2;
					}else{
						tris[ct+1] = startIndex+1;
					}
					ct+=3;
				}
				//Do the rest, 2*numVertices faces per ring
				for(int k=0; k < numRings-1; k++){
					for (int j=0; j<numVertices; j++){
						int a = startIndex+j+1+k*numVertices;
						int b =a+1;
						int c = a+ numVertices;
						if (b==startIndex+1+numVertices*(k+1)){
							b = startIndex+1+numVertices*k;
						}
						int d = b+numVertices;

						tris[ct] = a;
						tris[ct+1] = b;
						tris[ct+2] = d;
						tris[ct+3] = a;
						tris[ct+4] = d;
						tris[ct+5] = c;
						ct+=6;	
					}
				}
			}

			mesh.triangles = tris;		
			AssetDatabase.CreateAsset(mesh, meshPrefabPath);
			AssetDatabase.SaveAssets();
		}

		MeshFilter mf=newHemisphere.AddComponent<MeshFilter>();
		mf.mesh = mesh;

		newHemisphere.AddComponent<MeshRenderer>();

		if(addCollider){
			MeshCollider mc=newHemisphere.AddComponent<MeshCollider>();
			mc.sharedMesh=mf.sharedMesh;
		}

		Selection.activeObject = newHemisphere;
	}
//
//	public static void BuildHemisphereMesh(GameObject parent, int ringCount, int verticesPerRing, float FOV, bool buildOutside, bool buildInside){
//		numRings = ringCount;
//		numVertices =verticesPerRing;
//		angle = FOV;
//		outside =buildOutside;
//		inside = buildInside;
//
//		string meshName = newHemisphere.name + numVertices + "Rings" +numRings + "angle" + angle + (outside?"o":"") + (inside?"i":"");
//		parent.name = meshName+"Object";
//		Mesh mesh=new Mesh();
//		mesh.name=meshName;
//
//		int multiplier=(outside?1:0)+(inside?1:0);
//		int offset=(outside&&inside?(numRings*numVertices+1):0);
//		Vector3[] vertices=new Vector3[multiplier*(numVertices*numRings+1)]; 
//		Vector3[] normals=new Vector3[multiplier*(numVertices*numRings+1)];
//		Vector2[] uvs=new Vector2[multiplier*(numVertices*numRings+1)];
//		int[] tris;
//		float sweepAngle = Mathf.PI*2f/numVertices;
//		float ringWidth =sphereRadius*Mathf.Sin(angle/2f*Mathf.PI/180f)/numRings;
//
//		vertices[0] = new Vector3(0, sphereRadius, 0);
//		normals[0] = new Vector3(0,-1,0);
//		uvs[0] = new Vector2(0,0); //I'm guessing here.
//		if (outside){
//			int index =0;
//			if (inside&&outside){
//				index = offset;
//			}
//			vertices[index] = new Vector3(0, sphereRadius, 0);
//			normals[index] = new Vector3(0, 1,0); //this is for the outside face
//			uvs[index] = new Vector2(1,1); //I'm guessing here.
//		}
//		int i=1;
//		for(int k= 0; k<numRings; k++){
//			float rc = ringWidth*(k+1);
//			float yc =sphereRadius*Mathf.Cos(Mathf.Asin(rc/sphereRadius));
//			for(int j=0; j<numVertices;j++){
//				vertices[i] = new Vector3(rc*Mathf.Cos(sweepAngle*j), yc, rc*Mathf.Sin(sweepAngle*j));
//				if (outside&&inside){
//					vertices[i+offset] = vertices[i];
//				}
//				normals[i] = Vector3.Normalize(vertices[i]); //outside
//				if (outside&&inside){
//					normals[i+offset] = normals[i];//outside normal
//					normals[i] = new Vector3(-normals[i].x, -normals[i].y, -normals[i].z);
//				}else{
//					if(inside){
//						normals[i] = new Vector3(-normals[i].x, -normals[i].y, -normals[i].z); //inside normal
//					}
//				}
//				uvs[i] = new Vector2(0.25f*Mathf.Atan2(normals[i].x,normals[i].z)/Mathf.PI,0.25f-Mathf.Asin(-normals[i].y)/Mathf.PI);
//				if(outside&&inside){
//					uvs[i+offset] = new Vector2(0.25f*Mathf.Atan2(normals[i+offset].x,normals[i+offset].z)/Mathf.PI,0.25f-Mathf.Asin(-normals[i+offset].y)/Mathf.PI);
//				}
//				i++;
//			}
//		}
//		mesh.vertices = vertices;
//		mesh.normals = normals;		
//		mesh.uv = uvs;
//
//		// create triangles
//		// here we need to take care of point order, depending on inside and outside
//		tris = new int[numVertices*(numRings*2-1)*3*multiplier]; 
//		int ct=0;
//		//This does inside faces
//		if (inside){
//			//Do the top of the hemisphere
//			for (int j=0; j<numVertices; j++){
//				tris[ct] = 0;
//				tris[ct+1]=j+1;
//				if ((j+2)<=numVertices){
//					tris[ct+2] = j+2;
//				}else{
//					tris[ct+2] = 1;
//				}
//				ct+=3;
//			}
//			//Do the rest, 2*numVertices faces per ring
//			for(int k=0; k < numRings-1; k++){
//				for (int j=0; j<numVertices; j++){
//					int a = j+1+k*numVertices;
//					int b =a+1;
//					int c = a+ numVertices;
//					if (b==1+numVertices*(k+1)){
//						b = 1+numVertices*k;
//					}
//					int d = b+numVertices;
//
//					tris[ct] = a;
//					tris[ct+1] = d;
//					tris[ct+2] = b;
//					tris[ct+3] = a;
//					tris[ct+4] = c;
//					tris[ct+5] = d;
//					ct+=6;	
//				}
//			}
//		}
//		if(outside){
//			int startIndex = 0;
//			if (inside){
//				startIndex = offset;
//			}
//			//Do the center of the hemisphere
//			for (int j=0; j<numVertices; j++){
//				tris[ct] = startIndex;
//				tris[ct+2]=startIndex+j+1;
//				if ((j+2)<=numVertices){
//					tris[ct+1] = startIndex+j+2;
//				}else{
//					tris[ct+1] = startIndex+1;
//				}
//				ct+=3;
//			}
//			//Do the rest, 2*numVertices faces per ring
//			for(int k=0; k < numRings-1; k++){
//				for (int j=0; j<numVertices; j++){
//					int a = startIndex+j+1+k*numVertices;
//					int b =a+1;
//					int c = a+ numVertices;
//					if (b==startIndex+1+numVertices*(k+1)){
//						b = startIndex+1+numVertices*k;
//					}
//					int d = b+numVertices;
//
//					tris[ct] = a;
//					tris[ct+1] = b;
//					tris[ct+2] = d;
//					tris[ct+3] = a;
//					tris[ct+4] = d;
//					tris[ct+5] = c;
//					ct+=6;	
//				}
//			}
//		}
//
//		mesh.triangles = tris;		
//	}
//
//	MeshFilter mf=parent.AddComponent<MeshFilter>();
//	mf.mesh = mesh;
//
//	parent.AddComponent<MeshRenderer>();
//	}
}
