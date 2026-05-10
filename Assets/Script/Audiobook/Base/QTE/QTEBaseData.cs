using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTEBaseData
{
    public string keyName;
    public string describe;

    public KeyCode key;
    public KeyCode key2;    //双键QTE

    public int successTo;    //提醒一下自己，这里的值在QteManager已经设置好了
    public int failTo;

    public float holdTime;
    public int hitCount;

    public float successProdition;    //瞬时QTE

    public float failTime;

    public QTEBaseData(string keyName,string describe, KeyCode key,float failTime,float successProdition,int successTo, int failTo)       //瞬时QTE
    {
        this.keyName = keyName;
        this.describe = describe;
        this.key = key;
        this.failTime = failTime;
        this.successProdition = successProdition;
        this.successTo = successTo;
        this.failTo = failTo;
    }

    public QTEBaseData(string keyName, string describe, int successTo)       //单点QTE
    {
        this.keyName = keyName;
        this.describe = describe;
        this.successTo = successTo;
    }

    public QTEBaseData(string keyName, string describe, KeyCode key, float failTime, int successTo, int failTo)       //限时QTE
    {
        this.keyName = keyName;
        this.describe = describe;
        this.key = key;
        this.failTime = failTime;
        this.successTo = successTo;
        this.failTo = failTo;
    }
}
