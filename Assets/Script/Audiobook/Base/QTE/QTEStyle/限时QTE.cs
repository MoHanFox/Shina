using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class 限时QTE : MonoBehaviour
{
    // Start is called before the first frame update
    private QTEBaseData data;

    public RectTransform 进度条;
    public TextMeshProUGUI 按键;
    public TextMeshProUGUI 注释;

    float timer;

    private void Start()
    {
        按键.text = data.keyName;
        注释.text = data.describe;

        timer = 0f;
    }
    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetComponent<RectTransformZoomIn>().active)
            return;
            
        timer += Time.deltaTime;
        进度条.localScale = new(1 - (timer / data.failTime), 1);
        if (timer >= data.failTime)
        {
            GlobalHook.BookToRow(data.failTo);
            Destroy(gameObject);
        }
        else
        {
            if (Input.GetKeyDown(data.key))
            {
                if (data.successTo != -1)
                    GlobalHook.BookToRow(data.successTo);
            }
        }
    }

    public void SetData(QTEBaseData data)
    {
        this.data = data;
    }

    public void Click()
    {
        if (data.successTo != -1)
        {
            GlobalHook.BookToRow(data.successTo);
        }
    }
}
