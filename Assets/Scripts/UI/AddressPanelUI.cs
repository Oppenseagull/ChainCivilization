using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Address panel toggled with Tab. Demo-style identity card for traveler status.
/// </summary>
public class AddressPanelUI : MonoBehaviour
{
    [SerializeField] string demoAddress = "0xExplorer";

    bool _isOpen;

    GUIStyle _boxStyle;
    GUIStyle _sectionStyle;
    GUIStyle _valueStyle;
    GUIStyle _ruleStyle;
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

        if (GameplayInputGate.BlocksGameplayShortcuts)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            Toggle();
        }

        if (_isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
#else
        if (Input.GetKeyDown(KeyCode.Tab))
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

        TokenManager tokens = TokenManager.Instance;
        ReputationManager reputation = ReputationManager.Instance;
        DAOPassManager passes = DAOPassManager.Instance;

        int moon = tokens != null ? tokens.MoonBalance : 0;
        int rep = reputation != null ? reputation.GetReputation() : 0;
        string passLabel = GetPassLabel(passes);

        CivilizationType civType = CivilizationManager.selectedCivilizationType;
        bool hasCiv = CivilizationManager.HasSelectedCivilization;
        string civilizationLabel = CivilizationBonuses.GetAddressPanelCivilizationName(civType);
        string ruleTagline = hasCiv ? CivilizationBonuses.GetRuleTagline(civType) : string.Empty;
        string ruleBonus = hasCiv ? CivilizationBonuses.GetRuleBonusLine(civType) : string.Empty;

        const float width = 380f;
        const float sectionHeaderHeight = 20f;
        const float valueHeight = 26f;
        const float ruleLineHeight = 22f;
        const float sectionGap = 10f;
        const float paddingTop = 18f;
        const float paddingBottom = 30f;

        float contentHeight = paddingTop;
        contentHeight += sectionHeaderHeight + valueHeight + sectionGap; // Address
        contentHeight += sectionHeaderHeight + valueHeight + sectionGap; // MOON
        contentHeight += sectionHeaderHeight + valueHeight + sectionGap; // Reputation
        contentHeight += sectionHeaderHeight + valueHeight + sectionGap; // Pass
        contentHeight += sectionHeaderHeight + valueHeight + sectionGap; // Civilization
        contentHeight += sectionHeaderHeight;
        if (hasCiv)
        {
            contentHeight += ruleLineHeight * 2f;
        }
        else
        {
            contentHeight += valueHeight;
        }

        contentHeight += paddingBottom;

        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - contentHeight) * 0.5f;

        GUI.Box(new Rect(x, y, width, contentHeight), GUIContent.none, _boxStyle);

        float lineY = y + paddingTop;
        float innerX = x + 20f;
        float innerWidth = width - 40f;

        DrawSection(innerX, ref lineY, innerWidth, sectionHeaderHeight, valueHeight, "Address", demoAddress);
        lineY += sectionGap;
        DrawSection(innerX, ref lineY, innerWidth, sectionHeaderHeight, valueHeight, "MOON", moon.ToString());
        lineY += sectionGap;
        DrawSection(innerX, ref lineY, innerWidth, sectionHeaderHeight, valueHeight, "Reputation", rep.ToString());
        lineY += sectionGap;
        DrawSection(innerX, ref lineY, innerWidth, sectionHeaderHeight, valueHeight, "Pass", passLabel);
        lineY += sectionGap;
        DrawSection(innerX, ref lineY, innerWidth, sectionHeaderHeight, valueHeight, "Civilization", civilizationLabel);
        lineY += sectionGap;

        GUI.Label(new Rect(innerX, lineY, innerWidth, sectionHeaderHeight), "Rule", _sectionStyle);
        lineY += sectionHeaderHeight;

        if (hasCiv)
        {
            GUI.Label(new Rect(innerX, lineY, innerWidth, ruleLineHeight), ruleTagline, _ruleStyle);
            lineY += ruleLineHeight;
            GUI.Label(new Rect(innerX, lineY, innerWidth, ruleLineHeight), ruleBonus, _ruleStyle);
        }
        else
        {
            GUI.Label(new Rect(innerX, lineY, innerWidth, valueHeight), "创建文明后显示规则", _valueStyle);
        }

        GUI.Label(new Rect(x, y + contentHeight - 24f, width, 20f), "Tab / ESC 关闭", _footerStyle);
    }

    void DrawSection(float x, ref float y, float width, float headerHeight, float valueHeight, string header, string value)
    {
        GUI.Label(new Rect(x, y, width, headerHeight), header, _sectionStyle);
        y += headerHeight;
        GUI.Label(new Rect(x, y, width, valueHeight), value, _valueStyle);
        y += valueHeight;
    }

    static string GetPassLabel(DAOPassManager passes)
    {
        if (passes == null)
        {
            return "None";
        }

        if (passes.HasGreenPass)
        {
            return "Green DAO Pass";
        }

        return "None";
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
        _boxStyle.normal.background = MakeTexture(2, 2, new Color(0.06f, 0.09f, 0.14f, 0.94f));

        _sectionStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleLeft
        };
        _sectionStyle.normal.textColor = new Color(0.62f, 0.72f, 0.82f, 0.9f);

        _valueStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _valueStyle.normal.textColor = new Color(0.9f, 0.95f, 1f);

        _ruleStyle = new GUIStyle(_valueStyle)
        {
            fontSize = 18,
            fontStyle = FontStyle.Normal,
            wordWrap = true
        };
        _ruleStyle.normal.textColor = new Color(0.78f, 0.9f, 1f);

        _footerStyle = new GUIStyle(_sectionStyle)
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
