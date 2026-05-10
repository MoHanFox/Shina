using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using OfficeOpenXml;
using UnityEngine.Networking;
using UnityEngine.Android;

public class FirstStarting : MonoBehaviour
{
    public TextMeshProUGUI LOGOText;
    public TextMeshProUGUI LoadText;
    public RawImage Logo;

    // Start is called before the first frame update
    void Start()
    {
        LOGOText.alpha = 0f;

        gameObject.GetComponent<AudioSource>().Play();
    }


    bool enterAnim = false;
    void FixedUpdate()
    {
        if (!enterAnim)
        {
            //LOGO淡入动画
            if (LOGOText.alpha < 1f)
            {
                LOGOText.alpha = LoadText.alpha = LOGOText.alpha + 0.01f;
                Logo.color = new Color(1,1,1, LOGOText.alpha);
            }
            else
            {
                LOGOText.alpha = 1f;
                canLoadRes = true;          //动画完成时允许加载材质
            }
        }
        else
        {
            //淡出动画
            if (LOGOText.alpha > 0f)
            {
                LOGOText.alpha = LOGOText.alpha - 0.01f;
                Logo.color = new Color(Logo.color.a, Logo.color.g, Logo.color.b, LOGOText.alpha);
            }
            else
            {
                canChangeScene = true;    //在动画完成时切换界面
            }
        }
    }


    private bool canLoadRes = false;
    private bool loadingRes = false;


    private bool canChangeScene = false;
    private bool changingScene = false;

    // Update is called once per frame
    void Update()
    {
        if(canLoadRes && !loadingRes)
        {
            loadingRes = true;
            loadRes();
        }

        if (canChangeScene && !changingScene)
        {
            changingScene = true;
            changeScene();
        }
    }

    private AsyncOperation choiceMenuAsync;
    void loadRes()
    {
        Application.targetFrameRate = 60;

        if (Application.isMobilePlatform){
            Audiobook.usingPlatAssestPath = Application.persistentDataPath;
        }else{
            Audiobook.usingPlatAssestPath = Application.streamingAssetsPath;
        }

        LoadText.text = "LanguageManager.Inti();";
        LanguageManager.Inti();

        LoadText.text = "正在检查配置项";
        if (!File.Exists(Application.persistentDataPath + "/config.json"))
        {
            LoadText.text = "正在创建配置项";
            ConfigManager.config = ConfigManager.inti.CreateConfig();
        }
        else
        {
            ConfigManager.config = ConfigManager.inti.LoadByJSON();
            LanguageManager.languageType = (LanguageType) Enum.GetValues(typeof(LanguageType)).GetValue(ConfigManager.config.语言);
            Debug.Log("Using: " + LanguageManager.languageType);
        }

        if (!File.Exists(Application.persistentDataPath + "/collectedTags.json"))
        {
            LoadText.text = "正在创建Tag项";
            TagManager.collectedTags = TagManager.inti.CreateCollectedTags();
        }
        else
        {
            LoadText.text = "正在读取Tag项";
            TagManager.collectedTags = TagManager.inti.LoadByJSON();
        }

        LoadText.text = "正在检查存档目录";
        if (!Directory.Exists(Application.persistentDataPath + "/Saves"))
            Directory.CreateDirectory(Application.persistentDataPath + "/Saves");

        if (!File.Exists(Application.persistentDataPath + "/Saves/Data1.json"))
        {
            LoadText.text = "正在创建1号存档";
            SaveManager.inti.CreateSaveByJSON(1);
        }

        LoadText.text = "正在读取0号存档";
        SaveManager.data0 = SaveManager.inti.LoadByJSON(ConfigManager.config.ChosenSave);

        LoadText.text = " IdeologyRead.Inti() & CharacterInfoRead.Inti()";
        IdeologyRead.Inti();

        LoadText.text = "正在加载视觉小说: " + Audiobook.usingPlatAssestPath;

        string ChapterPath = Audiobook.usingPlatAssestPath + "/Chapter/" + LanguageManager.GetKeyName(SaveManager.data0.chapter);
        if (!File.Exists(ChapterPath))
        {
            LoadText.text = "未找到存档中小说文本";
            Audiobook.setupChapter("story.前言");
            LoadText.text = "回滚完成";
        }else{
            Audiobook.setupChapter(SaveManager.data0.chapter);
        }

        choiceMenuAsync = SceneManager.LoadSceneAsync("Audiobook");
        //禁止加载完成后自动切换场景
        choiceMenuAsync.allowSceneActivation = false;

        //这里是加载完成后隐藏LoadText
        LoadText.alpha = 0f;

        //这里是加载完成后显示任意键进入
        enterAnim = true;
    }

    void changeScene()
    {
        choiceMenuAsync.allowSceneActivation = true;
    }
}
