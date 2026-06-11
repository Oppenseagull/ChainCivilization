# ChainCivilizationV1 — 架构评估报告

> 黑客松安全重构 | 仅 reorganize + 分析，未改动玩法逻辑

---

## 当前架构评分：**6.5 / 10**

| 维度 | 分数 | 说明 |
|------|------|------|
| 功能完整性 | 9/10 | 出生→DAO→水晶→REP→Green Pass→边界→文明→Quest→Journal 链路完整 |
| 目录组织 | 7/10 | 已完成标准分层（本次重构） |
| 代码复用 | 4/10 | OnGUI / 石碑交互 / Green 门槛大量重复 |
| 可测试性 | 3/10 | 紧耦合单例 + `GameObject.Find` + OnGUI |
| 状态一致性 | 6/10 | PlayerPrefs 与 Session 混用，Quest 靠轮询对齐 |
| 可扩展性 | 6/10 | 线性 Quest 清晰，但每加一个交互要复制一整段 OnGUI |
| 稳定性风险 | 7/10 | 曾因 `.meta` GUID 损坏导致编译级联失败（已修复） |

---

## 优点

1. **玩法链路清晰**：7 步主线与各系统一一对应，黑客松演示路径明确。
2. **Manager 中枢明确**：`TokenManager` / `ReputationManager` / `DAOPassManager` / `CivilizationManager` 分工合理。
3. **零 Canvas 依赖的交互**：DAO / 边界 / 水晶用 OnGUI，场景搭建快、不依赖预制体。
4. **地面对齐已工具化**：`GroundSnapUtility` 统一了大部分世界物体的 Y 轴修正。
5. **Quest 可自愈**：`MainQuestManager` 启动时根据持久化状态 reconcile，避免回档后任务卡死。
6. **混合 UI 策略务实**：复杂输入用 UGUI（Journal、规则面板），轻量 HUD 用 OnGUI。

---

## 风险（按严重程度）

### 高风险

| 风险 | 影响 | 现状 |
|------|------|------|
| OnGUI 分散在 12 个脚本 | 改字体/布局需改多处；绘制顺序不可控；难以本地化 | 未改，仅统计（见 Phase 3） |
| PlayerPrefs + Session 双轨状态 | REP 总值会话、领奖标记持久化，理解成本高 | 行为正确，文档已标注 |
| 手动/非标准 `.meta` GUID | 曾导致 `ReputationManager` 找不到、场景 GUID 解析失败 | `ReputationManager` 已修复，需避免手改 GUID |
| `GameObject.Find` 硬编码名称 | 重命名场景对象即静默失效 | Blue/Red/Green DAO、Crystal、Quest 目标均依赖 |

### 中风险

| 风险 | 影响 | 现状 |
|------|------|------|
| Green DAO 门槛重复实现 | `GreenDAOSteleInteract` 与 `DAOPassEntryZone` 各有一套 `requiredMoon/Reputation` | 数值一致（200/20），建议抽 `DAORequirements` |
| DAO 石碑交互模板重复 | Blue/Red/Green 距离检测、E 键、GUIStyle 初始化高度相似 | 已提出 `SteleInteractBase` 方案，未实施 |
| `CivilizationFlagSpawner` / `MarkerSpawner` 重复 | URP Lit 材质、Collider 处理、订阅 `CivilizationManager` 模式相同 | 可抽 `CivilizationVisualSpawner` 基类，非紧急 |
| `InteractGroundDebugChecker` 自动创建 | 每次 Play 注入隐藏对象 | 开发有用，发布前应可开关 |
| `MainQuestManager` 轮询 + 多数据源 | 0.5s poll，与事件驱动混用 | 当前稳定，扩展步骤时易漏条件 |

### 低风险

| 风险 | 影响 | 现状 |
|------|------|------|
| `InteractGroundSnap` 死代码 | 编译包含但零引用 | 已列入待确认删除 |
| Tutorial / Editor 残留 | 增加编译体积、首次打开 Editor 弹 Readme | 已列入待确认删除 |
| `BillboardLabel` 嵌套在 `CivilizationMarkerSpawner.cs` | 单文件多类型 | 可拆文件，不影响运行 |
| `Core/` 目录空置 | 新同事不知放哪 | `ProjectStructure.md` 已约定 |

---

## Phase 3 — 重复逻辑分析

### 1. Green DAO 门槛（MOON ≥ 200, REP ≥ 20）

**确认重复：** 是

| 位置 | 实现方式 |
|------|----------|
| `DAO/GreenDAOSteleInteract.cs` | `[SerializeField] requiredMoon=200`, `requiredReputation=20` → `HasAccess()` |
| `DAO/DAOPassEntryZone.cs` | 同字段 → `OnTriggerEnter` 内分别判断 |

**建议抽离（未实施）：** 新增 `Core/DAORequirements.cs`

```csharp
public static class DAORequirements
{
    public const int GreenDaoRequiredMoon = 200;
    public const int GreenDaoRequiredReputation = 20;

    public static bool MeetsGreenDaoAccess(TokenManager tokens, ReputationManager reputation)
    {
        return tokens != null && tokens.MoonBalance >= GreenDaoRequiredMoon
            && reputation != null && reputation.GetReputation() >= GreenDaoRequiredReputation;
    }
}
```

两处改为调用 `DAORequirements`，Inspector 序列化字段可保留为只读镜像或移除（移除需验证场景序列化值不变）。**行为零变化。**

---

### 2. DAO 石碑交互重复（Blue / Red / Green）

**确认重复：** 是（结构级）

