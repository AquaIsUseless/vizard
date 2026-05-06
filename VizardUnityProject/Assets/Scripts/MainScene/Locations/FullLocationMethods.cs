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
using UnityEngine;
using VizProtobufferMessage;
/// <summary>
/// Adds Full Location features to a Location (including
/// line of sight lines and adding antenna collider cone for fov/range
/// and station collider).
/// </summary>
public class FullLocationMethods : MonoBehaviour
{
    private DrawLocationMarker parentLocation;
    private int parentBodyIndex;
    private bool parentBodyIsSpacecraft;
    public GameObject visibleGroup;
    public GameObject visibleCone;
    public GameObject visibleScoop;
    public GameObject invisibleCone;
    

    private float FOV = 160;
    private float stationRange = -1f;

    public List<GameObject> antennaInView = new List<GameObject>();
    public List<Vector3> antennasInRangeCoords;
    private float distanceToFarthestAntenna;
    private Vector3 scaleToGetCorrectFOV;
    private float viewableThreshold = 10000f;
    private bool userProvidedRange;
    private double[] locationOriginBSKCS = {0, 0, 0};

    public bool entireSpacecraftIsTarget = true;
    
    private readonly int layerMask = ((1 << 7) | (1 << 9) | (1 << 11) | (1 << 14)); //7=UnlitSC, 9=TrueBodySizeColliders, 11 = Spacecraft, 14 = Antenna (using this instead of LabelMarkers to get double work out of collider)

    static Material lineMaterial;

    [HideInInspector] public bool useLineRenderers = true;
    [HideInInspector] public float currentWidth = 0.0625f;
    private List<LineRenderer> lineRenderers = new List<LineRenderer>();

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

    void FixedUpdate()
    {
        if (VizardGUISettings.ShowStationCommunicationLines)
        {
            DrawLinesToAntennaInRange();
        }
        
        if (parentBodyIsSpacecraft)
        {
            UpdateVisibleConeAppearance();
        }
    }

    public void UpdateFullLocationFromMessages(VizMessage.Types.Location myMsg, float parentBodyRadius)
    {
        if (myMsg.Color.Count >= 3)
        {
            Color newColor = new Color(myMsg.Color[0] / 255f, myMsg.Color[1] / 255f,
                myMsg.Color[2] / 255f, 1f);
            if (myMsg.Color.Count >= 4)
            {
                newColor.a = myMsg.Color[3] / 255f;
            }
        
            SetColor(newColor);
        }
        
        
        float fov = (float) myMsg.FieldOfView;
        float userRange = (float) myMsg.Range;
        if ((fov < 0.0001) || (fov > 179.9999))
        {
            fov = FOV;
        }
        
        if (userRange <= 0)
        {
            userRange = -1;
        }
        
        SetFieldOfViewAndVisibleConeSize(fov, userRange, parentBodyRadius);
        SetInvisibleColliderConeSize();

        ToggleVisibleConeElements(VizardGUISettings.ShowStationCone);
    }

    public void InitializeFullLocation(VizMessage.Types.Location myMsg, float parentBodyRadius, bool onSpacecraft, Color stationColor, bool targetAntennasOnly = false)
        //(float parentBodyRadius, double[] locationOrigin,Vector3 normal, float fieldOfView, Color color, float range, bool targetAntennasOnly = false)
    {
        entireSpacecraftIsTarget = !targetAntennasOnly;
        parentBodyIsSpacecraft = onSpacecraft;
        parentLocation = transform.parent.GetComponent<DrawLocationMarker>();
        if (parentBodyIsSpacecraft) 
        {
            parentBodyIndex = parentLocation.parentBody.GetComponent<SpacecraftController>().spacecraftIndex;
        }
        else
        {
            parentBodyIndex = parentLocation.parentBody.CompareTag("Sun") ? CelestialBodyStateUtilities.SunIndex : parentLocation.parentBody.GetComponent<PlanetController>().planetIndex;
        }

        locationOriginBSKCS = parentLocation.locationOriginBSKCS;
        SetColor(stationColor);
        SetFieldOfViewAndVisibleConeSize((float) myMsg.FieldOfView, (float) myMsg.Range, parentBodyRadius);
        SetInvisibleColliderConeSize();
        

        ToggleVisibleConeElements(VizardGUISettings.ShowStationCone);

        if (!VizardGUISettings.UseShellLighting)
        {
            VizardGUISettings.SetShellLighting(true);
        }
        UpdateLineRendererSettings(PersistentUserSettings.persistentSettingsFromLastSave.UseLineRenderersForTargetLinesAndFrames==1);
    }

    public void SetColor(Color newColor)
    {
        visibleCone.GetComponent<MeshRenderer>().material.color = new Color(newColor.r, newColor.g, newColor.b, 0.2f);
        if (stationRange > 0)
        {
            visibleScoop.GetComponent<MeshRenderer>().material.color = new Color(newColor.r, newColor.g, newColor.b, 0.2f);
        }
    }
    



