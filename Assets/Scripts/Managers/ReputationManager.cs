using System;
using UnityEngine;

/// <summary>
/// Global reputation tracker, persisted through PlayerPrefs so one-time DAO rewards
/// cannot leave the player with claimed rewards but zero REP after a reload.
/// </summary>
public class ReputationManager : MonoBehaviour
{
    const string BalanceKey = GameSaveKeys.ReputationBalance;
    const string BlueDaoReputationClaimedKey = GameSaveKeys.BlueDaoReputationClaimed;
    const string RedDaoDonationClaimedKey = GameSaveKeys.RedDaoDonationClaimed;

    static ReputationManager _instance;

    int _reputation;

    [SerializeField] float reputationPopupDuration = 2.5f;
    [SerializeField] float floatingToastDuration = 2f;

    float _reputationPopupTimer;
    string _reputationPopupLine1;
    string _reputationPopupLine2;

    int _floatingToastAmount;
    float _floatingToastTimer;

    public static ReputationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ReputationManager>();
            }

            return _instance;
        }
    }

    public int Reputation => _reputation;

    public bool IsShowingReputationPopup => _reputationPopupTimer > 0f;
    public string ReputationPopupLine1 => _reputationPopupLine1;
    public string ReputationPopupLine2 => _reputationPopupLine2;

    public bool IsShowingFloatingToast => _floatingToastTimer > 0f;
    public int FloatingToastAmount => _floatingToastAmount;
    public float FloatingToastAlpha =>
        floatingToastDuration > 0f ? Mathf.Clamp01(_floatingToastTimer / floatingToastDuration) : 0f;

    public bool HasClaimedBlueDaoReputation =>
        PlayerPrefs.GetInt(BlueDaoReputationClaimedKey, 0) == 1;

    public bool HasDonatedRedDao =>
        PlayerPrefs.GetInt(RedDaoDonationClaimedKey, 0) == 1;

    public event Action<int> OnReputationChanged;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        LoadReputation();
    }

    void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    void Update()
    {
        if (_reputationPopupTimer > 0f)
        {
            _reputationPopupTimer -= Time.deltaTime;
        }

        if (_floatingToastTimer > 0f)
        {
            _floatingToastTimer -= Time.deltaTime;
        }
    }

    void LoadReputation()
    {
        if (PlayerPrefs.HasKey(BalanceKey))
        {
            _reputation = Mathf.Max(0, PlayerPrefs.GetInt(BalanceKey, 0));
            return;
        }

        int migratedReputation = 0;
        if (HasClaimedBlueDaoReputation)
        {
            migratedReputation += 10;
        }

        if (HasDonatedRedDao)
        {
            migratedReputation += CivilizationBonuses.DefaultRedDonationReputation;
        }

        _reputation = migratedReputation;
        if (_reputation > 0)
        {
            SaveReputation();
        }
    }

    void SaveReputation()
    {
        PlayerPrefs.SetInt(BalanceKey, _reputation);
        PlayerPrefs.Save();
    }

    public int GetReputation()
    {
        return _reputation;
    }

    public void AddReputation(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _reputation += amount;
        SaveReputation();
        ShowFloatingToast(amount);
        OnReputationChanged?.Invoke(_reputation);
    }

    void ShowFloatingToast(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _floatingToastAmount = amount;
        _floatingToastTimer = floatingToastDuration;
    }

    public void RemoveReputation(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _reputation = Mathf.Max(0, _reputation - amount);
        SaveReputation();
        OnReputationChanged?.Invoke(_reputation);
    }

    public void ShowReputationPopup(string line1, string line2, float duration = -1f)
    {
        _reputationPopupLine1 = line1 ?? string.Empty;
        _reputationPopupLine2 = line2 ?? string.Empty;
        _reputationPopupTimer = duration > 0f ? duration : reputationPopupDuration;
    }

    public bool TryClaimBlueDaoSteleReputation(
        int amount = 10,
        string messageLine1 = "You helped an open collaboration civilization.",
        string messageLine2 = "Reputation +10")
    {
        if (HasClaimedBlueDaoReputation || amount <= 0)
        {
            return false;
        }

        AddReputation(amount);
        PlayerPrefs.SetInt(BlueDaoReputationClaimedKey, 1);
        PlayerPrefs.Save();
        ShowReputationPopup(messageLine1, messageLine2);
        CivilizationJournalLog.RecordReputationRewardOnce("blue_dao_rep", amount);
        return true;
    }

    public void ResetDemoState()
    {
        _reputation = 0;
        _reputationPopupTimer = 0f;
        _reputationPopupLine1 = null;
        _reputationPopupLine2 = null;
        _floatingToastTimer = 0f;
        _floatingToastAmount = 0;
        PlayerPrefs.DeleteKey(BalanceKey);
        PlayerPrefs.DeleteKey(BlueDaoReputationClaimedKey);
        PlayerPrefs.DeleteKey(RedDaoDonationClaimedKey);
        PlayerPrefs.Save();
        OnReputationChanged?.Invoke(_reputation);
    }

    public bool TryClaimRedDaoDonationReputation(
        int amount = 20,
        string messageLine1 = "You funded a trade civilization.",
        string messageLine2 = "Reputation +20")
    {
        if (HasDonatedRedDao || amount <= 0)
        {
            return false;
        }

        AddReputation(amount);
        PlayerPrefs.SetInt(RedDaoDonationClaimedKey, 1);
        PlayerPrefs.Save();
        ShowReputationPopup(messageLine1, messageLine2);
        CivilizationJournalLog.RecordReputationRewardOnce("red_dao_donation_rep", amount);
        return true;
    }
}
