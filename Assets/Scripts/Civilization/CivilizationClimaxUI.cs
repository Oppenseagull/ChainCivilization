using System;
using System.Collections;
using UnityEngine;
using StarterAssets;

/// <summary>
/// Full-screen creation ceremony: pauses movement, staged titles, founder identity, rule write, welcome.
/// Other civilization notice UIs defer until <see cref="OnCeremonyCompleted"/>.
/// </summary>
public class CivilizationClimaxUI : MonoBehaviour
{
    public static bool IsCeremonyActive { get; private set; }
    public static event Action OnCeremonyCompleted;

    [SerializeField] string founderAddress = "0xExplorer";

    [SerializeField] float step2Duration = 5f;
    [SerializeField] float step3Duration = 4f;
    [SerializeField] float step4HoldDuration = 2f;
    [SerializeField] float step5Duration = 4f;
    [SerializeField] float step6Duration = 3f;

    [SerializeField] float fadeInDuration = 0.8f;
    [SerializeField] float fadeOutDuration = 0.7f;
    [SerializeField] float maxBackgroundAlpha = 0.88f;

    enum CeremonyPhase
    {
        None,
        TitleReveal,
        FounderIdentity,
        WorldFeedback,
        RuleWritten,
        WelcomeFounder
    }

    CeremonyPhase _phase = CeremonyPhase.None;
    float _phaseElapsed;
    float _backgroundAlpha;
    float _textAlpha;

    CivilizationType _civilizationType;
    string _joinRuleLine = string.Empty;

    ThirdPersonController _playerController;
    StarterAssetsInputs _playerInputs;
    bool _playerLocked;

    Coroutine _sequenceRoutine;

    GUIStyle _heroTitleStyle;
    GUIStyle _heroSubtitleStyle;
    GUIStyle _sectionLabelStyle;
    GUIStyle _sectionValueStyle;
    GUIStyle _ruleHeaderStyle;
    GUIStyle _ruleBodyStyle;
    GUIStyle _welcomeStyle;
    Texture2D _backgroundTexture;
    bool _stylesReady;

    void OnEnable()
    {
        CivilizationManager.OnCivilizationSelected += HandleCivilizationSelected;
    }

    void OnDisable()
    {
        CivilizationManager.OnCivilizationSelected -= HandleCivilizationSelected;
        StopSequence();
    }

    void HandleCivilizationSelected(CivilizationType type)
    {
        if (type == CivilizationType.None)
        {
            return;
        }

        StopSequence();
        _civilizationType = type;
        _joinRuleLine = CivilizationRuleSelection.GetDisplayLine(CivilizationRuleSelection.SelectedJoinRule);
        _sequenceRoutine = StartCoroutine(PlayCeremonySequence());
    }

    IEnumerator PlayCeremonySequence()
    {
        IsCeremonyActive = true;
        LockPlayerMovement();

        yield return RunPhase(CeremonyPhase.TitleReveal, step2Duration);
        yield return RunPhase(CeremonyPhase.FounderIdentity, step3Duration);
        yield return RunPhase(CeremonyPhase.WorldFeedback, step4HoldDuration);
        yield return RunPhase(CeremonyPhase.RuleWritten, step5Duration);
        yield return RunPhase(CeremonyPhase.WelcomeFounder, step6Duration);

        _phase = CeremonyPhase.None;
        _backgroundAlpha = 0f;
        _textAlpha = 0f;
        UnlockPlayerMovement();
        IsCeremonyActive = false;
        _sequenceRoutine = null;
        OnCeremonyCompleted?.Invoke();
    }

    IEnumerator RunPhase(CeremonyPhase phase, float duration)
    {
        _phase = phase;
        _phaseElapsed = 0f;

        while (_phaseElapsed < duration)
        {
            _phaseElapsed += Time.unscaledDeltaTime;
            UpdatePhaseAlpha(duration);
            yield return null;
        }

        _textAlpha = 0f;
    }

