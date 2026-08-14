using UnityEngine;

/// <summary>
/// 章节选择界面缩放逻辑（桌面滚轮 / 移动端双指）。
/// 优化：平滑改为与帧率无关的指数平滑；移除 UnityEditor 引用与无用 using（真机构建会编译失败）。
/// </summary>
public class MouseForScale : MonoBehaviour
{
    [SerializeField] private RectTransform carrier;       // 被缩放的UI元素
    [SerializeField] private RectTransform canvasRect;    // Canvas的RectTransform
    [SerializeField] private float minScale = 0.3f;       // 最小缩放比例
    [SerializeField] private float maxScale = 2f;         // 最大缩放比例
    [SerializeField] private float zoomSpeed = 10f;       // 缩放平滑速度

    private Touch oldTouch1, oldTouch2;                   // 用于存储旧的触摸点

    void Update()
    {
        HandleZoomInput();
    }

    private void HandleZoomInput()
    {
        // 桌面平台鼠标滚轮缩放
        if (!Application.isMobilePlatform)
        {
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scrollInput) > 0.001f)
            {
                HandleMouseZoom(scrollInput);
            }
        }
        // 移动平台双指缩放
        else if (Input.touchCount >= 2)
        {
            HandleTouchZoom();
        }
    }

    private void HandleMouseZoom(float scrollInput)
    {
        // 获取鼠标在屏幕上的位置（转换为UI坐标）
        Vector2 mousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            Input.mousePosition,
            null,
            out mousePosition
        );

        // 计算目标缩放比例
        float targetScale = carrier.localScale.x + scrollInput * 2.5f;
        targetScale = Mathf.Clamp(targetScale, minScale, maxScale);

        // 当缩放比例接近1时，确保鼠标位置最终在屏幕中心
        if (Mathf.Abs(targetScale - 1.0f) < 0.01f)
        {
            CenterOnMousePosition();
            carrier.localScale = Vector3.one;
            return;
        }

        // 计算鼠标在UI元素局部坐标系中的位置
        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            carrier,
            Input.mousePosition,
            null,
            out localMousePos
        );

        // 记录当前缩放比例
        float currentScale = carrier.localScale.x;

        // 平滑过渡到目标缩放（指数平滑，任意帧率手感一致）
        float 平滑系数 = 1f - Mathf.Exp(-zoomSpeed * Time.deltaTime);
        Vector3 newScale = Vector3.Lerp(
            carrier.localScale,
            Vector3.one * targetScale,
            平滑系数
        );
        carrier.localScale = newScale;

        // 计算位置调整量，使鼠标指向的点保持在同一屏幕位置
        AdjustPositionForZoom(currentScale, newScale.x, localMousePos);
    }

    private void HandleTouchZoom()
    {
        Touch touch1 = Input.GetTouch(0);
        Touch touch2 = Input.GetTouch(1);

        // 记录初始触摸位置
        if (touch2.phase == TouchPhase.Began)
        {
            oldTouch1 = touch1;
            oldTouch2 = touch2;
            return;
        }

        // 计算触摸点的中点（缩放中心）
        Vector2 touchMidpoint = (touch1.position + touch2.position) / 2f;

        // 转换为UI坐标
        Vector2 midpointUI;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            touchMidpoint,
            null,
            out midpointUI
        );

        // 计算新旧距离差
        float oldDistance = Vector2.Distance(oldTouch1.position, oldTouch2.position);
        float newDistance = Vector2.Distance(touch1.position, touch2.position);
        float distanceDelta = newDistance - oldDistance;

        // 计算目标缩放比例
        float scaleFactor = distanceDelta * 0.0045f;
        float targetScale = carrier.localScale.x + scaleFactor;
        targetScale = Mathf.Clamp(targetScale, minScale, maxScale);

        // 当缩放比例接近1时，确保触摸中点在屏幕中心
        if (Mathf.Abs(targetScale - 1.0f) < 0.01f)
        {
            CenterOnScreenPoint(touchMidpoint);
            carrier.localScale = Vector3.one;
            oldTouch1 = touch1;
            oldTouch2 = touch2;
            return;
        }

        // 计算触摸中点在UI元素局部坐标系中的位置
        Vector2 localMidpointPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            carrier,
            touchMidpoint,
            null,
            out localMidpointPos
        );

        // 记录当前缩放比例
        float currentScale = carrier.localScale.x;

        // 平滑过渡到目标缩放（指数平滑，任意帧率手感一致）
        float 平滑系数 = 1f - Mathf.Exp(-zoomSpeed * Time.deltaTime);
        Vector3 newScale = Vector3.Lerp(
            carrier.localScale,
            Vector3.one * targetScale,
            平滑系数
        );
        carrier.localScale = newScale;

        // 调整位置使触摸中点保持在同一屏幕位置
        AdjustPositionForZoom(currentScale, newScale.x, localMidpointPos);

        // 更新旧触摸点
        oldTouch1 = touch1;
        oldTouch2 = touch2;
    }

    // 调整位置以保持缩放中心不变
    private void AdjustPositionForZoom(float oldScale, float newScale, Vector2 localZoomCenter)
    {
        // 计算缩放比例变化
        float scaleRatio = newScale / oldScale;

        // 计算位置调整量
        Vector2 positionAdjustment = localZoomCenter * (1 - scaleRatio);

        // 应用位置调整（考虑UI的pivot点）
        carrier.localPosition += new Vector3(
            positionAdjustment.x * oldScale,
            positionAdjustment.y * oldScale,
            0
        );
    }

    // 使鼠标位置在缩放为1时居中
    private void CenterOnMousePosition()
    {
        CenterOnScreenPoint(Input.mousePosition);
    }

    // 使指定屏幕点在缩放为1时居中
    private void CenterOnScreenPoint(Vector2 screenPosition)
    {
        // 计算屏幕中心在Canvas中的位置
        Vector2 screenCenter;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            new Vector2(Screen.width / 2f, Screen.height / 2f),
            null,
            out screenCenter
        );

        // 计算目标点在Canvas中的位置
        Vector2 targetPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPosition,
            null,
            out targetPoint
        );

        // 计算需要移动的偏移量
        Vector2 offset = screenCenter - targetPoint;

        // 应用位置调整
        carrier.localPosition = new Vector3(offset.x, offset.y, 0);
    }
}
