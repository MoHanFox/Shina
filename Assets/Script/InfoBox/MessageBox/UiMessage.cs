using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UiMessage : MonoBehaviour
{
    public Button ok;
    public Button cancel;
    public TextMeshProUGUI okText;
    public TextMeshProUGUI cancelText;
    /// <summary>
    /// 遮罩
    /// </summary>
    public GameObject panel;
    public TextMeshProUGUI headText;
    public TextMeshProUGUI text;
    public AudioSource 弹出音效;

    public static bool opening = false;

    private void Awake()
    {
        init();
    }

    private void OnEnable()
    {
        init();
        弹出音效.Play();
    }

    private void init()
    {
        ok.onClick.AddListener(MessageBox.onClickOk);
        cancel.onClick.AddListener(MessageBox.onClickCancel);
        text.text = MessageBox.text;
        headText.text = MessageBox.headText;
        okText.text = MessageBox.okText;
        cancelText.text = MessageBox.cancelText;

        //根据传递的参数，进行样式的显示
        switch (MessageBox.dialogResult)
        {
            case DialogResult.Ok:
                ok.gameObject.SetActive(true);
                cancel.gameObject.SetActive(false);
                opening = false;

                ok.transform.localPosition = new Vector2(180, -130);
                break;
            case DialogResult.OkCancel:
                ok.gameObject.SetActive(true);
                cancel.gameObject.SetActive(true);
                opening = false;

                ok.transform.localPosition = new Vector2(-180, -130);
                break;
        }
    }

    private void Update()
    {
        panel.SetActive(MessageBox.type);
        gameObject.SetActive(MessageBox.state);
    }

    public static void open()
    {
        GlobalHook.openMessageBox();
        opening = true;
    }
}