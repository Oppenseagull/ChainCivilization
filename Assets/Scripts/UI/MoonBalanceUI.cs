using UnityEngine;

/// <summary>
/// Top-right Civilization Status HUD (MOON, REP, PASS, civilization) plus reward toasts.
/// </summary>
public class MoonBalanceUI : MonoBehaviour
{
    GUIStyle _headerStyle;
    GUIStyle _moonStyle;
    GUIStyle _repStyle;
    GUIStyle _passStyle;
    GUIStyle _civilizationStyle;
    GUIStyle _boxStyle;
    GUIStyle _rewardStyle;
    GUIStyle _credentialHeaderStyle;
    GUIStyle _credentialNameStyle;
    GUIStyle _credentialPanelStyle;
    GUIStyle _reputationPopupPanelStyle;
    GUIStyle _reputationPopupLine1Style;
    GUIStyle _reputationPopupLine2Style;
    GUIStyle _reputationFloatingStyle;
    bool _stylesReady;

    void OnGUI()
    {
        EnsureStyles();

        if (!GameHUDCanvas.IsActive)
        {
            DrawCivilizationStatusHud();
        }

        TokenManager tokens = TokenManager.Instance;
        ReputationManager reputation = ReputationManager.Instance;

        if (tokens != null && tokens.IsShowingRewardPopup)
        {
            const float popupWidth = 420f;
            float popupX = (Screen.width - popupWidth) * 0.5f;
            float popupY = Screen.height * 0.42f;
            GUI.Label(new Rect(popupX, popupY, popupWidth, 56f), tokens.RewardPopupText, _rewardStyle);
        }

        if (reputation != null && reputation.IsShowingReputationPopup)
        {
            const float popupWidth = 520f;
            const float popupHeight = 120f;
            float popupX = (Screen.width - popupWidth) * 0.5f;
            float popupY = Screen.height * 0.36f;

            GUI.Box(new Rect(popupX - 24f, popupY - 20f, popupWidth + 48f, popupHeight), GUIContent.none, _reputationPopupPanelStyle);
            GUI.Label(new Rect(popupX, popupY, popupWidth, 40f), reputation.ReputationPopupLine1, _reputationPopupLine1Style);
            GUI.Label(new Rect(popupX, popupY + 42f, popupWidth, 44f), reputation.ReputationPopupLine2, _reputationPopupLine2Style);
        }

        DrawReputationFloatingToast(reputation);

        DAOPassManager passes = DAOPassManager.Instance;
        if (passes != null && passes.IsShowingCredentialPopup)
        {
            const float popupWidth = 520f;
            const float popupHeight = 120f;
            float popupX = (Screen.width - popupWidth) * 0.5f;
            float popupY = Screen.height * 0.36f;

            GUI.Box(new Rect(popupX - 24f, popupY - 20f, popupWidth + 48f, popupHeight), GUIContent.none, _credentialPanelStyle);
            GUI.Label(new Rect(popupX, popupY, popupWidth, 36f), "New Credential:", _credentialHeaderStyle);
            GUI.Label(new Rect(popupX, popupY + 38f, popupWidth, 48f), passes.CredentialPassName, _credentialNameStyle);
        }
    }

    void DrawCivilizationStatusHud()
    {
        TokenManager tokens = TokenManager.Instance;
        ReputationManager reputation = ReputationManager.Instance;
        DAOPassManager passes = DAOPassManager.Instance;

        int moon = tokens != null ? tokens.MoonBalance : 0;
        int rep = reputation != null ? reputation.GetReputation() : 0;
        string passLabel = GetPassHudLabel(passes);
        string civilizationLabel = GetCivilizationHudLabel();

        const float margin = 12f;
        const float width = 210f;
        const float headerHeight = 18f;
        const float rowHeight = 22f;
        const float paddingTop = 6f;
        const float paddingBottom = 8f;
        const float paddingH = 10f;

        float hudHeight = paddingTop + headerHeight + rowHeight * 4f + paddingBottom;
        float x = Screen.width - width - margin;
        float y = margin;

        GUI.Box(new Rect(x, y, width, hudHeight), GUIContent.none, _boxStyle);

        float lineY = y + paddingTop;
        float lineW = width - paddingH * 2f;
        float lineX = x + paddingH;

        GUI.Label(new Rect(lineX, lineY, lineW, headerHeight), "Civilization Status", _headerStyle);
        lineY += headerHeight;

        GUI.Label(new Rect(lineX, lineY, lineW, rowHeight), $"MOON {moon}", _moonStyle);
        lineY += rowHeight;
        GUI.Label(new Rect(lineX, lineY, lineW, rowHeight), $"REP {rep}", _repStyle);
        lineY += rowHeight;
        GUI.Label(new Rect(lineX, lineY, lineW, rowHeight), $"PASS {passLabel}", _passStyle);
        lineY += rowHeight;
        GUI.Label(new Rect(lineX, lineY, lineW, rowHeight), $"Civilization {civilizationLabel}", _civilizationStyle);
    }

