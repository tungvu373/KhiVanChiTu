#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class MobileUIContentRepairTool
{
    public const string Evidence = "Assets/.docs/UI_Content_Repair_20261114/QA";

    [MenuItem("TuTien/UI/Repair framed text and close buttons")]
    public static void ApplyScene()
    {
        var scene = EditorSceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/SampleScene_MobileWork.unity")
            throw new InvalidOperationException("Requires the mobile scene in Edit Mode.");
        var canvas = scene.GetRootGameObjects().First(g => g.name == "Canvas");
        var legacy = canvas.transform.Find("CharacterDetailModal/ModalBox/HeaderBar/BtnDaoAn");
        var references = new List<string>();
        if (legacy != null)
        {
            var targets = new HashSet<UnityEngine.Object>(legacy.GetComponentsInChildren<Component>(true).Cast<UnityEngine.Object>()) { legacy.gameObject };
            foreach (var component in canvas.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component == null || component.transform.IsChildOf(legacy)) continue;
                var property = new SerializedObject(component).GetIterator();
                while (property.Next(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference && targets.Contains(property.objectReferenceValue))
                        references.Add(component.GetType().Name + ":" + property.propertyPath);
            }
            if (references.Count != 0) throw new InvalidOperationException("Review legacy shortcut references: " + string.Join(", ", references));
        }
        Undo.SetCurrentGroupName("Repair mobile text padding and corner close buttons");
        Undo.RecordObjects(canvas.GetComponentsInChildren<Component>(true).Where(c => c != null).Cast<UnityEngine.Object>().ToArray(), "Repair mobile UI content");
        if (legacy != null) Undo.DestroyObjectImmediate(legacy.gameObject);
        var hud = canvas.GetComponentInChildren<PortraitHudController>(true);
        hud.ApplyLayout();
        hud.ConfigureViews();
        var theme = canvas.GetComponent<JadeUITheme>();
        foreach (string name in new[] { "DaoAnModal", "SecretRealmModal", "StorageModalUI" })
        {
            var box = canvas.transform.Find(name + "/ModalBox") as RectTransform;
            if (box != null) JadeUIAdaptiveSkin.Apply(box, theme);
        }
        MobileUIResponsiveRepairs.ApplySavedContentLayout(canvas.transform);
        MobileUIContentSafety.Apply(canvas.transform);
        EditorSceneManager.MarkSceneDirty(scene);
        Directory.CreateDirectory(Evidence);
        var closes = canvas.GetComponentsInChildren<UnityEngine.UI.Button>(true).Where(MobileUIContentSafety.IsCornerClose)
            .Select(b => new { path = AnimationUtility.CalculateTransformPath(b.transform, null),
                labels = b.GetComponentsInChildren<UnityEngine.UI.Text>(true).Select(t => new { text = t.text, color = ColorUtility.ToHtmlStringRGBA(t.color) }).ToArray(),
                persistentCallbacks = b.onClick.GetPersistentEventCount() }).ToArray();
        File.WriteAllText(Evidence + "/scene-apply.json", Newtonsoft.Json.JsonConvert.SerializeObject(new { scene = scene.path, removedAvatarShortcut = legacy == null, closes }, Newtonsoft.Json.Formatting.Indented));
        Debug.Log("[UI Content Repair] Applied to mobile scene; corner close buttons: " + closes.Length);
    }
}
#endif
