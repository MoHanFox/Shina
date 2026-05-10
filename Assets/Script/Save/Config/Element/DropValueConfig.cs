using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropValueConfig : MonoBehaviour
{

    public TMP_Dropdown dropdown;
    public 更改项 Value;
    public enum 更改项 {
        分辨率,
        语言
    }

    void Awake()
    {
        switch (Value)
        {
            case 更改项.分辨率:
                dropdown.value = ConfigManager.config.分辨率;
                break;
            case 更改项.语言:
                dropdown.value = ConfigManager.config.语言;
                LanguageManager.languageType = (LanguageType)dropdown.value;
                break;
        }
    }

    public void UpdateValue()
    {
        switch (Value)
        {
            case 更改项.分辨率:
                ConfigManager.config.分辨率 = dropdown.value;

                if(Application.isMobilePlatform)
                    break;

                switch (dropdown.value)
                {
                    case 0:     //全屏
                        Resolution[] resolutions = Screen.resolutions;//获取设置当前屏幕分辩率
                        Screen.SetResolution(resolutions[resolutions.Length - 1].width, resolutions[resolutions.Length - 1].height, true);//设置当前分辨率
                        Screen.fullScreen = true;  //设置成全屏,
                        break;
                    case 1:     //1280×720
                        Screen.SetResolution(1280, 720, false);
                        break;
                }
                break;
            case 更改项.语言:
                ConfigManager.config.语言 = dropdown.value;
                LanguageManager.languageType = (LanguageType) dropdown.value;
                break;
        }
    }
}
