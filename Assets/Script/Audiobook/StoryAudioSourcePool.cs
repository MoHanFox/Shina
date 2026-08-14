using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// AudioSource 组件池：避免剧情切换时反复 AddComponent/Destroy 造成组件抖动与卡顿。
/// 组件始终挂在根物体上；空闲时停止并清空 clip，需要时取出复用。
/// 行为等价原实现的"每次重建"，但不再产生组件增删。
/// </summary>
public sealed class StoryAudioSourcePool
{
    private readonly GameObject root;
    private readonly List<AudioSource> all = new();
    private readonly List<AudioSource> free = new();

    public StoryAudioSourcePool(GameObject root)
    {
        this.root = root;
    }

    /// <summary>取一个空闲组件，没有则新建</summary>
    public AudioSource Get()
    {
        if (free.Count > 0)
        {
            AudioSource source = free[free.Count - 1];
            free.RemoveAt(free.Count - 1);
            return source;
        }

        AudioSource created = root.AddComponent<AudioSource>();
        all.Add(created);
        return created;
    }

    /// <summary>停止并回收组件（等待复用）</summary>
    public void Release(AudioSource source)
    {
        if (source == null)
            return;

        source.Stop();
        source.clip = null;

        if (!free.Contains(source))
            free.Add(source);
    }

    /// <summary>停止并回收全部组件（等价原 Destroy 全部）</summary>
    public void StopAll()
    {
        for (int i = 0; i < all.Count; i++)
            Release(all[i]);
    }
}
