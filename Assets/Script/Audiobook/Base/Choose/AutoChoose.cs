using OfficeOpenXml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AutoChoose : MonoBehaviour
{

    public RawImage 黑底;
    public GameObject 进度;

    // Update is called once per frame
    void Update()
    {
        if(ChoiseManager.AutoChooseTime > 0f)
        {
            if(ChoiseManager.chooseTimePassed < ChoiseManager.AutoChooseTime)
                ChoiseManager.chooseTimePassed += Time.deltaTime;
            else
            {
                ChoiseManager.chooseTimePassed = 0f;
                ChoiseManager.AutoChooseTime = -1f;

                GlobalHook.BookToRow(ChoiseManager.autoChooseTo);
                ChoiseManager.autoChooseTo = 0;
            }

            float 比值 = ChoiseManager.chooseTimePassed / ChoiseManager.AutoChooseTime;

            进度.GetComponent<RectTransform>().localScale = new Vector3(1 - 比值, 1,1);

        }
    }
}
