using UnityEngine;

/// <summary>
/// 章节选择界面拖动（平移）逻辑。
/// 优化：
///  1. OnGUI → Update：原实现在 OnGUI 中每帧执行多次，位移反复叠加、速度不稳；现每帧只执行一次；
///  2. 帧率无关：GetAxis / deltaPosition 本身就是"本帧位移"，直接应用即可（原实现再乘
///     Time.deltaTime，导致每秒总位移随帧率变化）；灵敏度按原 800/50 在 60fps 下的每帧值折算，
///     可在 Inspector 调整；
///  3. 去掉对"移动中目标"做 Lerp 的滞后，拖动 1:1 跟手；
///  4. 双指缩放时（MouseForScale 处理）不再参与拖动，避免拖/缩互相干扰；
///  5. 光标锁定只在状态变化时设置；移除 UnityEditor 引用（真机构建会编译失败）。
/// </summary>
public class MouseForMove : MonoBehaviour
{
    [Header("拖动灵敏度（原 800/50 是按 60fps 帧配合 Time.deltaTime 调校，这里已折算为每帧直接应用，值越大拖得越快）")]
    [SerializeField] private float 鼠标灵敏度 = 800f / 60f;
    [SerializeField] private float 触摸灵敏度 = 50f / 60f;

    private RectTransform 承载;

    void Start()
    {
        承载 = GetComponent<RectTransform>();
    }

    void Update()
    {
        //移动端：单指拖动；双指/多指交给 MouseForScale 处理缩放，避免互相干扰
        if (Application.isMobilePlatform)
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                承载.localPosition += new Vector3(touch.deltaPosition.x, touch.deltaPosition.y) * 触摸灵敏度;
            }
            return;
        }

        //桌面：按住右键拖动（左键用于选择章节）
        bool 拖动中 = Input.GetMouseButton(1);
        设置光标锁定(拖动中);
        if (!拖动中)
            return;

        Vector2 axis = new(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        承载.localPosition += (Vector3)axis * 鼠标灵敏度;
    }

    private static void 设置光标锁定(bool 锁定)
    {
        if (锁定 && Cursor.lockState != CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.Locked;
        else if (!锁定 && Cursor.lockState == CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.None;
    }
}
