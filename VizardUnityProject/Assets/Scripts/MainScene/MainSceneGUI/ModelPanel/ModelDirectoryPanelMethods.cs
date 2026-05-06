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
using UnityEngine.UI;
using VizProtobufferMessage;
using System.IO;
using TMPro;

/// <summary>
/// Sets up the model directory panel for the current
/// scenario and handles user input to allow importing models,
/// modifying them after import and applying them to scenario objects.
/// </summary>
public class ModelDirectoryPanelMethods : MonoBehaviour
{
    [Header("Panel GUI Components")] 
    public Button ApplyButton;
    public Button ModifyModelButton;
    public Button ModifyObjectButton;
    public GameObject ImportModelButton;
    public GameObject RevertToDefaultButton;
    public GameObject modelInventory;
    public ToggleGroup modelInventoryToggleGroup;
    public GameObject objectInventory;

    [Header("Adjust Model Panel")] public GameObject adjustModelPanel;
    public ImportModelMethods modelImporter;

    private List<GameObject> modelToggles = new List<GameObject>();
    private List<GameObject> objectToggles = new List<GameObject>();
    private List<GameObject> selectedObjects = new List<GameObject>();
    private List<GameObject> allSpacecraftToggles = new List<GameObject>();

    [HideInInspector] public GameObject selectedModelToggle;

    private bool firstClick = true;


    // Start is called before the first frame update
    void Start()
    {
        ApplyButton.onClick.AddListener(ApplyModelsToObjects);
        ModifyModelButton.onClick.AddListener(ModifyModel);
        ModifyObjectButton.onClick.AddListener(ModifyObject);
        RevertToDefaultButton.GetComponent<Button>().onClick.AddListener(RevertObjectToDefaultModel);
    }

    void OnEnable()
    {
        if (firstClick)
        {
            AddResourceModels();
            CreateSimulatedObjectsToggles();
            firstClick = false;
        }
    }


    private void AddResourceModels()
    {
        AddResourceModelToInventory("none", "Cube", true, false);
        AddResourceModelToInventory("none", "Cylinder", true, false);
        AddResourceModelToInventory("none", "Sphere", true, true);
        AddResourceModelToInventory("none", "Capsule", true, false);
        //AddResourceModelToInventory("Models/Triangle", "TriangularPrism",true, false);
        AddResourceModelToInventory("Models/BSKSAT_model", "bskSpacecraft", false, false);
        AddResourceModelToInventory("Models/CubeSAT_3U", "3U_CubeSat", false, false);
        AddResourceModelToInventory("Models/CubeSAT_6U", "6U_CubeSat", false, false);
        AddResourceModelToInventory("Models/BasicPlanetModel", "PlanetHighVertex", true, true);
    }

    private void AddResourceModelToInventory(string resourcePath, string modelName, bool isPrimitive, bool isSphere)
    {
        GameObject newModelToggle = Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericToggle") as GameObject);

        newModelToggle.name = modelName;
        newModelToggle.GetComponent<Toggle>().isOn = false;
        newModelToggle.AddComponent<InventoryToggle>();

        newModelToggle.GetComponent<InventoryToggle>().SetupToggleWithoutGUIObject(resourcePath, modelName,
            transform.gameObject, isPrimitive, isSphere, modelInventoryToggleGroup);

        AddToggleToModelInventory(newModelToggle);
    }

    public void AddModelToInventory(GameObject newModel, bool isPrimitive, bool isSphere)
    {
        GameObject newModelToggle = Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericToggle") as GameObject);

        newModelToggle.name = newModel.name;
        newModelToggle.GetComponent<Toggle>().isOn = false;
        newModelToggle.AddComponent<InventoryToggle>();

        newModelToggle.GetComponent<InventoryToggle>().SetupToggleWithGUIObject(newModel, transform.gameObject, "MODEL",
            isPrimitive, isSphere, modelInventoryToggleGroup);

        AddToggleToModelInventory(newModelToggle);
    }

