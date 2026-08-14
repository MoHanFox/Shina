using System;
using System.Collections.Generic;
using System.IO;
using OfficeOpenXml;
using UnityEngine;
using UnityEngine.Networking;

public partial class Audiobook
{
    /// <summary>当前打开的 Excel 包：切换章节时释放，避免 Android 上文件句柄泄漏</summary>
    private static ExcelPackage 当前剧情Excel包;

    private static readonly string 用户名 = Environment.UserName;
    private static readonly string 用户名域 = Environment.UserName + " (" + Environment.UserDomainName + ")";

    /// <summary>读取单元格文本（空单元格返回空串，等价原 IsUnityNull 判断）</summary>
    private static string CellString(int targetRow, int col)
    {
        if (剧情文本 == null)
            return "";

        object value = 剧情文本.Cells[targetRow, col].Value;
        return value == null ? "" : value.ToString();
    }

    /// <summary>全表扫描 TAG 列，建立 TAG 名 → 行号 索引</summary>
    public static Dictionary<string, int> SearchForTags()
    {
        Dictionary<string, int> dic = new();
        if (剧情文本 == null)
            return dic;

        int lastRow = 剧情文本.Dimension?.End.Row ?? 0;
        for (int i = 1; i <= lastRow; i++)
        {
            object value = 剧情文本.Cells[i, TAG].Value;
            if (value == null)
                continue;

            string tag = value.ToString();
            if (!dic.ContainsKey(tag))
                dic.Add(tag, i);
        }

        return dic;
    }

    /// <summary>按 TAG 名查行号（O(1) 查表，原实现为 O(n) 线性扫描）</summary>
    public static int 搜寻Tag行(string target)
    {
        return tags != null && tags.TryGetValue(target, out int result) ? result : 1;
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

        加载Excel(ChapterPath);
    }

    public static void setupChapterFromPath(string path)
    {
        加载Excel(path);
    }

    /// <summary>加载并持有 Excel 包；先挂好新表再释放旧包，避免悬空引用</summary>
    private static void 加载Excel(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("章节文件不存在: " + path);
            return;
        }

        ExcelPackage 新包 = new(new FileInfo(path));
        剧情文本 = 新包.Workbook.Worksheets[1];
        tags = SearchForTags();

        ExcelPackage 旧包 = 当前剧情Excel包;
        当前剧情Excel包 = 新包;
        if (旧包 != null)
            旧包.Dispose();

        row = 1;
    }

    /// <summary>应用退出时释放 Excel 文件句柄</summary>
    private static void 释放Excel()
    {
        if (当前剧情Excel包 != null)
        {
            当前剧情Excel包.Dispose();
            当前剧情Excel包 = null;
        }
    }

    /// <summary>
    /// 移动端把章节从 StreamingAssets 拷贝到可写目录。
    /// 已拷贝过则直接跳过（原实现每次切换章节都重新下载）。
    /// 忙等加 30 秒超时与错误检查，避免移动端永久卡死（原实现为无限忙等）。
    /// </summary>
    public static void CopyExcel(string chapterID)
    {
        string fileName = LanguageManager.GetKeyName(chapterID);
        string destPath = Application.persistentDataPath + "/Chapter/" + fileName;

        if (File.Exists(destPath))
            return;

        string sPath = Application.streamingAssetsPath + "/Chapter/" + fileName;
        UnityWebRequest www = UnityWebRequest.Get(sPath);
        www.timeout = 30;
        www.SendWebRequest();

        //保持同步语义（调用方依赖 setupChapter 后立即可读表）
        float deadline = Time.realtimeSinceStartup + 30f;
        while (!www.isDone && Time.realtimeSinceStartup < deadline)
        {
        }

        if (!www.isDone)
        {
            www.Abort();
            Debug.LogError("拷贝章节超时: " + sPath);
            return;
        }
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("拷贝章节失败(" + www.result + "): " + sPath);
            return;
        }

        new FileInfo(destPath).Directory.Create();
        File.WriteAllBytes(destPath, www.downloadHandler.data);
    }

    /// <summary>替换文本中的 %TAG% 占位符（%Admin%、%tag >> xxx% 等）</summary>
    public static string TAG文本替换(string 原文本)
    {
        if (string.IsNullOrEmpty(原文本) || 原文本.IndexOf('%') < 0)
            return 原文本;

        //设定项目
        原文本 = 原文本.Replace("%Admin%", 用户名).Replace("%Admin_UserDomainName%", 用户名域);

        //TAG    Normol%tag >> SomeThing%Normol
        foreach (string text in 原文本.Split('%'))
        {
            if (!text.StartsWith("tag >> "))
                continue;

            string tagKey = text.Substring(7);   //"tag >> " 的长度
            string 欲替换Value = "?";
            foreach (string gottenTag in SaveManager.data0.事件标签)
            {
                if (gottenTag.StartsWith(tagKey))
                {
                    欲替换Value = gottenTag.Split('=')[1];
                    break;
                }
            }

            原文本 = 原文本.Replace("%" + text + "%", 欲替换Value);
        }

        return 原文本;
    }
}
