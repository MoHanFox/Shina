using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RectTransformZoomIn : MonoBehaviour
{
    // Start is called before the first frame update
    //等待时间
    public float waittime = 0;
    //渐变时长
    public float time = 0.35f;
    //定时器
    float timer;
    //透明度 范围是0-1
    float alpha = 1f;

    public float toAlpha = 1f;

    public bool active = false;

    // Start is called before the first frame update
    public void Rest()
    {
        active = true;
        timer = 0;
        alpha = 1f;
        gameObject.GetComponent<RectTransform>().localScale = new(0,0,0);
    }

    // Start is called before the first frame update
    private void OnEnable()
    {
        timer = 0;
        alpha = 1f;
        gameObject.GetComponent<RectTransform>().localScale = new(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (!active)
            return;

        if (timer < waittime)
            return;

        if (alpha > (1 - toAlpha))
            timer += Time.deltaTime;

        if (alpha > (1 - toAlpha))
        {
            alpha = (time - (timer - waittime)) / time;
            gameObject.GetComponent<RectTransform>().localScale = new(toAlpha, toAlpha, 0);
        }else{
            gameObject.GetComponent<RectTransform>().localScale = new(toAlpha, toAlpha, 0);
            active = false;
        }
    }
}
