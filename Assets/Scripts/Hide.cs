using UnityEngine;

public class Hide : MonoBehaviour
{
    /// <summary>
    /// デバッグ機能（デバッグ機能用UIの非表示）
    /// </summary>
    void Start()
    {
#if !UNITY_EDITOR
        gameObject.SetActive(false);
#endif
    }
}
