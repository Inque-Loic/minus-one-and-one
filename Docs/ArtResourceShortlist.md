# 《负一和一》美术资源候选清单

目标：让当前偏纯色 UI 的界面更有“桌游 / 阵营推理 / 线索记录”的视觉层次，同时保持 Unity 2D/URP 项目导入成本低、授权清晰。

## 首选资源

1. Kenney UI Pack
   - 链接：https://kenney.nl/assets/ui-pack
   - 授权：Creative Commons CC0
   - 内容：按钮、面板、滑条等 430 个 UI 资源。
   - 用法建议：替换主菜单、身份卡、讨论面板、结算面板的纯色 Image；做 9-slice 面板和按钮底图。

2. Kenney Board Game Pack
   - 链接：https://kenney.nl/assets/boardgame-pack
   - 授权：Creative Commons CC0
   - 内容：棋子、骰子、卡牌、筹码等 490 个 2D 桌游资源。
   - 用法建议：玩家头像/身份牌可用棋子或卡牌视觉；回合结束和猜分阶段可用筹码、卡牌、骰子做状态符号。

3. Kenney Board Game Icons
   - 链接：https://kenney-assets.itch.io/board-game-icons
   - 授权：CC0 1.0 Universal
   - 内容：250+ 桌游图标，包含独立图标、图集和矢量文件。
   - 用法建议：用于“接触、拒绝、讨论、指控、信任、猜分、阵营”等按钮和提示图标。

4. Kenney Game Icons
   - 链接：https://kenney.nl/assets/game-icons
   - 授权：Creative Commons CC0
   - 内容：105 个游戏通用图标。
   - 用法建议：主菜单、返回、设置、开始、继续、警告、信息、确认等通用图标。

5. Kenney Input Prompts
   - 链接：https://kenney-assets.itch.io/input-prompts
   - 授权：CC0 1.0 Universal
   - 内容：1280+ 键鼠、手柄、触控提示图标。
   - 用法建议：如果后续加快捷键、教程提示或手柄支持，可以统一做操作提示。

## 备选资源池

1. itch.io CC0 UI assets
   - 链接：https://itch.io/game-assets/assets-cc0/free/tag-user-interface
   - 用法建议：查找更偏科幻、悬疑、像素或手绘风格的 UI 套件。下载前逐个核对页面授权。

2. OpenGameArt UI collection
   - 链接：https://opengameart.org/content/user-interface-ui-art-collection
   - 用法建议：作为补充资源池，适合找独立按钮、对话框、图标包。下载前逐个核对授权。

3. OpenGameArt Boardgame Pack
   - 链接：https://opengameart.org/content/boardgame-pack
   - 授权备注：页面说明署名 Kenney 可选，不强制。
   - 用法建议：如果 Kenney 官网下载不方便，可用这个页面拿桌游包。

## 推荐落地顺序

1. 先导入 Kenney UI Pack、Board Game Icons、Game Icons。
2. 在 `Assets/Art/Kenney/` 下按 `UI`、`Icons`、`BoardGame` 分目录存放。
3. 给面板底图和按钮底图设置 Sprite Mode 为 Single，Mesh Type 为 Full Rect，必要时开启 9-slice border。
4. 优先替换：
   - 主菜单按钮和卡片背景
   - 玩家列表按钮
   - 身份提示卡
   - 讨论消息条目
   - 接受 / 拒绝 / 指控 / 信任 / 猜分按钮图标
5. 保留现有深色基调，但增加两套阵营色：
   - 一阵营：暖金 / 绿青
   - 负一阵营：赤红 / 暗紫

## 风格建议

当前游戏是轻量社交推理，不建议上过于卡通或奇幻的整套 UI。更适合“深色桌面 + 卡牌 + 棋子 + 线索图标”的方向：信息密度不变，但每个阶段都有明显视觉符号，能减少纯文本界面的疲劳感。
