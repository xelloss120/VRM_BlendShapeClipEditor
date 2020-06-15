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
        var path = VRM.Samples.FileDialogForWindows.SaveDialog("save VRM", Path);
#endif
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        Vrm.Save(path);
        Csv.Save(path);
    }
}
