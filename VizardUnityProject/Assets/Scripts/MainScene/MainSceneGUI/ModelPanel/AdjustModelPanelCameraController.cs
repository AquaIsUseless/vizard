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
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// Camera controller for the AdjustModelPanel camera, provides view
/// to user of the model being modified in the panel's camera view.
/// Handles user input to moving the camera around
/// <remarks>Camera can only see objects in layer 12: CustomModel</remarks>
/// </summary>
public class AdjustModelPanelCameraController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Camera View Subpanel GUI")]
    public Button ZoomInButton;
    public Button ZoomOutButton;
    public GameObject myCamera;
    public GameObject myCameraImage;
    public RectTransform cameraImageRect;
    
    private AdjustModelPanelMethods panelMethods;
    [HideInInspector] public GameObject referenceCube;
    private GameObject originCS;
    private bool moveCamera;
    private bool mouseIsDownOnCameraImage;
    private bool lastFrameLeftMouseButtonDown;
    private float timeDownMark = 300000000f;
    private readonly float rotateSpeed = 5.0f; //Speed the camera rotates about target on drag
    private Vector3 cameraStartPosition;
    private Vector3 cameraEndPosition;
    private float startTime;
    void Start()
    {
        panelMethods = GetComponent<AdjustModelPanelMethods>();
        ZoomInButton.onClick.AddListener(ZoomInToTarget);
        ZoomOutButton.onClick.AddListener(ZoomOutFromTarget);
    }

    private void OnEnable()
    {
        if (panelMethods == null)
        {
            panelMethods = GetComponent<AdjustModelPanelMethods>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (moveCamera)
        {
            MoveCamera();
        }

        if (mouseIsDownOnCameraImage)
        {
            if (lastFrameLeftMouseButtonDown)
            {
                if ((Time.time - timeDownMark) > 0.2f)
                {
                    // Rotate about target after waiting long enough to make sure it's not a double-click
                    DragToPanAboutTarget();
                }
            }
            else
            {
                lastFrameLeftMouseButtonDown = true;
                timeDownMark = Time.time;
            }
        }
    }
    
    public void AddCameraLightAndReferenceCube()
    {
        //Add a BSK reference cube to aid scaling model
        if (referenceCube == null)
        {
            referenceCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            referenceCube.GetComponent<MeshRenderer>().material =
                ((Material) Resources.Load("Materials/Spacecraft_HUD/TransparentMat"));
            referenceCube.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, .15f);
            referenceCube.layer = 12;
            referenceCube.SetActive(false);
        }

        //Add a BSK coordinate system to aid orientation of model
        if (originCS == null)
        {
            originCS = Instantiate(Resources.Load("Prefabs/OriginCameraTarget") as GameObject);
            SpacecraftStateUtilities.MoveEntireGameObjectToLayer(originCS.transform,12);
            originCS.GetComponentInChildren<DrawAxes>().SetUpForModelTuningPanel();
        }

        //Add a camera to look at only the imported model
        if (myCamera == null)
        {
            myCamera = Instantiate(Resources.Load("Prefabs/AdjustModelCamera") as GameObject);
            myCameraImage.GetComponent<CameraViewImageMethods>()
                .InitializeCameraViewImage(myCamera.GetComponent<Camera>(), true, 285, 335,24,false);
            // Only render objects in the custom model layer
            myCamera.GetComponent<Camera>().cullingMask = 1 << 12;
        }

        panelMethods.CalculateImportedModelBounds();
        myCamera.transform.localPosition = CalculateCameraPosition();


        //Add a directional light to make it easy to see the model while tuning it
        GameObject lightGameObject = new GameObject("ModelLighting");
        lightGameObject.transform.SetParent(myCamera.transform);
        Light myLight = lightGameObject.AddComponent<Light>();
        myLight.type = LightType.Directional;
        myLight.intensity = 0.5f;
        myLight.cullingMask = 1 << 12;
    }
    


    private Vector3 CalculateCameraPosition()
    {
        float myMax = 2 * Mathf.Max(panelMethods.completeBounds.extents[0], panelMethods.completeBounds.extents[1], panelMethods.completeBounds.extents[2]);
        return new Vector3(0, 0,
            -(myMax * 1.3f) / Mathf.Tan(myCamera.GetComponent<Camera>().fieldOfView * Mathf.PI / 360f));
    }

    private void MoveCamera()
    {
        float interValue = Mathf.Clamp((Time.time - startTime) * 2f, 0.0f, 1.0f);
        myCamera.transform.localPosition = Vector3.Lerp(cameraStartPosition, cameraEndPosition, interValue);

        if (interValue >= 1.0f)
        {
            moveCamera = false;
        }

        myCamera.transform.LookAt(panelMethods.completeBounds.center);
    }
    
    private void ZoomInToTarget()
    {
        Vector3 vectorToTarget = originCS.transform.position - myCamera.transform.position;
        Vector3 cameraZoomChange = vectorToTarget/ 10.0f;

        myCamera.transform.position += cameraZoomChange;
        myCamera.transform.LookAt(originCS.transform);
    }

    private void ZoomOutFromTarget()
    {
        Vector3 vectorToTarget = originCS.transform.position - myCamera.transform.position;
        Vector3 cameraZoomChange = vectorToTarget/ 10.0f;

        if (cameraZoomChange == Vector3.zero)
        {
            cameraZoomChange = transform.forward.normalized;
        }

        myCamera.transform.position -= cameraZoomChange;
        myCamera.transform.LookAt(originCS.transform);
    }
    
    public void OnPointerDown(PointerEventData data)
    {
        cameraImageRect.SetAsLastSibling();
        mouseIsDownOnCameraImage = true;
    }

    public void OnPointerUp(PointerEventData data)
    {
        lastFrameLeftMouseButtonDown = false;
        mouseIsDownOnCameraImage = false;
    }
    private void DragToPanAboutTarget()
    {
        float deltaYaw = Input.GetAxis("Mouse X") * rotateSpeed;
        float deltaPitch = -Input.GetAxis("Mouse Y") * rotateSpeed;
        myCamera.transform.RotateAround(panelMethods.modelToTune.transform.position, transform.up, deltaYaw);
        myCamera.transform.RotateAround(panelMethods.modelToTune.transform.position, transform.right, deltaPitch);
    }

    public void UpdateCameraPosition()
    {
        cameraStartPosition = myCamera.transform.localPosition;
        cameraEndPosition = CalculateCameraPosition();
        if (cameraStartPosition != cameraEndPosition)
        {
            startTime = Time.time;
            moveCamera = true;
        }
    }

    public void DestroyCameraObjects()
    {
        Destroy(referenceCube);
        Destroy(originCS);
        Destroy(myCamera);
    }
}
