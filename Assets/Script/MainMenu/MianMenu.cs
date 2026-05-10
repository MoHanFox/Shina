using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MainMenu: MonoBehaviour
{
    // Start is called before the first frame update
    [Header("音频")]
    public AudioSource 主页音乐;

    [Header("界面")]
    public GameObject 小说界面;
    public GameObject 主界面;
    public GameObject 载入动画GameObject;

    private void OnEnable()
    {
        黑屏转场进();
    }

    public void 黑屏转场进()
    {
        GameObject 载入 = Instantiate(载入动画GameObject);
        载入.transform.SetParent(主界面.transform);
        载入.GetComponent<RectTransform>().localPosition = new(0, 0);
        载入.GetComponent<RectTransform>().localScale = new(1, 1, 1);
    }

    void Awake()
    {
        主页音乐.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (Audiobook.startPlaying && !小说界面.activeInHierarchy)
            小说界面.SetActive(true);

        if(!Audiobook.startPlaying && 小说界面.activeInHierarchy)
            小说界面.SetActive(false);
    }
}