    private void AddToggleToModelInventory(GameObject newModelToggle)
    {
        adjustModelPanel.SetActive(false);
        modelToggles.Add(newModelToggle);
        newModelToggle.transform.SetParent(modelInventory.transform);
        newModelToggle.GetComponent<RectTransform>().localScale = Vector3.one;

        newModelToggle.transform.SetAsFirstSibling();
        RevertToDefaultButton.transform.SetAsFirstSibling();
        ImportModelButton.transform.SetAsFirstSibling();


        if (modelToggles.Count > 6)
        {
            float width = modelInventory.GetComponent<RectTransform>().rect.width;
            modelInventory.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 25 * modelToggles.Count + 55);
        }

        int positionY = -5;
        foreach (Transform child in modelInventory.transform)
        {
            child.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(5, positionY);
            positionY -= 25;
        }

        ImportModelButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
        RevertToDefaultButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -25);
    }


    private void ApplyModelsToObjects()
    {
        if (selectedModelToggle != null)
        {
            GameObject selectedModel = GetSelectedModel();
            foreach (GameObject toggle in objectToggles)
            {
                if (toggle.name != "AllSpacecraftToggle")
                {
                    if (toggle.GetComponent<Toggle>().isOn)
                    {
                        //Body object to change
                        GameObject simBodyToChange = toggle.GetComponent<InventoryToggle>().myGUIObject;

                        GameObject modelCopyToUse = Instantiate(selectedModel);

                        //Update if the object's model is primitive or not
                        toggle.GetComponent<InventoryToggle>().isPrimitive =
                            selectedModelToggle.GetComponent<InventoryToggle>().isPrimitive;
                        toggle.GetComponent<InventoryToggle>().modelIsSphere =
                            selectedModelToggle.GetComponent<InventoryToggle>().modelIsSphere;

                        ApplyModelToObject(simBodyToChange, modelCopyToUse,
                            selectedModelToggle.GetComponent<InventoryToggle>().isPrimitive,
                            selectedModelToggle.GetComponent<InventoryToggle>().modelIsSphere);
                    }
                }
            }

            if (selectedModelToggle.GetComponent<InventoryToggle>().myGUIObject == null)
            {
                Destroy(selectedModel);
            }
            else
            {
                selectedModel.SetActive(false);
            }
        }
    }

    private void ApplyModelToObject(GameObject bodyToChange, GameObject modelToUse, bool modelIsPrimitiveShape,
        bool isSphere, bool useCustomTexture = false)
    {
        //Place the model copy into the correct layer for object type (Spacecraft vs. CelestialBody)
        int layerToPutModelIn = 11; //For spacecraft and effectors
        if (!bodyToChange.CompareTag("Spacecraft"))
        {
            layerToPutModelIn = 8; //For planets, moons, sun
        }

        if (selectedModelToggle != null)
        {
            modelToUse.name = bodyToChange.name + selectedModelToggle.name + "Model";
        }
        else
        {
            modelToUse.name = bodyToChange.name + modelToUse.name + "Model";
        }

        modelToUse.SetActive(true);
        //THIS IS WHERE I NEED TO FIND OUT WHAT THE EXTENT WOULD BE GIVEN THE USER SETTINGS IN CASE THERE IS NO RADIUS
        //NOTE THAT IT MATTERS IF THE LOADED MODEL IS TO BE USED AS A SPACECRAFT OR A PLANET
        SpacecraftStateUtilities.MoveEntireGameObjectToLayer(modelToUse.transform, layerToPutModelIn);
        //Save the material if it's a primitive shape << still thinking if this is a better default behavior
        if (bodyToChange.CompareTag("Planet"))
        {
            if ((modelIsPrimitiveShape) && (!useCustomTexture)) //Keep the texture that was being used
            {
                MeshRenderer bodyRenderer = bodyToChange.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
                if (bodyRenderer != null)
                {
                    Material materialToTransfer = bodyRenderer.material;
                    modelToUse.GetComponent<MeshRenderer>().material = materialToTransfer;
                    GameObject oldModel = bodyToChange.transform.GetChild(0).gameObject;
                    if (oldModel.GetComponent<AtmosphereShaderHelper>())
                    {
                        AtmosphereShaderHelper atmosphereHelper = modelToUse.AddComponent<AtmosphereShaderHelper>();
                        atmosphereHelper.PlanetMaterial = materialToTransfer;
                        atmosphereHelper.SetAtmosphereSettings(oldModel.GetComponent<AtmosphereShaderHelper>()
                            .GetAtmosphereSettings());
                        atmosphereHelper.HDatmosphere = oldModel.GetComponent<AtmosphereShaderHelper>().HDatmosphere;
                        atmosphereHelper.atmosphereUpdatesOn =
                            oldModel.GetComponent<AtmosphereShaderHelper>().atmosphereUpdatesOn;
                    }
                }
            }
        }
        else
        {
            //Destroy the old model instance

            Destroy(bodyToChange.GetComponent<SpacecraftController>().spacecraftModel);
            Vector3 desiredScale = modelToUse.transform.localScale;
            Transform desiredLocalTransform = modelToUse.transform;
            modelToUse.transform.SetParent(bodyToChange.transform);
            modelToUse.transform.localPosition = desiredLocalTransform.position;
            modelToUse.transform.localRotation = desiredLocalTransform.rotation;
            modelToUse.transform.localScale = desiredScale;
            //Move the new model to the first child position
            modelToUse.transform.SetSiblingIndex(0);
        }

        UpdateSimulatedBodyModelReference(modelToUse, bodyToChange, isSphere);
    }


    private GameObject GetSelectedModel()
    {
        GameObject selectedModel = null;
        if (selectedModelToggle.GetComponent<InventoryToggle>().myGUIObject == null)
        {
            if (selectedModelToggle.GetComponent<InventoryToggle>().pathToModelResource == "none")
            {
                if (selectedModelToggle.GetComponent<InventoryToggle>().modelName == "Cube")
                {
                    selectedModel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                }
                else if (selectedModelToggle.GetComponent<InventoryToggle>().modelName == "Cylinder")
                {
                    selectedModel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                }
                else if (selectedModelToggle.GetComponent<InventoryToggle>().modelName == "Sphere")
                {
                    selectedModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                }
                else if (selectedModelToggle.GetComponent<InventoryToggle>().modelName == "Capsule")
                {
                    selectedModel = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                }
                else
                {
                    Debug.LogFormat("Requested primitive model {0} and path {1} is not supported.",
                        selectedModelToggle.GetComponent<InventoryToggle>().modelName,
                        selectedModelToggle.GetComponent<InventoryToggle>().pathToModelResource);
                }
            }
            else
            {
                try
                {
                    selectedModel =
                        Instantiate(
                            Resources.Load(selectedModelToggle.GetComponent<InventoryToggle>().pathToModelResource) as
                                GameObject);
                    selectedModel.name = selectedModelToggle.GetComponent<InventoryToggle>().modelName;
                }
                catch
                {
                    Debug.LogFormat("Requested model {0} and path {1} is not supported.",
                        selectedModelToggle.GetComponent<InventoryToggle>().modelName,
                        selectedModelToggle.GetComponent<InventoryToggle>().pathToModelResource);
                }
            }

            if (selectedModel != null)
            {
                ModelBounds newModelBounds = !selectedModel.GetComponent<ModelBounds>()
                    ? selectedModel.AddComponent<ModelBounds>()
                    : selectedModel.GetComponent<ModelBounds>();

                bool useBoxCollider = (selectedModel.name != "Sphere");

                newModelBounds.SetupUnitBoundsForModel(selectedModel);
                newModelBounds.SetupModelBoundsWithModel(useBoxCollider, selectedModel);
            }
        }
        else
        {
            selectedModel = selectedModelToggle.GetComponent<InventoryToggle>().myGUIObject;
            selectedModel.SetActive(true);
        }

        return selectedModel;
    }

    private void ModifyModel()
    {
        GameObject modelToModify = GetSelectedModel();
        adjustModelPanel.SetActive(true);
        adjustModelPanel.GetComponent<AdjustModelPanelMethods>().ConfigurePanelView(modelToModify, selectedModelToggle);
        transform.gameObject.SetActive(false);
    }

    private void ModifyObject()
    {
        if (selectedObjects.Count == 1)
        {
            GameObject objectToModify = selectedObjects[0].GetComponent<InventoryToggle>().myGUIObject;
            adjustModelPanel.SetActive(true);
            adjustModelPanel.GetComponent<AdjustModelPanelMethods>()
                .ConfigurePanelView(objectToModify.transform.GetChild(0).gameObject, selectedObjects[0], true);
            transform.gameObject.SetActive(false);
        }
    }

    public void UpdateModel(GameObject updatedModel, GameObject selectedToggle)
    {
        updatedModel.SetActive(false);
        if (selectedToggle.GetComponent<InventoryToggle>().myGUIObject != null)
        {
            GameObject oldModel = selectedToggle.GetComponent<InventoryToggle>().myGUIObject;
            selectedToggle.GetComponent<InventoryToggle>().myGUIObject = updatedModel;
            Destroy(oldModel);
        }
        else
        {
            selectedToggle.GetComponent<InventoryToggle>().myGUIObject = updatedModel;
        }
    }

    public void UpdateObject(GameObject updatedObjectMesh, GameObject selectedToggle, bool materialUpdated)
    {
        UpdateSimulatedBodyModelReference(updatedObjectMesh, selectedToggle.GetComponent<InventoryToggle>().myGUIObject,
            selectedToggle.GetComponent<InventoryToggle>().modelIsSphere);
    }

    public void ModelToggleSelected(GameObject toggleSelected, bool toggledOn)
    {
        if (toggledOn)
        {
            selectedModelToggle = toggleSelected;
            ModifyModelButton.interactable = true;
            ApplyButton.interactable = true;
        }
        else
        {
            if (selectedModelToggle == toggleSelected)
            {
                ModifyModelButton.interactable = false;
                selectedModelToggle = null;
                ApplyButton.interactable = false;
            }
        }
    }

    public void ObjectToggleSelected(GameObject toggleSelected, bool toggledOn)
    {
        if (toggledOn)
        {
            selectedObjects.Add(toggleSelected);
        }
        else
        {
            selectedObjects.Remove(toggleSelected);
        }

        ModifyObjectButton.interactable = selectedObjects.Count == 1;
    }

    private void CreateSimulatedObjectsToggles()
    {
        if (SpacecraftStateUtilities.SpacecraftList.Count > 1)
        {
            CreateAllSpacecraftToggle();
        }

        foreach (GameObject sc in SpacecraftStateUtilities.SpacecraftList)
        {
            GameObject newObjectToggle = CreateObjectToggle(sc);
            allSpacecraftToggles.Add(newObjectToggle);
        }

        foreach (GameObject cb in CelestialBodyStateUtilities.CelestialBodiesList)
        {
            bool isSphere = true;
            if (cb.CompareTag("Planet"))
            {
                isSphere = !cb.GetComponent<PlanetController>().planetModel.GetComponent<ModelBounds>().useBoxCollider;
            }

            CreateObjectToggle(cb, isSphere, isSphere);
        }

        if (objectToggles.Count > 8)
        {
            float width = objectInventory.GetComponent<RectTransform>().rect.width;
            objectInventory.GetComponent<RectTransform>().sizeDelta = new Vector2(width, 25 * objectToggles.Count);
        }
    }

    private void CreateAllSpacecraftToggle()
    {
        GameObject newObjectToggle =
            Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericToggle") as GameObject, objectInventory.transform, true);
        newObjectToggle.name = "AllSpacecraftToggle";
        newObjectToggle.GetComponentInChildren<TextMeshProUGUI>().text = "Select All Spacecraft";
        newObjectToggle.GetComponent<Toggle>().isOn = false;
        newObjectToggle.GetComponent<Toggle>().onValueChanged.AddListener(SelectAllSpacecraft);
        objectToggles.Add(newObjectToggle);

        newObjectToggle.GetComponent<RectTransform>().localScale = Vector3.one;
        newObjectToggle.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(5, -(objectToggles.Count - 1) * 25 - 5);
    }

    private void SelectAllSpacecraft(bool isOn)
    {
        foreach (GameObject scToggle in allSpacecraftToggles)
        {
            scToggle.GetComponent<Toggle>().isOn = isOn;
        }
    }

    private GameObject CreateObjectToggle(GameObject simulatedBody, bool objectIsPrimitive = false,
        bool objectIsSphere = false)
    {
        GameObject newObjectToggle =
            Instantiate(Resources.Load("Prefabs/GUIGenerics/GenericToggle") as GameObject, objectInventory.transform, true);
        newObjectToggle.name = simulatedBody.name;
        newObjectToggle.GetComponent<Toggle>().isOn = false;
        newObjectToggle.AddComponent<InventoryToggle>();
        newObjectToggle.GetComponent<InventoryToggle>().SetupToggleWithGUIObject(simulatedBody, transform.gameObject,
            "SIMOBJECT", objectIsPrimitive, objectIsSphere);

        objectToggles.Add(newObjectToggle);

        newObjectToggle.GetComponent<RectTransform>().localScale = Vector3.one;
        newObjectToggle.GetComponent<RectTransform>().anchoredPosition =
            new Vector2(5, -(objectToggles.Count - 1) * 25 - 5);

        return newObjectToggle;
    }

    private void UpdateObjectToggle(GameObject simulatedBody, bool objectIsPrimitive = false,
        bool objectIsSphere = false)
    {
        foreach (GameObject toggle in objectToggles)
        {
            if (toggle.name == simulatedBody.name)
            {
                toggle.GetComponent<InventoryToggle>().isPrimitive = objectIsPrimitive;
                toggle.GetComponent<InventoryToggle>().modelIsSphere = objectIsSphere;
                return;
            }
        }
    }

    private void UpdateSimulatedBodyModelReference(GameObject updatedObjectMesh, GameObject simBodyToUpdate,
        bool isSphere)
    {
        if (simBodyToUpdate.GetComponent<SpacecraftController>() != null)
        {
            simBodyToUpdate.GetComponent<SpacecraftController>().ReplaceSpacecraftModelAndUpdate(updatedObjectMesh);
        }
        else if (simBodyToUpdate.GetComponent<PlanetController>() != null)
        {
            simBodyToUpdate.GetComponent<PlanetController>().FinalizeAppliedModel(updatedObjectMesh, isSphere);
        }
        else if (simBodyToUpdate.GetComponent<SunBuilder>() != null)
        {
            simBodyToUpdate.GetComponent<SunBuilder>().SetSunMesh(updatedObjectMesh);
        }
        else
        {
            Debug.LogFormat("{0} is a type of object not handled by ModelDirectory when updating it's model.",
                simBodyToUpdate.name);
        }
    }

    public void ApplyCustomModelMessageSettings(VizMessage.Types.CustomModel newSettings,
        bool newImport = false)
    {
        GameObject modelToUse = null;
        bool modelIsANewImport = newImport;
        //Get the model
        bool modelIsPrimitive = true;
        bool modelIsSphere = false;
        if (newSettings.ModelPath == "CUBE")
        {
            modelToUse = GameObject.CreatePrimitive(PrimitiveType.Cube);
            modelToUse.name = "CustomCube";
            modelIsANewImport = false;
        }
        else if (newSettings.ModelPath == "SPHERE")
        {
            modelToUse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            modelIsSphere = true;
            modelToUse.name = "CustomSphere";
            modelIsANewImport = false;
        }
        else if (newSettings.ModelPath == "CYLINDER")
        {
            modelToUse = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            modelToUse.name = "CustomCylinder";
            modelIsANewImport = false;
        }
        else if (newSettings.ModelPath == "CAPSULE")
        {
            modelToUse = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            modelToUse.name = "CustomCapsule";
            modelIsANewImport = false;
        }
        else if (newSettings.ModelPath == "TRI")
        {
            string[] modelInfo = SpacecraftStateUtilities.SpacecraftModels["TRI"];
            modelToUse = Instantiate(Resources.Load(modelInfo[0]) as GameObject);
            modelToUse.name = "CustomTriPanel";
            modelIsANewImport = false;
        }
        else if (newSettings.ModelPath == "HI_DEF_SPHERE")
        {
            modelToUse = Instantiate(Resources.Load("Models/BasicPlanetModel") as GameObject);
            modelIsSphere = true;
            modelToUse.name = "CustomHiDefSphere";
            modelIsANewImport = false;
        }
        else if (newSettings.ModelPath == "bskSat")
        {
            modelToUse = Instantiate(Resources.Load("Models/BSKSAT_model") as GameObject);
            modelIsPrimitive = false;
            modelToUse.name = "bskSat";
            modelIsANewImport = false;
        }
        else if (newSettings.ModelPath == "6USat")
        {
            modelToUse = Instantiate(Resources.Load("Models/CubeSAT_6U") as GameObject);
            modelIsPrimitive = false;
            modelToUse.name = "6U_CubeSat";
            modelIsANewImport = false;
        }
        else if (newSettings.ModelPath == "3USat")
        {
            modelToUse = Instantiate(Resources.Load("Models/CubeSAT_3U") as GameObject);
            modelIsPrimitive = false;
            modelToUse.name = "3U_CubeSat";
            modelIsANewImport = false;
        }
        else
        {
            //Loading an obj from files
            modelIsPrimitive = false;
            VizardGUISettings.UseDefaultSpecularShader = newSettings.Shader != 1;

            //Check to see if the model is already in the library if modelIsANewImport is false
            bool loadAModel = true;
            if (!newImport)
            {
                foreach (GameObject modelToggle in modelToggles)
                {
                    if (modelToggle.name == Path.GetFileNameWithoutExtension(newSettings.ModelPath))
                    {
                        modelToUse = Instantiate(modelToggle.GetComponent<InventoryToggle>().myGUIObject);
                        loadAModel = false;
                    }
                }
            }

            if (loadAModel)
            {
                //Will call FinalizeCustomModel after model is imported
                modelImporter.ImportModelAtRuntime(newSettings.ModelPath, newSettings);
                return;
            }
        }

        FinalizeCustomModel(modelToUse, newSettings, modelIsPrimitive, modelIsSphere, modelIsANewImport);
    }

    public void FinalizeCustomModel(GameObject modelToUse, VizMessage.Types.CustomModel newSettings,
        bool modelIsPrimitive, bool modelIsSphere, bool modelIsANewImport)
    {
        if (modelToUse != null)
        {
            //Model loaded successfully
            VizardGUISettings.PopRemoteAssetLoadFromList(newSettings.ModelPath, true);

            //Apply the scaling because that needs to be included for the model bounds calculations and planet radius calculations to work out correctly
            modelToUse.transform.localScale = new Vector3((float) newSettings.Scale[1], (float) newSettings.Scale[2],
                (float) newSettings.Scale[0]);
            if (modelToUse.transform.localScale == Vector3.zero)
            {
                modelToUse.transform.localScale = Vector3.one;
            }

            //Apply the custom texture/normal map if provided
            bool useCustomTexture = false;
            string textureString = newSettings.CustomTexturePath;
            string normalMapString = newSettings.NormalMapPath;
            Material customMaterial = null;
            Renderer[] allRenderers = modelToUse.transform.GetComponentsInChildren<Renderer>();
            MeshRenderer[] allMeshRenderers = modelToUse.transform.GetComponentsInChildren<MeshRenderer>();
            if (!string.IsNullOrEmpty(textureString))
            {
                customMaterial = CreateCustomMaterial(textureString, normalMapString);
                if (modelIsPrimitive)
                {
                    modelToUse.GetComponent<Renderer>().material = customMaterial;
                }
                else
                {
                    foreach (Renderer r in allRenderers)
                    {
                        r.material = customMaterial;
                    }

                    foreach (MeshRenderer r in allMeshRenderers)
                    {
                        r.material = customMaterial;
                    }
                }

                useCustomTexture = true;
            }

            if (newSettings.Color.Count >= 3)
            {
                Color newColor = new Color(newSettings.Color[0] / 255f, newSettings.Color[1] / 255f,
                    newSettings.Color[2] / 255f, 1f);
                if (newSettings.Color.Count >= 4)
                {
                    newColor.a = newSettings.Color[3] / 255f;
                }

                foreach (Renderer r in allRenderers)
                {
                    r.material.color = newColor;
                }

                foreach (MeshRenderer r in allMeshRenderers)
                {
                    r.material.color = newColor;
                }

                if ((allRenderers.Length == 0) && (allMeshRenderers.Length == 0))
                {
                    Debug.Log(
                        $"Custom model: {newSettings.ModelPath} does not have a renderer or mesh renderer component. Custom color could not be applied.");
                }
            }

            if (!modelToUse.GetComponent<ModelBounds>())
            {
                modelToUse.AddComponent<ModelBounds>();
                modelToUse.GetComponent<ModelBounds>().SetupUnitBoundsForModel(modelToUse);
            }

            modelToUse.GetComponent<ModelBounds>().SetupModelBoundsWithModel(true, modelToUse);

            if ((modelToUse.name == "CustomSphere") || (modelToUse.name == "CustomHiDefSphere"))
            {
                modelToUse.GetComponent<ModelBounds>().useBoxCollider = false;
            }

            //Apply the offset, rotation, scale settings to the model
            modelToUse.transform.localPosition = new Vector3((float) newSettings.Offset[1],
                (float) newSettings.Offset[2], -((float) newSettings.Offset[0]));

            Quaternion modelRotation = Quaternion.identity;
            modelRotation.eulerAngles = new Vector3((float) newSettings.Rotation[1], (float) newSettings.Rotation[2],
                -((float) newSettings.Rotation[0]));
            modelToUse.transform.localRotation = modelRotation;

            //Get the object(s) and apply the model to the object
            if (newSettings.SimBodiesToModify[0] == "ALL_SPACECRAFT")
            {
                foreach (GameObject sc in SpacecraftStateUtilities.SpacecraftList)
                {
                    ApplyModelToObject(sc, Instantiate(modelToUse), modelIsPrimitive, modelIsSphere, useCustomTexture);
                    UpdateObjectToggle(sc, modelIsPrimitive,
                        modelIsSphere);
                    sc.GetComponent<SpacecraftController>().SetDefaultModel(newSettings);
                }
            }
            else
            {
                foreach (string simBody in newSettings.SimBodiesToModify)
                {
                    GameObject objectToModify = CelestialBodyStateUtilities.GetGameObjectWithBodyName(simBody);
                    if (objectToModify != null)
                    {
                        if (objectToModify.CompareTag("Spacecraft"))
                        {
                            modelToUse.GetComponent<ModelBounds>().useBoxCollider = true;
                            objectToModify.GetComponent<SpacecraftController>().SetDefaultModel(newSettings);
                        }
                        else if (objectToModify.CompareTag("Sun"))
                        {
                            modelToUse.GetComponent<ModelBounds>().useBoxCollider = false;
                            objectToModify.GetComponent<SunBuilder>().SetDefaultModel(newSettings);
                            if (customMaterial != null)
                            {
                                objectToModify.GetComponent<SunBuilder>().SetDefaultMaterial(customMaterial);
                            }
                        }
                        else
                        {
                            //apply to planets/moons
                            modelToUse.GetComponent<ModelBounds>().useBoxCollider = false;
                            objectToModify.GetComponent<PlanetController>().SetDefaultModel(newSettings);
                        }

                        ApplyModelToObject(objectToModify, Instantiate(modelToUse), modelIsPrimitive, modelIsSphere,
                            useCustomTexture);
                        Debug.Log($"OBJECT TO MODIFY----------{objectToModify.name}");
                        UpdateObjectToggle(objectToModify, modelIsPrimitive, modelIsSphere);
                    }
                }
            }

            if (modelIsANewImport)
            {
                AddModelToInventory(modelToUse, modelIsPrimitive, modelIsSphere);
            }
            else
            {
                Destroy(modelToUse);
            }
        }
        else
        {
            VizardGUISettings.PopRemoteAssetLoadFromList(newSettings.ModelPath, false);
        }
    }

    public Material CreateCustomMaterial(string texturePath, string normalPath, float normalMapHeight = 1.0f)
    {
        Material customMaterial = Instantiate(Resources.Load("Materials/CustomModel", typeof(Material)) as Material);
        customMaterial.SetColor("_Color", new Color(0.9f, 0.9f, 0.9f, 1));
        string pathToTry = texturePath;
        if ((!DataManager.IsLiveSim) && (pathToTry.StartsWith(".")))
        {
            pathToTry = Path.GetFullPath(pathToTry, Path.GetDirectoryName(DataManager.FilePath));
        }

        Texture2D customTexture = CameraMessageUtilities.LoadTextureImage(pathToTry);
        if (customTexture != null)
        {
            if ((customTexture.width == 8) && (customTexture.height == 8))
            {
                string errMsg =
                    $"Custom texture {texturePath} could not be applied. Textures must be 16384 pixels x 16384 pixels or less.";
                VizardGUISettings.UpdateErrorMessages(errMsg);
            }
            else
            {
                customMaterial.SetTexture("_MainTex", customTexture);
            }
        }

        if (!string.IsNullOrEmpty(normalPath))
        {
            pathToTry = normalPath;
            if ((!DataManager.IsLiveSim) && (pathToTry.StartsWith(".")))
            {
                pathToTry = Path.GetFullPath(pathToTry, Path.GetDirectoryName(DataManager.FilePath));
            }

            Texture2D normalMap = CameraMessageUtilities.LoadNormalMap(pathToTry);
            if (normalMap != null)
            {
                if ((normalMap.width == 8) && (normalMap.height == 8))
                {
                    string errMsg =
                        $"Custom normal map {texturePath} could not be applied. Textures must be 16384 pixels x 16384 pixels or less.";
                    VizardGUISettings.UpdateErrorMessages(errMsg);
                }
                else
                {
                    customMaterial.EnableKeyword("_BumpMap");
                    customMaterial.SetTexture("_BumpMap", normalMap);
                    customMaterial.EnableKeyword("_BumpScale");
                    customMaterial.SetFloat("_BumpScale", normalMapHeight);
                }
            }
        }
        else
        {
            customMaterial.SetTexture("_BumpMap", null);
        }

        return customMaterial;
    }

    private void RevertObjectToDefaultModel()
    {
        foreach (GameObject objectToRevertToggle in selectedObjects)
        {
            GameObject objectToRevert = objectToRevertToggle.GetComponent<InventoryToggle>().myGUIObject;
            if (objectToRevert.CompareTag("Spacecraft"))
            {
                VizProtobufferMessage.VizMessage.Types.CustomModel defaultSettings =
                    objectToRevert.GetComponent<SpacecraftController>().GetDefaultModel();
                ApplyCustomModelMessageSettings(defaultSettings);
            }
            else if (objectToRevert.CompareTag("Sun"))
            {
                VizProtobufferMessage.VizMessage.Types.CustomModel defaultSettings =
                    objectToRevert.GetComponent<SunBuilder>().GetDefaultModel();
                ApplyCustomModelMessageSettings(defaultSettings);
                if (objectToRevert.GetComponent<SunBuilder>().GetDefaultMaterial() != null)
                {
                    objectToRevert.GetComponent<SunBuilder>().ApplyDefaultMaterial();
                }
            }
            else
            {
                objectToRevert.GetComponent<PlanetController>().ApplyDefaultMaterialAndModel();
            }
        }
    }
}