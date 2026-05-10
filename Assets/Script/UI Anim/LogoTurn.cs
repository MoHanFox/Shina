using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoTurn : MonoBehaviour
{

    //一圈时间
    public float time = 5;
    //定时器
    float timer;
    float a = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        a = (time - timer) / time;

        if (a <= 0)
            timer = 0;

        gameObject.transform.eulerAngles = new Vector3(0, 0,360 * a);
    }
}
