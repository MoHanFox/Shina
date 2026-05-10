using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class TextDisplayByTag : MonoBehaviour
{
    public string 未走过文本 = "？？？";
    public string 显示文本 = "";
    public string checkTag = "";
    TextMeshProUGUI text = null;
    public float 不满足透明度 = 0.5f;

    public enum From
    {
        Collected,
        Saving
    }

    public From from;

    // Start is called before the first frame update
    private void OnEnable()
    {
        if (text == null)
        {
            text = gameObject.GetComponent<TextMeshProUGUI>();
            text.alpha = 0.5f;
        }

        bool 满足条件 = false;
        bool 是否走过 = false;
        foreach (string tags in TagManager.collectedTags.collectTags)
        {
            if (tags.StartsWith(checkTag.Split(" ")[0]))
            {
                if (from == From.Collected)
                    满足条件 = true;
                是否走过 = true;
                break;
            }
        }
        if (from == From.Saving)
        {
            满足条件 = TagManager.满足条件(checkTag);
        }

        text.alpha = 满足条件 ? 1f : 不满足透明度;
        text.text = 是否走过 ? 显示文本 : 未走过文本;
        text.text = Audiobook.TAG文本替换(text.text);
    }
}
