# 负一和一

《负一和一》是一款 8 人轻量社交推理游戏原型。玩家被分为“一阵营”和“负一阵营”，通过每回合接触、讨论发言和终局猜分来争取阵营胜利。

## Demo

- Itch 页面：https://inque-loic.itch.io/minus-one-and-one
- Windows 构建包发布在 GitHub Releases 中。

## 当前状态

- Unity 版本：6000.3.10f1
- 单机 AI 局已可跑通
- 支持身份分配、接触结算、讨论阶段、终局猜分
- 支持批量 AI 测试统计
- 规则和 AI 参数接入 `Docs/Portfolio/ConfigTables`

## 文档

作品集文档位于 [Docs/Portfolio](Docs/Portfolio)：

- [PRD](Docs/Portfolio/PRD.md)
- [DevelopmentPlan](Docs/Portfolio/DevelopmentPlan.md)
- [CompetitiveAnalysis](Docs/Portfolio/CompetitiveAnalysis.md)
- [ConfigTables](Docs/Portfolio/ConfigTables)

## 本地运行

1. 使用 Unity 6000.3.10f1 打开项目。
2. 打开 `Assets/Scenes/SampleScene.unity`。
3. 点击 Play。

## 构建

Unity 菜单中提供：

- `Tools/负一和一/Configure Windows Player`
- `Tools/负一和一/Build Windows`

Windows 构建默认输出到 `Builds/Windows`。
