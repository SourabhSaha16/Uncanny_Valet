# Decisions log

Every question an agent asked, and the answer. Newest at the bottom.

| Date | Asked by | Question | Answer | Source |
| --- | --- | --- | --- | --- |
| 2026-11-05 | lead | Input system? | Input System package, behind `IInputSource` | Web build rules |
| 2026-11-05 | lead | Build compression? | Disabled, so GitHub Pages can serve the build | GitHub Pages headers |
| 2026-11-05 | lead | Prototype UI? | IMGUI debug overlay only | design.md section 10 |
| 2026-11-05 | lead | Which Unity version? | Unity 6000.2.5f1 (installed on the dev PC), not 6.3 LTS | Human, chat |
| 2026-11-05 | lead | How is the Web build made and published? | Built locally with tools/build-web.ps1 into the committed site/ folder; CI checks changelogs and publishes site/ to Pages. No Unity in CI, no secrets (the owner signs in to Unity with Google, so there is no password for CI activation) | Human, chat |
