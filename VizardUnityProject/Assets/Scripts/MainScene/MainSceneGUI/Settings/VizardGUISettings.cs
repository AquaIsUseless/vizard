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
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VizProtobufferMessage;
#if VIZARD_OPENXR
using UnityEngine.InputSystem;
#endif

public static class VizardGUISettings
{
    //Coordinate Frames
    public static bool CameraTargetCSOn;
    public static bool AllPlanetCSOn;
    public static bool AllSpacecraftCSOn;
    public static bool AllEffectorCSOn;
    public static bool ShowHillFrame;
    public static bool ShowVelocityFrame;

    //Orbit Lines	
    public static bool OsculatingOrbitLinesVisible = true;

    //Osculating orbit Options
    public static bool SpacecraftRelativeOsculatingOrbits;

    //True path trajectory Options
    public static bool TruePathLinesVisible;

    /*TruePathLineMode:
        1 for relative only to camera target current position(origin)
        2 for spacecraft relative to a chief spacecraft
        3 for relative to a celestial body (default is the spacecraft's parent body, but user can specify)
        4 for transformed into rotating frame
        5 for transformed into fixed frame (not implemented as of 2.3.0b*/
    public static int TruePathLineMode = 1;
    public static int SpacecraftRelativeOrbitMode = 1; // 1 for Hill Frame, 2 for Velocity Frame, 3 for Inertial
    public static int ChiefSpacecraftIndex; //use this to set relative spacecraft body for all three modes
    public static bool SetChiefToCamTgt = true;

    public static bool UseSpacecraftParentBodyForRelativeTraj = true;
    public static int RelativeBodyIndex = -1;

    public static bool UseSpacecraftParentBodyForFixedFrameTraj = true;
    public static int FixedBodyIndex = -1;
    public static bool FixedBodyIsSpacecraft;
    public static int RotatingFrameBody1Index = -1;
    public static int RotatingFrameBody2Index = -1;
    public static int RelativeTruePathChangeCount;

    //GroundTracks
    public static bool OsculatingGroundTrackOn = false;
    public static bool TruePathGroundTrackOn = false;

    //Skybox
    public static string CurrentSkybox;
    public static Color SkyboxColor;
    public static bool SkyboxIsTexture = true;

    //Instruments
    public static bool ShowCamBoresights;
    public static bool ShowCamFrustums;
    public static bool ShowCamPreviews;
    public static bool ShowStationCommunicationLines = true;
    public static bool ShowStationCone = true;

    //GUI Appearance
    private static float HUDElementsLineWidth = 0.025f;
    private static List<HUDLineRenderer> HUDLines = new List<HUDLineRenderer>();

    //Labels
    public static bool ShowSpacecraftLabels;
    public static bool ShowEffectorLabels;
    public static bool ShowCelestialBodyLabels;
    public static bool ShowCameraLabels;
    public static bool ShowCSLabels;
    public static bool ShowThrusterLabels;
    public static bool ShowRWLabels;
    public static bool ShowCSSLabels;
    public static bool ShowLocationLabels = true;
    public static bool SomeSpacecraftLabelsAreOn;
    public static bool SomeCelestialBodyLabelsAreOn;
    public static bool ShowGenericSensorLabels;
    public static bool ShowTransceiverLabels;
    public static bool ShowLightLabels;
    public static bool ShowMSMLabels;
    public static bool ShowQuadMapLabels = true;

    //Sprites
    public static bool ShowSpritesForSpacecraft; //This will get changed to true by SpacecraftManager if s/c count >1
    public static bool ShowSpritesForPlanets;
    public static float PlanetSpriteSize = 0.06f;
    public static float SpacecraftSpriteSize = 0.04f;
    private static float LocationMarkerSize = 0.006f;
    public static float SpacecraftApparentSizeThreshold = 0.01f;

    //Lighting
    public static bool UseShellLighting;
    public static GameObject MainLight;

    //Shaders
    public static bool UseDefaultSpecularShader = true;
    public static bool UseAtmosphereShaderIfAvailable = true;

