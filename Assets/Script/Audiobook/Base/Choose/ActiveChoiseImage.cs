using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveChoiseImage : MonoBehaviour
{
    // Start is called before the first frame update

    //限时条
    public GameObject 限时条;
    void Start()
    {
        限时条.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
         限时条.SetActive(ChoiseManager.AutoChooseTime > 0f);
    }
}
