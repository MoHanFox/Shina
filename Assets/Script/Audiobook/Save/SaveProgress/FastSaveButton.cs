using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastSaveButton : MonoBehaviour
{
    // Start is called before the first frame update
    public void Click()
    {
        SaveManager.inti.SaveByPresent();
    }
}
