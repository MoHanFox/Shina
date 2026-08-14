using UnityEngine;
using UnityEngine.Video;

public partial class Audiobook
{
    /// <summary>一行剧情的全部列数据（读取后统一 TAG 替换再应用）</summary>
    private struct StoryRowData
    {
        public string 人物, 对话, 语音, 背景, 背景音乐, 立绘, 头像, 选项, QTE, 指令;
    }

    /// <summary>
    /// 把一行剧情应用到界面。
    /// 顺序与原实现一致：名字 → 对话 → 语音 → 背景音乐 → 立绘 → 头像 → 背景 → 选项/QTE/指令/历史。
    /// </summary>
    void 内容更新(StoryRowData data, bool recovery = false)
    {
        //结构体未赋值的字段默认为 null（如存档恢复路径只填部分字段），统一规整为空串
        data.人物 ??= "";
        data.对话 ??= "";
        data.语音 ??= "";
        data.背景 ??= "";
        data.背景音乐 ??= "";
        data.立绘 ??= "";
        data.头像 ??= "";
        data.选项 ??= "";
        data.QTE ??= "";
        data.指令 ??= "";

        //预处理：淡入标记与中心文本标记（先于名字更新）
        if (data.对话 != "" && data.对话 != 文本框说话内容.text)
        {
            打字动画开关 = data.对话.Contains("<淡入>");
            data.对话 = data.对话.Replace("<淡入>", "");

            中心文本(data.对话.Contains("<中心文本>"));
            data.对话 = data.对话.Replace("<中心文本>", "");
        }

        更新名字(data);

        //tag >> 行：原逻辑在此直接 return，跳过语音/背景/选项等所有后续更新
        if (更新对话文本(data))
            return;

        更新语音(data);
        更新背景音乐(data);
        更新立绘(data, recovery);
        更新头像(data);
        更新背景与视频(data, recovery);

        if (recovery)
            return;

        //选项 10
        choiseManager.DeleteAll();
        if (data.选项 != "")
            choiseManager.post(data.选项);

        //QTE 11
        if (data.QTE != "")
        {
            qteRect.localPosition = new Vector3(0, 0);
            qTEManager.post(data.QTE);
        }

        //指令 12
        SpecialCommand(data.指令);

        //所作所为记入历史
        if (data.人物 != "&Scene")
            HistoryRecord.addContext((主名字.text == "&VO") ? "-" : 主名字.text, data.对话);
    }

    //============================== 1 名字 ==============================

    void 更新名字(StoryRowData data)
    {
        //注意：条件保持原实现（s对话 != 主名字.text），保证中心文本模式切换时名字OBJ显隐仍能刷新
        if (data.人物 == "" || data.对话 == 主名字.text)
            return;

        主名字.text = data.人物;
        SaveManager.data0.人物 = data.人物;

        if (data.人物 == "&Scene")
        {
            阅读覆盖.SetActive(false);
        }
        else if (!阅读覆盖.activeInHierarchy)
            阅读覆盖.SetActive(true);

        bool 隐藏名字OBJ = 主名字.text == "&VO" || 中心文本状态;
        if (名字OBJ.activeInHierarchy && 隐藏名字OBJ)
            名字OBJ.SetActive(false);
        if (!名字OBJ.activeInHierarchy && !隐藏名字OBJ)
            名字OBJ.SetActive(true);
    }

    //============================== 2 对话文本 ==============================

    /// <summary>更新对话文本。返回 true 表示该行是 tag 指令行（原逻辑直接 return，跳过后续所有更新）。</summary>
    bool 更新对话文本(StoryRowData data)
    {
        if (data.对话 == "")
        {
            TextWriter.isActive = false;
            文本框说话内容.text = "";
            同步中心文本();
            return false;
        }

        //tag >> #tag [ ] value >> true >> false
        if (data.对话.StartsWith("tag >> "))
        {
            string[] 分段 = data.对话.Split(" >> ");
            bool boolean = TagManager.满足条件(分段[1]);
            string target = boolean ? 分段[2] : 分段[3];

            GlobalHook.BookToRow(搜寻Tag行(target));

            TextWriter.isActive = false;
            return true;
        }

        //注意：这里不再"文本相同就早退"。
        //进入游戏时 GlobalHook 与 Unity 生命周期可能先后触发两次 OnEnable（读档恢复时则是
        //"还原 + ToRow"两次），第二次应用时文本已相同；若早退，<淡入> 的淡入、打字重放
        //以及中心文本同步都会被跳过 —— <中心文本> 行会因文本框移出屏幕而"字消失"。
        文本框说话内容.text = "";
        if (打字动画开关)
        {
            TextWriter.reRun = false;
            TextWriter.isActive = false;
            TextWriter.words = data.对话;
            文本框说话内容.text = data.对话;

            触发文本淡入();
        }
        else
        {
            TextWriter.words = data.对话;
            TextWriter.reRun = true;
        }

        SaveManager.data0.对话 = data.对话;
        同步中心文本();
        return false;
    }

