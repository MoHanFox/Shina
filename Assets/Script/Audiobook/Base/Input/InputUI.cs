using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InputUI : MonoBehaviour
{

    public int gotoRow;
    public int backRow;
    public string value;

    public TextMeshProUGUI title;
    public TMP_InputField TMP_InputField;

    public void cancel()
    {
        GlobalHook.BookToRow(backRow);
        Destroy(gameObject);
    }

    public void comfirm()
    {
        GlobalHook.BookToRow(gotoRow);
        //先删除
        foreach (string s in SaveManager.data0.事件标签)
        {
            if (s.StartsWith(value))
            {
                SaveManager.data0.事件标签.Remove(s);
                break;
            }
        }
        //再添加
        SaveManager.data0.事件标签.Add(value + "=" + TMP_InputField.text);
        Destroy(gameObject);
    }
}
