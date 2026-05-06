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
/// Draws a line from one scenario object (FromBody) in the direction of another scenario object (ToBody)
/// </summary>
public class DrawPointingVector : MonoBehaviour {

    public float myScale = 5;
    public GameObject fromBody;
    public GameObject toBody;
    private Color myLineColor;
    private Vector3 lineVector = Vector3.one;
    private bool drawLine = true;
    private LineRenderer myLineRenderer;
    private bool useLineRenderer;

    static Material lineMaterial;
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
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // Turn backface culling off
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            // Turn off depth writes
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }
	

    public void InitializePointingVector(GameObject lineStartBody, GameObject lineEndBody, Color color, bool lineRendererOn){
		
        myLineRenderer = GetComponent<LineRenderer>();
        fromBody = lineStartBody;
        toBody = lineEndBody;
        SetColor(color);
        UpdateLineRendererSettings(lineRendererOn);
        transform.gameObject.name = $"{fromBody.name} to {toBody.name} pointing vector";
    }

    public Color GetLineColor(){
        return myLineColor;
    }

    public void SetColor(Color newColor){
        myLineColor = newColor;
        myLineRenderer.material.color = newColor;
        myLineRenderer.startColor = newColor;
        myLineRenderer.endColor = newColor;
    }

    void Update(){
        if( (fromBody!=null)&&(toBody!=null)){
            lineVector = Vector3.Normalize(toBody.transform.position - fromBody.transform.position);
            if (fromBody.CompareTag("Spacecraft"))
            {
                myScale = fromBody.GetComponent<SpacecraftController>().meshDimension *5f;
            }
            else
            {
                myScale = fromBody.transform.localScale.x*5f;
            }

            if (CelestialBodyStateUtilities.ViewIsSpacecraftLocal)
            {
                myScale *= (float) CelestialBodyStateUtilities.SpacecraftLocalViewScale;
            }
            transform.eulerAngles = Vector3.zero;
            transform.localPosition = Vector3.zero;
            if (useLineRenderer)
            {
                Vector3 myPosition = transform.position;
                myLineRenderer.SetPositions(new[]{myPosition, myPosition+myScale*lineVector});
            }
            else
            {
                myLineRenderer.enabled = false;
            }
            drawLine = true;
        }else{
            drawLine = false;
        }
    }

    // Will be called after all regular rendering is done
    public void OnRenderObject()
    {
        float lineScale = myScale/ transform.parent.transform.localScale.x;			
		
        if (drawLine){
            CreateLineMaterial();
            // Apply the line material
            GL.PushMatrix();
            lineMaterial.SetPass(0);
            // Set transformation matrix for drawing to
            // match our transform
            GL.MultMatrix(transform.localToWorldMatrix);

            // Draw lines
            GL.Begin(GL.LINES);
            // Vertex colors change from red to green
            GL.Color(myLineColor);
            // One vertex at transform position
            GL.Vertex3(0, 0, 0);
            // Another vertex at x
            GL.Vertex3(lineScale*lineVector.x,lineScale*lineVector.y,lineScale*lineVector.z);

            GL.End();
            GL.PopMatrix();
        }
    }

    public void UpdateLineRendererSettings(bool isOn)
    {
        useLineRenderer = isOn;
        myLineRenderer.enabled= isOn;
        if (isOn)
        {
            float newWidth = SpacecraftStateUtilities.GetCurrentSpacecraftOrbitLineConstant() * (float) PersistentUserSettings.persistentSettingsFromLastSave.LinesAndFramesLineWidth;
            myLineRenderer.startWidth = newWidth;
            myLineRenderer.endWidth = newWidth;
        }
    }
}