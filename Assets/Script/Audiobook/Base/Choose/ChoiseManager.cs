using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ChoiseManager : MonoBehaviour
{
    public GameObject choiceParent;
    public GameObject 选项框;
    public GameObject 限时框;

    //关于自动选按钮
    public static float AutoChooseTime = -1f;
    public static float chooseTimePassed = 0f;
    public static int autoChooseTo = 0;

    public void DeleteAll()
    {
        Transform transform;
        for (int i = 0; i < choiceParent.transform.childCount; i++)
        {
            transform = choiceParent.transform.GetChild(i);
            Destroy(transform.gameObject);
        }
    }

    // Update is called once per frame
    public void post(string value)
    {
        string[] strings = value.Split("|");

        TextMeshProUGUI text = 选项框.GetComponentInChildren<TextMeshProUGUI>();
        text.text = strings[0].Split(" >> ")[0];

        for (int i = 0; i < strings.Length; i++)
        {
            GameObject 克隆 = Instantiate(选项框);

            RectTransform rectTransform = 克隆.GetComponent<RectTransform>();

            string 文本内容 = strings[i].Split(" >> ")[0];
            string targetTag = strings[i].Split(" >> ")[1].Split(" << ")[0];

            int 去哪行 = Audiobook.搜寻Tag行(targetTag);

            克隆.GetComponent<BottonsChoicesClick>().choicesData = new(文本内容, 去哪行);

            克隆.transform.SetParent(choiceParent.transform);

            rectTransform.localPosition = new Vector3(524 ,- 96f * i, 0);
            rectTransform.localScale = new Vector3(1, 1, 1);

            if (!strings[i].Split(" >> ")[1].Contains(" << "))
                克隆.GetComponentInChildren<TextMeshProUGUI>().text = 文本内容;
            else
            {
                //选项 >> 行 << 满足TAG << 样式
                string style = (strings[i].Split(" << ").Length==3) ? strings[i].Split(" << ")[2] : "Unlock"; //默认是Unlock样式
                if (style == "Unlock")
                {
                    if (TagManager.满足条件(strings[i].Split(" << ")[1]))
                    {
                        克隆.GetComponentInChildren<TextMeshProUGUI>().text = 文本内容;
                        克隆.transform.GetChild(3).gameObject.SetActive(true);   //UnLock
                    }
                    else
                    {
                        克隆.GetComponentInChildren<TextMeshProUGUI>().text = "";
                        克隆.transform.GetChild(2).gameObject.SetActive(true);   //Lock
                    }
                }
                if (style == "Lock")
                {
                    if (!TagManager.满足条件(strings[i].Split(" << ")[1]))
                    {
                        克隆.GetComponentInChildren<TextMeshProUGUI>().text = "";
                        克隆.transform.GetChild(3).gameObject.SetActive(true);   //UnLock
                    }
                    else
                    {
                        克隆.GetComponentInChildren<TextMeshProUGUI>().text = 文本内容;
                        克隆.transform.GetChild(2).gameObject.SetActive(true);   //Lock
                    }
                }
                if (style == "Hide")
                {
                    if (TagManager.满足条件(strings[i].Split(" << ")[1]))
                    {
                        Destroy(克隆);
                        continue;
                    }
                    else
                    {
                        克隆.GetComponentInChildren<TextMeshProUGUI>().text = 文本内容;
                        克隆.transform.GetChild(3).gameObject.SetActive(true);   //UnLock
                    }
                }
                if (style == "Unhide")
                {
                    if (!TagManager.满足条件(strings[i].Split(" << ")[1]))
                    {
                        Destroy(克隆);
                        continue;
                    }
                    else
                    {
                        克隆.GetComponentInChildren<TextMeshProUGUI>().text = 文本内容;
                        克隆.transform.GetChild(3).gameObject.SetActive(true);   //UnLock
                    }
                }
            }
        }

        //中间处理
        choiceParent.GetComponent<RectTransform>().localPosition = new(0, (96f * strings.Length) / 2 - 60);
        限时框.GetComponent<RectTransform>().localPosition = new(262 + 120, choiceParent.GetComponent<RectTransform>().localPosition.y + 53);

        ButtonsChoiceAnim.FlyIn();
    }

    public bool hasChoices()
    {
        return choiceParent.transform.childCount > 0;
    }
}
