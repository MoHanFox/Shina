using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphicFadeOutAndDestroy : MonoBehaviour
{
    // Start is called before the first frame update
    //等待时间
    public float waittime = 0;
    //渐变时长
    public float time = 0.5f;
    //定时器
    float timer;
    //透明度 范围是0-1
    float alpha = 1.0f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (alpha > 0f)
            timer += Time.deltaTime;
        else
            Destroy(gameObject);

        if (timer > waittime)
        {
            alpha = (time - (timer - waittime)) / time;
            gameObject.GetComponent<Graphic>().color = new Color(gameObject.GetComponent<Graphic>().color.r, gameObject.GetComponent<Graphic>().color.g, gameObject.GetComponent<Graphic>().color.b, alpha);
        }
    }
}