    private void SetFieldOfViewAndVisibleConeSize(float fieldOfView, float range, float parentBodyRadius)
    {
        FOV = fieldOfView;
        stationRange = range>0?range:-1f;
        //stationRange = -1f; //TEST
        float visibleRadius = 0.4f;
        if (parentBodyIsSpacecraft)
        {
            visibleRadius = 10.0f;
            if (range > 0)
            {
                visibleRadius = range;
            }
        }

        float visibleConeHeight = visibleRadius;
        if (stationRange > 0)
        {
            visibleRadius = stationRange / parentBodyRadius;
            visibleConeHeight = visibleRadius * Mathf.Cos(FOV / 2 * Mathf.PI / 180);
            int numRingsToCreate = (int) FOV / 5;
            if (numRingsToCreate <= 0)
            {
                numRingsToCreate = 1;
            }

            CSSUtilities.BuildHemisphereMesh(visibleScoop, numRingsToCreate, 36, FOV, true, true, visibleRadius);
            visibleScoop.GetComponent<MeshRenderer>().material.color =
                visibleCone.GetComponent<MeshRenderer>().material.color;
            visibleScoop.SetActive(true);
            userProvidedRange = true;
        }
        else
        {
            visibleScoop.SetActive(false);
        }

        scaleToGetCorrectFOV = new Vector3(Mathf.Tan(FOV / 2 * Mathf.PI / 180), Mathf.Tan(FOV / 2 * Mathf.PI / 180), 1);
        visibleCone.transform.localScale = scaleToGetCorrectFOV * visibleConeHeight;
    }

    private void UpdateVisibleConeAppearance()
    {
        if (userProvidedRange)
        {
            if (CelestialBodyStateUtilities.ViewIsLocal)
            {
                if (CelestialBodyStateUtilities.ViewIsSpacecraftLocal)
                {
                    visibleGroup.transform.localScale = Vector3.one;
                }
                else
                {
                    visibleGroup.transform.localScale =
                        Vector3.one * (1f / (float) CelestialBodyStateUtilities.LocalPlanetViewScale);
                }
            }
            else
            {
                visibleGroup.transform.localScale =
                    Vector3.one * (1f / (float) CelestialBodyStateUtilities.HelioCenteredViewScale);
            }
        }
        else
        {
            visibleGroup.transform.localScale = Vector3.one;
        }
    }

    private void SetInvisibleColliderConeSize()
    {
        double[] myPosition = GetRawAntennaPosition();

        for (int i = 0; i < SpacecraftStateUtilities.SpacecraftList.Count; i++)
        {
            distanceToFarthestAntenna = Mathf.Max(distanceToFarthestAntenna,
                1.5f * ((float) OrbitVectorMath.Magnitude(
                    (OrbitVectorMath.Subtract(SpacecraftStateUtilities.GetAbsSpacecraftPositionUnityCS(i),
                        myPosition)))));
        }

        if (parentBodyIsSpacecraft)
        {
            if (distanceToFarthestAntenna > viewableThreshold)
            {
                invisibleCone.transform.localScale = scaleToGetCorrectFOV *
                                                     (viewableThreshold + 10 * Mathf.Log10(distanceToFarthestAntenna));
            }
            else
            {
                invisibleCone.transform.localScale = scaleToGetCorrectFOV * (distanceToFarthestAntenna);
            }
        }
        else
        {
            invisibleCone.transform.localScale = scaleToGetCorrectFOV *
                                                 ((Mathf.Min(distanceToFarthestAntenna,
                                                      viewableThreshold +
                                                      10 * Mathf.Log10(distanceToFarthestAntenna))) /
                                                  parentLocation.parentBody.transform.localScale.x);
        }
    }

    public void AntennaEnteredStationRange(GameObject sc)
    {
        if (sc != parentLocation.gameObject)
        {
            if (!antennaInView.Contains(sc))
            {
                //Debug.Log($"I am {transform.parent.name} and I Added {sc.name}");
                antennaInView.Add(sc);
            }
        }
    }

    public void AntennaExitedStationRange(GameObject sc)
    {
        if (antennaInView.Contains(sc))
        {
            antennaInView.Remove(sc);
            //Debug.Log($"$I am {transform.parent.name} and I Removed {sc.name}");
        }
    }

    private void DrawLinesToAntennaInRange()
    {
        antennasInRangeCoords = new List<Vector3>();
        double[] stationPositionInertial = GetRawAntennaPosition();
        foreach (GameObject antBody in antennaInView)
        {
            if (antBody != null)
            {
                if (stationRange >= 0)
                {
                    double[] antRawPosition;
                    FullLocationMethods antFullLocationMethods = antBody.GetComponentInChildren<FullLocationMethods>();
                    if (antFullLocationMethods != null)
                    {
                        antRawPosition = GetRawAntennaPosition();
                    }
                    else
                    {
                        // Must be a spacecraft default antenna
                        Vector3 antennaOffset = antBody.GetComponent<SpacecraftController>().antennaCollider
                            .transform.localPosition;
                        int antBodyIndex = antBody.GetComponent<SpacecraftController>().spacecraftIndex;
                        double[] parentRawPosition =
                            SpacecraftStateUtilities
                                .GetAbsSpacecraftPositionUnityCS(antBodyIndex); //This is in Unity CS
                        antRawPosition = OrbitVectorMath.Add(parentRawPosition,
                            new double[] {antennaOffset.x, antennaOffset.y, antennaOffset.z});
                    }

                    double[] rangeToTargetV =
                        OrbitVectorMath.Subtract(antRawPosition, stationPositionInertial); //Unity CS
                    double rangeToTarget = OrbitVectorMath.Magnitude(rangeToTargetV);
                    if (rangeToTarget > stationRange)
                    {
                        break;
                    }
                }
                if (CheckAntennaLineOfSight(antBody))
                {
                    antennasInRangeCoords.Add(antBody.transform.position);
                }
            }
            else
            {
                CleanAntennaInViewList();
            }
        }
      
        UpdateLineRenderers();
        
    }

