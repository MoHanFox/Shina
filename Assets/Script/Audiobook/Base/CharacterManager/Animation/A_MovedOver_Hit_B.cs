using System;
using UnityEngine;

public class A_MovedOver_Hit_B : CharacterAnimation
{
    [Header("立绘引用")]
    public RectTransform A; // A立绘
    public RectTransform B; // B立绘

    [Header("移动参数")]
    public float offsetX = 200f;          // A最终停在B左侧的距离
    public float moveDuration = 0.8f;     // A平移耗时
    public float returnDuration = 0.5f; // A滑回去需要的时间

    [Header("跳跃参数")]
    public float jumpHeight = 40f;        // B跳起的高度
    public float jumpDuration = 0.6f;     // B整个跳跃周期（起跳→落地）
    public float waitBeforeJump = 0.15f;  // A停稳后，B“愣一下”再跳的停顿

    // ---------- 状态机变量 ----------
    private enum State
    {
        Idle,       // 待机/未开始
        MovingA,    // A正在平移动画
        Waiting,    // 平移结束，短暂停顿
        JumpingB,   // B正在跳跃
        ReturningA,
        Done        // 动画结束
    }
    private State currentState = State.Idle;

    // 动画运行时数据
    private float timer = 0f;
    private Vector2 aStartPos;
    private Vector2 aEndPos;
    private Vector2 bOriginPos;      // B的原始位置
    private Vector3 bOriginScale;    // B的原始缩放（用于跳跃挤压拉伸后复位）

    // ---------- 对外调用接口 ----------
    public void PlaySequence()
    {
        // 重置状态
        currentState = State.MovingA;
        timer = 0f;

        // 记录初始数据
        aStartPos = A.anchoredPosition;
        aEndPos = B.anchoredPosition + new Vector2(-offsetX, 0);
        bOriginPos = B.anchoredPosition;
        bOriginScale = B.localScale;

        // 确保B复位（防止上次动画残留）
        B.anchoredPosition = bOriginPos;
        B.localScale = bOriginScale;
    }

    // ---------- Update 逐帧驱动 ----------
    private void Update()
    {
        switch (currentState)
        {
            case State.MovingA:    UpdateMovingA();    break;
            case State.Waiting:    UpdateWaiting();    break;
            case State.JumpingB:   UpdateJumpingB();   break;
            case State.ReturningA: UpdateReturningA(); break;
            case State.Idle:
            case State.Done:       break;
        }
    }

    // 1. A 平移到 B 旁边
    private void UpdateMovingA()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / moveDuration);
        
        // 平滑缓动（SmoothStep，先快后慢）
        float smoothT = t * t * (3f - 2f * t);
        A.anchoredPosition = Vector2.Lerp(aStartPos, aEndPos, smoothT);

        if (t >= 1f)
        {
            // 平移完成，进入停顿
            currentState = State.Waiting;
            timer = 0f;
        }
    }

    // 2. 短暂的停顿（让视觉有“呼吸感”）
    private void UpdateWaiting()
    {
        timer += Time.deltaTime;
        if (timer >= waitBeforeJump)
        {
            // 停顿结束，B开始跳
            currentState = State.JumpingB;
            timer = 0f;
        }
    }
    
    private void UpdateJumpingB()
    {
        timer += Time.deltaTime;
        float progress = Mathf.Clamp01(timer / jumpDuration);

        // ---------- 位移：正弦波弧线 ----------
        // progress=0时y=0，progress=0.5时最高，progress=1时回到0
        float yOffset = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
        B.anchoredPosition = bOriginPos + new Vector2(0, yOffset);

        // ---------- 挤压拉伸（核心生动感） ----------
        // effect: 上升阶段为正（拉伸变瘦高），下降阶段为负（挤压变矮胖）
        // progress=0.25时 effect=1（拉伸最大），progress=0.75时 effect=-1（挤压最大）
        float effect = Mathf.Sin(progress * Mathf.PI * 2f); 

        // 横向和纵向反相变化，保持视觉体积感
        float scaleY = 1f + 0.12f * effect;   // 纵向变化幅度12%
        float scaleX = 1f - 0.08f * effect;   // 横向变化幅度8%（稍小一点更自然）

        // 应用缩放（保留Z轴不变）
        B.localScale = new Vector3(
            bOriginScale.x * scaleX,
            bOriginScale.y * scaleY,
            bOriginScale.z
        );

        if (progress >= 1f)
        {
            // B复位
            B.anchoredPosition = bOriginPos;
            B.localScale = bOriginScale;

            // 进入 ReturningA
            currentState = State.ReturningA;
            timer = 0f;
        }
    }

// 新增：A 从 B 旁边平滑返回原位
    private void UpdateReturningA()
    {
        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / returnDuration);
    
        // 用同样的 SmoothStep 缓动，也可以换成 t*t 等
        float smoothT = t * t * (3f - 2f * t);
        A.anchoredPosition = Vector2.Lerp(aEndPos, aStartPos, smoothT);

        if (t >= 1f)
        {
            // 精确归位
            A.anchoredPosition = aStartPos;
            currentState = State.Done;
        }
    }

    // （可选）如果你希望动画播放完毕后，点击继续执行下一步，可以加个检测
    public bool IsPlaying => currentState != State.Idle && currentState != State.Done;
}
