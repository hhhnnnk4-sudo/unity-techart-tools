# TechArt Tools 使用文档

## 目录结构

```
Packages/com.hhhnnnk4.techarttools/
├─ Runtime/                    # 运行时（无依赖，仅版本常量）
│  ├─ TechArtTools.Runtime.asmdef
│  └─ TechArtVersion.cs
├─ Editor/                     # 全部编辑器逻辑
│  ├─ TechArtTools.Editor.asmdef
│  ├─ Core/                    # 审计核心
│  ├─ UI/                      # 窗口
│  ├─ Batch/                   # 批量工具
│  └─ Menu/                    # 菜单项
├─ Tests~/Editor/              # EditMode 冒烟测试（~ 结尾目录不会被 Unity 导入）
└─ Documentation~/
```

## 审计规则详解

### Texture（贴图）

| 规则 | 严重级别 | 修复动作 |
| --- | --- | --- |
| `maxTextureSize > MaxTextureSize` | Warning | 设为 MaxTextureSize |
| Read/Write 开启 | Warning | 关闭 Read/Write |
| 法线贴图且 sRGB 开启 | Error | 关闭 sRGB |
| 非法线、非 Sprite、无 Mipmap | Warning | 开启 Mipmap |
| 基础平台未压缩（≥512px） | Warning | 设为 Compressed |
| 移动端平台未压缩 | Warning | 按配置应用压缩格式（默认 Android ASTC 6x6） |
| 非二次幂 | Info | 无（提示） |

## 报告导出

审计窗口工具栏的 **Export** 按钮可将当前结果导出为：

- **Markdown（.md）**：含汇总信息与问题表格，适合提交到美术/QA 流程
- **JSON（.json）**：结构化数据，便于 CI 或脚本继续处理

导出的 JSON 结构（`TechArtReportRoot`）：

```json
{
  "generated": "2026-08-12 12:00:00",
  "items": [
    {
      "category": "Texture",
      "severity": "Warning",
      "title": "...",
      "message": "...",
      "assetPath": "Assets/...",
      "fixable": true
    }
  ]
}
```

### Mesh（网格）

| 规则 | 严重级别 | 修复动作 |
| --- | --- | --- |
| Read/Write 开启（ModelImporter） | Warning | 关闭 |
| 顶点数 ≥ WarnHighVertexCount | Info | 无 |
| 三角形数 ≥ WarnHighTriangleCount（需可读） | Info | 无 |

### Material（材质）

| 规则 | 严重级别 | 修复动作 |
| --- | --- | --- |
| Shader 缺失 | Error | 无 |
| 使用 Hidden/ shader | Warning | 无（提示改用公开 shader） |
| 启用关键字数 > MaxShaderKeywords | Warning | 无（提示手动处理） |
| 存在 Shader 中不存在的关键字 | Warning | 移除失效关键字 |
| 贴图属性（Map/Tex/Normal/Bump）未赋值 | Info | 无 |

## Duplicate Finder 重复资源查找

`Tools > TechArt Tools > Duplicate Finder`

按**内容 MD5 哈希**找出字节级完全相同的重复资源（纹理 / 材质）：

- 范围：Selection（Project 窗口选中）或 Assets（整个项目）
- 按浪费空间从大到小排序展示，显示每组的哈希值
- 点击路径可 Ping 对应资源
- **Delete**：删组内重复（保留第一个），**Delete All Duplicates**：批量清理
  - 删除前有确认弹窗，被删资源进入 OS 回收站（可通过 AssetDatabase 恢复）

> 说明：哈希基于源文件字节，因此只有"同一文件的拷贝"会被识别为重复；
> 内容相似但重新导出/压缩过的文件不会被误判。

## Shader Usage 着色器使用分析

`Tools > TechArt Tools > Shader Usage`

统计每种 shader 被多少材质使用，并按使用量排序：

- **内置管线 shader**（Standard、Legacy Shaders/、Sprites/、UI/、Particles/、FX/、Skybox/、Nature/ 等）会被标记为 **built-in**，提示适合作为 URP/HDRP 迁移候选
- 点击 **Select** 可一键选中该 shader 对应的全部材质（方便批量迁移）

## Prefab 审计

在资源审计（Selection / Assets 范围）中，`.prefab` 也会被扫描：

- **缺失脚本（Missing Script）**：Prefab 内任一子物体存在丢失的脚本组件时报告 `Prefab` 类别 Warning
- 通过 `PrefabUtility.LoadPrefabContents` 在内存中检查，不会修改任何资源
- 由配置项 `CheckPrefabMissingScripts`（默认开启）控制

## Find References 引用查找

`Tools > TechArt Tools > Find References`（或在 Project 窗口右键资源 → `TechArt Tools > Find References`）

查找选中资源被哪些文件引用（按 GUID 匹配序列化文件）：

- 扫描范围：`Assets/` 与 `Packages/` 下的 `.unity`、`.prefab`、`.mat`、`.asset`、`.controller`、`.spriteatlas` 等文本序列化文件
- 支持**进度条与取消**；结果中 `.unity` 可直接打开场景
- 删除 / 移动 / 重命名资源前用它确认影响范围

> 原理：Unity 文本序列化用 `guid: xxx` 记录引用，直接按行文本匹配 GUID，无需构建依赖图，速度极快。

### Scene（场景）

| 规则 | 严重级别 | 修复动作 |
| --- | --- | --- |
| 存在 Missing Script | Warning | 无 |
| Realtime 灯光 > WarnRealtimeLights | Warning | 无 |
| Renderer 数量 > WarnRendererCount | Info | 无 |

## 开发说明

- 本包**不引用任何第三方包**，也不依赖渲染管线，因此适用于 URP / HDRP / Built-in 项目。
- 编辑器代码全部位于 `Editor/` 目录，通过 asmdef 限定 `Editor` 平台，不会进入构建产物。
- 运行时程序集仅包含版本常量，供代码里做包版本判断使用。

## 常见问题

**Q: 审计窗口把无关资源也算进去了？**
在 `Assets` 范围内，程序只处理贴图 / 网格 / 材质类扩展名（.png/.jpg/.tga/.psd/.mat/.fbx 等）。Shader、脚本、场景资产不会被当作资源审计。

**Q: Fix All 会不会误伤？**
所有修复都是可逆的导入设置变更，不做任何资源删除。建议先看一遍问题列表再 Fix All。

**Q: 兼容 Unity 2020？**
未在 2020 上测试，部分 API（`GetShaderGlobalKeywords` 等）在更早版本存在，但以 2021.3 LTS 为最低支持版本。
