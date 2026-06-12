using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Red DAO central stele: first E grants 100 MOON once; after that,
/// donate 50 MOON for Reputation once.
/// </summary>
public class RedDAOSteleInteract : MonoBehaviour
{
    [SerializeField] float interactRadius = 14f;
    [SerializeField] string welcomeMessage = "Welcome to Red DAO";
    [SerializeField] int moonReward = 100;
    [SerializeField] int donationMoonCost = 50;
    [SerializeField] int donationReputationReward = 20;
    [SerializeField] string donationMessageLine1 = "You funded a trade civilization.";
    [SerializeField] string donationMessageLine2 = "Reputation +20";
    [SerializeField] float rewardPopupDuration = 2.5f;

    Transform _player;
    bool _playerNear;
    bool _showingIntro;
    float _rewardPopupTimer;

    GUIStyle _rewardStyle;
    bool _rewardStyleReady;

    static bool WasEPressedThisFrame()
    {
        if (GameplayInputGate.BlocksGameplayShortcuts)
        {
            return false;
        }

#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }

    void Start()
    {
        GroundSnapUtility.SnapTransform(transform, 0f);
        LandmarkVisualFactory.ApplyDaoSanctuary(
            gameObject,
            "RedDAO",
            new Color(1f, 0.42f, 0.26f),
            new Color(0.82f, 0.68f, 0.56f));
        VisualHierarchyOptions options = VisualHierarchyOptions.ForInteractive("RED DAO", new Color(1f, 0.62f, 0.38f));
        options.EnableFloat = false;
        options.EnableSpin = false;
        options.EnableParticles = false;
        options.EnableGlowRing = false;
        options.LabelHeight = 5.5f;
        VisualHierarchy.Apply(gameObject, VisualHierarchyTier.Interactive, options);

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

        float distance = Vector3.Distance(_player.position, transform.position);
        _playerNear = distance <= interactRadius;

        if (!_playerNear)
        {
            _showingIntro = false;
            HUDPromptChannel.Clear(this);
            return;
        }

        if (!DAOIntroCard.HasSeen(DAOIntroCard.Kind.Red))
        {
            _showingIntro = true;
        }
        if (_showingIntro)
        {
            DAOIntroCard.TryDismissOnInteract(DAOIntroCard.Kind.Red, ref _showingIntro);
            return;
        }

        if (_rewardPopupTimer > 0f)
        {
            _rewardPopupTimer -= Time.deltaTime;
        }

        if (WasEPressedThisFrame())
        {
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

        GetHudLines(out string line1, out string line2);
        float priority = -Vector3.Distance(_player.position, transform.position);
        HUDPromptChannel.Set(this, line1, line2, priority);
    }

    void GetHudLines(out string line1, out string line2)
    {
        TokenManager tokens = TokenManager.Instance;
        ReputationManager reputation = ReputationManager.Instance;

        bool initialClaimed = tokens != null && tokens.HasClaimedRedDaoStele;
        bool donated = reputation != null && reputation.HasDonatedRedDao;
        int moon = tokens != null ? tokens.MoonBalance : 0;

        if (!initialClaimed)
        {
            line1 = welcomeMessage;
            line2 = $"Press E to claim {moonReward} MOON";
            return;
        }

        if (donated)
        {
            line1 = "Red DAO donation complete";
            line2 = "Your trade contribution is recorded.";
            return;
        }

        line1 = "Red DAO trade pledge";
        line2 = moon >= donationMoonCost
            ? $"Press E to donate {donationMoonCost} MOON for Reputation"
            : $"Need {donationMoonCost} MOON to donate (current {moon})";
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
            reputation.ShowReputationPopup("Red DAO donation complete", "This reward has already been claimed.");
            return;
        }

        if (tokens.MoonBalance < donationMoonCost)
        {
            tokens.ShowRewardPopup($"Need {donationMoonCost} MOON to donate", 2f);
            return;
        }

        if (!tokens.TrySpendMoon(donationMoonCost))
        {
            return;
        }

        int reputationReward = CivilizationManager.HasSelectedCivilization
            ? CivilizationBonuses.GetRedDonationReputationReward(CivilizationManager.selectedCivilizationType)
            : donationReputationReward;

        reputation.TryClaimRedDaoDonationReputation(
            reputationReward,
            donationMessageLine1,
            $"Reputation +{reputationReward}");
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
