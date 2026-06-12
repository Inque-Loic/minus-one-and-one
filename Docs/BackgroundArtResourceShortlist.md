# 背景美术资源候选清单

更新时间：2026-06-11

目标：为《负一和一》补充背景图和背景界面装饰，让当前界面从“纯色面板”升级为“桌面推理 / 卡牌线索 / 阵营对抗”的视觉风格。

## 推荐视觉方向

当前项目已经导入 Kenney UI、Icons、BoardGame 资源，因此背景资源建议继续走低成本、授权清晰、风格统一的路线：

1. 深色桌面或墙面纹理作为底层背景。
2. 可平铺 pattern 作为暗纹，降低纯色大面积背景的空感。
3. 使用卡牌、筹码、棋子、线索图标做轻量装饰。
4. 背景整体保持低对比度，避免影响玩家列表、讨论文本和按钮识别。

## 首选资源

### Kenney Background Elements

链接：

https://kenney.nl/assets/background-elements

授权：

Creative Commons CC0

内容：

- 110 个 2D 背景元素
- 独立 PNG、示例背景、spritesheet、vector

推荐用途：

- 背景层上的装饰元素
- 主菜单或身份揭示界面的环境剪影
- 可组合出轻量场景，但不建议作为游戏主背景的全部视觉，因为题材偏自然/幻想，需要控制使用量

优先级：

中

### Kenney Pattern Pack

链接：

https://kenney.nl/assets/pattern-pack

授权：

Creative Commons CC0

内容：

- 80 个 seamless pattern / texture
- 适合做可平铺背景和面板暗纹

推荐用途：

- 主菜单背景暗纹
- 游戏面板背后的低透明度纹理
- 讨论阶段、猜分阶段的背景区分

优先级：

高

### Kenney Pattern Pixel Pack

链接：

https://kenney-assets.itch.io/pattern-pixel-pack

授权：

CC0 1.0 Universal

内容：

- 50 个 pixel art seamless patterns
- 包含 tilesheet 和独立 sprites

推荐用途：

- 如果项目想稍微偏桌游原型、独立游戏、像素质感，可以用作背景暗纹
- 不建议大面积直接用高对比像素图案；建议降低透明度或叠深色蒙版

优先级：

中

### OpenGameArt Background Texture for a Board Game

链接：

https://opengameart.org/content/background-texture-for-a-board-game

授权：

CC0

内容：

- 专门面向 board / strategy 的背景纹理

推荐用途：

- 游戏主背景底图
- 玩家列表和操作面板背后的桌面底纹
- 可与已导入的 Kenney BoardGame 棋子、卡牌、筹码组合

优先级：

高

### OpenGameArt Modern Wood Seamless Textures

链接：

https://opengameart.org/content/modern-wood-seamless-textures

授权：

CC0

内容：

- 木纹 / 桌面 / 家具方向的 1024x1024 seamless textures

推荐用途：

- 暗色桌面背景
- 结算界面和主菜单底层背景
- 与卡牌、筹码、棋子资源组合，营造桌游桌面感

优先级：

高

### OpenGameArt CC0 Wall Textures

链接：

https://opengameart.org/content/cc0-wall-textures

授权：

CC0

内容：

- 8 张墙面纹理

推荐用途：

- 身份揭示、讨论阶段、结算界面的弱背景
- 可作为深色墙面 / 档案室 / 密谈空间质感

优先级：

中

## 备选资源池

### itch.io CC0 Backgrounds

链接：

https://itch.io/game-assets/assets-cc0/tag-backgrounds

说明：

- itch.io 上 CC0 Backgrounds 分类资源较多。
- 适合继续找抽象背景、空间背景、地下城背景、恐怖室内背景等。
- 下载前仍建议逐个打开资源详情页确认授权和是否允许商用。

推荐用途：

- 如果后续想把《负一和一》做成更强氛围的“密室 / 档案室 / 黑盒推理”风格，可以从这里继续挑。

## 推荐落地方案

### 方案 A：深色桌游桌面

适合当前版本，改动小、统一性强。

资源组合：

- OpenGameArt Background Texture for a Board Game
- OpenGameArt Modern Wood Seamless Textures
- Kenney Pattern Pack
- 已导入的 Kenney BoardGame / Icons

界面效果：

- 主背景是暗木纹或桌面纹理
- 面板后方叠低透明度 pattern
- 玩家列表用棋子图标做身份感
- 讨论消息周围用卡牌、筹码做弱装饰

### 方案 B：线索档案室

适合强化社交推理和悬疑感。

资源组合：

- OpenGameArt CC0 Wall Textures
- Kenney Pattern Pack
- Kenney Game Icons
- 已导入的卡牌和筹码

界面效果：

- 背景像暗色墙面或档案室
- 面板像贴在墙上的记录卡
- 指控、信任、猜分按钮搭配图标

### 方案 C：抽象阵营对抗

适合更现代、轻量、信息图表风格。

资源组合：

- Kenney Pattern Pack
- Kenney Pattern Pixel Pack
- Kenney Game Icons

界面效果：

- 不使用具象场景，使用低透明几何 pattern
- 一阵营用金绿调，负一阵营用暗红调
- 背景按阶段轻微变化，突出回合、讨论、猜分、结算

## 建议导入路径

如果后续导入，建议统一放在：

```text
Assets/Art/Backgrounds/
├── Kenney/
│   ├── BackgroundElements/
│   ├── PatternPack/
│   └── PatternPixelPack/
└── OpenGameArt/
    ├── BoardGameTexture/
    ├── ModernWood/
    └── WallTextures/
```

授权文件建议放在：

```text
Assets/Art/Backgrounds/Licenses/
```

## 我的推荐

优先采用方案 A：深色桌游桌面。

原因：

- 和现有游戏题材最贴合。
- 和已经导入的 Kenney BoardGame 资源天然兼容。
- 不会抢 UI 文本的注意力。
- 后续只需替换 Canvas 背景 Image，再加少量装饰层，就能明显丰富界面。
