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
using System.IO;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
/// <summary>
/// Manages loading requested models and materials from Unity
/// Addressables bundles located within the CustomModels directory.
/// Location of directory varies by platform:
/// MacOS: ~/Library/Application Support/Vizard/Vizard/Resources/CustomModels
/// Linux: home/your_user_name/.config/unity3d/Vizard/Vizard/Resources/CustomModels
/// Windows: C:/Users/your_user_name/AppData/LocalLow/Vizard/Vizard/Resources/CustomModels
/// <remarks>
/// Build your own remote bundles for distribution with the Vizard Public Content Unity Project
/// provided at: https://avslab.github.io/basilisk/Vizard/vizardAdvanced/vizardCustomModels.html</remarks>
/// </summary>
public static class GoodEnoughAddressables 
{
    public static string pathToSecondaryCatalogs;

    private static List<string> catalogPaths = new List<string>();
    private static List<IResourceLocator> catalogLocations = new List<IResourceLocator>();
    public static bool AllRemoteCatalogsLoaded = false;
    
    // Start is called before the first frame update

    private static void FindAllSecondaryCatalogs()
    {
        DirectoryInfo modDirectory = new DirectoryInfo(pathToSecondaryCatalogs);
        try
        {
            if (modDirectory.Exists)
            {
                bool catalogJsonFound = false;
                foreach (FileInfo file in modDirectory.GetFiles())
                {
                    if (file.Extension == ".json")
                    {
                        catalogPaths.Add(file.FullName);
                        catalogJsonFound = true;
                    }
                }
                if (!catalogJsonFound)
                {
                    string errorString = $"There were no catalog .json files found in the {pathToSecondaryCatalogs}";
                    VizardGUISettings.UpdateErrorMessages(errorString);
                }
            }
            else
            {
                modDirectory.Create();
                VizardGUISettings.UpdateErrorMessages($"The secondary data path directory did not exist and was created: {pathToSecondaryCatalogs}");
            }
        }
        catch (Exception e)
        {
            VizardGUISettings.UpdateErrorMessages($"The process failed: {e}");
        }
    }
    private static async void LoadSecondaryCatalogs()
    {
        foreach (string path in catalogPaths)
        {
             await LoadRemoteCatalog(path);
        }
    }
    
    private static async Task<IResourceLocator> LoadRemoteCatalog(string filepath)
    {
        AsyncOperationHandle<IResourceLocator> op =
            Addressables.LoadContentCatalogAsync(filepath);

        IResourceLocator modLocator = await op.Task;
        catalogLocations.Add(modLocator);
        
        if (catalogLocations.Count == catalogPaths.Count)
        {
            AllRemoteCatalogsLoaded = true;
            CelestialBodyStateUtilities.LoadAllAtmosphereMaterials();
        }
        return modLocator;
    }

    public static void InitializeAddressables()
    {
        pathToSecondaryCatalogs = Application.persistentDataPath + "/Resources/CustomModels";
        FindAllSecondaryCatalogs();
        LoadSecondaryCatalogs();
    }

}
