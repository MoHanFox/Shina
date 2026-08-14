using UnityEngine;

/// <summary>
/// 背景跟随鼠标/触摸平移。
/// 优化：消除原实现每帧 new Vector3[4] 与 GetComponent 分配，缓存引用、复用角点数组。
/// </summary>
public class AudiobookBgMove : MonoBehaviour
{
    public RectTransform QTE;
    public RectTransform Background;

    private RectTransform backgroundRect;
    private readonly Vector3[] corners = new Vector3[4];

    void Awake()
    {
        backgroundRect = Background.GetComponent<RectTransform>();
    }

    void Update()
    {
        float mouseX = 0f;
        float mouseY = 0f;

        if (Application.isMobilePlatform)
        {
            int touchCount = Input.touchCount;  // 获取触控点个数

            if (touchCount == 1)
            {
                mouseX = Input.GetTouch(0).deltaPosition.x * 20;
                mouseY = Input.GetTouch(0).deltaPosition.y * 20;
            }
        }
        else
        {
            if (Input.GetMouseButton(1))
            {
                Cursor.visible = false;
                mouseX = Input.GetAxis("Mouse X") * 250;
                mouseY = Input.GetAxis("Mouse Y") * 250;
            }
            else
                Cursor.visible = true;
        }

        backgroundRect.GetWorldCorners(corners);

        Vector2 newPosX_LU = corners[1] + new Vector3(mouseX, mouseY) * Time.fixedDeltaTime;
        Vector2 newPosX_RB = corners[3] + new Vector3(mouseX, mouseY) * Time.fixedDeltaTime;

        if (newPosX_LU.x < 0 && newPosX_RB.x > Screen.width)
        {
            Background.localPosition += new Vector3(mouseX, 0) * Time.fixedDeltaTime;
            QTE.localPosition = Background.localPosition;
        }
        else
        {
            if ((newPosX_LU.x < 0 && mouseX > 0) || (newPosX_RB.x > Screen.width && mouseX < 0))
            {
                Background.localPosition += new Vector3(mouseX, 0) * Time.fixedDeltaTime;
                QTE.localPosition = Background.localPosition;
            }
        }

        if (newPosX_RB.y < 0 && newPosX_LU.y > Screen.height)
        {
            Background.localPosition += new Vector3(0, mouseY) * Time.fixedDeltaTime;
            QTE.localPosition = Background.localPosition;
        }
        else
        {
            if ((newPosX_RB.y > 0 && mouseY < 0) || (newPosX_LU.y < Screen.height && mouseY > 0))
            {
                Background.localPosition += new Vector3(0, mouseY) * Time.fixedDeltaTime;
                QTE.localPosition = Background.localPosition;
            }
        }
    }
}
