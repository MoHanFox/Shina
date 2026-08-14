using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoticeUsing : MonoBehaviour
{

    private TextMeshProUGUI myText;
    private int lastChosen = -1;

    // Start is called before the first frame update
    void Start()
    {
        myText = GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        //只在档位变化时刷新，避免每帧字符串拼接 + TMP 网格重建
        if (lastChosen != ConfigManager.config.ChosenSave)
        {
            lastChosen = ConfigManager.config.ChosenSave;
            myText.text = "Num. " + lastChosen;
        }
    }
}
