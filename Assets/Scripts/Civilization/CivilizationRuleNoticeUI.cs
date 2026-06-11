using UnityEngine;

/// <summary>
/// Shows the first civilization rule notice for 5 seconds after selection.
/// </summary>
public class CivilizationRuleNoticeUI : MonoBehaviour
{
    [SerializeField] float displayDuration = 5f;

    float _timer;
    CivilizationType _shownType;
    bool _isShowing;

    GUIStyle _panelStyle;
    GUIStyle _headerStyle;
    GUIStyle _ruleStyle;
    bool _stylesReady;

    void OnEnable()
    {
        CivilizationManager.OnCivilizationSelected += HandleCivilizationSelected;
    }

    void OnDisable()
    {
        CivilizationManager.OnCivilizationSelected -= HandleCivilizationSelected;
    }

    void Update()
    {
        if (!_isShowing)
        {
            return;
        }

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
        {
            _isShowing = false;
        }
    }

    void HandleCivilizationSelected(CivilizationType type)
    {
        if (type == CivilizationType.None)
        {
            return;
        }

        // Rule notice is shown inside CivilizationClimaxUI ceremony (Step 5).
    }

    void OnGUI()
    {
        if (!_isShowing)
        {
            return;
        }

        EnsureStyles(_shownType);

        const float width = 680f;
        const float height = 140f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * 0.38f;

        GUI.Box(new Rect(x - 24f, y - 20f, width + 48f, height), GUIContent.none, _panelStyle);
        GUI.Label(new Rect(x, y + 8f, width, 36f), "第一条规则已写入：", _headerStyle);
        GUI.Label(new Rect(x, y + 52f, width, 48f), CivilizationManager.GetFirstRuleText(_shownType), _ruleStyle);
    }

    void EnsureStyles(CivilizationType type)
    {
        if (_stylesReady)
        {
            _ruleStyle.normal.textColor = GetAccentColor(type);
            return;
        }

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter
        };
        _panelStyle.normal.background = MakeTexture(2, 2, new Color(0.06f, 0.1f, 0.18f, 0.92f));

        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _headerStyle.normal.textColor = new Color(0.85f, 0.92f, 1f);

        _ruleStyle = new GUIStyle(_headerStyle)
        {
            fontSize = 28,
            fontStyle = FontStyle.Normal,
            wordWrap = true
        };
        _ruleStyle.normal.textColor = GetAccentColor(type);

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
