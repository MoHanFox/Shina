using System;
using System.Collections.Generic;
using UnityEngine;

public class QTEManager : MonoBehaviour
{
    public GameObject HitQTE;
    public GameObject HoldQTE;
    public GameObject 双键QTE;
    public GameObject 单点QTE;
    public GameObject 瞬时QTE;
    public GameObject 限时QTE;

    //KeyCode 名称静态查表：替代原实现每次 post 都遍历整个 KeyCode 枚举
    //（Enum.GetValues 每次都有数组分配，且一次要遍历约 500 个值）
    private static readonly Dictionary<string, KeyCode> keyCodeLookup = BuildKeyCodeLookup();

    private static Dictionary<string, KeyCode> BuildKeyCodeLookup()
    {
        Dictionary<string, KeyCode> dic = new();
        foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            dic[code.ToString()] = code;
        return dic;
    }

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
        foreach (string qteS in value.Split('|'))
        {
            string[] 参数 = qteS.Split(" >> ");   //原实现每个字段重复 Split 5~8 次
            string keyName = 参数[1];
            string describe = 参数[2];
            KeyCode code = keyCodeLookup.TryGetValue(keyName, out KeyCode found) ? found : KeyCode.None;

            //瞬时QTE：按键名 描述 失败时间 成功条件 成功TAG 失败TAG x y s
            if (qteS.StartsWith("瞬时"))
            {
                float failTime = float.Parse(参数[3]);
                float successProdition = float.Parse(参数[4]);

                int successTo = Audiobook.搜寻Tag行(参数[5]);
                int failTo = Audiobook.搜寻Tag行(参数[6]);

                int x = int.Parse(参数[7]);
                int y = int.Parse(参数[8]);
                float scale = 参数.Length >= 10 ? float.Parse(参数[9]) : 1f;

                GameObject qteButton = Instantiate(瞬时QTE);
                qteButton.transform.SetParent(gameObject.transform);
                qteButton.GetComponent<RectTransform>().localPosition = new(x, y, 0);
                qteButton.GetComponent<RectTransformZoomIn>().toAlpha = scale;    //因为这个的跳出方式不同
                qteButton.GetComponent<RectTransformZoomIn>().Rest();

                qteButton.GetComponent<瞬时QTE>().SetData(new(keyName, describe, code, failTime, successProdition, successTo, failTo));
            }

            //单点QTE：按键名 描述 成功TAG x y s << TAG条件
            if (qteS.StartsWith("单点"))
            {
                if (qteS.Contains(" << ") && !TagManager.满足条件(qteS.Split(" << ")[1]))
                    continue;

                string scaleText = 参数.Length >= 7 ? 参数[6].Split(" << ")[0] : "1";   //相对于TAG,这是参数最后
                float scale = float.Parse(scaleText);

                int successTo = Audiobook.搜寻Tag行(参数[3]);

                GameObject qteButton = Instantiate(单点QTE);
                qteButton.transform.SetParent(gameObject.transform);
                qteButton.GetComponent<RectTransform>().localPosition = new(int.Parse(参数[4]), int.Parse(参数[5]), 0);
                qteButton.GetComponent<RectTransform>().localScale = new(scale, scale, scale);

                qteButton.GetComponent<单点QTE>().SetData(new(keyName, describe, successTo));
            }

            //限时QTE：按键名 描述 失败时间 成功TAG 失败TAG x y s
            if (qteS.StartsWith("限时"))
            {
                float failTime = float.Parse(参数[3]);

                int successTo = Audiobook.搜寻Tag行(参数[4]);
                int failTo = Audiobook.搜寻Tag行(参数[5]);

                int x = int.Parse(参数[6]);
                int y = int.Parse(参数[7]);
                float scale = 参数.Length >= 9 ? float.Parse(参数[8]) : 1f;

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
