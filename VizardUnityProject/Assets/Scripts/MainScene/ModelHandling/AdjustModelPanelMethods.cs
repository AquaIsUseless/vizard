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
using UnityEngine.UI;
using TMPro;
#if USE_NATIVE_FILE_BROWSER
using Crosstales.FB;
#endif

/// <summary>
/// Supports user modifying the appearance of an imported obj model
/// </summary>
public class AdjustModelPanelMethods : MonoBehaviour
{
    [HideInInspector] public GameObject modelToTune;
    [Header("Panel GUI")]
    public TextMeshProUGUI panelName;
    public TMP_InputField nameField;
    public RectTransform entirePanelRect;
    
    [Header("Model Transform Input")]
    public TMP_InputField xOffset;
    public TMP_InputField yOffset;
    public TMP_InputField zOffset;
    public TMP_InputField xRotation;
    public TMP_InputField yRotation;
    public TMP_InputField zRotation;
    public TMP_InputField xScale;
    public TMP_InputField yScale;
    public TMP_InputField zScale;
    
    [Header("Reference Cube")]
    public Toggle showReferenceCubeToggle;
    public Toggle autoUpdateCubeToggle;
    public TMP_InputField xRefBox;
    public TMP_InputField yRefBox;
    public TMP_InputField zRefBox;
    public Bounds completeBounds;
    public TextMeshProUGUI boundingBoxUnits;
    
    [Header("Model Extents and Center")]
    public TextMeshProUGUI modelCenterText;
    public TextMeshProUGUI modelExtentsText;
    public TextMeshProUGUI bodyRadiusText;
    public TextMeshProUGUI offsetUnits;
    
    [Header("Error Text ")]
    public TextMeshProUGUI errorText;
    
    [Header("Submit Buttons")]
    public Button applySettingsButton;
    public Button cancelButton;
    public Button closeButton;
    
    [Header("Texture Subpanel Components")]
    public Toggle showTexturePanelToggle;
    public GameObject textureSubpanel;
    public Button previewMaterialButton;
    public Button textureSelectButton;
    public Button normalSelectButton;
    public TextMeshProUGUI textureFilepath;
    public TextMeshProUGUI normalFilepath;
    public TMP_InputField normalMapHeight;

    [Header("Required Panels")]
    public VizardFileBrowser fileChooser;
    public GameObject modelInventoryPanel;
    
    private bool needUpdate;
    private GameObject selectedToggle;
    private float parentRadius = -1;
    private string units = "";
    private bool boxAutoUpdateOn = true;
    private AdjustModelPanelCameraController camController;

    
    void Start()
    {
        entirePanelRect = transform.GetComponent<RectTransform>();
        camController = transform.GetComponent<AdjustModelPanelCameraController>();
        applySettingsButton.onClick.AddListener(ApplyPanelSettingsAndAddToModelInventory);
        cancelButton.onClick.AddListener(CancelAndReturnToModelInventory);
        closeButton.onClick.AddListener(CancelAndReturnToModelInventory);
        showTexturePanelToggle.onValueChanged.AddListener(ToggleTextureSubpanel);
        previewMaterialButton.onClick.AddListener(PreviewCustomMaterial);
        textureSelectButton.onClick.AddListener(SelectTextureFile);
        normalSelectButton.onClick.AddListener(SelectNormalMapFile);
        normalMapHeight.onValueChanged.AddListener(CheckNormalMapInput);

        foreach (TMP_InputField inputField in gameObject.GetComponentsInChildren<TMP_InputField>())
        {
            inputField.onEndEdit.AddListener(SetUpdateFlag);
        }

        showReferenceCubeToggle.onValueChanged.AddListener(ToggleReferenceCube);
        autoUpdateCubeToggle.onValueChanged.AddListener(ToggleAutoUpdate);
        errorText.text = "";
    }

    void OnEnable()
    {
        if (camController == null)
        {
            camController = GetComponent<AdjustModelPanelCameraController>();
        }

        transform.SetAsLastSibling();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (needUpdate)
        {
            UpdateModelToCurrentSettings();
            needUpdate = false;
        }
    }

    private void SetUpdateFlag(string newString)
    {
        if (!needUpdate)
        {
            needUpdate = true;
        }
    }

