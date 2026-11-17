#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using TMPro;
using KhiVanChiTu.UI.Foundation;
using KhiVanChiTu.UI.Navigation;

namespace KhiVanChiTu.UI.Equipment.Editor
{
    public static class EquipmentRedesignBuilder
    {
        public const string ScenePath="Assets/Scenes/SampleScene_MobileWork.unity";
        public static FoundationTheme Theme=>UnityEngine.Object.FindAnyObjectByType<GlobalNavigationController>(FindObjectsInactive.Include).theme;
        public static FoundationFactory Factory=>new FoundationFactory(Theme);
        public static void Require(){if(EditorApplication.isPlayingOrWillChangePlaymode||UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!=ScenePath)throw new InvalidOperationException("Open the mobile scene in Edit Mode.");}
        public static T Add<T>(GameObject go) where T:Component=>go.GetComponent<T>()??go.AddComponent<T>();
        public static RectTransform Scope(string name,Transform parent)
        {var r=RpgUI.Rect(name,parent);RpgUI.Fill(r);r.gameObject.AddComponent<RpgViewScope>();Factory.Surface(r,Theme.ink,true);return r;}
        public static void LegacyButton(UnityEngine.UI.Button b,Transform parent,string caption=null)
        {
            b.transform.SetParent(parent,false);RpgUI.Height(b.transform,132);
            var image=b.GetComponent<UnityEngine.UI.Image>();if(image!=null){image.sprite=Theme.surfaceSprite;image.type=UnityEngine.UI.Image.Type.Sliced;image.color=Theme.raised;}
            foreach(var text in b.GetComponentsInChildren<UnityEngine.UI.Text>(true)){text.font=UiTextStyle.BodyFont;text.fontSize=30;text.resizeTextForBestFit=false;text.horizontalOverflow=HorizontalWrapMode.Wrap;text.verticalOverflow=VerticalWrapMode.Truncate;text.color=Theme.ivory;RpgUI.Fill(text.rectTransform,12,8,12,8);if(caption!=null)text.text=caption;}
            foreach(var outline in b.GetComponents<UnityEngine.UI.Outline>())outline.enabled=false;
        }
        public static void Stack(RectTransform r,float gap=20)
        {var g=Add<UnityEngine.UI.VerticalLayoutGroup>(r.gameObject);g.spacing=gap;g.childControlHeight=g.childControlWidth=true;g.childForceExpandHeight=false;Add<UnityEngine.UI.ContentSizeFitter>(r.gameObject).verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;}
        public static RectTransform Row(Transform p,string n,float height,int columns=0)
        {var r=RpgUI.Rect(n,p);RpgUI.Height(r,height);if(columns>0){var grid=r.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();grid.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=columns;grid.cellSize=new Vector2(450,132);grid.spacing=new Vector2(16,16);}else {var h=r.gameObject.AddComponent<UnityEngine.UI.HorizontalLayoutGroup>();h.spacing=16;h.childControlWidth=h.childControlHeight=true;h.childForceExpandWidth=true;h.childForceExpandHeight=true;}return r;}
        public static TMP_Text Label(Transform p,string n,string value,float height,float size=32)
        {var text=Factory.Text(n,p,value,size);RpgUI.Height(text.transform,height);RpgUI.Fill(text.rectTransform,4,4,4,4);return text;}
        public static RectTransform Drawer(Transform drawer,string title,string subtitle)
        {
            foreach(Transform child in drawer)child.gameObject.SetActive(false);
            var root=Scope("RpgPresentation",drawer);var header=RpgUI.Rect("Header",root);RpgUI.Top(header,12,140,24,24);
            var t=Factory.Text("Title",header,title,48);RpgUI.Fill(t.rectTransform,8,62,160,0);t.font=Theme.headingFont;
            var sub=Factory.Text("Subtitle",header,subtitle,28);RpgUI.Fill(sub.rectTransform,8,0,160,74);sub.color=Theme.muted;
            var close=Factory.Action("Close",header,"Đóng");RpgUI.Top((RectTransform)close.transform,0,132);((RectTransform)close.transform).anchorMin=new Vector2(1,1);((RectTransform)close.transform).offsetMin=new Vector2(-144,-132);((RectTransform)close.transform).offsetMax=Vector2.zero;
            UnityEventTools.AddPersistentListener(close.button.onClick,UnityEngine.Object.FindAnyObjectByType<MobileDrawerUIController>(FindObjectsInactive.Include).CloseActiveDrawer);
            return root;
        }
        public static FoundationItem Item(Transform p,bool card)
        {
            var item=Factory.Item(p,card);var r=(RectTransform)item.transform;
            foreach(var x in item.GetComponentsInChildren<TMP_Text>(true)){x.fontSize=28;x.overflowMode=TextOverflowModes.Ellipsis;}
            if(!card)
            {
                r.sizeDelta=new Vector2(230,294);RpgUI.Height(r,294);
                RpgUI.Top(item.badge.rectTransform,0,52,8,8);item.badge.alignment=TextAlignmentOptions.MidlineRight;
                RpgUI.Top(item.icon.rectTransform,42,130,26,26);
                RpgUI.Top(item.title.rectTransform,174,68,12,12);item.title.fontSize=28;item.title.alignment=TextAlignmentOptions.Center;
                RpgUI.Bottom(item.detail.rectTransform,8,36,8,8);item.detail.fontSize=24;item.detail.alignment=TextAlignmentOptions.Center;item.detail.gameObject.SetActive(true);
            }
            else
            {
                RpgUI.Height(r,180);RpgUI.Fill(item.icon.rectTransform,12,24,0,24);item.icon.rectTransform.anchorMax=new Vector2(.16f,1);
                RpgUI.Top(item.title.rectTransform,16,72,170,160);RpgUI.Bottom(item.detail.rectTransform,20,64,170,160);item.detail.gameObject.SetActive(true);
                RpgUI.Fill(item.badge.rectTransform,0,20,16,20);item.badge.rectTransform.anchorMin=new Vector2(.82f,0);
            }
            var edge=RpgUI.Rect("RarityEdge",r);RpgUI.Fill(edge);edge.anchorMax=new Vector2(.015f,1);Factory.Surface(edge,Theme.gold);
            item.action.button.targetGraphic.color=Theme.surface;return item;
        }
        static ItemDetailSheet BuildSheet(Transform canvas)
        {
            var root=Scope("ItemDetailBottomSheet",canvas);root.GetComponent<UnityEngine.UI.Image>().color=new Color(0,.02f,.025f,.86f);
            var sheet=root.gameObject.AddComponent<ItemDetailSheet>();sheet.theme=Theme;
            var backdrop=root.gameObject.AddComponent<UnityEngine.UI.Button>();UnityEventTools.AddPersistentListener(backdrop.onClick,sheet.Close);
            var safe=RpgUI.Rect("SafeArea",root);RpgUI.Fill(safe);safe.gameObject.AddComponent<SafeAreaFitter>();
            var surface=RpgUI.Rect("SheetSurface",safe);RpgUI.Fill(surface,24,24,24,0);surface.anchorMax=new Vector2(1,.82f);Factory.Surface(surface,Theme.ink,true);
            sheet.close=Factory.Action("Close",surface,"Đóng");RpgUI.Top((RectTransform)sheet.close.transform,16,132,0,24);((RectTransform)sheet.close.transform).anchorMin=new Vector2(.78f,1);
            sheet.title=Factory.Text("ItemName",surface,"Chi tiết vật phẩm",44);RpgUI.Top(sheet.title.rectTransform,24,112,28,240);sheet.title.overflowMode=TextOverflowModes.Ellipsis;
            var art=RpgUI.Rect("ItemArt",surface);RpgUI.Top(art,154,200,28,0);art.anchorMax=new Vector2(.26f,1);sheet.art=Factory.Surface(art,Color.white);sheet.art.type=UnityEngine.UI.Image.Type.Simple;sheet.art.preserveAspect=true;
            sheet.rarity=Factory.Text("RarityAndState",surface,"",32);RpgUI.Top(sheet.rarity.rectTransform,174,156,280,28);
            sheet.scroll=RpgUI.Scroll(surface,out var content);RpgUI.Fill((RectTransform)sheet.scroll.transform,28,344,28,376);
            sheet.description=Label(content,"Description","",540);sheet.description.richText=false;sheet.description.overflowMode=TextOverflowModes.Overflow;
            // Preferred text height owns the reading area's content; no fixed truncation for descriptions.
            UnityEngine.Object.DestroyImmediate(sheet.description.GetComponent<UnityEngine.UI.LayoutElement>());
            sheet.feedback=Factory.Text("Feedback",surface,"",28);RpgUI.Bottom(sheet.feedback.rectTransform,280,56,28,28);sheet.feedback.color=Theme.jade;
            var actions=Row(surface,"Actions",248,2);RpgUI.Bottom(actions,24,248,28,28);var grid=actions.GetComponent<UnityEngine.UI.GridLayoutGroup>();grid.cellSize=new Vector2(456,116);
            sheet.equip=Factory.Action("Equip",actions,"Trang bị",true);sheet.selectMerge=Factory.Action("SelectMerge",actions,"Chọn ghép");sheet.dismantle=Factory.Action("Dismantle",actions,"Phân giải");
            root.gameObject.SetActive(false);return sheet;
        }
        [MenuItem("TuTien/UI Phase 4/Apply Equipment and Inventory Redesign")]
        public static void Apply()
        {
            Require();var canvas=UnityEngine.Object.FindAnyObjectByType<GlobalNavigationController>(FindObjectsInactive.Include).GetComponentInParent<Canvas>().rootCanvas.transform;
            if(canvas.Find("ItemDetailBottomSheet")!=null)throw new InvalidOperationException("Phase 4 already applied; update existing components.");
            var sheet=BuildSheet(canvas);var storage=StorageModalUI.Instance;var original=storage.modalBox;original.gameObject.SetActive(false);
            var root=Scope("InventoryScreen",storage.transform);var safe=RpgUI.Rect("InventorySafeArea",root);RpgUI.Fill(safe);safe.gameObject.AddComponent<SafeAreaFitter>();
            var panel=Scope("InventorySurface",safe);RpgUI.Fill(panel,24,16,24,16);storage.modalBox=panel;
            var v=panel.gameObject.AddComponent<InventoryPresentation>();v.theme=Theme;v.owner=storage;v.sheet=sheet;storage.presentation=v;
            var title=Factory.Text("Heading",panel,"Kho Đồ",52);RpgUI.Top(title.rectTransform,20,80,24,190);title.font=Theme.headingFont;
            var close=Factory.Action("CloseInventory",panel,"Đóng");RpgUI.Top((RectTransform)close.transform,12,132,0,24);((RectTransform)close.transform).anchorMin=new Vector2(.8f,1);storage.btnClose=close.button;
            v.wallet=Factory.Text("Resources",panel,"",28);RpgUI.Top(v.wallet.rectTransform,106,60,24,24);v.wallet.color=Theme.gold;
            var tabs=Row(panel,"Categories",132);RpgUI.Top(tabs,182,132,24,24);v.tabs=new FoundationAction[5];for(int i=0;i<5;i++){v.tabs[i]=Factory.Action("Category"+i,tabs,new[]{"Tất cả","Kiếm","Đan","Vật liệu","Hòm tạm"}[i]);v.tabs[i].label.fontSize=28;}
            var tools=Row(panel,"ViewTools",104);RpgUI.Top(tools,330,104,24,24);v.sort=Factory.Action("Sort",tools,"Kiếm: cấp giảm ↓");v.view=Factory.Action("View",tools,"Dạng danh sách");
            var filter=RpgUI.Rect("LootPriority",panel);RpgUI.Top(filter,450,196,24,24);var fg=filter.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();fg.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;fg.constraintCount=2;fg.cellSize=new Vector2(464,88);fg.spacing=new Vector2(12,12);
            foreach(var tog in new[]{storage.toggleSwordHighLevel,storage.togglePillBreakthrough,storage.toggleAutoDismantleLv1})
            {tog.transform.SetParent(filter,false);var bg=tog.transform.Find("Background") as RectTransform;RpgUI.Fill(bg,12,22,0,22);bg.anchorMax=new Vector2(0,1);bg.offsetMax=new Vector2(56,-22);var label=tog.GetComponentInChildren<UnityEngine.UI.Text>();label.fontSize=28;label.resizeTextForBestFit=false;label.color=Theme.ivory;RpgUI.Fill(label.rectTransform,72,6,8,6);}
            v.summary=Factory.Text("Hint",panel,"",26);RpgUI.Top(v.summary.rectTransform,656,60,24,24);v.summary.color=Theme.muted;
            var scroll=RpgUI.Scroll(panel,out var unused);UnityEngine.Object.DestroyImmediate(unused.gameObject);RpgUI.Fill((RectTransform)scroll.transform,24,196,24,732);storage.scrollRect=scroll;
            storage.gridContent=RpgUI.Rect("InventoryGrid",scroll.viewport);storage.gridContent.anchorMin=new Vector2(0,1);storage.gridContent.anchorMax=Vector2.one;storage.gridContent.pivot=new Vector2(.5f,1);storage.gridContent.anchoredPosition=Vector2.zero;storage.gridContent.sizeDelta=Vector2.zero;
            var grid=storage.gridContent.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();grid.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=4;grid.cellSize=new Vector2(230,294);grid.spacing=new Vector2(12,12);storage.gridContent.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>().verticalFit=UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
            storage.listContent=RpgUI.Rect("InventoryList",scroll.viewport);storage.listContent.anchorMin=new Vector2(0,1);storage.listContent.anchorMax=Vector2.one;storage.listContent.pivot=new Vector2(.5f,1);storage.listContent.anchoredPosition=Vector2.zero;storage.listContent.sizeDelta=Vector2.zero;Stack(storage.listContent);storage.listContent.gameObject.SetActive(false);
            storage.contentContainer=storage.gridContent;scroll.content=storage.gridContent;
            var empty=RpgUI.Rect("EmptyState",scroll.viewport);RpgUI.Fill(empty,36,12,36,12);storage.txtEmptyNotice=empty.gameObject.AddComponent<UnityEngine.UI.Text>();storage.txtEmptyNotice.font=UiTextStyle.BodyFont;storage.txtEmptyNotice.fontSize=36;storage.txtEmptyNotice.alignment=TextAnchor.MiddleCenter;storage.txtEmptyNotice.color=Theme.muted;storage.txtEmptyNotice.raycastTarget=false;
            var footer=Row(panel,"InventoryActions",144);RpgUI.Bottom(footer,20,144,24,24);LegacyButton(storage.btnClaimPriority,footer,"Nhận ưu tiên");LegacyButton(storage.btnBatchDismantle,footer,"Tách cấp thấp");
            var lib=UnityEngine.Object.FindAnyObjectByType<MobileUIReconstruction>(FindObjectsInactive.Include);v.pill=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Generated/JadeV5_AlchemyDecoration.png");v.herb=v.pill;v.ore=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Generated/JadeV5_ResourceOre.png");v.dust=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Generated/JadeV5_ResourceMedicineDust.png");v.pending=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Generated/JadeV5_RewardFrame.png");
            System.IO.Directory.CreateDirectory("Assets/UI/Equipment/Prefabs");
            var item=Item(null,false);PrefabUtility.SaveAsPrefabAsset(item.gameObject,"Assets/UI/Equipment/Prefabs/ItemSlot.prefab");UnityEngine.Object.DestroyImmediate(item.gameObject);v.slotPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Equipment/Prefabs/ItemSlot.prefab").GetComponent<FoundationItem>();
            item=Item(null,true);PrefabUtility.SaveAsPrefabAsset(item.gameObject,"Assets/UI/Equipment/Prefabs/ItemCard.prefab");UnityEngine.Object.DestroyImmediate(item.gameObject);v.cardPrefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/Equipment/Prefabs/ItemCard.prefab").GetComponent<FoundationItem>();
            BuildMerge();EditorUtility.SetDirty(storage);EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();Debug.Log("[Phase 4] Inventory, real item prefabs, detail sheet and merge workbench applied to mobile scene.");
        }
        [MenuItem("TuTien/UI Phase 4/Refine Existing Presentation")]
        public static void Refine()
        {
            Require();
            foreach(var scope in UnityEngine.Object.FindObjectsByType<RpgViewScope>(FindObjectsInactive.Include,FindObjectsSortMode.None))
            {
                foreach(var a in scope.GetComponentsInChildren<FoundationAction>(true))
                {var image=a.GetComponent<UnityEngine.UI.Image>();image.sprite=Theme.surfaceSprite;image.color=a.label.color.grayscale<.4f?Theme.gold:Theme.raised;image.type=UnityEngine.UI.Image.Type.Sliced;}
            }
            foreach(var scope in UnityEngine.Object.FindObjectsByType<RpgViewScope>(FindObjectsInactive.Include,FindObjectsSortMode.None))
                foreach(var row in scope.GetComponentsInChildren<UnityEngine.UI.HorizontalLayoutGroup>(true))row.childForceExpandHeight=true;
            var s=StorageModalUI.Instance;s.presentation.pending=s.presentation.ore;
            var work=UnityEngine.Object.FindAnyObjectByType<MergeWorkbench>(FindObjectsInactive.Include);
            foreach(var g in work.grids){g.enabled=true;g.padding=new RectOffset();}
            var drawer=(RectTransform)work.transform.parent;drawer.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,1440);
            var actions=work.transform.Find("ReadingScroll/Viewport/Items/MergeActions");if(actions!=null){actions.SetParent(work.transform,false);RpgUI.Bottom((RectTransform)actions,78,132,24,24);}
            work.feedback.transform.SetParent(work.transform,false);RpgUI.Bottom(work.feedback.rectTransform,8,60,24,24);
            RpgUI.Fill((RectTransform)work.transform.Find("ReadingScroll"),24,230,24,168);
            foreach(var i in new[]{work.first,work.second,work.result})RpgUI.Height(i.transform,294);
            RpgUI.Height(work.capacity.transform,120);
            foreach(var file in new[]{"ItemSlot","ItemCard"})
            {var root=PrefabUtility.LoadPrefabContents("Assets/UI/Equipment/Prefabs/"+file+".prefab");var item=root.GetComponent<FoundationItem>();RpgUI.Top(item.badge.rectTransform,0,52,8,8);PrefabUtility.SaveAsPrefabAsset(root,"Assets/UI/Equipment/Prefabs/"+file+".prefab");PrefabUtility.UnloadPrefabContents(root);}
            EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }
        static void BuildMerge()
        {
            var drawer=UnityEngine.Object.FindAnyObjectByType<MobileDrawerUIController>(FindObjectsInactive.Include).drawerPanels[0];var mgr=UnityEngine.Object.FindAnyObjectByType<MergeManager>(FindObjectsInactive.Include);
            var old=mgr.equipPanel.parent;var tools=old.Find("InventoryContainer/PaginationPanel");var root=Drawer(drawer,"Kiếm Lộ","Phi kiếm · Trang bị · Dung hợp");
            var work=root.gameObject.AddComponent<MergeWorkbench>();work.theme=Theme;work.manager=mgr;
            var scroll=RpgUI.Scroll(root,out var content);RpgUI.Fill((RectTransform)scroll.transform,24,20,24,168);
            work.capacity=Label(content,"Capacity","Xuất chiến",80,30);mgr.equipPanel.SetParent(content,false);var eg=mgr.equipPanel.GetComponent<UnityEngine.UI.GridLayoutGroup>();eg.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;eg.constraintCount=4;eg.cellSize=new Vector2(230,266);eg.spacing=new Vector2(12,12);RpgUI.Height(mgr.equipPanel,3*278);mgr.equipPanel.gameObject.SetActive(true);
            Label(content,"MergeTitle","DUNG HỢP PHÔI KIẾM",72,38);
            var slots=Row(content,"MergeIngredients",294);work.first=Item(slots,false);work.second=Item(slots,false);work.result=Item(slots,false);work.preview=Label(content,"Preview","Chọn hai kiếm cùng cấp",138,30);
            var actions=Row(content,"MergeActions",132);work.merge=Factory.Action("Merge",actions,"Dung hợp",true);work.clear=Factory.Action("Clear",actions,"Bỏ chọn");work.feedback=Label(content,"Feedback","",64,30);work.feedback.color=Theme.jade;
            var gridTools=Row(content,"InventoryTools",280,2);var gg=gridTools.GetComponent<UnityEngine.UI.GridLayoutGroup>();gg.cellSize=new Vector2(470,132);
            foreach(var n in new[]{"btn_QuickMerge","SortBtn","BtnSpawnSword"}){var b=tools.Find(n)?.GetComponent<UnityEngine.UI.Button>();if(b!=null)LegacyButton(b,gridTools);}
            var shortcut=old.Find("BtnDrawerDismantle")?.GetComponent<UnityEngine.UI.Button>();if(shortcut!=null)LegacyButton(shortcut,gridTools,"Kho đồ & Phân giải");
            Label(content,"InventoryTitle","KHO KIẾM · Kéo thả hoặc chạm để chọn",72,32);
            var pages=RpgUI.Rect("InventoryPages",content);RpgUI.Height(pages,7*278);foreach(var page in mgr.inventoryPages){page.SetParent(pages,false);RpgUI.Fill((RectTransform)page);var g=Add<UnityEngine.UI.GridLayoutGroup>(page.gameObject);g.constraint=UnityEngine.UI.GridLayoutGroup.Constraint.FixedColumnCount;g.constraintCount=4;g.cellSize=new Vector2(230,266);g.spacing=new Vector2(12,12);}
            var pagination=Row(content,"Pagination",132);foreach(var n in new[]{"PrevBtn","PageText","NextBtn"}){var t=tools.Find(n);if(t==null)continue;t.SetParent(pagination,false);RpgUI.Height(t,132);var b=t.GetComponent<UnityEngine.UI.Button>();if(b!=null)LegacyButton(b,pagination);else foreach(var txt in t.GetComponentsInChildren<UnityEngine.UI.Text>(true)){txt.fontSize=30;txt.resizeTextForBestFit=false;}}
            work.grids=new[]{eg}.Concat(mgr.inventoryPages.Select(p=>p.GetComponent<UnityEngine.UI.GridLayoutGroup>())).ToArray();
        }
    }
}
#endif