    static string GetPassHudLabel(DAOPassManager passes)
    {
        if (passes != null && passes.HasGreenPass)
        {
            return "Green";
        }

        return "None";
    }

    static string GetCivilizationHudLabel()
    {
        if (!CivilizationManager.HasSelectedCivilization)
        {
            return "None";
        }

        return CivilizationBonuses.GetAddressPanelCivilizationName(CivilizationManager.selectedCivilizationType);
    }

    void DrawReputationFloatingToast(ReputationManager reputation)
    {
        if (reputation == null || !reputation.IsShowingFloatingToast)
        {
            return;
        }

        float alpha = reputation.FloatingToastAlpha;
        Color textColor = _reputationFloatingStyle.normal.textColor;
        textColor.a = alpha;

        Color previousColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, alpha);

        const float width = 520f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * 0.44f;
        string text = $"REPUTATION +{reputation.FloatingToastAmount}";

        _reputationFloatingStyle.normal.textColor = textColor;
        GUI.Label(new Rect(x, y, width, 56f), text, _reputationFloatingStyle);
        _reputationFloatingStyle.normal.textColor = new Color(0.35f, 0.95f, 0.78f);

        GUI.color = previousColor;
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

        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 11,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleLeft
        };
        _headerStyle.normal.textColor = new Color(0.62f, 0.72f, 0.82f, 0.95f);

        _moonStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _moonStyle.normal.textColor = new Color(1f, 0.88f, 0.35f);

        _repStyle = new GUIStyle(_moonStyle);
        _repStyle.normal.textColor = new Color(0.35f, 0.95f, 0.78f);

        _passStyle = new GUIStyle(_moonStyle)
        {
            fontSize = 15
        };
        _passStyle.normal.textColor = new Color(0.55f, 0.95f, 0.55f);

        _civilizationStyle = new GUIStyle(_passStyle);
        _civilizationStyle.normal.textColor = new Color(0.82f, 0.9f, 1f);

        _rewardStyle = new GUIStyle(_moonStyle)
        {
            fontSize = 36,
            alignment = TextAnchor.MiddleCenter
        };
        _rewardStyle.normal.textColor = new Color(0.55f, 0.85f, 1f);

        _credentialPanelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter
        };
        _credentialPanelStyle.normal.background = MakeTexture(2, 2, new Color(0.04f, 0.14f, 0.08f, 0.92f));

        _credentialHeaderStyle = new GUIStyle(_moonStyle)
        {
            fontSize = 24,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter
        };
        _credentialHeaderStyle.normal.textColor = new Color(0.75f, 0.95f, 0.78f);

        _credentialNameStyle = new GUIStyle(_moonStyle)
        {
            fontSize = 34,
            alignment = TextAnchor.MiddleCenter
        };
        _credentialNameStyle.normal.textColor = new Color(0.45f, 1f, 0.55f);

        _reputationPopupPanelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter
        };
        _reputationPopupPanelStyle.normal.background = MakeTexture(2, 2, new Color(0.04f, 0.12f, 0.14f, 0.92f));

        _reputationPopupLine1Style = new GUIStyle(_moonStyle)
        {
            fontSize = 24,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        _reputationPopupLine1Style.normal.textColor = new Color(0.85f, 0.95f, 1f);

        _reputationPopupLine2Style = new GUIStyle(_reputationPopupLine1Style)
        {
            fontSize = 32,
            fontStyle = FontStyle.Bold
        };
        _reputationPopupLine2Style.normal.textColor = new Color(0.35f, 0.95f, 0.78f);

        _reputationFloatingStyle = new GUIStyle(_moonStyle)
        {
            fontSize = 42,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _reputationFloatingStyle.normal.textColor = new Color(0.35f, 0.95f, 0.78f);

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