    public void ConfigurePanelView(GameObject loadedObject, GameObject inventoryToggle = null, bool isSimObject = false)
    {
        selectedToggle = inventoryToggle;
        bodyRadiusText.color = Color.white;
        bodyRadiusText.text = "Model units will be applied as meters for spacecraft, kilometers for celestial bodies.";
        units = "";
        xScale.readOnly = false;
        yScale.readOnly = false;
        zScale.readOnly = false;


        if (isSimObject)
        {
            ConfigurePanelViewForObject(loadedObject);
        }
        else
        {
            ConfigurePanelViewForModel(loadedObject);
        }

        camController.AddCameraLightAndReferenceCube();
        camController.myCameraImage.GetComponent<CameraViewImageMethods>().ApplyPanelResize(entirePanelRect.sizeDelta);

        DisplayCurrentModelTransformSettings();
        UpdateBoundsDisplay();
        UpdateBoundingBox();
    }

    private void ConfigurePanelViewForModel(GameObject loadedObject)
    {
        panelName.text = "Adjust Model";

        if (selectedToggle == null) //If model was just loaded by the OBJ importer and user is reviewing the import
        {
            modelToTune = loadedObject;
            modelToTune.transform.localScale = Vector3.one;
        }
        else //If modifying a default model or previously imported and reviewed model
        {
            //Make a copy to apply changes to, just in case
            if (selectedToggle.GetComponent<InventoryToggle>().myGUIObject != null) //Non primitive models
            {
                modelToTune = Instantiate(loadedObject, VizardGUISettings.PlaybackManager.transform, true);
                modelToTune.name = loadedObject.name;
                //If it's an object, you need to leave it's model on during tuning, if it's a model turn it off
                loadedObject.SetActive(false);
            }
            else //primitive models
            {
                modelToTune = loadedObject;
                modelToTune.name = selectedToggle.GetComponent<InventoryToggle>().modelName;
            }

            if (modelToTune.GetComponent<ModelBounds>() == null)
            {
                Debug.Log("Adding model bounds script in Load Model to Panel.");
                ModelBounds myBounds = modelToTune.AddComponent<ModelBounds>();
                bool useBoxCollider = (modelToTune.name != "Sphere");
                myBounds.SetupUnitBoundsForModel(modelToTune);
                myBounds.SetupModelBoundsWithModel(useBoxCollider, modelToTune);
            }
        }

        SpacecraftStateUtilities.MoveEntireGameObjectToLayer(modelToTune.transform,
            12); //Model will only be visible in panel's camera

        nameField.text = modelToTune.name;

        DisplayCurrentModelTransformSettings();

        CalculateImportedModelBounds();

        offsetUnits.text = $"{units}";
        boundingBoxUnits.text = $"{units}";
    }

    private void UpdateBoundsDisplay()
    {
        modelCenterText.text = String.Format("Model Center:   ({0:0.000}, {1:0.000}, {2:0.000}) " + units,
            -completeBounds.center.z, completeBounds.center.x, completeBounds.center.y);
        modelExtentsText.text =
            String.Format("Model Extents:  (+/- {0:0.000}, +/- {1:0.000}, +/- {2:0.000}) " + units,
                completeBounds.extents.z, completeBounds.extents.x, completeBounds.extents.y);
    }

    private void ConfigurePanelViewForObject(GameObject loadedObject)
    {
        panelName.text = "Adjust Object";

        GameObject
            parentBody = loadedObject.transform.parent.gameObject; //Get top level spacecraft or planet game object

        if (parentBody.CompareTag("Spacecraft"))
        {
            ConfigurePanelViewForSpacecraftObject(loadedObject);
        }
        else
        {
            ConfigurePanelViewForPlanetObject(loadedObject, parentBody);
        }
    }

    private void ConfigurePanelViewForSpacecraftObject(GameObject loadedObject)
    {
        units = "meters";

        //Make a copy of model to apply changes to
        Vector3 positionOffset = loadedObject.transform.localPosition;
        Quaternion modelRotation = loadedObject.transform.localRotation;
        Vector3 modelScale = loadedObject.transform.localScale;

        modelToTune = Instantiate(loadedObject, VizardGUISettings.PlaybackManager.transform, true);
        modelToTune.transform.localPosition = positionOffset;
        modelToTune.transform.rotation = modelRotation;
        modelToTune.transform.localScale = modelScale;

        modelToTune.name = loadedObject.name;

        SpacecraftStateUtilities.MoveEntireGameObjectToLayer(modelToTune.transform,
            12); //Model will only be visible in panel's camera

        nameField.text = modelToTune.name;

        DisplayCurrentModelTransformSettings();

        CalculateImportedModelBounds();

        offsetUnits.text = $"({units})";
        boundingBoxUnits.text = $"({units})";
    }