    private void UpdateLineRenderers()
    {
        if (useLineRenderers)
        {
            Color myColor = visibleCone.GetComponent<MeshRenderer>().material.color;
            myColor.a = 1f;
            while (lineRenderers.Count < antennasInRangeCoords.Count)
            {
                GameObject newLine = Instantiate(Resources.Load("Prefabs/SpacecraftHUD/LineObject") as GameObject, transform);
                LineRenderer newLineRenderer = newLine.GetComponent<LineRenderer>();
                newLineRenderer.startWidth = currentWidth;
                newLineRenderer.endWidth = currentWidth;
                newLineRenderer.startColor = myColor;
                newLineRenderer.endColor = myColor;
                newLineRenderer.material.color = myColor;
                lineRenderers.Add(newLineRenderer);
            }

            Vector3 myPosition = transform.position;
            for (int i = 0; i < antennasInRangeCoords.Count; i++)
            {
                lineRenderers[i].enabled = true;
                lineRenderers[i].SetPositions(new []{myPosition, antennasInRangeCoords[i]});
            }

            for (int i = antennasInRangeCoords.Count; i<lineRenderers.Count; i++)
            {
                lineRenderers[i].SetPositions(new []{Vector3.zero, Vector3.zero});
                lineRenderers[i].enabled = false;
            }
        }
    }

    public double[] GetRawAntennaPosition()
    {
        double[] antRawPositionUnity = OrbitVectorMath.CalculateStationOffsetJ2000_UnityCS(parentBodyIndex,
            parentBodyIsSpacecraft, locationOriginBSKCS);

        return antRawPositionUnity; //Returned in Unity CS!
    }

    // Will be called after all regular rendering is done
    public void OnRenderObject()
    {
        if (VizardGUISettings.ShowStationCommunicationLines)
        {
            CreateLineMaterial();
            // Apply the line material

            GL.PushMatrix();
            lineMaterial.SetPass(0);

            // Draw lines
            GL.Begin(GL.LINES);
            GL.Color(visibleCone.GetComponent<MeshRenderer>().material.color);
            foreach (Vector3 endPt in antennasInRangeCoords)
            {
                // One vertex at transform position
                Vector3 transform1 = transform.position;
                GL.Vertex3(transform1.x, transform1.y, transform1.z);
                // Another vertex at x
                GL.Vertex3(endPt.x, endPt.y, endPt.z);
            }

            GL.End();
            GL.PopMatrix();
        }
    }

    private bool CheckAntennaLineOfSight(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;
        float maxDistance = direction.magnitude * 1.2f;

        if (Physics.Raycast(transform.position, direction, out var hit, maxDistance, layerMask))
        {
            if ((hit.collider.gameObject.layer == 14))
            {
                if (hit.collider.gameObject.transform.parent.gameObject != parentLocation.parentBody)
                {
                    return true;
                }
                return false;
            }
            if ((entireSpacecraftIsTarget) && (hit.collider.gameObject.transform.parent.gameObject == target))
            {
                return true;
            }
            return false;
        }
        return false;
    }

    public void CleanAntennaInViewList()
    {
        List<GameObject> remainingAntenna = new List<GameObject>();
        foreach (GameObject antBody in antennaInView)
        {
            if (antBody != null)
            {
                remainingAntenna.Add(antBody);
            }
        }
    
        antennaInView = remainingAntenna;
    }
    
    public void UpdateLineRendererSettings(bool isOn)
    {
        
        useLineRenderers = isOn;
        currentWidth = SpacecraftStateUtilities.GetCurrentSpacecraftOrbitLineWidth()
                       * (float) PersistentUserSettings.persistentSettingsFromLastSave.LinesAndFramesLineWidth /
                       (float) PersistentUserSettings.persistentSettingsFromLastSave.SpacecraftOrbitLineWidth;
        if (isOn)
        {
            foreach (LineRenderer line in lineRenderers)
            {
                line.startWidth = currentWidth;
                line.endWidth = currentWidth;
            }
        }
        else
        {
            foreach (LineRenderer line in lineRenderers)
            {
                line.enabled = false;
            }
        }
    }

    public void ToggleVisibleConeElements(bool isOn)
    {
        visibleCone.SetActive(isOn);
        visibleScoop.SetActive(isOn);
    }
}