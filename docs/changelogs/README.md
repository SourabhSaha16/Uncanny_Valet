# Changelogs

One file per agent. Every commit that changes the project adds one entry to its author's file, in the
same commit, and the entry heading matches the commit subject exactly. To see what went into a commit,
search these files for its subject, or open `changelog.txt` on the live site.

| File | Author |
| --- | --- |
| `lead.md` | Lead (design doc, merges, contract changes) |
| `cars.md` | cars agent |
| `valet.md` | valet agent |
| `lot.md` | lot agent |
| `patrons.md` | patrons agent |
| `economy-night.md` | economy-night agent |
| `integration.md` | integration agent |
| `qa.md` | qa agent |

Entry format (newest at the bottom):

```
## <agent>: <commit subject>
Date: YYYY-MM-DD
Files: <files added or changed>
Why: <one or two sentences, with the design.md section>
Tests: <tests added or run, and the result>
```

CI (`tools/check-changelog.sh`) rejects commits that change `Assets/`, `Packages/`, `ProjectSettings/`,
`.github/` or `tools/` without a changelog entry.
