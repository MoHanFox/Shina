using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class StoryProgress : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshProUGUI 文本进度;

    public TextMeshProUGUI 章节名称;
    public TextMeshProUGUI 章节名称_Shadow;

    public static bool rest = false;

    // 在主菜单才会刷新一下
    void OnEnable()
    {
        UpdateData();
    }

    private void Update()
    {
        if (rest == false)
            return;

        rest = false;

        UpdateData();
    }
    
    void UpdateData(){
        if (!SaveManager.data0.chapter.Equals("story.死亡"))
            文本进度.text = LanguageManager.GetKeyName("ui.mainmenu.button.进程") + ": " + (int)((float)(SaveManager.data0.progress/(float)Audiobook.剧情文本.Dimension.End.Row) * 100) + "%";
        else
            文本进度.text = LanguageManager.GetKeyName("story.死亡-副标题");

        章节名称.text = 章节名称_Shadow.text = LanguageManager.GetKeyName(SaveManager.data0.chapter + "-Name");
    }
}
