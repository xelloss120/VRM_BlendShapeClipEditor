using UnityEngine;
using UnityEngine.UI;

public class SliderToInput : MonoBehaviour
{
    [SerializeField] Slider Slider;
    [SerializeField] InputField Input;

    public int Index;
    public SkinnedMeshRenderer Mesh;

    /// <summary>
    /// スライダーを操作した時にインプットフィールドへ値を反映
    /// </summary>
    public void Chenged()
    {
        Input.text = Mathf.RoundToInt(Slider.value).ToString();
        if (Mesh != null)
        {
            // スライダーからブレンドシェイプへ値を反映
            Mesh.SetBlendShapeWeight(Index, Slider.value);
        }
    }
}
