---
name: unity-vfx
description: "Create or tune real-time Unity VFX from a prompt in Khi Van Chi Tu. Use for Shuriken particle prefabs, sword slashes, hit sparks, lightning, auras, shockwaves, and previews in the open Unity Editor."
license: MIT
---

# Unity VFX for Khí Vận Chi Tử

## Before touching the Editor

Work from the Unity project root, the folder with `Assets`, `Packages`, and `ProjectSettings`. Read its `AGENTS.md` and the applicable module instructions. Check `ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json` for the active Unity and URP versions. Use the project's existing `com.unity.pipeline` connection and the active Editor. From PowerShell, resolve `unity` on PATH, or use `$env:LOCALAPPDATA\Unity\bin\unity.exe`.

Run `unity status --format json` from the project root. Match the returned `projectPath` to this checkout and confirm `state: ready`. Then inspect the open scene, compilation status, and available command parameters.

```powershell
unity command scene list_open --caller plugin --skill unity-vfx --format json
unity command console status --caller plugin --skill unity-vfx --format json
unity command script --caller plugin --skill unity-vfx --no-pager
unity command capture --caller plugin --skill unity-vfx --no-pager
```

If the Editor is unreachable, follow the root instructions to check the process, project path, compilation, and localhost permissions. Use the current agent's approval mechanism for any required loopback access. For scene and prefab changes, use the live Editor APIs through Pipeline. Keep preview work in an approved scene or a temporary preview setup. For `Assets/Modules/PCContent` work, follow its ownership and save-isolation rules and use its Content Sandbox for previews.

## Turn the prompt into a prefab

1. Establish the intended event, silhouette, palette, size in the gameplay camera, and timing. Decide whether this is a one-shot or a loop and whether the user asked for a prefab, a preview, or gameplay integration. Where the prompt leaves reversible visual choices open, pick a first pass and report those choices.
2. Inspect existing effects and compatible assets before creating more. In `Assets/Scripts/Combat`, start with `HitVFXManager.cs` for hit and crit bursts, `LightningVFX.cs` for bolts and sparks, `AuraVFX.cs` for trails, and `ShockwaveVFX.cs` for rings. Check comparable prefabs and URP materials in the relevant asset folder. Respect the portrait combat framing in `Assets/.docs/GDD.md`.
3. Build the primary shape first. Use Shuriken for particle layers and a trail, line, or mesh where it gives the intended silhouette. Add secondary sparks or glow where they improve readability. For a one-shot, set burst timing and particle lifetimes together so the last visible particle or trail ends within the requested window. For a loop, verify how it stops and clears, especially when reused by a pool. Reuse compatible materials and keep large transparent layers sparse for the mobile camera.
4. Create or update the prefab through the Editor. Use `script eval` for small C# snippets or `script eval_file --file <absolute-cs-path>` for a longer snippet stored outside `Assets`. Verify the chosen path and existing asset before saving. For a new prefab, use `PrefabUtility.SaveAsPrefabAsset` with an explicit path in the relevant module. For changes to an existing prefab, use the prefab APIs and preserve its serialized references and overrides. Clean up temporary preview objects and save only scenes changed for the request.

## Preview and hand off

Use the intended gameplay camera, scale, and representative background. For fast effects, use fixed-time samples around onset, peak, and tail. Shuriken can be stepped with `ParticleSystem.Simulate` on a disposable instance, using a fixed seed for repeatable comparisons. Effects driven by gameplay or time-dependent scripts need a real playback check. Capture a frame while the effect is visible and open the PNG for visual inspection.

```powershell
unity command capture screenshot --view game --caller plugin --skill unity-vfx --format json
unity command console get --tail 10 --caller plugin --skill unity-vfx --format json
```

For an effect expected to appear repeatedly, preview representative overlap and check that it remains readable. Report the prefab path, relevant timing, preview, and any checks that could not run. If you changed C#, verify compilation. If you wired gameplay, trigger the actual event and confirm the effect stops or returns to its pool.

VFX workflow reference [Scenario Labs unity-vfx](https://github.com/scenario-labs/skills/tree/6e4a6f00939102bdccbf7d34a7fa8a3e2d22c8c6/skills/game-engines/unity/scenario-unity-vfx).
