using OfficeOpenXml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

public class LanguageManager
{
    public static LanguageType languageType = LanguageType.简体中文;

    public static ExcelPackage excelPackage;

    public static void Inti()
    {
        if(Application.isMobilePlatform)
            CopyExcel();

        FileInfo fileInfo = new(Audiobook.usingPlatAssestPath + "/Languages/TranslationKeys.xlsx");

        excelPackage = new(fileInfo);
    }

    public static string GetKeyName(string key)
    {
        ExcelWorksheet sheet;

        sheet = excelPackage.Workbook.Worksheets[1];

        string target = "UNKNOWN";

        for (int i = 1; i <= sheet.Dimension.End.Row; i++)
        {
            if (!sheet.Cells[i, 1].Value.IsUnityNull())
                if (sheet.Cells[i, 1].Value.ToString() == key && !sheet.Cells[i, ConfigManager.config.语言 + 2].Value.IsUnityNull())
                    return sheet.Cells[i, ConfigManager.config.语言 + 2].Value.ToString();
        }

        //对中文作出最后一次尝试
        if(target == "UNKNOWN")
        {
            sheet = excelPackage.Workbook.Worksheets[1];

            for (int i = 1; i <= sheet.Dimension.End.Row; i++)
            {
                if (!sheet.Cells[i, 1].Value.IsUnityNull())
                    if (sheet.Cells[i, 1].Value.ToString() == key && !sheet.Cells[i, 2].Value.IsUnityNull())
                        return sheet.Cells[i, 2].Value.ToString();
            }
        }

        return target;
    }

    public static string GetNameKey(string name)
    {
        ExcelWorksheet sheet;

        sheet = excelPackage.Workbook.Worksheets[1];

        string target = "UNKNOWN";

        for (int i = 1; i <= sheet.Dimension.End.Row; i++)
        {
            if(!sheet.Cells[i, ((int)languageType) + 2].Value.IsUnityNull())
                if (sheet.Cells[i, ((int)languageType) + 2].Value.ToString() == name)
                    return sheet.Cells[i, 1].Value.ToString();
        }

        //对中文作出最后一次尝试
        if (target == "UNKNOWN")
        {
            sheet = excelPackage.Workbook.Worksheets[1];

            for (int i = 1; i <= sheet.Dimension.End.Row; i++)
            {
                if (!sheet.Cells[i, 2].Value.IsUnityNull())
                    if (sheet.Cells[i, 2].Value.ToString() == name)
                        return sheet.Cells[i, 1].Value.ToString();
            }
        }

        return target;
    }

    public static void CopyExcel()
    {
        string sPath = Application.streamingAssetsPath + "/Languages/TranslationKeys.xlsx";
        UnityWebRequest www = UnityWebRequest.Get(sPath);
        www.SendWebRequest();

        while (!www.isDone)
        {

        }

        new FileInfo(Application.persistentDataPath + "/Languages/TranslationKeys.xlsx").Directory.Create();

        File.WriteAllBytes(Application.persistentDataPath + "/Languages/TranslationKeys.xlsx", www.downloadHandler.data);
    }
}
