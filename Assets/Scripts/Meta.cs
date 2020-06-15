using UnityEngine;
using UnityEngine.UI;
using VRM;

public class Meta : MonoBehaviour
{
    [SerializeField] Vrm Vrm;
    [SerializeField] Image Thumbnail;
    [SerializeField] InputField Title;
    [SerializeField] InputField Version;
    [SerializeField] InputField Author;
    [SerializeField] InputField Contact;
    [SerializeField] InputField Reference;
    [SerializeField] ApplyView ApplyView;
    [SerializeField] Sprite Sprite;

    /// <summary>
    /// メタ情報の取得
    /// </summary>
    public void Get()
    {
        var meta = Vrm.VRM.GetComponent<VRMMeta>();

        var tex = meta.Meta.Thumbnail;
        if (tex != null)
        {
            Thumbnail.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }
        else
        {
            // サムネイルが未設定の場合は強制的に3Dビューの表示状態を適用
            Thumbnail.sprite = Sprite;
            Invoke("SetThumbnail", 3);// 3秒後なのは読込待ち（雑なので間に合わない場合あり）
        }

        Title.text = meta.Meta.Title;
        Version.text = meta.Meta.Version;
        Author.text = meta.Meta.Author;
        Contact.text = meta.Meta.ContactInformation;
        Reference.text = meta.Meta.Reference;
    }

    /// <summary>
    /// メタ情報の設定
    /// </summary>
    public void Set()
    {
        var meta = Vrm.VRM.GetComponent<VRMMeta>();

        meta.Meta.Thumbnail = Thumbnail.sprite.texture;

        meta.Meta.Title = Title.text;
        meta.Meta.Version = Version.text;
        meta.Meta.Author = Author.text;
        meta.Meta.ContactInformation = Contact.text;
        meta.Meta.Reference = Reference.text;
    }

    /// <summary>
    /// 3Dビューの表示をVRMのサムネイルに適用
    /// </summary>
    void SetThumbnail()
    {
        ApplyView.Set();
    }
}
