using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class ReplaceStringByTag : MonoBehaviour
{
    TextMeshProUGUI text = null;

    // Start is called before the first frame update
    private void OnEnable()
    {
        if (text == null)
        {
            text = gameObject.GetComponent<TextMeshProUGUI>();
        }

        text.text = Audiobook.TAG文本替换(text.text);
    }
}
