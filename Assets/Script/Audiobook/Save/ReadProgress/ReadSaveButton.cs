using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadMenuButton : MonoBehaviour
{
    public RawImage image;
    public TextMeshProUGUI text;
    public GameObject parent;
    public int 档位;

    //每次打开读档界面都会触发 OnEnable；缩略图走缓存，不再逐次 IO + 新建纹理
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
            MessageBox.clickOk = () =>
            {
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
}
