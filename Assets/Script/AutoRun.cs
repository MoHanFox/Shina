using OfficeOpenXml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoRun
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        Application.targetFrameRate = 60;
#if UNITY_EDITOR
        Audiobook.usingPlatAssestPath = Application.streamingAssetsPath;
        LanguageManager.Inti();
        ConfigManager.config = ConfigManager.inti.LoadByJSON();
        SaveManager.data0 = SaveManager.inti.LoadByJSON(ConfigManager.config.ChosenSave);
        TagManager.collectedTags = TagManager.inti.LoadByJSON();

        IdeologyRead.Inti();

        Audiobook.setupChapter(SaveManager.data0.chapter);
#endif

    }
}
