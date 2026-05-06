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
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Handles user input to the fixed frame body selection panel
/// </summary>
public class ChangeBodyForTruePathFrameMethods : MonoBehaviour
{
	[Header("Panel GUI Elements")]
	public TMP_Dropdown bodyDropdown;
	private bool firstClick = true;
	private List<string> bodyList = new List<string> ();
	public bool isFixedFrameCaller;
	public Toggle frameToggle;
	private bool includeSpacecraftInFixedFrame;

	void Start(){
		bodyDropdown.onValueChanged.AddListener (ChangeSelectedBody);
	}

	void OnEnable ()
	{
		if (firstClick)
		{
			includeSpacecraftInFixedFrame = SpacecraftStateUtilities.ParentSpacecraftList.Count > 1;
			if (isFixedFrameCaller&&includeSpacecraftInFixedFrame)
			{
				bodyList = VizardGUISettings.CreateBodyListForDropdown(bodyDropdown, "Auto: Use Parent Body");
			}
			else
			{
				bodyList = VizardGUISettings.CreateBodyListForDropdown(bodyDropdown, "Auto: Use Parent Body", false,
					true, false);
			}

			VizardGUISettings.PopulateList (bodyDropdown, bodyList);
			firstClick = false;
		} 
		
		ChangeDropdownChoice();
	}

	private void ChangeDropdownChoice(){
		if (isFixedFrameCaller)
		{
			if (VizardGUISettings.UseSpacecraftParentBodyForFixedFrameTraj)
			{
				bodyDropdown.value = 0;
			}
			else
			{
				if (VizardGUISettings.FixedBodyIsSpacecraft)
				{
					bodyDropdown.value = VizardGUISettings.FixedBodyIndex + 1;
				}
				else
				{
					if (includeSpacecraftInFixedFrame)
					{
						bodyDropdown.value = VizardGUISettings.FixedBodyIndex +
						                     SpacecraftStateUtilities.ParentSpacecraftList.Count + 1;
					}
					else
					{
						bodyDropdown.value = VizardGUISettings.FixedBodyIndex + 1;
					}
				}
			}
		}else
		{
			
			if (VizardGUISettings.UseSpacecraftParentBodyForRelativeTraj)
			{
				bodyDropdown.value = 0;
			}
			else
			{
				bodyDropdown.value = VizardGUISettings.RelativeBodyIndex + 1;
			}
		}
	}

	private void ChangeSelectedBody(int listValue)
	{
		if (isFixedFrameCaller)
		{
			if (listValue == 0)
			{
				VizardGUISettings.UseSpacecraftParentBodyForFixedFrameTraj = true;
				VizardGUISettings.FixedBodyIsSpacecraft = false;
				VizardGUISettings.FixedBodyIndex = -1;
			}
			else
			{
				VizardGUISettings.UseSpacecraftParentBodyForFixedFrameTraj = false;
				if (includeSpacecraftInFixedFrame)
				{
					if (listValue <= SpacecraftStateUtilities.ParentSpacecraftList.Count)
					{
						VizardGUISettings.FixedBodyIndex = listValue - 1;
						VizardGUISettings.FixedBodyIsSpacecraft = true;
					}
					else
					{
						VizardGUISettings.FixedBodyIndex =
							listValue - SpacecraftStateUtilities.ParentSpacecraftList.Count - 1;
						VizardGUISettings.FixedBodyIsSpacecraft = false;
					}
				}
				else
				{
						VizardGUISettings.UseSpacecraftParentBodyForFixedFrameTraj = false;
						VizardGUISettings.FixedBodyIndex = listValue - 1;
				}
			}

			frameToggle.isOn = true;
			VizardGUISettings.TruePathLineMode = 5;
			VizardGUISettings.TruePathLinesVisible = true;
		}
		else
		{
			if (listValue == 0)
			{
				VizardGUISettings.UseSpacecraftParentBodyForRelativeTraj = true;
				VizardGUISettings.RelativeBodyIndex = -1;
			}
			else
			{
				VizardGUISettings.UseSpacecraftParentBodyForRelativeTraj = false;
				VizardGUISettings.RelativeBodyIndex = listValue - 1;
			}

			frameToggle.isOn = true;
			VizardGUISettings.TruePathLineMode = 3;
			VizardGUISettings.TruePathLinesVisible = true;
		}

		VizardGUISettings.RelativeTruePathChangeCount++;
	}
}
