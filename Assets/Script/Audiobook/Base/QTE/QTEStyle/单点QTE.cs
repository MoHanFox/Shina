using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class 单点QTE : MonoBehaviour
{
    // Start is called before the first frame update
    private QTEBaseData data;

    public TextMeshProUGUI 按键;
    public TextMeshProUGUI 注释;

    private void Start()
    {
        按键.text = data.keyName;
        注释.text = data.describe;
    }
    // Update is called once per frame

    public void SetData(QTEBaseData data)
    {
        this.data = data;
    }

    public void Click()
    {
        if (data.successTo != -1)
        {
            Audiobook.row = data.successTo - 1;
            GlobalHook.BookToRow(data.successTo);
        }
        Destroy(gameObject);
    }
}