    void UpdatePhaseAlpha(float duration)
    {
        float fadeOutStart = Mathf.Max(fadeInDuration, duration - fadeOutDuration);

        if (_phaseElapsed <= fadeInDuration)
        {
            float t = fadeInDuration > 0f ? _phaseElapsed / fadeInDuration : 1f;
            _backgroundAlpha = Mathf.Lerp(0f, maxBackgroundAlpha, t);
            _textAlpha = Mathf.Clamp01((t - 0.1f) / 0.9f);
            return;
        }

        if (_phaseElapsed >= fadeOutStart)
        {
            float t = fadeOutDuration > 0f ? (_phaseElapsed - fadeOutStart) / fadeOutDuration : 1f;
            _backgroundAlpha = Mathf.Lerp(maxBackgroundAlpha, 0f, t);
            _textAlpha = Mathf.Lerp(1f, 0f, t);
            return;
        }

        _backgroundAlpha = maxBackgroundAlpha;
        _textAlpha = 1f;
    }

    void LockPlayerMovement()
    {
        if (_playerLocked)
        {
            return;
        }

        if (_playerController == null)
        {
            _playerController = FindFirstObjectByType<ThirdPersonController>();
        }

        if (_playerInputs == null)
        {
            _playerInputs = FindFirstObjectByType<StarterAssetsInputs>();
        }

        if (_playerController != null)
        {
            _playerController.enabled = false;
        }

        if (_playerInputs != null)
        {
            _playerInputs.enabled = false;
        }

        _playerLocked = true;
    }

    void UnlockPlayerMovement()
    {
        if (!_playerLocked)
        {
            return;
        }

        if (_playerController != null)
        {
            _playerController.enabled = true;
        }

        if (_playerInputs != null)
        {
            _playerInputs.enabled = true;
        }

        _playerLocked = false;
    }

    void StopSequence()
    {
        if (_sequenceRoutine != null)
        {
            StopCoroutine(_sequenceRoutine);
            _sequenceRoutine = null;
        }

        if (IsCeremonyActive || _playerLocked)
        {
            _phase = CeremonyPhase.None;
            _backgroundAlpha = 0f;
            _textAlpha = 0f;
            UnlockPlayerMovement();
            IsCeremonyActive = false;
        }
    }

    void OnGUI()
    {
        if (_phase == CeremonyPhase.None)
        {
            return;
        }

        EnsureStyles();

        Color previousColor = GUI.color;

        if (_backgroundAlpha > 0.001f)
        {
            GUI.color = new Color(0.02f, 0.05f, 0.12f, _backgroundAlpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), _backgroundTexture);
        }

        if (_textAlpha > 0.001f)
        {
            GUI.color = new Color(1f, 1f, 1f, _textAlpha);

            switch (_phase)
            {
                case CeremonyPhase.TitleReveal:
                    DrawTitleReveal();
                    break;
                case CeremonyPhase.FounderIdentity:
                    DrawFounderIdentity();
                    break;
                case CeremonyPhase.WorldFeedback:
                    DrawWorldFeedbackHint();
                    break;
                case CeremonyPhase.RuleWritten:
                    DrawRuleWritten();
                    break;
                case CeremonyPhase.WelcomeFounder:
                    DrawWelcomeFounder();
                    break;
            }
        }

