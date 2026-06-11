using UnityEngine;

/// <summary>
/// Always-visible left-side civilization progress HUD. Reads MainQuestManager only.
/// </summary>
public class CivilizationProgressUI : MonoBehaviour
{
    [SerializeField] float topOffsetWhenQuestActive = 320f;

    MainQuestManager _questManager;

    GUIStyle _boxStyle;
    GUIStyle _titleStyle;
    GUIStyle _rowCompleteStyle;
    GUIStyle _rowPendingStyle;
    GUIStyle _completionHeaderStyle;
    GUIStyle _completionValueStyle;
    bool _stylesReady;

    void Awake()
    {
        _questManager = GetComponent<MainQuestManager>();
        if (_questManager == null)
        {
            _questManager = FindFirstObjectByType<MainQuestManager>();
        }
    }

    void OnGUI()
    {
        if (GameHUDCanvas.IsActive)
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

        const float width = 220f;
        const float margin = 12f;
        const float rowHeight = 22f;
        const float headerHeight = 24f;
        const float completionBlock = 44f;
        const float paddingTop = 8f;
        const float paddingBottom = 10f;

        float height = paddingTop + headerHeight + MainQuestManager.CivilizationProgressTotal * rowHeight
            + completionBlock + paddingBottom;

        float x = margin;
        float y = margin;
        if (!_questManager.IsAllQuestsComplete)
        {
            y += topOffsetWhenQuestActive;
        }

        GUI.Box(new Rect(x, y, width, height), GUIContent.none, _boxStyle);

        float lineX = x + 10f;
        float lineW = width - 20f;
        float lineY = y + paddingTop;

        GUI.Label(new Rect(lineX, lineY, lineW, headerHeight), "文明探索进度", _titleStyle);
        lineY += headerHeight;

        for (int i = 0; i < MainQuestManager.CivilizationProgressTotal; i++)
        {
            bool complete = _questManager.IsCivilizationProgressComplete(i);
            string label = MainQuestManager.GetCivilizationProgressLabel(i);
            string mark = complete ? "\u2713" : " ";
            GUIStyle rowStyle = complete ? _rowCompleteStyle : _rowPendingStyle;
            GUI.Label(new Rect(lineX, lineY, lineW, rowHeight), $"[{mark}] {label}", rowStyle);
            lineY += rowHeight;
        }

        lineY += 4f;
        GUI.Label(new Rect(lineX, lineY, lineW, 18f), "Completion", _completionHeaderStyle);
        lineY += 18f;
        GUI.Label(new Rect(lineX, lineY, lineW, 22f), $"{_questManager.GetCivilizationProgressPercent()}%", _completionValueStyle);
    }

    void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _boxStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.UpperLeft
        };
        _boxStyle.normal.background = MakeTexture(2, 2, new Color(0.06f, 0.08f, 0.12f, 0.72f));

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _titleStyle.normal.textColor = new Color(0.82f, 0.9f, 1f);

        _rowCompleteStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 13,
            fontStyle = FontStyle.Bold
        };
        _rowCompleteStyle.normal.textColor = new Color(0.4f, 0.92f, 0.55f);

        _rowPendingStyle = new GUIStyle(_rowCompleteStyle);
        _rowPendingStyle.normal.textColor = new Color(0.55f, 0.58f, 0.62f);

        _completionHeaderStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 12,
            fontStyle = FontStyle.Italic
        };
        _completionHeaderStyle.normal.textColor = new Color(0.62f, 0.72f, 0.82f, 0.95f);

        _completionValueStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 18,
            fontStyle = FontStyle.Bold
        };
        _completionValueStyle.normal.textColor = new Color(1f, 0.9f, 0.45f);

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
