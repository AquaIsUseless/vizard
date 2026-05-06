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

using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Dummiesman; //OBJ Importer 
using GLTFast;
using TMPro;
using VizProtobufferMessage;
#if USE_NATIVE_FILE_BROWSER
using Crosstales.FB;
#endif
/// <summary>
/// Supports runtime import of obj and glb models
/// using Dummiesman OBJ Importer and Unity GLTFast package
/// </summary>
public class ImportModelMethods : MonoBehaviour
{
    public VizardFileBrowser fileChooser;
    public GameObject adjustModelPanel;
    public GameObject modelInventory;
    public ModelDirectoryPanelMethods modelInventoryMethods;
    public Button importButton;
    public TextMeshProUGUI filepath;
    public TextMeshProUGUI errorText;
    public Toggle useSpecularShaderToggle;
    public Toggle useStandardShaderToggle;
    public Toggle useOBJImporterToggle;
    public Button infoButton;
    public Button cancelButton;
    public GameObject infoPanel;
    
    private bool reopenModelInventoryOnCancel;

    void Start()
    {
        importButton.onClick.AddListener(ImportModelAndTriggerModelTuningPanel);
        useSpecularShaderToggle.onValueChanged.AddListener(ToggleSpecularShader);
        useOBJImporterToggle.onValueChanged.AddListener(ToggleUseOBJ);
        infoButton.onClick.AddListener(ToggleInfoPanel);
        cancelButton.onClick.AddListener(ClosePanel);
    }

    void Awake()
    {
        importButton.interactable = false;
        errorText.color = Color.white;
        errorText.text = "Model importer supports .obj and .glb formats only.";
    }

    void OnEnable()
    {
        transform.SetAsLastSibling();
        if (modelInventory.activeInHierarchy)
        {
            reopenModelInventoryOnCancel = true;
            modelInventory.SetActive(false);
        }
    }


    void Update()
    {
        importButton.interactable = (!string.IsNullOrEmpty(filepath.text));
    }

    public void SelectFileButtonClicked()
    {
        string fileExtension = useOBJImporterToggle.isOn ? "obj" : "glb";
#if USE_NATIVE_FILE_BROWSER
        if (Application.platform == RuntimePlatform.LinuxPlayer)
        {
            fileChooser.OpenFileBrowser(filepath,"*."+fileExtension);
        }
        else
        {
            filepath.text =
                FileBrowser.Instance.OpenSingleFile("Choose model", DataManager.LastDirectory, string.Empty,
                    fileExtension);
        }
#else
        fileChooser.OpenFileBrowser(filepath, "*." + fileExtension);
#endif
    }

    private void ToggleSpecularShader(bool isOn)
    {
        VizardGUISettings.UseDefaultSpecularShader = isOn;
    }

    private void ToggleUseOBJ(bool isOBJ)
    {
        useSpecularShaderToggle.interactable = isOBJ;
        useSpecularShaderToggle.GetComponentInChildren<TextMeshProUGUI>().color = (isOBJ ? Color.white : Color.gray);
        useStandardShaderToggle.interactable = isOBJ;
        useStandardShaderToggle.GetComponentInChildren<TextMeshProUGUI>().color = (isOBJ ? Color.white : Color.gray);
    }

    public void ImportModelAtRuntime(string incomingModelFilePath, VizMessage.Types.CustomModel settings,
        bool triggerTuningPanel = false)
    {
        string fullPath = incomingModelFilePath;
        if ((!DataManager.IsLiveSim) && (fullPath.StartsWith(".")))
        {
            fullPath = Path.GetFullPath(fullPath, Path.GetDirectoryName(DataManager.FilePath));
        }

        if (!File.Exists(fullPath))
        {
            errorText.color = Color.red;
            errorText.text = "File doesn't exist.";
            VizardGUISettings.UpdateErrorMessages($"Could not import model at: {fullPath}, file not found.");
            modelInventoryMethods.FinalizeCustomModel(null, settings, false, false, true);
        }
        else
        {
            if (fullPath.EndsWith(".obj") || fullPath.EndsWith(".OBJ"))
            {
                GameObject newModel = new OBJLoader().Load(fullPath);
                if (newModel != null)
                {
                    FinalizeModel(newModel, settings, triggerTuningPanel);
                }
                else
                {
                    if (triggerTuningPanel)
                    {
                        errorText.color = Color.red;
                        errorText.text = "File doesn't exist.";
                    }
                }
            }
            else if (fullPath.EndsWith(".glb") || fullPath.EndsWith(".GLB"))
            {
                LoadGltfBinary(fullPath, settings, triggerTuningPanel);
            }
            else
            {
                VizardGUISettings.UpdateErrorMessages(
                    $"Could not import model at: {fullPath}, file is not a supported type.", true);
            }
        }
    }

    private void ImportModelAndTriggerModelTuningPanel()
    {
        errorText.color = Color.white;
        errorText.text = "Importing model, please stand by...";
        VizardGUISettings.UseDefaultSpecularShader = useSpecularShaderToggle.isOn;
        ImportModelAtRuntime(filepath.text, new VizMessage.Types.CustomModel() { }, true);
    }

    private void FinalizeModel(GameObject loadedObject, VizMessage.Types.CustomModel settings,
        bool triggerTuningPanel = false)
    {
        if (triggerTuningPanel)
        {
            ModelBounds newBounds = loadedObject.AddComponent<ModelBounds>();
            newBounds.SetupUnitBoundsForModel(loadedObject);
            adjustModelPanel.SetActive(true);
            adjustModelPanel.GetComponent<AdjustModelPanelMethods>().ConfigurePanelView(loadedObject);
            errorText.color = Color.white;
            errorText.text =
                "Model importer supports .obj and .glb formats only.\nModel units assumed to be in meters when applied to spacecraft, kilometers when applied to celestial bodies.";
            transform.gameObject.SetActive(false);
        }
        else
        {
            modelInventoryMethods.FinalizeCustomModel(loadedObject, settings, false, false, true);
        }
    }

    public void CancelImportPanel()
    {
        if (reopenModelInventoryOnCancel)
        {
            reopenModelInventoryOnCancel = false;
            modelInventory.SetActive(true);
        }
    }

    private void ToggleInfoPanel()
    {
        infoPanel.SetActive(!infoPanel.activeSelf);
    }

    async void LoadGltfBinary(string filePath, VizMessage.Types.CustomModel settings, bool triggerTuningPanel)
    {
        var gltf = new GltfImport();

        // Create a settings object and configure it accordingly
        var gltfSettings = new ImportSettings
        {
            GenerateMipMaps = true,
            AnisotropicFilterLevel = 3,
            NodeNameMethod = NameImportMethod.OriginalUnique
        };
        // Load the glTF and pass along the settings
        var success = await gltf.Load(filePath, gltfSettings);

        if (success)
        {
            var newObject = new GameObject(Path.GetFileNameWithoutExtension(filePath));
            await gltf.InstantiateMainSceneAsync(newObject.transform);
            FinalizeModel(newObject, settings, triggerTuningPanel);
        }
        else
        {
            Debug.LogError("Loading glTF failed!");
        }
    }

    private void ClosePanel()
    {
        gameObject.SetActive(false);
    }
}