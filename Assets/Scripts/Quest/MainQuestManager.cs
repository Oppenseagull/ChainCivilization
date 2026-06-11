using System.Text;
using UnityEngine;

/// <summary>
/// Linear main-quest tracker: always shows current goal, auto-advances on completion.
/// Session-only progress; reconciles with persisted game state on start.
/// </summary>
public class MainQuestManager : MonoBehaviour
{
    enum MainQuestStep
    {
        VisitBlueDao,
        VisitRedDao,
        CollectMoon,
        BuildReputation,
        JoinGreenDao,
        GoToBoundary,
        CreateCivilization,
        Complete
    }

    enum NotificationPhase
    {
        None,
        QuestComplete,
        NewQuest
    }

    struct StepData
    {
        public string Title;
        public string Description;
        public string Meaning;
        public string TargetObjectName;
        public string[] GuideSteps;
    }

    const int RequiredReputation = 20;
    const int RequiredMoon = 200;
    const float MapHintDistance = 60f;
    const float ApproachDistance = 18f;
    const float SteleInteractDistance = 14f;
    const float CrystalInteractDistance = 4f;
    const float BoundaryTriggerDistance = 10f;
    const float SeedInteractDistance = 6f;

    static readonly StepData[] Steps =
    {
        new StepData
        {
            Title = "寻找 Blue DAO",
            Description = "寻找蓝色文明遗迹。",
            Meaning = "理解不同文明拥有不同规则。",
            TargetObjectName = "BlueDAO_Core",
            GuideSteps = new[]
            {
                "打开地图(Tab)",
                "寻找蓝色区域",
                "靠近石碑",
                "按 E 进行交互"
            }
        },
        new StepData
        {
            Title = "访问 Red DAO",
            Description = "前往 Red DAO 领取 MOON。",
            Meaning = "获得你的第一枚 Token。",
            TargetObjectName = "RedDAO_Core",
            GuideSteps = new[]
            {
                "打开地图(Tab)",
                "寻找红色区域",
                "靠近石碑",
                "按 E 获得 100 MOON"
            }
        },
        new StepData
        {
            Title = "获得 MOON",
            Description = "累计获得 200 MOON。",
            Meaning = "积累链上资源，为文明准入做准备。",
            TargetObjectName = string.Empty,
            GuideSteps = new[]
            {
                "前往 Crystal 区域",
                "寻找漂浮 Crystal",
                "靠近后按 E",
                "累计获得 200 MOON"
            }
        },
        new StepData
        {
            Title = "建立信誉",
            Description = "累计获得 20 Reputation。",
            Meaning = "建立可信记录，获得社区认可。",
            TargetObjectName = "RedDAO_Core",
            GuideSteps = new[]
            {
                "前往 Blue DAO 按 E 获得信誉",
                "或 Red DAO 捐赠获得信誉",
                "累计 REP ≥ 20"
            }
        },
        new StepData
        {
            Title = "获得 Green Pass",
            Description = "证明你的准入资格。",
            Meaning = "体验准入机制与链上身份。",
            TargetObjectName = "GreenDAO_Core",
            GuideSteps = new[]
            {
                "确保 REP ≥ 20",
                "确保 MOON ≥ 200",
                "前往 Green DAO",
                "进入绿色区域",
                "自动获得 Green Pass"
            }
        },
        new StepData
        {
            Title = "前往文明边界",
            Description = "寻找文明边界石碑。",
            Meaning = "抵达文明边界，定义你的势力范围。",
            TargetObjectName = "BoundaryStone",
            GuideSteps = new[]
            {
                "打开地图",
                "寻找 Boundary Stone",
                "进入边界区域",
                "阅读文明创建说明"
            }
        },
        new StepData
        {
            Title = "创建文明",
            Description = "与 Civilization Seed 互动。",
            Meaning = "建立属于你的规则与共识。",
            TargetObjectName = "CivilizationSeed",
            GuideSteps = new[]
            {
                "前往 Civilization Seed",
                "按 E 打开规则面板",
                "选择文明规则",
                "完成文明创建"
            }
        }
    };

    static readonly Vector3 MoonCrystalFallback = new Vector3(200f, 0f, 25f);

