using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public class TagManager
{

    public static TagManager inti = new();
    public static CollectedTags collectedTags = inti.CreateCollectedTags();

    public CollectedTags CreateCollectedTags()
    {
        CollectedTags CollectedTags = new();
        return CollectedTags;
    }

    // #tag [ ] value
    public static bool 满足条件(string values)
    {
        if (values.Contains(" or "))
        {
            foreach (string value in values.Split(" or "))
            {
                if (满足单个(value))
                    return true;
            }
        }
        else if (values.Contains(" and "))
        {
            int and满足 = 0;
            foreach (string value in values.Split(" and "))
            {
                if (满足单个(value))
                {
                    and满足 += 1;
                    continue;
                }
                else
                    return false;
            }

            if (and满足 == values.Split(" and ").Length)
                return true;
        }
        else
            return 满足单个(values);

        return false;
    }

    public static bool 满足单个(string value)
    {
        if (value.Contains(" > "))
        {
            double compA = -1.0D;
            foreach (string tag_got in SaveManager.data0.事件标签)
            {
                if (tag_got.StartsWith(value.Split(" > ")[0]))
                    compA = double.Parse(tag_got.Split("=")[1]);
            }

            double compB = double.Parse(value.Split(" > ")[1]);

            if (compA > compB)
                return true;
        }
        if (value.Contains(" >= "))
        {
            double compA = -1.0D;
            foreach (string tag_got in SaveManager.data0.事件标签)
            {
                if (tag_got.StartsWith(value.Split(" >= ")[0]))
                    compA = double.Parse(tag_got.Split("=")[1]);
            }

            double compB = double.Parse(value.Split(" >= ")[1]);
            if (compA >= compB)
                return true;
        }
        if (value.Contains(" < "))
        {
            double compA = -1.0D;
            foreach (string tag_got in SaveManager.data0.事件标签)
            {
                if (tag_got.StartsWith(value.Split(" < ")[0]))
                    compA = double.Parse(tag_got.Split("=")[1]);
            }
            double compB = double.Parse(value.Split(" < ")[1]);
            if (compA < compB)
                return true;
        }
        if (value.Contains(" <= "))
        {
            double compA = -1.0D;
            foreach (string tag_got in SaveManager.data0.事件标签)
            {
                if (tag_got.StartsWith(value.Split(" <= ")[0]))
                    compA = double.Parse(tag_got.Split("=")[1]);
            }
            double compB = double.Parse(value.Split(" <= ")[1]);
            if (compA <= compB)
                return true;
        }
        if (value.Contains(" == "))
        {
            string compA = "NULL";
            foreach (string tag_got in SaveManager.data0.事件标签)
            {
                if (tag_got.StartsWith(value.Split(" == ")[0]))
                    compA = tag_got.Split("=")[1];
            }

            if (compA == value.Split(" == ")[1])
                return true;
        }
        if (value.Contains(" != "))
        {
            string compA = "NULL";
            foreach (string tag_got in SaveManager.data0.事件标签)
            {
                if (tag_got.StartsWith(value.Split(" != ")[0]))
                    compA = tag_got.Split("=")[1];
            }

            if (compA != value.Split(" != ")[1])
                return true;
        }
        if (value.Contains(" has"))
        {
            string compA = value.Replace(" has", "");
            if (SaveManager.data0.事件标签.Contains(compA))
                return true;
            foreach (string tags in SaveManager.data0.事件标签)
            {
                if (tags.StartsWith(compA + "="))
                {
                    return true;
                }
            }
        }
        if (value.Contains(" !has"))
        {
            string compA = value.Replace(" !has", "");
            if (!SaveManager.data0.事件标签.Contains(compA))
                return true;
            foreach (string tags in SaveManager.data0.事件标签)
            {
                if (tags.StartsWith(compA + "="))
                {
                    return false;
                }
            }
        }

        return false;
    }

    public static void addGlobalTag(string value)
    {
        if (!collectedTags.collectTags.Contains(value))
            collectedTags.collectTags.Add(value);
        inti.SaveByPresent();
    }

    public void CreatSaveByJSON()
    {
        CollectedTags CollectedTags = CreateCollectedTags();
        String JsonString = JsonUtility.ToJson(CollectedTags);

        StreamWriter sw = new(Application.persistentDataPath + "/collectedTags.json");

        sw.Write(JsonString);
        sw.Close();
    }

    public void SaveByPresent()
    {
        CollectedTags CollectedTags = collectedTags;
        String JsonString = JsonUtility.ToJson(CollectedTags);

        StreamWriter sw = new(Application.persistentDataPath + "/collectedTags.json");

        sw.Write(JsonString);
        sw.Close();
    }

    public CollectedTags LoadByJSON()
    {
        string file = Application.persistentDataPath + "/collectedTags.json";

        if (File.Exists(file))
        {
            StreamReader sr = new(file);
            String JsonString = sr.ReadToEnd();
            sr.Close();
            CollectedTags CollectedTags = JsonUtility.FromJson<CollectedTags>(JsonString);
            //BASE
            return CollectedTags;
        }
        else
        {
            Debug.LogError("File Not Found.");
            //同时创建文件
            CreatSaveByJSON();
        }

        return CreateCollectedTags();
    }
}
