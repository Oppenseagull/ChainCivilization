using System;
using UnityEngine;

/// <summary>
/// Manages player MOON token balance (persisted via PlayerPrefs).
/// </summary>
public class TokenManager : MonoBehaviour
{
    const string BalanceKey = "moon_balance";
    const string RedDaoSteleClaimedKey = "red_dao_stele_claimed";

    static TokenManager _instance;

    public static TokenManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<TokenManager>();
            }

            return _instance;
        }
    }

    public int MoonBalance { get; private set; }

    public event Action<int> OnMoonBalanceChanged;

    [SerializeField] float rewardPopupDuration = 2.5f;

    float _rewardPopupTimer;
    string _rewardPopupText;

    public bool IsShowingRewardPopup => _rewardPopupTimer > 0f;
    public string RewardPopupText => _rewardPopupText;

    public bool HasClaimedRedDaoStele =>
        PlayerPrefs.GetInt(RedDaoSteleClaimedKey, 0) == 1;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;
        LoadBalance();
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
        if (_rewardPopupTimer > 0f)
        {
            _rewardPopupTimer -= Time.deltaTime;
        }
    }

    void LoadBalance()
    {
        MoonBalance = PlayerPrefs.GetInt(BalanceKey, 0);
    }

    void SaveBalance()
    {
        PlayerPrefs.SetInt(BalanceKey, MoonBalance);
        PlayerPrefs.Save();
    }

    public void AddMoon(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        MoonBalance += amount;
        SaveBalance();
        OnMoonBalanceChanged?.Invoke(MoonBalance);
    }

    public bool TrySpendMoon(int amount)
    {
        if (amount <= 0 || MoonBalance < amount)
        {
            return false;
        }

        MoonBalance -= amount;
        SaveBalance();
        OnMoonBalanceChanged?.Invoke(MoonBalance);
        return true;
    }

    public void ShowRewardPopup(string text, float duration = -1f)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        _rewardPopupText = text;
        _rewardPopupTimer = duration > 0f ? duration : rewardPopupDuration;
    }

    /// <summary>
    /// First-time Red DAO stele reward (default 100 MOON).
    /// </summary>
    public void ResetDemoState()
    {
        MoonBalance = 0;
        _rewardPopupTimer = 0f;
        _rewardPopupText = null;
        PlayerPrefs.DeleteKey(BalanceKey);
        PlayerPrefs.DeleteKey(RedDaoSteleClaimedKey);
        PlayerPrefs.Save();
        OnMoonBalanceChanged?.Invoke(MoonBalance);
    }

    public bool TryClaimRedDaoSteleReward(int amount = 100)
    {
        if (HasClaimedRedDaoStele || amount <= 0)
        {
            return false;
        }

        AddMoon(amount);
        PlayerPrefs.SetInt(RedDaoSteleClaimedKey, 1);
        PlayerPrefs.Save();
        CivilizationJournalLog.RecordMoonRewardOnce("red_dao_stele_moon", amount);
        return true;
    }
}
