using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseForMove : MonoBehaviour
{
    // Start is called before the first frame update
    RectTransform 承载;
    void Start()
    {
        承载 = gameObject.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void OnGUI()
    {
        // 处理鼠标右键或触摸输入
        if (Input.GetMouseButton(1) || Input.touchCount > 0)
        {
            Cursor.lockState = CursorLockMode.Locked;
            float moveX = 0f;
            float moveY = 0f;


            // 根据平台处理输入
            if (Application.isMobilePlatform && Input.touchCount > 0)
            {
                // 移动平台触摸输入
                Touch touch = Input.GetTouch(0);
                moveX = touch.deltaPosition.x * 50f;
                moveY = touch.deltaPosition.y * 50f;
            }
            else if (!Application.isMobilePlatform)
            {
                // 桌面平台鼠标输入
                moveX = Input.GetAxis("Mouse X") * 800f; // 调整灵敏度以匹配触摸体验
                moveY = Input.GetAxis("Mouse Y") * 800f;
            }

            // 计算目标位置，使用Time.deltaTime确保不同帧率下移动速度一致
            Vector3 targetPosition = 承载.localPosition + new Vector3(moveX, moveY) * Time.deltaTime;

            // 使用Lerp实现平滑移动，减少抖动
            //承载.localPosition = Vector3.Lerp(承载.localPosition, targetPosition, 15f * Time.deltaTime);
            承载.localPosition = Vector3.Lerp(承载.localPosition, targetPosition, 15f * Time.deltaTime);
        }
        else
        {
            // 没有输入时解锁鼠标
            Cursor.lockState = CursorLockMode.None;
        }
    }

}
