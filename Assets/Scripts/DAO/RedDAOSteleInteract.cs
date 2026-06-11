using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Red DAO central stele: first E grants 100 MOON once; after that, donate 50 MOON for 20 Reputation once.
/// </summary>
public class RedDAOSteleInteract : MonoBehaviour
{
    [SerializeField] float interactRadius = 14f;
    [SerializeField] string welcomeMessage = "欢迎来到 Red DAO";
    [SerializeField] int moonReward = 100;
    [SerializeField] int donationMoonCost = 50;
    [SerializeField] int donationReputationReward = 20;
    [SerializeField] string donationMessageLine1 = "你资助了贸易文明。";
    [SerializeField] string donationMessageLine2 = "Reputation +20";
    [SerializeField] float rewardPopupDuration = 2.5f;


    Transform _player;
    bool _playerNear;
    bool _wasPlayerNear;
    bool _showingIntro;
    float _rewardPopupTimer;

    GUIStyle _rewardStyle;
    bool _rewardStyleReady;

    bool _wasShowingIntro;

    static bool WasEPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    void Start()
    {
        Debug.Log("[DAO DEBUG] Start OK RedDAO");

        GroundSnapUtility.SnapTransform(transform, 0f);
        VisualHierarchyOptions options = VisualHierarchyOptions.ForInteractive("RED DAO", new Color(1f, 0.62f, 0.38f));
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
            Debug.LogError("[DAO DEBUG] Player NULL RedDAO");
        }
        else
        {
            Debug.Log("[DAO DEBUG] Player Found: " + _player.name + " RedDAO");
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
            Debug.Log(_playerNear ? "[DAO DEBUG] Near=True RedDAO" : "[DAO DEBUG] Near=False RedDAO");
        }

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[DAO DEBUG] RedDAO PlayerPos={_player.position} DaoPos={transform.position} Distance={distance:F1} Radius={interactRadius}");
        }

        _wasPlayerNear = _playerNear;

        if (!_playerNear)
        {
            _showingIntro = false;
            _wasShowingIntro = false;
            HUDPromptChannel.Clear(this);
            return;
        }

        if (!DAOIntroCard.HasSeen(DAOIntroCard.Kind.Red))
        {
            _showingIntro = true;
        }

        if (_showingIntro && !_wasShowingIntro)
        {
            Debug.Log("[DAO DEBUG] Intro Active RedDAO");
        }

        _wasShowingIntro = _showingIntro;

        if (_showingIntro)
        {
            if (WasEPressedThisFrame())
            {
                Debug.Log("[DAO DEBUG] E Pressed RedDAO");
                Debug.Log("[DAO DEBUG] E Consumed By Intro RedDAO");
            }

            DAOIntroCard.TryDismissOnInteract(DAOIntroCard.Kind.Red, ref _showingIntro);
            return;
        }

        if (_rewardPopupTimer > 0f)
        {
            _rewardPopupTimer -= Time.deltaTime;
        }

        if (WasEPressedThisFrame())
        {
            Debug.Log("[DAO DEBUG] E Pressed RedDAO");
            Debug.Log("[DAO DEBUG] HandleInteract RedDAO");
            HandleInteract();
        }
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

        TokenManager tokens = TokenManager.Instance;
        bool initialClaimed = tokens != null && tokens.HasClaimedRedDaoStele;
        string line1 = initialClaimed ? "奖励已领取" : welcomeMessage;
        string line2 = initialClaimed ? "当前DAO暂无更多任务" : "按 E 获得 100 MOON";
        float priority = -Vector3.Distance(_player.position, transform.position);
        HUDPromptChannel.Set(this, line1, line2, priority);
    }

    void HandleInteract()
    {
        TokenManager tokens = TokenManager.Instance;
        if (tokens == null)
        {
            return;
        }

        if (!tokens.HasClaimedRedDaoStele)
        {
            if (tokens.TryClaimRedDaoSteleReward(moonReward))
            {
                tokens.ShowRewardPopup($"+{moonReward} MOON", rewardPopupDuration);
                _rewardPopupTimer = rewardPopupDuration;
            }

            return;
        }

        TryDonate();
    }

    void TryDonate()
    {
        ReputationManager reputation = ReputationManager.Instance;
        TokenManager tokens = TokenManager.Instance;
        if (reputation == null || tokens == null)
        {
            return;
        }

        if (reputation.HasDonatedRedDao)
        {
            return;
        }

        if (tokens.MoonBalance < donationMoonCost)
        {
            return;
        }

        if (!tokens.TrySpendMoon(donationMoonCost))
        {
            return;
        }

        int reputationReward = CivilizationManager.HasSelectedCivilization
            ? CivilizationBonuses.GetRedDonationReputationReward(CivilizationManager.selectedCivilizationType)
            : donationReputationReward;
        string reputationLine2 = $"Reputation +{reputationReward}";

        reputation.TryClaimRedDaoDonationReputation(
            reputationReward,
            donationMessageLine1,
            reputationLine2);
    }

    void OnGUI()
    {
        if (!_playerNear)
        {
            return;
        }

        if (_showingIntro)
        {
            DAOIntroCard.Draw(DAOIntroCard.Kind.Red);
            return;
        }

        if (_rewardPopupTimer <= 0f)
        {
            return;
        }

        EnsureRewardStyle();
        const float popupWidth = 420f;
        float popupX = (Screen.width - popupWidth) * 0.5f;
        float popupY = Screen.height * 0.42f;
        GUI.Label(new Rect(popupX, popupY, popupWidth, 56f), $"+{moonReward} MOON", _rewardStyle);
    }

    void EnsureRewardStyle()
    {
        if (_rewardStyleReady)
        {
            return;
        }

        _rewardStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 36,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            wordWrap = true
        };
        _rewardStyle.normal.textColor = new Color(1f, 0.85f, 0.35f);
        _rewardStyleReady = true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.25f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
