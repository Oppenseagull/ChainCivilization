using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Quest Log toggled with F. Shows completed, current, and future main quests.
/// </summary>
public class QuestLogUI : MonoBehaviour
{
    MainQuestManager _questManager;
    bool _isOpen;

    GUIStyle _panelStyle;
    GUIStyle _titleStyle;
    GUIStyle _sectionStyle;
    GUIStyle _completedStyle;
    GUIStyle _currentStyle;
    GUIStyle _futureStyle;
    GUIStyle _stepsStyle;
    GUIStyle _nextActionStyle;
    GUIStyle _hintStyle;
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
        if (GameplayInputGate.BlocksGameplayShortcuts)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            Toggle();
        }

        if (_isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
#else
        if (Input.GetKeyDown(KeyCode.F))
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

        const float width = 560f;
        const float height = 620f;
        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - height) * 0.5f;

        GUI.Box(new Rect(x - 24f, y - 24f, width + 48f, height + 48f), GUIContent.none, _panelStyle);
        GUI.Label(new Rect(x, y + 8f, width, 32f), "Quest Log", _titleStyle);

        float lineY = y + 52f;
        const float lineHeight = 24f;
        const float sectionGap = 12f;
        float contentWidth = width - 56f;

        lineY = DrawSectionHeader(x, width, lineY, lineHeight, "已完成任务", _sectionStyle);
        lineY = DrawQuestRange(x, width, lineY, lineHeight, 0, GetCompletedEndIndex(), _completedStyle, "- ");
        lineY += sectionGap;

        lineY = DrawSectionHeader(x, width, lineY, lineHeight, "当前任务", _sectionStyle);
        lineY = DrawCurrentQuest(x, width, contentWidth, lineY, lineHeight);
        lineY += sectionGap;

        lineY = DrawSectionHeader(x, width, lineY, lineHeight, "未来任务", _sectionStyle);
        DrawQuestRange(x, width, lineY, lineHeight, GetFutureStartIndex(), MainQuestManager.QuestStepCount, _futureStyle, "- ");

        GUI.Label(new Rect(x, y + height - 28f, width, 22f), "按 F 或 ESC 关闭", _hintStyle);
    }

    int GetCompletedEndIndex()
    {
        if (_questManager.IsAllQuestsComplete)
        {
            return MainQuestManager.QuestStepCount;
        }

        return _questManager.CurrentQuestIndex;
    }

    int GetFutureStartIndex()
    {
        if (_questManager.IsAllQuestsComplete)
        {
            return MainQuestManager.QuestStepCount;
        }

        return _questManager.CurrentQuestIndex + 1;
    }

    float DrawSectionHeader(float x, float width, float lineY, float lineHeight, string text, GUIStyle style)
    {
        GUI.Label(new Rect(x + 16f, lineY, width - 32f, lineHeight + 4f), text, style);
        return lineY + lineHeight + 8f;
    }

    float DrawCurrentQuest(float x, float width, float contentWidth, float lineY, float lineHeight)
    {
        if (_questManager.IsAllQuestsComplete)
        {
            GUI.Label(new Rect(x + 28f, lineY, contentWidth, lineHeight), "（全部完成）", _currentStyle);
            return lineY + lineHeight;
        }

        int index = _questManager.CurrentQuestIndex;
        string title = MainQuestManager.GetQuestTitle(index);
        GUI.Label(new Rect(x + 28f, lineY, contentWidth, lineHeight), $"> {title}", _currentStyle);
        lineY += lineHeight + 4f;

        string description = MainQuestManager.GetQuestDescription(index);
        GUI.Label(new Rect(x + 40f, lineY, contentWidth, lineHeight), description, _futureStyle);
        lineY += lineHeight + 6f;

        string stepsText = MainQuestManager.FormatGuideSteps(index, true);
        float stepsHeight = _stepsStyle.CalcHeight(new GUIContent(stepsText), contentWidth);
        GUI.Label(new Rect(x + 40f, lineY, contentWidth, stepsHeight), stepsText, _stepsStyle);
        lineY += stepsHeight + 8f;

        string nextAction = _questManager.GetQuestNextAction(index);
        if (!string.IsNullOrEmpty(nextAction))
        {
            GUI.Label(new Rect(x + 40f, lineY, contentWidth, lineHeight), "【下一步】", _sectionStyle);
            lineY += lineHeight;
            float nextHeight = _nextActionStyle.CalcHeight(new GUIContent(nextAction), contentWidth);
            GUI.Label(new Rect(x + 40f, lineY, contentWidth, nextHeight), nextAction, _nextActionStyle);
            lineY += nextHeight;
        }

        return lineY;
    }

    float DrawQuestRange(float x, float width, float lineY, float lineHeight, int startIndex, int endIndex, GUIStyle style, string prefix)
    {
        if (startIndex >= endIndex)
        {
            GUI.Label(new Rect(x + 28f, lineY, width - 44f, lineHeight), "（无）", style);
            return lineY + lineHeight;
        }

        for (int i = startIndex; i < endIndex; i++)
        {
            string title = MainQuestManager.GetQuestTitle(i);
            GUI.Label(new Rect(x + 28f, lineY, width - 44f, lineHeight), $"{prefix}{title}", style);
            lineY += lineHeight;
        }

        return lineY;
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
        _panelStyle.normal.background = MakeTexture(2, 2, new Color(0.05f, 0.08f, 0.14f, 0.94f));

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 26,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _titleStyle.normal.textColor = new Color(1f, 0.92f, 0.45f);

        _sectionStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 18,
            alignment = TextAnchor.MiddleLeft
        };
        _sectionStyle.normal.textColor = new Color(0.85f, 0.92f, 1f);

        _completedStyle = new GUIStyle(_sectionStyle)
        {
            fontSize = 16,
            fontStyle = FontStyle.Normal
        };
        _completedStyle.normal.textColor = new Color(0.55f, 0.75f, 0.55f);

        _currentStyle = new GUIStyle(_completedStyle)
        {
            fontStyle = FontStyle.Bold
        };
        _currentStyle.normal.textColor = new Color(1f, 0.92f, 0.45f);

        _futureStyle = new GUIStyle(_completedStyle);
        _futureStyle.normal.textColor = new Color(0.65f, 0.72f, 0.82f);

        _stepsStyle = new GUIStyle(_completedStyle)
        {
            fontSize = 14,
            wordWrap = true
        };
        _stepsStyle.normal.textColor = new Color(0.72f, 0.8f, 0.9f);

        _nextActionStyle = new GUIStyle(_stepsStyle)
        {
            fontStyle = FontStyle.Bold
        };
        _nextActionStyle.normal.textColor = new Color(0.55f, 0.95f, 0.75f);

        _hintStyle = new GUIStyle(_completedStyle)
        {
            fontSize = 14,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter
        };
        _hintStyle.normal.textColor = new Color(0.7f, 0.78f, 0.88f, 0.85f);

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