    [SerializeField] float pollInterval = 0.5f;
    [SerializeField] float completeNoticeDuration = 2f;
    [SerializeField] float newQuestNoticeDuration = 2f;

    MainQuestStep _currentStep = MainQuestStep.VisitBlueDao;
    float _pollTimer;
    bool _suppressNotifications;

    NotificationPhase _notificationPhase = NotificationPhase.None;
    float _notificationTimer;
    string _completedQuestTitle;
    string _nextQuestTitle;

    GUIStyle _boxStyle;
    GUIStyle _headerStyle;
    GUIStyle _titleStyle;
    GUIStyle _distanceStyle;
    GUIStyle _stepsStyle;
    GUIStyle _nextActionHeaderStyle;
    GUIStyle _nextActionStyle;
    GUIStyle _popupPanelStyle;
    GUIStyle _popupHeaderStyle;
    GUIStyle _popupTitleStyle;
    GUIStyle _popupAccentStyle;
    bool _stylesReady;

    void OnEnable()
    {
        TokenManager tokens = TokenManager.Instance;
        if (tokens != null)
        {
            tokens.OnMoonBalanceChanged += HandleMoonBalanceChanged;
        }

        DAOPassManager passes = DAOPassManager.Instance;
        if (passes != null)
        {
            passes.OnPassGranted += HandlePassGranted;
        }

        CivilizationManager.OnCivilizationSelected += HandleCivilizationSelected;
        QuestSignals.OnChanged += HandleQuestSignalsChanged;

        ReputationManager reputation = ReputationManager.Instance;
        if (reputation != null)
        {
            reputation.OnReputationChanged += HandleReputationChanged;
        }
    }

    void Start()
    {
        _suppressNotifications = true;
        SyncToWorldState();
        _suppressNotifications = false;
    }

    void OnDisable()
    {
        TokenManager tokens = TokenManager.Instance;
        if (tokens != null)
        {
            tokens.OnMoonBalanceChanged -= HandleMoonBalanceChanged;
        }

        DAOPassManager passes = DAOPassManager.Instance;
        if (passes != null)
        {
            passes.OnPassGranted -= HandlePassGranted;
        }

        CivilizationManager.OnCivilizationSelected -= HandleCivilizationSelected;
        QuestSignals.OnChanged -= HandleQuestSignalsChanged;

        ReputationManager reputation = ReputationManager.Instance;
        if (reputation != null)
        {
            reputation.OnReputationChanged -= HandleReputationChanged;
        }
    }

    void Update()
    {
        UpdateNotification();

        if (_notificationPhase != NotificationPhase.None)
        {
            return;
        }

        _pollTimer -= Time.deltaTime;
        if (_pollTimer <= 0f)
        {
            _pollTimer = pollInterval;
            TryAdvance();
        }
    }

    void HandleMoonBalanceChanged(int _) => TryAdvance();
    void HandlePassGranted(string _) => TryAdvance();
    void HandleCivilizationSelected(CivilizationType _) => TryAdvance();
    void HandleQuestSignalsChanged() => TryAdvance();
    void HandleReputationChanged(int _) => TryAdvance();

    void SyncToWorldState()
    {
        while (_currentStep != MainQuestStep.Complete && IsStepComplete(_currentStep))
        {
            _currentStep++;
        }
    }

    void TryAdvance()
    {
        if (_notificationPhase != NotificationPhase.None)
        {
            return;
        }

        if (_currentStep == MainQuestStep.Complete)
        {
            return;
        }

        if (!IsStepComplete(_currentStep))
        {
            return;
        }

        string completedTitle = GetStepTitle(_currentStep);
        _currentStep++;

        if (_suppressNotifications)
        {
            if (_currentStep != MainQuestStep.Complete && IsStepComplete(_currentStep))
            {
                TryAdvance();
            }

            return;
        }

        BeginCompletionNotice(completedTitle);
    }

    void BeginCompletionNotice(string completedTitle)
    {
        _completedQuestTitle = completedTitle;

        if (_currentStep == MainQuestStep.Complete)
        {
            _nextQuestTitle = string.Empty;
            _notificationPhase = NotificationPhase.QuestComplete;
            _notificationTimer = completeNoticeDuration;
            return;
        }

        _nextQuestTitle = GetStepTitle(_currentStep);
        _notificationPhase = NotificationPhase.QuestComplete;
        _notificationTimer = completeNoticeDuration;
    }

