using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionAnim : MonoBehaviour
{
    // Start is called before the first frame update
    //等待时间
    public float waittime = 0.15f;
    //渐变时长
    public float time = 0.5f;
    //定时器
    public Audiobook audiobook;

    static public float timer;
    //透明度 范围是0-1
    static float alpha = 0.9f;

    static bool canSee = false;
    public static bool doAnim = false;

    // Start is called before the first frame update
    void Start()
    {
        //gameObject.SetActive(false);    //Fku漠憨憨，在Start里写这个是不是想要每次启动都少一个黑场？？
    }

    private void OnEnable()
    {
        timer = 0f;
        alpha = 1.0f;
        canSee = false;
        doAnim = true;
    }

    private void OnDisable()
    {
        doAnim = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (doAnim)
        {
            if (canSee)      //淡出
            {
                if (alpha > 0f)
                    timer += Time.deltaTime;
                else
                {
                    canSee = false;
                    doAnim = false;
                    gameObject.SetActive(false);
                }

                if (timer > waittime)
                {
                    alpha = (time - (timer - waittime)) / time;
                    gameObject.GetComponent<Graphic>().color = new Color(gameObject.GetComponent<Graphic>().color.r, gameObject.GetComponent<Graphic>().color.g, gameObject.GetComponent<Graphic>().color.b, alpha);
                }
            }
            else         //淡入
            {
                if (alpha > 0f)
                {
                    timer += Time.deltaTime;

                    alpha = (time - timer) / time;
                    gameObject.GetComponent<Graphic>().color = new Color(gameObject.GetComponent<Graphic>().color.r, gameObject.GetComponent<Graphic>().color.g, gameObject.GetComponent<Graphic>().color.b, 1 - alpha);
                }
                else
                {
                    canSee = true;
                    timer = 0f;
                    alpha = 0.9f;

                    //转行
                    audiobook.ToRow(Audiobook.row);
                }
            }
        }
    }
}
