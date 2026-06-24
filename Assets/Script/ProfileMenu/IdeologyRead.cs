using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class IdeologyRead : MonoBehaviour
{
    public static ExcelPackage package;

    private static int configuredMaxRecords = 4000;
    public int maxRecords = 4000;

    public static void Inti()
    {
        if (Application.isMobilePlatform)
            CopyExcel();
        package = new(new FileInfo(Audiobook.usingPlatAssestPath + "/Profile/Ideologies.xlsx"));
    }

    public static void CopyExcel()
    {
        string sPath = Application.streamingAssetsPath + "/Profile/Ideologies.xlsx";
        UnityWebRequest www = UnityWebRequest.Get(sPath);
        www.SendWebRequest();

        while (!www.isDone)
        {

        }

        new FileInfo(Application.persistentDataPath + "/Profile/Ideologies.xlsx").Directory.Create();

        File.WriteAllBytes(Application.persistentDataPath + "/Profile/Ideologies.xlsx", www.downloadHandler.data);
    }

    public void 输入值改变()
    {
        if (ClickIdeLink.ExpandLinks.Count != 0)
        {
            ClickIdeLink.ExpandLinks.Clear();
        }
        needReloadIde = true;
        
        field.verticalScrollbar.value = 1f; //本来应该是0的
    }

    public bool 是否含有搜索内容(List<string> 搜索集, string 文本, bool 搜索内容)
    {
        if (搜索内容)
        {
            bool pass = false;

            foreach (var item in 搜索集)
            {
                pass = 文本.ToLower().Contains(item.Replace("[*]", "").ToLower());

                if (pass)
                    break;
            }

            return pass;
        }
        else
        {
            bool pass = false;

            foreach (var item in 搜索集)
            {
                if (item.StartsWith("[*]"))
                    pass = 文本.ToLower().Equals(item.Replace("[*]", "").ToLower());
                else
                    pass = 文本.ToLower().StartsWith(item);

                if (pass)
                    break;
            }

            return pass;
        }
    }

    private sealed class Entry
    {
        public readonly string title;
        public readonly string text;
        public float top;
        public float height;
        public float measuredWidth = -1f;

        public Entry(string text)
        {
            this.text = text ?? string.Empty;
        }
    }

    private sealed class EntryView
    {
        public readonly GameObject gameObject;
        public readonly RectTransform rect;
        public readonly TMP_Text text;

        public EntryView(GameObject gameObject)
        {
            this.gameObject = gameObject;
            rect = gameObject.GetComponent<RectTransform>();
            text = gameObject.GetComponent<TMP_Text>();
            this.gameObject.AddComponent<ClickIdeLink>();
        }
    }

    private const float EntryVerticalPadding = 28f;
    private const float BottomPadding = 24f;
    private const float TopPadding = 24f;
    private const float ScrollPadding = 16f;

    private static readonly List<Entry> entries = new();
    private static IdeologyRead activeInstance;
    private static IdeologyRead anyInstance;
    public static bool dataChanged;
    public static bool needReloadIde;

    public TMP_InputField field;

    public TMP_InputField 搜索;
    public Toggle 检索内容;

    private ScrollRect scrollRect;
    private RectTransform viewport;
    private RectTransform content;
    private TMP_Text templateText;
    private readonly List<EntryView> pool = new();
    private readonly List<EntryView> activeViews = new();
    private bool initialized;
    private bool stickToBottomNextRefresh;
    private float lastViewportWidth = -1f;
    private float lastNormalized = float.NaN;
    private int metricsCursor;
    private float totalHeight;

    private void Awake()
    {
        anyInstance = this;
        Initialize();
    }

    private void loadIde()
    {
        //重添加
        ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

        //[tag] Key

        //内容.text = "<size=48><b>● 设定</b></size><br><br>\t您可以在这里查看设定, 点击展开<br><br>";

        if (搜索.text == "")
            addContext("<size=48><b>● 设定</b></size><br><br>\t您可以在这里查看设定, 点击展开<br><br>");

        //StringBuilder addon = new();
        string lastTitle = "";

        for (int i = 2; i <= worksheet.Dimension.End.Row; i++)
        {
            bool 是否为补充项目 = !worksheet.Cells[i, 3].Value.IsUnityNull() && worksheet.Cells[i, 2].Value.IsUnityNull();


            if (!worksheet.Cells[i, 2].Value.IsUnityNull())
            {
                lastTitle = worksheet.Cells[i, 2].Value.ToString();
            }

            if (搜索.text != "")
            {
                string 合并内容;

                if (检索内容.isOn)
                    合并内容 = (worksheet.Cells[i, 2].Value.IsUnityNull() ? "" : worksheet.Cells[i, 2].Value.ToString()) + (worksheet.Cells[i, 3].Value.IsUnityNull() ? "" : worksheet.Cells[i, 3].Value.ToString());
                else
                    合并内容 = worksheet.Cells[i, 2].Value.IsUnityNull() ? "" : worksheet.Cells[i, 2].Value.ToString();


                var 搜索集合 = 搜索.text.Split(" | ").ToList();

                if (!是否为补充项目)
                {
                    if (!是否含有搜索内容(搜索集合, 合并内容, 检索内容.isOn))
                        continue;
                }
                else
                {
                    if (!ClickIdeLink.ExpandLinks.Contains(lastTitle))
                        continue;
                }
            }

            if (worksheet.Cells[i, 1].Value.IsUnityNull())
            {
                //Title
                if (!worksheet.Cells[i, 2].Value.IsUnityNull())
                    addContext("<size=42><b><color=#E2C854>丨</color><link=\"" + worksheet.Cells[i, 2].Value.ToString() + "\">" + worksheet.Cells[i, 2].Value.ToString() + "</link></b></size><br><br>"); ;

                if (ClickIdeLink.ExpandLinks.Contains(lastTitle))
                {
                    if (是否为补充项目)
                        addContext("<align=\"center\"><size=18><i>———————————补充———————————</i></align></size><br><br>");
                    if (!worksheet.Cells[i, 3].Value.IsUnityNull())
                        addContext("	" + worksheet.Cells[i, 3].Value.ToString() + "</size></i></b><br><br>");
                }
            }
            else
            {
                string mainTag = worksheet.Cells[i, 1].Value.ToString();

                if (TagManager.collectedTags.collectTags.Contains("Profile:Unlock:" + mainTag))
                {
                    //Title
                    if (!worksheet.Cells[i, 2].Value.IsUnityNull())
                        addContext("<size=42><b><color=#E2C854>丨</color><link=\"" + worksheet.Cells[i, 2].Value.ToString() + "\">" + worksheet.Cells[i, 2].Value.ToString() + "</link></b></size><br><br>");

                    if (ClickIdeLink.ExpandLinks.Contains(lastTitle))
                    {
                        if (是否为补充项目)
                            addContext("<align=\"center\"><size=18><i>———————————补充———————————</i></align></size><br><br>");
                        if (!worksheet.Cells[i, 3].Value.IsUnityNull())
                            addContext("	" + worksheet.Cells[i, 3].Value.ToString() + "</size></i></b><br><br>");
                    }
                }
            }
        }
        
        dataChanged = true;
    }

    private void Start()
    {
        needReloadIde = true;
        activeInstance = this;
        Initialize();
    }

    private void OnDisable()
    {
        if (activeInstance == this)
            activeInstance = null;
    }

    private void Update()
    {
        if (!initialized)
        {
            return;
        }

        float normalized = scrollRect != null ? scrollRect.verticalNormalizedPosition : 1f;
        float width = GetCurrentContentWidth();

        if (needReloadIde)
        {
            needReloadIde = false;
            ClearAll();
            loadIde();
        }

        if (dataChanged || Mathf.Abs(normalized - lastNormalized) > 0.0001f || Mathf.Abs(width - lastViewportWidth) > 0.5f)
        {
            RefreshVisible(false);
        }
    }

    public static void ClearAll()
    {
        entries.Clear();
        dataChanged = true;
        if (activeInstance != null)
        {
            activeInstance.metricsCursor = 0;
            activeInstance.lastViewportWidth = -1f;
            activeInstance.totalHeight = 0f;
            activeInstance.stickToBottomNextRefresh = false;
            if (activeInstance.initialized)
                activeInstance.RefreshVisible(true);
        }
    }

    public static void addContext(string text)
    {
        entries.Add(new Entry(text));

        bool overflowed = false;
        if (configuredMaxRecords > 0 && entries.Count > configuredMaxRecords)
        {
            int overflow = entries.Count - configuredMaxRecords;
            entries.RemoveRange(0, overflow);
            overflowed = true;

            if (activeInstance != null)
            {
                activeInstance.metricsCursor = 0;
                activeInstance.lastViewportWidth = -1f;
                activeInstance.totalHeight = 0f;
            }
        }

        if (activeInstance != null && activeInstance.IsNearBottom())
            activeInstance.stickToBottomNextRefresh = false;

        if (!overflowed && anyInstance != null)
            anyInstance.TryMeasureLatestEntry();
    }

    private void TryMeasureLatestEntry()
    {
        if (!initialized || templateText == null || entries.Count == 0)
            return;

        float contentWidth = GetCurrentContentWidth();
        if (contentWidth <= 1f)
            return;

        int index = entries.Count - 1;
        //int index = 0;
        Entry entry = entries[index];
        if (Mathf.Abs(entry.measuredWidth - contentWidth) <= 0.5f && entry.height > 0f)
            return;

        float top = index == 0 ? TopPadding : entries[index - 1].top + entries[index - 1].height + ScrollPadding;
        string richText = BuildRichText(entry);
        templateText.text = richText;

        Vector2 preferred = templateText.GetPreferredValues(richText, contentWidth, Mathf.Infinity);
        templateText.text = string.Empty;

        entry.height = Mathf.Max(64f, Mathf.Ceil(preferred.y) + EntryVerticalPadding);
        entry.measuredWidth = contentWidth;
        entry.top = top;

        totalHeight = top + entry.height + BottomPadding;
        if (content != null)
            content.sizeDelta = new Vector2(content.sizeDelta.x, Mathf.Max(totalHeight, viewport.rect.height));

        metricsCursor = entries.Count;
        lastViewportWidth = contentWidth;
    }

    private void Initialize()
    {
        if (initialized)
            return;

        if (field == null)
            field = GetComponentInChildren<TMP_InputField>(true);

        configuredMaxRecords = Mathf.Max(1, maxRecords);

        if (field == null)
        {
            Debug.LogError("HistoryRecord: missing TMP_InputField reference.");
            return;
        }

        viewport = field.textViewport as RectTransform;
        content = field.textComponent != null ? field.textComponent.rectTransform : null;
        templateText = field.textComponent;

        if (viewport == null || content == null || templateText == null)
        {
            Debug.LogError("HistoryRecord: incomplete history view hierarchy.");
            return;
        }

        scrollRect = field.GetComponent<ScrollRect>();
        if (scrollRect == null)
            scrollRect = field.gameObject.AddComponent<ScrollRect>();

        field.enabled = false;
        field.interactable = false;
        field.readOnly = true;

        templateText.enabled = false;
        templateText.text = string.Empty;
        templateText.richText = true;
        templateText.enableWordWrapping = true;
        templateText.overflowMode = TextOverflowModes.Overflow;
        templateText.alignment = TextAlignmentOptions.TopLeft;

        ConfigureScrollRect();
        ConfigureContentTransform();

        stickToBottomNextRefresh = true;

        initialized = true;
    }

    private void ConfigureScrollRect()
    {
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.inertia = true;
        scrollRect.scrollSensitivity = 28f;
        scrollRect.viewport = viewport;
        scrollRect.content = content;
        scrollRect.verticalScrollbar = field.verticalScrollbar;
        scrollRect.horizontalScrollbar = null;
        scrollRect.onValueChanged.RemoveAllListeners();
    }

    private void ConfigureContentTransform()
    {
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(1f, 1f);

        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.anchoredPosition = Vector2.zero;
        content.sizeDelta = new Vector2(0f, TopPadding + BottomPadding);
    }

    public void RefreshVisible(bool forceBottom)
    {
        if (!initialized || scrollRect == null || viewport == null || content == null || templateText == null)
            return;

        if (viewport.rect.width <= 1f || viewport.rect.height <= 1f)
            return;

        EnsureMetrics();
        RebuildVisibleItems();

        if (forceBottom || stickToBottomNextRefresh)
        {
            //scrollRect.verticalNormalizedPosition = 0f;
            stickToBottomNextRefresh = false;
            RebuildVisibleItems();
        }

        lastNormalized = scrollRect.verticalNormalizedPosition;
        lastViewportWidth = GetCurrentContentWidth();
        dataChanged = false;
    }

    private void EnsureMetrics()
    {
        float contentWidth = GetCurrentContentWidth();
        if (contentWidth <= 1f)
            return;

        if (entries.Count == 0)
        {
            totalHeight = TopPadding + BottomPadding;
            content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
            metricsCursor = 0;
            lastViewportWidth = contentWidth;
            return;
        }

        if (metricsCursor > entries.Count)
            metricsCursor = 0;

        if (Mathf.Abs(contentWidth - lastViewportWidth) > 0.5f)
            metricsCursor = 0;

        float currentTop = TopPadding;
        if (metricsCursor > 0)
        {
            Entry previous = entries[metricsCursor - 1];
            currentTop = previous.top + previous.height + ScrollPadding;
        }

        for (int i = metricsCursor; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            entry.height = CalculateEntryHeight(entry, contentWidth);
            entry.measuredWidth = contentWidth;
            entry.top = currentTop;
            currentTop += entry.height + ScrollPadding;
        }

        totalHeight = currentTop - ScrollPadding + BottomPadding;
        if (totalHeight < viewport.rect.height)
            totalHeight = viewport.rect.height;

        content.sizeDelta = new Vector2(content.sizeDelta.x, totalHeight);
        metricsCursor = entries.Count;
        lastViewportWidth = contentWidth;
    }

    private void RebuildVisibleItems()
    {
        HideActiveViews();

        if (entries.Count == 0)
        {
            content.sizeDelta = new Vector2(content.sizeDelta.x, Mathf.Max(viewport.rect.height, TopPadding + BottomPadding));
            return;
        }

        float viewportHeight = viewport.rect.height;
        float scrollOffset = GetScrollOffset(totalHeight, viewportHeight);
        float startOffset = Mathf.Max(0f, scrollOffset - 2f * ScrollPadding);
        float endOffset = scrollOffset + viewportHeight + 2f * ScrollPadding;

        int firstIndex = FindFirstVisibleIndex(startOffset);
        int lastIndex = FindLastVisibleIndex(endOffset);

        if (lastIndex < firstIndex)
            return;

        int required = lastIndex - firstIndex + 1;
        EnsurePoolSize(required);

        for (int i = firstIndex; i <= lastIndex; i++)
        {
            Entry entry = entries[i];
            EntryView view = GetViewFromPool();
            SetupView(view, entry);
            activeViews.Add(view);
        }
    }

    private void SetupView(EntryView view, Entry entry)
    {
        string richText = BuildRichText(entry);

        view.gameObject.SetActive(true);
        view.rect.anchorMin = new Vector2(0f, 1f);
        view.rect.anchorMax = new Vector2(1f, 1f);
        view.rect.pivot = new Vector2(0.5f, 1f);
        view.rect.anchoredPosition = new Vector2(0f, -entry.top);
        view.rect.sizeDelta = new Vector2(-ScrollPadding * 2f, entry.height);

        view.text.enabled = true;
        view.text.richText = true;
        view.text.enableWordWrapping = true;
        view.text.overflowMode = TextOverflowModes.Overflow;
        view.text.alignment = TextAlignmentOptions.TopLeft;
        view.text.text = richText;
    }

    private void HideActiveViews()
    {
        for (int i = 0; i < activeViews.Count; i++)
        {
            EntryView view = activeViews[i];
            view.gameObject.SetActive(false);
            pool.Add(view);
        }

        activeViews.Clear();
    }

    private void EnsurePoolSize(int required)
    {
        while (pool.Count < required)
            pool.Add(CreateView());
    }

    private EntryView GetViewFromPool()
    {
        int last = pool.Count - 1;
        EntryView view = pool[last];
        pool.RemoveAt(last);
        return view;
    }

    private EntryView CreateView()
    {
        GameObject clone = Instantiate(templateText.gameObject, content, false);
        clone.name = "IdeItem";

        TMP_Text text = clone.GetComponent<TMP_Text>();
        text.enabled = true;
        text.text = string.Empty;
        text.richText = true;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.alignment = TextAlignmentOptions.TopLeft;

        EntryView view = new EntryView(clone);
        view.rect.anchorMin = new Vector2(0f, 1f);
        view.rect.anchorMax = new Vector2(1f, 1f);
        view.rect.pivot = new Vector2(0.5f, 1f);
        view.gameObject.SetActive(false);
        return view;
    }

    private float CalculateEntryHeight(Entry entry, float contentWidth)
    {
        string richText = BuildRichText(entry);
        templateText.text = richText;

        Vector2 preferred = templateText.GetPreferredValues(richText, contentWidth, Mathf.Infinity);
        templateText.text = string.Empty;

        return Mathf.Max(64f, Mathf.Ceil(preferred.y) + EntryVerticalPadding);
    }

    private string BuildRichText(Entry entry)
    {
        return entry.text;
    }

    private int FindFirstVisibleIndex(float offset)
    {
        int left = 0;
        int right = entries.Count - 1;
        int result = entries.Count;

        while (left <= right)
        {
            int mid = left + ((right - left) >> 1);
            Entry entry = entries[mid];
            float bottom = entry.top + entry.height;

            if (bottom >= offset)
            {
                result = mid;
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        return Mathf.Clamp(result, 0, entries.Count - 1);
    }

    private int FindLastVisibleIndex(float offset)
    {
        int left = 0;
        int right = entries.Count - 1;
        int result = 0;

        while (left <= right)
        {
            int mid = left + ((right - left) >> 1);
            Entry entry = entries[mid];

            if (entry.top <= offset)
            {
                result = mid;
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }

        return Mathf.Clamp(result, 0, entries.Count - 1);
    }

    private float GetCurrentContentWidth()
    {
        if (viewport == null)
            return 0f;

        return viewport.rect.width - ScrollPadding * 2f;
    }

    private float GetScrollOffset(float contentHeight, float viewportHeight)
    {
        if (contentHeight <= viewportHeight)
            return 0f;

        float normalized = scrollRect != null ? scrollRect.verticalNormalizedPosition : 1f;
        return (1f - normalized) * (contentHeight - viewportHeight);
    }

    private bool IsNearBottom()
    {
        if (scrollRect == null)
            return true;

        return scrollRect.verticalNormalizedPosition <= 0.01f;
    }
}