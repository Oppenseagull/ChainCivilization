using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Civilization Seed in front of BoundaryStone: proximity UI + E to open rule selection panel.
/// </summary>
public class CivilizationSeedInteract : MonoBehaviour
{
    [SerializeField] float interactRadius = 6f;
    [SerializeField] string titleMessage = "文明种子";
    [SerializeField] string interactHint = "按E开始创造文明";
    [SerializeField] float hoverHeightMin = 1f;
    [SerializeField] float hoverHeightMax = 1.5f;

    Transform _player;
    bool _playerNear;
    bool _hasSelectedRule;

    CivilizationSeedRulePanel _rulePanel;

    GUIStyle _panelStyle;
    GUIStyle _titleStyle;
    GUIStyle _hintStyle;
    bool _stylesReady;

    void Start()
    {
        SnapSeedToGround();
        LandmarkVisualFactory.ApplyCivilizationSeed(gameObject);
        VisualHierarchy.Apply(gameObject, VisualHierarchyTier.CivilizationSeed, VisualHierarchyOptions.ForCivilizationSeed());

        _rulePanel = GetComponent<CivilizationSeedRulePanel>();
        if (_rulePanel == null)
        {
            _rulePanel = gameObject.AddComponent<CivilizationSeedRulePanel>();
        }

        _rulePanel.OnRuleSelected += HandleRuleSelected;

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            _player = player.transform;
        }
    }

    void OnDestroy()
    {
        if (_rulePanel != null)
        {
            _rulePanel.OnRuleSelected -= HandleRuleSelected;
        }
    }

    void Update()
    {
        if (!QuestSignals.BoundaryLoreComplete)
        {
            SetRenderersEnabled(false);
            return;
        }

        SetRenderersEnabled(true);

        if (_player == null || _hasSelectedRule)
        {
            return;
        }

        _playerNear = Vector3.Distance(_player.position, transform.position) <= interactRadius;
        if (!_playerNear || (_rulePanel != null && _rulePanel.IsOpen))
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
            TryOpenRulePanel();
        }
#else
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryOpenRulePanel();
        }
#endif
    }

    void SnapSeedToGround()
    {
        float hoverHeight = (hoverHeightMin + hoverHeightMax) * 0.5f;
        if (GroundSnapUtility.SnapTransform(transform, hoverHeight) && transform.parent != null)
        {
            transform.localPosition = transform.parent.InverseTransformPoint(transform.position);
        }
    }

    void TryOpenRulePanel()
    {
        if (_hasSelectedRule || _rulePanel == null || _rulePanel.IsOpen)
        {
            return;
        }

        _rulePanel.Open();
        Debug.Log("CivilizationSeed: OPEN RULE PANEL");
    }

    void HandleRuleSelected(string rule)
    {
        _hasSelectedRule = CivilizationManager.HasSelectedCivilization;
        Debug.Log($"CivilizationSeed: CREATION STARTED WITH RULE -> {rule} ({CivilizationManager.selectedCivilizationType})");
    }

    void LateUpdate()
    {
        if (!GameHUDCanvas.IsActive || _player == null)
        {
            HUDPromptChannel.Clear(this);
            return;
        }

        if (!_playerNear || _hasSelectedRule || (_rulePanel != null && _rulePanel.IsOpen))
        {
            HUDPromptChannel.Clear(this);
            return;
        }

        float priority = -Vector3.Distance(_player.position, transform.position);
        HUDPromptChannel.Set(this, titleMessage, interactHint, priority);
    }

    void OnGUI()
    {
        if (!_playerNear || _hasSelectedRule || (_rulePanel != null && _rulePanel.IsOpen))
        {
            return;
        }

        if (GameHUDCanvas.IsActive)
        {
            return;
        }

        EnsureStyles();

        const float width = 580f;
        const float height = 96f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * 0.68f;

        GUI.Box(new Rect(x - 20f, y - 18f, width + 40f, height), GUIContent.none, _panelStyle);
        GUI.Label(new Rect(x, y, width, 40f), titleMessage, _titleStyle);
        GUI.Label(new Rect(x, y + 44f, width, 28f), interactHint, _hintStyle);
    }

    void SetRenderersEnabled(bool isEnabled)
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            if (r.enabled != isEnabled)
            {
                r.enabled = isEnabled;
            }
        }
    }

    void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _panelStyle = new GUIStyle(GUI.skin.box)
        {
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter
        };
        _panelStyle.normal.background = MakeTexture(2, 2, new Color(0.08f, 0.12f, 0.22f, 0.9f));

        _titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        _titleStyle.normal.textColor = new Color(1f, 0.92f, 0.45f);

        _hintStyle = new GUIStyle(_titleStyle)
        {
            fontSize = 20,
            fontStyle = FontStyle.Italic
        };
        _hintStyle.normal.textColor = new Color(0.85f, 0.95f, 1f, 0.9f);

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.85f, 0.3f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
