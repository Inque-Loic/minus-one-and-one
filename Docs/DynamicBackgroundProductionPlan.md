# 《负一和一》动态主界面背景制作计划

更新时间：2026-06-11  
目标：采用“方案 3”，制作真正的动态主界面背景，并接入 Unity 标题页。  
推荐路线：**After Effects / Blender 制作循环动态背景视频 + Unity VideoPlayer 播放 + Unity UI 叠加菜单与轻量交互动效。**

## 1. 制作目标

当前主界面是一张静态仙山云海背景。新目标是把它升级为一段可循环播放的动态标题背景：

> 云海缓慢流动，日光呼吸，远山与宫殿有轻微纵深，卡牌/阵法/光点在画面中以克制方式运动，整体像一张活着的游戏封面。

注意：动态背景服务于标题页氛围，不做剧情动画，不做长片头，不抢菜单操作。

## 2. 推荐最终交付形态

| 交付物 | 规格 | 用途 |
| --- | --- | --- |
| 主背景视频 | MP4 / H.264，1920x1080，30fps，8-12 秒无缝循环 | Unity 主菜单背景 |
| 备用静态图 | PNG / JPG，1920x1080 | 视频加载失败或低性能 fallback |
| Unity 接入 Prefab | VideoPlayer + RawImage 或 RenderTexture | 主菜单场景播放 |
| 源工程文件 | AE `.aep` 或 Blender `.blend` | 后续修改 |
| 素材分层文件 | PSD / PNG layers | 保留可编辑资产 |

推荐先做 1920x1080。若包体或性能压力大，再导出 1280x720 版本。

## 3. 工具选择

### 3.1 推荐主工具：After Effects

适合原因：

- 2D 背景动态化效率高。
- 云雾、光晕、粒子、卡牌漂浮、镜头慢推都容易做。
- 输出 MP4 方便 Unity 播放。

推荐用于：

- 背景分层合成。
- 云雾漂移。
- 日光呼吸。
- 阵法线稿淡入淡出。
- 卡牌/牌影漂浮。
- 粒子光点。
- 无缝循环输出。

### 3.2 可选工具：Blender

适合原因：

- 如果想做 2.5D 空间感、镜头推进、卡牌在空间中旋转，Blender 更强。

推荐用于：

- 立体卡牌。
- 轻微镜头推拉。
- 2.5D 分层场景。

不建议当前全用 Blender 重建背景，成本较高。

### 3.3 辅助工具

| 工具 | 用途 |
| --- | --- |
| Photoshop / Krita / Photopea | 拆分静态背景、补图、绘制牌影和阵法贴图。 |
| After Effects Media Encoder | 输出 H.264 MP4。 |
| Unity | 播放视频、叠加菜单、性能检查。 |

## 4. 动态内容设计

### 4.1 背景层动态

| 层 | 动效 | 强度 |
| --- | --- | --- |
| 天空/日光 | 光晕 alpha 呼吸，8-10 秒一轮 | 很弱 |
| 云海 | 横向缓慢漂移，局部透明变化 | 中弱 |
| 远山 | 几乎不动，只做轻微视差 | 很弱 |
| 宫殿 | 保持稳定，可有轻微光照闪动 | 很弱 |
| 前景竹叶 | 轻微摆动 | 弱 |
| 左下棋盘 | 稳定不动，作为视觉锚点 | 无 |

### 4.2 桌游推理元素动态

| 元素 | 动效 | 设计要求 |
| --- | --- | --- |
| 半透明身份牌影 | 缓慢漂浮、轻微旋转、错位运动 | 不用廉价实体贴图，尽量像背景中的符号。 |
| 淡金阵法圆环 | 极慢旋转，透明度 8%-15% | 放在宫殿后或云层中。 |
| 红/绿微光 | 在牌影边缘或云路上呼吸 | 暗示阵营，不要太亮。 |
| 光点粒子 | 少量慢速漂浮 | 金色/青色为主。 |

### 4.3 镜头动态

推荐使用非常轻的镜头运动：

- 8-12 秒内缓慢推近 1%-2%。
- 或从左下棋盘向右上宫殿产生极轻视差。
- 不要明显晃动，不要产生晕眩。

## 5. 循环设计

动态背景必须无缝循环。

推荐时长：

- 8 秒：轻量，包体小。
- 10 秒：最推荐，节奏自然。
- 12 秒：更舒缓，但包体略大。

循环原则：

1. 所有位移使用首尾一致的循环曲线。
2. 云雾横移可用重复贴图或两层云雾交替。
3. 光晕 alpha 用正弦曲线，首尾同值。
4. 卡牌旋转和漂浮首尾回到同一姿态。
5. 粒子若难无缝，可在 Unity 中另做粒子，不烘进视频。

## 6. Unity 接入方案

### 6.1 推荐：VideoPlayer + RenderTexture + RawImage

结构：

```text
Canvas
└── BackgroundVideoRoot
    ├── RawImage
    └── VideoPlayer
```

做法：

1. 将 MP4 放入 `Assets/Art/Backgrounds/MainMenu/Video/`。
2. 创建 RenderTexture，例如 `MainMenuBackgroundRT`.
3. VideoPlayer 输出到 RenderTexture。
4. RawImage 全屏显示 RenderTexture。
5. VideoPlayer 设置：
   - Play On Awake: true
   - Loop: true
   - Audio Output: None
