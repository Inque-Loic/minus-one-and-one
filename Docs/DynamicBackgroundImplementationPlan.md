# 动态背景施工计划

更新时间：2026-06-11

## 目标

为《负一和一》制作一套富有游戏特色的动态背景系统，让界面从“静态深色 UI”升级为“桌面推理 / 阵营对抗 / 线索浮现”的动态视觉体验。

第一版目标不是做复杂特效，而是用低成本、稳定、可维护的 Unity UI 动画实现明显的氛围提升。

## 设计关键词

- 暗色桌面
- 卡牌
- 筹码
- 棋子
- 线索节点
- 阵营色
- 低对比动态
- 不抢文字可读性

## 推荐视觉方向

采用“深色桌游桌面 + 线索暗纹 + 卡牌筹码漂浮”的方向。

整体效果：

- 背景底层像一张暗色桌面或档案室桌面。
- 中层有非常淡的图案纹理缓慢移动。
- 前景点缀卡牌、筹码、棋子、图标，缓慢浮动。
- 不同游戏阶段切换轻微色调和动效强度。
- 一阵营偏金绿，负一阵营偏暗红。

## 范围

### 第一版包含

- 主菜单动态背景
- 游戏主界面动态背景
- 身份揭示背景色调变化
- 讨论阶段背景脉冲
- 结算阶段胜负色调变化
- 背景装饰物缓慢浮动、旋转、透明度呼吸
- 根据 `RoundPhase` 切换背景状态

### 第一版不包含

- 复杂 Shader Graph
- 3D 背景
- 粒子系统
- 关系网连线系统
- 真实物理掉落动画
- 每个按钮单独定制动效

这些可以作为第二版增强。

## 资源需求

### 已有资源

已导入：

```text
Assets/Art/Kenney/
├── BoardGame/
├── Icons/
├── Licenses/
├── Previews/
└── UI/
```

可直接使用：

- `BoardGame/PNG/Cards`：卡牌、卡背
- `BoardGame/PNG/Chips`：筹码
- `BoardGame/PNG/Pieces (...)`：棋子
- `Icons/PNG/White/2x`：白色图标，适合深色背景
- `UI/PNG/Grey`：可作为面板底图或暗纹辅助

### 建议补充导入

背景图和纹理建议放在：

```text
Assets/Art/Backgrounds/
├── Kenney/
│   ├── PatternPack/
│   └── BackgroundElements/
└── OpenGameArt/
    ├── BoardGameTexture/
    ├── ModernWood/
    └── WallTextures/
```

优先导入：

- Kenney Pattern Pack
- OpenGameArt board game / wood seamless texture

## 文件结构

建议新增：

```text
Assets/
├── Prefabs/
│   └── UI/
│       └── DynamicBackground.prefab
├── Scripts/
│   └── UI/
│       ├── DynamicBackgroundController.cs
│       └── DynamicBackgroundElement.cs
└── Art/
    └── Backgrounds/
```

如果当前项目不想新增 `Scripts/UI` 子目录，也可以先放在：

```text
Assets/Scripts/DynamicBackgroundController.cs
Assets/Scripts/DynamicBackgroundElement.cs
```

## Prefab 结构

`DynamicBackground.prefab` 建议结构：

```text
DynamicBackground
├── BaseLayer
│   └── BaseImage
├── PatternLayer
│   ├── PatternA
│   └── PatternB
├── OrnamentLayer
│   ├── Ornament_01
│   ├── Ornament_02
│   ├── Ornament_03
│   └── ...
├── VignetteLayer
│   └── Vignette
└── ColorOverlay
```

### BaseLayer

用途：

- 全屏底图。
- 使用暗木纹、桌面纹理或深色纯色。

建议：

- `Image` 设置为全屏拉伸。
- 颜色控制在深色低饱和。
- 不要高对比，避免影响 UI 文本。

### PatternLayer

用途：

- 承载低透明度暗纹。
- 使用棋盘、几何、纸纹、线索纹理。

动画：

- `PatternA` 极慢横向移动。
- `PatternB` 极慢纵向移动或反向移动。
- 透明度 0.04 到 0.12。

### OrnamentLayer

用途：

- 放置卡牌、筹码、棋子、图标等装饰。

建议数量：

- 桌面端 10 到 16 个。
- 移动端或低性能模式 6 到 10 个。

动画：

- 慢速上下浮动。
- 轻微旋转。
- 透明度呼吸。
- 少量不同步，避免机械感。

### VignetteLayer

用途：

- 暗角，帮助中心 UI 信息聚焦。

实现方式：

- 使用一张暗角 PNG。
- 或使用四周渐变图片。

### ColorOverlay

用途：

- 阶段色彩叠加。
- 身份揭示和胜负结算时切换阵营色。

建议颜色：

