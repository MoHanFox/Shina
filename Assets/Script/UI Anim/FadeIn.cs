using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    // Start is called before the first frame update
    //等待时间
    public float waittime = 0;
    //渐变时长
    public float time = 0.5f;
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
        gameObject.GetComponent<Graphic>().color = new Color(gameObject.GetComponent<Graphic>().color.r, gameObject.GetComponent<Graphic>().color.g, gameObject.GetComponent<Graphic>().color.b, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            if (alpha > (1 - toAlpha))
                timer += Time.deltaTime;

            if (timer < waittime)
                return;

            if (alpha > (1 - toAlpha))
            {
                alpha = (time - (timer - waittime)) / time;
                gameObject.GetComponent<Graphic>().color = new Color(gameObject.GetComponent<Graphic>().color.r, gameObject.GetComponent<Graphic>().color.g, gameObject.GetComponent<Graphic>().color.b, 1.0f - alpha);
            }
            else
            {
                gameObject.GetComponent<Graphic>().color = new Color(gameObject.GetComponent<Graphic>().color.r, gameObject.GetComponent<Graphic>().color.g, gameObject.GetComponent<Graphic>().color.b, toAlpha);
                active = false;
            }
        }
    }
}
