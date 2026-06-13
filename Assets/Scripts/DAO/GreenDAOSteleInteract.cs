using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Green DAO central stele: explains access requirements. The nearby entry zone
/// grants Green Pass once MOON and Reputation requirements are met.
/// </summary>
public class GreenDAOSteleInteract : MonoBehaviour
{
    [SerializeField] float interactRadius = 14f;

    Transform _player;
    bool _playerNear;
    bool _showingIntro;

    void Start()
    {
        GroundSnapUtility.SnapTransform(transform, 0f);
        LandmarkVisualFactory.ApplyDaoSanctuary(
            gameObject,
            "GreenDAO",
            new Color(0.32f, 0.92f, 0.48f),
            new Color(0.62f, 0.78f, 0.62f));
        VisualHierarchyOptions options = VisualHierarchyOptions.ForInteractive("GREEN DAO", new Color(0.48f, 0.9f, 0.5f));
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

        if (!DAOIntroCard.HasSeen(DAOIntroCard.Kind.Green))
        {
            _showingIntro = true;
        }

        if (_showingIntro)
        {
            DAOIntroCard.TryDismissOnInteract(DAOIntroCard.Kind.Green, ref _showingIntro);
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
        if (tokens == null)
        {
            HUDPromptChannel.Clear(this);
            return;
        }

        ReputationManager reputation = ReputationManager.Instance;
        DAOPassManager passes = DAOPassManager.Instance;
        int moonBalance = tokens.MoonBalance;
        int repBalance = reputation != null ? reputation.GetReputation() : 0;
        bool hasPass = passes != null && passes.HasGreenPass;
        DAORequirements.GetGreenDaoStatusHud(moonBalance, repBalance, hasPass, out string line1, out string line2);
        float priority = -Vector3.Distance(_player.position, transform.position);
        HUDPromptChannel.Set(this, line1, line2, priority);
    }

    void OnGUI()
    {
        if (!_playerNear || !_showingIntro)
        {
            return;
        }

        DAOIntroCard.Draw(DAOIntroCard.Kind.Green);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.2f, 1f, 0.35f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
