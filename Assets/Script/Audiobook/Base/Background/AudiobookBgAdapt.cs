using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class AudiobookBgMove : MonoBehaviour
{
    public RectTransform QTE;
    public RectTransform Background;
    // Update is called once per frame
    void Update()
    {
        float mouseX = 0f;
        float mouseY = 0f;

        if (Application.isMobilePlatform)
        {
            int touchCount = Input.touchCount;  // 获取触控点个数

            if (touchCount == 1)
            {
                mouseX = Input.GetTouch(0).deltaPosition.x * 20;
                mouseY = Input.GetTouch(0).deltaPosition.y * 20;
            }
        }
        else
        {
            if (Input.GetMouseButton(1))
            {
                Cursor.visible = false;
                mouseX = Input.GetAxis("Mouse X") * 250;
                mouseY = Input.GetAxis("Mouse Y") * 250;
            }else
                Cursor.visible = true;
        }

        Vector3[] cor = new Vector3[4];

        Background.GetComponent<RectTransform>().GetWorldCorners(cor);

        Vector2 newPosX_LU = cor[1] += new Vector3(mouseX, mouseY) * Time.fixedDeltaTime;
        Vector2 newPosX_RB = cor[3] += new Vector3(mouseX, mouseY) * Time.fixedDeltaTime;

        if (newPosX_LU.x < 0 && newPosX_RB.x > Screen.width)
        {
            Background.localPosition += new Vector3(mouseX, 0) * Time.fixedDeltaTime;
            QTE.localPosition = Background.localPosition;
        }else{
            if((newPosX_LU.x < 0 && mouseX > 0) || (newPosX_RB.x > Screen.width && mouseX < 0))
            {
                Background.localPosition += new Vector3(mouseX, 0) * Time.fixedDeltaTime;
                QTE.localPosition = Background.localPosition;
            }
        }

        if (newPosX_RB.y < 0 && newPosX_LU.y > Screen.height)
        {
            Background.localPosition += new Vector3(0, mouseY) * Time.fixedDeltaTime;
            QTE.localPosition = Background.localPosition;
        }else{
            if ((newPosX_RB.y > 0 && mouseY < 0) || (newPosX_LU.y < Screen.height && mouseY > 0))
            {
                Background.localPosition += new Vector3(0, mouseY) * Time.fixedDeltaTime;
                QTE.localPosition = Background.localPosition;
            }
        }
    }
}
