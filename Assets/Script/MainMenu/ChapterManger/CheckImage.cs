using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckImage : MonoBehaviour
{
    public GameObject 查看窗口;
    public RawImage 映射;

    public void Click()
    {
        映射.texture = gameObject.GetComponent<RawImage>().texture;

        Vector2 canvasSize = gameObject.GetComponentInParent<Canvas>().GetComponent<RectTransform>().sizeDelta;
        float screenxyRate = canvasSize.x / canvasSize.y;
        Vector2 bgSize = new(gameObject.GetComponent<RawImage>().texture.width, gameObject.GetComponent<RawImage>().texture.height);

        //视频尺寸长宽比
        float texturexyRate = bgSize.x / bgSize.y;

        if (texturexyRate > screenxyRate)
        {
            int newSizeY = Mathf.CeilToInt(canvasSize.y);
            int newSizeX = Mathf.CeilToInt((float)newSizeY / bgSize.y * bgSize.x);
            映射.rectTransform.sizeDelta = new Vector2(newSizeX, newSizeY);
        }
        else
        {
            int newVideoSizeX = Mathf.CeilToInt(canvasSize.x);
            int newVideoSizeY = Mathf.CeilToInt((float)newVideoSizeX / bgSize.x * bgSize.y);
            映射.rectTransform.sizeDelta = new Vector2(newVideoSizeX, newVideoSizeY);
        }
        
        查看窗口.SetActive(true);
    }
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
                mouseX = Input.GetTouch(0).deltaPosition.x * 15;
                mouseY = Input.GetTouch(0).deltaPosition.y * 15;
            }
        }
        else
        {
            if (Input.GetMouseButton(2))
            {
                Cursor.visible = false;
                mouseX = Input.GetAxis("Mouse X") * 250;
                mouseY = Input.GetAxis("Mouse Y") * 250;
            }
            else
                Cursor.visible = true;
        }

        Vector3[] cor = new Vector3[4];

        映射.GetComponent<RectTransform>().GetWorldCorners(cor);

        Vector2 newPosX_LU = cor[1] += new Vector3(mouseX, mouseY) * Time.fixedDeltaTime;
        Vector2 newPosX_RB = cor[3] += new Vector3(mouseX, mouseY) * Time.fixedDeltaTime;

        if (newPosX_LU.x < 0 && newPosX_RB.x > Screen.width)
        {
            映射.rectTransform.localPosition += new Vector3(mouseX, 0) * Time.fixedDeltaTime;
        }

        if (newPosX_RB.y < 0 && newPosX_LU.y > Screen.height)
        {
            映射.rectTransform.localPosition += new Vector3(0, mouseY) * Time.fixedDeltaTime;
        }
    }

}
