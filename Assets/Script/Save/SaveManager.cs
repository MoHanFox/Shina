using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class SaveManager
{

    public static SaveManager inti = new();
    public static Save data0 = inti.CreateSave();
    public Save CreateSave()
    { 
        Save save = new();
        return save;
    }

    public void CreateSaveByJSON(int number)
    {
        Save save = CreateSave();
        String JsonString = JsonUtility.ToJson(save);

        StreamWriter sw = new(Application.persistentDataPath + "/Saves/Data" + number + ".json");

        sw.Write(JsonString);
        sw.Close();
    }

    public void SaveByPresent()
    {
        Save save = data0;
        String JsonString = JsonUtility.ToJson(save);

        StreamWriter dataC = new(Application.persistentDataPath + "/Saves/Data" + ConfigManager.config.ChosenSave + ".json");

        if (!Application.isMobilePlatform)
            ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/Saves/Data" + ConfigManager.config.ChosenSave + ".png");   //截图
        else
        {
            File.WriteAllBytes(Application.persistentDataPath + "/Saves/Data" + ConfigManager.config.ChosenSave + ".png", ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG());
        }
        dataC.Write(JsonString);
        dataC.Close();
    }
    public void SaveByData(Save save, int number)
    {
        String JsonString = JsonUtility.ToJson(save);

        StreamWriter sw = new(Application.persistentDataPath + "/Saves/Data" + number + ".json");

        if(!Application.isMobilePlatform)
            ScreenCapture.CaptureScreenshot(Application.persistentDataPath + "/Saves/Data" + number + ".png");   //截图
        else{
            File.WriteAllBytes(Application.persistentDataPath + "/Saves/Data" + number + ".png", ScreenCapture.CaptureScreenshotAsTexture().EncodeToPNG());
        }

        sw.Write(JsonString);
        sw.Close();
    }


    public Save LoadByJSON(int number)
    {
        string file =  Application.persistentDataPath + "/Saves/Data" + number + ".json";

        if (File.Exists(file))
        //判断文件是否创建
        {
            StreamReader sr = new(file);
            //从流中读取字符串
            String JsonString = sr.ReadToEnd();
            //ReadToEnd()方法可以读取从流当前位置到结尾的所有字符
            //还有Read()方法，但是只读了一个字符，还有更多方法捏懒得打了
            sr.Close();
            //把流关了
            Save save = JsonUtility.FromJson<Save>(JsonString);
            //该方法属于泛型方法T，需要给出明确的类型定义，所以要写<Save>

            //BASE
            return save;
        }
        else
        {
            Debug.LogError("File Not Found.");
            //创建文件

            SaveByData(CreateSave(), number);
        }

        return CreateSave();
    }
}
