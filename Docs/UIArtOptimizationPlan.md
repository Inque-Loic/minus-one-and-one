# 《负一和一》界面美术优化方案

更新时间：2026-06-10  
依据文件：

- `Docs/ArtAssets.md`
- `Docs/ArtResourceShortlist.md`
- `Docs/Portfolio/ProjectHandoff.md`

## 1. 设计目标

当前 Demo 已经可玩、可发布，界面优化的目标不是重做 UI，而是在不破坏可读性和点击流程的前提下，让界面从“纯色功能原型”升级为“深色桌游推理 Demo”。

核心方向：

> 深色桌面 + 卡牌身份 + 棋子玩家 + 线索图标 + 克制的阵营色

需要保留：

- 信息清楚。
- 按钮大、可点击。
- 层级简单。
- 不新增复杂终局解释。
- 不做大规模 UI 重构。

## 2. 可用资源范围

正式资源只使用：

`Assets/Art/Kenney`

优先资源：

| 类型 | 路径 | 用途 |
| --- | --- | --- |
| UI Pack | `Assets/Art/Kenney/UI/PNG` | 面板底图、按钮底图、确认/取消图形。 |
| Game Icons | `Assets/Art/Kenney/Icons/PNG/White/2x` | 信息、警告、确认、返回、奖杯、排行榜、多人等图标。 |
| BoardGame Cards | `Assets/Art/Kenney/BoardGame/PNG/Cards` | 身份卡、主菜单装饰、阶段牌。 |
| BoardGame Chips | `Assets/Art/Kenney/BoardGame/PNG/Chips` | 分数、猜分、奖励/惩罚视觉。 |
| BoardGame Pieces | `Assets/Art/Kenney/BoardGame/PNG/Pieces (...)` | 玩家头像、玩家编号、当前行动者标识。 |

不要引用：

- `Temp/ArtDownloads`
- 原始下载包里的未导入文件
- 系统字体 YaHei/MSYH

## 3. 视觉规范

### 3.1 色彩

| 用途 | 建议颜色 | 说明 |
| --- | --- | --- |
| 背景 | 深蓝黑 / 暗桌面色 | 保持当前深色基调，避免大面积纯黑。 |
| 主面板 | 深灰蓝 + 低透明描边 | 用 Kenney Grey UI 资源做底图，再用 Image 颜色叠加。 |
| 一阵营 | 暖金 / 绿青 | 表示合作、可信、正收益。 |
| 负一阵营 | 赤红 / 暗紫 | 表示污染、风险、伪装。 |
| 普通按钮 | 深蓝灰 | 低干扰。 |
| 确认/接受 | 绿色 | 和正向行动绑定。 |
| 拒绝/危险 | 红色 | 和风险/否定绑定。 |
| 猜分/奖励 | 金色 / 筹码色 | 强调结算和收益。 |

### 3.2 图形语言

| 元素 | 视觉隐喻 |
| --- | --- |
| 玩家 | 棋子 |
| 身份 | 卡牌 |
| 分数 | 筹码 |
| 讨论 | 信息/气泡/目标图标 |
| 指控 | target / exclamation / cross |
| 辩护 | checkmark / shield-like icon if available |
| 猜分 | chip / question-like information icon |
| 胜利/排名 | trophy / medal / leaderboard |

### 3.3 字体与文字

- 继续使用 Noto Sans CJK SC。
- 标题使用 34-44 号，正文 20-26 号，按钮 22-28 号。
- 不为了装饰缩小核心操作文字。
- 图标只能辅助，不替代关键中文按钮文本。

## 4. 分界面优化方案

### 4.1 主菜单

目标：第一眼从“普通 Demo 菜单”变成“桌游推理入口”。

优化：

- 背景保持深色，可加入低透明卡牌背面或棋子作为角落装饰。
- 标题“负一和一”保持大字，不放进卡片里。
- “开始游戏”“游戏测试”按钮使用 Kenney UI Pack 的矩形按钮底图。
- 按钮左侧可加小图标：
  - 开始游戏：`checkmark` 或 `play` 类图标。
  - 游戏测试：`gear` / `information` / `leaderboardsSimple`。