- 默认：深蓝黑，透明度 0.1
- 一阵营：金绿，透明度 0.08 到 0.14
- 负一阵营：暗红，透明度 0.1 到 0.18
- 讨论：冷蓝紫，透明度 0.08 到 0.12
- 结算：根据胜负切换金绿或暗红

## 脚本设计

### DynamicBackgroundController

职责：

- 持有所有背景层引用。
- 持有阶段配色配置。
- 根据游戏阶段切换背景状态。
- 控制 Pattern 层移动。
- 控制 Overlay 颜色渐变。
- 通知 Ornament 元素切换动画强度。

关键字段：

```csharp
public Image baseImage;
public RectTransform patternA;
public RectTransform patternB;
public Image colorOverlay;
public List<DynamicBackgroundElement> ornaments;

public Color defaultOverlayColor;
public Color selectingOverlayColor;
public Color respondingOverlayColor;
public Color discussionOverlayColor;
public Color scoreGuessingOverlayColor;
public Color gameOverOverlayColor;

public float patternScrollSpeedA;
public float patternScrollSpeedB;
public float overlayLerpSpeed;
```

关键方法：

```csharp
public void SetPhase(RoundPhase phase);
public void SetIdentityTone(bool isVillain);
public void SetResultTone(bool villainWin, bool isDraw);
private void UpdatePatternMotion();
private void UpdateOverlayColor();
private void ApplyMotionIntensity(float intensity);
```

### DynamicBackgroundElement

职责：

- 控制单个装饰物的浮动、旋转和透明度呼吸。
- 提供强度参数，方便不同阶段统一调节。

关键字段：

```csharp
public RectTransform rectTransform;
public CanvasGroup canvasGroup;
public float floatAmplitude;
public float floatSpeed;
public float rotationAmplitude;
public float rotationSpeed;
public float alphaMin;
public float alphaMax;
```

关键方法：

```csharp
public void SetIntensity(float intensity);
private void UpdateMotion();
```

## 阶段动效设计

### Main Menu

氛围：

- 桌面推理感。
- 安静但有生命力。

效果：

- 暗木纹底图。
- 少量卡牌和筹码漂浮。
- Pattern 极慢移动。
- Overlay 为默认深蓝黑。

强度：

- 0.35

### Selecting

氛围：

- 玩家正在选择接触对象。

效果：

- 背景保持稳定。
- 棋子或目标图标轻微呼吸。
- Overlay 偏冷色。

强度：

- 0.45

### Responding

氛围：

- 等待接受或拒绝，有轻微紧张感。

效果：

- Overlay 加一点暖红。
- 装饰物浮动幅度略提升。
- 可在接受 / 拒绝按钮附近后续加一次性脉冲。

强度：

- 0.6

### Discussion

氛围：

- 信息流、指控、信任与怀疑。

效果：

- Pattern 移动略快。
- 图标透明度呼吸更明显。
- Overlay 偏冷蓝紫。
- 讨论消息出现时，后续可加局部闪烁。

强度：

- 0.75

### RoundEnd

氛围：

- 本回合线索揭示。

效果：

- Overlay 短暂加深。
- 卡牌或筹码稍微放大后回落。

强度：

- 0.65

### ScoreGuessing

氛围：

- 结算前的推理确认。

效果：

- 筹码和卡牌装饰权重提高。
- Overlay 偏金色或冷白。
- Pattern 速度降低，让玩家专注填写。

强度：

- 0.5

### GameOver

氛围：

- 胜负揭晓。

效果：

- 根据胜负切换阵营色。
- 一阵营胜：金绿 Overlay。
- 负一阵营胜：暗红 Overlay。
- 平局：灰蓝 Overlay。
- 装饰物慢慢归位或停止。

强度：

- 0.4

## 与现有 UIManager 的接入

当前 `UIManager` 已经负责：

- 主菜单
- 身份提示
- 游戏界面
- 讨论界面
- 猜分界面
- 结算界面

建议接入点：

1. 在场景 Canvas 下添加 `DynamicBackground`，放在所有 UI 面板之前。
2. 在 `UIManager` 中增加引用：

```csharp
public DynamicBackgroundController dynamicBackground;
```

3. 在关键 UI 刷新点调用：

```csharp
dynamicBackground.SetPhase(GameManager.Instance.GetPhase());
```

建议调用位置：

- `ReturnToMainMenu`
- `StartGame`
- `ShowIdentityPanel`
- `RefreshGameUI`
- `ShowHumanResponsePanel`
- `SetDiscussionPanelActive`
- `RunScoreGuessingPhase`
- `ShowEndGame`

4. 身份揭示时调用：

```csharp
dynamicBackground.SetIdentityTone(isVillain);
```

5. 结算时调用：

```csharp
dynamicBackground.SetResultTone(villainWin, isDraw);
```

## 性能要求

