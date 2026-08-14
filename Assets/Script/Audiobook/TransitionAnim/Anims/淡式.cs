using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 淡式（淡入）：直接克隆"切换前的旧背景"物体（图片/视频纹理一并复制引用，无需预制件），
/// 叠在原背景位置后立即换行，克隆体由 GraphicFadeOutAndDestroy 自动淡出并销毁，
/// 视觉上旧背景淡出、新背景浮现。
/// </summary>
public static class 淡式
{
    /// <summary>
    /// 执行淡入转场。
    /// </summary>
    /// <param name="audiobook">行数据源（取 背景，并在克隆后调用 ToRow）。</param>
    /// <param name="target">目标行号。</param>
    /// <param name="时长">克隆体淡出销毁时长（秒）。</param>
    /// <param name="onComplete">转场结束回调（克隆体销毁后触发）。</param>
    public static IEnumerator Play(Audiobook audiobook, int target, float 时长, Action onComplete = null)
    {
        if (audiobook == null || audiobook.背景 == null)
        {
            audiobook?.ToRow(target);
            onComplete?.Invoke();
            yield break;
        }

        // 直接克隆原背景物体（父级相同，仅拷贝布局与纹理引用）
        GameObject 实例 = UnityEngine.Object.Instantiate(audiobook.背景.gameObject, audiobook.背景.transform.parent);

        // 与背景完全重叠：拷贝 RectTransform 布局
        RectTransform 背景Rect = audiobook.背景.rectTransform;
        RectTransform 实例Rect = 实例.GetComponent<RectTransform>();
        实例Rect.anchorMin = 背景Rect.anchorMin;
        实例Rect.anchorMax = 背景Rect.anchorMax;
        实例Rect.anchoredPosition = 背景Rect.anchoredPosition;
        实例Rect.sizeDelta = 背景Rect.sizeDelta;
        实例Rect.localScale = 背景Rect.localScale;
        // 排在背景之后，保证渲染在背景上层
        实例.transform.SetSiblingIndex(背景Rect.GetSiblingIndex() + 1);

        // 克隆体不挡点击（淡入期间由 Manager.淡入中 在逻辑层拦截）
        RawImage 实例图 = 实例.GetComponent<RawImage>();
        if (实例图 != null)
        {
            实例图.raycastTarget = false;
        }

        // 挂自动淡出销毁脚本：透明度 1→0 后自动 Destroy(gameObject)
        GraphicFadeOutAndDestroy 动画 = 实例.GetComponent<GraphicFadeOutAndDestroy>();
        if (动画 == null)
        {
            动画 = 实例.AddComponent<GraphicFadeOutAndDestroy>();
        }
        动画.waittime = 0f;
        动画.time = 时长;

        // 切换行内容（背景等在此更新），随后旧背景克隆体淡出销毁
        audiobook.ToRow(target);

        // 等待克隆体自动销毁后结束转场
        while (实例 != null)
        {
            yield return null;
        }

        onComplete?.Invoke();
    }
}
