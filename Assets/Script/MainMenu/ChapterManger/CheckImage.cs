using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 章节图片查看窗口的拖动逻辑（桌面中键 / 移动端单指）。
/// 优化：消除每帧 new Vector3[4] 与 GetComponent 分配；修正 GetWorldCorners 直接改角点数组的隐患；
/// 移动增量不再乘以 Time.fixedDeltaTime（原写法在 Update 里用固定步长导致速度与帧率无关但值异常）。
/// </summary>
public class CheckImage : MonoBehaviour
{
    public GameObject 查看窗口;
    public RawImage 映射;

    private RectTransform 映射Rect;
    private readonly Vector3[] corners = new Vector3[4];

    void Awake()
    {
        映射Rect = 映射.GetComponent<RectTransform>();
    }

    public void Click()
    {
        RawImage source = GetComponent<RawImage>();
        映射.texture = source.texture;

        Vector2 canvasSize = GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta;
        float screenxyRate = canvasSize.x / canvasSize.y;
        Vector2 bgSize = new(source.texture.width, source.texture.height);

        //视频尺寸长宽比
        float texturexyRate = bgSize.x / bgSize.y;

        if (texturexyRate > screenxyRate)
        {
            int newSizeY = Mathf.CeilToInt(canvasSize.y);
            int newSizeX = Mathf.CeilToInt((float)newSizeY / bgSize.y * bgSize.x);
            映射Rect.sizeDelta = new Vector2(newSizeX, newSizeY);
        }
        else
        {
            int newVideoSizeX = Mathf.CeilToInt(canvasSize.x);
            int newVideoSizeY = Mathf.CeilToInt((float)newVideoSizeX / bgSize.x * bgSize.y);
            映射Rect.sizeDelta = new Vector2(newVideoSizeX, newVideoSizeY);
        }

        查看窗口.SetActive(true);
    }

    void Update()
    {
        Vector2 增量 = Vector2.zero;

        if (Application.isMobilePlatform)
        {
            int touchCount = Input.touchCount;  // 获取触控点个数

            if (touchCount == 1)
                增量 = Input.GetTouch(0).deltaPosition * 15f;
        }
        else
        {
            if (Input.GetMouseButton(2))
            {
                Cursor.visible = false;
                增量 = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 250f;
            }
            else
                Cursor.visible = true;
        }

        if (增量 == Vector2.zero)
            return;

        //复用角点数组，不再每帧分配
        映射Rect.GetWorldCorners(corners);

        Vector2 newPosX_LU = corners[1] + (Vector3)增量;
        Vector2 newPosX_RB = corners[3] + (Vector3)增量;

        //仅在图片仍覆盖屏幕两侧时允许平移（防止拖出视野）
        if (newPosX_LU.x < 0 && newPosX_RB.x > Screen.width)
            映射Rect.localPosition += new Vector3(增量.x, 0, 0);

        if (newPosX_RB.y < 0 && newPosX_LU.y > Screen.height)
            映射Rect.localPosition += new Vector3(0, 增量.y, 0);
    }
}
