using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 白场转场：画面渐变到全白（遮罩变为不透明的白色）。
/// 典型用途：闪白 / 回忆 / 场景切换的高光遮盖。
/// </summary>
public class TransFadeToWhite : TransAnimBase
{
    protected override IEnumerator PlayRoutine(Action onComplete)
    {
        // 强制白色遮罩
        SetMaskColor(Color.white);
        SetAlpha(0f);
        yield return FadeAlpha(0f, 1f, duration);
        Finish(onComplete);
    }
}