    void UpdateNotification()
    {
        if (_notificationPhase == NotificationPhase.None)
        {
            return;
        }

        _notificationTimer -= Time.deltaTime;
        if (_notificationTimer > 0f)
        {
            return;
        }

        if (_notificationPhase == NotificationPhase.QuestComplete)
        {
            if (_currentStep != MainQuestStep.Complete && !string.IsNullOrEmpty(_nextQuestTitle))
            {
                _notificationPhase = NotificationPhase.NewQuest;
                _notificationTimer = newQuestNoticeDuration;
                return;
            }

            _notificationPhase = NotificationPhase.None;
            TryAdvance();
            return;
        }

        _notificationPhase = NotificationPhase.None;
        TryAdvance();
    }

    bool IsStepComplete(MainQuestStep step)
    {
        TokenManager tokens = TokenManager.Instance;
        DAOPassManager passes = DAOPassManager.Instance;
        ReputationManager reputation = ReputationManager.Instance;

        switch (step)
        {
            case MainQuestStep.VisitBlueDao:
                return QuestSignals.BlueDaoVisited
                    || (tokens != null && tokens.HasClaimedRedDaoStele);

            case MainQuestStep.VisitRedDao:
                return tokens != null && tokens.HasClaimedRedDaoStele;

            case MainQuestStep.CollectMoon:
                return tokens != null && tokens.MoonBalance >= 200;

            case MainQuestStep.BuildReputation:
                return (reputation != null && reputation.GetReputation() >= RequiredReputation)
                    || (passes != null && passes.HasGreenPass);

            case MainQuestStep.JoinGreenDao:
                return passes != null && passes.HasGreenPass;

            case MainQuestStep.GoToBoundary:
                return QuestSignals.BoundaryLoreComplete
                    || CivilizationManager.HasSelectedCivilization;

            case MainQuestStep.CreateCivilization:
                return CivilizationManager.HasSelectedCivilization;

            default:
                return true;
        }
    }

    static string GetStepTitle(MainQuestStep step)
    {
        int index = (int)step;
        if (index < 0 || index >= Steps.Length)
        {
            return string.Empty;
        }

        return Steps[index].Title;
    }

    public int CurrentQuestIndex => (int)_currentStep;

    public bool IsAllQuestsComplete => _currentStep == MainQuestStep.Complete;

    public const int DemoMilestoneTotal = 6;

    static readonly int[] DemoMilestoneQuestIndices = { 0, 1, 2, 4, 5, 6 };

    public static int QuestStepCount => Steps.Length;

    /// <summary>
    /// Demo guide: milestone complete when quest has advanced past this step index.
    /// questStepIndex -1 = birth (always complete in play mode).
    /// </summary>
    public bool IsDemoMilestoneComplete(int questStepIndex)
    {
        if (questStepIndex < 0)
        {
            return true;
        }

        if (_currentStep == MainQuestStep.Complete)
        {
            return true;
        }

        return (int)_currentStep > questStepIndex;
    }

    /// <summary>
    /// Demo guide: highlight Green Pass row during BuildReputation and JoinGreenDao.
    /// </summary>
    public bool IsDemoGuideRowCurrent(int questStepIndex)
    {
        if (_currentStep == MainQuestStep.Complete)
        {
            return false;
        }

        if (questStepIndex < 0)
        {
            return false;
        }

        if (questStepIndex == (int)MainQuestStep.JoinGreenDao)
        {
            return _currentStep == MainQuestStep.BuildReputation
                || _currentStep == MainQuestStep.JoinGreenDao;
        }

        return (int)_currentStep == questStepIndex;
    }

    public void ResetProgress()
    {
        _currentStep = MainQuestStep.VisitBlueDao;
        _pollTimer = 0f;
        _notificationPhase = NotificationPhase.None;
        _notificationTimer = 0f;
        _completedQuestTitle = null;
        _nextQuestTitle = null;
    }

