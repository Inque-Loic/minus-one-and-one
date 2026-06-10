# 《负一和一》项目交接记录

更新日期：2026-06-10
当前版本：v0.1.1 Windows Demo

## 1. 当前结论

项目已经完成公开 Demo 的第一阶段：

- Windows 版本已构建并上传。
- Itch 页面已可访问。
- GitHub 仓库已公开。
- GitHub Release `v0.1.1` 已发布。
- 1000 局批量测试未卡死。
- 手动完整局可玩通。
- 中文字体改为 Noto Sans CJK SC，避免 YaHei/MSYH 依赖。

当前可以把它当作一个可下载、可演示、可继续调参的桌游机制原型。

## 2. 重要链接

| 类型 | 地址 |
| --- | --- |
| Itch | https://inque-loic.itch.io/minus-one-and-one |
| GitHub Repo | https://github.com/Inque-Loic/minus-one-and-one |
| GitHub Release | https://github.com/Inque-Loic/minus-one-and-one/releases/tag/v0.1.1 |
| Windows 包 | `Builds/MinusOneAndOne-Windows.zip` |

## 3. 当前不要再做的方向

不要继续推进完整终局复盘系统。该方向已从 Demo 范围中移除。

保留即可：

- 阵营胜负。
- 猜分结果摘要。
- 身份、分数、排名。

这样能保持 UI 简单，也更符合当前作品集 Demo 的体量。

## 4. 当前最值得做的事情

1. 新手引导轻量页  
   让第一次打开游戏的人能理解身份、目标和三个行动按钮。

2. 发布页截图  
   为 Itch 和 GitHub 增加主菜单、身份页、局内页、测试页截图。

3. CSV 字段说明  
   让 Portfolio 文件夹能说明参数如何控制规则和 AI。

## 5. 关键文件

| 文件 | 用途 |
| --- | --- |
| `Docs/Portfolio/README.md` | Portfolio 总入口和发布状态。 |
| `Docs/Portfolio/PRD.md` | 当前产品范围。 |
| `Docs/Portfolio/DevelopmentPlan.md` | 下一阶段开发计划。 |
| `Docs/Portfolio/BalanceTestReport.md` | 1000 局测试基线。 |
| `Docs/Portfolio/KnownIssues.md` | 当前已知问题。 |
| `Docs/Portfolio/DemoBacklog.md` | 后续任务列表。 |
| `Docs/Portfolio/ConfigTables/game_rules.csv` | 规则参数。 |
| `Docs/Portfolio/ConfigTables/ai_tuning.csv` | AI 参数。 |

## 6. 当前测试基线

测试模式 1000 局：

- 一阵营胜：580（58.0%）
- 负一胜：355（35.5%）
- 平局：65（6.5%）
- 平均分：一阵营 3.20 / 负一 2.73
- 接受率：72.1%
- 猜分参与率：80.3%
- 平均猜对：1.51 人

结论：偏向一阵营，但可作为 v0.1.1 Demo 发布基线。

## 7. 发布前检查清单

下次版本发布前建议执行：

- 手动完整玩 1 局。
- 批量测试 1000 局。
- 检查主菜单按钮文字是否可见。
- 检查中文字体是否正常。
- 检查行动按钮和子按钮是否可点击。
- 更新 `BalanceTestReport.md`。
- 更新 `KnownIssues.md`。
- 重新打 Windows Build。
- 更新 Itch 包。
- 更新 GitHub Release。

## 8. 工程注意事项

- 不要重新引入系统字体 YaHei/MSYH。
- 不要把复杂复盘系统作为当前 P0。
- 不要为了视觉效果牺牲按钮点击区域和层级。
- UI 简单优先，动作可点击优先，文案清楚优先。
