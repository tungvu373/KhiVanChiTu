# UI_REDESIGN_FINAL_REPORT

Phase 9 commit: `68e11c7`. Phase 10 completes local implementation, regression QA and reporting, then stops. Android/device release sign-off remains conditional on the BLOCKED/NOT RUN cases below.

## Delivered

Phase 9 added TMP press/release, item-selection accent, decorative view reveals, shared unscaled non-blocking toast, unavailable-action explanations, status feedback, and compact extreme-number display. Phase 10 corrected cultivation-unlock feedback, normalized remaining equipment/legacy window surfaces, corrected mute caption spacing, removed an opaque ornament cover found by screenshot review, and hardened QA cleanup/raster readiness. All business controllers, formulas, rarity, sword cap 10 and save schema remain unchanged. Existing user dirty work stays outside these commits.

## Executed coverage

| Suite | Assertions executed | Parameterized cases |
| --- | ---: | ---: |
| inventory | 48 | 48 |
| inventory-refined | 48 | 48 |
| cultivation | 739 | 134 |
| progression | 4072 | 111 |
| realms | 1263 | 86 |
| menu-final | 440 | 38 |
| menu-final-load | 134 | 16 |


{'PASS': 611, 'BLOCKED': 3, 'NOT RUN': 9} across the explicit/parameterized test-case matrix. See [UI_QA_REPORT](UI_QA_REPORT.md), [regression checklist](UI_REGRESSION_CHECKLIST.md), [machine-readable cases](test-cases.json).

The five realm names were checked against the project: Vạn Ma Triều Dâng, Trảm Phá Cấm Chế, Đan Thần Kỳ Duyên, Vượt Lôi Trì, Kiếm Chứng Cổ Thần. Realms include native success inputs and real timeout runs; Horde timeout also has a near-expiry isolated checkpoint. Large reward display is a UI fixture and does not claim an actual RNG reward of that size. No RNG/timer formula was rewritten.

Pause preserves the project’s existing realtime-menu policy, including a preexisting timeScale 0.35. Main Menu freezes at 0 and native New/Load returns to 1. Actual delete/quit/return-scene confirmation branches were not executed; open/cancel and data preservation were.

## Performance evidence

Inventory measured 120 advancing Editor frames at 1080×1920. Whole-frame counters:

- Main Thread: median 8216100.0 TimeNanoseconds (238 samples).
- Canvas.BuildBatch: median 12200.0 TimeNanoseconds (238 samples).
- GC Allocated In Frame: median 19187.0 Bytes (238 samples).
- Texture Memory: median 440641561.0 Bytes (238 samples).
- Draw Calls Count: median 0.0 Count (238 samples).
- SetPass Calls Count: median 63.0 Count (238 samples).
- GPU Frame Time: median 1293824.0 TimeNanoseconds (238 samples).

Combat measured 240 actual advancing frames at timeScale 1, 4.206 seconds observed; script-driven Editor-loop throughput 57.06 frames/s. This is not a release/player/device FPS result. Counters:

- Main Thread: median 16413500.0 TimeNanoseconds (242 samples).
- Canvas.BuildBatch: median 13400.0 TimeNanoseconds (242 samples).
- GC Allocated In Frame: median 19491.0 Bytes (242 samples).
- Texture Memory: median 426667107.0 Bytes (242 samples).
- SetPass Calls Count: median 63.0 Count (242 samples).
- GPU Frame Time: median 1465344.0 TimeNanoseconds (242 samples).

No before/after performance gain is claimed. The initial Phase 9 baseline was invalid because delayed native Load had not run. Draw Calls Count zero is not accepted as a real draw-call count. UI-only overdraw/Frame Debugger attribution, mobile thermal/fps and atlas-benefit measurements remain unverified. Texture memory includes Editor, world and font assets, not UI alone.

## Android build

Build result: **PASS**. [Build report](Evidence/android-build.json). Local APK path, size, duration and errors are recorded there; no publishing or installation was performed. The APK was built from the working project, including preserved user edits, not from an artificially cleaned checkout. A successful build is not device QA.

## Visual verification and baseline

Actual Game View PNGs are under [Evidence](Evidence), grouped by suite and actual raster. Screens examined include inventory/detail/merge, all cultivation systems, talents/milestones, the five minigames/results, Main Menu/roster/character/audio/pause and other global routes. Colors, type, CTA hierarchy, icon proportions and gameplay-area clearance were compared against Foundation and the project redesign plan. Compact card names use ellipsis with full names available in details.

The Phase 0 audit expressly recorded no trustworthy runtime baseline screenshots; therefore a pixel-identical Phase 0 comparison is NOT RUN. Historical images from other revisions cannot establish a pixel regression pass. The final same-build screenshots and structured layout checks are the available evidence.

## Remaining release gates

- Physical Android touch, system Back, cutout/gesture insets, soft keyboard, installed-APK stability and FPS/thermal/GC budget: BLOCKED/NOT RUN, no connected device.
- Actual character deletion, process quit and save/return scene reload: NOT RUN. Scene-scoped QA save isolation must be retained before those destructive/reload paths can safely be exercised.
- Exact UI draw calls, GPU overdraw and atlas benefit: not established from the available recorders.
- PC/backups/demo scenes were outside this mobile-redesign acceptance and kept intact.

Production save/hash guards and protected-file comparison are recorded in [preservation audit](preservation-audit.json). No user character was deleted or reset. Build-generated URP prefilter/performance-test metadata and dynamic font-cache changes were excluded; originals/cache backups are retained under Temp/Phase10BuildGenerated. The existing AutoRotation configuration was preserved; landscape/rotation remains outside the verified portrait scope. No known critical layout regression remains in the completed Editor scope; full production-ready Android sign-off is withheld until the remaining gates run.


APK file on disk: 115,453,402 bytes (110.10 MiB), SHA-256 `a76ff0b2a2a10771682a8bf2e3510f493be52473a294a3815bd497b3c10199ae`. Unity BuildReport totalSize is recorded separately; use the file-integrity record for the actual APK download size.
