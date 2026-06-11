using System.Collections;
using UnityEngine;

/// <summary>
/// Shows civilization creation completion notice; auto-closes after 6 seconds.
/// </summary>
public class CivilizationCompleteNoticeUI : MonoBehaviour
{
    [SerializeField] float delayBeforeShow = 5f;
    [SerializeField] float displayDuration = 6f;

    bool _isShowing;
    float _timer;
    Coroutine _showRoutine;

    GUIStyle _panelStyle;
    GUIStyle _titleStyle;
    GUIStyle _sectionStyle;
    GUIStyle _bodyStyle;
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

        if (_showRoutine != null)
        {
            StopCoroutine(_showRoutine);
        }

        _showRoutine = StartCoroutine(ShowAfterCeremony(type));
    }

    IEnumerator ShowAfterCeremony(CivilizationType type)
    {
        while (CivilizationClimaxUI.IsCeremonyActive)
        {
            yield return null;
        }

        // Ceremony already exceeds the original pre-show delay.
        _timer = displayDuration;
        _isShowing = true;
        _showRoutine = null;
    }

    void OnGUI()
    {
        if (!_isShowing)
        {
            return;
        }

        EnsureStyles();

        const float width = 720f;
        const float height = 320f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * 0.3f;

        GUI.Box(new Rect(x - 28f, y - 24f, width + 56f, height + 48f), GUIContent.none, _panelStyle);

        float lineY = y + 12f;
        const float lineHeight = 34f;

        GUI.Label(new Rect(x, lineY, width, lineHeight), "你已完成第一份文明共识。", _titleStyle);
        lineY += lineHeight + 8f;

        GUI.Label(new Rect(x, lineY, width, lineHeight), "未来：", _sectionStyle);
        lineY += lineHeight;

        GUI.Label(new Rect(x + 24f, lineY, width - 24f, lineHeight), "你可以创造货币。", _bodyStyle);
        lineY += lineHeight - 4f;
        GUI.Label(new Rect(x + 24f, lineY, width - 24f, lineHeight), "你可以制定规则。", _bodyStyle);
        lineY += lineHeight - 4f;
        GUI.Label(new Rect(x + 24f, lineY, width - 24f, lineHeight), "你可以吸引新的居民。", _bodyStyle);
        lineY += lineHeight + 12f;

        GUI.Label(new Rect(x, lineY, width, lineHeight + 8f), "文明已经开始。", _footerStyle);
    }

    void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter
        };
        _panelStyle.normal.background = MakeTexture(2, 2, new Color(0.06f, 0.1f, 0.18f, 0.94f));

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 26,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        _titleStyle.normal.textColor = new Color(1f, 0.92f, 0.45f);

        _sectionStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _sectionStyle.normal.textColor = new Color(0.85f, 0.92f, 1f);

        _bodyStyle = new GUIStyle(_sectionStyle)
        {
            fontSize = 20,
            fontStyle = FontStyle.Normal
        };
        _bodyStyle.normal.textColor = new Color(0.78f, 0.86f, 0.95f);

        _footerStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _footerStyle.normal.textColor = new Color(0.45f, 0.95f, 0.65f);

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
