using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 转场管理器（挂在 Audiobook 同一物体上）。
///
/// 职责：按行数据"转场特效"列内容分派转场（参考 QTEManager.post 风格），
/// 具体转场逻辑在 Anims 文件夹下的独立类中实现：
/// - 白场 / 黑场 → <see cref="x场式"/>
/// - 淡入 → <see cref="淡式"/>
/// - 视频场 → <see cref="视频场"/>（"视频场 >> UI/视频名"，视频放 Assets/Resources/UI/ 下）
/// - 空 → 直接 ToRow
/// </summary>
public class TransitionAnimManager : MonoBehaviour
{
    [Header("黑场/白场/视频场遮罩参数")]
    [Tooltip("遮罩完全盖住后停留的时长（秒），透明度 0→1→0 中的全遮盖停留段")]
    public float waittime = 0.15f;

    [Tooltip("渐变时长（秒）：0→1 与 1→0 各用这段时间")]
    public float time = 0.5f;

    [Tooltip("行数据源：透明度为 1 的时刻调用 audiobook.ToRow(Audiobook.row)")]
    public Audiobook audiobook;

    [Header("遮罩父节点（黑场/白场/视频场时动态生成遮罩子物体挂在这里）")]
    [Tooltip("场景中的全遮盖点（RectTransform）")]
    public RectTransform 全遮盖点;

    [Header("淡入")]
    [Tooltip("淡入克隆体淡出销毁的时长（秒）")]
    public float 淡入时长 = 0.25f;

    /// <summary>是否有黑场/白场/视频场遮罩正在播放（兼容旧代码 TransitionAnim.doAnim 的读取）。</summary>
    private static bool _anyPlaying;

    /// <summary>淡入克隆体是否仍存活（淡入动画期间为 true，克隆体自动销毁后为 false）。</summary>
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
    /// 转场特效分派入口。valueO 为行数据"转场特效"列内容：
    /// 白场 / 黑场 / 淡入 / 视频场 >> 视频路径 / 空。
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
            _anyPlaying = true;
            StartCoroutine(x场式.Play(audiobook, 全遮盖点, target, valueO == "白场", time, waittime, () => _anyPlaying = false));
            return;
        }

        if (valueO == "淡入")
        {
            淡入中 = true;
            StartCoroutine(淡式.Play(audiobook, target, 淡入时长, () => 淡入中 = false));
            return;
        }

        if (valueO.StartsWith("视频场", StringComparison.Ordinal))
        {
            播放视频场(target, valueO);
            return;
        }

        audiobook?.ToRow(target);
    }

    /// <summary>解析 "视频场 >> UI/xxx" 并播放视频场转场。</summary>
    private void 播放视频场(int target, string valueO)
    {
        // 取 ">>" 后的路径，如 "视频场 >> UI/xxx" → "UI/xxx"
        string 视频路径 = "";
        int 分隔符 = valueO.IndexOf(">>", StringComparison.Ordinal);
        if (分隔符 >= 0)
        {
            视频路径 = valueO[(分隔符 + 2)..].Trim();
        }

        if (string.IsNullOrEmpty(视频路径))
        {
            Debug.LogWarning("[TransitionAnimManager] 视频场缺少视频路径，格式应为：视频场 >> UI/视频名");
            audiobook?.ToRow(target);
            return;
        }

        _anyPlaying = true;
        StartCoroutine(视频场.Play(audiobook, 全遮盖点, target, 视频路径, time, waittime, () => _anyPlaying = false));
    }
}
