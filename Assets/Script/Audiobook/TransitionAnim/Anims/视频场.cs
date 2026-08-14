using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

/// <summary>
/// 视频场：在"全遮盖点"下动态生成全屏视频遮罩子物体（RawImage + VideoPlayer），
/// 播放 Resources/UI 下的视频，透明度 0 → 1 → 0；透明度到达 1 的那一刻才调用
/// ToRow 切换行，随后视频遮罩淡出并销毁（同时释放 RenderTexture）。
/// 逻辑与黑场/白场（x场式）相同，区别只是遮罩内容为视频。
///
/// 用法（转场特效列）："视频场 >> UI/视频名" 或 "视频场 >> UI/视频名.mp4"，
/// 视频文件放在 Assets/Resources/UI/ 下。
/// </summary>
public static class 视频场
{
    /// <summary>
    /// 执行视频场转场。
    /// </summary>
    /// <param name="audiobook">行数据源（透明度为 1 时调用 ToRow）。</param>
    /// <param name="全遮盖点">遮罩子物体的父节点（RectTransform）。</param>
    /// <param name="target">目标行号。</param>
    /// <param name="视频路径">Resources 下的视频路径（相对 Resources 根，如 "UI/xxx"）。</param>
    /// <param name="time">渐变时长（秒）：0→1 与 1→0 各用这段时间。</param>
    /// <param name="waittime">遮罩完全盖住后停留的时长（秒）。</param>
    /// <param name="onComplete">转场结束回调。</param>
    public static IEnumerator Play(Audiobook audiobook, RectTransform 全遮盖点, int target, string 视频路径, float time, float waittime, Action onComplete = null)
    {
        if (audiobook == null || 全遮盖点 == null)
        {
            audiobook?.ToRow(target);
            onComplete?.Invoke();
            yield break;
        }

        // 加载视频：兼容 "UI/xxx" 与 "UI/xxx.mp4" 两种写法
        string path = 视频路径;
        if (path.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".mov", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".avi", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
            path.EndsWith(".webm", StringComparison.OrdinalIgnoreCase))
        {
            path = path[..path.LastIndexOf('.')];
        }

        VideoClip clip = Resources.Load<VideoClip>(path);
        if (clip == null)
        {
            Debug.LogWarning($"[视频场] 视频资源不存在: {path}（文件应放在 Assets/Resources/UI/ 下）");
            audiobook.ToRow(target);
            onComplete?.Invoke();
            yield break;
        }

        // 生成视频遮罩子物体，父级为全遮盖点
        GameObject 遮罩 = new GameObject("视频场遮罩", typeof(RectTransform), typeof(RawImage), typeof(VideoPlayer));
        遮罩.transform.SetParent(全遮盖点, false);

        RectTransform rect = 遮罩.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 视频渲染到 RenderTexture，由 RawImage 显示
        RenderTexture rt = new RenderTexture((int)clip.width, (int)clip.height, 0);
        VideoPlayer vp = 遮罩.GetComponent<VideoPlayer>();
        vp.clip = clip;
        vp.playOnAwake = true;
        vp.isLooping = true;
        vp.renderMode = VideoRenderMode.RenderTexture;
        vp.targetTexture = rt;

        RawImage image = 遮罩.GetComponent<RawImage>();
        image.texture = rt;
        image.color = new Color(1, 1, 1, 0);
        image.raycastTarget = false;

        vp.Play();

        // 1. 0 → 1 渐盖（视频画面渐显）
        yield return Fade(image, 0f, 1f, time);

        // 2. 透明度为 1 的那一刻才换行
        audiobook.ToRow(target);

        // 3. 全遮盖停留（可调 0）
        yield return new WaitForSeconds(waittime);

        // 4. 1 → 0 渐开，完成后停止视频、销毁遮罩并释放 RenderTexture
        yield return Fade(image, 1f, 0f, time);

        vp.Stop();
        vp.targetTexture = null;
        UnityEngine.Object.Destroy(rt);
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
