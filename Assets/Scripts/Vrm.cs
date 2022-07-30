using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniGLTF;
using UniGLTF.MeshUtility;
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

        // 重力設定による再出力時の変形を防ぐため無効に設定
        var sb_list = VRM.GetComponentsInChildren<VRMSpringBone>();
        foreach (var sb in sb_list)
        {
            sb.enabled = false;
        }

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
    public void Save(string path, bool Reduce)
    {
        if (VRM == null)
        {
            return;
        }

        // メタ情報とブレンドシェイプを更新
        Meta.Set();
        BlendShape.Set();

        ExportingGltfData vrm;
        if (Reduce)
        {
            ReplaceMesh(VRM);
            vrm = VRMExporter.Export(new GltfExportSettings(), VRM, new RuntimeTextureSerializer());
        }
        else
        {
            vrm = VRMExporter.Export(new GltfExportSettings(), VRM, new RuntimeTextureSerializer());
        }
        var bytes = vrm.ToGlbBytes();
        File.WriteAllBytes(path, bytes);
    }

    /// <summary>
    /// 未使用のBlendShapeを間引く
    /// </summary>
    /// <remarks>
    /// .\VRM\Editor\Format\VRMEditorExporter.cs
    /// </remarks>
    void ReplaceMesh(GameObject target)
    {
        // 元のBlendShapeClipに変更を加えないように複製
        var proxy = target.GetComponent<VRMBlendShapeProxy>();
        if (proxy != null)
        {
            var copyBlendShapeAvatar = CopyBlendShapeAvatar(proxy.BlendShapeAvatar, true);
            proxy.BlendShapeAvatar = copyBlendShapeAvatar;

            // BlendShape削減
            if (true)
            {
                foreach (SkinnedMeshRenderer smr in target.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    // 未使用のBlendShapeを間引く
                    ReplaceMesh(target, smr, copyBlendShapeAvatar);
                }
            }
        }
    }

    /// <summary>
    /// 元のBlendShapeClipに変更を加えないように複製
    /// </summary>
    /// <remarks>
    /// .\VRM\Editor\Format\VRMEditorExporter.cs
    /// </remarks>
    static BlendShapeAvatar CopyBlendShapeAvatar(BlendShapeAvatar src, bool removeUnknown)
    {
        var avatar = GameObject.Instantiate(src);
        avatar.Clips = new List<BlendShapeClip>();
        foreach (var clip in src.Clips)
        {
            if (removeUnknown && clip.Preset == BlendShapePreset.Unknown)
            {
                continue;
            }
            avatar.Clips.Add(GameObject.Instantiate(clip));
        }
        return avatar;
    }

    /// <summary>
    /// 使用されない BlendShape を間引いた Mesh を作成して置き換える
    /// </summary>
    /// <remarks>
    /// .\VRM\Editor\Format\VRMEditorExporter.cs
    /// </remarks>
    void ReplaceMesh(GameObject target, SkinnedMeshRenderer smr, BlendShapeAvatar copyBlendShapeAvatar)
    {
        Mesh mesh = smr.sharedMesh;
        if (mesh == null) return;
        if (mesh.blendShapeCount == 0) return;

        // Mesh から BlendShapeClip からの参照がある blendShape の index を集める
        var usedBlendshapeIndexArray = copyBlendShapeAvatar.Clips
            .SelectMany(clip => clip.Values)
            .Where(val => target.transform.Find(val.RelativePath) == smr.transform)
            .Select(val => val.Index)
            .Distinct()
            .ToArray();

        var copyMesh = mesh.Copy(copyBlendShape: false);
        // 使われている BlendShape だけをコピーする
        foreach (var i in usedBlendshapeIndexArray)
        {
            var name = mesh.GetBlendShapeName(i);
            var vCount = mesh.vertexCount;
            var vertices = new Vector3[vCount];
            var normals = new Vector3[vCount];
            var tangents = new Vector3[vCount];
            mesh.GetBlendShapeFrameVertices(i, 0, vertices, normals, tangents);

            copyMesh.AddBlendShapeFrame(name, 100f, vertices, normals, tangents);
        }

        // BlendShapeClip の BlendShapeIndex を更新する(前に詰める)
        var indexMapper = usedBlendshapeIndexArray
            .Select((x, i) => new { x, i })
            .ToDictionary(pair => pair.x, pair => pair.i);
        foreach (var clip in copyBlendShapeAvatar.Clips)
        {
            for (var i = 0; i < clip.Values.Length; ++i)
            {
                var value = clip.Values[i];
                if (target.transform.Find(value.RelativePath) != smr.transform) continue;
                value.Index = indexMapper[value.Index];
                clip.Values[i] = value;
            }
        }

        // mesh を置き換える
        smr.sharedMesh = copyMesh;
    }
}