建议资源：

- `Assets/Art/Kenney/UI/PNG/Green/Default/button_rectangle_depth_flat.png`
- `Assets/Art/Kenney/UI/PNG/Blue/Default/button_rectangle_depth_flat.png`
- `Assets/Art/Kenney/BoardGame/PNG/Cards/cardBack_blue*.png`

### 4.2 身份页

目标：把身份页做成“身份卡”，强化阵营感和新手理解。

优化：

- 页面中心使用卡牌式面板。
- 一阵营身份卡使用绿色/金色边框或卡背装饰。
- 负一身份卡使用红色/紫色边框或红色卡背装饰。
- 增加一个身份符号：
  - 一阵营：绿色棋子或 checkmark。
  - 负一阵营：红色棋子或 cross/exclamation。
- 文案分成三段：
  - 你的身份。
  - 你的目标。
  - 接触规则提示。

建议资源：

- `Assets/Art/Kenney/BoardGame/PNG/Cards/cardBack_green*.png`
- `Assets/Art/Kenney/BoardGame/PNG/Cards/cardBack_red*.png`
- `Assets/Art/Kenney/BoardGame/PNG/Pieces (Green)`
- `Assets/Art/Kenney/BoardGame/PNG/Pieces (Red)`

注意：

- 新手引导可优先在身份页完成，不需要新增复杂教程系统。

### 4.3 局内主界面

目标：提升玩家列表和当前行动状态的可读性。

优化：

- 顶部回合信息做成窄条状态栏。
- 玩家列表按钮加棋子头像，颜色按玩家编号循环使用。
- 当前行动者按钮增加亮边或小箭头。
- 已接触/不可选目标降低透明度，保留文字可读。
- 玩家分数前加小筹码图标。
- 消息区使用信息图标作为开头，减少纯文本疲劳。

建议资源：

- `BoardGame/PNG/Pieces (...)`
- `BoardGame/PNG/Chips`
- `Icons/PNG/White/2x/information.png`
- `UI/PNG/Grey/...button_rectangle...`

注意：

- 玩家按钮仍然要保持现有点击区域，不要为了头像压缩文本。

### 4.4 响应弹窗

目标：接受/拒绝选择更像一次有风险的桌面决策。

优化：

- 弹窗使用 Grey 面板底图。
- 接受按钮使用绿色按钮 + checkmark。
- 拒绝按钮使用红色按钮 + cross。
- 如果因连续不行动不能拒绝，拒绝按钮置灰，并用 warning/information 图标提示原因。

建议资源：

- `UI/PNG/Green/...button_rectangle...`
- `UI/PNG/Red/...button_rectangle...`
- `Icons/PNG/White/2x/checkmark.png`
- `Icons/PNG/White/2x/cross.png`
- `Icons/PNG/White/2x/exclamation.png`

### 4.5 讨论界面

目标：让“声明 / 指控 / 辩护”三类发言更好区分。

优化：

- 讨论消息条目改成轻卡片行。
- 每条消息左侧放类型图标：
  - 声明分数：information / chip。
  - 指控：target / exclamation / cross。
  - 辩护：checkmark。
- 人类发言按钮使用三色区分：
  - 声明：蓝灰。
  - 指控：红。
  - 辩护：绿。
- 目标按钮可用小棋子头像增强“点人”的感觉。

建议资源：

- `DiscussionMessageItem.prefab`
- `DiscussionTargetButton.prefab`
- `Icons/PNG/White/2x/information.png`
- `Icons/PNG/White/2x/exclamation.png`
- `Icons/PNG/White/2x/checkmark.png`

注意：

- 不要把讨论页改成复杂聊天软件界面。
- 仍保持一屏能看见发言记录和当前操作。

### 4.6 猜分界面

目标：把猜分从表单输入变成“下注/筹码”氛围，但不改变输入逻辑。

