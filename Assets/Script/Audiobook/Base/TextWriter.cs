using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class TextWriter : MonoBehaviour {

    public static float charsPerSecond = ConfigManager.config.每秒打字间隔;//打字时间间隔
    public static string words = "";//保存需要显示的文字

    public static bool isActive = false;
    public static float timer;//计时器
    private TextMeshProUGUI myText;
    private int currentPos=0;//当前打字位置
    public static bool reRun = false;

    public Audiobook audiobook;

    // Use this for initialization
    void Start () {
        timer = 0;
        myText = GetComponent<TextMeshProUGUI> ();
    }

    private void OnDisable()
    {
        isActive = false;
    }

    // Update is called once per frame
    void Update () {
        if (reRun)
        {
            reRun = false;

            isActive = true;
            timer = 0;
            currentPos = 0;
            myText.text = "";
        }
        OnStartWriter ();
        //Debug.Log (isActive);
    }
    /// <summary>
    /// 执行打字任务
    /// </summary>
    void OnStartWriter(){

        if (isActive)
        {
            timer += Time.deltaTime;
            if (timer >= charsPerSecond)
            {//判断计时器时间是否到达
                timer = 0;
                if (words.Length == 0)
                    return;
                if (words[currentPos].ToString() != "<")
                    currentPos++;
                else
                {
                    int hasTag = 0;
                    foreach (char s in words.Substring(currentPos))
                    {
                        if (s.ToString() == "<")
                            hasTag++;

                        if (s.ToString() == ">")
                            break;
                    }

                    int i = 0;
                    int count = 0;

                    foreach (char s in words.Substring(currentPos))
                    {
                        if (s.ToString() == ">")
                        {
                            i++;
                        }

                        count++;

                        if (i == hasTag)
                        {
                            break;
                        }
                    }

                    currentPos += count;
                }

                myText.text = words.Substring(0, currentPos);//刷新文本显示内容

                if (currentPos >= words.Length)
                {
                    OnFinish();
                }
            }

        }
        else
            myText.text = words;
    }
    /// <summary>
    /// 结束打字，初始化数据
    /// </summary>
    void OnFinish(){
        isActive = false;
        timer = 0;
        currentPos = 0;
        myText.text = words;
    }
}
