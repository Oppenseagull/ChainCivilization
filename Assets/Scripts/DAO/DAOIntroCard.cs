using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// One-time DAO introduction cards. Dismiss with E.
/// </summary>
public static class DAOIntroCard
{
    public enum Kind
    {
        Blue,
        Red,
        Green
    }

    struct IntroContent
    {
        public string Title;
        public string Tagline;
        public string Description;
        public Color Accent;
        public Color PanelTint;
    }

    static GUIStyle _panelStyle;
    static GUIStyle _titleStyle;
    static GUIStyle _taglineStyle;
    static GUIStyle _descriptionStyle;
    static GUIStyle _footerStyle;
    static bool _stylesReady;

    public static bool HasSeen(Kind kind)
    {
        return PlayerPrefs.GetInt(GetPrefsKey(kind), 0) == 1;
    }

    public static void MarkSeen(Kind kind)
    {
        PlayerPrefs.SetInt(GetPrefsKey(kind), 1);
        PlayerPrefs.Save();
        CivilizationJournalLog.RecordDaoVisited(ToJournalDaoKind(kind));
    }

    static CivilizationJournalLog.DaoVisitKind ToJournalDaoKind(Kind kind)
    {
        switch (kind)
        {
            case Kind.Blue:
                return CivilizationJournalLog.DaoVisitKind.Blue;
            case Kind.Red:
                return CivilizationJournalLog.DaoVisitKind.Red;
            case Kind.Green:
                return CivilizationJournalLog.DaoVisitKind.Green;
            default:
                return CivilizationJournalLog.DaoVisitKind.Blue;
        }
    }

    public static string GetPrefsKey(Kind kind)
    {
        switch (kind)
        {
            case Kind.Blue:
                return GameSaveKeys.DaoIntroBlueSeen;
            case Kind.Red:
                return GameSaveKeys.DaoIntroRedSeen;
            case Kind.Green:
                return GameSaveKeys.DaoIntroGreenSeen;
            default:
                return "dao_intro_unknown_seen";
        }
    }

    public static bool TryDismissOnInteract(Kind kind, ref bool showingIntro)
    {
        if (!showingIntro)
        {
            return false;
        }

        if (GameplayInputGate.BlocksGameplayShortcuts)
        {
            return true;
        }

#if ENABLE_INPUT_SYSTEM
        bool pressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        bool pressed = Input.GetKeyDown(KeyCode.E);
#endif

        if (!pressed)
        {
            return true;
        }

        MarkSeen(kind);
        showingIntro = false;
        return false;
    }

    public static void Draw(Kind kind)
    {
        IntroContent content = GetContent(kind);
        EnsureStyles(content);

        const float width = 520f;
        const float height = 200f;
        float x = (Screen.width - width) * 0.5f;
        float y = (Screen.height - height) * 0.5f;

        GUI.Box(new Rect(x - 24f, y - 20f, width + 48f, height + 40f), GUIContent.none, _panelStyle);

        Color previousColor = GUI.color;
        GUI.color = content.Accent;
        GUI.Label(new Rect(x, y + 12f, width, 40f), content.Title, _titleStyle);
        GUI.color = previousColor;

        GUI.Label(new Rect(x, y + 56f, width, 36f), content.Tagline, _taglineStyle);
        GUI.Label(new Rect(x, y + 96f, width, 40f), content.Description, _descriptionStyle);
        GUI.Label(new Rect(x, y + height - 28f, width, 24f), "Press E to close", _footerStyle);
    }

    static IntroContent GetContent(Kind kind)
    {
        switch (kind)
        {
            case Kind.Blue:
                return new IntroContent
                {
                    Title = "Blue DAO",
                    Tagline = "Open Collaboration",
                    Description = "Anyone can contribute and earn reputation.",
                    Accent = new Color(0.45f, 0.75f, 1f),
                    PanelTint = new Color(0.05f, 0.08f, 0.18f, 0.94f)
                };
            case Kind.Red:
                return new IntroContent
                {
                    Title = "Red DAO",
                    Tagline = "Market Civilization",
                    Description = "Exchange resources to grow influence.",
                    Accent = new Color(1f, 0.55f, 0.45f),
                    PanelTint = new Color(0.18f, 0.05f, 0.05f, 0.94f)
                };
            case Kind.Green:
                return new IntroContent
                {
                    Title = "Green DAO",
                    Tagline = "Access Civilization",
                    Description = "Identity and credentials decide participation.",
                    Accent = new Color(0.45f, 0.95f, 0.55f),
                    PanelTint = new Color(0.04f, 0.14f, 0.08f, 0.94f)
                };
            default:
                return default;
        }
    }

    static void EnsureStyles(IntroContent content)
    {
        if (_stylesReady)
        {
            _panelStyle.normal.background = MakeTexture(2, 2, content.PanelTint);
            return;
        }

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter
        };
        _panelStyle.normal.background = MakeTexture(2, 2, content.PanelTint);

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 32,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };

        _taglineStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 24,
            fontStyle = FontStyle.Bold
        };
        _taglineStyle.normal.textColor = new Color(0.9f, 0.94f, 1f);

        _descriptionStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 20,
            fontStyle = FontStyle.Italic
        };
        _descriptionStyle.normal.textColor = new Color(0.78f, 0.86f, 0.95f);

        _footerStyle = new GUIStyle(_descriptionStyle)
        {
            fontSize = 16,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter
        };
        _footerStyle.normal.textColor = new Color(0.65f, 0.78f, 0.9f, 0.9f);

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
