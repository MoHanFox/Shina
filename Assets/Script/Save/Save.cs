using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Save
{
    //BASE
    public string chapter = "story.前言";        //章节
    public int progress = 1;   //进度
    public string 人物 = "";
    public string 对话 = "";
    public string 背景 = "";
    public string 背景音乐 = "";
    public string 立绘 = "";
    public string 头像 = "";

    //界面
    public float 背景缩放比例 = 1.05f;
    public float 背景明度 = 1f;
    public bool 头像统一 = true;

    //特殊点
    public List<string> 事件标签 = new() { };          //数据记录
}
