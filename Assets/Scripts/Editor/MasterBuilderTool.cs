#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// TOOL ĐỒNG NHẤT - Xây dựng TOÀN BỘ bố cục Scene từ số 0.
/// Gộp tất cả các giai đoạn đã qua lại thành 1 cú click duy nhất.
/// </summary>
public class MasterBuilderTool
{
    [MenuItem("TuTien/⚡ BUILD TOÀN BỘ SCENE (1 Click)")]
    public static void BuildEverything()
    {
        Debug.Log("====== BẮT ĐẦU XÂY DỰNG TOÀN BỘ SCENE ======");

        // ====== BƯỚC 0: DỌN SẠCH SCENE CŨ ======
        CleanScene();

        // ====== BƯỚC 1: TẠO GAME MANAGER + CÁC MANAGER ======
        GameObject gmObj = CreateGameManager();

        // ====== BƯỚC 2: TẠO MÔI TRƯỜNG 3D (COMBAT BÊN TRÁI) ======
        CreateCombatEnvironment(gmObj);

        // ====== BƯỚC 3: TẠO CAMERA ======
        SetupCamera();

        // ====== BƯỚC 4: TẠO PREFABS (Enemy, Sword, DamagePopup, MergeItem) ======
        CreatePrefabs(gmObj);

        // ====== BƯỚC 5: TẠO CANVAS + TOÀN BỘ UI (BÊN PHẢI) ======
        CreateFullUI(gmObj);

        // ====== BƯỚC 6: NẠP DỮ LIỆU (ScriptableObjects) ======
        LinkData(gmObj);

        // ====== HOÀN THÀNH ======
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        Debug.Log("====== ✅ XÂY DỰNG HOÀN TẤT! Bấm Play để chơi ngay. ======");
    }

    // ================================================================
    //  BƯỚC 0: DỌN SẠCH
    // ================================================================
    static void CleanScene()
    {
        // Xóa tất cả các object cũ (trừ camera mặc định)
        string[] keepNames = { };
        foreach (GameObject go in Object.FindObjectsOfType<GameObject>())
        {
            if (go.transform.parent != null) continue; // Chỉ xóa root
            if (go.GetComponent<Camera>() != null) continue; // Giữ camera gốc
            Object.DestroyImmediate(go);
        }
        // Xóa hết Light cũ
        foreach (Light l in Object.FindObjectsOfType<Light>())
        {
            Object.DestroyImmediate(l.gameObject);
        }
        Debug.Log("[Master] Bước 0: Đã dọn sạch Scene.");
    }

    // ================================================================
    //  BƯỚC 1: GAME MANAGER
    // ================================================================
    static GameObject CreateGameManager()
    {
        GameObject gmObj = new GameObject("GameManager");

        gmObj.AddComponent<GameManager>();
        gmObj.AddComponent<EconomyManager>();
        gmObj.AddComponent<CultivationManager>();
        gmObj.AddComponent<UpgradeManager>();
        gmObj.AddComponent<CombatManager>();
        gmObj.AddComponent<SkillManager>();
        gmObj.AddComponent<MergeManager>();
        gmObj.AddComponent<UpgradeUIPlaceholder>();

        Debug.Log("[Master] Bước 1: Tạo GameManager + 7 Managers.");
        return gmObj;
    }

