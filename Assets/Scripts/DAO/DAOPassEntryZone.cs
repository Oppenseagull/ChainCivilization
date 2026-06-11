using UnityEngine;

/// <summary>
/// Grants a DAO Pass when the player enters this trigger zone (once requirements are met).
/// </summary>
[RequireComponent(typeof(Collider))]
public class DAOPassEntryZone : MonoBehaviour
{
    [SerializeField] string passId = DAOPassManager.GreenPassId;
    [SerializeField] string passDisplayName = "Green DAO Pass";
    [SerializeField] bool requireMoon = true;
    [SerializeField] bool requireReputation = true;
    [SerializeField] float groundTriggerCenterHeight = 1f;

    bool _playerInside;
    Transform _player;

    void Start()
    {
        Debug.Log("[DAO DEBUG] Start OK DAOPassEntryZone");

        GroundSnapUtility.SnapTransform(transform, groundTriggerCenterHeight);

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            _player = player.transform;
        }

        if (_player == null)
        {
            Debug.LogError("[DAO DEBUG] Player NULL DAOPassEntryZone");
        }
        else
        {
            Debug.Log("[DAO DEBUG] Player Found: " + _player.name + " DAOPassEntryZone");
        }
    }

    void Reset()
    {
        Collider zoneCollider = GetComponent<Collider>();
        if (zoneCollider != null)
        {
            zoneCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        Debug.Log("[DAO DEBUG] Enter Pass Zone");

        _playerInside = true;
        if (_player == null)
        {
            _player = other.transform.root;
        }

        TokenManager tokens = TokenManager.Instance;
        ReputationManager reputation = ReputationManager.Instance;
        int moon = tokens != null ? tokens.MoonBalance : 0;
        int rep = reputation != null ? reputation.GetReputation() : 0;

        if (requireMoon && !DAORequirements.MeetsGreenDaoMoonRequirement(tokens))
        {
            Debug.Log("[DAO DEBUG] Pass Failed Moon=" + moon + " Rep=" + rep);
            return;
        }

        if (requireReputation && !DAORequirements.MeetsGreenDaoReputationRequirement(reputation))
        {
            Debug.Log("[DAO DEBUG] Pass Failed Moon=" + moon + " Rep=" + rep);
            return;
        }

        DAOPassManager passes = DAOPassManager.Instance;
        if (passes != null && passes.TryGrantPass(passId, passDisplayName))
        {
            Debug.Log("[DAO DEBUG] Pass Granted");
        }
        else
        {
            Debug.Log("[DAO DEBUG] Pass Failed Moon=" + moon + " Rep=" + rep + " (already has pass or manager null)");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!IsPlayer(other))
        {
            return;
        }

        _playerInside = false;
        HUDPromptChannel.Clear(this);
    }

    void LateUpdate()
    {
        if (!GameHUDCanvas.IsActive || !_playerInside)
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

        float priority = _player != null ? -Vector3.Distance(_player.position, transform.position) : 0f;
        HUDPromptChannel.Set(this, line1, line2, priority);
    }

    void OnDisable()
    {
        HUDPromptChannel.Clear(this);
    }

    static bool IsPlayer(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            return true;
        }

        Transform root = other.transform.root;
        return root.name == "Player" || root.GetComponent<CharacterController>() != null;
    }
}
