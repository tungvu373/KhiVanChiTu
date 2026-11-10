# PC Content workspace

The project owner handles Android. The collaborator owns Windows presentation, opening content and QA in this module.

- Read README.md, Docs/TASKS.md and Docs/UNITY_CLI_ANTIGRAVITY.md before work.
- Run Unity CLI from the project root (the directory containing Assets, Packages, ProjectSettings), targeting this checkout's running Editor.
- Use the open Editor through Pipeline. Do not hand-edit .unity/.prefab YAML or open another Editor on the same checkout.
- Gameplay scene: Assets/Modules/PCContent/Scenes/PCGameplayWork.unity. Content preview: Assets/Modules/PCContent/Scenes/ContentSandbox.unity.
- Keep __PCWorkspace_SaveIsolation and its pc-workspace profile. Never copy Android progress into this profile.
- Keep Android's global scene list and PlayerSettings unchanged. The PC build tool explicitly selects PCGameplayWork.
- Own files within this module; request coordination before changing shared Scripts, Packages, ProjectSettings, imported originals or Android scenes.
- Make prefab variants or owned asset copies when modifying shared art. Copy a TerrainData asset separately if changing terrain.
- Runtime core, saves, economy and backend belong to the project owner. Report desired rule changes through a task, not silent code edits.
- Workspace tools: TuTien/Collaboration. Save modified scenes before running validation or switching scenes. Create Workspace is idempotent and never resets existing workspace scenes/content.
- Keep commits small and include .meta files. Do not commit Builds, Library, logs, credentials or local test saves.
- Never merge the PC gameplay scene over Android's scene. Integrate reviewed content prefabs and approved shared changes deliberately.
- Before handoff: compile, validate workspace, play test, save scenes, inspect console and attach Windows QA results with build/commit/device.
- A Windows EXE test is not an Android test. Do not claim the Android build passed without running it on a device.
