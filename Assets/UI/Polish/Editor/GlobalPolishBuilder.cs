#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Foundation;

namespace KhiVanChiTu.UI.Polish.Editor
{
    public static class GlobalPolishBuilder
    {
        [MenuItem("TuTien/UI Phase 9/Apply Global Polish")]
        public static void Apply()
        {
            KhiVanChiTu.UI.Equipment.Editor.EquipmentRedesignBuilder.Require();
            foreach(var scope in Object.FindObjectsByType<RpgViewScope>(FindObjectsInactive.Include))
            {
                foreach(var button in scope.GetComponentsInChildren<UnityEngine.UI.Button>(true))
                {if(button.GetComponent<JadeButtonFeedback>()==null)button.gameObject.AddComponent<JadeButtonFeedback>();if(button.GetComponent<UnavailableActionHint>()==null)button.gameObject.AddComponent<UnavailableActionHint>();}
                foreach(var text in scope.GetComponentsInChildren<TMP_Text>(true))
                {text.raycastTarget=false;if(text.name=="Feedback"||text.name=="ResultFeedback")if(text.GetComponent<StatusFeedbackAccent>()==null)text.gameObject.AddComponent<StatusFeedbackAccent>();}
                var reveal=scope.GetComponent<ViewRevealAccent>()??scope.gameObject.AddComponent<ViewRevealAccent>();
                reveal.visuals=scope.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Where(g=>g.name=="Title"||g.name=="SubjectArt").Take(2).ToArray();
            }
            // The exact feedback labels are owned by their presenters; observe only their output.
            foreach(var v in Object.FindObjectsByType<KhiVanChiTu.UI.Cultivation.CultivationScreen>(FindObjectsInactive.Include))Accent(v.feedback);
            foreach(var v in Object.FindObjectsByType<KhiVanChiTu.UI.Progression.ProgressionScreen>(FindObjectsInactive.Include))Accent(v.feedback);
            foreach(var v in Object.FindObjectsByType<MergeWorkbench>(FindObjectsInactive.Include))Accent(v.feedback);
            foreach(var v in Object.FindObjectsByType<ItemDetailSheet>(FindObjectsInactive.Include))Accent(v.feedback);
            var pause=Object.FindAnyObjectByType<PauseMenuManager>(FindObjectsInactive.Include);
            if(pause!=null&&pause.muteToggle!=null)
            {var bg=pause.muteToggle.transform.Find("Background") as RectTransform;if(bg!=null){bg.pivot=new Vector2(.5f,.5f);bg.anchoredPosition=new Vector2(56,0);}}
            var achievement=Object.FindAnyObjectByType<AchievementModalUI>(FindObjectsInactive.Include);
            if(achievement!=null)
            {
                var polish=Legacy(achievement.transform,achievement.modalBox);
                var observer=achievement.contentContainer.GetComponent<LegacyPolishDirty>()??achievement.contentContainer.gameObject.AddComponent<LegacyPolishDirty>();observer.owner=polish;
            }
            var hud=Object.FindAnyObjectByType<PortraitHudController>(FindObjectsInactive.Include);
            if(hud!=null&&hud.secondaryOverlay!=null){var polish=Legacy(hud.secondaryOverlay.transform,hud.secondaryOverlay.transform);polish.backdrop=hud.menuBackdrop!=null?hud.menuBackdrop.GetComponent<UnityEngine.UI.Image>():null;}
            var manager=Object.FindAnyObjectByType<MergeManager>(FindObjectsInactive.Include);
            if(manager!=null)foreach(var slot in manager.equipPanel.GetComponentsInChildren<MergeSlot>(true).Concat(manager.inventoryPages.SelectMany(p=>p.GetComponentsInChildren<MergeSlot>(true))))
            {
                if(slot.GetComponentInParent<RpgViewScope>(true)==null)continue;
                var image=slot.GetComponent<UnityEngine.UI.Image>();if(image==null)continue;
                image.sprite=KhiVanChiTu.UI.Equipment.Editor.EquipmentRedesignBuilder.Theme.surfaceSprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=KhiVanChiTu.UI.Equipment.Editor.EquipmentRedesignBuilder.Theme.surface;
                foreach(var outline in slot.GetComponents<UnityEngine.UI.Outline>())outline.enabled=false;
            }
            if(Object.FindAnyObjectByType<GlobalFeedbackHost>(FindObjectsInactive.Include)==null)
            {
                var theme=KhiVanChiTu.UI.Equipment.Editor.EquipmentRedesignBuilder.Theme;var f=new FoundationFactory(theme);
                var root=RpgUI.Rect("GlobalFeedback",GameObject.Find("Canvas").transform);RpgUI.Fill(root);root.gameObject.AddComponent<SafeAreaFitter>();
                var canvas=root.gameObject.AddComponent<Canvas>();canvas.overrideSorting=true;canvas.sortingOrder=30000;
                var host=root.gameObject.AddComponent<GlobalFeedbackHost>();host.theme=theme;host.notice=f.Notice(root,false);
                RpgUI.Top((RectTransform)host.notice.transform,176,112,44,44);
            }
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        static void Accent(TMP_Text text){if(text!=null&&text.GetComponent<StatusFeedbackAccent>()==null)text.gameObject.AddComponent<StatusFeedbackAccent>();}
        static LegacySurfacePolish Legacy(Transform owner,Transform target)
        {var p=owner.GetComponent<LegacySurfacePolish>()??owner.gameObject.AddComponent<LegacySurfacePolish>();p.theme=KhiVanChiTu.UI.Equipment.Editor.EquipmentRedesignBuilder.Theme;p.target=target;p.Invalidate();return p;}
    }
}
#endif