    //Locations
    public static bool UseSimpleMarkersForLocations;

    //References
    public static bool AssetLoadingComplete;
    private static List<string> remoteAssetsInLoading = new List<string>();
    public static int StartupCount;
    public static string StatusText;
    public static GameObject GUICanvas;
    public static PostProcessingManager PostProcessingMgr;
    public static PanelViewManager PanelViewMgr;
    public static SettingsPanelMethods SettingsPanel;
    public static GameObject ColorWheelPanel;
    public static ItsAboutTime PlaybackManager;
    public static GameObject FadingStatusTextBox;
    public static string ConsoleMsgText;
    public static GameObject ConsoleLog;

    private static bool isVRRadialMenuActive;
    private static readonly LayerMask secondaryCameraDefaultLayerMask = ((1 << 0) | (1 << 7) | (1 << 8) | (1 << 11));

    private static readonly LayerMask secondaryCameraHUDLayersIncludedMask =
        ((1 << 0) | (1 << 7) | (1 << 8) | (1 << 11) | (1 << 14) | (1 << 16) | (1 << 22));

    public const int VRUIRaycastLayer = 26;


    public static VizMessage.Types.ActuatorSettings GetActuatorSettings(string spacecraftName)
    {
        if (MessageList.CurrentMessage.Settings != null)
        {
            if (MessageList.CurrentMessage.Settings.ActuatorSettings != null)
            {
                foreach (VizMessage.Types.ActuatorSettings actSetting in MessageList.CurrentMessage.Settings
                             .ActuatorSettings)
                {
                    if (actSetting.SpacecraftName == spacecraftName)
                    {
                        return actSetting;
                    }
                }
            }
        }

        return null;
    }

    public static VizMessage.Types.InstrumentSettings GetInstrumentSettings(string spacecraftName)
    {
        if (MessageList.CurrentMessage.Settings != null)
        {
            if (MessageList.CurrentMessage.Settings.InstrumentSettings != null)
            {
                foreach (VizMessage.Types.InstrumentSettings instSetting in MessageList.CurrentMessage.Settings
                             .InstrumentSettings)
                {
                    if (instSetting.SpacecraftName == spacecraftName)
                    {
                        return instSetting;
                    }
                }
            }
        }

        return null;
    }

    public static float CalculateScaleForPlanetSprite(Transform body)
    {
        if (MainCameraUtilities.MainCamera == null)
        {
            MainCameraUtilities.MainCamera = Camera.main;
        }

        float distToCam = (body.position - MainCameraUtilities.MainCamera.transform.position).magnitude;
        float scale = PlanetSpriteSize * distToCam;
        return scale;
    }

    public static float CalculateScaleForSpacecraftSprite(Transform spriteObject)
    {
        if (MainCameraUtilities.MainCamera == null)
        {
            MainCameraUtilities.MainCamera = Camera.main;
        }

        float distToCam = (spriteObject.position - MainCameraUtilities.MainCamera.transform.position).magnitude;
        float scale = SpacecraftSpriteSize * distToCam;
        return scale;
    }

    public static float CalculateScaleForLocationMarker(Transform body)
    {
        if (MainCameraUtilities.MainCamera == null)
        {
            MainCameraUtilities.MainCamera = Camera.main;
        }

        float distToCam = (body.position - MainCameraUtilities.MainCamera.transform.position).magnitude;
        float scale = LocationMarkerSize * distToCam;
        return scale;
    }

