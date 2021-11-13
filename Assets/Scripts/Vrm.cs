using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UniGLTF;
using VRM;
using VRMLoader;
using VRMShaders;

public class Vrm : MonoBehaviour
{
    [SerializeField] Transform Camera;
    [SerializeField] Meta Meta;
    [SerializeField] BlendShape BlendShape;
    [SerializeField] Export Export;

    [SerializeField] Text Text;

    [SerializeField] Canvas m_canvas;
    [SerializeField] GameObject m_modalWindowPrefab;

    public GameObject VRM = null;
    VRMImporterContext Context;
    string Path;

    /// <summary>
    /// vrm読み込み確認
    /// </summary>
    public void Load(string path)
    {
        var bytes = File.ReadAllBytes(path);

        var data = new GlbFileParser(path).Parse();
        var vrm = new VRMData(data);
        Context = new VRMImporterContext(vrm);

        // VRMLoaderUI
        var modalObject = Instantiate(m_modalWindowPrefab, m_canvas.transform) as GameObject;
        var modalUI = modalObject.GetComponentInChildren<VRMPreviewUI>();
        modalUI.setMeta(Context.ReadMeta(true));
        modalUI.setLoadable(true);
        modalUI.m_ok.onClick.AddListener(Load);

        Path = path;
    }

    /// <summary>
    /// vrm読み込み
    /// </summary>
    void Load()
    {
        if (VRM != null)
        {
            Destroy(VRM);
        }

        var loaded = default(RuntimeGltfInstance);
        loaded = Context.Load();
        loaded.ShowMeshes();

        VRM = loaded.Root;

        // 3Dビューの初期表示位置を頭に設定
        var anim = VRM.GetComponent<Animator>();
        var head = anim.GetBoneTransform(HumanBodyBones.Head);
        Camera.position = new Vector3(0, head.position.y, 0);

        // メタ情報とブレンドシェイプの取得
        Meta.Get();
        BlendShape.Get();

        Export.Path = Path;

        Text.gameObject.SetActive(false);
    }

    /// <summary>
    /// vrm書き出し
    /// </summary>
    public void Save(string path)
    {
        if (VRM == null)
        {
            return;
        }

        // メタ情報とブレンドシェイプを更新
        Meta.Set();
        BlendShape.Set();

        var vrm = VRMExporter.Export(new GltfExportSettings(), VRM, new RuntimeTextureSerializer());
        var bytes = vrm.ToGlbBytes();
        File.WriteAllBytes(path, bytes);
    }
}
