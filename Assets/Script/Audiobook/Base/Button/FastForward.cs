using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FastForward : MonoBehaviour
{
    // Start is called before the first frame update
    bool Ffing = false;
    public GameObject 选项框;
    public Audiobook audiobook;
    public TextMeshProUGUI 文本;

    string FF_text;
    string FFing_text;

    void Start()
    {
        Ffing = false;

        FFing_text = LanguageManager.GetKeyName("ui.audiobook.快进中标签");
        FF_text = LanguageManager.GetKeyName("ui.audiobook.快进标签");
    }

    // Update is called once per frame
    void Update()
    {
        if (audiobook.场景加载界面.activeInHierarchy == true && Ffing)
            state();
            
        if (Ffing)
        {
            if (Audiobook.startPlaying)
            {
                Audiobook.MouseClickedInContent = true;
            }
            else
            {
                文本.text = FF_text;
                Ffing = false;
            }
        }
        else
        {
            文本.text = FF_text;
        }
    }

    public void state()
    {
        Ffing = !Ffing;

        if (Ffing)
        {
            文本.text =FFing_text;
        }
        else
        {
            文本.text = FF_text;
        }
    }
}
