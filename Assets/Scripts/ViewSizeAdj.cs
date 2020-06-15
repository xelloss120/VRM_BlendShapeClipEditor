using UnityEngine;

public class ViewSizeAdj : MonoBehaviour
{
    [SerializeField] RectTransform Panel;
    [SerializeField] RectTransform RawImage;

    /// <summary>
    /// 3Dビューを正方形に維持
    /// </summary>
    void Update()
    {
        var size = Mathf.Min(Panel.rect.width, Panel.rect.height);
        RawImage.localScale = Vector3.one * size;
    }
}
