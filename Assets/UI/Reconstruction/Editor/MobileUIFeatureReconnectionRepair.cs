#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class MobileUIFeatureReconnectionChecks
{
    [MenuItem("TuTien/UI/Reconnection/Restore body maximum action")]
    public static void RestoreBodyMaximumAction()
    {
        var scene=EditorSceneManager.GetActiveScene();
        if(EditorApplication.isPlaying||scene.isDirty||scene.path!=MobileUIReconstructionBuilder.ScenePath)
            throw new InvalidOperationException("Requires saved mobile scene in Edit mode");
        var canvas=GameObject.Find("Canvas");var ui=canvas.transform.Find("MobileUIRoot/Drawer_1").GetComponent<BodyTemperingPanelUI>();
        if(ui.btnModalUpgradeMax!=null)return;
        var parent=ui.btnModalUpgrade.transform.parent;
        if(parent.Find("P7UpgradeMax")!=null)throw new InvalidOperationException("Existing unbound P7 action needs inspection");
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Reconstruction/Prefabs/CostActionButton.prefab");
        var go=(GameObject)PrefabUtility.InstantiatePrefab(prefab,parent);go.name="P7UpgradeMax";Undo.RegisterCreatedObjectUndo(go,"Reconnect body maximum action");
        var action=go.GetComponent<MobileUIActionView>();
        action.caption.text="NÂNG TỐI ĐA";action.caption.fontSize=34;
        action.value.text="Tối đa 50 cấp";action.value.fontSize=26;
        if(action.disabledReason!=null)action.disabledReason.gameObject.SetActive(false);
        Undo.RecordObject(ui,"Bind maximum body upgrade");ui.btnModalUpgradeMax=action.button;
        void Layout(RectTransform rt,float left,float right)
        {
            Undo.RecordObject(rt,"Fit three upgrade commands");rt.anchorMin=new Vector2(left,0.02f);rt.anchorMax=new Vector2(right,0.16f);
            rt.offsetMin=rt.offsetMax=Vector2.zero;
        }
        Layout((RectTransform)ui.btnModalUpgrade.transform,0.06f,0.40f);
        Layout((RectTransform)ui.btnModalUpgrade5.transform,0.42f,0.66f);
        Layout((RectTransform)go.transform,0.68f,0.94f);
        foreach(var c in go.GetComponentsInChildren<Component>(true))PrefabUtility.RecordPrefabInstancePropertyModifications(c);
        EditorUtility.SetDirty(ui);EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
    }
}
#endif
