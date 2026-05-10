using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigRecord : MonoBehaviour
{
    public void save()
    {
        ConfigManager.inti.SaveByPresent();
    }
}
