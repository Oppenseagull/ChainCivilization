# ChainCivilizationV1 — 项目脚本结构规范

> 黑客松安全重构后生效。新增脚本必须放入对应目录，禁止在 `Assets/Scripts/` 根目录直接堆放 `.cs` 文件。

## 目录约定

| 目录 | 用途 |
|------|------|
| `Scripts/Core/` | 跨系统共享类型、常量、静态工具 |
| `Scripts/Managers/` | 全局单例 / 会话状态管理器 |
| `Scripts/DAO/` | Blue / Red / Green DAO 交互与 Pass 发放 |
| `Scripts/Civilization/` | 文明种子、规则、旗帜、日志、通知 UI |
| `Scripts/Quest/` | 主线任务、任务日志、任务信号 |
| `Scripts/UI/` | 玩家 HUD 与信息面板 |
| `Scripts/World/` | 世界物体交互（水晶、边界石） |
| `Scripts/Tools/` | 开发辅助、地面对齐、放置规则 |

**不纳入本结构（第三方 / 模板）：**

- `Assets/StarterAssets/` — 第三人称控制器
- `Assets/TutorialInfo/` — Unity 模板 Readme（待确认删除）
- `Assets/Editor/` — Editor 工具脚本（待确认删除）

---

## 脚本清单

### Core/

| 脚本 | 职责 | 依赖 |
|------|------|------|
| `DAORequirements.cs` | Green DAO 准入门槛唯一来源（MOON≥200, REP≥20） | `TokenManager`, `ReputationManager` |

### Managers/

| 脚本 | 职责 | 依赖 |
|------|------|------|
| `TokenManager.cs` | MOON 余额（PlayerPrefs）、Red DAO 首次领奖状态、`TrySpendMoon` | 无（被多处读取） |
| `ReputationManager.cs` | REP 会话值、Blue/Red 一次性奖励、弹窗/浮动字状态 | 无（`MoonBalanceUI` 负责绘制弹窗） |
| `DAOPassManager.cs` | Green Pass 等凭证（PlayerPrefs）、凭证弹窗状态 | 无（`MoonBalanceUI` 负责绘制） |
| `CivilizationManager.cs` | 文明类型选择（会话静态类）、`CivilizationType` 枚举 | 被 `CivilizationSeedRulePanel`、`FlagSpawner`、`MarkerSpawner`、`JournalUI` 订阅 |

### DAO/

| 脚本 | 职责 | 依赖 |
|------|------|------|
| `BlueDAOSteleInteract.cs` | Blue 石碑欢迎/按 E 揭示、+10 REP 一次性 | `GroundSnapUtility`, `QuestSignals`, `ReputationManager` |
| `RedDAOSteleInteract.cs` | Red 石碑首次 +100 MOON、捐赠 50 MOON → +20 REP | `GroundSnapUtility`, `TokenManager`, `ReputationManager` |
| `GreenDAOSteleInteract.cs` | Green 石碑门槛 UI | `DAORequirements`, `GroundSnapUtility`, `TokenManager`, `ReputationManager` |
| `DAOPassEntryZone.cs` | 触发器内发放 Green Pass | `DAORequirements`, `GroundSnapUtility`, `TokenManager`, `ReputationManager`, `DAOPassManager` |

### Civilization/

| 脚本 | 职责 | 依赖 |
|------|------|------|
| `CivilizationSeedInteract.cs` | 种子近距离交互、打开规则面板 | `GroundSnapUtility`, `CivilizationSeedRulePanel`, `CivilizationManager` |
| `CivilizationSeedRulePanel.cs` | Canvas 规则三选一面板 | `CivilizationManager` |
| `CivilizationFlagSpawner.cs` | 选文明后生成旗帜 | `CivilizationManager` |
| `CivilizationMarkerSpawner.cs` | 选文明后生成 "My Civilization" 标记 + `BillboardLabel` | `CivilizationManager` |
| `CivilizationRuleNoticeUI.cs` | 规则创建后 5s 通知 | `CivilizationManager` |
| `CivilizationCompleteNoticeUI.cs` | 规则通知结束后 6s 完成通知 | `CivilizationRuleNoticeUI`（时序链） |
| `CivilizationJournalUI.cs` | J 键文明日志（UGUI Canvas） | `CivilizationManager` |

### Quest/

| 脚本 | 职责 | 依赖 |
|------|------|------|
| `MainQuestManager.cs` | 7 步主线、距离 HUD、完成/新任务弹窗 | `TokenManager`, `ReputationManager`, `DAOPassManager`, `QuestSignals`, `CivilizationManager` |
| `QuestLogUI.cs` | F 键任务日志 | `MainQuestManager`（同 Player 组件） |
| `QuestSignals.cs` | 会话任务标志（BlueDaoVisited, BoundaryLoreComplete） | 被 `BlueDAOSteleInteract`, `BoundaryTrigger`, `MainQuestManager` 使用 |

### UI/

| 脚本 | 职责 | 依赖 |
|------|------|------|
| `MoonBalanceUI.cs` | 左上角 MOON/REP HUD、凭证/声望/MOON 弹窗绘制 | `TokenManager`, `ReputationManager`, `DAOPassManager` |
| `AddressPanelUI.cs` | Tab 地址/余额/REP/当前 DAO 面板 | `TokenManager`, `ReputationManager` |

