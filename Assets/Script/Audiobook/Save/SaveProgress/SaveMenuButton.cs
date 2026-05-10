using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class SaveMenuButton : MonoBehaviour
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
            text.text ="Num. " + 档位 + " - <size=24>" + new FileInfo(Application.persistentDataPath + "/Saves/Data" + 档位 + ".png").LastWriteTime + "</size>";
        }
    }

    public void Click()
    {
        if (File.Exists(Application.persistentDataPath + "/Saves/Data" + 档位 + ".png"))
        {
            MessageBox.Show(LanguageManager.GetKeyName("save.text.覆盖存档详细"), LanguageManager.GetKeyName("save.text.覆盖存档"), DialogResult.OkCancel);
            MessageBox.okText = LanguageManager.GetKeyName("ui.text.Yes");
            MessageBox.cancelText = LanguageManager.GetKeyName("ui.text.Cancel");
            MessageBox.clickOk = () =>
            {
                parent.SetActive(false);
                SaveManager.inti.SaveByData(SaveManager.data0, 档位);
            };
            MessageBox.clickCancel = () =>
            {
            };

            UiMessage.open();
        }else{
            parent.SetActive(false);
            SaveManager.inti.SaveByData(SaveManager.data0, 档位);
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
