using UnityEngine;

/// <summary>
/// Grants a DAO Pass when the player enters this trigger zone once requirements are met.
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
    Collider _zoneCollider;

    void Start()
    {
        _zoneCollider = GetComponent<Collider>();
        if (_zoneCollider != null)
        {
            _zoneCollider.isTrigger = true;
        }

        GroundSnapUtility.SnapTransform(transform, groundTriggerCenterHeight);

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            _player = player.transform;
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

        _playerInside = true;
        if (_player == null)
        {
            _player = other.transform.root;
        }

        TryGrantPassIfEligible();
    }

    void Update()
    {
        if (_player == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                _player = player.transform;
            }
        }

        if (_player == null)
        {
            return;
        }

        bool isInside = IsPlayerInsideZone();
        if (isInside)
        {
            _playerInside = true;
            TryGrantPassIfEligible();
        }
        else if (_playerInside)
        {
            _playerInside = false;
            HUDPromptChannel.Clear(this);
        }
    }

    void TryGrantPassIfEligible()
    {
        TokenManager tokens = TokenManager.Instance;
        ReputationManager reputation = ReputationManager.Instance;

        if (requireMoon && !DAORequirements.MeetsGreenDaoMoonRequirement(tokens))
        {
            Debug.Log($"[DAOPassEntryZone] MOON requirement not met: {(tokens != null ? tokens.MoonBalance : 0)}/{DAORequirements.GreenDaoRequiredMoon}");
            return;
        }

        if (requireReputation && !DAORequirements.MeetsGreenDaoReputationRequirement(reputation))
        {
            Debug.Log($"[DAOPassEntryZone] REP requirement not met: {(reputation != null ? reputation.GetReputation() : 0)}/{DAORequirements.GreenDaoRequiredReputation}");
            return;
        }

        DAOPassManager passes = DAOPassManager.Instance;
        if (passes != null && passes.HasPass(passId))
        {
            return; // Already has pass, no need to log
        }

        Debug.Log($"[DAOPassEntryZone] Requirements met! Granting pass: {passDisplayName}");
        passes?.TryGrantPass(passId, passDisplayName);
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

    bool IsPlayerInsideZone()
    {
        if (_zoneCollider == null)
        {
            _zoneCollider = GetComponent<Collider>();
        }

        if (_zoneCollider == null || _player == null)
        {
            return false;
        }

        Vector3 playerPosition = _player.position;

        // Use closest point on the collider for a reliable horizontal-distance check.
        // This avoids false negatives when the collider is thin on the Y axis
        // or when GroundSnap places it slightly above/below the player's feet.
        Vector3 closestPoint = _zoneCollider.ClosestPoint(playerPosition);
        Vector3 delta = closestPoint - playerPosition;

        // Ignore vertical difference (Y) so height mismatches don't block the trigger.
        float horizontalDistanceSq = delta.x * delta.x + delta.z * delta.z;
        float verticalDistance = Mathf.Abs(delta.y);

        return horizontalDistanceSq <= 25f && verticalDistance <= 50f;
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
