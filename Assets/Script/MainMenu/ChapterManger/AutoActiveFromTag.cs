using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoActiveFromTag : MonoBehaviour
{
    public GameObject child;
    public string checkTag = "";

    public enum From
    {
        Collected,
        Saving
    }

    public From from;

    // Start is called before the first frame update
    private void OnEnable()
    {
        bool 满足条件 = false;
        if (from == From.Collected)
            foreach (string tags in TagManager.collectedTags.collectTags)
            {
                if (tags.StartsWith(checkTag.Split(" ")[0]))
                {
                    满足条件 = true;
                    break;
                }
            }
        if (from == From.Saving)
        {
            满足条件 = TagManager.满足条件(checkTag);
        }

        child.SetActive(满足条件);
    }
}
