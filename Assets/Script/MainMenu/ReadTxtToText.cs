using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReadTxtToText : MonoBehaviour
{

    public TextAsset txt;

    // Start is called before the first frame update
    void Start()
    {
        string text = txt.text.Replace("%Version%", Application.version).Replace("%Unity_Version%", Application.unityVersion);
        gameObject.GetComponent<TMP_InputField>().text = text;
    }

    public void setTextAsset(TextAsset text)
    {
        txt = text;
    }
}
