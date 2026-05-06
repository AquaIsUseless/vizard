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
/// Monitors the entrance and exit of other colliders in the
/// antenna layer from this Location's field of view & range. 
/// </summary>
public class StationTrigger : MonoBehaviour
{
	public GameObject station;
	
	void Start()
	{
		station = transform.parent.transform.parent.gameObject;
	}

	private void OnTriggerEnter(Collider other){
		if (other.transform.gameObject.layer == 14) //The collider is in the Antenna layer
		{
			//Debug.LogFormat ("{0} has entered {1} range!", other.name, stationName);
			GameObject enteredObject = other.transform.parent.gameObject;
			if (enteredObject != station)
			{
				transform.GetComponentInParent<FullLocationMethods>()
					.AntennaEnteredStationRange(enteredObject);
			}
		}
	}

	private void OnTriggerExit(Collider other){
		if (other.transform.gameObject.layer  == 14){ //The collider is in the Antenna layer
			//Debug.LogFormat ("{0} has exited {1} range!", other.name, stationName);
			GameObject enteredObject = other.transform.parent.gameObject;
			if (enteredObject != station)
			{
				transform.GetComponentInParent<FullLocationMethods>()
					.AntennaExitedStationRange(enteredObject);
			}
		}
	}

	private void OnTriggerStay(Collider other){
		if (other.transform.gameObject.layer  == 14) //The collider is in the Antenna Layer
		{
			//Debug.LogFormat ("{0} remains in {1} range!", other.name, stationName);
			GameObject enteredObject = other.transform.parent.gameObject;
			if (enteredObject != station)
			{
				transform.GetComponentInParent<FullLocationMethods>()
					.AntennaEnteredStationRange(enteredObject);
			}
		}
	}
}
