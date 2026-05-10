using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReadVerToText : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<TextMeshProUGUI>().text = Application.version;
    }
}
