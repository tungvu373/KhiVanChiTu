#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

/// <summary>Read-only parity snapshots and repeatable portrait preview profiles.</summary>
public static class JadeUIRedesignAudit
{
    public const string Folder = "Assets/.docs/UIRedesignV2";
    [Serializable] public class Snapshot { public string scene; public List<Node> nodes = new List<Node>(); public List<Binding> bindings = new List<Binding>(); public List<string> events = new List<string>(); }
    [Serializable] public class Node { public string id, path, type, state; }
    [Serializable] public class Binding { public string owner, property, target; }
    static string Id(UnityEngine.Object obj) => obj == null ? "null" : GlobalObjectId.GetGlobalObjectIdSlow(obj).ToString();
    public static Snapshot Read()
    {
        var result = new Snapshot { scene = EditorSceneManager.GetActiveScene().path };
        foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        foreach (var component in root.GetComponentsInChildren<Component>(true))
        {
            if (component == null) continue;
            string path = AnimationUtility.CalculateTransformPath(component.transform, null);
            if (!(component is MonoBehaviour) && !(component is UnityEngine.UI.Selectable)) continue;
            result.nodes.Add(new Node { id = Id(component), path = path, type = component.GetType().FullName, state = component.gameObject.activeSelf.ToString() });
            var so = new SerializedObject(component);
            var prop = so.GetIterator();
            while (prop.Next(true))
            {
                if (prop.propertyType == SerializedPropertyType.ObjectReference && prop.objectReferenceValue != null && prop.propertyPath != "m_Script")
                    result.bindings.Add(new Binding { owner = Id(component), property = prop.propertyPath, target = Id(prop.objectReferenceValue) });
            }
            for (var type = component.GetType(); type != null && type != typeof(Component); type = type.BaseType)
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
            {
                if (!typeof(UnityEventBase).IsAssignableFrom(field.FieldType)) continue;
                var evt = field.GetValue(component) as UnityEventBase;
                if (evt == null) continue;
                for (int i = 0; i < evt.GetPersistentEventCount(); i++)
                    result.events.Add(Id(component) + "/" + field.Name + "/" + i + "/" + Id(evt.GetPersistentTarget(i)) + "/" + evt.GetPersistentMethodName(i) + "/" + evt.GetPersistentListenerState(i));
            }
        }
        return result;
    }
    [MenuItem("TuTien/UI/Jade V2/00 Record baseline")]
    public static void Baseline()
    {
        Directory.CreateDirectory(Folder);
        if (File.Exists(Folder + "/baseline.json")) throw new InvalidOperationException("Baseline already exists; keep the original parity record.");
        File.WriteAllText(Folder + "/baseline.json", JsonUtility.ToJson(Read(), true));
        string backup = "Assets/Scenes/Backups/SampleScene_MobileWork_PreJadeV2.unity";
        Directory.CreateDirectory("Assets/Scenes/Backups");
        if (!File.Exists(backup)) AssetDatabase.CopyAsset(EditorSceneManager.GetActiveScene().path, backup);
        Debug.Log("[Jade V2] Baseline and original scene backup recorded.");
    }
    [MenuItem("TuTien/UI/Jade V2/09 Verify bindings")]
    public static void Verify()
    {
        var before = JsonUtility.FromJson<Snapshot>(File.ReadAllText(Folder + "/baseline.json"));
        var after = Read();
        var ids = new HashSet<string>(after.nodes.Select(n => n.id));
        var bindings = new HashSet<string>(after.bindings.Select(b => b.owner + "/" + b.property + "/" + b.target));
        var missing = before.nodes.Where(n => !ids.Contains(n.id)).Select(n => "COMPONENT: " + n.path + " " + n.type).ToList();
        // Transform parenting is presentation. All component/service references must survive.
        missing.AddRange(before.bindings.Where(b => !b.property.StartsWith("m_") && !bindings.Contains(b.owner + "/" + b.property + "/" + b.target)).Select(b => "BINDING: " + b.owner + " " + b.property));
        missing.AddRange(before.events.Except(after.events).Select(e => "EVENT: " + e));
        string report = "Original components: " + before.nodes.Count + "\nCurrent components: " + after.nodes.Count + "\nOriginal object bindings: " + before.bindings.Count + "\nOriginal persistent listeners: " + before.events.Count + "\nMissing contracts: " + missing.Count + "\n" + string.Join("\n", missing);
        File.WriteAllText(Folder + "/binding-verification.txt", report);
        File.WriteAllText(Folder + "/after.json", JsonUtility.ToJson(after, true));
        if (missing.Count > 0) throw new InvalidOperationException(report);
        Debug.Log("[Jade V2] " + report);
    }
    public static void SetResolution(int width, int height)
    {
        var assembly = typeof(Editor).Assembly;
        var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        var instance = sizesType.GetProperty("instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).GetValue(null);
        var group = sizesType.GetProperty("currentGroup").GetValue(instance);
        var groupType = group.GetType();
        int count = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);
        int index = -1;
        for (int i = 0; i < count; i++)
        {
            var size = groupType.GetMethod("GetGameViewSize").Invoke(group, new object[] { i });
            var type = size.GetType();
            if ((int)type.GetProperty("width").GetValue(size) == width && (int)type.GetProperty("height").GetValue(size) == height && type.GetProperty("sizeType").GetValue(size).ToString() == "FixedResolution") { index = i; break; }
        }
        if (index < 0)
        {
            var sizeType = assembly.GetType("UnityEditor.GameViewSize");
            var enumType = assembly.GetType("UnityEditor.GameViewSizeType");
            var size = Activator.CreateInstance(sizeType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null,
                new object[] { Enum.ToObject(enumType, 1), width, height, "Jade QA " + width + "x" + height }, null);
            groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
            index = count;
        }
        var view = EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView"));
        view.GetType().GetMethod("SizeSelectionCallback", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(view, new object[] { index, null });
        view.Repaint();
    }
    [MenuItem("TuTien/UI/Jade V2/Preview 1080x1920")]
    public static void Portrait() => SetResolution(1080, 1920);
}
#endif
