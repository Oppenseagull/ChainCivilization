using System;
using UnityEngine;

/// <summary>
/// Tracks DAO Pass credentials (persisted via PlayerPrefs) and drives credential popups.
/// </summary>
public class DAOPassManager : MonoBehaviour
{
    const string PassKeyPrefix = "dao_pass_";
    public const string GreenPassId = "green";

    static DAOPassManager _instance;

    [SerializeField] float credentialPopupDuration = 3.5f;

    float _credentialPopupTimer;
    string _credentialPassName;

    public static DAOPassManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<DAOPassManager>();
            }

            return _instance;
        }
    }

    public bool IsShowingCredentialPopup => _credentialPopupTimer > 0f;
    public string CredentialPassName => _credentialPassName;

    public event Action<string> OnPassGranted;

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;
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
        if (_credentialPopupTimer > 0f)
        {
            _credentialPopupTimer -= Time.deltaTime;
        }
    }

    public bool HasPass(string passId)
    {
        return PlayerPrefs.GetInt(PassKeyPrefix + passId, 0) == 1;
    }

    public bool HasGreenPass => HasPass(GreenPassId);

    public bool TryGrantPass(string passId, string displayName)
    {
        if (string.IsNullOrEmpty(passId) || HasPass(passId))
        {
            return false;
        }

        PlayerPrefs.SetInt(PassKeyPrefix + passId, 1);
        PlayerPrefs.Save();
        ShowCredentialPopup(displayName);
        CivilizationJournalLog.RecordPass(passId, displayName);
        OnPassGranted?.Invoke(passId);
        return true;
    }

    public void ResetDemoState()
    {
        _credentialPopupTimer = 0f;
        _credentialPassName = null;
        PlayerPrefs.DeleteKey(PassKeyPrefix + GreenPassId);
        PlayerPrefs.Save();
    }

    public bool TryGrantGreenPass(string displayName = "Green DAO Pass")
    {
        return TryGrantPass(GreenPassId, displayName);
    }

    public void ShowCredentialPopup(string passName, float duration = -1f)
    {
        if (string.IsNullOrEmpty(passName))
        {
            return;
        }

        _credentialPassName = passName;
        _credentialPopupTimer = duration > 0f ? duration : credentialPopupDuration;
    }
}
