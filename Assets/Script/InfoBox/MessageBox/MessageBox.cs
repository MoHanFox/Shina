using System;

public enum DialogResult
{
    Ok,
    OkCancel,
}

public static class MessageBox
{
    /// <summary>
    /// true表示模态框
    /// </summary>
    public static bool type;
    //三个委托，分别为三个按钮的点击运行事件
    public static Action clickOk;
    public static Action clickCancel;
    public static DialogResult dialogResult;
    //标题
    public static string headText;
    //文本
    public static string text;
    //选项文本
    public static string okText = "是";
    public static string cancelText = "否";
    //状态。用于显示或隐藏弹出框
    public static bool state;

    /// <summary>
    ///重试按钮点击事件
    /// </summary>
    public static void onClickRetry()
    {
        state = false;
    }
    /// <summary>
    /// 取消按钮点击事件
    /// </summary>
    public static void onClickCancel()
    {
        state = false;
        clickCancel?.Invoke();
        clickCancel = null;
    }
    /// <summary>
    /// 确定按钮点击事件
    /// </summary>
    public static void onClickOk()
    {
        state = false;
        clickOk?.Invoke();
        clickOk = null;
    }

    /// <summary>
    /// 显示
    /// </summary>
    /// <param name="_text">内容</param>
    /// <param name="_head">标题</param>
    /// <param name="dialog">样式</param>
    /// <param name="type">模式</param>
    public static void Show(string _text, string _head, DialogResult _dialog, bool _type = true)
    {
        text = _text;
        headText = _head;
        dialogResult = _dialog;
        type = _type;
        state = true;

        clickOk = () => { };
        clickCancel = () => { };

        okText = "是";
        cancelText = "否";

    }
    public static void Show(string _text, string _head, bool _type = true)
    {
        text = _text;
        headText = _head;
        dialogResult = DialogResult.Ok;
        type = _type;
        state = true;

        clickOk = () => { };
        clickCancel = () => { };

        okText = "是";
        cancelText = "否";
    }
    public static void Show(string _text, bool _type = true)
    {
        text = _text;
        headText = "信息";
        dialogResult = DialogResult.Ok;
        type = _type;
        state = true;

        clickOk = () => { };
        clickCancel = () => { };

        okText = "是";
        cancelText = "否";
    }

}