    private void ConfigurePanelViewForPlanetObject(GameObject loadedObject, GameObject parentBody)
    {
        units = "kilometers";
        //Make a copy of model to apply changes to
        Vector3 positionOffset = loadedObject.transform.localPosition;
        Quaternion modelRotation = loadedObject.transform.localRotation;
        Vector3 modelScale = loadedObject.transform.localScale;

        modelToTune = Instantiate(loadedObject, VizardGUISettings.PlaybackManager.transform, true);
        modelToTune.transform.localPosition = positionOffset; //Worried about this part here
        modelToTune.transform.rotation = modelRotation;

        modelToTune.name = loadedObject.name;

        bodyRadiusText.transform.gameObject.SetActive(true);

        parentRadius = 0;

        if (!parentBody.CompareTag("Sun"))
        {
            parentRadius = CelestialBodyStateUtilities.GetCelestialBodyRadiusInMeters(parentBody
                .GetComponent<PlanetController>().bodyDictionaryKey);
        }
        else
        {
            parentRadius = CelestialBodyStateUtilities.GetCelestialBodyRadiusInMeters("sun");
        }

        if (parentRadius > 0)
        {
            xScale.readOnly = true;
            yScale.readOnly = true;
            zScale.readOnly = true;

            bodyRadiusText.color = new Color(1f, .5f, 0f, 1f);

            bodyRadiusText.text = "Celestial Body equatorial radius was set in messages for " + parentBody.name +
                                  " to: " + parentRadius / 1000 +
                                  " km.\nChanges to this model's scale will not alter the drawn size of " +
                                  parentBody.name + " which is driven by the specified radius.";
        }
        else
        {
            bodyRadiusText.color = new Color(1f, .5f, 0f, 1f);
            parentRadius = parentBody.GetComponent<PlanetController>().planetRadius;

            bodyRadiusText.text = "Celestial Body equatorial radius was not set in messages for " + parentBody.name +
                                  " and the scaled model extents as shown above will be assumed to be the desired size of this object in kilometers.";

            modelScale *= (parentRadius / 1000);
        }

        modelToTune.transform.localScale = modelScale;

        if (parentBody.CompareTag("Planet"))
        {
            try
            {
                modelToTune.GetComponent<AtmosphereShaderHelper>().atmosphereUpdatesOn = false;
                modelToTune.GetComponent<MeshRenderer>().material.SetFloat("_SphereRadius", 1);
            }
            catch
            {
                Debug.Log("No sphere radius property on this material.");
            }
        }

        SpacecraftStateUtilities.MoveEntireGameObjectToLayer(modelToTune.transform,
            12); //Model will only be visible in panel's camera

        nameField.text = modelToTune.name;

        DisplayCurrentModelTransformSettings();

        CalculateImportedModelBounds();

        offsetUnits.text = $"({units})";
        boundingBoxUnits.text = $"({units})";
    }

    private void UpdateModelToCurrentSettings()
    {
        try
        {
            modelToTune.transform.position = new Vector3(float.Parse(yOffset.text), float.Parse(zOffset.text),
                -float.Parse(xOffset.text));


            Quaternion modelRotation = Quaternion.identity;
            modelRotation.eulerAngles = new Vector3(float.Parse(yRotation.text), float.Parse(zRotation.text),
                -float.Parse(xRotation.text));
            modelToTune.transform.localRotation = modelRotation;

            modelToTune.transform.localScale = new Vector3(float.Parse(yScale.text), float.Parse(zScale.text),
                float.Parse(xScale.text));

            CalculateImportedModelBounds();

            camController.UpdateCameraPosition();

            UpdateBoundsDisplay();

            UpdateBoundingBox();

            modelToTune.name = nameField.text;
        }
        catch (FormatException)
        {
            Debug.Log("Current string can't be parsed into a float.");
        }
    }

    private void ToggleReferenceCube(bool showCube)
    {
        if (showCube)
        {
            camController.referenceCube.SetActive(true);
            UpdateBoundingBox();
        }
        else
        {
            camController.referenceCube.SetActive(false);
        }
    }

    private void ResizePanelAndCameraImage(bool showPanel)
    {
        Vector2 newSize = entirePanelRect.sizeDelta;
        if (showPanel)
        {
            if (newSize.y < 550)
            {
                newSize = new Vector2(newSize.x, 550);
            }
        }
        else
        {
            newSize = new Vector2(newSize.x, 420);
        }

        entirePanelRect.sizeDelta = newSize;
        ApplyPanelResize(newSize);
    }

