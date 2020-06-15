using UnityEngine;

public class CtrlView : MonoBehaviour
{
    [SerializeField] float Rot = 1;
    [SerializeField] float Pos = 0.005f;
    [SerializeField] float Size = 0.2f;

    [SerializeField] Camera Camera;
    [SerializeField] Transform ViewPoint;

    [SerializeField] RectTransform Left;
    [SerializeField] RectTransform Center;

    Vector3 MousePos;

    /// <summary>
    /// 3Dビューのマウス操作
    /// </summary>
    void Update()
    {
        var mousePosDif = MousePos - Input.mousePosition;
        MousePos = Input.mousePosition;

        if (MousePos.y < 0 ||
            MousePos.y > Center.rect.height ||
            MousePos.x < Left.rect.width ||
            MousePos.x > Left.rect.width + Center.rect.width)
        {
            // 範囲外なら中断
            return;
        }

        if (Input.GetMouseButton(0))
        {
            // 左クリック（回転）
            var tmp = mousePosDif.x;
            mousePosDif.x = mousePosDif.y;
            mousePosDif.y = tmp;
            ViewPoint.eulerAngles -= mousePosDif * Camera.orthographicSize * Rot;
        }
        else if (Input.GetMouseButton(1))
        {
            // 右クリック（移動）
            mousePosDif.x *= -1;
            Camera.transform.localPosition += mousePosDif * Camera.orthographicSize * Pos;
        }

        // スクロール（拡縮というか前後）
        var scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Camera.orthographicSize < 0.01 && scroll > 0)
        {
            scroll = 0;
        }
        Camera.orthographicSize -= scroll * Size;
    }
}
