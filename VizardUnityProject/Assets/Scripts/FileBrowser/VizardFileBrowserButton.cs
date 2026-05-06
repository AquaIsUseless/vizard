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
/// Handles the button behavior for a file browser inventory button
/// </summary>
public class VizardFileBrowserButton : MonoBehaviour
{
    private VizardFileBrowser fileChooser;
    private bool isDirectory;
    public GameObject binIcon;
    public GameObject directoryIcon;
    public GameObject fileIcon;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(FileSelected);
    }

    private void FileSelected()
    {
        fileChooser.FileSelected(GetComponentInChildren<TextMeshProUGUI>().text, isDirectory);
    }

    public void SetButtonType(string ext, VizardFileBrowser fileBrowser)
    {
        fileChooser = fileBrowser;
        switch (ext)
        {
            case "dir":
                isDirectory = true;
                directoryIcon.SetActive(true);
                binIcon.SetActive(false);
                break;
            case "*.bin":
                break;
            case "*.obj":
                fileIcon.SetActive(true);
                binIcon.SetActive(false);
                break;
            case "*.glb":
                fileIcon.SetActive(true);
                binIcon.SetActive(false);
                break;
            case "jpg|bmp|exr|gif|hdr|iff|pict|png|psd|tga|tiff":
                fileIcon.SetActive(true);
                binIcon.SetActive(false);
                break;
        }
    }
}
