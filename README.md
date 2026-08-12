# TechArt Tools

> A collection of technical-art oriented editor tools for Unity.
> 面向 Unity 技术美术（TA）的编辑器工具集。

**TechArt Tools** 是一套纯编辑器工具包（UPM Package），帮你做**资源体检、批量修复、内存与导入设置分析**。它是为技术美术和美术团队设计的：打开窗口、一键扫描、看到问题、点一下就修。

- 无需运行时代码，**零运行时开销**
- 兼容 URP / HDRP / Built-in（本包不依赖任何渲染管线）
- Unity **2021.3 LTS** 及以上

> English summary below.

---

## Features 功能

### 1. Audit Window 资产审计窗口
`Tools > TechArt Tools > Audit Window`

对选中资源、整个项目或当前打开的场景进行审计，并列出问题清单：

| 类别 | 检查项 |
| --- | --- |
| **Texture** | 尺寸超限、Read/Write 开启、法线贴图误开 sRGB、缺少 Mipmap、移动端未压缩、非二次幂贴图 |
| **Mesh** | Read/Write 开启（CPU 常驻内存）、高顶点数 / 高三角形数提醒 |
| **Material** | Shader 缺失、关键字过多（变体膨胀）、**失效的 shader keywords**、未赋值的贴图属性 |
| **Scene** | 丢失的脚本（Missing Script）、Realtime 灯光过多、Renderer 数量过高 |

每个问题都可以 **一键 Fix**，也可以 **Fix All** 批量修复。

### 2. Inspector 资源信息面板
`Tools > TechArt Tools > Inspector`

跟随 Project 窗口的选择，显示网格 / 贴图 / 材质的：
- 顶点数、三角形数、索引格式、包围盒、**运行时内存**
- 贴图导入设置（尺寸、Mipmap、Read/Write、sRGB、压缩格式）
- 材质关键字数量、**失效关键字警告**

### 3. Batch Tools 批量工具
`Tools > TechArt Tools > Batch`

| 菜单 | 作用 |
| --- | --- |
| Textures / Disable Read-Write | 批量关闭贴图 Read/Write |
| Textures / Enable Mipmaps | 批量开启 Mipmap |
| Textures / Cap Max Size to 2048 | 批量限制最大尺寸为 2048 |
| Textures / Set Android ASTC 6x6 | 批量设置 Android 平台 ASTC 6x6 压缩 |
| Meshes / Disable Read-Write | 批量关闭网格 Read/Write |
| Materials / Clear Stale Keywords | 批量清理失效的 shader keywords |

所有批量操作都有进度条，可随时取消。

---

## Installation 安装

### 方式一：通过 Unity Package Manager（推荐）

1. 打开 `Window > Package Manager`
2. 点击左上角 **+** → **Add package from git URL**
3. 粘贴：

```
https://github.com/hhhnnnk4-sudo/unity-techart-tools.git
```

### 方式二：OpenUPM（待发布）

```bash
openupm add com.hhhnnnk4.techarttools
```

### 方式三：手动

把整个仓库复制到项目的 `Packages/` 目录下（例如 `Packages/unity-techart-tools/`）。

---

## Usage 使用

1. 打开 `Tools > TechArt Tools > Audit Window`
2. 选择审计范围：`Selection`（Project 窗口中选中的资源）/ `Assets`（整个项目）/ `OpenScenes`（当前打开的场景）
3. 点击 **Audit**
4. 查看问题，点击 **Fix** 单修或 **Fix All** 全部修复
5. 阈值和开关都在配置资产中调整：`Tools > TechArt Tools > Create Audit Config`
   （首次打开审计窗口会自动在 `Assets/TechArtTools/` 下生成）

---

## Configuration 配置

配置资产 `TechArtAuditConfig` 是 `ScriptableObject`，包含以下分组：

- **Texture**：`MaxTextureSize`（默认 2048）、移动端压缩平台与格式（默认 Android / ASTC 6x6）、各检查项开关
- **Mesh**：Read/Write 检查、高顶点数 / 高三角形数阈值
- **Material**：关键字数量上限（默认 24）、失效关键字检查、未赋值贴图检查
- **Scene**：Missing Script、Realtime 灯光阈值、Renderer 数量阈值
- **General**：是否显示 Info 级别提示

---

## CI / Tests

仓库内置 GitHub Actions：
- **validate-package**：校验 `package.json` 与 asmdef 合法性（无需任何密钥，任何 fork 都会跑）
- **Unity EditMode tests**：在 Unity 2021.3 中运行 EditMode 冒烟测试。

> Unity 测试需要配置仓库 Secrets：`UNITY_EMAIL`、`UNITY_PASSWORD`、`UNITY_LICENSE`（可参考 [game-ci/unity-test-runner](https://github.com/game-ci/unity-test-runner)）。
> 未配置密钥时，该 Job 会自动跳过，不影响其他检查。

---

## Roadmap

- [ ] OpenUPM 发布与徽章
- [ ] 更多修复规则（贴图压缩质量、图集打包建议等）
- [ ] 场景资产引用 / 无引用资源清理（Dependencies 分析）
- [ ] 一键导出审计报告（JSON / Markdown）

欢迎提交 Issue / PR 完善规则与文档。

---

## License

[MIT](LICENSE) © 2026 hhhnnnk4

---

## English Summary

**TechArt Tools** is an editor-only Unity package (UPM, no runtime cost, pipeline-agnostic) for technical artists:

- **Audit Window** — scans textures / meshes / materials / open scenes for common production issues (oversized textures, read/write enabled, sRGB normal maps, missing mipmaps, uncompressed mobile textures, mesh read/write, stale shader keywords, missing scripts, too many realtime lights) with **one-click fixes** and **Fix All**.
- **Inspector** — live statistics (memory, import settings, keyword health) for the selected mesh / texture / material.
- **Batch tools** — bulk texture / mesh / material import fixes with progress and cancel.

**Install:** `Window > Package Manager > + > Add package from git URL` then paste `https://github.com/hhhnnnk4-sudo/unity-techart-tools.git`. Requires Unity 2021.3 LTS or newer.