    public static Sprite GetSprite(string spriteName)
    {
        Sprite currentSprite;
        if ((spriteName == "bskSat") || (spriteName == "BSKSAT"))
        {
            currentSprite = Resources.Load<Sprite>("Sprites/BodySprites/bskSat_sprite");
        }
        else if ((spriteName == "Circle") || (spriteName == "CIRCLE"))
        {
            currentSprite = Resources.Load<Sprite>("Sprites/BodySprites/circle_primitive");
        }
        else if ((spriteName == "Square") || (spriteName == "SQUARE"))
        {
            currentSprite = Resources.Load<Sprite>("Sprites/BodySprites/square_primitive");
        }
        else if ((spriteName == "Star") || (spriteName == "STAR"))
        {
            currentSprite = Resources.Load<Sprite>("Sprites/BodySprites/star_primitive");
        }
        else if ((spriteName == "Triangle") || (spriteName == "TRIANGLE"))
        {
            currentSprite = Resources.Load<Sprite>("Sprites/BodySprites/triangle_primitive");
        }
        else
        {
            currentSprite = Resources.Load<Sprite>("Sprites/BodySprites/circle_primitive");
            Debug.Log("Invalid selection from sprite list.");
        }

        return currentSprite;
    }

    public static List<string> CreateBodyListForDropdown(TMP_Dropdown dropdown, string option0Label = "",
        bool includeLocations = false, bool includePlanets = true, bool includeSpacecraft = true,
        bool excludeEffectors = true)
    {
        List<string> bodyList = new List<string>();
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        if (option0Label == "")
        {
            bodyList.Add("Select Body");
            options.Add(new TMP_Dropdown.OptionData("Select Body"));
        }
        else
        {
            bodyList.Add(option0Label);
            options.Add(new TMP_Dropdown.OptionData(option0Label));
        }


        if (includeSpacecraft)
        {
            if (SpacecraftStateUtilities.ParentAndEffectorDictionary.Count > 0)
            {
                if (excludeEffectors)
                {
                    foreach (GameObject sc in SpacecraftStateUtilities.ParentSpacecraftList)
                    {
                        bodyList.Add(sc.name);
                        options.Add(new TMP_Dropdown.OptionData(sc.name));
                    }
                }
                else
                {
                    HoverDropdown hoverDropdown = dropdown.transform.gameObject.GetComponent<HoverDropdown>();
                    if (hoverDropdown == null)
                    {
                        hoverDropdown = dropdown.transform.gameObject.AddComponent<HoverDropdown>();
                    }

                    foreach (GameObject sc in SpacecraftStateUtilities.ParentSpacecraftList)
                    {
                        bodyList.Add(sc.name);
                        if (SpacecraftStateUtilities.ParentAndEffectorDictionary.ContainsKey(sc.name))
                        {
                            List<string> effectorOptions = new List<string> {"Select Effector"};
                            foreach (int index in SpacecraftStateUtilities.ParentAndEffectorDictionary[sc.name])
                            {
                                effectorOptions.Add(SpacecraftStateUtilities.SpacecraftList[index].name);
                            }

                            GameObject effDropdown = hoverDropdown.AddSubDropdownMenu(bodyList.Count, effectorOptions,
                                sc.name, option0Label);
                            effDropdown.SetActive(false);

                            options.Add(new TMP_Dropdown.OptionData(sc.name, hoverDropdown.arrowSprite, Color.white));
                        }
                        else
                        {
                            options.Add(new TMP_Dropdown.OptionData(sc.name));
                        }
                    }
                }
            }
            else
            {
                foreach (GameObject sc in SpacecraftStateUtilities.SpacecraftList)
                {
                    bodyList.Add(sc.name);
                    options.Add(new TMP_Dropdown.OptionData(sc.name));
                }
            }
        }

        if (includePlanets)
        {
            //Then add planets/moons
            foreach (GameObject cb in CelestialBodyStateUtilities.CelestialBodiesList)
            {
                bodyList.Add(cb.name);
                options.Add(new TMP_Dropdown.OptionData(cb.name));
            }
        }

        if (includeLocations)
        {
            foreach (string key in CelestialBodyStateUtilities.LocationsDictionary.Keys)
            {
                bodyList.Add(key);
                options.Add(new TMP_Dropdown.OptionData(key));
            }
        }

        VizardGUISettings.PopulateList(dropdown, options);
        return bodyList;
    }

