"""Read-only comparison of serialized objects outside the original Canvas hierarchy."""
from pathlib import Path
import json
import re

ASSETS = Path(__file__).resolve().parents[2]


def documents(path):
    source = path.read_text(encoding="utf-8-sig").replace("\r\n", "\n")
    matches = list(re.finditer(r"^--- !u!(\d+) &(-?\d+)(?: stripped)?\n", source, re.M))
    return {
        int(match.group(2)): (
            int(match.group(1)),
            source[match.end():matches[i + 1].start() if i + 1 < len(matches) else len(source)].strip(),
        )
        for i, match in enumerate(matches)
    }


def canvas_gameobjects(docs):
    canvas = next(key for key, (kind, body) in docs.items()
                  if kind == 1 and re.search(r"^  m_Name: Canvas$", body, re.M))
    transforms = {}
    for key, (kind, body) in docs.items():
        if kind not in (4, 224):
            continue
        gameobject = re.search(r"m_GameObject: \{fileID: (-?\d+)\}", body)
        father = re.search(r"m_Father: \{fileID: (-?\d+)\}", body)
        if gameobject and father:
            transforms[key] = (int(gameobject.group(1)), int(father.group(1)))
    roots = {key for key, (go, _) in transforms.items() if go == canvas}
    while True:
        expanded = roots | {key for key, (_, father) in transforms.items() if father in roots}
        if expanded == roots:
            break
        roots = expanded
    return {go for key, (go, _) in transforms.items() if key in roots}


def main():
    before = documents(ASSETS / "Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity")
    after = documents(ASSETS / "Scenes/SampleScene_MobileWork.unity")
    ui_gameobjects = canvas_gameobjects(before)
    changed, missing = [], []
    checked = 0
    for key, (kind, body) in before.items():
        owner = re.search(r"m_GameObject: \{fileID: (-?\d+)\}", body)
        if (kind == 1 and key in ui_gameobjects) or (owner and int(owner.group(1)) in ui_gameobjects):
            continue
        checked += 1
        if key not in after:
            missing.append(key)
        elif after[key] != (kind, body):
            changed.append({"fileID": key, "classID": kind})
    result = {"checkedOutsideCanvas": checked, "changed": changed, "missing": missing,
              "passed": not changed and not missing}
    output = Path(__file__).with_name("scene-scope-verification.json")
    output.write_text(json.dumps(result, indent=2), encoding="utf-8")
    print(json.dumps(result, indent=2))
    return 0 if result["passed"] else 1


if __name__ == "__main__":
    raise SystemExit(main())
