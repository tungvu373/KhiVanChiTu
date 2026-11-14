---
name: balance-check
description: >-
  Analyze combat, economy, progression and loot balance in Khi Van Chi Tu's Unity
  idle RPG. Use for DPS/TTK, sword merge tiers, cultivation gates, upgrade costs,
  currency sources and sinks, or offline rewards. Base findings on project
  formulas, ScriptableObject data and stated design targets.
metadata:
  upstream: "https://github.com/Donchitos/Claude-Code-Game-Studios"
  upstream-commit: "05ad7c8a5b8aa2aa86bbd3439ae7cf7295cac418"
  adaptation: "Project-local Unity/Codex workflow; Claude hooks and studio configuration removed."
---

# Balance Check - Khi Van Chi Tu

Adapted from Donchitos/Claude-Code-Game-Studios's balance-check skill.
The upstream MIT license is retained at ../../licenses/claude-code-game-studios/LICENSE.
This is a standalone project adaptation; no external studio hooks or model selection are required.

## Scope and inputs

Use the domain named in the request. If none is named, infer it from the feature
being worked on; ask only when choosing a domain would materially change the task.

Read the relevant parts of Assets/.docs/GDD.md and Assets/.docs/TDD.md, then trace
the current implementation. Record design/code disagreements instead of silently
choosing one as the intended rule. Typical entry points, relative to the project root:

| Domain | Entry points |
| --- | --- |
| Combat | Assets/Scripts/Managers/CombatManager.cs, PlayerStatsService.cs and SwordLoadoutService.cs in the same folder; Assets/Scripts/Combat/FlyingSword.cs, Enemy.cs, BossEnemy.cs |
| Economy and upgrades | Assets/Scripts/Managers/EconomyManager.cs, UpgradeManager.cs, ShopManager.cs and AlchemyService.cs; Assets/Scripts/Data/UpgradeData.cs |
| Cultivation and merge | Assets/Scripts/Managers/CultivationManager.cs, StageManager.cs, MergeManager.cs and FormationService.cs; Assets/Scripts/Data/CultivationStageData.cs and SkillData.cs |
| Offline and persistence | Assets/Scripts/Managers/OfflineEarningsManager.cs; Assets/Scripts/Data/DataManager.cs, SaveMigration.cs and the save DTOs |
| Tuning assets | Assets/ScriptableObjects and Assets/Resources; resolve actual assets and scene overrides used by the target scene |

Read code and serialized data as evidence. Use the connected Unity Editor to query
live scene references/overrides when necessary; follow the root AGENTS.md and the
available unity-cli skill. Do not hand-edit scene or prefab YAML. Observe the
additional ownership and save-isolation rules in Assets/Modules/PCContent/AGENTS.md
when working on that module.

## Evidence before verdict

- List which formulas, assets, design targets and runtime observations were found.
  A C# field default does not prove the value used by a scene.
- Separate derived calculations, design targets and runtime measurements.
  Label assumptions, including unit, time scale, active sword count and combat tier.
- When input is missing, mark the affected result NOT ASSESSED and name the missing
  data. Continue useful analysis of available inputs.
- Do not label a value unbalanced solely because no target was documented.
  Report its derived behavior and identify the target needed to judge it.

## Domain checks

- Combat: derive effective damage and attack cadence before calculating DPS/TTK.
  Account for sword count, target availability, boss rules, armor, crits, cooldowns,
  AoE and formation effects only where implemented. Check defensive caps and
  dominant choices using comparable costs and progression tiers.
- Economy: enumerate resource sources and sinks, distinguishing active/offline
  income and one-time/repeatable rewards. Compare upgrade costs with income over
  the same time interval. Trace any repeatable conversion loop before claiming
  an exploit; inspect consumption and reward-claim guards.
- Progression: evaluate cultivation requirements, breakthrough requirements,
  unlocked sword slots, merge tiers and body upgrades together. Sample meaningful
  milestones and boundaries around unlocks, caps and realm changes. Include a
  table or plot when it makes a curve easier to assess.
- Offline: inspect elapsed-time units, implemented caps, clock rollback behavior,
  repeated claims and any rewarded-ad multiplier. Keep real player saves intact.
- Loot: use implemented drop probabilities and inventory rules. State the
  assumptions behind expected acquisition time; do not invent pity mechanics.

## Output and changes

Report the domain, evidence, derived values, missing targets, actionable findings
and prioritized recommendations. Include source paths/lines for formulas and
enough arithmetic to reproduce a result.

Use HEALTHY only for checks fully evaluated against stated targets with no
findings. Use CONCERNS or CRITICAL ISSUES for substantiated problems and
NOT ASSESSED for missing evidence; report partial coverage explicitly.

For a review request, report findings. When tuning or fixes are part of the
authorized task, make scoped changes, preserve save compatibility, and rerun the
affected calculations and appropriate Unity checks. Keep existing uncommitted
work and module ownership boundaries. Never claim runtime validation without
observing a running game or target build.
