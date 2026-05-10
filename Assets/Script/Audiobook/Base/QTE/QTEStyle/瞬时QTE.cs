using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = System.Random;

public class 瞬时QTE : MonoBehaviour
{
    // Start is called before the first frame update
    private QTEBaseData data;

    public RectTransform 进度条;
    public RectTransform 判定条;
    public TextMeshProUGUI 按键;
    public TextMeshProUGUI 注释;

    public RectTransform 判定线父类;
    public BoxCollider2D 判定线;
    public BoxCollider2D 碰撞箱;

    float timer;

    private void Start()
    {
        按键.text = data.keyName;
        注释.text = data.describe;

        float 比值 = data.successProdition/data.failTime;
        判定条.localPosition = new(new Random().Next(-(int)(gameObject.GetComponent<RectTransform>().sizeDelta.x)/2, (int)(gameObject.GetComponent<RectTransform>().sizeDelta.x - gameObject.GetComponent<RectTransform>().sizeDelta.x * 比值)/2 - 50), 判定条.localPosition.y);
        判定条.sizeDelta = new(gameObject.GetComponent<RectTransform>().sizeDelta.x * 比值, 判定条.sizeDelta.y);

        碰撞箱.size = 判定条.sizeDelta;

        timer = 0f;
    }
    // Update is called once per frame
    void Update()
    {
        if (gameObject.GetComponent<RectTransformZoomIn>().active)
            return;
        
        timer += Time.deltaTime;
        进度条.localScale = new(1-(timer/data.failTime),1);
        判定线父类.localPosition = new(-(gameObject.GetComponent<RectTransform>().sizeDelta.x) * (timer / data.failTime), 判定线父类.localPosition.y);
        if (timer >= data.failTime)
        {
            GlobalHook.BookToRow(data.failTo);
            Destroy(gameObject);
        }
        else
        {
            if (Input.GetKeyDown(data.key))
            {
                if (判定线.IsTouching(碰撞箱))
                {
                    if (data.successTo != -1)
                    {
                        GlobalHook.BookToRow(data.successTo);
                    }
                    Destroy(gameObject);
                }
                else
                {
                    GlobalHook.BookToRow(data.failTo);
                    Destroy(gameObject);
                }
            }
        }
    }

    public void SetData(QTEBaseData data)
    {
        this.data = data;
    }

    public void Click()
    {
        if (timer >= data.failTime)
        {
            GlobalHook.BookToRow(data.failTo);
            Destroy(gameObject);
        }
        else
        {

            if (判定线.IsTouching(碰撞箱))
            {
                if (data.successTo != -1)
                {
                    GlobalHook.BookToRow(data.successTo);
                }
                Destroy(gameObject);
            }
            else
            {
                Audiobook.row = data.failTo - 1;
                GlobalHook.BookToRow(data.successTo);
                Destroy(gameObject);
            }

        }
    }
}
