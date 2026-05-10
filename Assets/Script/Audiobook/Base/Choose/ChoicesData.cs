using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoicesData
{
    public string text;
    public int gotoRaw;

    public ChoicesData(string text,int gotoRaw)
    {
        this.text = text;
        this.gotoRaw = gotoRaw;
    }
}
