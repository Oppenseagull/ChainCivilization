using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Blue DAO central stele: show welcome text when player is near, reveal lore on E.
/// Uses OnGUI for minimal setup (no Canvas prefab required).
/// </summary>
public class BlueDAOSteleInteract : MonoBehaviour
{
    [SerializeField] float interactRadius = 14f;
    [SerializeField] string welcomeMessage = "欢迎来到 Blue DAO";
    [SerializeField] string interactMessage = "这里信仰开放协作";
    [SerializeField] int reputationReward = 10;
    [SerializeField] string reputationMessageLine1 = "你帮助了开放协作文明。";
    [SerializeField] string reputationMessageLine2 = "Reputation +10";

    const string DebugTag = "BlueDAO";

    Transform _player;
    bool _playerNear;
    bool _wasPlayerNear;
    bool _wasShowingIntro;
    bool _revealed;
    bool _showingIntro;

    void Start()
    {
        Debug.Log("[DAO DEBUG] Start OK " + DebugTag);

        GroundSnapUtility.SnapTransform(transform, 0f);
        VisualHierarchyOptions options = VisualHierarchyOptions.ForInteractive("BLUE DAO", new Color(0.62f, 0.84f, 1f));
        options.EnableFloat = false;
        options.LabelHeight = 5.5f;
        VisualHierarchy.Apply(gameObject, VisualHierarchyTier.Interactive, options);

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            _player = player.transform;
        }

        if (_player == null)
        {
            Debug.LogError("[DAO DEBUG] Player NULL " + DebugTag);
        }
        else
        {
            Debug.Log("[DAO DEBUG] Player Found: " + _player.name + " " + DebugTag);
        }
    }

    void Update()
    {
        if (_player == null)
        {
            return;
        }

        float distance = Vector3.Distance(_player.position, transform.position);
        _playerNear = distance <= interactRadius;

        if (_playerNear != _wasPlayerNear)
        {
            Debug.Log(_playerNear ? "[DAO DEBUG] Near=True " + DebugTag : "[DAO DEBUG] Near=False " + DebugTag);
        }

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[DAO DEBUG] {DebugTag} PlayerPos={_player.position} DaoPos={transform.position} Distance={distance:F1} Radius={interactRadius}");
        }

        _wasPlayerNear = _playerNear;

        if (!_playerNear)
        {
            _revealed = false;
            _showingIntro = false;
            _wasShowingIntro = false;
            HUDPromptChannel.Clear(this);
            return;
        }

        if (!DAOIntroCard.HasSeen(DAOIntroCard.Kind.Blue))
        {
            _showingIntro = true;
        }

        if (_showingIntro && !_wasShowingIntro)
        {
            Debug.Log("[DAO DEBUG] Intro Active " + DebugTag);
        }

        _wasShowingIntro = _showingIntro;

        if (_showingIntro)
        {
            if (WasEPressedThisFrame())
            {
                Debug.Log("[DAO DEBUG] E Pressed " + DebugTag);
                Debug.Log("[DAO DEBUG] E Consumed By Intro " + DebugTag);
            }

            DAOIntroCard.TryDismissOnInteract(DAOIntroCard.Kind.Blue, ref _showingIntro);
            return;
        }

        if (WasEPressedThisFrame())
        {
            Debug.Log("[DAO DEBUG] E Pressed " + DebugTag);
            Debug.Log("[DAO DEBUG] HandleInteract " + DebugTag);
            HandleInteract();
        }
    }


    static bool WasEPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    void LateUpdate()
    {
        if (!GameHUDCanvas.IsActive || _player == null)
        {
            HUDPromptChannel.Clear(this);
            return;
        }

        if (!_playerNear)
        {
            HUDPromptChannel.Clear(this);
            return;
        }

        ReputationManager reputation = ReputationManager.Instance;
        bool claimed = reputation != null && reputation.HasClaimedBlueDaoReputation;
        string line2 = claimed
            ? "信誉奖励已领取\n开放协作理念仍然欢迎你"
            : "这里信仰开放协作\n按 E 获得信誉";
        float priority = -Vector3.Distance(_player.position, transform.position);
        HUDPromptChannel.Set(this, welcomeMessage, line2, priority);
    }

    void HandleInteract()
    {
        _revealed = true;
        QuestSignals.MarkBlueDaoVisited();

        ReputationManager reputation = ReputationManager.Instance;
        if (reputation != null)
        {
            reputation.TryClaimBlueDaoSteleReputation(
                reputationReward,
                reputationMessageLine1,
                reputationMessageLine2);
        }
    }

    void OnGUI()
    {
        if (!_playerNear || !_showingIntro)
        {
            return;
        }

        DAOIntroCard.Draw(DAOIntroCard.Kind.Blue);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
