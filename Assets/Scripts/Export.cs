using UnityEngine;

public class Export : MonoBehaviour
{
    [SerializeField] Vrm Vrm;
    [SerializeField] Csv Csv;

    public string Path;

    /// <summary>
    /// エクスポート（vrmとcsvを一緒に保存）
    /// </summary>
    public void Save()
    {
#if UNITY_EDITOR
        var path = Application.dataPath + "/../_Debug/Export.vrm";
#else
        var path = VRM.SimpleViewer.FileDialogForWindows.SaveDialog("Save VRM", Path);
#endif
        if (string.IsNullOrEmpty(path))
        {
            return;
        }
        else if (string.Compare(System.IO.Path.GetExtension(path), ".vrm", true) != 0)
        {
            path += ".vrm";
        }

        Vrm.Save(path);
        Csv.Save(path.Replace(".vrm", ".csv"));
    }
}
