using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

public partial class Audiobook
{
    private RectTransform canvasRect;
    private Vector2 bgSize = new(1920, 1080);
    private bool 背景对齐需要重算 = true;
    private Vector2 lastCanvasSize;
    private Texture lastVideoTexture;
    private float lastVideoVolume = float.MinValue;

    /// <summary>
    /// 按画布与素材比例缩放背景，使任意分辨率下铺满。
    /// 原实现每帧调用且每帧 GetComponentInParent 找 Canvas；
    /// 现改为缓存 + 脏标记驱动（背景/缩放比例/画布尺寸变化时才会重算）。
    /// </summary>
    public void 背景对齐()
    {
        if (背景.texture.IsUnityNull())
        {
            背景对齐需要重算 = true;   //纹理还没就绪（如视频首帧），下一帧重试
            return;
        }

        try
        {
            Vector2 canvasSize = canvasRect.sizeDelta;
            float screenxyRate = canvasSize.x / canvasSize.y;

            //素材尺寸长宽比（视频/图片）
            float texturexyRate = bgSize.x / bgSize.y;

            if (texturexyRate > screenxyRate)
            {
                int newSizeY = Mathf.CeilToInt(canvasSize.y);
                int newSizeX = Mathf.CeilToInt((float)newSizeY / bgSize.y * bgSize.x);
                背景Rect.sizeDelta = new Vector2(newSizeX * 背景缩放比例, newSizeY * 背景缩放比例);
            }
            else
            {
                int newVideoSizeX = Mathf.CeilToInt(canvasSize.x);
                int newVideoSizeY = Mathf.CeilToInt((float)newVideoSizeX / bgSize.x * bgSize.y);
                背景Rect.sizeDelta = new Vector2(newVideoSizeX * 背景缩放比例, newVideoSizeY * 背景缩放比例);
            }

            lastCanvasSize = canvasSize;
            背景对齐需要重算 = false;
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    /// <summary>
    /// 背景明度渐变（原在 OnGUI 中执行，每帧会跑多遍导致渐变偏快且不稳定；
    /// 移入 Update 后每帧执行一次，乘以 2 保持与原 OnGUI 两次回调时的观感一致）。
    /// </summary>
    void 明度渐变Tick()
    {
        float 当前背景明度 = 背景.color.r;

        if (背景目标明度 < 当前背景明度)
        {
            当前背景明度 -= Time.deltaTime * 2f;

            if (背景目标明度 < 0)
                当前背景明度 = 背景目标明度;

            背景.color = new(当前背景明度, 当前背景明度, 当前背景明度);
        }
        else if (背景目标明度 > 当前背景明度)
        {
            当前背景明度 += Time.deltaTime * 2f;

            if (当前背景明度 > 背景目标明度)
                当前背景明度 = 背景目标明度;

            背景.color = new(当前背景明度, 当前背景明度, 当前背景明度);
        }
    }

    void 视频周期委托(VideoPlayer videoPlayer)
    {
        if (BeforeVideo)
        {
            BeforeVideo = false;

            转场(BeforeVideo_row, CellString(BeforeVideo_row, 转场特效Line));
        }
    }
}
