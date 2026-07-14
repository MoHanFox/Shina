using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AutoRead : MonoBehaviour
{
    public Audiobook audiobook;
    public GameObject 进度;
    public TextMeshProUGUI 文本;

    //关于自动
    public static float passed = 0f;
    bool autoing = false;

    string Aing_text;
    string A_text;

    // Start is called before the first frame update
    void Start()
    {
        autoing = false;
        进度.GetComponent<RectTransform>().localScale = new Vector3(0, 1, 1);

        Aing_text = LanguageManager.GetKeyName("ui.audiobook.自动中标签");
        A_text = LanguageManager.GetKeyName("ui.audiobook.自动标签");
    }

    // Update is called once per frame
    void Update()
    {
        if (autoing && !audiobook.不可点击切换() && audiobook.autoCheckTimer >= audiobook.autoWaitTime && !TextWriter.isActive && 判断视频性质() & !CharacterAnimationManger.playing)
        {
            passed += Time.deltaTime;

            if (passed >= ConfigManager.config.自动顿停 * 10)
            {
                passed = 0f;
                Audiobook.MouseClickedInContent = true;
            }

            float 比值 = passed / (ConfigManager.config.自动顿停 * 10);

            进度.GetComponent<RectTransform>().localScale = new Vector3(比值, 1, 1);
        }
        else
        {
            if(passed > 0)
                passed = 0;
        }
    }

    public void state()
    {
        autoing = !autoing;

        if (autoing)
        {
            文本.text = Aing_text;
            进度.GetComponent<RectTransform>().localScale = new Vector3(0, 1, 1);
            passed = 0;
        }
        else
        {
            文本.text = A_text;
            进度.GetComponent<RectTransform>().localScale = new Vector3(0, 1, 1);
            passed = 0;
        }
    }

    bool 判断视频性质()
    {
        if (audiobook.视频播放器.isLooping)
            return true;
        else
            return !audiobook.视频播放器.isPlaying;
    }
}
