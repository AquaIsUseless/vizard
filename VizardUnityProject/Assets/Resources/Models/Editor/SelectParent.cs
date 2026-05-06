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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SelectParent : EditorWindow
{
	//Turn this on to allow you to move selected objects into a container that is a child of the grandparent object.
//	[MenuItem("Edit/Select parent &c")]
//	static void SelectParentOfObjects()
//	{
//		GameObject[] parents = Selection.gameObjects;
//		GameObject container = parents[0].transform.parent.gameObject.transform.parent.gameObject.transform.GetChild(0).gameObject;
//		for (int i=0; i<parents.Length;i++){
//			parents[i] = parents[i].transform.parent.gameObject;
//			parents[i].transform.SetParent(container.transform);
//			Debug.Log(parents[i]);
//		}
//		Selection.gameObjects = parents;
//	}
}
