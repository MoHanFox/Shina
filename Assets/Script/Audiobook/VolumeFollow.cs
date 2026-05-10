using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeFollow : MonoBehaviour
{
    public 更改项 Value;
    public enum 更改项
    {
        主菜单音乐响度,
        背景音乐响度,
        语音响度,
        按钮响度
    }
    void Update()
    {
        foreach(var audioSource in gameObject.GetComponents<AudioSource>()){
            switch (Value)
            {
                case 更改项.主菜单音乐响度:
                    audioSource.volume = ConfigManager.config.主菜单音乐响度;
                    break;
                case 更改项.背景音乐响度:
                    audioSource.volume = ConfigManager.config.背景音乐响度;
                    break;
                case 更改项.语音响度:
                    audioSource.volume = ConfigManager.config.语音响度;
                    break;
                case 更改项.按钮响度:
                    audioSource.volume = ConfigManager.config.按钮响度;
                    break;
            }
        }
    }
}
