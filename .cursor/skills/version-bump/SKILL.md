---
name: version-bump
description: >-
  Bump the OpenContent module version consistently across all locations. Use when the user asks
  for a version bump, release prep, or when AssemblyInfo, OpenContent.dnn, or appveyor.yml are
  out of sync with the module version.
---

# OpenContent version bump

**Source of truth:** `OpenContent/OpenContent.dnn` → `package version="MM.mm.bb"` (e.g. `05.02.05`).

## Steps

1. **Determine the new version**
   - User provides an explicit version → use it.
   - Otherwise: patch +1 on the current `.dnn` version (`05.02.05` → `05.02.06`).
   - Format: always two digits per segment (`05`, not `5`).

2. **Update files** (exactly these three, in this order)

   | File | What to change |
   |------|----------------|
   | `OpenContent/OpenContent.dnn` | `package name="OpenContent" … version="MM.mm.bb"` |
   | `OpenContent/Properties/AssemblyInfo.cs` | `AssemblyVersion` and `AssemblyFileVersion` → `"MM.mm.bb.00"` |
   | `appveyor.yml` | `version: MM.mm.bb.{build}-{branch}` |

3. **Do not change** (unless the user explicitly asks)
   - SQL script versions in `.dnn` (`00.00.01`, `03.05.00`, …)
   - `upgradeVersionsList`
   - Third-party JS versions (ckeditor, alpaca, …)
   - `OpenContentTests/Properties/AssemblyInfo.cs`

4. **Verify** — search for the old version and confirm consistency:

   ```powershell
   rg "05\.02\.(04|05)" OpenContent/OpenContent.dnn OpenContent/Properties/AssemblyInfo.cs appveyor.yml
   ```

   Expected: the same new `MM.mm.bb` everywhere; assembly uses a `.00` suffix.

5. **Optional** (only on request)
   - Add an entry to `OpenContent/ReleaseNotes.txt`
   - `ModulePackage.targets` has commented-out `FileUpdate` steps — do not uncomment without explicit request

## Example

Bump `05.02.05` → `05.02.06`:

- `.dnn`: `version="05.02.06"`
- `AssemblyInfo.cs`: `"05.02.06.00"`
- `appveyor.yml`: `version: 05.02.06.{build}-{branch}`

## Commit message

```
Bump OpenContent to 05.02.06
```
