using UnityEngine;

/// <summary>
/// Registry of all PlayerPrefs keys used by Chain Civilization.
/// Restart Game clears every key via PlayerPrefs.DeleteAll().
/// </summary>
public static class GameSaveKeys
{
    // TokenManager
    public const string MoonBalance = "moon_balance";
    public const string RedDaoSteleClaimed = "red_dao_stele_claimed";

    // ReputationManager
    public const string ReputationBalance = "reputation_balance";
    public const string BlueDaoReputationClaimed = "blue_dao_reputation_claimed";
    public const string RedDaoDonationClaimed = "red_dao_donation_claimed";

    // Civilization choice
    public const string SelectedCivilizationType = "selected_civilization_type";
    public const string SelectedJoinRule = "selected_join_rule";

    // DAOPassManager
    public const string DaoPassPrefix = "dao_pass_";
    public const string GreenPass = "dao_pass_green";

    // MoonCrystalInteract
    public const string MoonCrystalCollectedPrefix = "moon_crystal_collected_";
    public const string MoonCrystal01 = "moon_crystal_collected_crystal_01";
    public const string MoonCrystal02 = "moon_crystal_collected_crystal_02";
    public const string MoonCrystal03 = "moon_crystal_collected_crystal_03";
    public const string MoonCrystal04 = "moon_crystal_collected_crystal_04";
    public const string MoonCrystal05 = "moon_crystal_collected_crystal_05";

    // WelcomeIntroUI
    public const string WelcomeIntroSeen = "chain_civ_welcome_seen";

    // DAO intro cards (first visit only)
    public const string DaoIntroBlueSeen = "dao_intro_blue_seen";
    public const string DaoIntroRedSeen = "dao_intro_red_seen";
    public const string DaoIntroGreenSeen = "dao_intro_green_seen";

    /// <summary>
    /// Session-only (not PlayerPrefs): QuestSignals, MainQuestManager step.
    /// Cleared in GameResetManager.ResetSessionState() before scene reload.
    /// </summary>
    public static readonly string[] AllPlayerPrefsKeys =
    {
        MoonBalance,
        RedDaoSteleClaimed,
        ReputationBalance,
        BlueDaoReputationClaimed,
        RedDaoDonationClaimed,
        SelectedCivilizationType,
        SelectedJoinRule,
        GreenPass,
        MoonCrystal01,
        MoonCrystal02,
        MoonCrystal03,
        MoonCrystal04,
        MoonCrystal05,
        WelcomeIntroSeen,
        DaoIntroBlueSeen,
        DaoIntroRedSeen,
        DaoIntroGreenSeen
    };

    public static void DeleteAllSavedData()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
