using OfficeOpenXml.FormulaParsing.Excel.Functions.RefAndLookup;
using OfficeOpenXml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BottonsChoicesClick : MonoBehaviour
{

    public ChoicesData choicesData = null;

    // Start is called before the first frame update
    public void onButtonClick()
    {
        TextMeshProUGUI text = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (choicesData.text == text.text)
        {
            //所作所为记入历史
            HistoryRecord.addContext("选择", "( " + choicesData.text + " )");

            GlobalHook.BookToRow(choicesData.gotoRaw);
        }
    }
}
