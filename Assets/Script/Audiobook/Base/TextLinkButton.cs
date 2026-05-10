using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextLinkButton : MonoBehaviour, IPointerClickHandler
{
    public IdeologyRead ideologyRead;
    public Audiobook audiobook;
    public GameObject 档案界面;
    private TextMeshProUGUI text;
    public bool 下一行 = false;
    // Start is called before the first frame update
    void Start()
    {
        text = gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Vector3 pos = new Vector3(eventData.position.x, eventData.position.y, 0);
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, pos, null);
        if (linkIndex > -1)
        {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
            var linkText = linkInfo.GetLinkID();
            //Debug.Log("ClickLink: " + linkText);

            if (linkText.ToLower().StartsWith("profile:"))
            {
                var ideo = linkText.Split(":")[1];

                ideologyRead.搜索.text = ideo;
                ideologyRead.field.verticalScrollbar.value = 0f;

                ClickIdeLink.ExpandLinks.Clear();

                foreach (var item in ideo.Split(" | "))
                {
                    var originItem = item.Replace("[*]", "");

                    if (!ClickIdeLink.ExpandLinks.Contains(originItem))
                        ClickIdeLink.ExpandLinks.Add(originItem);
                }

                IdeologyRead.needReloadIde = true;
                档案界面.SetActive(true);
            }
        }
        else
        {
            if(下一行)
                audiobook.StateMouseClickedInContent(true);
        }
    }
}
