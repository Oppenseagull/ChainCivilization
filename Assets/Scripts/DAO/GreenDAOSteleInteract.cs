using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Green DAO central stele: access gated by MOON (200) and Reputation (20).
/// </summary>
public class GreenDAOSteleInteract : MonoBehaviour
{
    [SerializeField] float interactRadius = 14f;
    [SerializeField] string welcomeMessage = "欢迎加入 Green DAO";
    [SerializeField] string passGrantedLine1 = "Green Pass 已获得";
    [SerializeField] string passGrantedLine2 = "你已经拥有建立文明资格";
    [SerializeField] string eligibleLine2 = "进入区域即可获得 Green Pass";


    Transform _player;
    bool _playerNear;
    bool _wasPlayerNear;
    bool _showingIntro;

    bool _wasShowingIntro;

    void Start()
    {
        Debug.Log("[DAO DEBUG] Start OK GreenDAO");

        GroundSnapUtility.SnapTransform(transform, 0f);
        VisualHierarchyOptions options = VisualHierarchyOptions.ForInteractive("GREEN DAO", new Color(0.48f, 0.9f, 0.5f));
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
            Debug.LogError("[DAO DEBUG] Player NULL GreenDAO");
        }
        else
        {
            Debug.Log("[DAO DEBUG] Player Found: " + _player.name + " GreenDAO");
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
            Debug.Log(_playerNear ? "[DAO DEBUG] Near=True GreenDAO" : "[DAO DEBUG] Near=False GreenDAO");
        }

        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"[DAO DEBUG] GreenDAO PlayerPos={_player.position} DaoPos={transform.position} Distance={distance:F1} Radius={interactRadius}");
        }

        _wasPlayerNear = _playerNear;

        if (!_playerNear)
        {
            _showingIntro = false;
            _wasShowingIntro = false;
            HUDPromptChannel.Clear(this);
            return;
        }

        if (!DAOIntroCard.HasSeen(DAOIntroCard.Kind.Green))
        {
            _showingIntro = true;
        }

        if (_showingIntro && !_wasShowingIntro)
        {
            Debug.Log("[DAO DEBUG] Intro Active GreenDAO");
        }

        _wasShowingIntro = _showingIntro;

        if (_showingIntro)
        {
            if (WasEPressedThisFrame())
            {
                Debug.Log("[DAO DEBUG] E Pressed GreenDAO");
                Debug.Log("[DAO DEBUG] E Consumed By Intro GreenDAO");
            }

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
