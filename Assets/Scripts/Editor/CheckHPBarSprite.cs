using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class CheckHPBarSprite
{
    [MenuItem("TuTien/QA/Check HPBar Sprite")]
    public static void Check()
    {
        var hpBar = GameObject.Find("Canvas/MobileUIRoot/PortraitHudShell/PrimaryActions/VitalBars/PlayerHPBar");
        if (hpBar != null)
        {
            var img = hpBar.GetComponent<Image>();
            Debug.Log($"PlayerHPBar Image: sprite={(img.sprite != null ? img.sprite.name : "NULL")}, type={img.type}, color={img.color}");
            var bg = hpBar.transform.Find("BG")?.GetComponent<Image>();
            Debug.Log($"PlayerHPBar/BG Image: sprite={(bg != null && bg.sprite != null ? bg.sprite.name : "NULL")}, color={(bg != null ? bg.color.ToString() : "NULL")}");
            var fill = hpBar.transform.Find("BG/Fill")?.GetComponent<Image>();
            Debug.Log($"PlayerHPBar/BG/Fill Image: sprite={(fill != null && fill.sprite != null ? fill.sprite.name : "NULL")}, color={(fill != null ? fill.color.ToString() : "NULL")}");
        }

        var hud = UnityEngine.Object.FindAnyObjectByType<PortraitHudController>(FindObjectsInactive.Include);
        if (hud != null)
        {
            Debug.Log($"hud.panelSprite: {(hud.panelSprite != null ? hud.panelSprite.name : "NULL")}");
            Debug.Log($"hud.slotSprite: {(hud.slotSprite != null ? hud.slotSprite.name : "NULL")}");
        }
    }
}