优化：

- 标题旁加入筹码图标。
- 每个猜分输入行加玩家棋子。
- 提交按钮使用金色/绿色，跳过按钮使用灰色。
- 猜分结果摘要中，奖励和惩罚用 + / - 颜色区分。

建议资源：

- `Assets/Prefabs/GuessingInputRow.prefab`
- `BoardGame/PNG/Chips`
- `UI/PNG/Yellow/...button_rectangle...`

### 4.7 结算界面

目标：保持简单结算，但更有完成感。

优化：

- 阵营结果前加 trophy 或 medal。
- 排名前三名可加金/银/铜感图标或筹码装饰。
- 猜分结果摘要用单独小面板承载。
- 不新增完整终局解释，不做接触链图谱。

建议资源：

- `Icons/PNG/White/2x/trophy.png`
- `Icons/PNG/White/2x/medal*.png`
- `Icons/PNG/White/2x/leaderboardsSimple.png`
- `BoardGame/PNG/Chips`

## 5. 推荐实施顺序

### Phase 1：低风险统一外观

目标：不动流程，只替换视觉底图和图标。

1. 给通用按钮套 Kenney UI Pack 底图。
2. 给主要面板套 Grey 面板/按钮风格。
3. 给主菜单、身份页、结算页加入少量卡牌/棋子/筹码装饰。
4. 确认中文仍可读、按钮仍可点。

验收：

- 主菜单、身份页、局内页、讨论页、猜分页、结算页视觉统一。
- 所有按钮点击区域不缩小。

### Phase 2：强化玩法信息符号

目标：让玩家更快理解动作类型和阶段。

1. 玩家按钮加棋子头像。
2. 分数或猜分相关位置加筹码图标。
3. 讨论消息按类型加图标。
4. 接受/拒绝/指控/辩护按钮加入图标。

验收：

- 不看长文案也能大致区分“接触、讨论、猜分、结算”。
- 图标不会遮挡文字。

### Phase 3：发布截图优化

目标：让 Itch/GitHub 截图更有吸引力。

1. 调整主菜单构图，保留一眼可读的标题和两个入口。
2. 身份页做成最能代表游戏机制的截图。
3. 局内页展示玩家棋子、回合信息、行动提示。
4. 讨论页展示类型化发言。
5. 结算页展示阵营胜负和排名。

验收：

- 至少产出 5 张截图：主菜单、身份页、局内、讨论、结算。
- 截图中文字不乱码、不重叠。

## 6. 开发注意事项

1. 优先修改 prefab 和现有 `UIManager` 配置方法，不要重写 UI 流程。
2. 如果使用 9-slice，需要检查 Sprite Import Settings 的 Border。
3. 所有新增 Image 都要确认 Raycast Target 是否必要，装饰图应关闭 Raycast Target，避免挡按钮。
4. 深色背景上使用 White/2x 图标，浅色按钮上使用 Black/1x 或按钮自带图形。
5. 不要在按钮里塞过多图标和文字；按钮最多“一个图标 + 一个短文本”。
6. 棋子和筹码只作为辅助符号，不要替代玩家名和分数数字。
7. 每次界面调整后至少检查 1280x720 和 1920x1080。

## 7. 给执行会话的任务描述

可以直接把下面这段交给负责实现的 Codex 会话：

```text
请阅读 Docs/UIArtOptimizationPlan.md、Docs/ArtAssets.md、Docs/ArtResourceShortlist.md。
目标是在不重构游戏流程的前提下，使用 Assets/Art/Kenney 下的 Kenney UI、Icons、BoardGame 资源优化当前 Unity UI。
优先做 Phase 1 和 Phase 2：按钮/面板统一、身份页卡牌化、玩家棋子头像、讨论/猜分/结算图标化。
不要做终局解释系统，不要做复杂接触链图谱，不要牺牲按钮点击区域和中文可读性。
完成后请在 1280x720 和 1920x1080 下检查主菜单、身份页、局内页、讨论页、猜分页、结算页。
```

