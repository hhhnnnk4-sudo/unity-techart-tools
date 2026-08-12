# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.1.0] - 2026-08-12

### Added
- TechArt Audit Window: asset audits for Textures, Meshes, Materials and open Scenes.
- One-click fixes for common issues (mipmaps, read/write, texture size, mobile compression, sRGB normal maps, stale shader keywords).
- TechArt Inspector window: runtime statistics for the selected mesh / texture / material.
- Batch tools: texture (read-write, mipmaps, max size, Android ASTC), mesh (read-write), material (stale keyword cleanup).
- Auditable `TechArtAuditConfig` settings asset, auto-created on first use.
- Unit tests (EditMode smoke tests) and GitHub Actions CI (metadata validation + Unity EditMode tests).
- README, Documentation and MIT license.
