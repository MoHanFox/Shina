using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 转场动画基类。
/// 驱动一张全屏 Graphic（Image / RawImage）的透明度，子类实现具体转场流程：
/// 淡入（FadeIn）、黑场（FadeToBlack）、白场（FadeToWhite）。
///
/// 用法：把基类或其子类挂在全屏遮罩物体上（须有 Graphic），
/// 调用 <see cref="Play"/> 播放，播放结束后触发 onComplete 回调。
/// </summary>
public abstract class TransAnimBase : MonoBehaviour
{
    [Tooltip("转场时长（秒）")]
    public float duration = 0.5f;

    [Tooltip("转场遮罩颜色（黑场/白场时决定最终颜色，淡入时决定遮罩底色）")]
    public Color maskColor = Color.black;

    /// <summary>遮罩的 Graphic 组件（自动获取，须与脚本同物体）。</summary>
    protected Graphic _graphic;

    private bool _playing;

    /// <summary>是否正在播放转场。</summary>
    public bool IsPlaying => _playing;

    protected virtual void Awake()
    {
        _graphic = GetComponent<Graphic>();
        if (_graphic == null)
        {
            Debug.LogWarning($"[{GetType().Name}] 缺少 Graphic 组件（Image/RawImage），转场无效。", this);
        }
    }

    /// <summary>
    /// 开始播放转场。若已在本物体上播放，重复调用会被忽略。
    /// </summary>
    /// <param name="onComplete">转场完成回调（可能为 null）。</param>
    public void Play(Action onComplete = null)
    {
        if (_playing)
        {
            return;
        }

        if (_graphic == null)
        {
            _graphic = GetComponent<Graphic>();
        }

        _playing = true;
        gameObject.SetActive(true);
        StartCoroutine(PlayRoutine(onComplete));
    }

    /// <summary>立即停止转场（不触发完成回调）。</summary>
    public void Stop()
    {
        StopAllCoroutines();
        _playing = false;
    }

    /// <summary>子类实现具体转场流程，结束时必须调用 <see cref="Finish"/>。</summary>
    protected abstract IEnumerator PlayRoutine(Action onComplete);

    /// <summary>设置遮罩的 RGB 颜色（不影响透明度 alpha）。</summary>
    protected void SetMaskColor(Color color)
    {
        if (_graphic == null)
        {
            return;
        }

        Color c = _graphic.color;
        c.r = color.r;
        c.g = color.g;
        c.b = color.b;
        _graphic.color = c;
    }

    /// <summary>设置遮罩透明度（0 全透明，1 不透明）。</summary>
    protected void SetAlpha(float alpha)
    {
        if (_graphic == null)
        {
            return;
        }

        Color c = _graphic.color;
        c.a = alpha;
        _graphic.color = c;
    }

    /// <summary>把透明度从 from 渐变到 to，耗时 time 秒。</summary>
    protected IEnumerator FadeAlpha(float from, float to, float time)
    {
        float elapsed = 0f;
        while (elapsed < time)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / time)));
            yield return null;
        }

        SetAlpha(to);
    }

    /// <summary>结束播放状态并触发完成回调。</summary>
    protected void Finish(Action onComplete)
    {
        _playing = false;
        onComplete?.Invoke();
    }
}