6. UI 菜单保持在视频层之上。

### 6.2 备用静态图

如果视频无法播放：

- 显示备用静态背景。
- 不影响开始游戏/游戏测试。

建议实现一个简单脚本：

```text
MainMenuVideoBackground
- VideoPlayer
- RawImage videoImage
- Image fallbackImage
- On video prepared: show video
- On error: show fallback
```

## 7. 文件结构建议

```text
Assets/
└── Art/
    └── Backgrounds/
        └── MainMenu/
            ├── Video/
            │   ├── main_menu_bg_1080p_loop.mp4
            │   └── main_menu_bg_720p_loop.mp4
            ├── Textures/
            │   ├── main_menu_bg_fallback.png
            │   ├── mist_layer_01.png
            │   └── sigil_overlay.png
            └── RenderTextures/
                └── MainMenuBackgroundRT.renderTexture

ArtSource/
└── MainMenuBackground/
    ├── main_menu_bg_layers.psd
    ├── main_menu_bg_motion.aep
    └── exports/
```

说明：

- `Assets` 下放 Unity 运行需要的文件。
- `ArtSource` 放源工程，是否纳入版本管理可按体积决定。

## 8. 制作阶段

### Phase 0：素材准备，0.5-1 天

任务：

1. 确认当前主界面背景原图分辨率。
2. 保存一份不含 UI 的纯背景图。
3. 用 Photoshop/Krita/Photopea 拆出基础层：
   - 天空/日光
   - 云海
   - 远山/宫殿
   - 前景竹叶
   - 左下棋盘
   - 右下塔阁
4. 简单补图，避免分层移动露出空洞。

交付：

- `main_menu_bg_layers.psd`
- 各层 PNG 导出

### Phase 1：动态样片，1-2 天

任务：

1. 在 After Effects 中建立 1920x1080 / 30fps / 10s 合成。
2. 制作云雾漂移。
3. 制作日光呼吸。
4. 制作轻微镜头推近。
5. 加入淡金阵法圆环。
6. 加入半透明红/绿牌影。
7. 输出低码率预览 MP4。

交付：

- 10 秒预览视频
- 一张关键帧截图

验收：

- 看起来像动态标题页，不像 PPT 动画。
- 首尾循环不跳。
- 不影响左侧文字区。

### Phase 2：Unity 接入，0.5-1 天

任务：

1. 导入 MP4。
2. 创建 VideoPlayer + RenderTexture。
3. 保留备用静态图。
4. 将菜单 UI 放在视频层上方。
5. 确认开始游戏、游戏测试点击正常。

交付：

- Unity 主菜单可播放动态背景。

### Phase 3：优化与压缩，0.5-1 天

任务：

1. 调整视频码率。
2. 观察启动加载时间。
3. 检查 Windows 包体积变化。
4. 如有性能问题，导出 720p 或降低码率版本。

建议码率：

| 分辨率 | 码率 |
| --- | --- |
| 1920x1080 30fps | 8-16 Mbps |
| 1280x720 30fps | 4-8 Mbps |

### Phase 4：最终验收，0.5 天

检查：

- 主菜单视频自动播放。
- 视频循环无明显跳帧。
- 左侧标题和菜单清楚。
- 1280x720 与 1920x1080 UI 不错位。
- Windows 构建包可播放视频。
- 如果视频加载失败，静态图 fallback 可见。

## 9. 风险与应对

| 风险 | 表现 | 应对 |
| --- | --- | --- |
| 视频包体过大 | Release 包明显变大 | 降码率，导出 720p，缩短循环时长。 |
| 视频播放兼容问题 | 某些机器黑屏 | 保留 fallback 静态图。 |
| 循环不自然 | 首尾跳动 | AE 中使用循环表达式或首尾同帧。 |
| 背景抢 UI | 文字看不清 | 左侧加暗色渐变，降低视频局部亮度。 |
| 动效太花 | 玩家分心 | 减少粒子，降低卡牌运动幅度。 |
| 分层穿帮 | 移动时露出空洞 | 外部补图，或降低视差幅度。 |

## 10. 推荐执行口径

当前项目建议先做：

1. **10 秒 1080p 循环视频**
2. **AE 制作**
3. **Unity VideoPlayer 接入**
4. **保留静态 fallback**

暂时不做：

- 长片头。
- 角色立绘动态。
- 大量 3D 重建。
- 复杂交互背景。

## 11. 给执行会话的任务描述

```text
请阅读 Docs/DynamicBackgroundProductionPlan.md。
目标是为主菜单制作真正的动态背景，而不是 Unity 伪动态。

推荐路线：
1. 用 Photoshop/Krita/Photopea 将当前主菜单纯背景拆成天空、云海、宫殿、前景竹叶、棋盘、塔阁等层。
2. 用 After Effects 制作 1920x1080、30fps、10 秒无缝循环动态背景。
3. 动效包括云雾漂移、日光呼吸、轻微镜头推近、淡金阵法、半透明红绿牌影。
4. 导出 MP4/H.264，并保留静态 fallback。
5. 在 Unity 中用 VideoPlayer + RenderTexture + RawImage 接入主菜单。
6. 确认开始游戏和游戏测试按钮仍可点击。
7. 检查 Windows 构建包中视频能正常播放。
```

