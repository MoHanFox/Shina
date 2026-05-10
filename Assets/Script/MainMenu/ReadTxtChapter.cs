using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReadTxtChapter : MonoBehaviour
{

    public TextAsset txt;

    // Start is called before the first frame update
    void OnEnable()
    {
        string text = "	" + txt.text.Replace("\r\n", "<br>\r\n<br>" + "	");
        gameObject.GetComponent<TMP_InputField>().text = text;
        gameObject.GetComponent<TMP_InputField>().verticalScrollbar.value = 0;
    }

    public void setTextAsset(TextAsset text)
    {
        txt = text;
    }
}
