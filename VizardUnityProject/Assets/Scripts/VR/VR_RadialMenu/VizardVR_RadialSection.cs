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
using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Radial section class to hold desired event, icon, name and caption string
/// </summary>
[Serializable]
public class VizardVR_RadialSection  {

    [Tooltip("Action to execute on section selection")]
    public UnityEvent onPress = new UnityEvent(); 
    [Tooltip("Icon to display in section (optional)")]
    public GameObject icon;
    [Tooltip("Option name")]
    public string name;
    [Tooltip("Caption to display below radial menu when section is in user focus")]
    public string captionString; //only needed if different from option name
}

