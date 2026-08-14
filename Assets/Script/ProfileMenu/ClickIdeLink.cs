using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickIdeLink : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text text;
    public static ArrayList ExpandLinks = new();

    void Awake()
    {
        text = gameObject.GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //本组件由 IdeologyRead 在运行时 AddComponent 动态挂载：
        //内容刷新与点击可能同帧发生，Start 尚未执行；或刷新时文本对象被销毁。
        //这里做兜底取引用，取不到（无 TMP_Text / 已销毁）则忽略本次点击。
        if (text == null)
        {
            text = gameObject.GetComponent<TMP_Text>();
            if (text == null)
                return;
        }

        Vector3 pos = new Vector3(eventData.position.x, eventData.position.y, 0);
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, pos, null); //--UI相机
        if (linkIndex > -1)
        {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];

            if (ExpandLinks.Contains(linkInfo.GetLinkID()))
                ExpandLinks.Remove(linkInfo.GetLinkID());
            else
                ExpandLinks.Add(linkInfo.GetLinkID());

            IdeologyRead.needReloadIde = true;
        }
    }
}