    private void ToggleTextureSubpanel(bool showPanel)
    {
        errorText.text = "";
        textureSubpanel.SetActive(showPanel);
        normalMapHeight.text = "1.0";

        ResizePanelAndCameraImage(showPanel);
    }

    private void ApplyPanelSettingsAndAddToModelInventory()
    {
        bool safeToReturn = true;
        bool customMaterialApplied = false;
        if (showTexturePanelToggle.isOn)
        {
            safeToReturn = CreateCustomMaterial();
            customMaterialApplied = safeToReturn;
        }

        if (safeToReturn)
        {
            camController.DestroyCameraObjects();

            if (showTexturePanelToggle.isOn)
            {
                showTexturePanelToggle.isOn = false;
            }

            modelInventoryPanel.SetActive(true);
            if (selectedToggle == null)
            {
                modelInventoryPanel.GetComponent<ModelDirectoryPanelMethods>()
                    .AddModelToInventory(modelToTune, false, false);
            }
            else
            {
                if (selectedToggle.GetComponent<InventoryToggle>().inventoryType == "MODEL")
                {
                    modelToTune.name = selectedToggle.name; //Gets rid of the "(Clone)" that gets attached to the name
                    modelInventoryPanel.GetComponent<ModelDirectoryPanelMethods>()
                        .UpdateModel(modelToTune, selectedToggle);
                }
                else if (selectedToggle.GetComponent<InventoryToggle>().inventoryType == "SIMOBJECT")
                {
                    int layerToPutModelIn = 11; //For spacecraft
                    if (modelToTune.GetComponent<SpacecraftController>() == null)
                    {
                        layerToPutModelIn = 8; //For planets, moons, sun
                    }

                    SpacecraftStateUtilities.MoveEntireGameObjectToLayer(modelToTune.transform, layerToPutModelIn);

                    modelInventoryPanel.GetComponent<ModelDirectoryPanelMethods>()
                        .UpdateObject(modelToTune, selectedToggle, customMaterialApplied);
                }

                transform.gameObject.SetActive(false);
                selectedToggle = null;
                modelToTune = null;
            }
        }
    }

    public void CancelAndReturnToModelInventory()
    {
        camController.DestroyCameraObjects();
        Destroy(modelToTune);
        textureSubpanel.SetActive(false);
        if (selectedToggle != null)
        {
            modelInventoryPanel.SetActive(true);
        }

        transform.gameObject.SetActive(false);
    }

    public void CalculateImportedModelBounds()
    {
        //     completeBounds = modelToTune.GetComponent<ModelBounds>().
        completeBounds = SpacecraftStateUtilities.CalculateModelBounds(modelToTune);


        if (modelToTune.GetComponent<ModelBounds>() == null)
        {
            modelToTune.AddComponent<ModelBounds>();
        }

        ModelBounds boundsMethods = modelToTune.GetComponent<ModelBounds>();
        boundsMethods.SetupUnitBoundsForModel(modelToTune);
        boundsMethods.SetupModelBoundsWithModel(boundsMethods.useBoxCollider, modelToTune);
    }


    private void UpdateBoundingBox()
    {
        if (boxAutoUpdateOn)
        {
            xRefBox.text = (2 * completeBounds.extents.z).ToString("E3");
            yRefBox.text = (2 * completeBounds.extents.x).ToString("E3");
            zRefBox.text = (2 * completeBounds.extents.y).ToString("E3");
        }

        if (camController.referenceCube.gameObject.activeSelf)
        {
            camController.referenceCube.transform.position = completeBounds.center;
            camController.referenceCube.transform.localRotation = modelToTune.transform.localRotation;
            camController.referenceCube.transform.localScale = new Vector3(float.Parse(yRefBox.text),
                float.Parse(zRefBox.text),
                float.Parse(xRefBox.text));
        }
    }

    public void UseDefaults()
    {
        nameField.text = modelToTune.name;
        xOffset.text = "0.0";
        yOffset.text = "0.0";
        zOffset.text = "0.0";
        xRotation.text = "0.0";
        yRotation.text = "0.0";
        zRotation.text = "0.0";
        xScale.text = "1.0";
        yScale.text = "1.0";
        zScale.text = "1.0";
        normalMapHeight.text = "1.0";
    }

