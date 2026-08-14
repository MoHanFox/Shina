using System.Collections.Generic;
using System.IO;
using UnityEngine;

public partial class Audiobook
{
    /// <summary>
    /// 语音：每次行更新全部停止并回收（等价原 Destroy 全部后 AddComponent，改为组件池复用）。
    /// </summary>
    void 更新语音(StoryRowData d)
    {
        语音池.StopAll();

        if (d.语音 == "" || !startPlaying)
            return;

        foreach (string value in d.语音.Split('|'))
        {
            AudioClip clip = Resources.Load<AudioClip>("Audio/" + value);
            if (clip == null)
            {
                Debug.LogWarning("语音资源不存在: Audio/" + value);
                continue;
            }

            AudioSource source = 语音池.Get();
            source.clip = clip;
            source.Play();
        }
    }

    /// <summary>
    /// 背景音乐：差集切换（交集保留不打断、新增播放、移除停止），
    /// 原实现用 LINQ Intersect/Except 每行分配，改为 HashSet 比较。
    /// </summary>
    void 更新背景音乐(StoryRowData d)
    {
        if (d.背景音乐 == "" || d.背景音乐 == 上个背景音乐集)
            return;

        if (d.背景音乐 == "None")
        {
            背景音乐池.StopAll();
        }
        else
        {
            string[] 使用集合 = d.背景音乐.Split('|');

            //新集合的曲名（去掉路径前缀），用于判断现存曲目是否保留
            HashSet<string> 保留曲名 = new();
            foreach (string item in 使用集合)
                保留曲名.Add(Path.GetFileName(item));

            //停止并回收已移除的曲目
            foreach (AudioSource source in 背景音乐Object.GetComponents<AudioSource>())
            {
                if (source == null || source.clip == null)
                    continue;
                if (!保留曲名.Contains(source.clip.name))
                    背景音乐池.Release(source);
            }

            //添加新增曲目
            foreach (string value in 使用集合)
            {
                AudioClip clip = Resources.Load<AudioClip>("Audio/" + value);
                if (clip == null)
                {
                    Debug.LogWarning("背景音乐资源不存在: Audio/" + value);
                    continue;
                }

                if (背景音乐已在播放(clip))
                    continue;

                AudioSource source = 背景音乐池.Get();
                source.clip = clip;
                source.loop = true;
                source.Play();
            }
        }

        SaveManager.data0.背景音乐 = d.背景音乐;
        上个背景音乐集 = d.背景音乐;
    }

    bool 背景音乐已在播放(AudioClip clip)
    {
        foreach (AudioSource source in 背景音乐Object.GetComponents<AudioSource>())
        {
            if (source != null && source.clip == clip)
                return true;
        }
        return false;
    }

    //============================== 背景音乐控制指令（供指令表调用） ==============================

    void 暂停背景音乐(string[] 参数)
    {
        if (参数.Length == 1)     //暂停所有
        {
            foreach (var audio in 背景音乐Object.GetComponents<AudioSource>())
                audio.Pause();
        }
        else
        {
            foreach (var item in 背景音乐Object.GetComponents<AudioSource>())
            {
                if (item.clip != null && item.clip.name.Equals(参数[1]))
                {
                    item.Pause();
                    break;
                }
            }
        }
    }

    void 继续背景音乐(string[] 参数)
    {
        if (参数.Length == 1)
        {
            foreach (var audio in 背景音乐Object.GetComponents<AudioSource>())
            {
                audio.UnPause();

                if (!audio.isPlaying)      //防止因播放完成而造成无法开始
                    audio.Play();
            }
        }
        else
        {
            foreach (var item in 背景音乐Object.GetComponents<AudioSource>())
            {
                if (item.clip != null && item.clip.name.Equals(参数[1]))
                {
                    item.UnPause();

                    if (!item.isPlaying)      //防止因播放完成而造成无法开始
                        item.Play();

                    break;
                }
            }
        }
    }

    //背景音乐进度 >> 音乐名字 >> xx(float)
    void 背景音乐进度(string[] 参数)
    {
        foreach (var item in 背景音乐Object.GetComponents<AudioSource>())
        {
            if (item.clip != null && item.clip.name.Equals(参数[1]))
            {
                if (!item.isPlaying)    //防止因 [播放完成/未播放] 而造成无法开始
                    item.Play();

                item.time = float.Parse(参数[2]);
                item.UnPause();

                break;
            }
        }
    }

    //背景音乐循环 >> 音乐名字 >> boolean
    void 背景音乐循环(string[] 参数)
    {
        foreach (var item in 背景音乐Object.GetComponents<AudioSource>())
        {
            if (item.clip != null && item.clip.name.Equals(参数[1]))
            {
                item.loop = bool.Parse(参数[2]);
                item.UnPause();             //在loop时，音频会无限循环，无需停止检测

                break;
            }
        }
    }
}
