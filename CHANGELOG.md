# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.5.0] - 2026-08-12

### Added
- OpenUPM publishing workflow (`.github/workflows/openupm.yml`) that triggers a registry scan on `v*` tags.
- `.npmignore` so published tarballs only contain the package itself.

## [0.4.0] - 2026-08-12

### Added
- **Shader Usage** window (`Tools > TechArt Tools > Shader Usage`): material usage per shader, flags built-in pipeline shaders as URP/HDRP migration candidates, select-all-materials per shader.
- New **Prefab audit**: detects missing scripts inside prefabs during asset audits (new `Prefab` issue category).
- New `CheckPrefabMissingScripts` option in `TechArtAuditConfig` (default on).

### Changed
- `Assets` audit scope now also scans `.prefab` assets.

## [0.3.0] - 2026-08-12

### Added
- **Duplicate Finder** window (`Tools > TechArt Tools > Duplicate Finder`): finds byte-identical duplicate textures/materials by content hash, shows wasted size, with per-group and bulk deletion (keeps first, moves rest to OS trash).
- New material rule: **Hidden/ shader assigned** (warning).
- New `WarnOnHiddenShaders` option in `TechArtAuditConfig` (default on).
- Unit tests for duplicate detection and MD5 hashing.

## [0.2.0] - 2026-08-12

### Added
- Audit report export as **Markdown or JSON** (Export button in the Audit window).
- New texture rule: **base platform uncompressed** (warning + one-click compress fix).
- OSS housekeeping: `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md`, `SECURITY.md`, issue templates and PR template.
- README badges (Unity / release / license / CI).

### Changed
- `CheckBaseCompression` option in `TechArtAuditConfig` (default on).

## [0.1.0] - 2026-08-12

### Added
- TechArt Audit Window: asset audits for Textures, Meshes, Materials and open Scenes.
- One-click fixes for common issues (mipmaps, read/write, texture size, mobile compression, sRGB normal maps, stale shader keywords).
- TechArt Inspector window: runtime statistics for the selected mesh / texture / material.
- Batch tools: texture (read-write, mipmaps, max size, Android ASTC), mesh (read-write), material (stale keyword cleanup).
- Auditable `TechArtAuditConfig` settings asset, auto-created on first use.
- Unit tests (EditMode smoke tests) and GitHub Actions CI (metadata validation + Unity EditMode tests).
- README, Documentation and MIT license.
