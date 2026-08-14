using System.Collections.Generic;
using OfficeOpenXml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Random = System.Random;

/// <summary>
/// 视觉小说主控制器：持有全部界面引用，驱动行流转与内容更新。
/// 对外静态/实例 API 与全部序列化字段保持原样（场景与外部调用点零改动），
/// 内部按职责拆分为 partial 文件：
///   Audiobook.Content.cs    —— 内容更新（名字/对话/语音/背景音乐/立绘/头像/背景/选项/QTE/指令/历史）
///   Audiobook.Audio.cs      —— 语音与背景音乐（AudioSource 组件池，消除每行 AddComponent/Destroy）
///   Audiobook.Commands.cs   —— 剧情指令表
///   Audiobook.Chapter.cs    —— 章节加载 / Excel 生命周期 / TAG 工具
///   Audiobook.Background.cs —— 背景对齐 / 明度渐变 / 视频同步
/// </summary>
public partial class Audiobook : MonoBehaviour
{
    //============================== 静态状态（工程既有 API，其他脚本直接读写，勿删） ==============================
    public static bool startPlaying = false;
    public static string Chapter;
    public static string usingPlatAssestPath;

    public static ExcelWorksheet 剧情文本;
    public static int row;
    public static int gotoRow = -1;
    public static Dictionary<string, int> tags = new();

    public static bool MouseClickedInContent = false;   //鼠标点击切换文本（AutoRead / FastForward / TextLinkButton 置位）

    //============================== 列定义（与 Excel 表头一致） ==============================
    public static readonly int 人物Line = 1, 对话Line = 2, 语音Line = 3, 背景Line = 4, 背景音乐Line = 5, 转场特效Line = 6, 立绘Line = 7, 头像Line = 8, TAG = 9, 选项Line = 10, QTELine = 11, 指令Line = 12;

    //============================== 界面引用（Inspector 绑定，勿改名） ==============================
    [Header("基础文本与图片的配置")]
    public TextMeshProUGUI 主名字;
    public GameObject 名字OBJ;
    public TextMeshProUGUI 文本框说话内容;
    public GameObject 立绘层;
    public RawImage 头像;
    bool 头像统一 = true;
    private string 上个立绘集 = "";
    private string 上个背景 = "";
    public RawImage 覆盖图片;
    public GameObject 遮盖;
    public GameObject 淡入转场覆盖_Obj;
    private GameObject 淡入转场覆盖_实例 = null;

    [Header("背景与gc")]
    public VideoPlayer 视频播放器;
    public RawImage 背景;
    public float 背景缩放比例 = 1.05f;
    float 背景目标明度 = 1.0F;
    private bool BeforeVideo;
    private int BeforeVideo_row;
    public bool 锁定背景 = false;

    //文本选取
    private int childRow;
    private string lastParentWords = "";

    //cg
    bool 上个为视频;

    //拓展
    //场景
    private AsyncOperation 预加载场景 = null;
    public GameObject 场景加载界面;
    private OnEnableFadeIn 场景加载界面动画;   //缓存，避免每帧 GetComponent

    [Header("声音类")]
    public GameObject 语音Object;
    public GameObject 背景音乐Object;
    private string 上个背景音乐集 = "";

    [Header("游戏功能类")]
    public ChoiseManager choiseManager;
    public GameObject 文本框;
    public GameObject 阅读覆盖;
    public AudioSource 切换音频;
    public QTEManager qTEManager;
    public CameraShake cameraShake;
    public GameObject 输入;
    public GameObject 输入框落点;
    public TextMeshProUGUI 中心文本Text;
    public CharacterAnimationManger 立绘动画管理器;

    //关于延迟切换
    float clickCheckTimer = 0f;
    float clickWaitTime = -1f;
    public float autoCheckTimer = 0f;
    public float autoWaitTime = -1f;

    public GameObject 内容更新提示;

    [Header("游戏内界面类")]
    public GameObject 历史;
    public GameObject 游戏内主界面;
    public GameObject 档案;
    public GameObject 设置;
    public GameObject 主菜单;
    public GameObject 提示框;
    bool 打字动画开关;

