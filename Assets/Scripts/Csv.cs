using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using VRM;
using UniGLTF;

public class Csv : MonoBehaviour
{
    [SerializeField] Vrm Vrm;
    [SerializeField] BlendShape BlendShape;

    /// <summary>
    /// csvの行列
    /// </summary>
    const int CSV_ROW_LABEL = 0;
    const int CSV_ROW_ISBINARY = 1;
    const int CSV_ROW_WEIGHT = 2;
    const int CSV_COL_PATH = 0;
    const int CSV_COL_INDEX = 1;
    const int CSV_COL_NAME = 2;
    const int CSV_COL_WEIGHT = 3;

    /// <summary>
    /// 表情プリセットにcsvから設定を読み込み
    /// </summary>
    public void Load(string path)
    {
        if (Vrm.VRM == null)
        {
            return;
        }

        // csv読み込み
        var list = new List<List<string>>();
        var reader = new StreamReader(path);
        while (reader.Peek() >= 0)
        {
            list.Add(new List<string>());
            string[] cols = reader.ReadLine().Split(',');
            for (int n = 0; n < cols.Length; n++)
            {
                list[list.Count - 1].Add(cols[n]);
            }
        }
        reader.Close();

        var index = CSV_ROW_WEIGHT;
        var meshes = Vrm.VRM.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer mesh in meshes)
        {
            var meshPath = mesh.transform.RelativePathFrom(Vrm.VRM.transform);
            for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
            {
                var meshName = mesh.sharedMesh.GetBlendShapeName(i);
                if (list[index][CSV_COL_PATH] != meshPath ||
                    list[index][CSV_COL_INDEX] != i.ToString() ||
                    list[index][CSV_COL_NAME] != meshName)
                {
                    // オブジェクトのパス、インデックス、ブレンドシェイプ名が一致しなければ中断
                    return;
                }
                index++;
            }
        }

        var proxy = Vrm.VRM.GetComponent<VRMBlendShapeProxy>();
        var clips = proxy.BlendShapeAvatar.Clips;
        if (clips.Count != list[CSV_ROW_LABEL].Count - CSV_COL_WEIGHT)
        {
            // 表情プリセットの数が一致しなければ中断
            return;
        }

        // 表情プリセット設定
        for (int col = CSV_COL_WEIGHT; col < list[CSV_ROW_LABEL].Count; col++)
        {
            var clip = clips[col - CSV_COL_WEIGHT];
            clip.IsBinary = bool.Parse(list[CSV_ROW_ISBINARY][col]);

            var values = new List<BlendShapeBinding>();
            for (int row = CSV_ROW_WEIGHT; row < list.Count; row++)
            {
                if (list[row][col] != "0")
                {
                    // 0でない値が設定されていた場合だけ表情プリセットに追加
                    var shape = new BlendShapeBinding();
                    shape.RelativePath = list[row][CSV_COL_PATH];
                    shape.Index = int.Parse(list[row][CSV_COL_INDEX]);
                    shape.Weight = float.Parse(list[row][col]);
                    values.Add(shape);
                }
            }

            clip.Values = values.ToArray();
            proxy.BlendShapeAvatar.Clips[col - CSV_COL_WEIGHT] = clip;
        }

        BlendShape.Get();
    }

    /// <summary>
    /// 表情プリセットの設定をcsvで書き出し
    /// </summary>
    public void Save(string path)
    {
        if (Vrm.VRM == null)
        {
            return;
        }

        var list = new List<List<string>>();

        // 1行目はラベル（表情プリセット名も）
        list.Add(new List<string>());
        list[CSV_ROW_LABEL].Add("RelativePath");
        list[CSV_ROW_LABEL].Add("Index");
        list[CSV_ROW_LABEL].Add("Name");

        // 2行目はIsBinary
        list.Add(new List<string>());
        list[CSV_ROW_ISBINARY].Add("");
        list[CSV_ROW_ISBINARY].Add("");
        list[CSV_ROW_ISBINARY].Add("IsBinary");

        // 1行目と2行目に表情プリセット名とIsBinaryを書き出し
        var proxy = Vrm.VRM.GetComponent<VRMBlendShapeProxy>();
        foreach (BlendShapeClip clip in proxy.BlendShapeAvatar.Clips)
        {
            list[CSV_ROW_LABEL].Add(clip.BlendShapeName);
            list[CSV_ROW_ISBINARY].Add(clip.IsBinary.ToString());
        }

        // 基準は全メッシュから繰り返し（かなり気持ち悪い多重ループ）
        var meshes = proxy.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer mesh in meshes)
        {
            // メッシュに含まれるブレンドシェイプで繰り返し
            for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
            {
                var name = mesh.sharedMesh.GetBlendShapeName(i);

                // 1～3列目（オブジェクトのパス、インデックス、ブレンドシェイプ名）を書き出し
                list.Add(new List<string>());
                list[list.Count - 1].Add(mesh.transform.RelativePathFrom(proxy.transform));
                list[list.Count - 1].Add(i.ToString());
                list[list.Count - 1].Add(name);

                // 表情プリセットで繰り返し
                foreach (BlendShapeClip clip in proxy.BlendShapeAvatar.Clips)
                {
                    // 一旦0で書き出し
                    list[list.Count - 1].Add("0");
                    foreach (BlendShapeBinding b in clip.Values)
                    {
                        var proxyMesh = proxy.transform.Find(b.RelativePath).GetComponent<SkinnedMeshRenderer>();
                        var proxyName = proxyMesh.sharedMesh.GetBlendShapeName(b.Index);
                        if (name == proxyName)
                        {
                            // 表情プリセットに含まれる名前と一致したら値を書き出し
                            list[list.Count - 1][list[list.Count - 1].Count - 1] = b.Weight.ToString();
                            break;
                        }
                    }
                }
            }
        }

        // csv保存
        var csv = "";
        foreach (List<string> line in list)
        {
            csv += String.Join(",", line) + "\n";
        }
        File.WriteAllText(path, csv);
    }
}
