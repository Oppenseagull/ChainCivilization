using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Collectible blue Moon Crystal: press E when near to gain MOON, then disappear.
/// </summary>
public class MoonCrystalInteract : MonoBehaviour
{
    const string CollectedKeyPrefix = "moon_crystal_collected_";

    static readonly string[] KnownCrystalIds =
    {
        "crystal_01", "crystal_02", "crystal_03", "crystal_04", "crystal_05"
    };

    [SerializeField] string crystalId = "crystal_01";
    [SerializeField] float interactRadius = 4f;
    [SerializeField] int moonReward = 50;
    [SerializeField] string rewardPopupText = "+50 Moon";
    [SerializeField] string interactHint = "Press E to collect";
    [SerializeField] float groundHeightOffset = 0.5f;

    Transform _player;
    bool _playerNear;
    bool _wasPlayerNear;

    GUIStyle _panelStyle;
    GUIStyle _hintStyle;
    bool _stylesReady;

    void Start()
    {
        if (string.IsNullOrEmpty(crystalId))
        {
            crystalId = name;
        }

        GroundSnapUtility.SnapTransform(transform, groundHeightOffset);

        if (IsCollected())
        {
            Destroy(gameObject);
            return;
        }

        VisualHierarchy.Apply(
            gameObject,
            VisualHierarchyTier.Interactive,
            VisualHierarchyOptions.ForInteractive("MOON CRYSTAL", new Color(0.55f, 0.84f, 1f)));

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            _player = player.transform;
        }
    }

    void Update()
    {
        if (_player == null)
        {
            return;
        }

        bool inRange = Vector3.Distance(_player.position, transform.position) <= interactRadius;
        UpdateRangeState(inRange);

        if (!_playerNear || IsCollected())
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            Debug.Log($"PRESS E ({name}, id={crystalId})");
            TryCollect();
        }
#else
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"PRESS E ({name}, id={crystalId})");
            TryCollect();
        }
#endif
    }

    void UpdateRangeState(bool inRange)
    {
        _playerNear = inRange;

        if (inRange && !_wasPlayerNear)
        {
            Debug.Log($"ENTER CRYSTAL ({name}, id={crystalId})");
        }
        else if (!inRange && _wasPlayerNear)
        {
            Debug.Log($"EXIT CRYSTAL ({name}, id={crystalId})");
        }

        _wasPlayerNear = inRange;
    }

    void LateUpdate()
    {
        if (!GameHUDCanvas.IsActive)
        {
            HUDPromptChannel.Clear(this);
            return;
        }

        if (!_playerNear || IsCollected())
        {
            HUDPromptChannel.Clear(this);
            return;
        }

        float priority = -Vector3.Distance(_player.position, transform.position);
        HUDPromptChannel.Set(this, interactHint, null, priority);
    }

    void TryCollect()
    {
        if (IsCollected())
        {
            Debug.Log($"COLLECT BLOCKED already collected ({name}, id={crystalId})");
            return;
        }

        TokenManager tokens = TokenManager.Instance;
        if (tokens == null)
        {
            Debug.LogWarning($"COLLECT FAILED TokenManager missing ({name}, id={crystalId})");
            return;
        }

        int bonus = CivilizationManager.HasSelectedCivilization
            ? CivilizationBonuses.GetMoonCrystalBonus(CivilizationManager.selectedCivilizationType)
            : 0;
        int totalReward = moonReward + bonus;

        tokens.AddMoon(totalReward);
        tokens.ShowRewardPopup(bonus > 0 ? $"+{totalReward} Moon" : rewardPopupText);
        CivilizationJournalLog.RecordMoonRewardOnce($"crystal_{crystalId}", totalReward);
        MarkCollected();
        Debug.Log($"COLLECT SUCCESS ({name}, id={crystalId}, +{totalReward}, bonus={bonus})");
        Destroy(gameObject);
    }

    public static void ClearAllCollectedPrefs()
    {
        for (int i = 0; i < KnownCrystalIds.Length; i++)
        {
            PlayerPrefs.DeleteKey(CollectedKeyPrefix + KnownCrystalIds[i]);
        }

        PlayerPrefs.Save();
    }

    bool IsCollected()
    {
        return PlayerPrefs.GetInt(CollectedKeyPrefix + crystalId, 0) == 1;
    }

    void MarkCollected()
    {
        PlayerPrefs.SetInt(CollectedKeyPrefix + crystalId, 1);
        PlayerPrefs.Save();
    }

    void OnGUI()
    {
        if (!_playerNear || IsCollected())
        {
            return;
        }

        if (GameHUDCanvas.IsActive)
        {
            return;
        }

        EnsureStyles();

        const float width = 360f;
        const float height = 72f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * 0.72f;

        GUI.Box(new Rect(x - 16f, y - 12f, width + 32f, height), GUIContent.none, _panelStyle);
        GUI.Label(new Rect(x, y + 8f, width, 28f), interactHint, _hintStyle);
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
        _panelStyle.normal.background = MakeTexture(2, 2, new Color(0.05f, 0.1f, 0.22f, 0.88f));

        _hintStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            fontStyle = FontStyle.Italic,
            alignment = TextAnchor.MiddleCenter
        };
        _hintStyle.normal.textColor = new Color(0.65f, 0.85f, 1f, 0.95f);

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
        Gizmos.color = new Color(0.25f, 0.55f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
