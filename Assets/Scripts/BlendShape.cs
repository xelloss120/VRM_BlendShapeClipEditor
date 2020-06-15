using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VRM;
using UniGLTF;

public class BlendShape : MonoBehaviour
{
    [SerializeField] Vrm Vrm;
    [SerializeField] Dropdown Dropdown;
    [SerializeField] GameObject Content;
    [SerializeField] GameObject IsBinaryPrefab;
    [SerializeField] GameObject BlendShapePrefab;

    int PreviousIndex;
    VRMBlendShapeProxy Proxy;

    /// <summary>
    /// VRMから表情プリセットを取得
    /// </summary>
    public void Get()
    {
        var proxy = Vrm.VRM.GetComponent<VRMBlendShapeProxy>();
        var clips = proxy.BlendShapeAvatar.Clips;

        // ドロップダウンに表情プリセットを設定
        Dropdown.ClearOptions();
        for (int i = 0; i < clips.Count; i++)
        {
            var item = new Dropdown.OptionData(clips[i].BlendShapeName);
            Dropdown.options.Add(item);
        }
        Dropdown.RefreshShownValue();

        PreviousIndex = 0;

        Proxy = proxy;

        SetContent();
    }

    /// <summary>
    /// エクスポート時に表情プリセットを設定
    /// </summary>
    public void Set()
    {
        SetClip(Dropdown.value);
    }

    /// <summary>
    /// ドロップダウン操作時に表情プリセットを設定
    /// </summary>
    public void DropdownChanged()
    {
        SetClip(PreviousIndex);
        SetContent();

        PreviousIndex = Dropdown.value;
    }

    /// <summary>
    /// 選択中の表情プリセットでUIを設定
    /// </summary>
    void SetContent()
    {
        // 一旦空にする
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }

        // 最初に表情設定を全部0にする
        var meshes = Proxy.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer mesh in meshes)
        {
            for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
            {
                mesh.SetBlendShapeWeight(i, 0);
            }
        }

        var clips = Proxy.BlendShapeAvatar.Clips;
        var clip = clips[Dropdown.value];

        // IsBinaryのUI設定
        var isBinary = Instantiate(IsBinaryPrefab);
        isBinary.transform.parent = Content.transform;
        isBinary.transform.Find("Toggle").GetComponent<Toggle>().isOn = clip.IsBinary;

        // ブレンドシェイプのUI設定
        foreach (SkinnedMeshRenderer mesh in meshes)
        {
            for (int i = 0; i < mesh.sharedMesh.blendShapeCount; i++)
            {
                var blendShape = Instantiate(BlendShapePrefab);
                blendShape.transform.parent = Content.transform;

                var text = blendShape.transform.Find("Text");
                var slider = blendShape.transform.Find("Slider");
                var input = blendShape.transform.Find("InputField");

                // 一旦表情設定0でUIを設定
                var name = mesh.sharedMesh.GetBlendShapeName(i);
                text.GetComponent<Text>().text = name;
                slider.GetComponent<SliderToInput>().Mesh = mesh;
                slider.GetComponent<SliderToInput>().Index = i;
                slider.GetComponent<Slider>().value = 0;

                // 表情プリセット
                foreach (BlendShapeBinding b in clip.Values)
                {
                    var obj = Proxy.transform.Find(b.RelativePath);
                    if (obj != null)
                    {
                        var objMesh = obj.GetComponent<SkinnedMeshRenderer>();
                        var objName = objMesh.sharedMesh.GetBlendShapeName(b.Index);
                        if (name == objName)
                        {
                            // 表情プリセットに含まれる名前と一致したら値を設定
                            slider.GetComponent<Slider>().value = b.Weight;
                            break;
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 表示中のUIから表情プリセットを設定
    /// </summary>
    void SetClip(int index)
    {
        var clips = Proxy.BlendShapeAvatar.Clips;
        var clip = clips[index];
        var values = new List<BlendShapeBinding>();

        foreach (Transform child in Content.transform)
        {
            // IsBinaryは最初の一回だけ（気持ち悪いけど我慢する）
            var toggle = child.transform.Find("Toggle");
            if (toggle != null)
            {
                clip.IsBinary = toggle.GetComponent<Toggle>().isOn;
            }

            // ブレンドシェイプ
            var text = child.transform.Find("Text");
            var slider = child.transform.Find("Slider");
            var input = child.transform.Find("InputField");
            if (text != null && slider != null && input != null)
            {
                var value = slider.GetComponent<Slider>().value;
                if (value != 0)
                {
                    // UIに0でない値が設定されていた場合だけ表情プリセットに追加
                    var s2i = slider.GetComponent<SliderToInput>();
                    var shape = new BlendShapeBinding();
                    shape.RelativePath = s2i.Mesh.transform.RelativePathFrom(Proxy.transform);
                    shape.Index = s2i.Index;
                    shape.Weight = value;
                    values.Add(shape);
                }
            }
        }

        clip.Values = values.ToArray();
        Proxy.BlendShapeAvatar.Clips[index] = clip;
    }
}