    public int GetDemoCompletedMilestoneCount()
    {
        int count = 0;
        for (int i = 0; i < DemoMilestoneQuestIndices.Length; i++)
        {
            if (IsDemoMilestoneComplete(DemoMilestoneQuestIndices[i]))
            {
                count++;
            }
        }

        return count;
    }

    public const int CivilizationProgressTotal = 6;

    public static string GetCivilizationProgressLabel(int rowIndex)
    {
        switch (rowIndex)
        {
            case 0: return "Blue DAO";
            case 1: return "Red DAO";
            case 2: return "Green DAO";
            case 3: return "Green Pass";
            case 4: return "Boundary";
            case 5: return "Civilization";
            default: return string.Empty;
        }
    }

    /// <summary>
    /// Civilization Progress HUD rows — derived from quest step state only.
    /// </summary>
    public bool IsCivilizationProgressComplete(int rowIndex)
    {
        if (_currentStep == MainQuestStep.Complete)
        {
            return true;
        }

        switch (rowIndex)
        {
            case 0:
                return IsDemoMilestoneComplete((int)MainQuestStep.VisitBlueDao);
            case 1:
                return IsDemoMilestoneComplete((int)MainQuestStep.VisitRedDao);
            case 2:
                return (int)_currentStep >= (int)MainQuestStep.JoinGreenDao;
            case 3:
                return IsDemoMilestoneComplete((int)MainQuestStep.JoinGreenDao);
            case 4:
                return IsDemoMilestoneComplete((int)MainQuestStep.GoToBoundary);
            case 5:
                return IsDemoMilestoneComplete((int)MainQuestStep.CreateCivilization);
            default:
                return false;
        }
    }

    public int GetCivilizationProgressCompletedCount()
    {
        int count = 0;
        for (int i = 0; i < CivilizationProgressTotal; i++)
        {
            if (IsCivilizationProgressComplete(i))
            {
                count++;
            }
        }

        return count;
    }

    public int GetCivilizationProgressPercent()
    {
        return Mathf.RoundToInt(100f * GetCivilizationProgressCompletedCount() / (float)CivilizationProgressTotal);
    }

    public struct QuestHudSnapshot
    {
        public bool IsVisible;
        public string Title;
        public string StepsText;
        public string NextActionText;
        public string DistanceText;
    }

    public QuestHudSnapshot GetQuestHudSnapshot()
    {
        QuestHudSnapshot snapshot = default;
        if (_currentStep == MainQuestStep.Complete || _notificationPhase != NotificationPhase.None)
        {
            return snapshot;
        }

        int index = (int)_currentStep;
        if (index < 0 || index >= Steps.Length)
        {
            return snapshot;
        }

        StepData step = Steps[index];
        float distance = GetDistanceToTarget(_currentStep);
        snapshot.IsVisible = true;
        snapshot.Title = step.Title;
        snapshot.StepsText = FormatGuideSteps(index, true);
        snapshot.NextActionText = GetQuestNextAction(index);
        snapshot.DistanceText = distance >= 0f ? $"距离：{Mathf.RoundToInt(distance)}m" : string.Empty;
        return snapshot;
    }

    public static string GetQuestTitle(int index)
    {
        if (index < 0 || index >= Steps.Length)
        {
            return string.Empty;
        }

        return Steps[index].Title;
    }

    public static string GetQuestDescription(int index)
    {
        if (index < 0 || index >= Steps.Length)
        {
            return string.Empty;
        }

        return Steps[index].Description;
    }

    public static string GetQuestMeaning(int index)
    {
        if (index < 0 || index >= Steps.Length)
        {
            return string.Empty;
        }

        return Steps[index].Meaning;
    }

    public string GetQuestNextAction(int index)
    {
        if (index < 0 || index >= Steps.Length)
        {
            return string.Empty;
        }

        if (_currentStep == MainQuestStep.Complete || index < (int)_currentStep)
        {
            return string.Empty;
        }

        if (index > (int)_currentStep)
        {
            string[] steps = Steps[index].GuideSteps;
            return steps != null && steps.Length > 0 ? steps[0] : string.Empty;
        }

        return ComputeNextAction(_currentStep);
    }