    //============================== 内部缓存（Awake 中一次性解析，避免逐帧/逐行 GetComponent） ==============================
    private RectTransform 背景Rect;
    private RectTransform 文本框Rect;
    private RectTransform qteRect;
    private OnEnableFadeIn 中心文本淡入;
    private OnEnableFadeIn 文本框淡入;
    private OnEnableFadeIn 提示淡入;
    private StoryAudioSourcePool 语音池;
    private StoryAudioSourcePool 背景音乐池;
    private readonly Random random = new();
    private bool 中心文本状态;
    private float oldAnchoredPositionY = -1f;

    private void Awake()
    {
        背景Rect = 背景.rectTransform;
        文本框Rect = 文本框.GetComponent<RectTransform>();
        qteRect = qTEManager.GetComponent<RectTransform>();
        场景加载界面动画 = 场景加载界面 != null ? 场景加载界面.GetComponent<OnEnableFadeIn>() : null;
        中心文本淡入 = 中心文本Text.GetComponent<OnEnableFadeIn>();
        文本框淡入 = 文本框说话内容.GetComponent<OnEnableFadeIn>();
        提示淡入 = 内容更新提示.GetComponent<OnEnableFadeIn>();
        语音池 = new StoryAudioSourcePool(语音Object);
        背景音乐池 = new StoryAudioSourcePool(背景音乐Object);
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        lastCanvasSize = canvasRect.sizeDelta;
        注册指令();
    }

    public void Start()
    {
        视频播放器.loopPointReached += 视频周期委托;
    }

    public void OnEnable()
    {
        初始化();

        if (SaveManager.data0.progress > 2)
        {
            row = SaveManager.data0.progress;

            //还原
            内容更新(new StoryRowData
            {
                人物 = SaveManager.data0.人物,
                对话 = SaveManager.data0.对话,
                背景 = SaveManager.data0.背景,
                背景音乐 = SaveManager.data0.背景音乐,
                立绘 = SaveManager.data0.立绘,
                头像 = SaveManager.data0.头像,
            }, true);
            ToRow(SaveManager.data0.progress);
        }
        else
        {
            转场(2, null);
        }
    }

    private void OnDisable()
    {
        if (语音Object != null && 语音Object.activeInHierarchy)
            语音Object.SetActive(false);

        if (背景音乐Object != null && 背景音乐Object.activeInHierarchy)
            背景音乐Object.SetActive(false);
    }

    private void OnApplicationQuit()
    {
        释放Excel();
    }

    public void StateMouseClickedInContent(bool s)       //鼠标点击，有点地方切不了（适用于普通点击，有不可切换判断）
    {
        if (不可点击切换())
            return;
        MouseClickedInContent = s;
    }

    void Update()
    {
        //主菜单显隐（原在 OnGUI 中每帧重复设置，这里只在状态变化时设置）
        bool wantMenu = !startPlaying;
        if (主菜单.activeInHierarchy != wantMenu)
            主菜单.SetActive(wantMenu);

        if (!startPlaying)
            return;

        //延迟/自动计时
        if (clickWaitTime > 0f)
            clickCheckTimer += Time.deltaTime;

        if (autoWaitTime > 0f)
            autoCheckTimer += Time.deltaTime;

        //场景预加载
        if (预加载场景 != null)
        {
            场景加载界面.SetActive(true);
            if (场景加载界面动画 != null && 场景加载界面动画.done) //淡入动画完成才允许切换
            {
                场景加载界面.SetActive(false);
                预加载场景.allowSceneActivation = true;
                预加载场景 = null;
            }
        }

        //视频纹理与音量（仅变化时赋值，原为每帧赋值）
        if (视频播放器.isPlaying)
        {
            Texture tex = 视频播放器.texture;
            if (tex != null && tex != lastVideoTexture)
            {
                lastVideoTexture = tex;
                背景.texture = tex;
            }
        }
        float volume = ConfigManager.config.背景音乐响度;
        if (!Mathf.Approximately(lastVideoVolume, volume))
        {
            lastVideoVolume = volume;
            视频播放器.SetDirectAudioVolume(0, volume);
        }

        //背景对齐：仅在背景/缩放比例/画布尺寸变化时重算
        if (背景对齐需要重算 || canvasRect.sizeDelta != lastCanvasSize)
            背景对齐();

        明度渐变Tick();

        //中心文本跟随打字进度（原在 OnGUI 每帧同步，这里仅在文本变化时同步）
        if (中心文本状态 && TextWriter.isActive)
            同步中心文本();

        //DEBUG：Shift+R 重载章节
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
        {
            int lastRow = row;
            setupChapter(Chapter);
            row = lastRow;

            NotiBox.Show("刷新小说内容", "DEBUG");
        }

        //GlobalHook 跳行请求
        if (GlobalHook.bookToRow)
        {
            GlobalHook.bookToRow = false;
            row = GlobalHook.bookToRowInt - 1;
            下一行();
        }

        //内容更新提示（仅状态变化时设置）
        bool showHint = !不可点击切换();
        if (内容更新提示.activeInHierarchy != showHint)
            内容更新提示.SetActive(showHint);

        //ESC：呼出/关闭游戏内主界面
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!历史.activeInHierarchy & !档案.activeInHierarchy & !设置.activeInHierarchy)
                游戏内主界面.SetActive(!游戏内主界面.activeInHierarchy);

