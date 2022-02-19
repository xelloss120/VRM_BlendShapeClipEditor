using UnityEngine;
using UnityEngine.UI;

public class ApplyView : MonoBehaviour
{
    [SerializeField] Camera Camera;
    [SerializeField] Image Image;

    /// <summary>
    /// 3Dビューの表示をVRMのサムネイルに適用
    /// </summary>
    public void Set()
    {
        var cam = Camera.targetTexture;
        var tex = new Texture2D(cam.width, cam.height, TextureFormat.RGB24, false);

        RenderTexture.active = cam;
        tex.ReadPixels(new Rect(0, 0, cam.width, cam.height), 0, 0);
        var color = tex.GetPixels();
        for (int i = 0; i < color.Length; i++)
        {
            color[i].r = Mathf.LinearToGammaSpace(color[i].r);
            color[i].g = Mathf.LinearToGammaSpace(color[i].g);
            color[i].b = Mathf.LinearToGammaSpace(color[i].b);
        }
        tex.SetPixels(color);
        tex.Apply();

        Image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero); ;
    }
}