    public static string FormatGuideSteps(int index, bool includeHeader)
    {
        if (index < 0 || index >= Steps.Length)
        {
            return string.Empty;
        }

        MainQuestManager manager = FindFirstObjectByType<MainQuestManager>();
        string[] steps = manager != null
            ? manager.GetEffectiveGuideSteps((MainQuestStep)index)
            : Steps[index].GuideSteps;

        if (steps == null || steps.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new StringBuilder();
        if (includeHeader)
        {
            builder.AppendLine("【完成步骤】");
        }

        for (int i = 0; i < steps.Length; i++)
        {
            builder.Append(i + 1).Append(". ").AppendLine(steps[i]);
        }

        if (builder.Length > 0 && builder[builder.Length - 1] == '\n')
        {
            builder.Length--;
        }

        return builder.ToString();
    }

    string[] GetEffectiveGuideSteps(MainQuestStep step)
    {
        int index = (int)step;
        if (index < 0 || index >= Steps.Length)
        {
            return null;
        }

        string[] source = Steps[index].GuideSteps;
        if (source == null || source.Length == 0)
        {
            return source;
        }

        string[] steps = (string[])source.Clone();
        TokenManager tokens = TokenManager.Instance;
        ReputationManager reputation = ReputationManager.Instance;
        int moon = tokens != null ? tokens.MoonBalance : 0;
        int rep = reputation != null ? reputation.GetReputation() : 0;

        switch (step)
        {
            case MainQuestStep.CollectMoon:
                if (steps.Length > 3)
                {
                    steps[3] = $"累计获得 {RequiredMoon} MOON（当前 {moon}）";
                }

                break;

            case MainQuestStep.BuildReputation:
                if (steps.Length > 2)
                {
                    steps[2] = $"累计 REP ≥ {RequiredReputation}（当前 {rep}）";
                }

                break;

            case MainQuestStep.JoinGreenDao:
                if (steps.Length > 1)
                {
                    steps[0] = $"确保 REP ≥ {RequiredReputation}（当前 {rep}）";
                    steps[1] = $"确保 MOON ≥ {RequiredMoon}（当前 {moon}）";
                }

                break;
        }

        return steps;
    }

    string ComputeNextAction(MainQuestStep step)
    {
        TokenManager tokens = TokenManager.Instance;
        ReputationManager reputation = ReputationManager.Instance;
        DAOPassManager passes = DAOPassManager.Instance;
        int moon = tokens != null ? tokens.MoonBalance : 0;
        int rep = reputation != null ? reputation.GetReputation() : 0;

        switch (step)
        {
            case MainQuestStep.VisitBlueDao:
                if (TryGetObjectDistance("BlueDAO_Core", out float blueDistance))
                {
                    if (blueDistance > MapHintDistance)
                    {
                        return "打开地图(Tab)，寻找蓝色区域";
                    }

                    if (blueDistance > SteleInteractDistance)
                    {
                        return "靠近 Blue DAO 石碑";
                    }
                }

                return "按 E 进行交互";

            case MainQuestStep.VisitRedDao:
                if (TryGetObjectDistance("RedDAO_Core", out float redDistance))
                {
                    if (redDistance > MapHintDistance)
                    {
                        return "打开地图(Tab)，寻找红色区域";
                    }

                    if (redDistance > SteleInteractDistance)
                    {
                        return "靠近 Red DAO 石碑";
                    }
                }

                return "按 E 获得 100 MOON";

            case MainQuestStep.CollectMoon:
                if (moon >= RequiredMoon)
                {
                    return "MOON 已达标，继续推进主线";
                }

                if (TryGetNearestMoonCrystalDistance(out float crystalDistance))
                {
                    if (crystalDistance > MapHintDistance)
                    {
                        return "前往 Crystal 区域，寻找漂浮 Crystal";
                    }

                    if (crystalDistance > CrystalInteractDistance)
                    {
                        return "靠近 Crystal 后按 E 收集";
                    }

                    return "按 E 收集 Crystal";
                }

                return $"继续收集 MOON（当前 {moon}/{RequiredMoon}）";

            case MainQuestStep.BuildReputation:
                if (rep >= RequiredReputation)
                {
                    return "前往 Green DAO 申请 Green Pass";
                }

                if (reputation != null && !reputation.HasClaimedBlueDaoReputation
                    && TryGetObjectDistance("BlueDAO_Core", out float blueRepDistance)
                    && blueRepDistance <= ApproachDistance)
                {
                    return "在 Blue DAO 按 E 获得信誉";
                }

                if (tokens != null && tokens.HasClaimedRedDaoStele
                    && reputation != null && !reputation.HasDonatedRedDao
                    && tokens.MoonBalance >= 50
                    && TryGetObjectDistance("RedDAO_Core", out float redRepDistance)
                    && redRepDistance <= ApproachDistance)
                {
                    return "在 Red DAO 按 E 捐赠获得信誉";
                }

                return $"积累信誉（当前 REP {rep}/{RequiredReputation}）";

            case MainQuestStep.JoinGreenDao:
                if (passes != null && passes.HasGreenPass)
                {
                    return "Green Pass 已获得";
                }

                if (rep < RequiredReputation)
                {
                    return $"提升 REP 至 {RequiredReputation}（当前 {rep}）";
                }

                if (moon < RequiredMoon)
                {
                    return $"收集 MOON 至 {RequiredMoon}（当前 {moon}）";
                }

                if (TryGetObjectDistance("GreenDAO_EntryZone", out float entryDistance)
                    && entryDistance <= ApproachDistance)
                {
                    return "进入绿色区域，自动获得 Green Pass";
                }

                if (TryGetObjectDistance("GreenDAO_Core", out float greenDistance))
                {
                    if (greenDistance > MapHintDistance)
                    {
                        return "打开地图，前往 Green DAO";
                    }

                    return "进入 Green DAO 绿色区域";
                }

                return "前往 Green DAO";

            case MainQuestStep.GoToBoundary:
                if (QuestSignals.BoundaryLoreComplete)
                {
                    return "已阅读边界说明，前往创建文明";
                }

                if (passes == null || !passes.HasGreenPass)
                {
                    return "先获得 Green Pass";
                }

                if (TryGetObjectDistance("BoundaryStone", out float boundaryDistance))
                {
                    if (boundaryDistance > MapHintDistance)
                    {
                        return "打开地图，寻找 Boundary Stone";
                    }

                    if (boundaryDistance > BoundaryTriggerDistance)
                    {
                        return "进入边界区域";
                    }
                }

                return "按 E 阅读文明创建说明";

            case MainQuestStep.CreateCivilization:
                if (CivilizationManager.HasSelectedCivilization)
                {
                    return "文明已创建";
                }

                if (TryGetObjectDistance("CivilizationSeed", out float seedDistance))
                {
                    if (seedDistance > MapHintDistance)
                    {
                        return "前往 Civilization Seed";
                    }

                    if (seedDistance > SeedInteractDistance)
                    {
                        return "靠近 Civilization Seed";
                    }
                }

                return "按 E 打开规则面板并创建文明";

            default:
                return string.Empty;
        }
    }

    bool TryGetObjectDistance(string objectName, out float distance)
    {
        distance = -1f;
        if (string.IsNullOrEmpty(objectName))
        {
            return false;
        }

        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            return false;
        }

        distance = Vector3.Distance(transform.position, target.transform.position);
        return true;
    }

