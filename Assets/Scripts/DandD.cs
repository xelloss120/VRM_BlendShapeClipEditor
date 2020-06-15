using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using B83.Win32;

public class DandD : MonoBehaviour
{
    [SerializeField] Vrm Vrm;
    [SerializeField] Csv Csv;
    [SerializeField] Img Img;

    [SerializeField] Dropdown Dropdown;

    void OnEnable()
    {
        UnityDragAndDropHook.InstallHook();
        UnityDragAndDropHook.OnDroppedFiles += OnFiles;
    }

    void OnDisable()
    {
        UnityDragAndDropHook.UninstallHook();
    }

    /// <summary>
    /// ドラッグ&ドロップで各種形式を読み込み
    /// </summary>
    void OnFiles(List<string> aFiles, POINT aPos)
    {
        foreach (string f in aFiles)
        {
            if (string.Compare(Path.GetExtension(f), ".vrm", true) == 0)
            {
                Vrm.Load(f);
                break;
            }
            if (string.Compare(Path.GetExtension(f), ".csv", true) == 0)
            {
                Csv.Load(f);
                break;
            }
            if (string.Compare(Path.GetExtension(f), ".jpg", true) == 0 ||
                string.Compare(Path.GetExtension(f), ".png", true) == 0)
            {
                Img.Load(f);
                break;
            }
        }
    }

    /// <summary>
    /// デバッグ機能（vrm読み込み）
    /// </summary>
    public void DebugVrm()
    {
        switch (Dropdown.value)
        {
            case 1:
                SetOnFile("/../_Debug/ModelA.vrm");
                break;
            case 2:
                SetOnFile("/../_Debug/ModelB.vrm");
                break;
            case 3:
                SetOnFile("/../_Debug/ModelC.vrm");
                break;
            case 4:
                SetOnFile("/../_Debug/ModelD.vrm");
                break;
            case 5:
                SetOnFile("/../_Debug/ModelE.vrm");
                break;
        }
    }

    /// <summary>
    /// デバッグ機能（csv読み込み）
    /// </summary>
    public void DebugCsv()
    {
        SetOnFile("/../_Debug/Export.csv");
    }

    /// <summary>
    /// デバッグ機能（png読み込み）
    /// </summary>
    public void DebugImg()
    {
        SetOnFile("/../_Debug/thumbnail.png");
    }

    /// <summary>
    /// デバッグ機能（ファイル読み込み）
    /// </summary>
    void SetOnFile(string path)
    {
        var aPos = new POINT(0, 0);
        var aFiles = new List<string>();
        aFiles.Add(@Application.dataPath + path);
        OnFiles(aFiles, aPos);
    }
}
