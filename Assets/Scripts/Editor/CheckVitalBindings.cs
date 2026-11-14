using System;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class CheckVitalBindingsCombined
{
    [MenuItem("TuTien/QA/Check Vital Bindings Single Log")]
    public static void CheckBindings()
    {
        var cm = UnityEngine.Object.FindAnyObjectByType<CombatManager>(FindObjectsInactive.Include);
        var sm = UnityEngine.Object.FindAnyObjectByType<SkillManager>(FindObjectsInactive.Include);
        var hud = UnityEngine.Object.FindAnyObjectByType<PortraitHudController>(FindObjectsInactive.Include);

        var sb = new StringBuilder();
        sb.AppendLine("=== COMBAT MANAGER BINDINGS ===");
        if (cm != null)
        {
            sb.AppendLine($"cm.playerHpFill: {(cm.playerHpFill != null ? Phase11UIInspector.GetHierarchyPath(cm.playerHpFill.transform) : "NULL")}");
            sb.AppendLine($"cm.playerHpText: {(cm.playerHpText != null ? Phase11UIInspector.GetHierarchyPath(cm.playerHpText.transform) : "NULL")}");
        }

        sb.AppendLine("=== SKILL MANAGER BINDINGS ===");
        if (sm != null)
        {
            sb.AppendLine($"sm.manaFill: {(sm.manaFill != null ? Phase11UIInspector.GetHierarchyPath(sm.manaFill.transform) : "NULL")}");
            sb.AppendLine($"sm.skillText: {(sm.skillText != null ? Phase11UIInspector.GetHierarchyPath(sm.skillText.transform) : "NULL")}");
        }

        sb.AppendLine("=== PORTRAIT HUD CONTROLLER BINDINGS ===");
        if (hud != null)
        {
            sb.AppendLine($"hud.hp: {(hud.hp != null ? Phase11UIInspector.GetHierarchyPath(hud.hp) : "NULL")}");
            sb.AppendLine($"hud.mana: {(hud.mana != null ? Phase11UIInspector.GetHierarchyPath(hud.mana) : "NULL")}");
            sb.AppendLine($"hud.vitals: {(hud.vitals != null ? Phase11UIInspector.GetHierarchyPath(hud.vitals) : "NULL")}");
            sb.AppendLine($"hud.manaValue: {(hud.manaValue != null ? Phase11UIInspector.GetHierarchyPath(hud.manaValue.transform) : "NULL")}");
        }

        Debug.Log(sb.ToString());
    }
}