    bool TryGetNearestMoonCrystalDistance(out float distance)
    {
        distance = -1f;
        MoonCrystalInteract[] crystals = FindObjectsByType<MoonCrystalInteract>(FindObjectsSortMode.None);
        if (crystals == null || crystals.Length == 0)
        {
            distance = Vector3.Distance(transform.position, MoonCrystalFallback);
            return true;
        }

        float nearestDistance = float.MaxValue;
        Vector3 playerPosition = transform.position;

        for (int i = 0; i < crystals.Length; i++)
        {
            MoonCrystalInteract crystal = crystals[i];
            if (crystal == null)
            {
                continue;
            }

            float crystalDistance = Vector3.Distance(playerPosition, crystal.transform.position);
            if (crystalDistance < nearestDistance)
            {
                nearestDistance = crystalDistance;
            }
        }

        distance = nearestDistance;
        return true;
    }

    void OnGUI()
    {
        EnsureStyles();

        if (_notificationPhase != NotificationPhase.None)
        {
            DrawNotificationPopup();
            return;
        }

        if (!GameHUDCanvas.IsActive)
        {
            DrawTracker();
        }
    }

    void DrawTracker()
    {
        if (_currentStep == MainQuestStep.Complete)
        {
            return;
        }

        int index = (int)_currentStep;
        if (index < 0 || index >= Steps.Length)
        {
            return;
        }

        StepData step = Steps[index];
        float distance = GetDistanceToTarget(_currentStep);
        string distanceText = distance >= 0f ? $"距离：{Mathf.RoundToInt(distance)}m" : string.Empty;
        string stepsText = FormatGuideSteps(index, true);
        string nextAction = GetQuestNextAction(index);

        const float width = 380f;
        const float margin = 14f;
        const float innerPad = 12f;
        float contentWidth = width - innerPad * 2f;

        float contentHeight = 22f + 26f + 6f;
        if (!string.IsNullOrEmpty(distanceText))
        {
            contentHeight += 20f;
        }

        contentHeight += _stepsStyle.CalcHeight(new GUIContent(stepsText), contentWidth) + 8f;
        contentHeight += 20f;
        contentHeight += _nextActionStyle.CalcHeight(new GUIContent(nextAction), contentWidth);
        contentHeight += innerPad * 2f;

        float boxY = margin;
        GUI.Box(new Rect(margin, boxY, width, contentHeight), GUIContent.none, _boxStyle);

        float lineY = boxY + innerPad;
        float lineX = margin + innerPad;

        GUI.Label(new Rect(lineX, lineY, contentWidth, 22f), "【当前目标】", _headerStyle);
        lineY += 24f;
        GUI.Label(new Rect(lineX, lineY, contentWidth, 26f), step.Title, _titleStyle);
        lineY += 28f;

        if (!string.IsNullOrEmpty(distanceText))
        {
            GUI.Label(new Rect(lineX, lineY, contentWidth, 20f), distanceText, _distanceStyle);
            lineY += 22f;
        }

        lineY += 4f;
        float stepsHeight = _stepsStyle.CalcHeight(new GUIContent(stepsText), contentWidth);
        GUI.Label(new Rect(lineX, lineY, contentWidth, stepsHeight), stepsText, _stepsStyle);
        lineY += stepsHeight + 6f;

        GUI.Label(new Rect(lineX, lineY, contentWidth, 18f), "【下一步】", _nextActionHeaderStyle);
        lineY += 20f;
        float nextHeight = _nextActionStyle.CalcHeight(new GUIContent(nextAction), contentWidth);
        GUI.Label(new Rect(lineX, lineY, contentWidth, nextHeight), nextAction, _nextActionStyle);
    }

