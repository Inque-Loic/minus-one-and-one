# 美术资源导入记录

更新时间：2026-06-10

## 总览

本次导入的资源统一放在：

`Assets/Art/Kenney`

资源来源均为 Kenney，授权文件已随资源一起保存到：

`Assets/Art/Kenney/Licenses`

当前导入内容：

- PNG 图片：1834 个
- Spritesheet XML：11 个
- License 文本：3 个

未导入内容：

- 原包里的音频、字体、AI、SVG、SWF、URL 文件暂未导入。
- 原因：当前需求是先丰富游戏界面，PNG 和 spritesheet 已足够直接用于 Unity UI；其余文件可在后续需要音效、矢量源文件或字体时再补充。

## 目录结构

```text
Assets/Art/
└── Kenney/
    ├── BoardGame/
    │   ├── PNG/
    │   └── Spritesheets/
    ├── Icons/
    │   ├── PNG/
    │   └── Spritesheets/
    ├── Licenses/
    ├── Previews/
    └── UI/
        └── PNG/
```

## 资源分类

### UI Pack

来源：

https://kenney.nl/assets/ui-pack

项目路径：

`Assets/Art/Kenney/UI/PNG`

许可证：

`Assets/Art/Kenney/Licenses/kenney_ui-pack_license.txt`

内容：

- Blue / Green / Grey / Red / Yellow 多色按钮
- Default / Double 两套按钮厚度
- 矩形、圆形、方形按钮
- 方向箭头
- 勾选、叉号、星标、滑条等基础 UI 元素

推荐用途：

- 主菜单按钮
- 身份提示卡按钮
- 玩家列表按钮
- 讨论阶段操作按钮
- 回合结束、结算面板的按钮底图

### Game Icons

来源：

https://kenney.nl/assets/game-icons

项目路径：

`Assets/Art/Kenney/Icons/PNG`

Spritesheet 路径：

`Assets/Art/Kenney/Icons/Spritesheets`

许可证：

`Assets/Art/Kenney/Licenses/kenney_game-icons_license.txt`

内容：

- Black / White 两套图标
- 1x / 2x 两种尺寸
- home、gear、return、checkmark、cross、warning、information、target、trophy、multiplayer 等通用图标
- 黑白图标 spritesheet 与 XML

推荐用途：

- 开始、返回、确认、取消、说明、警告等通用按钮
- 讨论阶段的“指控 / 信任 / 信息”辅助图标
- 结算界面的奖杯、排行榜、分数提示图标

### Boardgame Pack

来源：

https://kenney.nl/assets/boardgame-pack

项目路径：

`Assets/Art/Kenney/BoardGame/PNG`

Spritesheet 路径：

`Assets/Art/Kenney/BoardGame/Spritesheets`

许可证：

`Assets/Art/Kenney/Licenses/kenney_boardgame-pack_license.txt`

内容：

- Cards：卡背与扑克牌
- Chips：筹码
- Dice：骰子
- Pieces：黑、蓝、绿、紫、红、白、黄棋子
- 对应 spritesheet 与 XML

推荐用途：

- 玩家头像或玩家按钮上的棋子符号
- 一阵营 / 负一阵营身份卡装饰
- 猜分阶段的筹码或卡牌视觉
- 回合流程中的“桌游感”背景点缀
- 最终结算界面的排名和阵营结果装饰

## 建议使用方式

1. UI 面板底图优先使用 `Assets/Art/Kenney/UI/PNG/Grey`，再用 Unity Image 颜色叠加保持当前深色风格。
2. 一阵营按钮和提示可优先用 `Green` 或 `Yellow`；负一阵营可优先用 `Red`。
3. 图标优先使用 `Icons/PNG/White/2x`，适合深色 UI。
4. 玩家列表可从 `BoardGame/PNG/Pieces (...)` 中选棋子做头像，颜色可对应玩家编号或状态。
5. 需要批量切图时，使用 `Spritesheets` 目录下的 PNG + XML；需要单独拖到 UI Image 时，直接使用 `PNG` 目录里的独立图片。

## 临时下载位置

下载与解压的临时文件位于：

`Temp/ArtDownloads`

该目录属于 Unity 临时目录范围，已被 `.gitignore` 忽略，不作为正式项目资源引用。正式资源请只引用 `Assets/Art/Kenney` 下的文件。
