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
using System.Linq;
using UnityEngine;
using VizProtobufferMessage;
/// <summary>
/// Sets up and updates an individual thruster HUD using its Thruster VizMessage
/// </summary>
public class ThrusterHUDMethods : MonoBehaviour
{
    public int spacecraftIndex;
    public int thrusterIndex;
    public GameObject thrusterLabel;
    private GameObject mySpacecraft;
    private bool inSpriteMode;
    
    private Color maxThrustColor;
    private Color minThrustColor;
    private bool useDefaultColor = true;
    private double maxThrust;
    public Vector3 thrustVector;
    public ParticleSystem thrustPlume;

    private bool isFiring;

    public int particleCount = 300;
    public float particleLife = 0.5f;

    public float minParticleLife = 1f;
    private int myParticleCount = 300;
    private float myParticleLife = 2;
    private readonly float minParticleSize = 2f;
    private bool particleEnginePaused;

    //Thruster geometry
    private MeshRenderer thrusterCone;
    public bool thrusterGeomOn;

    //Thruster normal line 
    private float lineScale;
    private readonly float minLineSize = 1f;
    private readonly float maxLineSize = 3f;
    static Material lineMaterial;
    private bool thrusterNormalVisible;
    
    //Check last frame setting:
    private float lastFrameParticleLifeScalar;


    public void InitializeThrusterHUDUnit(int mySpacecraftIndex, int myThrusterIndex, GameObject spacecraft)
    {
        spacecraftIndex = mySpacecraftIndex;
        thrusterIndex = myThrusterIndex;
        mySpacecraft = spacecraft;
        VizMessage.Types.Thruster myThruster =
            MessageList.FirstMessage.Spacecraft[spacecraftIndex].Thrusters[thrusterIndex];
        thrusterCone = GetComponentInChildren<MeshRenderer>();
        thrusterCone.enabled = thrusterGeomOn;
        
        SetThrusterColorFromMessage(myThruster);
        
        //Establish thruster position and orientation
        transform.position = OrbitVectorMath.ReturnVector3(OrbitVectorMath.TransformFromBSKCStoUnity(myThruster.Position.ToArray()));
        maxThrust = myThruster.MaxThrust;
        
        lastFrameParticleLifeScalar = ThrusterUtilities.GetParticleLifeUserSettingScalar();
        SetParticleLifeSizeAndCount();
        
        thrustVector = OrbitVectorMath.ReturnVector3(OrbitVectorMath.TransformFromBSKCStoUnity(myThruster.ThrustVector.ToArray()));
        transform.forward = thrustVector;
    }

    void OnEnable()
    {
        if (thrusterLabel != null)
        {
            thrusterLabel.SetActive(VizardGUISettings.ShowThrusterLabels);
            if (useDefaultColor)
            {
                UpdateDefaultThrusterColor(ThrusterUtilities.GetDefaultThrusterColor());
            }
        }
    }

    void OnDisable()
    {
        if (thrusterLabel != null)
        {
            thrusterLabel.SetActive(false);
            thrustPlume.Clear();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!inSpriteMode)
        {
            thrusterLabel.SetActive(VizardGUISettings.ShowThrusterLabels);
            VizMessage.Types.Thruster currentMsg =
                MessageList.CurrentMessage.Spacecraft[spacecraftIndex].Thrusters[thrusterIndex];
            double currentThrust = currentMsg.CurrentThrust;
            var main = thrustPlume.main; 

            if (currentThrust > 0)
            {
                isFiring = true;
                if (!useDefaultColor)
                {
                    SetThrusterColorFromMessage(currentMsg);
                }

                if (!thrustPlume.isPlaying)
                {
                    thrustPlume.Play();
                }
            }
            else
            {
                isFiring = false;
                if (thrustPlume.isPlaying)
                {
                    thrustPlume.Stop();
                }
            }

            if (Math.Abs(lastFrameParticleLifeScalar - ThrusterUtilities.GetParticleLifeUserSettingScalar()) > OrbitVectorMath.EPS)
            {
                lastFrameParticleLifeScalar = ThrusterUtilities.GetParticleLifeUserSettingScalar();
                SetParticleLifeSizeAndCount();
                thrustPlume.Clear();
                thrustPlume.Play();
            }
            
            if (isFiring)
            {
                main.maxParticles = (int) (myParticleCount * currentThrust / maxThrust);
                main.startColor = Color.Lerp(minThrustColor, maxThrustColor, (float) (currentThrust / maxThrust));

                //Check to see if playback is paused and the particle engine should be paused also:
                if (particleEnginePaused != MessageList.PlaybackPaused)
                {
                    if (MessageList.PlaybackPaused)
                    {
                        thrustPlume.Pause();
                        particleEnginePaused = true;
                    }
                    else
                    {
                        thrustPlume.Play();
                        particleEnginePaused = false;
                    }
                }
            }

            thrusterCone.enabled = thrusterGeomOn;
        }
        else
        {
            //sprite is on, so don't show thrust plumes or geometry or labels
            thrusterLabel.SetActive(false);
            if (thrustPlume.isPlaying)
            {
                thrustPlume.Stop();
            }

            if (thrusterCone.enabled)
            {
                thrusterCone.enabled = false;
            }
        }
    }

