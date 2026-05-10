using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config
{
    //基础
    public int 分辨率 = 2;    //720p
    public float 主菜单音乐响度 = 0.125f;
    public float 背景音乐响度 = 0.15f;
    public float 语音响度 = 0.5f;
    public float 按钮响度 = 1f;

    //选中存档
    public int ChosenSave = 1;    //默认1号存档
    public float 每秒打字间隔 = 0.025f;    //打字时间   0-1
    public float 自动顿停 = 0.2f;    //自动阅读       0-10        默认0.2*10 2s

    //语言
    public int 语言 = 0;   // 0 cn
}