### World/

| 脚本 | 职责 | 依赖 |
|------|------|------|
| `MoonCrystalInteract.cs` | 月亮水晶采集 +MOON | `GroundSnapUtility`, `TokenManager` |
| `BoundaryTrigger.cs` | 边界石多阶段 lore、Green Pass 校验 | `GroundSnapUtility`, `DAOPassManager`, `QuestSignals` |

### Tools/

| 脚本 | 职责 | 依赖 |
|------|------|------|
| `GroundSnapUtility.cs` | 静态地面对齐射线工具 | Physics（被多数 World/DAO 脚本调用） |
| `InteractPlacementRules.cs` | 交互物放置规则常量/校验 | 被 `InteractGroundDebugChecker` 使用 |
| `InteractGroundDebugChecker.cs` | 运行时自动创建，校验场景交互物 Y 坐标 | `InteractPlacementRules`, `GroundSnapUtility` |
| `InteractGroundSnap.cs` | **未挂载** MonoBehaviour 地面对齐（已被 `GroundSnapUtility` 替代） | 无引用 |

---

## 场景挂载关系（SampleScene）

**Player 对象：**

- `TokenManager`, `ReputationManager`, `DAOPassManager`
- `MoonBalanceUI`, `AddressPanelUI`
- `MainQuestManager`, `QuestLogUI`
- `CivilizationJournalUI`
- StarterAssets 控制器与输入

**世界对象（节选）：**

- `BlueDAO_Core` → `BlueDAOSteleInteract`
- `RedDAO_Core` → `RedDAOSteleInteract`
- `GreenDAO_Core` → `GreenDAOSteleInteract` + `DAOPassEntryZone`
- `BoundaryStone` → `BoundaryTrigger`
- `CivilizationSeed` → `CivilizationSeedInteract`, `CivilizationSeedRulePanel`
- `Boundary_Zone` 子物体 → Flag/Marker Spawner、Rule/Complete Notice UI

---

## 依赖关系简图

```
                    ┌─────────────────┐
                    │  QuestSignals   │ (session flags)
                    └────────┬────────┘
                             │
    ┌────────────────────────┼────────────────────────┐
    ▼                        ▼                        ▼
BlueDAOStele          BoundaryTrigger          MainQuestManager
                             │
                    ┌────────┴────────┐
                    ▼                 ▼
            TokenManager      ReputationManager
                    │                 │
                    └────────┬────────┘
                             ▼
                      DAOPassManager
                             │
              ┌──────────────┼──────────────┐
              ▼              ▼              ▼
      DAOPassEntryZone   MoonBalanceUI   AddressPanelUI
      GreenDAOStele      QuestLogUI

CivilizationManager (static)
        │
        ├── CivilizationSeedRulePanel
        ├── CivilizationFlagSpawner
        ├── CivilizationMarkerSpawner
        ├── CivilizationJournalUI
        └── MainQuestManager (CreateCivilization step)

GroundSnapUtility ← DAO / World / Tools 脚本
```

---

## 新增脚本规则

1. **先选目录**：按上表职责放入对应子文件夹；跨系统常量放 `Core/`。
2. **禁止根目录**：不得在 `Assets/Scripts/` 根级新建 `.cs`。
3. **命名**：`PascalCase`，交互类以 `Interact` 或区域名结尾；Manager 以 `Manager` 结尾。
4. **单例**：玩家身上的 Manager 使用 `Instance` + `Awake` 去重；静态会话状态用 `static class`（参考 `QuestSignals`）。
5. **UI 技术选型**：
   - 黑客松快速 HUD → 可继续 OnGUI（与现有脚本一致）
   - 需输入/复杂布局 → UGUI Canvas（参考 `CivilizationJournalUI`）
6. **地面对齐**：世界交互物 `Start()` 中调用 `GroundSnapUtility.SnapTransform`。
7. **玩家查找**：优先 Inspector 序列化引用；临时方案可用 `GameObject.Find("Player")`（与现有 DAO 脚本一致）。
8. **持久化**：MOON / Pass / 部分 REP 领奖用 PlayerPrefs；REP 总值与会话 Quest 标志不持久化。
9. **修改门槛常量**：Green DAO 的 MOON/REP 门槛仅允许修改 `Core/DAORequirements.cs`，禁止在其他脚本硬编码。

---

## 待确认删除（尚未执行）

| 路径 | 原因 |
|------|------|
| `Assets/Scripts/Tools/InteractGroundSnap.cs` | 场景/预制体零引用 |
| `Assets/TutorialInfo/` 整个目录 | Unity 模板 Readme，与玩法无关 |
| `Assets/Readme.asset` | 关联 Tutorial Readme |
| `Assets/Editor/StarterAssetsPackageImporter.cs` | Starter Assets 包导入残留 |
| `Assets/Editor/McpInstallHelper.cs` | MCP 安装辅助（开发用，非运行时） |

**请确认后再删除。**

---

*文档生成：黑客松安全重构 Phase 2D | Unity 6000.4.9f1*
