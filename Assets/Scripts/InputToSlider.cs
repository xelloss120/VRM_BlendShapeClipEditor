using UnityEngine;
using UnityEngine.UI;

public class InputToSlider : MonoBehaviour
{
    [SerializeField] InputField Input;
    [SerializeField] Slider Slider;

    /// <summary>
    /// インプットフィールドを操作した時にスライダーへ値を反映
    /// </summary>
    public void Chenged()
    {
        if (Input.text == "")
        {
            Input.text = "0";
        }
        Slider.value = float.Parse(Input.text);
    }
}
