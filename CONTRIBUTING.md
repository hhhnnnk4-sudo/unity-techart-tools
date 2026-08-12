# Contributing

Thanks for taking the time to contribute to **TechArt Tools**! 🎉

This is a small, focused project — every contribution counts, from bug reports and docs to new audit rules.

## Ways to contribute

- Report bugs or missing rules via [Issues](https://github.com/hhhnnnk4-sudo/unity-techart-tools/issues)
- Suggest new audit rules / one-click fixes
- Improve README, documentation or translation
- Submit PRs that fix bugs or add tests

## Development setup

1. Clone the repo into a Unity project's `Packages/` folder:
   ```bash
   git clone https://github.com/hhhnnnk4-sudo/unity-techart-tools.git Packages/unity-techart-tools
   ```
2. Open the project in **Unity 2021.3 LTS** or newer.
3. All editor code lives under `Editor/`. Runtime assembly contains only package metadata.
4. Add your feature under `Tools > TechArt Tools`.

## Coding conventions

- Target **Unity 2021.3 LTS** API surface — avoid newer-only APIs unless guarded.
- Namespace: `Hhnnnk4.TechArtTools` (runtime), `Hhnnnk4.TechArtTools.Editor` (editor).
- No runtime package dependencies; keep the package pipeline-agnostic (URP / HDRP / Built-in).
- Follow existing style: `EditorGUILayout`-based UI, progress bars for long operations.
- Keep every new rule fixable and non-destructive (import settings only, never delete assets).

## Tests

- EditMode smoke tests live in `Tests~/Editor/` and run via GitHub Actions (`game-ci/unity-test-runner`).
- When adding a new audit rule or exporter, add a test in the same file.

## Pull request checklist

- [ ] Branch from `main`, PR title describes the change
- [ ] Update `CHANGELOG.md` under an `[Unreleased]` or new version section
- [ ] Bump `package.json` version + `Runtime/TechArtVersion.cs` for new releases
- [ ] Tests pass (or CI is green)

## Release process

Maintainers bump the version (`x.y.z`), update the changelog, tag, and draft a release on GitHub with release notes. Releases follow [Semantic Versioning](https://semver.org/).