    float GetDistanceToTarget(MainQuestStep step)
    {
        if (!TryGetTargetPosition(step, out Vector3 targetPosition))
        {
            return -1f;
        }

        return Vector3.Distance(transform.position, targetPosition);
    }

    bool TryGetTargetPosition(MainQuestStep step, out Vector3 position)
    {
        if (step == MainQuestStep.CollectMoon)
        {
            return TryGetNearestMoonCrystalPosition(out position);
        }

        if (step == MainQuestStep.BuildReputation)
        {
            GameObject redDao = GameObject.Find("RedDAO_Core");
            if (redDao != null)
            {
                position = redDao.transform.position;
                return true;
            }

            GameObject blueDao = GameObject.Find("BlueDAO_Core");
            if (blueDao != null)
            {
                position = blueDao.transform.position;
                return true;
            }
        }

        int index = (int)step;
        if (index < 0 || index >= Steps.Length)
        {
            position = Vector3.zero;
            return false;
        }

        string targetName = Steps[index].TargetObjectName;
        if (string.IsNullOrEmpty(targetName))
        {
            position = Vector3.zero;
            return false;
        }

        GameObject target = GameObject.Find(targetName);
        if (target == null)
        {
            position = Vector3.zero;
            return false;
        }

        position = target.transform.position;
        return true;
    }

