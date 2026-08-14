using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 黑场转场：画面渐变到全黑（遮罩变为不透明的黑色）。
/// 典型用途：切行 / 切场景前遮盖画面。
/// </summary>
public class TransFadeToBlack : TransAnimBase
{
    protected override IEnumerator PlayRoutine(Action onComplete)
    {
        // 强制黑色遮罩
        SetMaskColor(Color.black);
        SetAlpha(0f);
        yield return FadeAlpha(0f, 1f, duration);
        Finish(onComplete);
    }
}
