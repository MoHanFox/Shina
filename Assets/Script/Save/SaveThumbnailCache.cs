using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 存档缩略图缓存：每个档位只从磁盘解码一次并常驻内存，之后打开存档/读档界面直接复用。
/// 原实现每次打开界面都用 UnityWebRequest 重新下载 PNG 并新建 Texture2D：
///   1. 旧纹理从不销毁 → 内存随每次打开持续上升（泄漏）；
///   2. 每个协程结尾都调用 Resources.UnloadUnusedAssets() → 打开界面时多次全量资源扫描导致卡顿。
/// 档位被覆盖后调用 <see cref="Invalidate"/>，下次打开会重新解码新截图。
/// </summary>
public static class SaveThumbnailCache
{
    private static readonly Dictionary<int, Texture2D> cache = new();

    /// <summary>取档位缩略图；文件不存在或解码失败返回 null；已缓存则直接复用</summary>
    public static Texture2D Get(int 档位)
    {
        if (cache.TryGetValue(档位, out Texture2D cached) && cached != null)
            return cached;

        string path = Application.persistentDataPath + "/Saves/Data" + 档位 + ".png";
        if (!File.Exists(path))
            return null;

        Texture2D tex = new(2, 2)
        {
            name = "SaveThumbnail_" + 档位,
        };

        if (!tex.LoadImage(File.ReadAllBytes(path)))
        {
            Object.Destroy(tex);
            return null;
        }

        cache[档位] = tex;
        return tex;
    }

    /// <summary>档位被覆盖时调用：销毁旧图并移除缓存，下次打开重新解码</summary>
    public static void Invalidate(int 档位)
    {
        if (cache.TryGetValue(档位, out Texture2D tex) && tex != null)
            Object.Destroy(tex);

        cache.Remove(档位);
    }
}
