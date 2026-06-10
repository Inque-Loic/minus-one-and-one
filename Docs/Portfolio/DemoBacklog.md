# 《负一和一》Demo Backlog

更新日期：2026-06-10
当前版本：v0.1.1 Windows Demo
原则：UI 保持简单，优先让玩家能理解和完成一局。

## P0：下一步必须处理

| 任务 | 状态 | 说明 |
| --- | --- | --- |
| 新手引导轻量页 | Todo | 用 1-2 屏解释身份、目标和三个行动按钮，不做复杂教程。 |
| 身份说明文案再压缩 | Todo | 减少长句，避免玩家开局被文本压住。 |
| 主流程再手测 1 局 | Todo | 新手引导完成后重新验证按钮、结算和返回主菜单。 |

## P1：发布页和作品集材料

| 任务 | 状态 | 说明 |
| --- | --- | --- |
| 更新 README 发布信息 | Done | 已写入 Itch、GitHub、Release、Windows 包信息。 |
| 同步 PRD 与开发计划 | Done | 已同步 v0.1.1 当前范围和下一阶段目标。 |
| 添加平衡测试报告 | Done | 已新增 `BalanceTestReport.md`。 |
| 添加 Known Issues | Done | 已新增 `KnownIssues.md`。 |
| 截图素材 | Todo | 主菜单、身份页、局内页、测试页、结算页。 |
| Itch 页面说明精修 | Todo | 补 1 段玩法说明和截图。 |
| GitHub README 展示图 | Todo | 可用截图或 GIF 增强项目可读性。 |

## P1：平衡与体验

| 任务 | 状态 | 说明 |
| --- | --- | --- |
| 观察一阵营胜率偏高 | Todo | 当前 1000 局一阵营 58.0%，负一 35.5%，先记录，不急改。 |
| 负一收益微调实验 | Todo | 可尝试调整负一得分或 AI 接触倾向。 |
| 猜分参与率观察 | Todo | 当前参与率 80.3%，平均猜对 1.51 人，可作为后续调参参考。 |

## P2：工程与配置

| 任务 | 状态 | 说明 |
| --- | --- | --- |
| CSV 字段说明 | Todo | 为 `game_rules.csv`、`ai_tuning.csv` 增加字段解释。 |
| Build 流程说明 | Todo | 简短记录 Unity Build、压缩和上传步骤。 |
| WebGL 可行性 | Backlog | 目前只发布 Windows，WebGL 暂缓。 |
| macOS/Linux 包 | Backlog | 暂无需求，先不做。 |

## 已取消

| 事项 | 原因 |
| --- | --- |
| 完整终局复盘系统 | 用户已明确不需要；当前只保留阵营胜负和猜分结果摘要。 |
| 复杂时间线回放 | 超出 Demo 目标，会增加 UI 和维护成本。 |
| 接触链/推理图谱 | 暂不需要，避免界面变复杂。 |

## 发布状态

| 渠道 | 状态 | 地址 |
| --- | --- | --- |
| Itch | Done | https://inque-loic.itch.io/minus-one-and-one |
| GitHub Repo | Done | https://github.com/Inque-Loic/minus-one-and-one |
| GitHub Release | Done | https://github.com/Inque-Loic/minus-one-and-one/releases/tag/v0.1.1 |
| Windows Zip | Done | `Builds/MinusOneAndOne-Windows.zip` |
