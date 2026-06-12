using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Civilization summary popup after creation. Dismiss with E.
/// </summary>
public class CivilizationSummaryUI : MonoBehaviour
{
    bool _isShowing;
    CivilizationType _shownType;
    Coroutine _showRoutine;

    GUIStyle _panelStyle;
    GUIStyle _headerStyle;
    GUIStyle _choiceStyle;
    GUIStyle _sectionStyle;
    GUIStyle _bodyStyle;
    GUIStyle _advantageStyle;
    GUIStyle _riskStyle;
    GUIStyle _footerStyle;
    bool _stylesReady;

    void OnEnable()
    {
        CivilizationManager.OnCivilizationSelected += HandleCivilizationSelected;
    }

    void OnDisable()
    {
        CivilizationManager.OnCivilizationSelected -= HandleCivilizationSelected;

        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
            _showRoutine = null;
        }

        _isShowing = false;
    }

    void Update()
    {
        if (!_isShowing)
        {
            return;
        }

        if (GameplayInputGate.BlocksGameplayShortcuts)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            _isShowing = false;
        }
#else
        if (Input.GetKeyDown(KeyCode.E))
        {
            _isShowing = false;
        }
#endif
    }

    void HandleCivilizationSelected(CivilizationType type)
    {
        if (type == CivilizationType.None)
        {
            return;
        }

        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
        }

        _showRoutine = StartCoroutine(ShowWhenReady(type));
    }

    IEnumerator ShowWhenReady(CivilizationType type)
    {
        while (CivilizationClimaxUI.IsCeremonyActive)
        {
            yield return null;
        }

        _shownType = type;
        _isShowing = true;
        _showRoutine = null;
    }

    void OnGUI()
    {
        if (!_isShowing)
        {
            return;
        }

        EnsureStyles(_shownType);

        CivilizationSummaryContent.SummaryData summary = CivilizationSummaryContent.GetSummary(_shownType);
        if (string.IsNullOrEmpty(summary.ChoiceLabel))
        {
            return;
        }

        const float width = 700f;
        const float lineHeight = 30f;
        const float sectionGap = 8f;
        const float paddingTop = 24f;
        const float paddingBottom = 34f;

        int evalLines = summary.WorldEvaluation != null ? summary.WorldEvaluation.Length : 0;
        float contentHeight = paddingTop;
        contentHeight += lineHeight; // 你选择了
        contentHeight += lineHeight + sectionGap; // choice
        contentHeight += lineHeight; // 世界评价 header
        contentHeight += evalLines * lineHeight;
        contentHeight += sectionGap;
        contentHeight += lineHeight + lineHeight; // 优势
        contentHeight += sectionGap;
        contentHeight += lineHeight + lineHeight; // 风险
        contentHeight += paddingBottom;

        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - contentHeight) * 0.5f;

        GUI.Box(new Rect(x - 28f, y - 20f, width + 56f, contentHeight + 40f), GUIContent.none, _panelStyle);

        float lineY = y + paddingTop;

        GUI.Label(new Rect(x, lineY, width, lineHeight), "你选择了：", _headerStyle);
        lineY += lineHeight;
        GUI.Label(new Rect(x, lineY, width, lineHeight), summary.ChoiceLabel, _choiceStyle);
        lineY += lineHeight + sectionGap;

        GUI.Label(new Rect(x, lineY, width, lineHeight), "世界评价：", _sectionStyle);
        lineY += lineHeight;
        for (int i = 0; i < evalLines; i++)
        {
            GUI.Label(new Rect(x + 12f, lineY, width - 12f, lineHeight), summary.WorldEvaluation[i], _bodyStyle);
            lineY += lineHeight;
        }

        lineY += sectionGap;
        GUI.Label(new Rect(x, lineY, width, lineHeight), "优势：", _sectionStyle);
        lineY += lineHeight;
        GUI.Label(new Rect(x + 12f, lineY, width - 12f, lineHeight), summary.Advantage, _advantageStyle);
        lineY += lineHeight + sectionGap;

        GUI.Label(new Rect(x, lineY, width, lineHeight), "风险：", _sectionStyle);
        lineY += lineHeight;
        GUI.Label(new Rect(x + 12f, lineY, width - 12f, lineHeight), summary.Risk, _riskStyle);

        GUI.Label(new Rect(x, y + contentHeight + 8f, width, 24f), "按 E 关闭", _footerStyle);
    }

    void EnsureStyles(CivilizationType type)
    {
        Color accent = GetAccentColor(type);

        if (_stylesReady)
        {
            _choiceStyle.normal.textColor = accent;
            _advantageStyle.normal.textColor = accent;
            return;
        }

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.UpperCenter
        };
        _panelStyle.normal.background = MakeTexture(2, 2, new Color(0.05f, 0.09f, 0.16f, 0.95f));

        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _headerStyle.normal.textColor = new Color(0.82f, 0.9f, 1f);

        _choiceStyle = new GUIStyle(_headerStyle)
        {
            fontSize = 30,
            alignment = TextAnchor.MiddleCenter
        };
        _choiceStyle.normal.textColor = accent;

        _sectionStyle = new GUIStyle(_headerStyle)
        {
            fontSize = 20
        };
        _sectionStyle.normal.textColor = new Color(0.9f, 0.94f, 1f);

        _bodyStyle = new GUIStyle(_sectionStyle)
        {
            fontSize = 18,
            fontStyle = FontStyle.Normal
        };
        _bodyStyle.normal.textColor = new Color(0.78f, 0.86f, 0.95f);

        _advantageStyle = new GUIStyle(_bodyStyle);
        _advantageStyle.normal.textColor = accent;

        _riskStyle = new GUIStyle(_bodyStyle);
        _riskStyle.normal.textColor = new Color(1f, 0.62f, 0.5f);

        _footerStyle = new GUIStyle(_headerStyle)
        {
            fontSize = 16,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter
        };
        _footerStyle.normal.textColor = new Color(0.65f, 0.78f, 0.9f, 0.9f);

        _stylesReady = true;
    }

    static Color GetAccentColor(CivilizationType type)
    {
        switch (type)
        {
            case CivilizationType.OpenDAO:
                return new Color(0.45f, 0.75f, 1f);
            case CivilizationType.TradeDAO:
                return new Color(1f, 0.55f, 0.45f);
            case CivilizationType.KnowledgeDAO:
                return new Color(0.45f, 1f, 0.55f);
            default:
                return Color.white;
        }
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
