using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LanguageTranslator : MonoBehaviour
{
    public string Key;

    private TextMeshProUGUI myText;
    private LanguageType 之前的语言;

    // Start is called before the first frame update
    void Start()
    {
        myText = myText = GetComponent<TextMeshProUGUI>();
        之前的语言 = LanguageManager.languageType;

        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {
        if(之前的语言 != LanguageManager.languageType)
        {
            之前的语言 = LanguageManager.languageType;
            UpdateText();
        }
    }

    void UpdateText()
    {
        string get = LanguageManager.GetKeyName(Key);

        if (get != "UNKNOWN")
        {
            myText.text = get;
        }
    }
}
