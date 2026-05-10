using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickIdeLink : MonoBehaviour, IPointerClickHandler
{
    private TMP_Text text;
    public static ArrayList ExpandLinks = new();

    void Start()
    {
        text = gameObject.GetComponent<TMP_Text>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
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
