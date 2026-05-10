using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class ConfigManager
{

    public static ConfigManager inti = new();
    public static Config config = inti.CreateConfig();

    public Config CreateConfig()
    {
        Config config = new();
        return config;
    }

    public void CreatSaveByJSON()
    {
        Config config = CreateConfig();
        String JsonString = JsonUtility.ToJson(config);

        StreamWriter sw = new(Application.persistentDataPath + "/config.json");

        sw.Write(JsonString);
        sw.Close();
    }

    public void SaveByPresent()
    {
        Config config = ConfigManager.config;
        String JsonString = JsonUtility.ToJson(config);

        StreamWriter sw = new(Application.persistentDataPath + "/config.json");

        sw.Write(JsonString);
        sw.Close();
    }

    public Config LoadByJSON()
    {
        string file = Application.persistentDataPath + "/config.json";

        if (File.Exists(file))
        {
            StreamReader sr = new(file);
            String JsonString = sr.ReadToEnd();
            sr.Close();
            Config config = JsonUtility.FromJson<Config>(JsonString);
            //BASE
            return config;
        }
        else
        {
            Debug.LogError("File Not Found.");
            //同时创建文件
            CreatSaveByJSON();
        }

        return CreateConfig();
    }
}
