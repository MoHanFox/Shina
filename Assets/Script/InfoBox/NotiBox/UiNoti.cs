using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiNoti : MonoBehaviour
{
    /// <summary>
    /// 遮罩
    /// </summary>
    public TextMeshProUGUI headText;
    public TextMeshProUGUI text;
    public AudioSource 弹出音效;
    public RectTransform parent;

    public float time = 0.22f;

    float timer;
    float wait_timer;

    float i = 0f;
    bool IN = true;

    private void OnEnable()
    {
        init();
        弹出音效.Play();
    }

    private void init()
    {
        parent.localScale = new Vector3(0, 0, 1);

        text.text = NotiBox.text;
        headText.text = NotiBox.headText;

        wait_timer = 0f;
        timer = 0f;
        i = 0f;
        IN = true;
    }

    private void Update()
    {
        gameObject.SetActive(NotiBox.state);
        parent.localScale = new Vector3(IN ? i:1-i, 1, 1); ;

        if (i < 0f)
        {
            NotiBox.state = false;
        }

        if (i < 1f)
            timer += Time.deltaTime;

        if (i > 1f)
        {
            if (IN)
                i = 1F;
            else
                NotiBox.state = false;
        }

        wait_timer += Time.deltaTime;

        if (wait_timer >= NotiBox.showTime && IN)
        {
            timer = 0f;
            i = 0f;
            IN = false;
        }

        i = timer / time;
    }

    public static void open()
    {
        GlobalHook.openNotiBox();
    }
}