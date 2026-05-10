using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class FloatingCursor : MonoBehaviour
{
    [Header("悬浮设置")]
    public float floatSpeed = 1.5f;
    public float floatHeight = 0.5f;
    public bool randomizeStart = true;
    public Audiobook audiobook;
    private Vector3 localStartPos;   // 改为局部坐标
    private float randomOffset;
    private float fixedOffset;

    void Start()
    {
        CacheLocalStartPos();
        randomOffset = randomizeStart ? Random.Range(0f, 2f * Mathf.PI) : 0f;
    }

    void Update()
    {
        if (TextWriter.isActive && !audiobook.不可点击切换())
            return;

        // 用局部坐标做偏移，再转回世界坐标或直接设 anchoredPosition3D
        fixedOffset = Mathf.Sin((Time.time * floatSpeed) + randomOffset) * floatHeight;
        float newY = localStartPos.y + fixedOffset;

        gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(localStartPos.x, newY, localStartPos.z);
    }

    // 当 RectTransform 尺寸变化（分辨率改变、锚点移动等）时自动调用
    void OnRectTransformDimensionsChange()
    {
        CacheLocalStartPos();
    }

    private void CacheLocalStartPos()
    {
        localStartPos = gameObject.GetComponent<RectTransform>().anchoredPosition3D;
        localStartPos = new(localStartPos.x,localStartPos.y - fixedOffset);
    }
}