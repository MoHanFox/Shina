using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveMenuButton : MonoBehaviour
{
    public RawImage image;
    public TextMeshProUGUI text;
    public GameObject parent;
    public int 档位;

    //每次打开存档界面都会触发 OnEnable；缩略图走缓存，不再逐次 IO + 新建纹理
    void OnEnable()
    {
        string path = Application.persistentDataPath + "/Saves/Data" + 档位 + ".png";
        if (File.Exists(path))
        {
            image.texture = SaveThumbnailCache.Get(档位);
            text.text = "Num. " + 档位 + " - <size=24>" + new FileInfo(path).LastWriteTime + "</size>";
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
        }
        else
        {
            parent.SetActive(false);
            SaveManager.inti.SaveByData(SaveManager.data0, 档位);
        }
    }
}
