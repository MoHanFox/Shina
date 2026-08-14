using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 转场管理器（挂在 Audiobook 同一物体上）。
///
/// 职责：按行数据"转场特效"列内容分派转场（参考 QTEManager.post 风格）：
/// - 白场 / 黑场：在"全遮盖点"下动态生成全屏遮罩子物体，
///   透明度 0 → 1 → 0；透明度到达 1 的那一刻才调用 ToRow 切换行，随后遮罩淡出销毁。
/// - 淡入：在背景图片上生成"切换前的旧背景"快照覆盖层（带 GraphicFadeOutAndDestroy，
///   透明度渐降后自动销毁），然后立即 ToRow，视觉呈现旧背景淡出、新背景浮现。
/// - 空：直接 ToRow。
/// </summary>
public class TransitionAnimManager : MonoBehaviour
{
    [Header("黑场/白场遮罩参数")]
    [Tooltip("遮罩完全盖住后停留的时长（秒），透明度 0→1→0 中的全黑停留段")]
    public float waittime = 0.15f;

    [Tooltip("渐变时长（秒）：0→1 与 1→0 各用这段时间")]
    public float time = 0.5f;

    [Tooltip("行数据源：透明度为 1 的时刻调用 audiobook.ToRow(Audiobook.row)")]
    public Audiobook audiobook;

    [Header("遮罩父节点（黑场/白场时动态生成遮罩子物体挂在这里）")]
    [Tooltip("场景中的全遮盖点（RectTransform）")]
    public RectTransform 全遮盖点;

    [Header("淡入快照")]
    [Tooltip("淡入覆盖层预制体（须带 GraphicFadeOutAndDestroy 脚本，淡出后自动销毁）")]
    public GameObject 淡入转场覆盖_Obj;

    [Tooltip("淡入覆盖层淡出销毁的时长（秒）")]
    public float 淡入时长 = 0.25f;

    /// <summary>是否有黑场/白场遮罩正在播放（兼容旧代码 TransitionAnim.doAnim 的读取）。</summary>
    private static bool _anyPlaying;

    /// <summary>淡入快照覆盖层是否仍存活（淡入动画期间为 true，覆盖层自动销毁后为 false）。</summary>
    public static bool 淡入中;

    /// <summary>是否有转场动画正在进行（兼容旧代码 TransitionAnim.doAnim 的读取）。</summary>
    public static bool doAnim => _anyPlaying;

    private void Awake()
    {
        // 与 Audiobook 同物体挂载时自动取引用，省去 Inspector 手动拖拽
        if (audiobook == null)
        {
            audiobook = GetComponent<Audiobook>();
        }

        // 全遮盖点未拖时，按名字查找场景物体（一次性，仅初始化）
        if (全遮盖点 == null)
        {
            GameObject found = GameObject.Find("全遮盖点");
            if (found != null)
            {
                全遮盖点 = found.GetComponent<RectTransform>();
            }
        }
    }

    //============================== 转场分派（参考 QTEManager.post） ==============================

    /// <summary>
    /// 转场特效分派入口。valueO 为行数据"转场特效"列内容：白场 / 黑场 / 淡入 / 空。
    /// </summary>
    /// <param name="target">目标行号（会写入 Audiobook.row）。</param>
    /// <param name="valueO">转场特效名，空串或 null 时直接切行。</param>
    public void post(int target, string valueO)
    {
        Audiobook.row = target;

        if (string.IsNullOrEmpty(valueO))
        {
            audiobook?.ToRow(target);
            return;
        }

        if (valueO == "白场" || valueO == "黑场")
        {
            黑场白场(target, valueO == "白场");
            return;
        }

        if (valueO == "淡入")
        {
            淡入转场(target);
            return;
        }

        audiobook?.ToRow(target);
    }

    //============================== 黑场 / 白场 ==============================

    /// <summary>
    /// 黑场/白场：在"全遮盖点"下动态生成全屏遮罩子物体，
    /// 透明度 0→1→0，透明度到达 1 的那一刻才 ToRow 换行，最后销毁遮罩。
    /// </summary>
    private void 黑场白场(int target, bool 白场)
    {
        if (audiobook == null || 全遮盖点 == null)
        {
            audiobook?.ToRow(target);
            return;
        }

        _anyPlaying = true;

        // 生成遮罩子物体，父级为全遮盖点
        GameObject 遮罩 = new GameObject("转场遮罩", typeof(RectTransform), typeof(Image));
        遮罩.transform.SetParent(全遮盖点, false);

        RectTransform rect = 遮罩.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        Image image = 遮罩.GetComponent<Image>();
        image.color = 白场 ? new Color(1, 1, 1, 0) : new Color(0, 0, 0, 0);
        image.raycastTarget = false;

        StartCoroutine(遮罩动画(遮罩, image, target));
    }

    private IEnumerator 遮罩动画(GameObject 遮罩, Image image, int target)
    {
        // 1. 0 → 1 渐盖（黑场变黑 / 白场变白）
        yield return Fade(image, 0f, 1f, time);

        // 2. 透明度为 1 的那一刻才换行
        audiobook?.ToRow(target);

        // 3. 全遮盖停留（按旧逻辑保留 waittime，可调 0）
        yield return new WaitForSeconds(waittime);

        // 4. 1 → 0 渐开，完成后销毁遮罩
        yield return Fade(image, 1f, 0f, time);

        Destroy(遮罩);
        _anyPlaying = false;
    }

    /// <summary>把 Image 透明度从 from 渐变到 to，耗时 dur 秒。</summary>
    private static IEnumerator Fade(Image image, float from, float to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            Color c = image.color;
            c.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / dur));
            image.color = c;
            yield return null;
        }

        Color final = image.color;
        final.a = to;
        image.color = final;
    }

    //============================== 淡入 ==============================

    /// <summary>
    /// 淡入转场：背景已切换到新图，在原有背景上叠一张"切换前的旧背景"快照覆盖层，
    /// 透明度从当前背景明度渐降到 0 后自动销毁（GraphicFadeOutAndDestroy），
    /// 视觉上呈现旧背景淡出、新背景浮现。
    /// </summary>
    private void 淡入转场(int target)
    {
        if (audiobook == null)
        {
            return;
        }

        if (淡入转场覆盖_Obj == null || audiobook.背景 == null)
        {
            audiobook.ToRow(target);
            return;
        }

        GameObject 实例 = Instantiate(淡入转场覆盖_Obj);
        RectTransform 实例Rect = 实例.GetComponent<RectTransform>();
        RawImage 实例图 = 实例.GetComponent<RawImage>();
        GraphicFadeOutAndDestroy 实例动画 = 实例.GetComponent<GraphicFadeOutAndDestroy>();

        // 快照覆盖层：父级、尺寸、纹理对齐"切换前的旧背景"
        实例Rect.SetParent(audiobook.背景.rectTransform);
        实例动画.time = 淡入时长;
        实例Rect.localScale = Vector3.one;
        实例Rect.localPosition = Vector3.zero;
        实例Rect.sizeDelta = audiobook.背景.rectTransform.sizeDelta;
        实例图.texture = audiobook.背景.texture;
        实例图.color = new Color(1f, 1f, 1f, SaveManager.data0.背景明度);
        
        audiobook.ToRow(target);  //淡入不需要等待
    }
}
