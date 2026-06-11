using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Civilization vision handbook toggled with V. Travel-guide tone, not help docs.
/// </summary>
public class CivilizationVisionPanelUI : MonoBehaviour
{
    bool _isOpen;

    GUIStyle _panelStyle;
    GUIStyle _titleStyle;
    GUIStyle _epigraphStyle;
    GUIStyle _sectionLeadStyle;
    GUIStyle _proseStyle;
    GUIStyle _principleStyle;
    GUIStyle _goalsHeaderStyle;
    GUIStyle _goalStyle;
    GUIStyle _dividerStyle;
    GUIStyle _footerStyle;
    bool _stylesReady;

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

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.vKey.wasPressedThisFrame)
        {
            Toggle();
        }

        if (_isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
#else
        if (Input.GetKeyDown(KeyCode.V))
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

        EnsureStyles();

        const float width = 540f;
        const float paddingTop = 28f;
        const float paddingBottom = 32f;
        const float innerPad = 28f;
        const float lineHeight = 24f;
        const float goalLineHeight = 28f;

        float contentHeight = paddingTop;
        contentHeight += 36f; // title
        contentHeight += 8f + 26f; // epigraph
        contentHeight += 20f; // lead
        contentHeight += lineHeight * 4f; // four prose lines
        contentHeight += 16f;
        contentHeight += lineHeight * 3f; // three principles
        contentHeight += 20f;
        contentHeight += 18f; // divider
        contentHeight += 12f;
        contentHeight += 24f; // goals header
        contentHeight += goalLineHeight * 5f;
        contentHeight += paddingBottom;

        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - contentHeight) * 0.5f;

        GUI.Box(new Rect(x, y, width, contentHeight), GUIContent.none, _panelStyle);

        float lineX = x + innerPad;
        float lineW = width - innerPad * 2f;
        float lineY = y + paddingTop;

        GUI.Label(new Rect(lineX, lineY, lineW, 36f), "Chain Civilization", _titleStyle);
        lineY += 40f;
        GUI.Label(new Rect(lineX, lineY, lineW, 26f), "一个关于 Web3 文明的实验世界。", _epigraphStyle);
        lineY += 34f;

        GUI.Label(new Rect(lineX, lineY, lineW, 20f), "在这里", _sectionLeadStyle);
        lineY += 22f;
        GUI.Label(new Rect(lineX, lineY, lineW, lineHeight), "没有统一国家。", _proseStyle);
        lineY += lineHeight;
        GUI.Label(new Rect(lineX, lineY, lineW, lineHeight), "没有统一规则。", _proseStyle);
        lineY += lineHeight;
        GUI.Label(new Rect(lineX, lineY, lineW, lineHeight), "每个 DAO 拥有自己的价值观。", _proseStyle);
        lineY += lineHeight;
        GUI.Label(new Rect(lineX, lineY, lineW, lineHeight), "每个文明拥有自己的共识。", _proseStyle);
        lineY += lineHeight + 16f;

        GUI.Label(new Rect(lineX, lineY, lineW, lineHeight), "Token 代表激励。", _principleStyle);
        lineY += lineHeight;
        GUI.Label(new Rect(lineX, lineY, lineW, lineHeight), "Pass 代表身份。", _principleStyle);
        lineY += lineHeight;
        GUI.Label(new Rect(lineX, lineY, lineW, lineHeight), "规则决定文明的发展方向。", _principleStyle);
        lineY += lineHeight + 20f;

        GUI.Label(new Rect(lineX, lineY, lineW, 18f), "—  —  —", _dividerStyle);
        lineY += 28f;

        GUI.Label(new Rect(lineX, lineY, lineW, 24f), "玩家目标", _goalsHeaderStyle);
        lineY += 28f;

        DrawGoalLine(lineX, ref lineY, lineW, goalLineHeight, "探索文明");
        DrawGoalLine(lineX, ref lineY, lineW, goalLineHeight, "理解规则");
        DrawGoalLine(lineX, ref lineY, lineW, goalLineHeight, "获得身份");
        DrawGoalLine(lineX, ref lineY, lineW, goalLineHeight, "建立共识");
        DrawGoalLine(lineX, ref lineY, lineW, goalLineHeight, "创造属于自己的文明");

        GUI.Label(new Rect(x, y + contentHeight - 26f, width, 20f), "V / ESC 关闭", _footerStyle);
    }

    void DrawGoalLine(float x, ref float y, float width, float height, string text)
    {
        GUI.Label(new Rect(x, y, 18f, height), "\u00b7", _goalStyle);
        GUI.Label(new Rect(x + 16f, y, width - 16f, height), text, _goalStyle);
        y += height;
    }

    void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.UpperCenter
        };
        _panelStyle.normal.background = MakeTexture(2, 2, new Color(0.09f, 0.07f, 0.05f, 0.94f));

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _titleStyle.normal.textColor = new Color(0.95f, 0.82f, 0.45f);

        _epigraphStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 17,
            fontStyle = FontStyle.Italic
        };
        _epigraphStyle.normal.textColor = new Color(0.82f, 0.76f, 0.62f, 0.95f);

        _sectionLeadStyle = new GUIStyle(_epigraphStyle)
        {
            fontSize = 15,
            alignment = TextAnchor.MiddleLeft
        };
        _sectionLeadStyle.normal.textColor = new Color(0.7f, 0.62f, 0.5f, 0.9f);

        _proseStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 16,
            fontStyle = FontStyle.Normal,
            alignment = TextAnchor.MiddleLeft
        };
        _proseStyle.normal.textColor = new Color(0.88f, 0.84f, 0.76f);

        _principleStyle = new GUIStyle(_proseStyle)
        {
            fontStyle = FontStyle.Italic
        };
        _principleStyle.normal.textColor = new Color(0.78f, 0.88f, 0.82f, 0.95f);

        _goalsHeaderStyle = new GUIStyle(_epigraphStyle)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _goalsHeaderStyle.normal.textColor = new Color(0.92f, 0.78f, 0.42f);

        _goalStyle = new GUIStyle(_proseStyle)
        {
            fontSize = 17
        };
        _goalStyle.normal.textColor = new Color(0.9f, 0.86f, 0.78f);

        _dividerStyle = new GUIStyle(_sectionLeadStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 14
        };

        _footerStyle = new GUIStyle(_sectionLeadStyle)
        {
            fontSize = 13,
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
