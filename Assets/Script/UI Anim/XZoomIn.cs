using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class XZoomIn : MonoBehaviour
{
    // Start is called before the first frame update
    //等待时间
    public float waittime = 0;
    //渐变时长
    public float time = 0.5f;
    //定时器
    float timer;
    //透明度 范围是0-1
    float size = 1f;

    public float toSize = 1f;

    // Start is called before the first frame update
    private void OnEnable()
    {
        timer = 0;
        size = 1f;
        gameObject.GetComponent<RectTransform>().localScale = new(0, 1, 1);
    }

    // Update is called once per frame
    void Update()
    {
        if (size > (1 - toSize))
            timer += Time.deltaTime;

        if (timer > waittime && size > (1 - toSize))
        {
            size = (time - (timer - waittime)) / time;
            gameObject.GetComponent<RectTransform>().localScale = new(1 - size, 1, 1);
        }
    }
}
