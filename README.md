# TechArt Tools

> A collection of technical-art oriented editor tools for Unity.

![Unity](https://img.shields.io/badge/Unity-2021.3%20LTS%2B-black?logo=unity&logoColor=white)
![Release](https://img.shields.io/github/v/release/hhhnnnk4-sudo/unity-techart-tools)
![License](https://img.shields.io/github/license/hhhnnnk4-sudo/unity-techart-tools)
![CI](https://github.com/hhhnnnk4-sudo/unity-techart-tools/actions/workflows/ci.yml/badge.svg)

**TechArt Tools** is an editor-only Unity package (UPM) that helps technical artists and art teams **audit assets, apply one-click fixes, and analyze memory & import settings**. Open a window, scan, spot the problem, click to fix.

- **Zero runtime cost** — editor-only, no code in your builds
- **Pipeline-agnostic** — works with URP, HDRP and Built-in (no render pipeline dependency)
- Requires Unity **2021.3 LTS** or newer

---

## Features

### 1. Audit Window
`Tools > TechArt Tools > Audit Window`

Audits your selection, your whole project, or the currently open scenes and reports issues:

| Category | Checks |
| --- | --- |
| **Texture** | Oversized textures, Read/Write enabled, sRGB normal maps, missing mipmaps, uncompressed base/mobile formats, non-power-of-two |
| **Mesh** | Read/Write enabled (CPU-resident copy), high vertex / triangle count alerts |
| **Material** | Missing shader, too many keywords (variant bloat), **stale shader keywords**, unassigned texture properties, **Hidden/ shaders assigned** |
| **Scene** | Missing scripts, too many realtime lights, high renderer counts |
| **Prefab** | Missing scripts inside prefabs |

Every issue can be fixed with a **one-click Fix**, or batch-applied with **Fix All**. Reports can be **exported as Markdown or JSON** (button in the toolbar) for sharing or archiving in your pipeline.

### 2. Inspector
`Tools > TechArt Tools > Inspector`

A statistics panel that follows the Project window selection, showing for meshes / textures / materials:

- Vertex count, triangle count, index format, bounds and **runtime memory**
- Texture import settings (size, mipmaps, Read/Write, sRGB, compression)
- Material keyword count and **stale keyword warnings**

### 3. Shader Usage
`Tools > TechArt Tools > Shader Usage`

Analyzes which shaders are used by how many materials across your selection or the whole project:

- Material count per shader, sorted by usage
- **Built-in pipeline shaders** (Standard, Legacy, Sprites, UI, Particles, FX, Skybox, Nature) are flagged as URP/HDRP migration candidates
- One click selects every material on a given shader — handy before a mass migration

### 4. Duplicate Finder
`Tools > TechArt Tools > Duplicate Finder`

Finds **byte-identical duplicate textures and materials** across your selection or the whole project by content hash:

- Groups duplicates with wasted size per group (and total)
- Ping any asset with one click
- **Delete duplicates** per group or all at once (keeps the first, moves the rest to the OS trash)

A great first step when trimming project size or migrating asset sets.

### 5. Batch Tools
`Tools > TechArt Tools > Batch`

| Menu item | Action |
| --- | --- |
| Textures / Disable Read-Write | Bulk-disable Read/Write on textures |
| Textures / Enable Mipmaps | Bulk-enable mipmaps |
| Textures / Cap Max Size to 2048 | Bulk-cap maximum texture size at 2048 |
| Textures / Set Android ASTC 6x6 | Bulk-set Android ASTC 6x6 compression |
| Meshes / Disable Read-Write | Bulk-disable Read/Write on meshes |
| Materials / Clear Stale Keywords | Bulk-remove stale shader keywords |

All batch operations show a progress bar and can be cancelled at any time.

---

## Installation

### Option 1 — Unity Package Manager (recommended)

1. Open `Window > Package Manager`
2. Click **+** in the top-left → **Add package from git URL**
3. Paste:

```
https://github.com/hhhnnnk4-sudo/unity-techart-tools.git
```

### Option 2 — OpenUPM (pending publication)

```bash
openupm add com.hhhnnnk4.techarttools
```

### Option 3 — Manual

Copy this repository into your project's `Packages/` folder (e.g. `Packages/unity-techart-tools/`).

---

## Usage

1. Open `Tools > TechArt Tools > Audit Window`
2. Choose an audit scope: `Selection` (assets selected in the Project window) / `Assets` (whole project) / `OpenScenes` (currently open scenes)
3. Click **Audit**
4. Review the issues, then click **Fix** for a single issue or **Fix All** to apply everything
5. Tune thresholds and toggles in the config asset: `Tools > TechArt Tools > Create Audit Config`
   (a default config is auto-created under `Assets/TechArtTools/` the first time you open the Audit window)

---

## Configuration

The `TechArtAuditConfig` settings asset (a `ScriptableObject`) is grouped as follows:

- **Texture**: `MaxTextureSize` (default 2048), mobile platform & compression format (default Android / ASTC 6x6), per-check toggles
- **Mesh**: Read/Write check, high vertex / triangle thresholds
- **Material**: keyword limit (default 24), stale keyword check, missing texture check
- **Scene**: missing scripts, realtime light threshold, renderer count threshold
- **General**: whether to show Info-level findings

---

## CI / Tests

The repository ships GitHub Actions:

- **validate-package** — validates `package.json` and asmdef files (no secrets needed, runs on every fork)
- **Unity EditMode tests** — runs EditMode smoke tests on Unity 2021.3

> Unity tests require repo secrets: `UNITY_EMAIL`, `UNITY_PASSWORD`, `UNITY_LICENSE` (see [game-ci/unity-test-runner](https://github.com/game-ci/unity-test-runner)).
> If the secrets are not configured, that job is skipped automatically and does not affect other checks.

---

## Roadmap

- [x] Audit report export (Markdown / JSON)
- [x] Base & mobile compression rules
- [x] Duplicate asset finder (textures / materials)
- [x] Shader usage analyzer (URP/HDRP migration candidates)
- [ ] OpenUPM publication & badges
- [ ] More fix rules (atlas packing suggestions, etc.)
- [ ] Dependency analysis / unused-asset cleanup

Issues and PRs are welcome. See [CONTRIBUTING](CONTRIBUTING.md).

---

## License

[MIT](LICENSE) © 2026 hhhnnnk4
