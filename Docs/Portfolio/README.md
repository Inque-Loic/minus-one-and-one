# 《负一和一》系统策划作品集

更新时间：2026-06-10
工程版本：Unity 6000.3.10f1  
当前状态：v0.1.1 可发布 Demo。单机 Windows 版本已完成主流程、测试模式、中文字体替换、GitHub Release 与 Itch 上传。

## 发布入口

| 平台 | 链接/文件 | 状态 |
| --- | --- | --- |
| Itch.io | https://inque-loic.itch.io/minus-one-and-one | 已上传 Windows Demo |
| GitHub 仓库 | https://github.com/Inque-Loic/minus-one-and-one | 公开 |
| GitHub Release | https://github.com/Inque-Loic/minus-one-and-one/releases/tag/v0.1.1 | Latest |
| Windows 包 | `Builds/MinusOneAndOne-Windows.zip` | v0.1.1，52,411,840 bytes |

## 文档索引

| 文档 | 用途 |
| --- | --- |
| [PRD.md](PRD.md) | 面向产品/策划评审，定义目标用户、核心体验、玩法规则、版本范围和验收标准。 |
| [DevelopmentPlan.md](DevelopmentPlan.md) | 从现有原型推进到可发布 Demo 的里程碑、任务拆分、风险和交付标准。 |
| [CompetitiveAnalysis.md](CompetitiveAnalysis.md) | 类狼人杀/社交推理竞品拆解，提炼本项目差异化机会。 |
| [BalanceTestReport.md](BalanceTestReport.md) | v0.1.1 平衡测试记录，包含 1000 局批量测试结果。 |
| [DemoBacklog.md](DemoBacklog.md) | 当前 Demo 剩余事项和非目标范围。 |
| [KnownIssues.md](KnownIssues.md) | 已知问题、影响范围和处理建议。 |
| [ProjectHandoff.md](ProjectHandoff.md) | 当前工程状态、后续推进边界和交接提示。 |
| [ConfigTables/game_rules.csv](ConfigTables/game_rules.csv) | 全局规则参数表，承接当前 `GameManager` 内硬编码数值。 |
| [ConfigTables/role_config.csv](ConfigTables/role_config.csv) | 阵营/身份基础配置表。 |
| [ConfigTables/phase_flow.csv](ConfigTables/phase_flow.csv) | 回合状态机与阶段流转配置表。 |
| [ConfigTables/ai_tuning.csv](ConfigTables/ai_tuning.csv) | AI 行为、怀疑度、测试模式参数表。 |
| [ConfigTables/ui_text_inventory.csv](ConfigTables/ui_text_inventory.csv) | Demo 前需要统一校对的关键 UI 文案清单。 |

## 当前工程观察

项目采用轻量 2D/UGUI 原型结构，核心资产集中在 `Assets`：

| 模块 | 当前内容 | 作品集价值 |
| --- | --- | --- |
| 场景 | `Assets/Scenes/SampleScene.unity` | 已串起菜单、身份提示、游戏面板、讨论、猜分、结算等主流程。 |
| 规则 | `Assets/Scripts/GameManager.cs` | 已具备明确状态机：Selecting、Responding、Discussion、RoundEnd、ScoreGuessing、GameOver。 |
| 玩家数据 | `Assets/Scripts/Player.cs` | 已包含身份、分数、接触记录、怀疑度、回合行为标记。 |
| UI | `Assets/Scripts/UIManager.cs` | 已完成全流程界面驱动、AI 自动推进、批量测试入口。 |
| Prefab | 玩家按钮、讨论消息、目标按钮、猜分输入行 | 可继续沉淀为正式 UI 组件库。 |
| 配置 | `Assets/Scripts/PortfolioConfig.cs` + CSV | 已接入 `game_rules.csv` 与 `ai_tuning.csv`，支持 Demo 参数外置。 |
| 测试 | 批量 AI 模拟统计胜率 | 1000 局测试可稳定完成，具备系统策划调参证据。 |
| 字体 | `Assets/Fonts/OpenSource` | 使用 Noto Sans CJK SC 与 OFL 许可证，已清理旧字体引用。 |
| 构建 | `Assets/Editor/MinusOneAndOneBuild.cs` | Windows 构建前会强制应用开源字体并清理旧输出目录。 |

## 作品集叙事主线

《负一和一》不是传统“夜晚杀人、白天投票”的狼人杀复刻，而是一个以“接触结算”和“分数不完全信息”为核心的社交推理游戏：

1. 玩家每回合选择是否与他人接触，接触会改变双方分数。
2. 好人希望通过安全接触提高总收益，负一阵营希望伪装并污染分数。
3. 玩家只掌握局部信息，需要通过分数变化、发言声明和他人行为推断身份。
4. 终局前的猜分机制把“记忆、推理、谎言识别”转化为可量化收益。

Demo 阶段的设计重点是证明三件事：

1. 接触选择是否能制造足够明确的风险/收益。
2. 讨论阶段是否能补足纯数值系统的社交表达。
3. AI 批量测试是否能支持身份比例、回合数、分数规则的可解释调参。

## v0.1.1 当前结论

| 项目 | 结论 |
| --- | --- |
| 主流程 | 手动完整局已通过。 |
| 测试模式 | 1000 局批量测试不卡死，统计正常。 |
| 平衡状态 | 当前偏向一阵营：一阵营胜 58.0%，负一胜 35.5%，平局 6.5%。 |
| 中文字体 | 已迁移到 Noto Sans CJK SC；公开仓库和构建包不再使用微软雅黑资产。 |
| 发布 | Itch 与 GitHub Release 均更新到 v0.1.1。 |
| 下一步 | 不做终局复盘；优先补轻量新手引导、发布截图和作品集说明。 |