    public static void PopulateList(TMP_Dropdown dropdown, List<string> listToUse, TextMeshProUGUI label,
        string labelText)
    {
        dropdown.ClearOptions();
        //Populate the dropdown menu with body names
        dropdown.AddOptions(listToUse);
        label.text = labelText;
    }

    public static void PopulateList(TMP_Dropdown dropdown, List<string> listToUse)
    {
        dropdown.ClearOptions();
        //Populate the dropdown menu with body names
        dropdown.AddOptions(listToUse);
    }

    private static void PopulateList(TMP_Dropdown dropdown, List<TMP_Dropdown.OptionData> listToUse)
    {
        dropdown.ClearOptions();
        //Populate the dropdown menu with body names
        dropdown.AddOptions(listToUse);
    }

    public static void UpdateErrorMessages(string msgToAdd, bool popUpConsoleWindow = false)
    {
        Debug.Log(msgToAdd);
        ConsoleMsgText += (">> " + msgToAdd + "\n");
        if (ConsoleLog != null)
        {
            ConsoleLog.GetComponent<ConsoleLogUpdater>().UpdateErrorMessages();
            if (popUpConsoleWindow)
            {
                ConsoleLog.SetActive(true);
            }
        }
    }

    public static Color CreateColorFromIntArray(int[] values)
    {
        if (values.Length < 3)
        {
            return Color.white;
        }
        else
        {
            float r = (Mathf.Clamp(values[0] / 255f, 0f, 1f));
            float g = (Mathf.Clamp(values[1] / 255f, 0f, 1f));
            float b = (Mathf.Clamp(values[2] / 255f, 0f, 1f));
            float a = 1f;
            if (values.Length == 4)
            {
                a = (Mathf.Clamp(values[3] / 255f, 0f, 1f));
            }

            return new Color(r, g, b, a);
        }
    }

    public static void AddRemoteAssetLoadToList(string assetToLoad, int loadType = 0)
    {
        MessageList.PlaybackPaused = true;
        remoteAssetsInLoading.Add(assetToLoad);
        string msg;
        switch (loadType)
        {
            case 0:

                msg = "Loading " + assetToLoad + " model from Addressables.";
                break;

            case 1:
                string filetype = assetToLoad.EndsWith("obj") ? "OBJ" : "GLB";
                msg = $"Loading custom model from: {assetToLoad} with {filetype} runtime importer.";
                break;
            case 2:
                msg = $"Loading basic Unity 3D model: {assetToLoad}.";
                break;
            case 3:
                msg = $"Loading HD Material: {assetToLoad}";
                break;
            default:
                msg = $"Loading unknown type: {assetToLoad}";
                break;
        }

        UpdateErrorMessages(msg);
        StatusText += msg + "\n";
    }

    public static void PopRemoteAssetLoadFromList(string modelLoaded, bool isLoaded)
    {
        if (!String.IsNullOrEmpty(modelLoaded))
        {
            remoteAssetsInLoading.Remove(modelLoaded);

            string msg;
            if (isLoaded)
            {
                msg = modelLoaded + " asset loaded successfully.";
            }
            else
            {
                msg = modelLoaded + " asset load failed.";
            }

            UpdateErrorMessages(msg, !isLoaded);
            StatusText += msg;
        }

        if (remoteAssetsInLoading.Count == 0)
        {
            if (StartupCount == 10)
            {
                AssetLoadingComplete = true;
                bool stayPaused = false;
                VizMessage.Types.LiveVizSettingsPb liveSettings = MessageList.FirstMessage.LiveSettings;
                if (liveSettings != null)
                {
                    stayPaused = liveSettings.PlaybackPaused;
                }

                MessageList.SetNextIndex(0);
                MessageList.PlaybackPaused = stayPaused;
                PlaybackManager.EnablePlaybackControls(true, stayPaused);
            }
        }
    }

    public static void SetShellLighting(bool shellLightsOn)
    {
        UseShellLighting = shellLightsOn;
        if (CelestialBodyStateUtilities.SunMsgAvailable)
        {
            GameObject sun = GameObject.FindWithTag("Sun");
            sun.GetComponent<SunBuilder>().UseShellLighting();
        }
        else
        {
            GameObject mainLight = GameObject.FindWithTag("DefaultMainLight");
            mainLight.GetComponent<DirectionalLightPointingHandler>().UseShellLighting();
        }
    }

