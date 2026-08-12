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
| 移动端平台未压缩 | Warning | 按配置应用压缩格式（默认 Android ASTC 6x6） |
| 非二次幂 | Info | 无（提示） |

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
| 启用关键字数 > MaxShaderKeywords | Warning | 无（提示手动处理） |
| 存在 Shader 中不存在的关键字 | Warning | 移除失效关键字 |
| 贴图属性（Map/Tex/Normal/Bump）未赋值 | Info | 无 |

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
