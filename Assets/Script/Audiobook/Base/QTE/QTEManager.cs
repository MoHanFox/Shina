using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class QTEManager : MonoBehaviour
{
    public GameObject HitQTE;
    public GameObject HoldQTE;
    public GameObject 双键QTE;
    public GameObject 单点QTE;
    public GameObject 瞬时QTE;
    public GameObject 限时QTE;

    public void DeleteAll()
    {
        Transform transform;
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            transform = gameObject.transform.GetChild(i);
            Destroy(transform.gameObject);
        }
    }

    public void post(string value)
    {
        foreach (string qteS in value.Split("|"))
        {
            string keyName = qteS.Split(" >> ")[1];
            string describe = qteS.Split(" >> ")[2];
            KeyCode code = KeyCode.None;

            //基础 - 按键名字 按键code - QTE描述
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (keyCode.ToString() == qteS.Split(" >> ")[1])
                {
                    keyName = keyCode.ToString();
                    code = keyCode;
                }
            }

            if (qteS.StartsWith("瞬时"))   //int key,float string describe failTime,int successProdition,int successTo, int failTo    [x,y,s]
            {
                float failTime = float.Parse(qteS.Split(" >> ")[3]);
                float successProdition = float.Parse(qteS.Split(" >> ")[4]);

                string successToTAG = qteS.Split(" >> ")[5];
                string failToTAG = qteS.Split(" >> ")[6];

                int successTo = Audiobook.搜寻Tag行(successToTAG);
                int failTo = Audiobook.搜寻Tag行(failToTAG); ;

                int x = int.Parse(qteS.Split(" >> ")[7]);
                int y = int.Parse(qteS.Split(" >> ")[8]);
                float scale = qteS.Split(" >> ").Length>=10?float.Parse(qteS.Split(" >> ")[9]):1f;

                GameObject qteButton = Instantiate(瞬时QTE);
                qteButton.transform.SetParent(gameObject.transform);
                qteButton.GetComponent<RectTransform>().localPosition = new(x, y, 0);
                qteButton.GetComponent<RectTransformZoomIn>().toAlpha = scale;    //因为这个的跳出方式不同
                qteButton.GetComponent<RectTransformZoomIn>().Rest();

                qteButton.GetComponent<瞬时QTE>().SetData(new(keyName, describe, code, failTime, successProdition, successTo, failTo));
            }
            if (qteS.StartsWith("单点"))   //string keyName, string describe, int successTo [x,y]
            {
                string successToTAG = qteS.Split(" >> ")[3];
                int x = int.Parse(qteS.Split(" >> ")[4]);
                int y = int.Parse(qteS.Split(" >> ")[5]);
                float scale = float.Parse(qteS.Split(" >> ").Length>=7?qteS.Split(" >> ")[6].Split(" << ")[0]:"1");   //相对于TAG,这是参数最后

                if (qteS.Split(" << ").Length != 1 && !TagManager.满足条件(qteS.Split(" << ")[1]))
                    continue;

                int successTo = Audiobook.搜寻Tag行(successToTAG); ;

                GameObject qteButton = Instantiate(单点QTE);
                qteButton.transform.SetParent(gameObject.transform);
                qteButton.GetComponent<RectTransform>().localPosition = new(x, y, 0);
                qteButton.GetComponent<RectTransform>().localScale = new(scale, scale, scale);

                qteButton.GetComponent<单点QTE>().SetData(new(keyName, describe, successTo));
            }
            if (qteS.StartsWith("限时"))   //int key,float string describe failTime,int successTo, int failTo    [x,y,s]
            {
                float failTime = float.Parse(qteS.Split(" >> ")[3]);

                string successToTAG = qteS.Split(" >> ")[4];
                string failToTAG = qteS.Split(" >> ")[5];

                int successTo = Audiobook.搜寻Tag行(successToTAG);
                int failTo = Audiobook.搜寻Tag行(failToTAG); ;

                int x = int.Parse(qteS.Split(" >> ")[6]);
                int y = int.Parse(qteS.Split(" >> ")[7]);
                float scale = qteS.Split(" >> ").Length >= 9 ? float.Parse(qteS.Split(" >> ")[8]) : 1f;

                GameObject qteButton = Instantiate(限时QTE);
                qteButton.transform.SetParent(gameObject.transform);
                qteButton.GetComponent<RectTransform>().localPosition = new(x, y, 0);
                qteButton.GetComponent<RectTransformZoomIn>().toAlpha = scale;    //因为这个的跳出方式不同
                qteButton.GetComponent<RectTransformZoomIn>().Rest();

                qteButton.GetComponent<限时QTE>().SetData(new(keyName, describe, code, failTime, successTo, failTo));
            }
        }
    }

    public bool hasQTE()
    {
        return gameObject.transform.childCount > 0;
    }
}
