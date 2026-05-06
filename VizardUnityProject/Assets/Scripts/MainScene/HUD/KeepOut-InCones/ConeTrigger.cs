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
/// Monitors the entrance and exit of colliders in the
///  keep out (or in) cone's field of view. 
/// </summary>
public class ConeTrigger : MonoBehaviour {
	
	private GameObject bodyOfInterest;
	private bool isKeepOut;

	public void InitializeTrigger(GameObject body, bool coneIsKeepOut)
	{
		isKeepOut = coneIsKeepOut;
		bodyOfInterest = body;

	}
	private void OnTriggerEnter(Collider other){
		if (other.transform.parent.gameObject ==bodyOfInterest) {
			transform.GetComponentInParent<DrawKeepOutInCone> ().SetConeViolated(isKeepOut);
		}
	}

	private void OnTriggerExit(Collider other){
		if (other.transform.parent.gameObject ==bodyOfInterest) {
			transform.GetComponentInParent<DrawKeepOutInCone> ().SetConeViolated(!isKeepOut);
		}
	}
		private void OnTriggerStay(Collider other){
			if (other.transform.parent.gameObject ==bodyOfInterest)
			{
				transform.GetComponentInParent<DrawKeepOutInCone>().SetConeViolated(isKeepOut);
			}
		}

}