    public static void SetToggleInteractable(bool canInteract, Toggle toggle)
    {
        Color colorToSet = Color.gray;
        if (canInteract)
        {
            colorToSet = Color.white;
        }

        toggle.interactable = canInteract;
        toggle.GetComponentInChildren<TextMeshProUGUI>().color = colorToSet;
    }

    public static void SetButtonInteractable(bool canInteract, Button button, bool hasText = true,
        bool hasImage = false)
    {
        Color colorToSet = Color.gray;
        if (canInteract)
        {
            colorToSet = Color.white;
        }

        button.interactable = canInteract;

        if (hasText)
        {
            button.transform.GetComponentInChildren<TextMeshProUGUI>().color = colorToSet;
        }

        if (hasImage)
        {
            button.transform.GetChild(0).GetComponent<Image>().color = colorToSet;
        }
    }

    public static void DisplayTextInFadingStatusTextBox(string text)
    {
        FadingStatusTextBox.SetActive(true);
        FadingStatusTextBox.GetComponent<TextMeshProUGUI>().text = text;
    }

    public static void SetSecondaryCameraLayerMask(Camera cameraToSet, bool showHUDLayers)
    {
        cameraToSet.cullingMask =
            showHUDLayers ? secondaryCameraHUDLayersIncludedMask : secondaryCameraDefaultLayerMask;
    }

    public static void UpdateCSandLineRenderers()
    {
        CelestialBodyStateUtilities.UpdatePlanetCSVisibility();
        SpacecraftStateUtilities.UpdateSpacecraftCSVisibility();
        UpdateAllLineRenderers();
        SettingsPanel.UpdateScalesText();

        if (!DataManager.InNoDisplayMode)
        {
            if ((SetChiefToCamTgt) && (MainCameraUtilities.CameraTargetIsSpacecraftOrEffector))
            {
                //Effectors cannot be chief spacecraft
                RelativeTruePathChangeCount += 1;
                SpacecraftStateUtilities.UpdateChiefSpacecraft(MainCameraUtilities.CameraTargetIndex);
            }
        }
    }

    private static void SetHUDLineRenderersWidth()
    {
        HUDElementsLineWidth = 0.025f;

        HUDElementsLineWidth = SpacecraftStateUtilities.GetCurrentSpacecraftOrbitLineConstant();
        foreach (HUDLineRenderer line in HUDLines)
        {
            line.SetLineWidth(HUDElementsLineWidth);
        }
    }

    public static void UpdateAllLineRenderers()
    {
        SetHUDLineRenderersWidth();
        SpacecraftStateUtilities.UpdateSpacecraftOrbitLineWidth();
        CelestialBodyStateUtilities.UpdateCelestialBodyOrbitLineWidth();
        SettingsPanel.UpdateTargetLinesPointingLinesAndCoordinateFrames();
    }

    public static void ApplySpriteSettingString(string spriteSetting, GameObject spriteObject)
    {
        char[] sep = {' '};
        string[] splitSpriteSetting = spriteSetting.Split(sep);
        spriteObject.GetComponent<SpriteRenderer>().sprite =
            GetSprite(splitSpriteSetting[0]);
        if (splitSpriteSetting.Length >= 4)
        {
            spriteObject.GetComponent<SpriteRenderer>().color = new Color(
                float.Parse(splitSpriteSetting[1]) / 255f, float.Parse(splitSpriteSetting[2]) / 255f,
                float.Parse(splitSpriteSetting[3]) / 255f, 1.0f);
            if (splitSpriteSetting.Length >= 5)
            {
                float alpha = float.Parse(splitSpriteSetting[4]) / 255f;
                if (alpha > 0)
                {
                    spriteObject.GetComponent<SpriteRenderer>().color = new Color(
                        float.Parse(splitSpriteSetting[1]) / 255f, float.Parse(splitSpriteSetting[2]) / 255f,
                        float.Parse(splitSpriteSetting[3]) / 255f, alpha);
                }
            }
        }
    }

