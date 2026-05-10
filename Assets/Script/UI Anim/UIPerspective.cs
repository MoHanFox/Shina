using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPerspective : MonoBehaviour
{
    public float X比例 = 50;
    public float Y比例 = 60;

    void Update()
    {
        if(Application.isMobilePlatform){
            //安卓不作处理
        }else{
            Vector2 pos = Input.mousePosition;

            float x = (pos.x - Screen.width / 2) / Screen.width / 2 * X比例;
            float y = (pos.y - Screen.height / 2) / Screen.height / 2 * Y比例;

            gameObject.GetComponent<RectTransform>().localEulerAngles = new(y,x);
        }
    }
}
