using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Img : MonoBehaviour
{
    [SerializeField] Image Image;

    /// <summary>
    /// 画像をUIに設定
    /// </summary>
    public void Load(string path)
    {
        FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryReader br = new BinaryReader(fs);
        byte[] read = br.ReadBytes((int)br.BaseStream.Length);
        br.Close();

        var tex = new Texture2D(1, 1);
        tex.LoadImage(read);

        Image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
    }
}