    // ================================================================
    //  BƯỚC 2: MÔI TRƯỜNG 3D
    // ================================================================
    static void CreateCombatEnvironment(GameObject gmObj)
    {
        // Nền đất (Plane)
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.position = new Vector3(-1.5f, 0, -5);
        ground.transform.localScale = new Vector3(1.5f, 1, 1.5f);
        MaterialPropertyBlock block = new MaterialPropertyBlock();
        block.SetColor("_BaseColor", new Color(0.08f, 0.08f, 0.1f));
        block.SetColor("_Color", new Color(0.08f, 0.08f, 0.1f));
        ground.GetComponent<Renderer>().SetPropertyBlock(block);

        // Player (hình trụ đại diện nhân vật)
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        player.name = "Player";
        player.transform.position = new Vector3(-1.5f, 0.5f, -5);
        player.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
        MaterialPropertyBlock pb = new MaterialPropertyBlock();
        pb.SetColor("_BaseColor", new Color(0.2f, 0.6f, 1f));
        pb.SetColor("_Color", new Color(0.2f, 0.6f, 1f));
        player.GetComponent<Renderer>().SetPropertyBlock(pb);

        // Gán playerTransform cho CombatManager
        CombatManager cm = gmObj.GetComponent<CombatManager>();
        cm.playerTransform = player.transform;

        // Ánh sáng
        GameObject lightObj = new GameObject("Directional Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.color = new Color(0.9f, 0.85f, 0.8f);
        lightObj.transform.rotation = Quaternion.Euler(50, -30, 0);

        Debug.Log("[Master] Bước 2: Tạo môi trường 3D (Sàn + Nhân vật + Ánh sáng).");
    }

    // ================================================================
    //  BƯỚC 3: CAMERA
    // ================================================================
    static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
        }
        // Camera nhìn chéo xuống (Isometric feel)
        cam.transform.position = new Vector3(-1.5f, 12f, -15f);
        cam.transform.rotation = Quaternion.Euler(35, 0, 0);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.02f, 0.02f, 0.05f);

        // Camera chỉ chiếm 50% bên TRÁI màn hình
        cam.rect = new Rect(0, 0, 0.5f, 1f);

        // Thêm CameraShake
        if (cam.GetComponent<CameraShake>() == null)
            cam.gameObject.AddComponent<CameraShake>();

        Debug.Log("[Master] Bước 3: Setup Camera (50% trái).");
    }

    // ================================================================
    //  BƯỚC 4: PREFABS
    // ================================================================
    static void CreatePrefabs(GameObject gmObj)
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        // -- Enemy Prefab --
        string enemyPath = "Assets/Prefabs/Enemy.prefab";
        GameObject enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(enemyPath);
        if (enemyPrefab == null)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            enemy.name = "Enemy";
            enemy.AddComponent<Enemy>();
            MaterialPropertyBlock eb = new MaterialPropertyBlock();
            eb.SetColor("_BaseColor", Color.red);
            eb.SetColor("_Color", Color.red);
            enemy.GetComponent<Renderer>().SetPropertyBlock(eb);
            enemyPrefab = PrefabUtility.SaveAsPrefabAsset(enemy, enemyPath);
            Object.DestroyImmediate(enemy);
        }
        gmObj.GetComponent<CombatManager>().enemyPrefab = enemyPrefab;

        // -- DamagePopup Prefab --
        string popupPath = "Assets/Prefabs/DamagePopup.prefab";
        GameObject popupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(popupPath);
        if (popupPrefab == null)
        {
            GameObject popup = new GameObject("DamagePopup");
            TextMesh tm = popup.AddComponent<TextMesh>();
            tm.fontSize = 64;
            tm.characterSize = 0.2f;
            tm.anchor = TextAnchor.MiddleCenter;
            popup.AddComponent<DamagePopup>();
            popupPrefab = PrefabUtility.SaveAsPrefabAsset(popup, popupPath);
            Object.DestroyImmediate(popup);
        }
        gmObj.GetComponent<CombatManager>().damagePopupPrefab = popupPrefab;

        // -- FlyingSword Prefab --
        string swordPath = "Assets/Prefabs/FlyingSword.prefab";
        GameObject swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(swordPath);
        if (swordPrefab == null)
        {
            GameObject sword = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            sword.name = "FlyingSword";
            sword.transform.localScale = new Vector3(0.15f, 0.6f, 0.15f);
            sword.AddComponent<FlyingSword>();
            TrailRenderer trail = sword.AddComponent<TrailRenderer>();
            trail.time = 0.3f;
            trail.startWidth = 0.1f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = Color.cyan;
            trail.endColor = new Color(0, 1, 1, 0);
            Object.DestroyImmediate(sword.GetComponent<CapsuleCollider>());
            swordPrefab = PrefabUtility.SaveAsPrefabAsset(sword, swordPath);
            Object.DestroyImmediate(sword);
        }
        gmObj.GetComponent<CombatManager>().swordPrefab = swordPrefab;

        // -- Lightning VFX Prefab --
        string lightningPath = "Assets/Prefabs/LightningVFX.prefab";
        GameObject lightningPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(lightningPath);
        if (lightningPrefab == null)
        {
            GameObject vfx = new GameObject("LightningVFX");
            ParticleSystem ps = vfx.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = 0.3f;
            main.startSpeed = 30f;
            main.startColor = new Color(0.6f, 0.8f, 1f);
            main.loop = false;
            main.maxParticles = 50;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0, 30)
            });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(0.3f, 8f, 0.3f);
            vfx.AddComponent<DestroyAfterTime>();
            lightningPrefab = PrefabUtility.SaveAsPrefabAsset(vfx, lightningPath);
            Object.DestroyImmediate(vfx);
        }
        gmObj.GetComponent<CultivationManager>().lightningVFXPrefab = lightningPrefab;

        // -- MergeItem Prefab --
        string mergeItemPath = "Assets/Prefabs/MergeItem.prefab";
        GameObject mergeItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(mergeItemPath);
        if (mergeItemPrefab == null)
        {
            GameObject mi = new GameObject("MergeItem", typeof(RectTransform));
            Image miImg = mi.AddComponent<Image>();
            miImg.color = Color.HSVToRGB(0.15f, 0.7f, 0.9f);
            mi.AddComponent<CanvasGroup>();
            mi.AddComponent<MergeItem>();
            RectTransform miRt = mi.GetComponent<RectTransform>();
            miRt.sizeDelta = new Vector2(80, 80);
            // Text con hiển thị Level
            GameObject txtObj = new GameObject("LevelText", typeof(RectTransform));
            txtObj.transform.SetParent(mi.transform, false);
            Text txt = txtObj.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = 14;
            txt.alignment = TextAnchor.MiddleCenter;
            txt.color = Color.white;
            txt.text = "Lv.1";
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = Vector2.zero;
            txtRt.anchorMax = Vector2.one;
            txtRt.sizeDelta = Vector2.zero;

            mergeItemPrefab = PrefabUtility.SaveAsPrefabAsset(mi, mergeItemPath);
            Object.DestroyImmediate(mi);
        }
        gmObj.GetComponent<MergeManager>().mergeItemPrefab = mergeItemPrefab;

        Debug.Log("[Master] Bước 4: Tạo/Nạp Prefabs (Enemy, Popup, Sword, Lightning, MergeItem).");
    }

    // ================================================================
    //  BƯỚC 5: CANVAS + UI
    // ================================================================
    static void CreateFullUI(GameObject gmObj)
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        UpgradeUIPlaceholder uiPlaceholder = gmObj.GetComponent<UpgradeUIPlaceholder>();
        CultivationManager cultMgr = gmObj.GetComponent<CultivationManager>();
        MergeManager mergeMgr = gmObj.GetComponent<MergeManager>();

        // --- CANVAS ---
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // --- EventSystem ---
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // ======= NỀN ĐEN cho nửa PHẢI (Chống lưu vệt) =======
        GameObject bgRight = new GameObject("BG_Right", typeof(RectTransform));
        bgRight.transform.SetParent(canvasObj.transform, false);
        Image bgRImg = bgRight.AddComponent<Image>();
        bgRImg.color = new Color(0.05f, 0.05f, 0.08f, 1f);
        bgRImg.raycastTarget = false;
        RectTransform bgRRt = bgRight.GetComponent<RectTransform>();
        bgRRt.anchorMin = new Vector2(0.5f, 0);
        bgRRt.anchorMax = Vector2.one;
        bgRRt.sizeDelta = Vector2.zero;

        // ======= THANH TRÊN (Linh Thạch + Cơ Duyên + Cảnh Giới) =======
        GameObject topBar = new GameObject("TopBar", typeof(RectTransform));
        topBar.transform.SetParent(canvasObj.transform, false);
        RectTransform topRt = topBar.GetComponent<RectTransform>();
        topRt.anchorMin = new Vector2(0, 1);
        topRt.anchorMax = new Vector2(1, 1);
        topRt.pivot = new Vector2(0.5f, 1);
        topRt.anchoredPosition = Vector2.zero;
        topRt.sizeDelta = new Vector2(0, 40);
        Image topBg = topBar.AddComponent<Image>();
        topBg.color = new Color(0, 0, 0, 0.6f);

        // Linh Thạch text
        GameObject ltObj = MakeText(topBar.transform, "txt_LinhThach", font, 22, Color.white,
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(20, 0), new Vector2(250, 30));
        uiPlaceholder.txtLinhThach = ltObj.GetComponent<Text>();
        uiPlaceholder.txtLinhThach.text = "Linh Thạch: 0";

        // Cơ Duyên text
        GameObject cdObj = MakeText(topBar.transform, "txt_CoDuyen", font, 22, new Color(1, 0.8f, 0),
            new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f),
            new Vector2(280, 0), new Vector2(200, 30));
        cdObj.AddComponent<CoDuyenUIUpdater>().txt = cdObj.GetComponent<Text>();
        cdObj.GetComponent<Text>().text = "Cơ Duyên: 0";

        // Cảnh Giới text (giữa trên)
        GameObject stageObj = MakeText(topBar.transform, "txt_Stage", font, 20, Color.yellow,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(500, 30));
        stageObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        cultMgr.stageText = stageObj.GetComponent<Text>();

        // ======= THÔNG TIN NHÂN VẬT (Góc trái dưới) =======
        GameObject charPanel = new GameObject("CharPanel", typeof(RectTransform));
        charPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform cpRt = charPanel.GetComponent<RectTransform>();
        cpRt.anchorMin = Vector2.zero;
        cpRt.anchorMax = Vector2.zero;
        cpRt.pivot = Vector2.zero;
        cpRt.anchoredPosition = new Vector2(10, 10);
        cpRt.sizeDelta = new Vector2(250, 100);
        Image cpBg = charPanel.AddComponent<Image>();
        cpBg.color = new Color(0, 0, 0, 0.7f);

        GameObject charTxt = MakeText(charPanel.transform, "txt_CharInfo", font, 16, Color.white,
            Vector2.zero, Vector2.one, new Vector2(0, 0), Vector2.zero, Vector2.zero);
        charTxt.GetComponent<Text>().alignment = TextAnchor.UpperLeft;
        RectTransform ctRt = charTxt.GetComponent<RectTransform>();
        ctRt.offsetMin = new Vector2(10, 5);
        ctRt.offsetMax = new Vector2(-5, -5);
        cultMgr.characterInfoText = charTxt.GetComponent<Text>();

        // ======= RIGHT PANEL (Nửa phải màn hình) =======
        GameObject rightPanel = new GameObject("RightPanel", typeof(RectTransform));
        rightPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform rpRt = rightPanel.GetComponent<RectTransform>();
        rpRt.anchorMin = new Vector2(0.5f, 0);
        rpRt.anchorMax = new Vector2(1, 1);
        rpRt.pivot = new Vector2(0.5f, 0.5f);
        rpRt.anchoredPosition = new Vector2(0, -20);
        rpRt.sizeDelta = new Vector2(0, -40);

        // ======= MERGE PANEL (Nửa trên bên phải) =======
        BuildMergePanel(rightPanel.transform, font, gmObj);

        // ======= UPGRADE PANEL (Nửa dưới bên phải, Scroll) =======
        BuildUpgradePanel(rightPanel.transform, font, uiPlaceholder);

        Debug.Log("[Master] Bước 5: Tạo Canvas + Toàn bộ UI.");
    }

    // ================================================================
    //  MERGE PANEL
    // ================================================================
    static void BuildMergePanel(Transform parent, Font font, GameObject gmObj)
    {
        MergeManager mergeMgr = gmObj.GetComponent<MergeManager>();

        GameObject mergePanel = new GameObject("MergePanel", typeof(RectTransform));
        mergePanel.transform.SetParent(parent, false);
        RectTransform mpRt = mergePanel.GetComponent<RectTransform>();
        mpRt.anchorMin = new Vector2(0, 0.5f);
        mpRt.anchorMax = Vector2.one;
        mpRt.pivot = new Vector2(0.5f, 0.5f);
        mpRt.anchoredPosition = Vector2.zero;
        mpRt.sizeDelta = Vector2.zero;

        // -- 4 ô Trang Bị (Equip) --
        GameObject equipRow = new GameObject("EquipPanel", typeof(RectTransform));
        equipRow.transform.SetParent(mergePanel.transform, false);
        RectTransform erRt = equipRow.GetComponent<RectTransform>();
        erRt.anchorMin = new Vector2(0, 1);
        erRt.anchorMax = new Vector2(1, 1);
        erRt.pivot = new Vector2(0.5f, 1);
        erRt.anchoredPosition = new Vector2(0, -5);
        erRt.sizeDelta = new Vector2(0, 80);
        HorizontalLayoutGroup elg = equipRow.AddComponent<HorizontalLayoutGroup>();
        elg.spacing = 10;
        elg.childAlignment = TextAnchor.MiddleCenter;
        elg.childForceExpandWidth = false;
        elg.childForceExpandHeight = false;

        for (int i = 0; i < 4; i++)
        {
            GameObject slot = new GameObject($"EquipSlot_{i}", typeof(RectTransform));
            slot.transform.SetParent(equipRow.transform, false);
            Image slotImg = slot.AddComponent<Image>();
            slotImg.color = new Color(0.3f, 0.5f, 0.3f, 0.8f);
            slot.AddComponent<MergeSlot>();
            LayoutElement le = slot.AddComponent<LayoutElement>();
            le.preferredWidth = 80;
            le.preferredHeight = 80;
        }
        mergeMgr.equipPanel = equipRow.transform;

        // -- Thanh điều hướng Trang --
        GameObject navBar = new GameObject("NavBar", typeof(RectTransform));
        navBar.transform.SetParent(mergePanel.transform, false);
        RectTransform nbRt = navBar.GetComponent<RectTransform>();
        nbRt.anchorMin = new Vector2(0, 1);
        nbRt.anchorMax = new Vector2(1, 1);
        nbRt.pivot = new Vector2(0.5f, 1);
        nbRt.anchoredPosition = new Vector2(0, -90);
        nbRt.sizeDelta = new Vector2(0, 30);
        HorizontalLayoutGroup nlg = navBar.AddComponent<HorizontalLayoutGroup>();
        nlg.spacing = 10;
        nlg.childAlignment = TextAnchor.MiddleRight;
        nlg.childForceExpandWidth = false;

        // Nút Sắp xếp
        Button btnSort = CreateButton(navBar.transform, "btn_Sort", font, "Sắp xếp", new Color(0.2f, 0.6f, 0.3f));
        btnSort.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.SortInventory(); });

        // Nút Prev
        Button btnPrev = CreateButton(navBar.transform, "btn_Prev", font, "<", new Color(0.3f, 0.3f, 0.4f));
        btnPrev.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.PrevPage(); });

        // Text số trang
        GameObject pageTxtObj = MakeText(navBar.transform, "txt_Page", font, 18, Color.white,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(60, 25));
        pageTxtObj.GetComponent<Text>().text = "1/20";
        pageTxtObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        pageTxtObj.AddComponent<LayoutElement>().preferredWidth = 60;
        mergeMgr.pageText = pageTxtObj.GetComponent<Text>();

        // Nút Next
        Button btnNext = CreateButton(navBar.transform, "btn_Next", font, ">", new Color(0.3f, 0.3f, 0.4f));
        btnNext.onClick.AddListener(() => { if (MergeManager.Instance != null) MergeManager.Instance.NextPage(); });

        // -- 20 Trang kho đồ (mỗi trang 4x4 = 16 ô) --
        Transform[] pages = new Transform[20];
        for (int p = 0; p < 20; p++)
        {
            GameObject page = new GameObject($"InvPage_{p}", typeof(RectTransform));
            page.transform.SetParent(mergePanel.transform, false);
            RectTransform pgRt = page.GetComponent<RectTransform>();
            pgRt.anchorMin = new Vector2(0, 0);
            pgRt.anchorMax = new Vector2(1, 1);
            pgRt.pivot = new Vector2(0.5f, 0.5f);
            pgRt.anchoredPosition = new Vector2(0, -20);
            pgRt.sizeDelta = new Vector2(-20, -130);

            GridLayoutGroup glg = page.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(80, 80);
            glg.spacing = new Vector2(5, 5);
            glg.padding = new RectOffset(5, 5, 5, 5);
            glg.childAlignment = TextAnchor.UpperLeft;

            for (int s = 0; s < 16; s++)
            {
                GameObject slot = new GameObject($"Slot_{s}", typeof(RectTransform));
                slot.transform.SetParent(page.transform, false);
                Image slotImg = slot.AddComponent<Image>();
                slotImg.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);
                slot.AddComponent<MergeSlot>();
            }

            page.SetActive(p == 0); // Chỉ trang đầu bật sẵn
            pages[p] = page.transform;
        }
        mergeMgr.inventoryPages = pages;
    }

    // ================================================================
    //  UPGRADE PANEL (Scroll View chứa 4 Thẻ Chỉ Số + 4 Thẻ Kỹ Năng)
    // ================================================================
    static void BuildUpgradePanel(Transform parent, Font font, UpgradeUIPlaceholder uiPlaceholder)
    {
        // --- ScrollView Container ---
        GameObject scrollObj = new GameObject("UpgradePanel", typeof(RectTransform));
        scrollObj.transform.SetParent(parent, false);
        RectTransform scrollRt = scrollObj.GetComponent<RectTransform>();
        scrollRt.anchorMin = new Vector2(0, 0);
        scrollRt.anchorMax = new Vector2(1, 0.5f);
        scrollRt.pivot = new Vector2(0.5f, 0.5f);
        scrollRt.anchoredPosition = Vector2.zero;
        scrollRt.sizeDelta = Vector2.zero;

        Image scrollBg = scrollObj.AddComponent<Image>();
        scrollBg.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);
        scrollObj.AddComponent<Mask>().showMaskGraphic = true;

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.scrollSensitivity = 30f;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        // --- Content ---
        GameObject content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(scrollObj.transform, false);
        RectTransform contentRt = content.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot = new Vector2(0.5f, 1);
        contentRt.sizeDelta = new Vector2(0, 800);

        scroll.content = contentRt;

        GridLayoutGroup grid = content.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(340, 130);
        grid.spacing = new Vector2(15, 15);
        grid.padding = new RectOffset(15, 15, 15, 15);
        grid.childAlignment = TextAnchor.UpperCenter;

        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // --- 4 Thẻ Nâng Cấp Chỉ Số ---
        MakeUpgradeCard(content.transform, "LinhLuc", font, ref uiPlaceholder.txtLinhLuc, ref uiPlaceholder.btnLinhLuc);
        MakeUpgradeCard(content.transform, "KiemY", font, ref uiPlaceholder.txtKiemY, ref uiPlaceholder.btnKiemY);
        MakeUpgradeCard(content.transform, "ThanThuc", font, ref uiPlaceholder.txtThanThuc, ref uiPlaceholder.btnThanThuc);
        MakeUpgradeCard(content.transform, "TuLinh", font, ref uiPlaceholder.txtTuLinh, ref uiPlaceholder.btnTuLinh);

        // --- 4 Thẻ Kỹ Năng ---
        string[] skillNames = { "Vạn Kiếm Quy Tông", "Ấn Chưởng", "Lôi Phạt", "Phân Thân" };
        Color[] colors = { Color.yellow, Color.cyan, Color.magenta, Color.green };

        for (int i = 0; i < 4; i++)
        {
            GameObject card = new GameObject($"SkillCard_{i}", typeof(RectTransform));
            card.transform.SetParent(content.transform, false);
            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

            // Icon màu
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(card.transform, false);
            Image icon = iconObj.AddComponent<Image>();
            icon.color = colors[i];
            RectTransform iRt = iconObj.GetComponent<RectTransform>();
            iRt.anchorMin = new Vector2(0, 0.5f);
            iRt.anchorMax = new Vector2(0, 0.5f);
            iRt.pivot = new Vector2(0, 0.5f);
            iRt.anchoredPosition = new Vector2(10, 0);
            iRt.sizeDelta = new Vector2(40, 40);

            // Tên kỹ năng
            GameObject titleObj = MakeText(card.transform, "Title", font, 16, Color.white,
                new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1),
                new Vector2(60, -5), new Vector2(-70, 25));
            titleObj.GetComponent<Text>().text = skillNames[i];

            // Điều kiện
            GameObject condObj = MakeText(card.transform, "Condition", font, 12, Color.red,
                new Vector2(0, 0), new Vector2(1, 1), new Vector2(0, 0),
                new Vector2(60, 30), new Vector2(-70, -30));
            condObj.GetComponent<Text>().text = "Đang tải...";

            // Nút Học
            GameObject btnObj = new GameObject("UnlockButton", typeof(RectTransform));
            btnObj.transform.SetParent(card.transform, false);
            Button btn = btnObj.AddComponent<Button>();
            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.8f, 0.6f, 0.1f);
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0, 0);
            btnRt.anchorMax = new Vector2(1, 0);
            btnRt.pivot = new Vector2(0.5f, 0);
            btnRt.anchoredPosition = new Vector2(0, 5);
            btnRt.sizeDelta = new Vector2(-20, 25);

            GameObject btnTxtObj = MakeText(btnObj.transform, "Text", font, 13, Color.white,
                Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            btnTxtObj.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            // Gán SkillCardUI script
            SkillCardUI scUI = card.AddComponent<SkillCardUI>();
            scUI.skillIndex = i;
            scUI.titleText = titleObj.GetComponent<Text>();
            scUI.condText = condObj.GetComponent<Text>();
            scUI.unlockButton = btn;
            scUI.unlockBtnText = btnTxtObj.GetComponent<Text>();
            btn.onClick.AddListener(scUI.OnUnlockClicked);
        }
    }

    // ================================================================
    //  BƯỚC 6: LINK DATA (ScriptableObjects)
    // ================================================================
    static void LinkData(GameObject gmObj)
    {
        UpgradeManager um = gmObj.GetComponent<UpgradeManager>();
        um.linhLucData = AssetDatabase.LoadAssetAtPath<UpgradeData>("Assets/ScriptableObjects/Upgrades/LinhLuc.asset");
        um.kiemYData = AssetDatabase.LoadAssetAtPath<UpgradeData>("Assets/ScriptableObjects/Upgrades/KiemY.asset");
        um.thanThucData = AssetDatabase.LoadAssetAtPath<UpgradeData>("Assets/ScriptableObjects/Upgrades/ThanThuc.asset");
        um.tuLinhData = AssetDatabase.LoadAssetAtPath<UpgradeData>("Assets/ScriptableObjects/Upgrades/TuLinh.asset");

        // Link Cultivation Stages
        CultivationManager cm = gmObj.GetComponent<CultivationManager>();
        string[] guids = AssetDatabase.FindAssets("t:CultivationStageData", new[] { "Assets/ScriptableObjects/Stages" });
        System.Collections.Generic.List<CultivationStageData> stages = new System.Collections.Generic.List<CultivationStageData>();
        foreach (string guid in guids)
        {
            stages.Add(AssetDatabase.LoadAssetAtPath<CultivationStageData>(AssetDatabase.GUIDToAssetPath(guid)));
        }
        stages.Sort((a, b) => a.requiredTuVi.CompareTo(b.requiredTuVi));
        cm.allStages = stages.ToArray();

        if (um.linhLucData == null)
        {
            Debug.LogWarning("⚠️ Chưa có dữ liệu ScriptableObject! Hãy chạy 'TuTien -> Tạo Dữ liệu mẫu' trước.");
        }

        Debug.Log("[Master] Bước 6: Nạp dữ liệu ScriptableObjects.");
    }

    // ================================================================
    //  HELPER FUNCTIONS
    // ================================================================
    static GameObject MakeText(Transform parent, string name, Font font, int size, Color color,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 pos, Vector2 sizeDelta)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);
        Text txt = obj.AddComponent<Text>();
        txt.font = font;
        txt.fontSize = size;
        txt.color = color;
        txt.alignment = TextAnchor.MiddleLeft;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = sizeDelta;
        return obj;
    }

    static Button CreateButton(Transform parent, string name, Font font, string text, Color bgColor)
    {
        GameObject btnObj = new GameObject(name, typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = bgColor;
        Button btn = btnObj.AddComponent<Button>();
        LayoutElement le = btnObj.AddComponent<LayoutElement>();
        le.preferredWidth = 80;
        le.preferredHeight = 25;

        GameObject txtObj = new GameObject("Text", typeof(RectTransform));
        txtObj.transform.SetParent(btnObj.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = font;
        txt.text = text;
        txt.fontSize = 14;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        RectTransform tRt = txtObj.GetComponent<RectTransform>();
        tRt.anchorMin = Vector2.zero;
        tRt.anchorMax = Vector2.one;
        tRt.sizeDelta = Vector2.zero;
        return btn;
    }

    static void MakeUpgradeCard(Transform parent, string name, Font font, ref Text outText, ref Button outBtn)
    {
        GameObject card = new GameObject($"Card_{name}");
        card.transform.SetParent(parent, false);
        card.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.25f, 1f);

        GameObject txtObj = new GameObject("Info", typeof(RectTransform));
        txtObj.transform.SetParent(card.transform, false);
        Text txt = txtObj.AddComponent<Text>();
        txt.font = font;
        txt.fontSize = 15;
        txt.color = Color.white;
        txt.alignment = TextAnchor.UpperCenter;
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = new Vector2(0, 0.4f);
        txtRt.anchorMax = new Vector2(1, 1);
        txtRt.pivot = new Vector2(0.5f, 1);
        txtRt.anchoredPosition = new Vector2(0, -5);
        txtRt.sizeDelta = new Vector2(-10, -10);
        outText = txt;

        GameObject btnObj = DefaultControls.CreateButton(new DefaultControls.Resources());
        btnObj.name = "Button";
        btnObj.transform.SetParent(card.transform, false);
        RectTransform btnRt = btnObj.GetComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.1f, 0.05f);
        btnRt.anchorMax = new Vector2(0.9f, 0.4f);
        btnRt.pivot = new Vector2(0.5f, 0.5f);
        btnRt.anchoredPosition = Vector2.zero;
        btnRt.sizeDelta = Vector2.zero;
        outBtn = btnObj.GetComponent<Button>();
        outBtn.GetComponentInChildren<Text>().font = font;
        outBtn.GetComponentInChildren<Text>().fontSize = 14;
    }
}
#endif