    public static void AddHUDLine(HUDLineRenderer newLine)
    {
        HUDLines.Add(newLine);
//		Debug.Log($"I am adding {newLine.transform.parent.parent.parent.parent.name}:{newLine.transform.parent.name}");
    }

    public static void SetVRRadialMenuActive(bool isActive)
    {
        isVRRadialMenuActive = isActive;
        float eventDialogPanelSize = isActive ? 0.001f : 1f;
        VizInputUtilities.eventDialogManager.eventDialogPanels.GetComponent<RectTransform>().localScale =
            eventDialogPanelSize * Vector3.one;
    }

    public static bool GetVRMenuActive()
    {
        return isVRRadialMenuActive;
    }

    public static int GetModelsInLoadingCount()
    {
        return remoteAssetsInLoading.Count;
    }

    public static void ResetVizardGUISettings()
    {
        //Coordinate Frames
        CameraTargetCSOn = false;
        AllPlanetCSOn = false;
        AllSpacecraftCSOn = false;
        AllEffectorCSOn = false;
        ShowHillFrame = false;
        ShowVelocityFrame = false;

        OsculatingOrbitLinesVisible = true;

        SpacecraftRelativeOsculatingOrbits = false;
        TruePathLinesVisible = false;

        TruePathLineMode = 1;
        SpacecraftRelativeOrbitMode = 1; // 1 for Hill Frame, 2 for Velocity Frame, 3 for Inertial
        ChiefSpacecraftIndex = 0; //use this to set relative spacecraft body for all three modes
        SetChiefToCamTgt = true;

        UseSpacecraftParentBodyForRelativeTraj = true;
        RelativeBodyIndex = -1;

        UseSpacecraftParentBodyForFixedFrameTraj = true;
        FixedBodyIndex = -1;
        FixedBodyIsSpacecraft = false;
        RotatingFrameBody1Index = -1;
        RotatingFrameBody2Index = -1;
        RelativeTruePathChangeCount = 0;

        //Instruments
        ShowCamBoresights = false;
        ShowCamFrustums = false;
        ShowCamPreviews = false;
        ShowStationCommunicationLines = true;
        ShowStationCone = true;

        //GUI Appearance
        HUDLines = new List<HUDLineRenderer>();

        //Labels
        ShowSpacecraftLabels = false;
        ShowEffectorLabels = false;
        ShowCelestialBodyLabels = false;
        ShowCameraLabels = false;
        ShowCSLabels = false;
        ShowThrusterLabels = false;
        ShowRWLabels = false;
        ShowCSSLabels = false;
        ShowLocationLabels = true;
        SomeSpacecraftLabelsAreOn = false;
        SomeCelestialBodyLabelsAreOn = false;
        ShowGenericSensorLabels = false;
        ShowTransceiverLabels = false;
        ShowLightLabels = false;
        ShowMSMLabels = false;

        //Sprites
        ShowSpritesForSpacecraft = false; //This will get changed to true by SpacecraftManager if s/c count >1
        ShowSpritesForPlanets = false;
        PlanetSpriteSize = 0.06f;
        SpacecraftSpriteSize = 0.04f;
        LocationMarkerSize = 0.006f;
        SpacecraftApparentSizeThreshold = 0.01f;

        //Lighting
        UseShellLighting = false;
        MainLight = null;

        //Shaders
        UseDefaultSpecularShader = true;
        UseAtmosphereShaderIfAvailable = true;

        //Locations
        UseSimpleMarkersForLocations = false;

        //References
        AssetLoadingComplete = false;
        remoteAssetsInLoading = new List<string>();
        StartupCount = 0;
        StatusText = "";
        PostProcessingMgr = null;
        ConsoleMsgText = "";
        ConsoleLog = null;

        isVRRadialMenuActive = false;
    }
}