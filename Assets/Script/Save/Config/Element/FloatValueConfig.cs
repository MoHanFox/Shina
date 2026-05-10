using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FloatValue : MonoBehaviour
{

    public Scrollbar scrollbar;
    public TextMeshProUGUI text;
    public 更改项 Value;
    public enum 更改项 {
        主菜单音乐响度,
        背景音乐响度,
        语音响度,
        按钮响度,
        每秒打字间隔,
        自动顿停
    }

    void Awake()
    {
        switch (Value)
        {
            case 更改项.主菜单音乐响度:
                scrollbar.value = ConfigManager.config.主菜单音乐响度;
                text.text = Convert.ToString(Math.Floor(scrollbar.value * 100));
                break;
            case 更改项.背景音乐响度:
                scrollbar.value = ConfigManager.config.背景音乐响度;
                text.text = Convert.ToString(Math.Floor(scrollbar.value * 100));
                break;
            case 更改项.语音响度:
                scrollbar.value = ConfigManager.config.语音响度;
                text.text = Convert.ToString(Math.Floor(scrollbar.value * 100));
                break;
            case 更改项.按钮响度:
                scrollbar.value = ConfigManager.config.按钮响度;
                text.text = Convert.ToString(Math.Floor(scrollbar.value * 100));
                break;
            case 更改项.每秒打字间隔:
                scrollbar.value = ConfigManager.config.每秒打字间隔;
                TextWriter.charsPerSecond = scrollbar.value;
                text.text = scrollbar.value.ToString("#.###") + "s";
                break;
            case 更改项.自动顿停:
                scrollbar.value = ConfigManager.config.自动顿停;
                text.text = Math.Floor(scrollbar.value * 10).ToString("#.#") + "s";
                break;
        }
    }

    public void UpdateValue()
    {
        switch (Value)
        {
            case 更改项.主菜单音乐响度:
                ConfigManager.config.主菜单音乐响度 = scrollbar.value;
                text.text = Convert.ToString(Math.Floor(scrollbar.value * 100));
                break;
            case 更改项.背景音乐响度:
                ConfigManager.config.背景音乐响度 = scrollbar.value;
                text.text = Convert.ToString(Math.Floor(scrollbar.value * 100));
                break;
            case 更改项.语音响度:
                ConfigManager.config.语音响度 = scrollbar.value;
                text.text = Convert.ToString(Math.Floor(scrollbar.value * 100));
                break;
            case 更改项.按钮响度:
                ConfigManager.config.按钮响度 = scrollbar.value;
                text.text = Convert.ToString(Math.Floor(scrollbar.value * 100));
                break;
            case 更改项.每秒打字间隔:
                ConfigManager.config.每秒打字间隔 = scrollbar.value;
                TextWriter.charsPerSecond = scrollbar.value;
                text.text = scrollbar.value.ToString("#.###") + "s";
                break;
            case 更改项.自动顿停:
                ConfigManager.config.自动顿停 = scrollbar.value;
                text.text = Math.Floor(scrollbar.value * 10).ToString("#.#") + "s";
                break;
        }
    }
}
