#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Equipment;
using KhiVanChiTu.UI.Equipment.Editor;

namespace KhiVanChiTu.UI.Cultivation.Editor
{
    public static class CultivationRedesignBuilder
    {
        [MenuItem("TuTien/UI Phase 5/Apply Cultivation Redesign")]
        public static void Apply()
        {
            EquipmentRedesignBuilder.Require();
            var drawer=UnityEngine.Object.FindAnyObjectByType<MobileDrawerUIController>(FindObjectsInactive.Include);
            var f=EquipmentRedesignBuilder.Factory;var theme=EquipmentRedesignBuilder.Theme;
            string[] titles={"Luyện Thể","Luyện Đan","Pháp Khí","Trận Pháp"};
            string[] subtitles={"Rèn thể phách · Khai thông kinh mạch","Nhóm linh hỏa · Kết tụ đan tâm","Luyện khí hộ thân · Dưỡng linh bảo","Ngưng tụ linh lực · Khai mở đại trận"};
            for(int index=1;index<=4;index++)
            {
                var panel=drawer.drawerPanels[index];
                if(panel.GetComponentInChildren<CultivationScreen>(true)!=null)throw new InvalidOperationException("Phase 5 already exists. Update the bound view instead of rebuilding.");
                foreach(var old in panel.GetComponents<MonoBehaviour>())if(old is BodyTemperingPanelUI||old is AlchemyPanelUI||old is ArtifactPanelUI||old is FormationPanelUI)old.enabled=false;
                var root=EquipmentRedesignBuilder.Drawer(panel,titles[index-1],subtitles[index-1]);panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,1440);
                var v=root.gameObject.AddComponent<CultivationScreen>();v.kind=(CultivationScreenKind)(index-1);v.theme=theme;
                var scroll=RpgUI.Scroll(root,out var content);v.scroll=scroll;RpgUI.Fill((RectTransform)scroll.transform,28,index==3?224:380,28,168);
                v.wallet=EquipmentRedesignBuilder.Label(content,"Wallet","",72,28);v.wallet.color=theme.gold;
                int count=index==2?2:index==3?4:6;int columns=index==2?2:2;int rows=Mathf.CeilToInt(count/(float)columns);
                var selectors=EquipmentRedesignBuilder.Row(content,"Selection",rows*132+(rows-1)*12,columns);var grid=selectors.GetComponent<UnityEngine.UI.GridLayoutGroup>();grid.cellSize=new Vector2(474,132);grid.spacing=new Vector2(16,12);v.selectors=new FoundationAction[count];
                for(int i=0;i<count;i++)v.selectors[i]=f.Action("Select"+i,selectors,v.Names[i]);
                var hero=RpgUI.Rect("Subject",content);RpgUI.Height(hero,280);f.Surface(hero,theme.surface);
                var art=RpgUI.Rect("SubjectArt",hero);RpgUI.Fill(art,24,24,0,24);art.anchorMax=new Vector2(.25f,1);v.subjectArt=f.Surface(art,Color.white);v.subjectArt.preserveAspect=true;v.subjectArt.type=UnityEngine.UI.Image.Type.Simple;
                if(index==2)v.subjectArt.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Generated/JadeV5_AlchemyDecoration.png");
                else RpgEmblem.Show(v.subjectArt,index==1?RpgEmblemKind.Body:index==3?RpgEmblemKind.Robe:RpgEmblemKind.Formation,theme.gold);
                v.subject=f.Text("SubjectName",hero,v.Names[0],40);RpgUI.Top(v.subject.rectTransform,24,112,270,20);v.subject.font=theme.headingFont;
                v.level=f.Text("Level",hero,"",30);RpgUI.Bottom(v.level.rectTransform,24,96,270,20);v.level.color=theme.gold;
                v.progress=f.Progress(content);RpgUI.Height(v.progress.transform,110);v.progress.label.fontSize=28;
                v.current=EquipmentRedesignBuilder.Label(content,"CurrentStats","HIỆN TẠI",124,32);v.current.richText=true;v.current.color=theme.ivory;
                v.next=EquipmentRedesignBuilder.Label(content,"NextStats","SAU NÂNG CẤP",124,32);v.next.richText=true;v.next.color=theme.jade;
                v.condition=EquipmentRedesignBuilder.Label(content,"ActionCondition","",90,30);v.condition.color=theme.gold;
                v.materials=EquipmentRedesignBuilder.Label(content,"Materials","",112,32);
                v.explanation=EquipmentRedesignBuilder.Label(content,"EffectsAndRules","",128,30);v.explanation.color=theme.muted;
                v.result=EquipmentRedesignBuilder.Label(content,"ResultsAndInventory","",96,30);
                var primary=f.Action("PrimaryAction",root,"Tu luyện",true);v.primary=primary;RpgUI.Bottom((RectTransform)primary.transform,index==3?64:220,132,28,28);
                var actions=EquipmentRedesignBuilder.Row(root,"AdditionalActions",132);RpgUI.Bottom(actions,72,132,28,28);v.secondary=f.Action("SecondaryAction",actions,"Hành động");v.tertiary=f.Action("TertiaryAction",actions,"Hành động");
                if(index==3)actions.gameObject.SetActive(false);
                v.feedback=f.Text("Feedback",root,"",28);RpgUI.Bottom(v.feedback.rectTransform,8,56,28,28);v.feedback.color=theme.jade;
                EditorUtility.SetDirty(panel);
            }
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();Debug.Log("[Phase 5] Four cultivation systems redesigned and bound to the existing services.");
        }
    }
}
#endif
