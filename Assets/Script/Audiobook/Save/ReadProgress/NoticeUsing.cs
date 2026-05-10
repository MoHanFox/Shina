using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NoticeUsing : MonoBehaviour
{

    private TextMeshProUGUI myText;

    // Start is called before the first frame update
    void Start()
    {
        myText = GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        myText.text = "Num. " + ConfigManager.config.ChosenSave;
    }
}
