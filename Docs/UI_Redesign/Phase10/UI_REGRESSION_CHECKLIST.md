# UI_REGRESSION_CHECKLIST

This checklist refers to final completed runs, not archived failed attempts. Functional formulas, stats, unlock conditions, RNG and persistence implementation were not changed.

| ID | Regression | Expected | Actual / resolution | Status | Evidence |
| --- | --- | --- | --- | --- | --- |
| REG-01 | Missing scripts / new local references | None | Editor/runtime and staged scene validation | PASS | [Scene](Evidence/final-scene.json), [staging](scene-staging.json) |
| REG-02 | Native callbacks / data binding | Original actions still execute | Merge/equip, cultivation, talent/reward, minigame inputs, audio and save/load rerun | PASS | [QA](UI_QA_REPORT.md) |
| REG-03 | Modal touch-through / stuck panels | HUD blocked; Back/Close restores view | Menu raycasts, progression shields, item sheet stack, realm exits | PASS | [Menu](Evidence/menu-load/checks.json), [realms](Evidence/realms/checks.json) |
| REG-04 | Slider distortion | Stable handle proportions and native drag | Menu three-raster volume/mute persistence tests | PASS | [Menu](Evidence/menu-create/checks.json) |
| REG-05 | Extreme values / long text | Readable wrap or intentional ellipsis | UI scientific number formatting; long item/talent/realm/character names | PASS | [Global](Evidence/final/cases.json) |
| REG-06 | Unlock toast omitted | Announce real cultivation unlocks | Fixed UI host subscription: FeatureUnlock event only covered sword-acquisition path; now also observes Cultivation.OnStageChanged and waits for initial session load | PASS | [Combat](Evidence/combat/cases.json) |
| REG-07 | Old heavy empty equipment frames | Match Foundation surfaces | Restyled actual equip/inventory slot references in Editor; no lock/capacity change | PASS | [Refined inventory](Evidence/inventory-refined/checks.json) |
| REG-08 | Late QA cleanup enters next Play session | Cleanup is idempotent and Edit-mode-only | Fixed test harness guards; subsequent suites ran with no runtime errors | PASS | [Attempts retained](Evidence/Attempts), [final runs](UI_QA_REPORT.md) |
| REG-09 | Wrong transient Game View raster | Actual raster equals requested | Final runner locks and waits on Screen dimensions; prior Phase 9 mismatches are excluded from requested-resolution acceptance | PASS | [Global](Evidence/final/cases.json) |
| REG-10 | Save corruption / settings reset | Production data stays byte-identical | Hash/prefs guards true; audio prefs restored; QA characters retained | PASS | [QA preservation](UI_QA_REPORT.md) |
| REG-11 | Scene return / process quit / deletion | Native final confirmation completes | Only open/cancel tested for save isolation and non-destructive validation | NOT RUN | [Remaining gates](UI_QA_REPORT.md) |
| REG-13 | Mute caption overlaps checkbox | Clear label gap | Pivot corrected; native drag/mute and three-raster spacing assertions rerun | PASS | [Menu final](Evidence/menu-final/checks.json) |
| REG-14 | Opaque ornament covers legacy window content | Content stays visible | Disabled obsolete ornament graphics and retained native dimmer; rechecked screenshots and final global sweep | PASS | [Achievement screenshot](Evidence/final/Achievements-1080x1920.png), [resources](Evidence/final/Resources-1080x1920.png) |
| REG-12 | Physical notch/touch/FPS | Hardware validation | No connected Android device | BLOCKED | [ADB](Evidence/adb-devices.txt) |

No known unresolved critical layout regression in the completed Editor runs. This is not a claim that unexecuted device/scene-exit paths pass.
