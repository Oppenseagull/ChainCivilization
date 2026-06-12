using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Blue DAO central stele: shows intro lore and grants first reputation reward.
/// </summary>
public class BlueDAOSteleInteract : MonoBehaviour
{
    [SerializeField] float interactRadius = 14f;
    [SerializeField] string welcomeMessage = "Welcome to Blue DAO";
    [SerializeField] string interactMessage = "This DAO values open collaboration.";
    [SerializeField] int reputationReward = 10;
    [SerializeField] string reputationMessageLine1 = "You helped an open collaboration civilization.";
    [SerializeField] string reputationMessageLine2 = "Reputation +10";

    Transform _player;
    bool _playerNear;
    bool _showingIntro;

    void Start()
    {
        GroundSnapUtility.SnapTransform(transform, 0f);
        LandmarkVisualFactory.ApplyDaoSanctuary(
            gameObject,
            "BlueDAO",
            new Color(0.42f, 0.72f, 1f),
            new Color(0.72f, 0.8f, 0.88f));
        VisualHierarchyOptions options = VisualHierarchyOptions.ForInteractive("BLUE DAO", new Color(0.62f, 0.84f, 1f));
        options.EnableFloat = false;
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

        _playerNear = Vector3.Distance(_player.position, transform.position) <= interactRadius;
        if (!_playerNear)
        {
            _showingIntro = false;
            HUDPromptChannel.Clear(this);
            return;
        }

        if (!DAOIntroCard.HasSeen(DAOIntroCard.Kind.Blue))
        {
            _showingIntro = true;
        }

        if (_showingIntro)
        {
            DAOIntroCard.TryDismissOnInteract(DAOIntroCard.Kind.Blue, ref _showingIntro);
            return;
        }

        if (WasEPressedThisFrame())
        {
            HandleInteract();
        }
    }

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
            ? "Reputation reward claimed\nOpen collaboration still welcomes you"
            : $"{interactMessage}\nPress E to gain Reputation";
        float priority = -Vector3.Distance(_player.position, transform.position);
        HUDPromptChannel.Set(this, welcomeMessage, line2, priority);
    }

    void HandleInteract()
    {
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
