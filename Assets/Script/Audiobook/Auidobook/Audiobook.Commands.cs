using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class Audiobook
{
    private readonly Dictionary<string, Action<string[]>> 指令表 = new();

    /// <summary>本段指令中是否包含 autoChoose（用于修复原实现的顺序依赖重置 bug）</summary>
    private bool hasAutoChooseCommand;

    private void 注册指令()
    {
        指令表.Add("关闭游戏", 指令_关闭游戏);
        指令表.Add("重置立绘", 指令_重置立绘);
        指令表.Add("输入", 指令_输入);
        指令表.Add("背景明度", 指令_背景明度);
        指令表.Add("锁定背景", 指令_锁定背景);
        指令表.Add("BeforeVideo", 指令_BeforeVideo);
        指令表.Add("OverTheChapter", 指令_OverTheChapter);
        指令表.Add("goto", 指令_goto);
        指令表.Add("gotoN", 指令_gotoN);
        指令表.Add("autoChoose", 指令_autoChoose);
        指令表.Add("wait", 指令_wait);
        指令表.Add("自动延时", 指令_自动延时);
        指令表.Add("goChapter", 指令_goChapter);
        指令表.Add("信息框", 指令_信息框);
        指令表.Add("值信息框", 指令_值信息框);
        指令表.Add("noti", 指令_noti);
        指令表.Add("save", 指令_save);
        指令表.Add("saveN", 指令_saveN);
        指令表.Add("random", 指令_random);
        指令表.Add("界面抖动", 指令_界面抖动);
        指令表.Add("addTag", 指令_addTag);
        指令表.Add("removeTag", 指令_removeTag);
        指令表.Add("removeTags", 指令_removeTags);
        指令表.Add("addGlobalTag", 指令_addGlobalTag);
        指令表.Add("addValueTag", 指令_addValueTag);
        指令表.Add("removeValueTag", 指令_removeValueTag);
        指令表.Add("changeValueTag", 指令_changeValueTag);
        指令表.Add("解锁档案", 指令_解锁档案);
        指令表.Add("解锁档案_提示", 指令_解锁档案_提示);
        指令表.Add("暂停背景音乐", 暂停背景音乐);
        指令表.Add("继续背景音乐", 继续背景音乐);
        指令表.Add("头像统一", 指令_头像统一);
        指令表.Add("背景缩放比例", 指令_背景缩放比例);
        指令表.Add("LoadScene", 指令_LoadScene);
        指令表.Add("背景音乐进度", 背景音乐进度);
        指令表.Add("背景音乐循环", 背景音乐循环);
        指令表.Add("播放立绘动画", 指令_播放立绘动画);
    }

    /// <summary>执行一行剧情中的全部指令（| 分隔）</summary>
    public void SpecialCommand(string OValue)
    {
        //整段指令的默认重置（原实现逻辑保留）
        if (!OValue.Contains("goto"))
            gotoRow = -1;
        if (!OValue.Contains("wait"))
        {
            clickCheckTimer = 0f;
            clickWaitTime = -1f;
        }
        if (!OValue.Contains("自动延时"))
        {
            autoCheckTimer = 0f;
            autoWaitTime = -1f;
        }
        if (!OValue.Contains("锁定背景"))
            锁定背景 = false;

        hasAutoChooseCommand = false;

        foreach (string value in OValue.Split('|'))
        {
            string[] 参数 = value.Split(" >> ");
            if (指令表.TryGetValue(参数[0], out Action<string[]> handler))
                handler(参数);
        }

        //修复：原实现在"非 autoChoose 指令"的 else 分支里重置 AutoChooseTime，
        //导致最终状态取决于指令顺序（如 goto|autoChoose 与 autoChoose|goto 结果不同）；
        //改为整段指令处理完后统一判定。
        if (!hasAutoChooseCommand)
            ChoiseManager.AutoChooseTime = -1f;
    }

    //============================== 指令实现 ==============================

    void 指令_关闭游戏(string[] 参数)
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void 指令_重置立绘(string[] 参数)      //重新生成立绘，用于结束动画后手动结束
    {
        if (立绘动画管理器.hasAnimation())
            立绘动画管理器.disEmploy();
    }

    //输入 >> value >> 标题 >> goto >> back
    void 指令_输入(string[] 参数)
    {
        GameObject input = Instantiate(输入);
        InputUI inputUI = input.GetComponent<InputUI>();
        inputUI.value = 参数[1];
        inputUI.title.text = 参数[2];
        inputUI.gotoRow = 搜寻Tag行(参数[3]);
        inputUI.backRow = 搜寻Tag行(参数[4]);
        input.transform.SetParent(输入框落点.transform);

        RectTransform rt = input.GetComponent<RectTransform>();
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    void 指令_背景明度(string[] 参数)
    {
        背景目标明度 = float.Parse(参数[1]);
        SaveManager.data0.背景明度 = 背景目标明度;
    }

    void 指令_锁定背景(string[] 参数)
    {
        锁定背景 = bool.Parse(参数[1]);
    }

    void 指令_BeforeVideo(string[] 参数)
    {
        BeforeVideo_row = 搜寻Tag行(参数[1]);
        BeforeVideo = true;
    }

    void 指令_OverTheChapter(string[] 参数)
    {
        StatePlay(false);     //退出
    }

    //goto —— 下次去到某个row
    void 指令_goto(string[] 参数)
    {
        gotoRow = 搜寻Tag行(参数[1]);
    }

    //gotoN —— 直接去到某个row
    void 指令_gotoN(string[] 参数)
    {
        row = 搜寻Tag行(参数[1]) - 1;
        下一行();
    }

    //autoChoose >> 几秒 >> 去哪（用于Button自动选择）
    void 指令_autoChoose(string[] 参数)
    {
        ChoiseManager.chooseTimePassed = 0f;
        ChoiseManager.AutoChooseTime = float.Parse(参数[1]);
        ChoiseManager.autoChooseTo = 搜寻Tag行(参数[2]);
        hasAutoChooseCommand = true;
    }

    void 指令_wait(string[] 参数)
    {
        clickCheckTimer = 0f;
        clickWaitTime = float.Parse(参数[1]);
    }

    void 指令_自动延时(string[] 参数)
    {
        autoCheckTimer = 0f;
        autoWaitTime = float.Parse(参数[1]);
    }

    void 指令_goChapter(string[] 参数)
    {
        setupChapter(参数[1]);

        //删除历史
        HistoryRecord.ClearHistory();

        下一行();
    }

    //弹出信息框  (信息框 >> 文本 >> 标题 >> 是)
    void 指令_信息框(string[] 参数)
    {
        MessageBox.Show(TAG文本替换(参数[1]), 参数[2], DialogResult.Ok);
        MessageBox.okText = 参数[3];

        UiMessage.open();
    }

    //弹出Value信息框  (值信息框 >> Tag >> 文本 >> 标题 >> 是 >> 否)
    void 指令_值信息框(string[] 参数)
    {
        string creatV = 参数[1];
        string text = 参数[2];
        string title = 参数[3];
        string yes = 参数[4];
        string no = 参数[5];

        MessageBox.Show(text, title, DialogResult.OkCancel);
        MessageBox.okText = yes;
        MessageBox.cancelText = no;

        //先删除旧值（修复：原实现误用整段指令字符串匹配，导致旧值永远不会被删除）
        foreach (string s in SaveManager.data0.事件标签)
        {
            if (s.StartsWith(creatV))
            {
                SaveManager.data0.事件标签.Remove(s);
                break;
            }
        }

        MessageBox.clickOk = () =>
        {
            //再添加
            SaveManager.data0.事件标签.Add(creatV + "=True");
        };
        MessageBox.clickCancel = () =>
        {
            //再添加
            SaveManager.data0.事件标签.Add(creatV + "=False");
        };

        UiMessage.open();
    }

    //弹出提示框 noti >> 文本 >> 标题 >> 时间
    void 指令_noti(string[] 参数)
    {
        NotiBox.showTime = float.Parse(参数[3]);
        NotiBox.Show(参数[1], 参数[2]);
    }

    void 指令_save(string[] 参数)
    {
        MessageBox.Show("您或许需要存档，你的存档将会保存在你使用的存档位中……", "存档", DialogResult.OkCancel);
        MessageBox.okText = "存档";
        MessageBox.cancelText = "不需要";
        UiMessage.open();
        MessageBox.clickOk = () =>
        {
            SaveManager.inti.SaveByPresent();
            NotiBox.Show("已保存当前进度", "存档");
            NotiBox.showTime = 2;
        };
    }

    void 指令_saveN(string[] 参数)
    {
        UiMessage.open();
        SaveManager.inti.SaveByPresent();
        NotiBox.Show("已保存当前进度", "存档");
        NotiBox.showTime = 2;
    }

    //random >> 1 >> 2 >> 3(>...)
    void 指令_random(string[] 参数)
    {
        int i = random.Next(1, 参数.Length);
        row = 搜寻Tag行(参数[i]) - 1;
        下一行();
    }

    //摄像机摇动
    void 指令_界面抖动(string[] 参数)
    {
        cameraShake.Restart();
    }

    //存档事件标签
    void 指令_addTag(string[] 参数)
    {
        if (!SaveManager.data0.事件标签.Contains(参数[1]))
            SaveManager.data0.事件标签.Add(参数[1]);

        TagManager.addGlobalTag(参数[1]);
    }

    void 指令_removeTag(string[] 参数)
    {
        if (SaveManager.data0.事件标签.Contains(参数[1]))
            SaveManager.data0.事件标签.Remove(参数[1]);
    }

    void 指令_removeTags(string[] 参数)
    {
        foreach (string singleTag in 参数[1].Split())
        {
            if (SaveManager.data0.事件标签.Contains(singleTag))
                SaveManager.data0.事件标签.Remove(singleTag);
        }
    }

    //全局事件标签
    void 指令_addGlobalTag(string[] 参数)
    {
        TagManager.addGlobalTag(参数[1]);
    }

    //addValueTag >> 名字 >> 值
    void 指令_addValueTag(string[] 参数)
    {
        //先删除
        foreach (string s in SaveManager.data0.事件标签)
        {
            if (s.StartsWith(参数[1] + "="))
            {
                SaveManager.data0.事件标签.Remove(s);
                break;
            }
        }
        //再添加
        SaveManager.data0.事件标签.Add(参数[1] + "=" + 参数[2]);
        TagManager.addGlobalTag(参数[1]);
    }

    //removeValueTag >> 名字
    void 指令_removeValueTag(string[] 参数)
    {
        foreach (string s in SaveManager.data0.事件标签)
        {
            if (s.StartsWith(参数[1] + "="))
            {
                SaveManager.data0.事件标签.Remove(s);
                break;
            }
        }
    }

    //changeValueTag >> 名字 >> +/-/=数值
    void 指令_changeValueTag(string[] 参数)
    {
        foreach (string s in SaveManager.data0.事件标签)
        {
            if (!s.StartsWith(参数[1] + "="))
                continue;

            SaveManager.data0.事件标签.Remove(s);

            string tagName = s.Split("=")[0];
            string oldValue = s.Split("=")[1];

            //value >> -1 / +1 / 114 / =-114
            if (参数[2].StartsWith("+"))
                tagName += "=" + (int.Parse(oldValue) + int.Parse(参数[2]));
            else if (参数[2].StartsWith("-"))
                tagName += "=" + (int.Parse(oldValue) - int.Parse(参数[2]));
            else if (参数[2].StartsWith("="))
                tagName += "=" + 参数[2][1..];
            else
                tagName += "=" + 参数[2];

            SaveManager.data0.事件标签.Add(tagName);
            TagManager.addGlobalTag(tagName);

            break;
        }
    }

    //解锁档案 >> #TAG  (I - 世界观设定)
    void 指令_解锁档案(string[] 参数)
    {
        TagManager.addGlobalTag("Profile:Unlock:" + 参数[1]);
    }

    void 指令_解锁档案_提示(string[] 参数)
    {
        if (!TagManager.collectedTags.collectTags.Contains("Profile:Unlock:" + 参数[1]))
        {
            NotiBox.Show("已更新", "档案");
            NotiBox.showTime = 4;
        }
        TagManager.addGlobalTag("Profile:Unlock:" + 参数[1]);
    }

    void 指令_头像统一(string[] 参数)
    {
        头像统一 = bool.Parse(参数[1]);
        SaveManager.data0.头像统一 = 头像统一;
    }

    void 指令_背景缩放比例(string[] 参数)
    {
        背景缩放比例 = 参数[1].Equals("默认") ? 1.00f : float.Parse(参数[1]);
        SaveManager.data0.背景缩放比例 = 背景缩放比例;
        背景对齐需要重算 = true;
    }

    void 指令_LoadScene(string[] 参数)
    {
        预加载场景 = SceneManager.LoadSceneAsync(参数[1]);
        预加载场景.allowSceneActivation = false;

        场景加载界面.SetActive(true);
    }

    //立绘动作
    void 指令_播放立绘动画(string[] 参数)
    {
        立绘动画管理器.Apply(参数[1]);
    }
}
