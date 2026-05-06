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
/// Calculate the bounds of the attached game object's model (including all sub-meshes)
/// </summary>
public class ModelBounds : MonoBehaviour
{
	public bool useBoxCollider=true;
	public Vector3 modelExtents = Vector3.one;
	public Vector3 modelCenter = Vector3.zero;
	
	public Vector3 unitModelExtents = Vector3.one;
	public Vector3 unitModelCenter = Vector3.zero;
	public float unitModelMaxExtent = 1;
	public bool recalcBounds;
	public bool setUnitBounds;
	
	void FixedUpdate(){
		if (recalcBounds){
			SetupModelBoundsWithModel(useBoxCollider, transform.gameObject);
			recalcBounds = false;
		}
		if (setUnitBounds){
			SetupUnitBoundsForModel(transform.gameObject);
			setUnitBounds = false;
		}
	}

	public void SetupModelBoundsWithModel(bool useBox, GameObject model){
		Bounds completeBounds = SpacecraftStateUtilities.CalculateModelBounds(model);

		useBoxCollider = useBox;
		modelCenter = completeBounds.center;
		modelExtents = completeBounds.extents;
	}
		
	public void SetupUnitBoundsForModel(GameObject model){
		Bounds completeBounds = SpacecraftStateUtilities.CalculateModelBounds(model);
		unitModelCenter = completeBounds.center;
		unitModelExtents = completeBounds.extents;
		unitModelMaxExtent = Mathf.Max(unitModelExtents.x, unitModelExtents.y, unitModelExtents.z);
	}

	public int GetAxisOfMaxExtent(bool useUnitBounds=true)
	{
		Vector3 extentsToCheck = unitModelExtents;
		int axisOfMaxExtent = 0;
		if (!useUnitBounds)
		{
			extentsToCheck = modelExtents;
		}

		if (extentsToCheck.x < extentsToCheck.y)
		{
			axisOfMaxExtent = 1;
		}

		if (extentsToCheck[axisOfMaxExtent] < extentsToCheck.z)
		{
			axisOfMaxExtent = 2;
		}

		return axisOfMaxExtent;
	}

}
