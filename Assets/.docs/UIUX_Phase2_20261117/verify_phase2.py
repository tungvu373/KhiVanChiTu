"""Read-only source/serialized reference checks; never writes Unity assets or player data."""
from pathlib import Path
import hashlib
import json
import re
import zipfile
import difflib

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
ALLOWED = {
    "Assets/Scripts/UI/PortraitHudController.cs",
    "Assets/Scripts/UI/JadeUITheme.cs",
    "Assets/UI/Reconstruction/Runtime/MobileUIReconstruction.cs",
    "Assets/Scripts/Combat/DamagePopup.cs",
}

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest() if path.is_file() else "missing"

baseline = json.loads((HERE / "protected_source_before.json").read_text(encoding="utf-8-sig"))
changed = [p for p, value in baseline.items() if digest(ROOT / p) != value]
manifest = json.loads((HERE / "before_manifest.json").read_text(encoding="utf-8-sig"))
meta_checks = {p: digest(ROOT / p) == value for p, value in manifest.items() if p.endswith(".meta")}

with zipfile.ZipFile(HERE / "Backups/before-phase2.zip") as archive:
    names = archive.namelist()
    def original(path):
        name = next(n for n in names if n.replace("\\", "/").endswith(path))
        return archive.read(name).decode("utf-8-sig")
    before_scene = original("Assets/Scenes/SampleScene_MobileWork.unity")
    diffs = []
    for path in sorted(ALLOWED):
        diffs.extend(difflib.unified_diff([line+"\n" for line in original(path).splitlines()], [line+"\n" for line in (ROOT / path).read_text(encoding="utf-8-sig").splitlines()], fromfile="before/" + path, tofile="after/" + path))

after_scene = (ROOT / "Assets/Scenes/SampleScene_MobileWork.unity").read_text(encoding="utf-8-sig")
def blocks(text):
    return {int(m.group(1)): m.group(2) for m in re.finditer(r"^--- !u!\d+ &(-?\d+)[^\n]*\n(.*?)(?=^--- !u!|\Z)", text, re.M | re.S)}
old, new = blocks(before_scene), blocks(after_scene)
before_buttons = json.loads((HERE / "Evidence/buttons-before.json").read_text(encoding="utf-8-sig"))
example = int(before_buttons[0]["id"].split("-")[-2])
button_guid = re.search(r"m_Script:.*guid: (\w+)", old[example]).group(1)
buttons = {key: block for key, block in old.items() if "guid: " + button_guid in block}
events_changed = []
for key, block in buttons.items():
    if key not in new:
        events_changed.append(key)
        continue
    # Button's last serialized field is m_OnClick; skin/navigation/color fields are outside it.
    old_event = block[block.index("  m_OnClick:"):].strip() if "  m_OnClick:" in block else block.strip()
    new_event = new[key][new[key].index("  m_OnClick:"):].strip() if "  m_OnClick:" in new[key] else new[key].strip()
    if old_event != new_event:
        events_changed.append(key)

result = {
    "protectedFileCount": len(baseline),
    "changedOriginalSource": changed,
    "unexpectedProtectedChanges": [p for p in changed if p not in ALLOWED],
    "originalMetaGuidsUnchanged": meta_checks,
    "originalSerializedObjectCount": len(old),
    "missingOriginalObjectIds": sorted(set(old) - set(new)),
    "originalSceneButtonCount": len(buttons),
    "missingOriginalButtonIds": sorted(set(buttons) - set(new)),
    "changedOriginalPersistentButtonEvents": events_changed,
    "temporaryQaMarkerSerialized": "__CombatHUD_Phase2_QA_Temporary" in after_scene,
    "newRuntimeScripts": [str(p.relative_to(ROOT)).replace("\\", "/") for p in (ROOT / "Assets/UI/CombatHUD/Runtime").glob("*.cs")],
}
result["passed"] = not (result["unexpectedProtectedChanges"] or result["missingOriginalButtonIds"] or events_changed or result["temporaryQaMarkerSerialized"]) and all(meta_checks.values())
result["codeAndSerializedReferencesPreserved"] = not ([p for p in changed if p.endswith(".cs") and p not in ALLOWED] or result["missingOriginalObjectIds"] or events_changed) and all(meta_checks.values())
result["engineSerializationReview"] = "engine-serialization-review.json"
(HERE / "Evidence/source-preservation.json").write_text(json.dumps(result, ensure_ascii=False, indent=2), encoding="utf-8")
(HERE / "Evidence/existing-source-changes.diff").write_text("".join(diffs), encoding="utf-8")
print(json.dumps(result, ensure_ascii=False, indent=2))
raise SystemExit(0 if result["passed"] else 1)
