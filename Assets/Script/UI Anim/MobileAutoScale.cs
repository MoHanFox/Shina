using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileAutoScale : MonoBehaviour
{
    void Start()
    {
        if (Screen.width / Screen.height > 16 / 9)
        {
            float present = Screen.width / Screen.height;

            float factor = 16 / 9 / present;

            gameObject.GetComponent<RectTransform>().localScale = new(factor * 1.5f, factor * 1.5f, factor * 1.5f);
        }
    }
}