            if (历史.activeInHierarchy)
                历史.SetActive(false);
            if (档案.activeInHierarchy)
                档案.SetActive(false);
            if (设置.activeInHierarchy)
                设置.SetActive(false);
        }

        if (不可点击切换())
            return;

        //文本推进（Space / Enter / 点击内容）
        if ((Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.KeypadEnter) || MouseClickedInContent) && !TransitionAnim.doAnim)
        {
            if (MouseClickedInContent)
                MouseClickedInContent = false;

            //多段对话（|| 分隔）逐段推进
            if (lastParentWords.Contains(" || "))
            {
                var 间断 = lastParentWords.Split(" || ");
                if (childRow != 间断.Length - 1)
                {
                    childRow++;
                    文本框说话内容.text = "";
                    if (打字动画开关)
                    {
                        TextWriter.isActive = false;
                        TextWriter.words = 间断[childRow];
                        文本框说话内容.text = 间断[childRow];

                        触发文本淡入();
                    }
                    else
                    {
                        if (!TextWriter.isActive)
                        {
                            TextWriter.words = 间断[childRow];
                            TextWriter.reRun = true;
                        }
                        else
                        {
                            TextWriter.isActive = false;
                            文本框说话内容.text = 间断[childRow];
                            同步中心文本();
                            return;
                        }
                    }

                    同步中心文本();
                    切换音频.Play();
                    return;
                }
                else
                {
                    childRow = 0;
                    下一行();
                    return;
                }
            }

            if (!TextWriter.isActive)
            {
                下一行();
            }
            else
            {
                TextWriter.isActive = false;
                文本框说话内容.text = TextWriter.words;
                同步中心文本();
            }
        }
    }

    //============================== 行流转 ==============================

    public void ToRow(int targetRow)
    {
        if (内容更新提示.activeInHierarchy)
            提示淡入.OnEnable();

        StoryRowData d = new()
        {
            人物 = CellString(targetRow, 人物Line),
            对话 = CellString(targetRow, 对话Line),
            语音 = CellString(targetRow, 语音Line),
            背景 = CellString(targetRow, 背景Line),
            背景音乐 = CellString(targetRow, 背景音乐Line),
            立绘 = CellString(targetRow, 立绘Line),
            头像 = CellString(targetRow, 头像Line),
            选项 = CellString(targetRow, 选项Line),
            QTE = CellString(targetRow, QTELine),
            指令 = CellString(targetRow, 指令Line),
        };

        //TAG 替换（%tag% 等）
        d.人物 = TAG文本替换(d.人物);
        d.对话 = TAG文本替换(d.对话);
        d.立绘 = TAG文本替换(d.立绘);
        d.头像 = TAG文本替换(d.头像);
        d.QTE = TAG文本替换(d.QTE);
        d.选项 = TAG文本替换(d.选项);
        d.背景 = TAG文本替换(d.背景);
        d.语音 = TAG文本替换(d.语音);
        d.指令 = TAG文本替换(d.指令);

        //处理超链接
        d.对话 = d.对话.Replace("<link=", "<color=#FFC901>「<link=").Replace("</link>", "</link>」</color>");

        lastParentWords = d.对话;

        if (d.对话.Contains(" || "))
            d.对话 = d.对话.Split(" || ")[0];

        内容更新(d);
    }

    public void 上一行()
    {
        切换音频.Play();

        if (qTEManager.hasQTE() || choiseManager.限时框.activeInHierarchy)
            return;

        //多段对话（|| 分隔）逐段回退
        if (lastParentWords.Contains(" || ") && childRow != 0)
        {
            var 间断 = lastParentWords.Split(" || ");
            childRow--;

            文本框说话内容.text = "";

            if (打字动画开关)
            {
                TextWriter.isActive = false;
                TextWriter.words = 间断[childRow];

                触发文本淡入();

                文本框说话内容.text = 间断[childRow];
            }
            else
            {
                TextWriter.words = 间断[childRow];
                TextWriter.reRun = true;
            }

            同步中心文本();
            return;
        }

        if (row <= 2)
            return;

        //后退标志 backTo：跳转到指定 TAG
        string 指令行 = CellString(row, 指令Line);
        if (!string.IsNullOrEmpty(指令行) && 指令行.Contains("backTo"))
        {
            foreach (string cmd in 指令行.Split('|'))
            {
                if (cmd.StartsWith("backTo"))
                {
                    int backToTag = 搜寻Tag行(cmd.Split(" >> ")[1]);
                    //重置自动
                    AutoRead.passed = 0;

                    SaveManager.data0.progress = backToTag;
                    SaveManager.data0.chapter = Chapter;

                    转场(backToTag, CellString(backToTag, 转场特效Line));
                    return;
                }
            }
        }

        int oldRow = row;
        int backTo = row - 1;

        //无法后退标志
        string 上一行人物 = CellString(backTo, 人物Line);
        if (!string.IsNullOrEmpty(上一行人物) && 上一行人物.Contains("//"))
            return;

        string 上一行指令 = CellString(backTo, 指令Line);
        if (!string.IsNullOrEmpty(上一行指令) && 上一行指令.Contains("cantBackTo"))
            return;

        if (!string.IsNullOrEmpty(指令行) && 指令行.Contains("cantBack"))
            return;

        //重置自动
        AutoRead.passed = 0;

        SaveManager.data0.progress = backTo;
        SaveManager.data0.chapter = Chapter;

        转场(backTo, CellString(oldRow, 转场特效Line));
    }

    public void 下一行()
    {
        切换音频.Play();

        //重置自动
        AutoRead.passed = 0;
        //删除qtes与选项
        qTEManager.DeleteAll();
        choiseManager.DeleteAll();
        BeforeVideo = false;

        if (gotoRow != -1)
        {
            row = gotoRow;
            gotoRow = -1;
        }
        else
            row++;

        SaveManager.data0.progress = row;
        SaveManager.data0.chapter = Chapter;

        转场(row, CellString(row, 转场特效Line));
    }

    /// <summary>行切换：先应用转场特效，再展示目标行</summary>
    public void 转场(int target, string valueO)         //valueO 为转场特效列内容（白场/黑场/淡入/空）
    {
        row = target;

        //6 是否填写了黑场白场的转场特效
        if (!string.IsNullOrEmpty(valueO))
        {
            if (valueO == "白场" || valueO == "黑场")
            {
                覆盖图片.color = valueO == "白场" ? new Color(1, 1, 1, 0) : new Color(0, 0, 0, 0);
                遮盖.SetActive(true);
            }
            if (valueO == "淡入")
            {
                淡入转场覆盖_实例 = Instantiate(淡入转场覆盖_Obj);
                RectTransform 实例Rect = 淡入转场覆盖_实例.GetComponent<RectTransform>();
                RawImage 实例图 = 淡入转场覆盖_实例.GetComponent<RawImage>();
                GraphicFadeOutAndDestroy 实例动画 = 淡入转场覆盖_实例.GetComponent<GraphicFadeOutAndDestroy>();

                实例Rect.SetParent(背景Rect);
                实例动画.time = 0.25f;
                实例Rect.localScale = new(1, 1, 1);
                实例Rect.localPosition = new(0, 0);
                实例Rect.sizeDelta = 背景Rect.sizeDelta;
                实例图.texture = 背景.texture;
                实例图.color = new(1f, 1f, 1f, 背景目标明度);
                ToRow(target);
            }
        }
        else
        {
            ToRow(target);
        }
    }

    public bool 不可点击切换()        //你点不了，但可以用强制手段
    {
        if (历史.activeInHierarchy)
            return true;
        if (档案.activeInHierarchy)
            return true;
        if (提示框.activeInHierarchy)
            return true;
        if (qTEManager.hasQTE())
            return true;
        if (choiseManager.hasChoices())
            return true;
        if (TransitionAnim.doAnim)
            return true;
        if (clickCheckTimer <= clickWaitTime)
            return true;
        if (!淡入转场覆盖_实例.IsUnityNull())
            return true;
        if (输入框落点.transform.childCount > 0)
            return true;
        if (!startPlaying)
            return true;

        return false;
    }

    public void StatePlay(bool state)
    {
        startPlaying = state;

        if (!startPlaying)
            Resources.UnloadUnusedAssets();
    }

    public void 中心文本(bool state)
    {
        if (oldAnchoredPositionY < 0f)
            oldAnchoredPositionY = 文本框Rect.anchoredPosition.y;

        中心文本状态 = state;
        中心文本Text.text = "";
        文本框Rect.anchoredPosition = new(文本框Rect.anchoredPosition.x, state ? 114514 : oldAnchoredPositionY);
    }

    public void 初始化()
    {
        Resources.UnloadUnusedAssets();

        //搜索Tag
        tags = SearchForTags();

        gotoRow = -1;

        //重置
        ChoiseManager.AutoChooseTime = -1f;
        ChoiseManager.chooseTimePassed = 0f;
        ChoiseManager.autoChooseTo = 0;

        视频播放器.Stop();
        lastVideoTexture = null;
        lastVideoVolume = float.MinValue;

        背景目标明度 = SaveManager.data0.背景明度;
        背景缩放比例 = SaveManager.data0.背景缩放比例;
        头像统一 = SaveManager.data0.头像统一;
        背景对齐需要重算 = true;

        背景Rect.localPosition = new Vector3(0, 0);

        //清空当前立绘
        清空立绘();
        头像.color = new(1, 1, 1, 0);

        if (!语音Object.activeInHierarchy)
            语音Object.SetActive(true);

        if (!背景音乐Object.activeInHierarchy)
            背景音乐Object.SetActive(true);

        //删除qtes
        qTEManager.DeleteAll();

        //删除历史
        HistoryRecord.ClearHistory();

        //语音与背景音乐全部停止并回收（等价原 Destroy 全部 AudioSource）
        语音池.StopAll();
        背景音乐池.StopAll();

        //重置"上个X"跟踪：退出到主菜单再进入时（同一实例 OnEnable）若不清空，
        //背景音乐/背景/立绘会因"与上一行相同"被跳过 —— 音乐消失、立绘不显示。
        //清空后，首次进入的行会重新应用背景音乐/背景/立绘。
        上个背景音乐集 = "";
        上个背景 = "";
        上个立绘集 = "";

        立绘动画管理器.disEmploy();
    }

    //============================== 立绘 ==============================

    public void 清空立绘()
    {
        Transform transform;
        for (int i = 0; i < 立绘层.transform.childCount; i++)
        {
            transform = 立绘层.transform.GetChild(i);
            Destroy(transform.gameObject);
        }
    }

    public void 生成立绘(bool 入场动画 = true)
    {
        清空立绘();
        if (上个立绘集 == "None")
            return;

        var 分隔号 = 上个立绘集.Split("|");
        int 角色数位 = 0;

        float totalWidth = 1920;
        float leftBound = -totalWidth / 2f; // -960
        float segmentWidth = totalWidth / 分隔号.Length; // 每一份的宽度

        foreach (var item in 分隔号)
        {
            var 参数 = item.Split(" >> ");
            bool 翻转 = 参数[0].StartsWith("^");

            //创建立绘
            GameObject 新立绘 = new(角色数位.ToString()); //以使用ID为名字，方便后续调用动作
            RawImage newImage = 新立绘.AddComponent<RawImage>();
            //目前只支持淡入淡出，但是要入场动画的话，就只有先透明了
            newImage.color = new Color(1, 1, 1, 0f);
            newImage.texture = Resources.Load<Texture2D>("Characters/" + 参数[0].Replace("^", ""));
            newImage.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080); //和文件一致
            if (翻转)
                newImage.rectTransform.localScale = new Vector2(-1, 1);

            新立绘.transform.SetParent(立绘层.transform, false); //设置到立绘层
            RectTransform rt = newImage.GetComponent<RectTransform>();

            float x = leftBound + (角色数位 + 0.5f) * segmentWidth;
            rt.anchoredPosition = new Vector2(x, 0); // Y轴置中

            if (item.Contains(" >> ") && 入场动画)
            {
                if (参数[1] == "淡入")
                    newImage.AddComponent<FadeIn>().Rest();
                if (参数[1] == "淡出")
                    newImage.AddComponent<FadeOut>().Rest();
            }
            else
                newImage.color = new Color(1, 1, 1, 1.0f);

            角色数位++;
        }
    }
}
