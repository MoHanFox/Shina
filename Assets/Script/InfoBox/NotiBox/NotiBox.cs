using System;

public static class NotiBox
{
    //标题
    public static string headText;
    //文本
    public static string text;
    //状态。用于显示或隐藏弹出框
    public static bool state;

    public static float showTime = 5;

    public static void Show(string _text, string _head)
    {
        text = _text;
        headText = _head;
        state = true;

        UiNoti.open();
    }
}