    bool TryGetNearestMoonCrystalPosition(out Vector3 position)
    {
        MoonCrystalInteract[] crystals = FindObjectsByType<MoonCrystalInteract>(FindObjectsSortMode.None);
        if (crystals == null || crystals.Length == 0)
        {
            position = MoonCrystalFallback;
            return true;
        }

        float nearestDistance = float.MaxValue;
        position = MoonCrystalFallback;
        Vector3 playerPosition = transform.position;

        for (int i = 0; i < crystals.Length; i++)
        {
            MoonCrystalInteract crystal = crystals[i];
            if (crystal == null)
            {
                continue;
            }

            Vector3 crystalPosition = crystal.transform.position;
            float distance = Vector3.Distance(playerPosition, crystalPosition);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                position = crystalPosition;
            }
        }

        return true;
    }

    void DrawNotificationPopup()
    {
        const float width = 560f;
        const float height = 132f;
        float x = (Screen.width - width) * 0.5f;
        float y = Screen.height * 0.38f;

        GUI.Box(new Rect(x - 28f, y - 24f, width + 56f, height + 48f), GUIContent.none, _popupPanelStyle);

        if (_notificationPhase == NotificationPhase.QuestComplete)
        {
            GUI.Label(new Rect(x, y + 8f, width, 36f), "Quest Complete", _popupHeaderStyle);
            GUI.Label(new Rect(x, y + 52f, width, 44f), _completedQuestTitle, _popupTitleStyle);
            return;
        }

        GUI.Label(new Rect(x, y + 8f, width, 36f), "New Quest", _popupAccentStyle);
        GUI.Label(new Rect(x, y + 52f, width, 44f), _nextQuestTitle, _popupTitleStyle);
    }

    void EnsureStyles()
    {
        if (_stylesReady)
        {
            return;
        }

        _boxStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.UpperLeft
        };
        _boxStyle.normal.background = MakeTexture(2, 2, new Color(0.06f, 0.08f, 0.14f, 0.88f));

        _headerStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft
        };
        _headerStyle.normal.textColor = new Color(0.75f, 0.85f, 0.95f);

        _titleStyle = new GUIStyle(_headerStyle)
        {
            fontSize = 20,
            fontStyle = FontStyle.Bold
        };
        _titleStyle.normal.textColor = new Color(1f, 0.92f, 0.45f);

        _distanceStyle = new GUIStyle(_headerStyle)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold
        };
        _distanceStyle.normal.textColor = new Color(0.55f, 0.9f, 1f);

        _stepsStyle = new GUIStyle(_headerStyle)
        {
            fontSize = 13,
            fontStyle = FontStyle.Normal,
            wordWrap = true
        };
        _stepsStyle.normal.textColor = new Color(0.78f, 0.84f, 0.92f, 0.95f);

        _nextActionHeaderStyle = new GUIStyle(_headerStyle)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold
        };
        _nextActionHeaderStyle.normal.textColor = new Color(0.68f, 0.78f, 0.88f);

        _nextActionStyle = new GUIStyle(_headerStyle)
        {
            fontSize = 14,
            fontStyle = FontStyle.Bold,
            wordWrap = true
        };
        _nextActionStyle.normal.textColor = new Color(0.55f, 0.95f, 0.75f);

        _popupPanelStyle = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter
        };
        _popupPanelStyle.normal.background = MakeTexture(2, 2, new Color(0.04f, 0.07f, 0.14f, 0.94f));

        _popupHeaderStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 28,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter
        };
        _popupHeaderStyle.normal.textColor = new Color(1f, 0.88f, 0.35f);

        _popupAccentStyle = new GUIStyle(_popupHeaderStyle)
        {
            fontSize = 28
        };
        _popupAccentStyle.normal.textColor = new Color(0.55f, 0.9f, 1f);

        _popupTitleStyle = new GUIStyle(_popupHeaderStyle)
        {
            fontSize = 24,
            fontStyle = FontStyle.Normal
        };
        _popupTitleStyle.normal.textColor = new Color(0.92f, 0.95f, 1f);

        _stylesReady = true;
    }

    static Texture2D MakeTexture(int width, int height, Color color)
    {
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = color;
        }

        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
}