第一版全部基于 UI Image 和 RectTransform 动画。

控制目标：

- 装饰物数量不超过 16 个。
- 每帧只做简单 sin、cos、颜色 lerp 和位置更新。
- 不实时 Instantiate / Destroy 背景元素。
- 不在 Update 中频繁 Find。
- 不使用高分辨率大图铺满多个层级。

建议贴图尺寸：

- 底图：1920x1080 或 2048x2048 以内。
- Pattern：512x512 或 1024x1024。
- 装饰 PNG：128 到 512。

## 实施步骤

### 步骤 1：导入背景资源

任务：

- 导入 Pattern Pack。
- 导入桌面 / 木纹 / 墙面背景。
- 放入 `Assets/Art/Backgrounds`。
- 保存授权文件。
- 更新 `Docs/ArtAssets.md`。

验收：

- Unity Project 视图能看到背景资源。
- 文档能查到来源、授权、路径。

### 步骤 2：创建 DynamicBackground Prefab

任务：

- 创建全屏 UI Prefab。
- 搭建 BaseLayer、PatternLayer、OrnamentLayer、VignetteLayer、ColorOverlay。
- 选择 10 到 16 个装饰资源。

验收：

- Prefab 单独拖到 Canvas 下能覆盖全屏。
- 不遮挡前景 UI 交互。
- 背景视觉不影响文字阅读。

### 步骤 3：实现 DynamicBackgroundElement

任务：

- 单个装饰元素可以浮动、旋转、透明度呼吸。
- 支持强度参数。

验收：

- 每个装饰元素动画节奏不同。
- 动画平滑，无突兀跳变。

### 步骤 4：实现 DynamicBackgroundController

任务：

- 控制 Pattern 移动。
- 控制 Overlay 颜色渐变。
- 统一调节装饰元素强度。
- 提供阶段切换 API。

验收：

- 调用 `SetPhase` 后背景颜色和强度会平滑变化。
- 多次切换阶段不会闪烁或重置位置。

### 步骤 5：接入 UIManager

任务：

- 在场景 Canvas 添加背景 Prefab。
- 在 UIManager 增加引用。
- 在关键流程点调用背景切换方法。

验收：

- 主菜单、身份、游戏、讨论、猜分、结算阶段都有对应背景状态。
- 游戏流程不受影响。
- Null 引用安全，未配置背景时游戏仍可运行。

### 步骤 6：调色和可读性检查

任务：

- 检查深色背景下所有文本可读性。
- 检查按钮和玩家列表是否被背景抢视觉。
- 调整透明度、暗角和 Overlay。

验收：

- 主菜单文字清晰。
- 玩家列表清晰。
- 讨论消息清晰。
- 猜分输入清晰。
- 结算排名清晰。

### 步骤 7：测试

任务：

- Unity Play Mode 手动跑一局。
- 覆盖主菜单、身份揭示、接触、响应、讨论、猜分、结算。
- 检查 Console 是否有错误。

验收：

- 无编译错误。
- 无运行时 NullReference。
- 背景在各阶段正常变化。
- 没有明显卡顿。

## 验收标准

第一版完成后应满足：

- 背景不再是单调纯色。
- 视觉能传达桌游推理氛围。
- 所有 UI 文本仍然清楚。
- 动态效果低调，不干扰操作。
- 阶段切换有明确但克制的视觉反馈。
- 资源和授权记录完整。
- 代码结构可继续扩展到第二版。

## 第二版增强方向

### 关系网背景

在讨论阶段加入节点和连线，表现玩家之间的怀疑关系。

可用方式：

- UI LineRenderer
- 自定义 Graphic
- 简单 RectTransform 线段

### 局部事件脉冲

在以下事件触发局部动效：

- 接触发起
- 接触接受
- 接触拒绝
- 指控
- 信任
- 猜分提交
- 阵营胜利

### Shader Graph

可加入：

- 噪声流动
- 扫描线
- 纸面纹理波动
- 柔光边缘

### 粒子层

可加入：

- 微弱尘埃
- 信息点漂浮
- 结算时轻微亮点

## 风险与注意事项

- 不要让背景动画过快，否则会影响阅读和决策。
- 不要使用过亮、过饱和背景。
- 不要在每个阶段都强烈变化，阶段反馈应该克制。
- 不要在 Update 中查找对象或频繁分配内存。
- 不要把背景元素放到前景 UI 之上。
- 不要让装饰元素遮挡按钮、输入框、讨论文本。

## 推荐优先级

优先做：

1. 背景资源导入
2. DynamicBackground Prefab
3. 装饰物浮动
4. 阶段 Overlay 颜色变化
5. UIManager 接入

暂缓做：

1. Shader Graph
2. 粒子系统
3. 关系网连线
4. 事件局部脉冲

第一版先把“氛围”做出来，第二版再把“反馈”做精。
