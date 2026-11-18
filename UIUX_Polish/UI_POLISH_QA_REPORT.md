# UI polish QA report

7020 PASS / 0 FAIL / 4 BLOCKED / 2 NOT RUN. Counts include individual executed native assertions; they are not distinct manual scenarios.

244 actual PNG captures, with dimensions and SHA-256 in [screenshots.json](Evidence/screenshots.json). Target rasters: 1080×1920, 1080×2340, 720×1280; global regression additionally 1440×3200.

Every case has ID, description, expected, actual, status and evidence in [test-cases.json](test-cases.json). Only emitted native assertions are assigned PASS; unexecuted device/baseline checks remain BLOCKED/NOT RUN.

| Suite | Executed assertions | Captures |
|---|---:|---:|
| HUD/navigation/toggle/badge/realm presentation | 317 | 114 |
| inventory | 48 | 13 |
| cultivation | 739 | 32 |
| progression | 4072 | 21 |
| realms | 1251 | 31 |
| menu-create | 440 | 27 |
| menu-load | 134 | 6 |

## Targeted acceptance

| Test | Expected / actual | Status | Evidence |
|---|---|---|---|
| AI asset | Built-in generation, real alpha RGBA PNG imported; static HUD image references replaced; native fills/icons/cooldowns retained | PASS | generated-art.json; live generated-art cases |
| Toggle | 20 consecutive native button clicks at each of 3 rasters, plus 20 with advancing combat/boss banner fixture; position and raycast remain valid | PASS | refinement/cases.json, toggle and combat PNGs |
| Dao Van | Existing Talents/DaoAn route first under Equipment, unique primary route, Back restores Equipment; absent in Cultivation/Other | PASS | dao-first/open/back cases; equipment PNGs |
| Badges | Native economy/purchase/pending item/claim transitions, correct parent aggregation, no clearing available actions merely on viewing, native save/load | PASS | badge-* cases |
| Realms | 5 names verified against actual project; title centered within content region; symmetric metrics/action label gutters; native outcomes/exit/rewards preserved | PASS | realm-text-* cases; realms/checks.json |
| Preserved data | 121 protected files, all unrelated preexisting dirty source files, and 315 original OnClick documents unchanged; production save hashes unchanged | PASS | source-preservation.json and each save-preservation.json |

## Measurement limits

Inventory idle observation: 120 advancing Editor frames, 0 dirty-layout and 0 dirty-vertex callbacks. This is one observed UI window, not a whole-game or device zero-allocation guarantee.
- Main Thread: median 1.1494e+07 TimeNanoseconds; whole Editor/frame scope.
- Canvas.BuildBatch: median 15500 TimeNanoseconds; whole Editor/frame scope.
- GC Allocated In Frame: median 19243 Bytes; whole Editor/frame scope.
- Texture Memory: median 4.61517e+08 Bytes; whole Editor/frame scope.
- Draw Calls Count: median 0 Count; whole Editor/frame scope.
- SetPass Calls Count: median 59 Count; whole Editor/frame scope.
- GPU Frame Time: median 1.60358e+06 TimeNanoseconds; whole Editor/frame scope.

No performance improvement is claimed. Texture memory includes world, fonts and Editor-loaded assets. Draw Calls Count=0 is not accepted as valid UI draw-call evidence. Physical Android FPS, touch, cutouts and overdraw are blocked by no attached device.

HP/MP 0/100% captures are presentation fixtures with the two native gameplay writers temporarily suspended: native auto-cast otherwise immediately spends full MP. Writers are restored before live combat/toggle testing. Boss banner uses an existing StageManager state fixture; this is not proof of defeating a naturally spawned boss. Realm timeout coverage includes a disposable near-expiry checkpoint, explicitly described in raw checks.

Prior failed attempts are retained in Evidence/Attempts: missing badge CanvasRenderer was fixed; incompatible nine-slice scale and native MP auto-cast fixture interference were corrected and the 317-case suite rerun with zero failures. Earlier attempt PNGs remain locally and are ignored by Git.
