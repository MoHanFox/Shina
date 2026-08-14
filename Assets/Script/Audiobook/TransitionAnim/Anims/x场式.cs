using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// x场式（黑场 / 白场）：在"全遮盖点"下动态生成全屏遮罩子物体，
/// 透明度 0 → 1 → 0；透明度到达 1 的那一刻才调用 ToRow 切换行，随后遮罩淡出销毁。
/// 黑场用黑色遮罩，白场用白色遮罩。
/// </summary>
public static class x场式
{
    /// <summary>
    /// 执行黑场/白场转场。
    /// </summary>
    /// <param name="audiobook">行数据源（透明度为 1 时调用 ToRow）。</param>
    /// <param name="全遮盖点">遮罩子物体的父节点（RectTransform）。</param>
    /// <param name="target">目标行号。</param>
    /// <param name="白场">true 白场（白色遮罩），false 黑场（黑色遮罩）。</param>
    /// <param name="time">渐变时长（秒）：0→1 与 1→0 各用这段时间。</param>
    /// <param name="waittime">遮罩完全盖住后停留的时长（秒）。</param>
    /// <param name="onComplete">转场结束回调。</param>
    public static IEnumerator Play(Audiobook audiobook, RectTransform 全遮盖点, int target, bool 白场, float time, float waittime, Action onComplete = null)
    {
        if (audiobook == null || 全遮盖点 == null)
        {
            audiobook?.ToRow(target);
            onComplete?.Invoke();
            yield break;
        }

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

        // 1. 0 → 1 渐盖（黑场变黑 / 白场变白）
        yield return Fade(image, 0f, 1f, time);

        // 2. 透明度为 1 的那一刻才换行
        audiobook.ToRow(target);

        // 3. 全遮盖停留（可调 0）
        yield return new WaitForSeconds(waittime);

        // 4. 1 → 0 渐开，完成后销毁遮罩
        yield return Fade(image, 1f, 0f, time);

        UnityEngine.Object.Destroy(遮罩);
        onComplete?.Invoke();
    }

    /// <summary>把 Graphic 透明度从 from 渐变到 to，耗时 dur 秒。</summary>
    private static IEnumerator Fade(Graphic graphic, float from, float to, float dur)
    {
        float elapsed = 0f;
        while (elapsed < dur)
        {
            elapsed += Time.deltaTime;
            Color c = graphic.color;
            c.a = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / dur));
            graphic.color = c;
            yield return null;
        }

        Color final = graphic.color;
        final.a = to;
        graphic.color = final;
    }
}
