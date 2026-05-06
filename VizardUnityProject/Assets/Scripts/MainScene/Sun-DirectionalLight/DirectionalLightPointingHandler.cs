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
/// Points the directional light that is added to light the scene
/// when no Sun celestial body is present in VizMessages
/// </summary>
public class DirectionalLightPointingHandler : MonoBehaviour
{
    
    // Update is called once per frame
    void FixedUpdate()
    {
		transform.LookAt(MainCameraUtilities.CameraTarget.transform);
    }
    
    public void UseShellLighting()
    {
        transform.GetChild(0).gameObject.SetActive(true); //Main Shell Lighting
        transform.GetChild(1).gameObject.SetActive(true); //Back Shell Lighting
    }
}
