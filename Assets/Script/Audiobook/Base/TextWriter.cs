using TMPro;
using UnityEngine;

/// <summary>
/// 打字机效果。
/// 优化：消除原实现每帧的字符级 ToString()/Substring 分配；
/// 富文本标签整体跳过（行为与原实现一致）；文本未变化时跳过 TMP 赋值（避免无谓的网格重建）。
/// </summary>
public class TextWriter : MonoBehaviour
{
    public static float charsPerSecond = ConfigManager.config.每秒打字间隔;//打字时间间隔
    public static string words = "";//保存需要显示的文字

    public static bool isActive = false;
    public static bool reRun = false;

    private TextMeshProUGUI myText;
    private float timer;//计时器
    private int currentPos;//当前打字位置

    public Audiobook audiobook;

    void Awake()
    {
        timer = 0;
        currentPos = 0;
        myText = GetComponent<TextMeshProUGUI>();
    }

    private void OnDisable()
    {
        isActive = false;
    }

    void Update()
    {
        if (reRun)
        {
            reRun = false;

            isActive = true;
            timer = 0;
            currentPos = 0;
            myText.text = "";
        }

        if (isActive)
        {
            timer += Time.deltaTime;

            if (timer < charsPerSecond)
                return;

            timer = 0;

            if (words.Length == 0)
                return;

            //前进一个可见字符；富文本标签（如 <color=#fff>）整体跳过，不占打字进度
            if (words[currentPos] != '<')
                currentPos++;
            else
                currentPos = 跳过富文本标签(currentPos);

            myText.text = words.Substring(0, currentPos);//刷新文本显示内容

            if (currentPos >= words.Length)
            {
                //结束打字，初始化数据
                isActive = false;
                timer = 0;
                currentPos = 0;
                myText.text = words;
            }
        }
        else if (myText.text != words)
        {
            myText.text = words;
        }
    }

    /// <summary>从 tagStart 起跳过一层富文本标签，返回标签结束后的下标（标签未闭合则视为文本结束）</summary>
    int 跳过富文本标签(int tagStart)
    {
        int depth = 0;
        for (int i = tagStart; i < words.Length; i++)
        {
            if (words[i] == '<')
                depth++;
            else if (words[i] == '>')
            {
                depth--;
                if (depth == 0)
                    return i + 1;
            }
        }

        return words.Length;
    }
}