    private void DisplayCurrentModelTransformSettings()
    {
        nameField.text = modelToTune.name;
        var localPosition = modelToTune.transform.localPosition;
        xOffset.text = $"{(-localPosition.z):0.000}";
        yOffset.text = $"{localPosition.x:0.000}";
        zOffset.text = $"{localPosition.y:0.000}";
        Vector3 currentEulerAngles = (modelToTune.transform.localRotation).eulerAngles;
        xRotation.text = $"{(-currentEulerAngles[2]):0.000}";
        yRotation.text = $"{currentEulerAngles[0]:0.000}";
        zRotation.text = $"{currentEulerAngles[1]:0.000}";
        var localScale = modelToTune.transform.localScale;
        xScale.text = $"{(localScale.z):0.000}";
        yScale.text = $"{(localScale.x):0.000}";
        zScale.text = $"{(localScale.y):0.000}";
    }

    private void SelectTextureFile()
    {
#if USE_NATIVE_FILE_BROWSER
        if (Application.platform == RuntimePlatform.LinuxPlayer)
        {
            fileChooser.OpenFileBrowser(textureFilepath, "jpg|bmp|exr|gif|hdr|iff|pict|png|psd|tga|tiff");
        }
        else
        {
            textureFilepath.text = FileBrowser.Instance.OpenSingleFile("Choose texture", DataManager.LastDirectory,
                string.Empty,
                new string[] {"jpg", "bmp", "exr", "gif", "hdr", "iff", "pict", "png", "psd", "tga", "tiff"});
        }
#else
        fileChooser.OpenFileBrowser(textureFilepath, "jpg|bmp|exr|gif|hdr|iff|pict|png|psd|tga|tiff");
#endif

        DataManager.LastDirectory = textureFilepath.text;
    }

    private void SelectNormalMapFile()
    {
#if USE_NATIVE_FILE_BROWSER
        if (Application.platform == RuntimePlatform.LinuxPlayer)
        {
            fileChooser.OpenFileBrowser(normalFilepath, "jpg|bmp|exr|gif|hdr|iff|pict|png|psd|tga|tiff");
        }
        else
        {
            normalFilepath.text = FileBrowser.Instance.OpenSingleFile("Choose normal map", DataManager.LastDirectory,
                string.Empty,
                new string[] {"jpg", "bmp", "exr", "gif", "hdr", "iff", "pict", "png", "psd", "tga", "tiff"});
        }
#else
        fileChooser.OpenFileBrowser(normalFilepath, "jpg|bmp|exr|gif|hdr|iff|pict|png|psd|tga|tiff");
#endif

        DataManager.LastDirectory = normalFilepath.text;
    }

    private void PreviewCustomMaterial()
    {
        CreateCustomMaterial();
    }

    private bool CreateCustomMaterial()
    {
        string normalMapPath = normalFilepath.text;
        float normalMapHeightValue = float.Parse(this.normalMapHeight.text);
        if (normalMapHeightValue <= 0)
        {
            normalMapHeightValue = 1.0f;
            errorText.text = "Please set normal map height to value greater than zero.";
            normalMapHeight.text = normalMapHeightValue.ToString("E1");
        }

        if (!string.IsNullOrEmpty(textureFilepath.text))
        {
            Material customMaterial = modelInventoryPanel.GetComponent<ModelDirectoryPanelMethods>()
                .CreateCustomMaterial(textureFilepath.text, normalMapPath, normalMapHeightValue);
            if (customMaterial != null)
            {
                Renderer[] rr = modelToTune.transform.GetComponentsInChildren<Renderer>();
                foreach (Renderer r in rr)
                {
                    r.material = customMaterial;
                }

                return true;
            }
            else
            {
                errorText.text = "Error creating custom material. Please check provided filepath(s).";
                return false;
            }
        }
        else
        {
            errorText.text = "Please provide valid texture filepath or deselect Import Custom Texture";
            return false;
        }
    }

    /// <summary>
    ///This method must be implemented for any subpanel component that needs to do something when the panel is resized
    /// Do not delete or make private.
    /// </summary>
    /// <param name="newPanelDimensions">new panel extents</param>
    private void ApplyPanelResize(Vector2 newPanelDimensions)
    {
        int imageWidth = (int) newPanelDimensions.x - 395;
        int imageHeight = (int) newPanelDimensions.y - 45;

        GetComponentInChildren<CameraViewImageMethods>()
            .InitializeCameraViewImage(camController.myCamera.GetComponent<Camera>(), true, imageWidth, imageHeight);
    }

    private void ToggleAutoUpdate(bool isOn)
    {
        boxAutoUpdateOn = isOn;
        if (boxAutoUpdateOn)
        {
            UpdateBoundingBox();
        }
    }

    private void CheckNormalMapInput(string value)
    {
        errorText.text = float.Parse(value) <= 0 ? "Please set normal map height to value greater than zero." : "";
    }
}