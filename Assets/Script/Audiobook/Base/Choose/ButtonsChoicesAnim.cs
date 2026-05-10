using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsChoiceAnim : MonoBehaviour
{
    //渐变时长
    public float timeIn = 0.15f;
    //定时器
    public static float timer;

    // Start is called before the first frame update

    public static float i = 0f;
    // Update is called once per frame
    void Update()
    {
        RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
        rectTransform.localScale = new Vector3(i, 1, 1);

        if (i < 1f)
        {
            timer += Time.deltaTime;
            i = timer / timeIn;
        }else{
            i = 1;
        }
    }

    public static void Rest()
    {
        timer = 0f;
        i = 0f;
    }
    public static void FlyIn()
    {
        timer = 0f;
        i = 0f;
    }
}
