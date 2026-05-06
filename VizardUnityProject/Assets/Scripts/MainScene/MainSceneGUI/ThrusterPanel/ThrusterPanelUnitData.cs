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
using TMPro;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// Sets up and updates the data display for a single thruster on a thruster display panel
/// </summary>
public class ThrusterPanelUnitData : MonoBehaviour {
    public int spacecraftID;
    public string thrusterTag;
    public int thrusterID;
    private double currentThrust;
    public double maxThrust;

    private bool isFiring;
    private double lastFiringStartTime;
    private double lastFiringDuration;
    private int updatesSinceLastFiring;
    private float lastFiringThrustFraction;

    private Color greenColor = new Vector4 (0.32f, 0.73f, 0.13f, 1f);

	
    // Update is called once per frame
    void Update () {

        currentThrust = MessageList.CurrentMessage.Spacecraft[spacecraftID].Thrusters[thrusterID].CurrentThrust;

        if (isFiring) {
            if (currentThrust > 0) {
                lastFiringDuration = MessageList.CurrentMessage.CurrentTime.SimTimeElapsed/1e9 - lastFiringStartTime;
                UpdateThrusterBarSetting ();
                UpdateThrusterBarText ();
            } else {
                isFiring = false;
                updatesSinceLastFiring = 1;
                UpdateThrusterBarSetting ();
                UpdateThrusterBarText ();
            }

        } else {
            if (currentThrust > 0) {
                isFiring = true;
                lastFiringStartTime = MessageList.CurrentMessage.CurrentTime.SimTimeElapsed/1e9;
                UpdateThrusterBarSetting ();
                UpdateThrusterBarText ();
            } else {
                if (updatesSinceLastFiring is > 0 and < 20) {
                    updatesSinceLastFiring += 1;
                    UpdateThrusterBarSetting ();
                    UpdateThrusterBarText ();
                } else {
                    updatesSinceLastFiring += 1;
                }
            }
			
        }
    }

    private void UpdateThrusterBarSetting(){
	
        Vector2 currentThrusterBarSettings = GetCurrentThrusterBarSettings ();
        GetComponent<RectTransform> ().sizeDelta = new Vector2 (60, (40 * currentThrusterBarSettings[0]));

        //Set the color of the thruster bar - green if currently firing, shading yellow to indicate recently fired
        if (currentThrusterBarSettings [1] >= 0) {
            if (currentThrusterBarSettings[1] >= 100){
                GetComponentInParent<Image>().color = greenColor;
            }
            else{
                GetComponentInParent<Image>().color = new Vector4 (0.89f, 1.0f, 0.13f, (1 - (currentThrusterBarSettings [1] + 1) / 20));
            }
        }
    }

    private void UpdateThrusterBarText(){
        //if the thruster is firing or has just finished firing, update the text below the bar
        //Build a string with the DD:HH:MM:SS format for the simTime at firing
        double simTime = lastFiringStartTime; //Gets sim time in seconds

        int simDays = (int) simTime/86400;
        string simDstr = simDays.ToString ();
        if (simDays < 10) {
            simDstr = "0" + simDstr;
        }
        //Calculate how many hours in day fraction:
        int simHours = (int) (simTime - simDays*86400)/3600;
        string simHstr = simHours.ToString ();
        if (simHours < 10) {
            simHstr = "0" + simHstr;
        }
        //Calculate how many minutes in hour fraction:
        int simMins = (int) (simTime - simDays*86400-simHours*3600)/60;
        string simMstr = simMins.ToString ();
        if (simMins < 10) {
            simMstr = "0" + simMstr;
        }
        //Calculate how many seconds in minute fraction:
        double simSecs = simTime - simDays*86400-simHours*3600 - simMins*60;
        string simSstr = simSecs.ToString ("F0");
        if (simSecs < 10) {
            simSstr = "0" + simSstr;
        }
        string displayString = $"{simDstr}:{simHstr}:{simMstr}:{simSstr}";

        transform.GetChild (0).GetComponent<TextMeshProUGUI> ().text = displayString;
        transform.GetChild (1).GetComponent<TextMeshProUGUI> ().text = lastFiringDuration.ToString("F3");

    }
	
    private Vector2 GetCurrentThrusterBarSettings(){
        //Return the current thrust and an integer to set the color of the thrust bar
        if (isFiring) {
            // Return color thrust setting of 100 (to get green)
            lastFiringThrustFraction = (float) currentThrust/(float) maxThrust;
            return new Vector2 (lastFiringThrustFraction, 100);

        }
        // Return a frame elapsed number from 0 to 19 (to get a shade of yellow) if the 
        // last thruster firing ended within the last 20 frames (change to time steps before release)
        if (updatesSinceLastFiring is >= 0 and < 20) {
            return new Vector2 (lastFiringThrustFraction, updatesSinceLastFiring);
        } 
        // Return a negative integer to indicate that thruster has not fired recently
        return new Vector2 (0, -10);
    }
}