| 重复块 | Blue | Red | Green |
|--------|------|-----|-------|
| `interactRadius` + `Update` 距离检测 | ✓ | ✓ | ✓ |
| `GameObject.Find("Player")` | ✓ | ✓ | ✓ |
| E 键（Input System + Legacy 双分支） | ✓ | ✓ | ✗（Green 仅展示） |
| `GroundSnapUtility.SnapTransform` | ✓ | ✓ | ✓ |
| `OnGUI` 面板 + `EnsureStyles()` | ✓ | ✓ | ✓ |
| `GUIStyle _panelStyle/_titleStyle/...` | ✓ | ✓ | ✓ |

**建议方案（未实施）：**

```
Scripts/Core/ 或 Scripts/DAO/
├── SteleProximitySensor.cs      // 半径检测 + Player 引用
├── SteleGuiStyles.cs              // 共享 GUIStyle 工厂
└── SteleInteractBase.cs           // abstract: Near + OnGUI 框架
    ├── BlueDAOSteleInteract
    ├── RedDAOSteleInteract
    └── GreenDAOSteleInteract      // 覆盖：无 E 键，门槛 UI
```

**注意：** Green 无 E 键、Red 有捐赠逻辑、Blue 有 `_revealed` 状态——基类应只抽 **距离 + 样式 + 布局辅助**，业务 `HandleInteract` 保持子类实现。黑客松期间可 defer。

---

### 3. OnGUI 使用清单（12 个脚本）

| # | 脚本 | 用途 |
|---|------|------|
| 1 | `Quest/MainQuestManager.cs` | 左上任务 HUD + 完成/新任务弹窗 |
| 2 | `Quest/QuestLogUI.cs` | F 键任务日志全屏面板 |
| 3 | `UI/MoonBalanceUI.cs` | MOON/REP HUD + 凭证/奖励/声望弹窗 |
| 4 | `UI/AddressPanelUI.cs` | Tab 信息面板 |
| 5 | `DAO/BlueDAOSteleInteract.cs` | 欢迎/揭示面板 |
| 6 | `DAO/RedDAOSteleInteract.cs` | 欢迎/捐赠提示/奖励弹窗 |
| 7 | `DAO/GreenDAOSteleInteract.cs` | 准入/拒绝面板 + 数值展示 |
| 8 | `World/MoonCrystalInteract.cs` | 采集提示面板 |
| 9 | `World/BoundaryTrigger.cs` | 多阶段 lore / 拒绝面板 |
| 10 | `Civilization/CivilizationSeedInteract.cs` | 种子交互提示 |
| 11 | `Civilization/CivilizationRuleNoticeUI.cs` | 规则创建通知 |
| 12 | `Civilization/CivilizationCompleteNoticeUI.cs` | 文明完成通知 |

**未使用 OnGUI 的 UI：** `CivilizationJournalUI`, `CivilizationSeedRulePanel`（UGUI）

**后续替换建议（非本次）：** 统一 `HudOverlayService` 或单一 `OnGuiHost` 组件集中绘制，降低 `GUI.depth` 冲突风险。黑客松结束前 **不建议** 全量替换。

---

## 推荐优化顺序

| 优先级 | 任务 | 风险 | 预估工时 |
|--------|------|------|----------|
| P0 | ✅ 目录分层 + `ProjectStructure.md` | 低 | 已完成 |
| P1 | 抽离 `DAORequirements.cs` | 低 | 30 min |
| P2 | 确认并删除死代码（InteractGroundSnap、Tutorial、Editor 残留） | 低 | 15 min |
| P3 | `SteleInteractBase` 抽离距离+样式 | 中 | 2–3 h |
| P4 | Flag/Marker Spawner 合并材质逻辑 | 低 | 1 h |
| P5 | 单一 OnGUI Host 或逐步迁 UGUI | 高 | 1–2 天 |
| P6 | `GameObject.Find` → 序列化引用或 Player tag 缓存 | 中 | 2 h |
| P7 | Quest 事件化（替代部分 poll） | 中 | 3–4 h |

---

## 黑客松结束前建议 **不处理** 的问题

1. **全量 OnGUI → UGUI 迁移** — 工作量大，易引入布局/输入回归。
2. **Quest 系统重写** — 当前线性 7 步已满足演示，轮询虽丑但稳定。
3. **DAO 玩法规则调整** — 门槛数值、奖励逻辑均已验证。
4. **文明创建流程改动** — Seed → Panel → Manager → Spawner 链路完整。
5. **持久化架构统一** — PlayerPrefs 够用，改 ScriptableObject/存档系统超出范围。
6. **单元测试基础设施** — 时间不允许，手动 Play Mode 回归即可。
7. **SteleInteractBase 大规模继承重构** — 除非 P1 之后仍有大量 DAO 变体需求。

---

## 回归测试清单（重构后必做）

- [ ] Play Mode 无编译错误
- [ ] 玩家出生 `(0, 1, 0)` 正常
- [ ] Blue DAO：靠近 → E → REP +10 → Quest 推进
- [ ] Red DAO：首次 E → +100 MOON；二次捐赠 → +20 REP
- [ ] Moon Crystal：采集增加 MOON
- [ ] Green DAO：不足门槛显示拒绝文案；满足后 Pass 发放
- [ ] Boundary Stone：lore 流程 + QuestSignals
- [ ] Civilization Seed：E → 规则面板 → 旗帜/标记生成 → 通知 UI
- [ ] F 任务日志 / J 文明日志 / Tab 地址面板
- [ ] MOON/REP HUD 与弹窗正常

---

*报告生成：黑客松安全重构 Phase 4 | 2026-06-03*
