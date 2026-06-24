using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEngine.Video;
using Random = System.Random;
using System;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Linq;

public class Audiobook : MonoBehaviour
{
    // Start is called before the first frame update
    public static bool startPlaying = false;
    public static string Chapter;
    public static string usingPlatAssestPath;

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
    public static ExcelWorksheet 剧情文本;
    public static int row;
    public static int gotoRow = -1;
    private int childRow;
    private string lastParentWords = "";
    public static Dictionary<string, int> tags = new();

    //cg
    bool 上个为视频;

    //拓展
    //场景
    private AsyncOperation 预加载场景 = null;
    public GameObject 场景加载界面;

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

    public static readonly int 人物Line = 1, 对话Line = 2, 语音Line = 3, 背景Line = 4, 背景音乐Line = 5, 转场特效Line = 6, 立绘Line = 7, 头像Line = 8, TAG = 9, 选项Line = 10, QTELine = 11, 指令Line = 12;

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
            内容更新(SaveManager.data0.人物, SaveManager.data0.对话, "", SaveManager.data0.背景, SaveManager.data0.背景音乐, SaveManager.data0.立绘, SaveManager.data0.头像, "", "", "", true);
            ToRow(SaveManager.data0.progress);
        }
        else
        {
            转场(2, null);
        }
    }

    private void OnDisable()
    {
        if (语音Object.activeInHierarchy)
            语音Object.SetActive(false);

        if (背景音乐Object.activeInHierarchy)
            背景音乐Object.SetActive(false);
    }

    public static bool MouseClickedInContent = false;       //这个是用于鼠标点击切换文本

    public void StateMouseClickedInContent(bool s)       //鼠标点击，有点地方切不了（适用于普通点击，有不可切换判断）
    {
        if (不可点击切换())
            return;
        MouseClickedInContent = s;
    }

    void Update()
    {
        if (!startPlaying)
            return;

        if (clickWaitTime > 0f)
            clickCheckTimer += Time.deltaTime;

        if (autoWaitTime > 0f)
            autoCheckTimer += Time.deltaTime;

        if (视频播放器.isPlaying)
            背景.texture = 视频播放器.texture;

        if (预加载场景 != null)
        {
            场景加载界面.SetActive(true);
            if (场景加载界面.GetComponent<OnEnableFadeIn>().done) //说明动画完成
            {
                场景加载界面.SetActive(false);
                预加载场景.allowSceneActivation = true;
                预加载场景 = null;
            }
        }

        背景对齐();

        视频播放器.SetDirectAudioVolume(0, ConfigManager.config.背景音乐响度);
    }

    void OnGUI()
    {
        主菜单.SetActive(!startPlaying);

        if (!startPlaying)
            return;

        //中心文本跟随
        if (中心文本状态)
            中心文本Text.text = 文本框说话内容.text;

        //背景明度
        float 当前背景明度 = 背景.color.r;

        if (背景目标明度 < 当前背景明度)
        {
            当前背景明度 -= Time.deltaTime;

            if (背景目标明度 < 0)
                当前背景明度 = 背景目标明度;

            背景.color = new(当前背景明度, 当前背景明度, 当前背景明度);
        }
        else if (背景目标明度 > 当前背景明度)
        {
            当前背景明度 += Time.deltaTime;

            if (当前背景明度 > 背景目标明度)
                当前背景明度 = 背景目标明度;

            背景.color = new(当前背景明度, 当前背景明度, 当前背景明度);
        }

        //DEBUG   -- Rest
        if (Event.current.isKey && Event.current.keyCode != KeyCode.None)
            if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.R))
            {
                int lastRow = row;
                setupChapter(Chapter);
                row = lastRow;

                NotiBox.Show("刷新小说内容", "DEBUG");
            }

        if (GlobalHook.bookToRow)
        {
            GlobalHook.bookToRow = false;
            row = GlobalHook.bookToRowInt - 1;
            下一行();
        }

        内容更新提示.SetActive(!不可点击切换());

        //ESC
        if (Event.current.isKey && Event.current.keyCode != KeyCode.None)
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

        if ((Event.current.isKey && Event.current.keyCode != KeyCode.None || MouseClickedInContent) && !TransitionAnim.doAnim)
        {
            if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.KeypadEnter) || MouseClickedInContent)
            {

                if (MouseClickedInContent)
                    MouseClickedInContent = false;

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

                            if (中心文本状态)
                                中心文本Text.GetComponent<OnEnableFadeIn>().OnEnable();
                            else
                                文本框说话内容.GetComponent<OnEnableFadeIn>().OnEnable();
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

                                return;
                            }
                        }

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
                }
            }
        }
    }

    // Update is called once per frame
    public void ToRow(int targetRow)
    {
        if (内容更新提示.activeInHierarchy)
            内容更新提示.GetComponent<OnEnableFadeIn>().OnEnable();

        string s人物 = "";
        string s对话 = "";
        string s语音 = "";
        string s背景 = "";
        string s背景音乐 = "";
        //string s转场特效 = "";
        string s立绘 = "";
        string s头像 = "";
        string s选项 = "";
        string s指令 = "";
        string sQTE = "";

        //获取人物
        if (!剧情文本.Cells[targetRow, 人物Line].Value.IsUnityNull())
            s人物 = 剧情文本.Cells[targetRow, 人物Line].Value.ToString();
        if (!剧情文本.Cells[targetRow, 对话Line].Value.IsUnityNull())
            s对话 = 剧情文本.Cells[targetRow, 对话Line].Value.ToString();
        if (!剧情文本.Cells[targetRow, 语音Line].Value.IsUnityNull())
            s语音 = 剧情文本.Cells[targetRow, 语音Line].Value.ToString();
        if (!剧情文本.Cells[targetRow, 背景Line].Value.IsUnityNull())
            s背景 = 剧情文本.Cells[targetRow, 背景Line].Value.ToString();
        if (!剧情文本.Cells[targetRow, 背景音乐Line].Value.IsUnityNull())
            s背景音乐 = 剧情文本.Cells[targetRow, 背景音乐Line].Value.ToString();
        //if (!剧情文本.Cells[targetRow, 转场特效Line].Value.IsUnityNull())
        //    s转场特效 = 剧情文本.Cells[row, 转场特效Line].Value.ToString();
        if (!剧情文本.Cells[targetRow, 立绘Line].Value.IsUnityNull())
            s立绘 = 剧情文本.Cells[targetRow, 立绘Line].Value.ToString();
        if (!剧情文本.Cells[targetRow, 头像Line].Value.IsUnityNull())
            s头像 = 剧情文本.Cells[targetRow, 头像Line].Value.ToString();
        if (!剧情文本.Cells[targetRow, 选项Line].Value.IsUnityNull())
            s选项 = 剧情文本.Cells[targetRow, 选项Line].Value.ToString();
        if (!剧情文本.Cells[targetRow, QTELine].Value.IsUnityNull())
            sQTE = 剧情文本.Cells[targetRow, QTELine].Value.ToString();
        if (!剧情文本.Cells[targetRow, 指令Line].Value.IsUnityNull())
            s指令 = 剧情文本.Cells[targetRow, 指令Line].Value.ToString();

        s对话 = TAG文本替换(s对话);
        s人物 = TAG文本替换(s人物);
        s立绘 = TAG文本替换(s立绘);
        s头像 = TAG文本替换(s头像);
        sQTE = TAG文本替换(sQTE);
        s选项 = TAG文本替换(s选项);
        s背景 = TAG文本替换(s背景);
        s语音 = TAG文本替换(s语音);
        s指令 = TAG文本替换(s指令);

        //处理超链接
        s对话 = s对话.Replace("<link=", "<color=#FFC901>「<link=").Replace("</link>", "</link>」</color>");

        lastParentWords = s对话;

        if (s对话.Contains(" || "))
            s对话 = s对话.Split(" || ")[0];

        内容更新(s人物, s对话, s语音, s背景, s背景音乐, s立绘, s头像, s选项, sQTE, s指令);
    }

    void 内容更新(string s人物, string s对话, string s语音, string s背景, string s背景音乐, string s立绘, string s头像, string s选项, string sQTE, string s指令, bool recovery = false)
    {
        Resources.UnloadUnusedAssets();
        if (s对话 != "" && s对话 != 文本框说话内容.text)
        {
            打字动画开关 = s对话.Contains("<淡入>");
            s对话 = s对话.Replace("<淡入>", "");

            中心文本(s对话.Contains("<中心文本>"));
            s对话 = s对话.Replace("<中心文本>", "");
        }

        //名字 1
        if (s人物 != "" && s对话 != 主名字.text)
        {
            if (s人物 != "&VO" && !s人物.StartsWith("//"))
            {    //旁白
                主名字.text = s人物;
                SaveManager.data0.人物 = s人物;
            }

            if (s人物 == "&VO")
            {    //旁白
                主名字.text = "";
                SaveManager.data0.人物 = "&VO";
            }


            if (s人物 == "&Scene")
            {
                阅读覆盖.SetActive(false);
            }
            else
                if (!阅读覆盖.activeInHierarchy)
                    阅读覆盖.SetActive(true);

            bool 隐藏名字OBJ = 主名字.text == "&Scene" || 主名字.text == "" || 中心文本状态;
            if (名字OBJ.activeInHierarchy && 隐藏名字OBJ)
                名字OBJ.SetActive(false);
            if (!名字OBJ.activeInHierarchy && !隐藏名字OBJ)
                名字OBJ.SetActive(true);
        }

        //文本框说话内容 2
        if (s对话 != "")
        {
            if (!s对话.StartsWith("tag >> "))
            {
                if (s对话 != 文本框说话内容.text)
                {
                    文本框说话内容.text = "";
                    //文本框说话内容.text = 剧情文本.Cells[row, 2].Value.ToString();    

                    if (打字动画开关)
                    {
                        TextWriter.reRun = false;
                        TextWriter.isActive = false;
                        TextWriter.words = s对话;
                        文本框说话内容.text = s对话;

                        if (中心文本状态)
                            中心文本Text.GetComponent<OnEnableFadeIn>().OnEnable();
                        else
                            文本框说话内容.GetComponent<OnEnableFadeIn>().OnEnable();
                    }
                    else
                    {
                        TextWriter.words = s对话;
                        TextWriter.reRun = true;
                    }

                    SaveManager.data0.对话 = s对话;
                }
            }
            else
            {
                //tag >> #tag [ ] value >> true >> false
                bool boolean = TagManager.满足条件(s对话.Split(" >> ")[1]);

                string target = boolean ? s对话.Split(" >> ")[2] : s对话.Split(" >> ")[3];

                int 去哪行 = 搜寻Tag行(target);

                GlobalHook.BookToRow(去哪行);

                TextWriter.isActive = false;
                return;
            }
        }
        else
        {
            TextWriter.isActive = false;
        }

        //语音 3 
        //删除克隆
        foreach (var audio in 语音Object.GetComponents<AudioSource>())
        {
            Destroy(audio);
        }
        if (s语音 != "" && startPlaying)
        {
            foreach (var value in s语音.Split("|"))
            {
                AudioClip loadedRes = Resources.Load("Audio/" + value, typeof(AudioClip)) as AudioClip;
                var newO = 语音Object.AddComponent<AudioSource>();
                newO.clip = loadedRes;
                newO.Play();
            }
        }

        //背景音乐 5 
        if (s背景音乐 != "" && s背景音乐 != 上个背景音乐集)
        {
            if (s背景音乐 == "None")
                foreach (var audio in 背景音乐Object.GetComponents<AudioSource>())
                    Destroy(audio);
            else
            {
                var 使用背景音乐集合 = s背景音乐.Split("|");
                var 过去使用的背景音乐集合 = 上个背景音乐集.Split("|");

                var intersectedList = 使用背景音乐集合.Intersect(过去使用的背景音乐集合);  //交集

                //删除交集以外的背景音乐脚本
                foreach (var audio in 背景音乐Object.GetComponents<AudioSource>())
                {
                    try
                    {
                        var 交集音乐名称 = new List<string>();
                        foreach (var item in intersectedList)
                        {
                            var items = item.Split("/");
                            交集音乐名称.Add(items[items.Length - 1]);
                        }
                        if (!交集音乐名称.Contains(audio.clip.name))   //如果当前背景音乐没有包含到过去的
                            Destroy(audio);          //就删除
                    }
                    catch (Exception e)
                    {
                        Debug.Log(e.Message);
                    }
                }
                //添加新增元素
                var 新增元素 = 使用背景音乐集合.Except(intersectedList);
                foreach (var value in 新增元素)
                {
                    AudioClip loadedRes = Resources.Load("Audio/" + value, typeof(AudioClip)) as AudioClip;
                    var newO = 背景音乐Object.AddComponent<AudioSource>();
                    newO.clip = loadedRes;
                    newO.loop = true;
                    newO.Play();
                }
            }
            SaveManager.data0.背景音乐 = s背景音乐;
            上个背景音乐集 = s背景音乐;
        }
        //立绘7
        if (s立绘 != "" && (s立绘 != 上个立绘集 || recovery))
        {
            上个立绘集 = s立绘;
            SaveManager.data0.立绘 = s立绘;
            
            生成立绘();
        }

        //立绘头像 9
        if (s头像 != "")
        {
            文本框说话内容.rectTransform.offsetMax = new((s头像 == "None") ? -125 : -365, 文本框说话内容.rectTransform.offsetMax.y);

            if (s头像 == "None")
                头像.color = new(1, 1, 1, 0);
            else
                头像.color = new(1, 1, 1, 1);

            头像.texture = Resources.Load("Characters/Feature/" + s头像, typeof(Texture2D)) as Texture2D;

            SaveManager.data0.头像 = s头像;
        }
        else
        {
            if (s人物 != "")
            {
                if (!Resources.Load("Characters/Feature/" + s人物, typeof(Texture2D)).IsUnityNull() && 头像统一)
                {
                    文本框说话内容.rectTransform.offsetMax = new(-365, 文本框说话内容.rectTransform.offsetMax.y);
                    头像.color = new(1, 1, 1, 1);
                    头像.texture = Resources.Load("Characters/Feature/" + s人物, typeof(Texture2D)) as Texture2D;
                }
                else
                {
                    文本框说话内容.rectTransform.offsetMax = new(-125, 文本框说话内容.rectTransform.offsetMax.y);
                    头像.color = new(1, 1, 1, 0);
                }
            }
        }

        //背景与cg 4
        if (s背景 != "" && s背景 != 上个背景)     //是否填写的切换什么背景
        {
            背景.rectTransform.localPosition = new Vector3(0, 0);

            if (!recovery)
                背景缩放比例 = 1.00f;
            else
                背景缩放比例 = SaveManager.data0.背景缩放比例;

            bool loop = s背景.EndsWith("|Loop");
            视频播放器.isLooping = loop;
            if (loop)
                s背景 = s背景.Replace("|Loop", "");

            UnityEngine.Object needLoadBackground = Resources.Load<UnityEngine.Object>("Background/" + s背景);

            //判断是否是背景
            if (needLoadBackground is Texture2D)
            {
                var t2dBg = (Texture2D)needLoadBackground;
                bgSize = new(t2dBg.width, t2dBg.height);

                背景对齐();

                背景.texture = t2dBg;

                if (上个为视频)
                {
                    视频播放器.Stop();
                    视频播放器.clip = null;
                }
            }
            else if (needLoadBackground is VideoClip)
            {
                var vCBg = (VideoClip)needLoadBackground;

                视频播放器.clip = vCBg;
                bgSize = new(vCBg.width, vCBg.height);

                背景对齐();

                视频播放器.Play();

                上个为视频 = true;
            }

            SaveManager.data0.背景 = s背景;
            上个背景 = s背景;
        }
        if (!recovery)
        {
            //选项 10
            choiseManager.DeleteAll();

            if (s选项 != "")
            {
                choiseManager.post(s选项);
            }

            if (sQTE != "")
            {
                qTEManager.gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0, 0);
                qTEManager.post(sQTE);
            }

            //广播 11
            SpecialCommand(s指令);

            //所作所为记入历史
            if (s人物 != "&Scene")
                HistoryRecord.addContext((主名字.text == "") ? "-" : 主名字.text, s对话);
        }
    }

    public void 上一行()
    {
        切换音频.Play();

        if (qTEManager.hasQTE())
            return;

        if (choiseManager.限时框.activeInHierarchy)
            return;

        if (lastParentWords.Contains(" || ") && childRow != 0)
        {
            var 间断 = lastParentWords.Split(" || ");
            childRow--;

            文本框说话内容.text = "";

            if (打字动画开关)
            {
                TextWriter.isActive = false;
                TextWriter.words = 间断[childRow];

                if (中心文本状态)
                    中心文本Text.GetComponent<OnEnableFadeIn>().OnEnable();
                else
                    文本框说话内容.GetComponent<OnEnableFadeIn>().OnEnable();

                文本框说话内容.text = 间断[childRow];
            }
            else
            {
                TextWriter.words = 间断[childRow];
                TextWriter.reRun = true;
            }

            return;
        }

        if (row <= 2)
            return;

        if (!剧情文本.Cells[row, 指令Line].Value.IsUnityNull() && 剧情文本.Cells[row, 指令Line].Value.ToString().Contains("backTo"))   //后退标志
        {
            foreach (string cmd in 剧情文本.Cells[row, 指令Line].Value.ToString().Split("|"))
            {
                if (cmd.StartsWith("backTo"))
                {
                    int backToTag = 搜寻Tag行(cmd.Split(" >> ")[1]);
                    //重置自动
                    AutoRead.passed = 0;

                    SaveManager.data0.progress = backToTag;
                    SaveManager.data0.chapter = Chapter;

                    转场(backToTag, 剧情文本.Cells[backToTag, 转场特效Line].Value);

                    return;
                }
            }
        }

        int oldRow = row;
        int backTo = row - 1;

        if (!剧情文本.Cells[backTo, 人物Line].Value.IsUnityNull() && 剧情文本.Cells[backTo, 人物Line].Value.ToString().Contains("//"))  //无法后退标志
            return;

        if (!剧情文本.Cells[backTo, 指令Line].Value.IsUnityNull() && 剧情文本.Cells[backTo, 指令Line].Value.ToString().Contains("cantBackTo"))  //无法后退标志
            return;

        if (!剧情文本.Cells[row, 指令Line].Value.IsUnityNull() && 剧情文本.Cells[row, 指令Line].Value.ToString().Contains("cantBack"))  //无法后退标志
            return;

        //重置自动
        AutoRead.passed = 0;

        SaveManager.data0.progress = backTo;
        SaveManager.data0.chapter = Chapter;

        立绘动画管理器.disEmploy();
        转场(backTo, 剧情文本.Cells[oldRow, 转场特效Line].Value);
    }
    public void 下一行()
    {
        切换音频.Play();

        //重置自动
        AutoRead.passed = 0;
        //删除qtes
        qTEManager.DeleteAll();
        choiseManager.DeleteAll();
        立绘动画管理器.disEmploy();
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

        转场(row, 剧情文本.Cells[row, 转场特效Line].Value);
    }

    public void SpecialCommand(string OValue)
    {
        if (!OValue.Contains("goto"))
        {
            gotoRow = -1;
        }
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
        {
            锁定背景 = false;
        }
        foreach (string value in OValue.Split("|"))
        {
            string[] 参数 = value.Split(" >> ");
            string 指令头 = 参数[0];
            //普通指令
            if (指令头.Equals("关闭游戏"))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
            }
            if (指令头.Equals("输入"))   //输入 >> value >> 标题 >> goto >> back
            {
                GameObject input = Instantiate(输入);
                input.GetComponent<InputUI>().value = 参数[1];
                input.GetComponent<InputUI>().title.text = 参数[2];
                input.GetComponent<InputUI>().gotoRow = 搜寻Tag行(参数[3]);
                input.GetComponent<InputUI>().backRow = 搜寻Tag行(参数[4]);
                input.transform.SetParent(输入框落点.transform);

                input.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
                input.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);

                input.GetComponent<RectTransform>().localScale = new(1, 1, 1);
            }
            if (指令头.Equals("背景明度"))
            {
                背景目标明度 = float.Parse(参数[1]);
                SaveManager.data0.背景明度 = 背景目标明度;
            }
            if (指令头.Equals("锁定背景"))
            {
                锁定背景 = bool.Parse(参数[1]);
            }
            if (指令头.Equals("BeforeVideo"))
            {
                int 去哪行 = 搜寻Tag行(参数[1]);
                BeforeVideo_row = 去哪行;
                BeforeVideo = true;
            }
            //结束
            if (指令头.Equals("OverTheChapter"))
            {
                StatePlay(false);     //退出;
            }
            //goto    ——下次去到某个raw    [ goto>number ]
            if (指令头.Equals("goto"))
            {
                gotoRow = 搜寻Tag行(参数[1]);
            }
            //goto    ——直接去到某个raw    [ gotoN>number ]
            if (指令头.Equals("gotoN"))
            {
                int 去哪行 = 搜寻Tag行(参数[1]);
                row = 去哪行 - 1;

                下一行();
            }
            //用于Button， 若使用autoChoose，请使用Button   [autoChoose>几秒>去哪]
            if (指令头.Equals("autoChoose"))
            {
                int 去哪行 = 搜寻Tag行(参数[2]);

                ChoiseManager.chooseTimePassed = 0f;
                ChoiseManager.AutoChooseTime = float.Parse(参数[1]);
                ChoiseManager.autoChooseTo = 去哪行;
            }
            else
            {
                ChoiseManager.AutoChooseTime = -1f;
            }
            //等待
            if (指令头.Equals("wait"))
            {
                clickCheckTimer = 0f;
                clickWaitTime = float.Parse(参数[1]);
            }
            if (指令头.Equals("自动延时"))
            {
                autoCheckTimer = 0f;
                autoWaitTime = float.Parse(参数[1]);
            }
            if (指令头.Equals("goChapter"))
            {
                //初始化();    - 没必要

                setupChapter(参数[1]);

                //删除历史
                HistoryRecord.ClearHistory();

                下一行();
            }
            //弹出信息框  (信息框 >> 文本 >> 标题 >> 是)
            if (指令头.Equals("信息框"))
            {
                string text = 参数[1];
                string title = 参数[2];
                string yes = 参数[3];
                MessageBox.Show(TAG文本替换(text), title, DialogResult.Ok);
                MessageBox.okText = yes;

                UiMessage.open();
            }
            //弹出Value信息框  (值信息框 >> Tag >> 文本 >> 标题 >> 是 >> 否)
            if (指令头.Equals("值信息框"))
            {
                string creatV = 参数[1];
                string text = 参数[2];
                string title = 参数[3];
                string yes = 参数[4];
                string no = 参数[5];
                MessageBox.Show(text, title, DialogResult.OkCancel);
                MessageBox.okText = yes;
                MessageBox.cancelText = no;

                //先删除
                foreach (string s in SaveManager.data0.事件标签)
                {
                    if (s.StartsWith(value))
                    {
                        SaveManager.data0.事件标签.Remove(s);
                        break;
                    }
                }

                MessageBox.clickOk = () =>
                {
                    //再添加
                    SaveManager.data0.事件标签.Add(creatV + "=" + "True");
                };
                MessageBox.clickCancel = () =>
                {
                    //再添加
                    SaveManager.data0.事件标签.Add(creatV + "=" + "False");
                };

                UiMessage.open();
            }
            //弹出提示框 noti >> 文本 >> 标题 >> 时间
            if (指令头.Equals("noti"))
            {
                string text = 参数[1];
                string title = 参数[2];
                float time = float.Parse(参数[3]);

                NotiBox.showTime = time;
                NotiBox.Show(text, title);
            }
            if (指令头.Equals("save"))
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
            if (指令头.Equals("saveN"))
            {
                UiMessage.open();
                SaveManager.inti.SaveByPresent();
                NotiBox.Show("已保存当前进度", "存档");
                NotiBox.showTime = 2;
            }
            //random>1>2>3(>.......)
            if (指令头.Equals("random"))
            {
                Random ran = new();
                int i = ran.Next(1, 参数.Length);
                string target = 参数[i];
                row = 搜寻Tag行(target) - 1;
                下一行();
            }
            //摄像机摇动
            if (指令头.Equals("界面抖动"))
            {
                cameraShake.Restart();
            }
            //存档事件标签
            if (指令头.Equals("addTag"))
            {
                if (!SaveManager.data0.事件标签.Contains(参数[1]))
                    SaveManager.data0.事件标签.Add(参数[1]);

                TagManager.addGlobalTag(参数[1]);
            }
            //存档事件标签
            if (指令头.Equals("removeTag"))
            {
                if (SaveManager.data0.事件标签.Contains(参数[1]))
                    SaveManager.data0.事件标签.Remove(参数[1]);
            }
            if (指令头.Equals("removeTags"))
            {
                foreach (string singleTag in 参数[1].Split())
                    if (SaveManager.data0.事件标签.Contains(singleTag))
                        SaveManager.data0.事件标签.Remove(singleTag);
            }
            //全局事件标签
            if (指令头.Equals("addGlobalTag"))
            {
                TagManager.addGlobalTag(参数[1]);
            }
            //AddValueTag
            if (指令头.Equals("addValueTag"))
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
            //ReMoveValueTag
            if (指令头.Equals("removeValueTag"))
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
            //ChangeValueTag
            if (指令头.Equals("changeValueTag"))
            {
                foreach (string s in SaveManager.data0.事件标签)
                {
                    if (s.StartsWith(参数[1] + "="))
                    {
                        SaveManager.data0.事件标签.Remove(s);
                        string tagName = s.Split("=")[0];
                        string oldValue = s.Split("=")[1];
                        //value >> -1 / +1 / 114 / =-114
                        //+n
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
            }
            //Profile  I - 世界观设定
            if (指令头.Equals("解锁档案"))
            {
                //UnLockPCInfo >> #TAG
                TagManager.addGlobalTag("Profile:Unlock:" + 参数[1]);
            }
            //Profile    C - 角色    I - 世界观设定
            if (指令头.Equals("解锁档案_提示"))
            {
                if (!TagManager.collectedTags.collectTags.Contains("Profile:Unlock:" + 参数[1]))
                {
                    NotiBox.Show("已更新", "档案");
                    NotiBox.showTime = 4;
                }
                TagManager.addGlobalTag("Profile:Unlock:" + 参数[1]);
            }
            //暂停背景音乐 or 暂停背景音乐 >> xx >> xx
            if (指令头.Equals("暂停背景音乐"))
            {
                if (参数.Length == 1)
                {
                    foreach (var audio in 背景音乐Object.GetComponents<AudioSource>())
                    {
                        audio.Pause();
                    }
                }
                else
                {
                    foreach (var item in 背景音乐Object.GetComponents<AudioSource>())
                    {
                        if (item.clip != null)
                            if (item.clip.name.Equals(参数[1]))
                            {
                                item.Pause();

                                break;
                            }
                    }
                }
            }
            //继续背景音乐 or 继续背景音乐 >> xx
            if (指令头.Equals("继续背景音乐"))
            {
                if (参数.Length == 1)
                {
                    foreach (var audio in 背景音乐Object.GetComponents<AudioSource>())
                    {
                        audio.UnPause();
                    }
                }
                else
                {
                    foreach (var item in 背景音乐Object.GetComponents<AudioSource>())
                    {
                        if (item.clip != null)
                            if (item.clip.name.Equals(参数[1]))
                            {
                                item.UnPause();

                                break;
                            }
                    }
                }
            }
            if (指令头.Equals("头像统一"))
            {
                头像统一 = bool.Parse(参数[1]);
                SaveManager.data0.头像统一 = 头像统一;
            }
            if (指令头.Equals("背景缩放比例"))
            {
                背景缩放比例 = 参数[1].Equals("默认") ? 1.00f : float.Parse(参数[1]);
                SaveManager.data0.背景缩放比例 = 背景缩放比例;
            }
            if (指令头.Equals("LoadScene"))
            {
                预加载场景 = SceneManager.LoadSceneAsync(参数[1]);
                预加载场景.allowSceneActivation = false;

                场景加载界面.SetActive(true);
            }
            //背景音乐进度 >> 音乐名字 >> xx(float)
            if (指令头.Equals("背景音乐进度"))
            {
                foreach (var item in 背景音乐Object.GetComponents<AudioSource>())
                {
                    if (item.clip != null)
                        if (item.clip.name.Equals(参数[1]))
                        {
                            item.time = float.Parse(参数[2]);
                            item.UnPause();

                            break;
                        }
                }
            }
            //背景音乐循环 >> 音乐名字 >> boolean
            if (指令头.Equals("背景音乐循环"))
            {
                foreach (var item in 背景音乐Object.GetComponents<AudioSource>())
                {
                    if (item.clip != null)
                        if (item.clip.name.Equals(参数[1]))
                        {
                            item.loop = bool.Parse(参数[2]);
                            item.UnPause();

                            break;
                        }
                }
            }
            //立绘动作
            if (指令头.Equals("播放立绘动画"))
            {
                立绘动画管理器.Apply(参数[1]);
            }
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

    public static Dictionary<string, int> SearchForTags()
    {
        Dictionary<string, int> dic = new();
        for (int i = 1; i <= 剧情文本.Dimension.End.Row; i++)
        {
            if (!剧情文本.Cells[i, TAG].Value.IsUnityNull())
            {
                if (!dic.ContainsKey(剧情文本.Cells[i, TAG].Value.ToString()))
                    dic.Add(剧情文本.Cells[i, TAG].Value.ToString(), i);
            }
        }

        return dic;
    }

    bool 中心文本状态;
    float oldAnchoredPositionY = -1f;
    public void 中心文本(bool state)
    {
        if (oldAnchoredPositionY < 0f)
            oldAnchoredPositionY = 文本框.GetComponent<RectTransform>().anchoredPosition.y;

        中心文本状态 = state;
        中心文本Text.text = "";
        //文本框说话内容.alignment = state ? TextAlignmentOptions.Midline : TextAlignmentOptions.MidlineLeft;

        文本框.GetComponent<RectTransform>().anchoredPosition = new(文本框.GetComponent<RectTransform>().anchoredPosition.x, state ? 114514 : oldAnchoredPositionY);

        //文本框.GetComponent<RawImage>().enabled = !state;
    }

    public void 转场(int target, object valueO)         //提醒自己: usingLine指的是用的哪一行的转场
    {
        row = target;
        //6 是否填写了黑场白场的转场特效
        if (!valueO.IsUnityNull())
        {
            string mode = valueO.ToString();
            if (mode == "白场" || mode == "黑场")
            {
                if (mode == "白场")
                {
                    覆盖图片.color = new Color(1, 1, 1, 0);
                }
                else
                {
                    覆盖图片.color = new Color(0, 0, 0, 0);
                }
                遮盖.SetActive(true);
            }
            if (mode == "淡入")
            {
                淡入转场覆盖_实例 = Instantiate(淡入转场覆盖_Obj);
                淡入转场覆盖_实例.transform.SetParent(背景.gameObject.transform);
                淡入转场覆盖_实例.GetComponent<GraphicFadeOutAndDestroy>().time = 0.25f;
                淡入转场覆盖_实例.GetComponent<RectTransform>().localScale = new(1, 1, 1);
                淡入转场覆盖_实例.GetComponent<RectTransform>().localPosition = new(0, 0);
                淡入转场覆盖_实例.GetComponent<RectTransform>().sizeDelta = 背景.rectTransform.sizeDelta;
                淡入转场覆盖_实例.GetComponent<RawImage>().texture = 背景.texture;
                淡入转场覆盖_实例.GetComponent<RawImage>().color = new(1f, 1f, 1f, 背景目标明度);
                ToRow(target);
            }
        }
        else
        {
            ToRow(target);
        }
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

        背景目标明度 = SaveManager.data0.背景明度;
        背景缩放比例 = SaveManager.data0.背景缩放比例;
        头像统一 = SaveManager.data0.头像统一;

        背景.rectTransform.localPosition = new Vector3(0, 0);

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

        //语音 3 
        //删除克隆
        foreach (var audio in 语音Object.GetComponents<AudioSource>())
        {
            Destroy(audio);
        }
    }

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
        if (上个立绘集 != "None")
        {
            var 分隔号 = 上个立绘集.Split("|");
            int 角色数位 = 0;

            float totalWidth = Screen.width;
            float leftBound = -totalWidth / 2f; // -960
            float segmentWidth = totalWidth / 分隔号.Length; // 每一份的宽度

            foreach (var item in 分隔号)
            {
                var 参数 = item.Split(" >> ");

                //创建立绘

                GameObject 新立绘 = new GameObject(角色数位.ToString()); //以使用ID为名字，方便后续调用动作
                RawImage newImage = 新立绘.AddComponent<RawImage>();
                //目前只支持淡入淡出，但是要入场动画的话，就只有先透明了
                newImage.color = new Color(1, 1, 1, 0f);
                newImage.texture = Resources.Load("Characters/" + 参数[0], typeof(Texture2D)) as Texture2D;
                newImage.GetComponent<RectTransform>().sizeDelta = new Vector2(1920, 1080); //和文件一致 

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

    public static int 搜寻Tag行(string target)
    {
        int 去哪行 = 1;

        foreach (KeyValuePair<string, int> kvp in tags)
        {
            if (kvp.Key == target)
            {
                去哪行 = kvp.Value;

                break;
            }
        }

        return 去哪行;
    }

    public static void setupChapter(string chapterID)
    {
        Chapter = chapterID;
        SaveManager.data0.chapter = chapterID;
        string ChapterPath = usingPlatAssestPath + "/Chapter/" + LanguageManager.GetKeyName(chapterID);

        if (Application.isMobilePlatform)
            CopyExcel(chapterID);

        if (!File.Exists(ChapterPath))
        {
            if (Application.isMobilePlatform)
                CopyExcel("story.前言");
            ChapterPath = usingPlatAssestPath + "/Chapter/" + LanguageManager.GetKeyName("story.前言");
        }
        FileInfo fileInfo = new(ChapterPath);

        ExcelPackage excelPackage = new(fileInfo);
        //准备界面
        剧情文本 = excelPackage.Workbook.Worksheets[1];
        tags = SearchForTags();

        row = 1;
    }

    public static void setupChapterFromPath(string path)
    {
        FileInfo fileInfo = new(path);

        ExcelPackage excelPackage = new(fileInfo);
        //准备界面
        剧情文本 = excelPackage.Workbook.Worksheets[1];
        tags = SearchForTags();

        row = 1;
    }

    public static void CopyExcel(string chapterID)
    {
        string sPath = Application.streamingAssetsPath + "/Chapter/" + LanguageManager.GetKeyName(chapterID);
        UnityWebRequest www = UnityWebRequest.Get(sPath);
        www.SendWebRequest();

        while (!www.isDone)
        {

        }

        new FileInfo(Application.persistentDataPath + "/Chapter/" + LanguageManager.GetKeyName(chapterID)).Directory.Create();

        File.WriteAllBytes(Application.persistentDataPath + "/Chapter/" + LanguageManager.GetKeyName(chapterID), www.downloadHandler.data);
    }

    public static string TAG文本替换(string 原文本)
    {
        //设定项目
        原文本 = 原文本.Replace("%Admin%", Environment.UserName);
        原文本 = 原文本.Replace("%Admin_UserDomainName%", Environment.UserName + " (" + Environment.UserDomainName + ")");
        原文本 = 原文本.Replace("%Admin%", Environment.UserName);
        原文本 = 原文本.Replace("%Admin_UserDomainName%", Environment.UserName + " (" + Environment.UserDomainName + ")");

        //TAG    Normol%tag >> SomeThing%Normol
        foreach (string text in 原文本.Split("%"))
        {
            if (text.StartsWith("tag >> "))
            {
                string 欲替换Value = "?";
                foreach (string gottenTag in SaveManager.data0.事件标签)
                {
                    if (gottenTag.StartsWith(text.Replace("tag >> ", "")))
                    {
                        欲替换Value = gottenTag.Split("=")[1];
                        break;
                    }
                }

                原文本 = 原文本.Replace("%" + text + "%", 欲替换Value);
            }
            //other
        }

        return 原文本;
    }

    Vector2 bgSize = new(1920, 1080);
    public void 背景对齐()
    {
        if (背景.texture.IsUnityNull())
            return;

        try
        {
            Vector2 canvasSize = gameObject.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta;
            float screenxyRate = canvasSize.x / canvasSize.y;

            //视频尺寸长宽比
            float texturexyRate = bgSize.x / bgSize.y;

            if (texturexyRate > screenxyRate)
            {
                int newSizeY = Mathf.CeilToInt(canvasSize.y);
                int newSizeX = Mathf.CeilToInt((float)newSizeY / bgSize.y * bgSize.x);
                背景.rectTransform.sizeDelta = new Vector2(newSizeX * 背景缩放比例, newSizeY * 背景缩放比例);
            }
            else
            {
                int newVideoSizeX = Mathf.CeilToInt(canvasSize.x);
                int newVideoSizeY = Mathf.CeilToInt((float)newVideoSizeX / bgSize.x * bgSize.y);
                背景.rectTransform.sizeDelta = new Vector2(newVideoSizeX * 背景缩放比例, newVideoSizeY * 背景缩放比例);
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    void 视频周期委托(VideoPlayer videoPlayer)
    {
        if (BeforeVideo)
        {
            BeforeVideo = false;

            转场(BeforeVideo_row, 剧情文本.Cells[BeforeVideo_row, 转场特效Line].Value);
        }
    }
}