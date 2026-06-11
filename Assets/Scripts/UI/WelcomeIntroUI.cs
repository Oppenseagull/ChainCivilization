using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// First-launch welcome intro. Shown once, dismissed with E. Persisted via PlayerPrefs.
/// </summary>
public class WelcomeIntroUI : MonoBehaviour
{
    const string SeenKey = "chain_civ_welcome_seen";

    bool _isShowing;
    float _previousTimeScale = 1f;

    GUIStyle _panelStyle;
    GUIStyle _titleStyle;
    GUIStyle _bodyStyle;
    GUIStyle _sectionStyle;
    GUIStyle _controlKeyStyle;
    GUIStyle _controlActionStyle;
    GUIStyle _footerStyle;
    Texture2D _overlayTexture;
    Texture2D _panelTexture;
    bool _stylesReady;

    void Start()
    {
        if (PlayerPrefs.GetInt(SeenKey, 0) == 1)
        {
            return;
        }

        _isShowing = true;
        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!_isShowing)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Dismiss();
        }
#else
        if (Input.GetKeyDown(KeyCode.E))
        {
            Dismiss();
        }
#endif
    }

    void OnDisable()
    {
        if (_isShowing)
        {
            RestoreGameplay();
            _isShowing = false;
        }
    }

    void Dismiss()
    {
        PlayerPrefs.SetInt(SeenKey, 1);
        PlayerPrefs.Save();
        RestoreGameplay();
        _isShowing = false;
    }

    void RestoreGameplay()
    {
        Time.timeScale = _previousTimeScale > 0f ? _previousTimeScale : 1f;
    }

    void OnGUI()
    {
        if (!_isShowing)
        {
            return;
        }

        EnsureStyles();

        GUI.color = new Color(0.02f, 0.04f, 0.1f, 0.82f);
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _overlayTexture);
        GUI.color = Color.white;

        const float width = 560f;
        const float height = 520f;
        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - height) * 0.5f;

        GUI.Box(new Rect(x, y, width, height), GUIContent.none, _panelStyle);

        float lineX = x + 28f;
        float lineW = width - 56f;
        float lineY = y + 24f;
        const float lineH = 26f;

        GUI.Label(new Rect(lineX, lineY, lineW, 36f), "欢迎来到 Chain Civilization", _titleStyle);
        lineY += 42f;
        GUI.Label(new Rect(lineX, lineY, lineW, lineH), "这是一个关于 Web3 文明的世界。", _bodyStyle);
        lineY += lineH + 14f;

        GUI.Label(new Rect(lineX, lineY, lineW, lineH), "你将：", _sectionStyle);
        lineY += lineH;
        GUI.Label(new Rect(lineX + 8f, lineY, lineW - 8f, lineH), "探索 DAO", _bodyStyle);
        lineY += lineH - 2f;
        GUI.Label(new Rect(lineX + 8f, lineY, lineW - 8f, lineH), "获得 Token", _bodyStyle);
        lineY += lineH - 2f;
        GUI.Label(new Rect(lineX + 8f, lineY, lineW - 8f, lineH), "获取身份", _bodyStyle);
        lineY += lineH - 2f;
        GUI.Label(new Rect(lineX + 8f, lineY, lineW - 8f, lineH), "理解规则", _bodyStyle);
        lineY += lineH - 2f;
        GUI.Label(new Rect(lineX + 8f, lineY, lineW - 8f, lineH), "建立自己的文明", _bodyStyle);
        lineY += lineH + 14f;

        GUI.Label(new Rect(lineX, lineY, lineW, lineH), "操作：", _sectionStyle);
        lineY += lineH;
        DrawControlLine(lineX, ref lineY, lineW, lineH, "WASD", "移动");
        DrawControlLine(lineX, ref lineY, lineW, lineH, "Shift", "奔跑");
        DrawControlLine(lineX, ref lineY, lineW, lineH, "Space", "跳跃");
        DrawControlLine(lineX, ref lineY, lineW, lineH, "E", "交互");
        DrawControlLine(lineX, ref lineY, lineW, lineH, "Tab", "身份面板");
        DrawControlLine(lineX, ref lineY, lineW, lineH, "F", "任务日志");
        DrawControlLine(lineX, ref lineY, lineW, lineH, "G", "Demo 流程");
        DrawControlLine(lineX, ref lineY, lineW, lineH, "V", "文明理念");
        lineY += 10f;

        GUI.Label(new Rect(lineX, y + height - 44f, lineW, 32f), "按 E 开始探索", _footerStyle);
    }

    void DrawControlLine(float x, ref float y, float width, float height, string key, string action)
    {
        float keyWidth = 72f;
        GUI.Label(new Rect(x + 8f, y, keyWidth, height), key, _controlKeyStyle);
        GUI.Label(new Rect(x + 8f + keyWidth, y, width - keyWidth - 8f, height), action, _controlActionStyle);
        y += height - 2f;
    }

    void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _overlayTexture = MakeTexture(2, 2, Color.white);
        _panelTexture = MakeTexture(2, 2, new Color(0.06f, 0.09f, 0.15f, 0.96f));

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.UpperLeft
        };
        _panelStyle.normal.background = _panelTexture;

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _titleStyle.normal.textColor = new Color(0.95f, 0.85f, 0.45f);

        _bodyStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 17,
            alignment = TextAnchor.MiddleLeft
        };
        _bodyStyle.normal.textColor = new Color(0.86f, 0.9f, 0.96f);

        _sectionStyle = new GUIStyle(_bodyStyle)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };
        _sectionStyle.normal.textColor = new Color(0.72f, 0.82f, 0.92f);

        _controlKeyStyle = new GUIStyle(_bodyStyle)
        {
            fontSize = 15,
            fontStyle = FontStyle.Bold
        };
        _controlKeyStyle.normal.textColor = new Color(0.55f, 0.88f, 1f);

        _controlActionStyle = new GUIStyle(_bodyStyle)
        {
            fontSize = 15
        };
        _controlActionStyle.normal.textColor = new Color(0.78f, 0.84f, 0.92f);

        _footerStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 22,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter
        };
        _footerStyle.normal.textColor = new Color(0.55f, 0.92f, 0.75f);

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
