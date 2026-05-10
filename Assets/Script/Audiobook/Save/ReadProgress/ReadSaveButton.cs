using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ReadMenuButton : MonoBehaviour
{
    public RawImage image;
    public TextMeshProUGUI text;
    public GameObject parent;
    public int 档位;

    // Start is called before the first frame update
    void OnEnable()
    {
        if (File.Exists(Application.persistentDataPath + "/Saves/Data" + 档位 + ".png"))
        {
            StartCoroutine(UnityWebRequestGetData(Application.persistentDataPath + "/Saves/Data" + 档位 + ".png", (txt) => { image.texture = txt; }));
            text.text = "Num. " + 档位 + " - <size=24>" + new FileInfo(Application.persistentDataPath + "/Saves/Data" + 档位 + ".png").LastWriteTime + "</size>";
        }
    }

    public void Click()
    {
        if (File.Exists(Application.persistentDataPath + "/Saves/Data" + 档位 + ".png"))
        {
            SaveManager.data0 = SaveManager.inti.LoadByJSON(档位);

            parent.SetActive(false);
            GlobalHook.UpdateAuidoBookO();

            if (!Audiobook.startPlaying) 
            {
                MessageBox.Show(LanguageManager.GetKeyName("save.text.切换存档提示") + " " + 档位, LanguageManager.GetKeyName("ui.audiobook.存档标签"), DialogResult.Ok);
                MessageBox.okText = LanguageManager.GetKeyName("ui.text.Know");
                StoryProgress.rest = true;
                UiMessage.open();
            }     

            ConfigManager.config.ChosenSave = 档位;
            ConfigManager.inti.SaveByPresent();
        }
        else
        {
            MessageBox.Show(LanguageManager.GetKeyName("save.text.创建存档提示"), LanguageManager.GetKeyName("save.text.创建存档提示标题"), DialogResult.OkCancel);
            MessageBox.okText = LanguageManager.GetKeyName("ui.text.Yes");
            MessageBox.cancelText = LanguageManager.GetKeyName("ui.text.Cancel");
            MessageBox.clickOk = () => {
                SaveManager.inti.CreateSaveByJSON(档位);
                SaveManager.data0 = SaveManager.inti.LoadByJSON(档位);
                parent.SetActive(false);

                ConfigManager.config.ChosenSave = 档位;
                ConfigManager.inti.SaveByPresent();

                GlobalHook.UpdateAuidoBookO();

                if (!Audiobook.startPlaying)
                    Audiobook.startPlaying = true; 

                SaveManager.inti.SaveByPresent();
            };
            MessageBox.clickCancel = () =>
            {
            };

            UiMessage.open();
        }
    }

    IEnumerator UnityWebRequestGetData(string _url, Action<Texture2D> OnTextureLoad)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture("file://" + _url))
        {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ProtocolError || uwr.result == UnityWebRequest.Result.ConnectionError)
                Debug.Log(uwr.error);
            else if
            (uwr.isDone) OnTextureLoad?.Invoke(DownloadHandlerTexture.GetContent(uwr));
        }

        Resources.UnloadUnusedAssets();
    }
}