        GUI.color = previousColor;
    }

    void DrawTitleReveal()
    {
        const float titleHeight = 72f;
        const float subtitleHeight = 40f;
        float blockHeight = titleHeight + subtitleHeight + 12f;
        float titleY = (Screen.height - blockHeight) * 0.5f;

        GUI.Label(new Rect(0f, titleY, Screen.width, titleHeight), "CHAIN CIVILIZATION", _heroTitleStyle);
        GUI.Label(new Rect(0f, titleY + titleHeight + 12f, Screen.width, subtitleHeight),
            "FIRST CIVILIZATION CREATED",
            _heroSubtitleStyle);
    }

    void DrawFounderIdentity()
    {
        const float lineHeight = 36f;
        const float gap = 10f;
        const float blockHeight = lineHeight * 6f + gap * 5f;
        float startY = (Screen.height - blockHeight) * 0.5f;

        DrawLabeledLine("Founder Address", founderAddress, startY, lineHeight);
        startY += lineHeight + gap;
        DrawLabeledLine("Civilization", CivilizationBonuses.GetAddressPanelCivilizationName(_civilizationType), startY, lineHeight);
        startY += lineHeight + gap * 2f;
        DrawLabeledLine("Rule Accepted", _joinRuleLine, startY, lineHeight);
    }

    void DrawLabeledLine(string label, string value, float y, float height)
    {
        GUI.Label(new Rect(0f, y, Screen.width, height * 0.45f), label, _sectionLabelStyle);
        GUI.Label(new Rect(0f, y + height * 0.45f, Screen.width, height * 0.55f), value, _sectionValueStyle);
    }

    void DrawWorldFeedbackHint()
    {
        const float lineHeight = 34f;
        float y = (Screen.height - lineHeight) * 0.5f;
        GUI.Label(new Rect(0f, y, Screen.width, lineHeight), "Your flag rises. Your mark glows.", _heroSubtitleStyle);
    }

    void DrawRuleWritten()
    {
        const float headerHeight = 40f;
        const float bodyHeight = 36f;
        const float englishHeight = 28f;
        float blockHeight = headerHeight + bodyHeight + englishHeight + 16f;
        float startY = (Screen.height - blockHeight) * 0.5f;

        GUI.Label(new Rect(0f, startY, Screen.width, headerHeight), "第一条规则已写入文明共识", _ruleHeaderStyle);
        GUI.Label(new Rect(0f, startY + headerHeight + 4f, Screen.width, bodyHeight),
            CivilizationManager.GetFirstRuleText(_civilizationType),
            _sectionValueStyle);
        GUI.Label(new Rect(0f, startY + headerHeight + bodyHeight + 12f, Screen.width, englishHeight),
            "The Rule Has Been Written",
            _heroSubtitleStyle);
    }

    void DrawWelcomeFounder()
    {
        const float height = 64f;
        float y = (Screen.height - height) * 0.5f;
        GUI.Label(new Rect(0f, y, Screen.width, height), "WELCOME FOUNDER", _welcomeStyle);
    }

    void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _backgroundTexture = MakeTexture(2, 2, Color.white);

        _heroTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 56,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _heroTitleStyle.normal.textColor = Color.white;

        _heroSubtitleStyle = new GUIStyle(_heroTitleStyle)
        {
            fontSize = 26,
            fontStyle = FontStyle.Italic
        };
        _heroSubtitleStyle.normal.textColor = new Color(0.82f, 0.9f, 1f);

        _sectionLabelStyle = new GUIStyle(_heroSubtitleStyle)
        {
            fontSize = 20,
            fontStyle = FontStyle.Normal
        };
        _sectionLabelStyle.normal.textColor = new Color(0.65f, 0.78f, 0.92f);

        _sectionValueStyle = new GUIStyle(_heroTitleStyle)
        {
            fontSize = 32,
            fontStyle = FontStyle.Bold
        };
        _sectionValueStyle.normal.textColor = new Color(1f, 0.92f, 0.45f);

        _ruleHeaderStyle = new GUIStyle(_sectionValueStyle)
        {
            fontSize = 28
        };
        _ruleHeaderStyle.normal.textColor = new Color(0.85f, 0.92f, 1f);

        _ruleBodyStyle = _sectionValueStyle;

        _welcomeStyle = new GUIStyle(_heroTitleStyle)
        {
            fontSize = 48,
            fontStyle = FontStyle.Bold
        };
        _welcomeStyle.normal.textColor = new Color(0.45f, 0.95f, 0.65f);

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