    private void SetParticleLifeSizeAndCount()
    {
        var main = thrustPlume.main; 
        //Start Life of the particles determines the length of the thrust trail
        myParticleLife = particleLife * Mathf.Log((float) maxThrust, 10f) + minParticleLife;
        if (myParticleLife <= 0.05f)
        {
            myParticleLife = 0.05f;
        }

        myParticleLife *= lastFrameParticleLifeScalar;
        if (myParticleLife <= 0.05f)
        {
            myParticleLife = 0.05f;
        }

        myParticleLife *= mySpacecraft.transform.localScale.x;
        main.startLifetime = myParticleLife;

        //Setting the particle count to the minimum count required to maintain a constant stream of particles at maxThrust
        //This ensures that reduced thrust is reflected in puffing of the particles
        myParticleCount = (int) (particleCount * myParticleLife);

        float myStartSize = (myParticleLife) * minParticleSize + 1f;
        main.startSize = myStartSize;
        //Adjust the length of the thrust vector line to indicate thruster strength:
        lineScale = (maxLineSize / Mathf.Log(100)) * Mathf.Log((float) maxThrust) + minLineSize;
        lineScale *= lastFrameParticleLifeScalar;
        if (lineScale < minLineSize)
        {
            lineScale = minLineSize;
        }
    }

    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            // Unity has a built-in shader that is useful for drawing
            // simple colored things.
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            // Turn on alpha blending
            lineMaterial.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int) UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    //Will be called after all regular rendering is done
    public void OnRenderObject()
    {
        if ((thrusterNormalVisible) && (!inSpriteMode))
        {
            CreateLineMaterial();
            // Apply the line material
            lineMaterial.SetPass(0);

            GL.PushMatrix();
            // Set transformation matrix for drawing to
            // match our transform
            GL.MultMatrix(transform.localToWorldMatrix);

            // Draw lines
            GL.Begin(GL.LINES);
            // Vertex colors change from red to green
            GL.Color(Color.yellow);
            // One vertex at transform position
            GL.Vertex3(0, 0, 0);
            // Another vertex at x
            GL.Vertex3(0, 0, -lineScale);
            GL.End();
            GL.PopMatrix();
        }
    }

    public void ToggleThrusterGeometry(bool isOn)
    {
        thrusterGeomOn = isOn;
    }

    public void ToggleThrusterNormals(bool isOn)
    {
        thrusterNormalVisible = isOn;
    }

    /// <summary>
    /// This receives a BroadcastMessage from the parent Spacecraft when
    /// going into sprite mode. Don't delete. 
    /// </summary>
    /// <param name="spriteOn">True if attached spacecraft is in sprite mode</param>
    public void ConfigureHUDForSpriteMode(bool spriteOn)
    {
        inSpriteMode = spriteOn;
    }

    private void SetThrusterColorFromMessage(VizMessage.Types.Thruster myMsg)
    {
        minThrustColor = ThrusterUtilities.GetDefaultThrusterColor();
        useDefaultColor = true;
        Color thrusterColor = BuildThrusterColor(myMsg);
        if (thrusterColor != new Color(0, 0, 0, 0))
        {
            minThrustColor = thrusterColor;
            useDefaultColor = false;
        }
        maxThrustColor = minThrustColor;
    }

    public void UpdateDefaultThrusterColor(Color newDefault)
    {
        if (useDefaultColor)
        {
            maxThrustColor = newDefault;
            minThrustColor = newDefault;
        }
    }

    private Color BuildThrusterColor(VizMessage.Types.Thruster myMsg)
    {
        Color newColor = new Color(0,0,0,0);
        if (myMsg.Color.Count >= 3)
        {
            newColor = new Color(myMsg.Color[0] / 255f, myMsg.Color[1] / 255f,
                myMsg.Color[2] / 255f, 1f);

            if (myMsg.Color.Count > 3)
            {
                newColor.a = myMsg.Color[3] / 255f;
            }
        }
        return newColor;
    }


    public void UpdateThrusterGeometryCone()
    {
        thrusterCone.transform.localScale = Vector3.one * (1f / (float)CelestialBodyStateUtilities.SpacecraftLocalViewScale);
    }
}