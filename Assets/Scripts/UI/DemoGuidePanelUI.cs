using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Hackathon demo journey checklist. Reads progress from MainQuestManager only.
/// Toggle with G, close with Esc. Safe to delete after demo.
/// </summary>
public class DemoGuidePanelUI : MonoBehaviour
{
    struct GuideRow
    {
        public string Title;
        public string Subtitle;
        public int QuestStepIndex;
    }

    static readonly GuideRow[] GuideRows =
    {
        new GuideRow { Title = "出生于荒原", Subtitle = string.Empty, QuestStepIndex = -1 },
        new GuideRow { Title = "访问 Blue DAO", Subtitle = "理解开放协作", QuestStepIndex = 0 },
        new GuideRow { Title = "访问 Red DAO", Subtitle = "获得第一枚 MOON", QuestStepIndex = 1 },
        new GuideRow { Title = "收集 Moon Crystal", Subtitle = "积累资源", QuestStepIndex = 2 },
        new GuideRow { Title = "获得 Green Pass", Subtitle = "理解准入机制", QuestStepIndex = 4 },
        new GuideRow { Title = "抵达文明边界", Subtitle = "理解文明扩张", QuestStepIndex = 5 },
        new GuideRow { Title = "创建自己的 Civilization", Subtitle = "制定第一条规则", QuestStepIndex = 6 }
    };

    bool _isOpen;
    MainQuestManager _questManager;

    GUIStyle _panelStyle;
    GUIStyle _titleStyle;
    GUIStyle _checkStyle;
    GUIStyle _rowTitleStyle;
    GUIStyle _rowTitleCurrentStyle;
    GUIStyle _rowSubtitleStyle;
    GUIStyle _progressStyle;
    GUIStyle _footerStyle;
    bool _stylesReady;

    void Awake()
    {
        _questManager = GetComponent<MainQuestManager>();
        if (_questManager == null)
        {
            _questManager = FindFirstObjectByType<MainQuestManager>();
        }
    }

    void Update()
    {
        if (TabMenuUI.IsOpen)
        {
            if (_isOpen)
            {
                Close();
            }

            return;
        }

        if (GameplayInputGate.BlocksGameplayShortcuts)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
        {
            Toggle();
        }

        if (_isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
#else
        if (Input.GetKeyDown(KeyCode.G))
        {
            Toggle();
        }

        if (_isOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Close();
        }
#endif
    }

    void Toggle()
    {
        _isOpen = !_isOpen;
    }

    void Close()
    {
        _isOpen = false;
    }

    void OnGUI()
    {
        if (!_isOpen)
        {
            return;
        }

        if (_questManager == null)
        {
            _questManager = FindFirstObjectByType<MainQuestManager>();
            if (_questManager == null)
            {
                return;
            }
        }

        EnsureStyles();

        const float width = 360f;
        const float rowHeight = 42f;
        const float birthRowHeight = 28f;
        const float paddingTop = 20f;
        const float paddingBottom = 68f;
        const float innerPad = 18f;

        float contentHeight = paddingTop + 30f + 12f;
        contentHeight += birthRowHeight;
        contentHeight += (GuideRows.Length - 1) * rowHeight;
        contentHeight += paddingBottom;

        float margin = 14f;
        float x = Screen.width - width - margin;
        float y = margin;

        GUI.Box(new Rect(x, y, width, contentHeight), GUIContent.none, _panelStyle);

        float lineX = x + innerPad;
        float lineW = width - innerPad * 2f;
        float lineY = y + paddingTop;

        GUI.Label(new Rect(lineX, lineY, lineW, 28f), "Demo Journey", _titleStyle);
        lineY += 34f;

        for (int i = 0; i < GuideRows.Length; i++)
        {
            GuideRow row = GuideRows[i];
            bool complete = _questManager.IsDemoMilestoneComplete(row.QuestStepIndex);
            bool current = _questManager.IsDemoGuideRowCurrent(row.QuestStepIndex);
            float rowH = row.QuestStepIndex < 0 ? birthRowHeight : rowHeight;

            DrawGuideRow(lineX, lineY, lineW, rowH, row, complete, current);
            lineY += rowH;
        }

        int completed = _questManager.GetDemoCompletedMilestoneCount();
        string progressText = $"{completed} / {MainQuestManager.DemoMilestoneTotal} Completed";
        GUI.Label(new Rect(lineX, y + contentHeight - 56f, lineW, 22f), progressText, _progressStyle);
        GUI.Label(new Rect(x, y + contentHeight - 38f, width, 18f), "Ctrl+R = Restart Game", _footerStyle);
        GUI.Label(new Rect(x, y + contentHeight - 22f, width, 18f), "G / ESC 关闭", _footerStyle);
    }

    void DrawGuideRow(float x, float y, float width, float height, GuideRow row, bool complete, bool current)
    {
        string mark = complete ? "\u2611" : "\u25a1";
        GUIStyle titleStyle = current ? _rowTitleCurrentStyle : _rowTitleStyle;

        GUI.Label(new Rect(x, y, 22f, height), mark, _checkStyle);
        GUI.Label(new Rect(x + 24f, y, width - 24f, 20f), row.Title, titleStyle);

        if (!string.IsNullOrEmpty(row.Subtitle))
        {
            GUI.Label(new Rect(x + 24f, y + 20f, width - 24f, 18f), row.Subtitle, _rowSubtitleStyle);
        }
    }

    void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.UpperLeft
        };
        _panelStyle.normal.background = MakeTexture(2, 2, new Color(0.05f, 0.08f, 0.12f, 0.9f));

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _titleStyle.normal.textColor = new Color(0.55f, 0.88f, 1f);

        _checkStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 18,
            alignment = TextAnchor.UpperLeft
        };
        _checkStyle.normal.textColor = new Color(0.5f, 0.95f, 0.65f);

        _rowTitleStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };
        _rowTitleStyle.normal.textColor = new Color(0.85f, 0.9f, 0.95f);

        _rowTitleCurrentStyle = new GUIStyle(_rowTitleStyle);
        _rowTitleCurrentStyle.normal.textColor = new Color(1f, 0.9f, 0.4f);

        _rowSubtitleStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 13,
            fontStyle = FontStyle.Italic
        };
        _rowSubtitleStyle.normal.textColor = new Color(0.62f, 0.72f, 0.82f, 0.9f);

        _progressStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        _progressStyle.normal.textColor = new Color(0.45f, 0.95f, 0.7f);

        _footerStyle = new GUIStyle(_rowSubtitleStyle)
        {
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter
        };

        _stylesReady = true;
    }

    static Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