    /// <summary>触发文本淡入（中心文本或普通文本框）</summary>
    void 触发文本淡入()
    {
        if (中心文本状态)
            中心文本淡入.OnEnable();
        else
            文本框淡入.OnEnable();
    }

    /// <summary>中心文本跟随普通文本框内容</summary>
    void 同步中心文本()
    {
        if (!中心文本状态)
            return;
        if (中心文本Text.text != 文本框说话内容.text)
            中心文本Text.text = 文本框说话内容.text;
    }

    //============================== 7 立绘 ==============================

    void 更新立绘(StoryRowData data, bool recovery)
    {
        if (data.立绘 == "" || (data.立绘 == 上个立绘集 && !recovery))
            return;

        上个立绘集 = data.立绘;
        SaveManager.data0.立绘 = data.立绘;

        生成立绘();
    }

    //============================== 8 头像 ==============================

    void 更新头像(StoryRowData data)
    {
        //注意：offsetMax 调整的是"文本框文字区域"（文本框说话内容）而不是整个对话框容器（文本框），
        //给头像让出右侧空间——原实现如此，缓存 rect 时不要错用容器。
        if (data.头像 != "")
        {
            文本框说话内容.rectTransform.offsetMax = new((data.头像 == "None") ? -125 : -365, 文本框说话内容.rectTransform.offsetMax.y);
            头像.color = (data.头像 == "None") ? new(1, 1, 1, 0) : new(1, 1, 1, 1);
            头像.texture = Resources.Load<Texture2D>("Characters/Feature/" + data.头像);

            SaveManager.data0.头像 = data.头像;
            return;
        }

        if (data.人物 == "")
            return;

        //人物名对应的立绘头像（只加载一次，原实现重复加载了两次）
        Texture2D 人物头像 = Resources.Load<Texture2D>("Characters/Feature/" + data.人物);
        if (人物头像 != null && 头像统一)
        {
            文本框说话内容.rectTransform.offsetMax = new(-365, 文本框说话内容.rectTransform.offsetMax.y);
            头像.color = new(1, 1, 1, 1);
            头像.texture = 人物头像;
        }
        else
        {
            文本框说话内容.rectTransform.offsetMax = new(-125, 文本框说话内容.rectTransform.offsetMax.y);
            头像.color = new(1, 1, 1, 0);
        }
    }

    //============================== 4 背景与CG ==============================

    void 更新背景与视频(StoryRowData data, bool recovery)
    {
        if (data.背景 == "" || data.背景 == 上个背景)     //是否填写的切换什么背景
            return;

        背景Rect.localPosition = new Vector3(0, 0);

        背景缩放比例 = recovery ? SaveManager.data0.背景缩放比例 : 1.00f;

        bool loop = data.背景.EndsWith("|Loop");
        视频播放器.isLooping = loop;
        if (loop)
            data.背景 = data.背景.Replace("|Loop", "");

        UnityEngine.Object needLoadBackground = Resources.Load<UnityEngine.Object>("Background/" + data.背景);

        //判断是否是背景
        if (needLoadBackground is Texture2D t2dBg)
        {
            bgSize = new(t2dBg.width, t2dBg.height);

            if (上个为视频)
            {
                视频播放器.Stop();
                视频播放器.clip = null;
                lastVideoTexture = null;
            }
            上个为视频 = false;   //修复：原实现切回图片后未复位，导致之后每张图都重复 Stop

            背景对齐();
            背景.texture = t2dBg;
        }
        else if (needLoadBackground is VideoClip vCBg)
        {
            视频播放器.clip = vCBg;
            bgSize = new(vCBg.width, vCBg.height);

            背景对齐();

            视频播放器.Play();
            lastVideoTexture = null;   //强制下一帧同步视频纹理

            上个为视频 = true;
        }
        else
        {
            Debug.LogWarning("背景资源不存在: Background/" + data.背景);
        }

        SaveManager.data0.背景 = data.背景;
        上个背景 = data.背景;
    }
}
